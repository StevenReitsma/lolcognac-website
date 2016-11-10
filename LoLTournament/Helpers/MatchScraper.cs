using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using LoLTournament.Models;
using Microsoft.Ajax.Utilities;
using MongoDB.Driver.Builders;
using RiotSharp;
using RiotSharp.MatchEndpoint;
using RiotSharp.TournamentEndpoint;

namespace LoLTournament.Helpers
{
    public class MatchScraper
    {
        private readonly TournamentRiotApi _api;

        public MatchScraper()
        {
            var key = WebConfigurationManager.AppSettings["RiotTournamentApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            _api = TournamentRiotApi.GetInstance(key, rateLimit1, rateLimit2);
        }

        public void StartTimer()
        {
            new Timer(ScrapeCurrentMatches, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            new Timer(ScrapeFinishedMatches, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Scrapes current lobby events.
        /// </summary>
        /// <param name="args">Unused</param>
        public void ScrapeCurrentMatches(object args)
        {
            // Disable outside tournament to save API calls
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            var tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            if (DateTime.Now < tournamentStart || DateTime.Now > tournamentStart + TimeSpan.FromHours(12))
                // assuming tournament lasts for a maximum of 12 hours
                return;

            // For each match that is next up to be played (except ingame matches)
            var matches =
                Mongo.Teams.Find(Query<Models.Team>.Where(x => !x.Cancelled))
                    .Select(team => team.GetNextMatch())
                    .Where(x => x != null && x.Status != Status.InGame)
                    .DistinctBy(x => x.Id).ToList();

            foreach (var m in matches)
            {
                List<TournamentLobbyEvent> lobbyEvents;

                // Get lobby events
                try
                {
                    lobbyEvents = _api.GetTournamentLobbyEvents(m.TournamentCode);

                    if (lobbyEvents.Count == 0)
                        throw new Exception();
                }
                catch (Exception)
                {
                    try
                    {
                        lobbyEvents = _api.GetTournamentLobbyEvents(m.TournamentCodeBlind);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                if (lobbyEvents == null || lobbyEvents.Count == 0)
                {
                    m.Status = Status.Pending;
                    continue;
                }

                var latest = lobbyEvents.OrderByDescending(x => x.Timestamp).FirstOrDefault();
                if (latest == null)
                {
                    m.Status = Status.Pending;
                    continue;
                }

                switch (latest.EventType)
                {
                    case "PracticeGameCreatedEvent":
                        m.Status = Status.Lobby;
                        break;
                    case "ChampSelectStartedEvent":
                        m.Status = Status.ChampionSelect;
                        break;
                    case "GameAllocationStartedEvent":
                        m.Status = Status.Loading;
                        break;
                    case "GameAllocatedToLsmEvent":
                        m.Status = Status.InGame;
                        break;
                }
            }
        }

        public void ScrapeFinishedMatches(object args)
        {
            // Disable outside tournament to save API calls
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            var tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            if (DateTime.Now < tournamentStart || DateTime.Now > tournamentStart + TimeSpan.FromHours(12))
                // assuming tournament lasts for a maximum of 12 hours
                return;

            // Find matches that still have no valid matchDetails associated after callback date + 5 minutes
            var matches =
                Mongo.Matches.Find(
                    Query<Match>.Where(
                        x =>
                            x.Finished && x.Invalid && x.InvalidReason == "MATCH_NOT_FOUND")).Where(x => DateTime.UtcNow >= x.FinishDate + TimeSpan.FromMinutes(5));

            foreach (var match in matches)
                GetMatchDetails(_api, match, true);
        }

        public static void GetMatchDetails(TournamentRiotApi api, Match match, bool callNewMatchHook)
        {
            MatchDetail matchDetails;

            // Get more match details
            try
            {
                if (match.RiotMatchId != 0)
                    matchDetails = api.GetTournamentMatch(Region.euw, match.RiotMatchId, match.TournamentCode, false);
                else
                {
                    var matchId = api.GetTournamentMatchId(Region.euw, match.TournamentCode);
                    matchDetails = api.GetTournamentMatch(Region.euw, matchId, match.TournamentCode, false);
                    match.RiotMatchId = matchId;
                }
            }
            catch (Exception)
            {
                try
                {
                    if (match.RiotMatchId != 0)
                        matchDetails = api.GetTournamentMatch(Region.euw, match.RiotMatchId, match.TournamentCodeBlind,
                            false);
                    else
                    {
                        var matchId = api.GetTournamentMatchId(Region.euw, match.TournamentCodeBlind);
                        matchDetails = api.GetTournamentMatch(Region.euw, matchId, match.TournamentCodeBlind, false);
                        match.RiotMatchId = matchId;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }

            // Get the players that were supposed to be on blue side
            var supposedToBeBluePlayers = matchDetails.Participants.Where(
                matchDetailParticipant =>
                    match.BlueTeam.Participants.Select(
                        localMatchParticipant => localMatchParticipant.Summoner.Name.ToLower())
                        .Contains(
                            matchDetails.ParticipantIdentities.Single(
                                identity => identity.ParticipantId == matchDetailParticipant.ParticipantId)
                                .Player.SummonerName.ToLower())).ToList();

            if (supposedToBeBluePlayers.Count == 0)
            {
                // Teams did not have correct summoners playing the match. Match invalid.
                match.Invalid = true;
                match.InvalidReason = "INCORRECT_SUMMONER_COUNT_BLUE_TEAM";

                // Save to database
                Mongo.Matches.Save(match);
            }

            // Set the team id to the side they actually played
            var blueTeamId = supposedToBeBluePlayers.First().TeamId;

            // Get the players that were supposed to be on red side
            var supposedToBeRedPlayers = matchDetails.Participants.Where(
                matchDetailParticipant =>
                    match.RedTeam.Participants.Select(
                        localMatchParticipant => localMatchParticipant.Summoner.Name.ToLower())
                        .Contains(
                            matchDetails.ParticipantIdentities.Single(
                                identity => identity.ParticipantId == matchDetailParticipant.ParticipantId)
                                .Player.SummonerName.ToLower())).ToList();

            if (supposedToBeRedPlayers.Count == 0)
            {
                // Teams did not have correct summoners playing the match. Match invalid.
                match.Invalid = true;
                match.InvalidReason = "INCORRECT_SUMMONER_COUNT_RED_TEAM";

                // Save to database
                Mongo.Matches.Save(match);
            }

            // Set the team id to the side they actually played
            var redTeamId = supposedToBeRedPlayers.First().TeamId;

            // Set statistics
            match.Duration = matchDetails.MatchDuration;
            match.CreationTime = matchDetails.MatchCreation;
            match.RiotMatchId = matchDetails.MatchId;

            match.ChampionIds = matchDetails.Participants.Select(x => x.ChampionId).ToArray();

            // Exclude null bans for blind pick and for teams that forgot all their bans
            match.BanIds =
                matchDetails.Teams.Where(x => x.Bans != null)
                    .SelectMany(x => x.Bans)
                    .Select(x => x.ChampionId)
                    .ToArray();

            match.AssistsBlueTeam =
                matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Assists);
            match.KillsBlueTeam =
                matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Kills);
            match.DeathsBlueTeam =
                matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Deaths);

            match.AssistsRedTeam =
                matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Assists);
            match.KillsRedTeam = matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Kills);
            match.DeathsRedTeam =
                matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Deaths);

            match.Invalid = false;

            if (blueTeamId == 200)
                match.PlayedWrongSide = true;

            // Save to database
            Mongo.Matches.Save(match);

            // Call the new match hook
            if (callNewMatchHook)
                BracketHelper.NewMatch(match);
        }
    }
}