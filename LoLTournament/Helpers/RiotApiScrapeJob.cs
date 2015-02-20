using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using LoLTournament.Models;
using MongoDB.Driver;
using RiotSharp;
using RiotSharp.GameEndpoint;
using RiotSharp.LeagueEndpoint;
using RiotSharp.MatchEndpoint;
using RiotSharp.StatsEndpoint;
using RiotSharp.SummonerEndpoint;
using Participant = LoLTournament.Models.Participant;
using Season = RiotSharp.StatsEndpoint.Season;
using System.Diagnostics;
using System.Globalization;
using Team = LoLTournament.Models.Team;

namespace LoLTournament.Helpers
{
    public class RiotApiScrapeJob
    {
        private readonly RiotApi _api;

        public RiotApiScrapeJob()
        {
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            _api = RiotApi.GetInstance(key, 3000, 180000);

            // For summoner statistics, always 1 hour
            var intervalSummoners = new TimeSpan(1, 0, 0);
            // For matches, every minute
            var intervalMatches = new TimeSpan(0, 1, 0);

            //new Timer(ScrapeSummoners, null, new TimeSpan(0, 0, 0, 0, 0), intervalSummoners);
            //new Timer(ScrapeMatches, null, new TimeSpan(0, 0, 0, 0, 0), intervalMatches);
        }

        private void ScrapeMatches(object arg)
        {
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            var tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            if (DateTime.Now >= tournamentStart && DateTime.Now <= tournamentStart + TimeSpan.FromHours(12)) // assuming tournament lasts for a maximum of 12 hours
            {
                // Tournament is currently ongoing
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");
                var teamCaptains = col.FindAll().Select(x => x.Captain);

                // For each team captain
                foreach (var tc in teamCaptains)
                {
                    // Get match that this team is currently playing (or should be)
                    var nextMatch = tc.Team.GetNextMatch();
                    
                    // No match?
                    if (nextMatch == null)
                        continue;

                    // Query Riot match history for team captain
                    var matchHistory = tc.Summoner.GetRecentGames();

                    // Filter: matches that finished after the tournament start
                    //         matches that have gameMode = CLASSIC
                    //         matches that have gameType = CUSTOM_GAME
                    //         matches that have subType = NONE or NORMAL
                    //         matches that have mapId = 11
                    //         matches that has creation date >= tournament start date
                    var validMatches = from m in matchHistory
                        where 
                              m.GameMode == GameMode.Classic &&
                              m.GameType == GameType.CustomGame &&
                              (m.SubType == GameSubType.None || m.SubType == GameSubType.Normal) &&
                              m.MapType == MapType.SummonersRiftCurrent &&
                              m.CreateDate >= tournamentStart
                        select m;

                    Game validMatch = null;

                    // Check for each match whether it has valid participants
                    foreach (var match in validMatches)
                    {
                        bool valid =
                            match.FellowPlayers.All(
                                p =>
                                    nextMatch.BlueTeam.Participants.Any(x => x.Summoner.Id == p.SummonerId) ||
                                    nextMatch.PurpleTeam.Participants.Any(x => x.Summoner.Id == p.SummonerId));

                        if (valid)
                        {
                            validMatch = match;
                            break;
                        }
                    }

                    // No valid match for this team captain
                    if (validMatch == null)
                        continue;

                    // Get full match statistics
                    var matchData = _api.GetMatch(Region.euw, validMatch.GameId);

                    // validMatch is now the Riot-Match for nextMatch, so set fields
                    nextMatch.AssistsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Assists);
                    nextMatch.DeathsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Deaths);
                    nextMatch.KillsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Kills);

                    nextMatch.AssistsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Assists);
                    nextMatch.DeathsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Deaths);
                    nextMatch.KillsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Kills);

                    nextMatch.Duration = matchData.MatchDuration;
                    nextMatch.FinishTime = matchData.MatchCreation;

                    nextMatch.WinnerId = matchData.Participants.First(x => x.TeamId == 100).Stats.Winner
                        ? nextMatch.BlueTeamId
                        : nextMatch.PurpleTeamId;

                    nextMatch.Finished = true;

                    // Save to database
                    var matchCol = db.GetCollection<Match>("Matches");
                    matchCol.Save(nextMatch);
                }
            }
        }

        private void ScrapeSummoners(object arg)
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Participant>("Participants");

            var participants = col.FindAll();

            // Loop through participants and retrieve their summoner and league information
            foreach (var p in participants)
            {
                // Get summoner information
                Summoner s;
                try
                {
                    s = _api.GetSummoner(Region.euw, p.SummonerName);
                }
                catch (Exception)
                {
                    // Summoner does not exist or Riot API offline
                    continue;
                }

                // Get Season 4 league
                var season4Tier = Tier.Unranked;
                try
                {
                    var previousSeason = _api.GetMatchHistory(Region.euw, s.Id, 0, 1, null, new List<Queue> {Queue.RankedSolo5x5});

                    if (previousSeason != null)
                        season4Tier = previousSeason[0].Participants[0].HighestAchievedSeasonTier;
                }
                catch (Exception)
                {
                    // Unranked in season 4
                }

                // Get wins and losses in season 4
                int wins;
                int losses;
                try
                {
                    var winLoss = _api.GetStatsSummaries(Region.euw, s.Id, Season.Season4);
                    var winLossSoloQueue = winLoss.Single(x => x.PlayerStatSummaryType == PlayerStatsSummaryType.RankedSolo5x5);
                    wins = winLossSoloQueue.Wins;
                    losses = winLossSoloQueue.Losses;
                }
                catch (Exception)
                {
                    // No matches played in season 4
                    wins = 0;
                    losses = 0;
                }

                // Get Season 5 league
                Tier tier;
                int divisionInt;
                try
                {
                    var season5League = _api.GetLeagues(Region.euw, new List<int> {(int) s.Id});
                    var season5LeagueSoloQueue = season5League.First().Value.Single(x => x.Queue == Queue.RankedSolo5x5);
                    tier = season5LeagueSoloQueue.Tier;
                    var division =
                        season5LeagueSoloQueue.Entries.Single(x => x.PlayerOrTeamId == s.Id.ToString()).Division;

                    divisionInt = RomanToInt(division);
                }
                catch (Exception)
                {
                    // Unranked in season 5
                    tier = Tier.Unranked;
                    divisionInt = 0;
                }

                // Set values and update DB
                p.Summoner = s;
                p.Season4Wins = wins;
                p.Season4Losses = losses;
                p.Season4Tier = season4Tier;
                p.Season5Tier = tier;
                p.Season5Division = divisionInt;
                p.LastUpdateTime = DateTime.Now;

                col.Save(p);
            }


            }
        private static int RomanToInt(string numeral)
        {
            switch (numeral)
            {
                case "I":
                    return 1;
                case "II":
                    return 2;
                case "III":
                    return 3;
                case "IV":
                    return 4;
                case "V":
                    return 5;
                default:
                    return 0;
            }
        }
    }
}