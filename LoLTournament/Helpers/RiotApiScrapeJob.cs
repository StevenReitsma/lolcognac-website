using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using LoLTournament.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp;
using RiotSharp.GameEndpoint;
using RiotSharp.LeagueEndpoint;
using RiotSharp.MatchEndpoint;
using RiotSharp.StatsEndpoint;
using RiotSharp.SummonerEndpoint;
using Participant = LoLTournament.Models.Participant;
using Season = RiotSharp.StatsEndpoint.Season;
using System.Globalization;
using Team = LoLTournament.Models.Team;

namespace LoLTournament.Helpers
{
    public class RiotApiScrapeJob
    {
        private readonly RiotApi _api;
        private readonly StaticRiotApi _staticApi;

        public RiotApiScrapeJob()
        {
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            _api = RiotApi.GetInstance(key, rateLimit1, rateLimit2);
            _staticApi = StaticRiotApi.GetInstance(key);

            // For summoner statistics, always 1 hour
            var intervalSummoners = new TimeSpan(1, 0, 0);
            // For matches, every minute
            var intervalMatches = new TimeSpan(0, 1, 0);

            //new Timer(ScrapeSummoners, null, new TimeSpan(0, 0, 0, 0, 0), intervalSummoners);
            new Timer(ScrapeMatches, null, new TimeSpan(0, 0, 0, 0, 0), intervalMatches);
            new Timer(ScrapeStatic, null, TimeSpan.Zero, TimeSpan.FromDays(1.0));
        }

        private void ScrapeStatic(object arg)
        {
            Mongo.Champions.DropAllIndexes();
            var champions = _staticApi.GetChampions(Region.euw).Champions;
            foreach(var champion in champions.Keys)
            {
                Mongo.Champions.Save(new Champion() { Name = champions[champion].Name, ChampionId = champions[champion].Id });
            }
        }

        private void ScrapeMatches(object arg)
        {
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            var tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            // Tournament is currently ongoing
            var teamCaptains = Mongo.Teams.FindAll().Select(x => x.Captain);

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

                // Get:
                //         matches that have gameMode = CLASSIC
                //         matches that have gameType = CUSTOM_GAME
                //         matches that have subType = NONE or NORMAL
                //         matches that have mapId = 11
                //         matches that has creation date >= tournament start date
                //         matches that have 9 fellow players
                //         matches that are not invalid
                var validMatches = from m in matchHistory
                    where 
                        m.GameMode == GameMode.Classic &&
                        m.GameType == GameType.CustomGame &&
                        (m.SubType == GameSubType.None || m.SubType == GameSubType.Normal) &&
                        m.MapType == MapType.SummonersRift &&
                        m.FellowPlayers.Count == 9 &&
                        !m.Invalid
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
                MatchDetail matchData = null;
                try
                {
                    matchData = _api.GetMatch(Region.euw, validMatch.GameId);

                }
                catch (Exception)
                {
                    // wtf?
                }

                if (matchData == null)
                {
                    nextMatch.WinnerId = (validMatch.Statistics.Team == 100 && validMatch.Statistics.Win) || (validMatch.Statistics.Team == 200 && !validMatch.Statistics.Win)
                        ? nextMatch.BlueTeamId
                        : nextMatch.PurpleTeamId;
                    nextMatch.Duration = validMatch.Statistics.TimePlayed;
                    nextMatch.CreationTime = validMatch.CreateDate + new TimeSpan(0, 2, 0, 0); // add two hours for timezone difference
                    nextMatch.RiotMatchId = validMatch.GameId;
                    nextMatch.Finished = true;
                    nextMatch.FinishDate = DateTime.Now;
                    nextMatch.ChampionIds = validMatch.FellowPlayers.Select(p => p.ChampionId).Concat(new[] { validMatch.ChampionId }).ToArray();
                    // Save to database
                    Mongo.Matches.Save(nextMatch);

                    NewMatch(nextMatch);
                    continue;
                }

                // validMatch is now the Riot-Match for nextMatch, so set fields
                nextMatch.AssistsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Assists);
                nextMatch.DeathsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Deaths);
                nextMatch.KillsBlueTeam = matchData.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Kills);

                nextMatch.AssistsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Assists);
                nextMatch.DeathsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Deaths);
                nextMatch.KillsPurpleTeam = matchData.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Kills);

                nextMatch.Duration = matchData.MatchDuration;
                nextMatch.CreationTime = matchData.MatchCreation + new TimeSpan(0, 2, 0, 0); // add two hours for timezone difference;
                nextMatch.FinishDate = DateTime.Now;

                nextMatch.ChampionIds = matchData.Participants.Select(p => p.ChampionId).ToArray();

                nextMatch.WinnerId = matchData.Participants.First(x => x.TeamId == 100).Stats.Winner
                    ? nextMatch.BlueTeamId
                    : nextMatch.PurpleTeamId;

                // Check if teams played correct side. If not, switch winner.
                // If captain team equals team that should have played blue side
                bool shouldPlayBlue = nextMatch.BlueTeamId == tc.TeamId; // team captain should have played blue side
                bool playedBlue = matchData.Participants.Any(x => x.TeamId == 100 && x.ParticipantId == tc.Summoner.Id); // team captain played blue side
                if ((shouldPlayBlue && !playedBlue) || (!shouldPlayBlue && playedBlue)) // if sides don't match up
                    // Switch
                    nextMatch.WinnerId = nextMatch.WinnerId == nextMatch.BlueTeamId
                        ? nextMatch.PurpleTeamId
                        : nextMatch.BlueTeamId;

                nextMatch.Finished = true;
                nextMatch.RiotMatchId = validMatch.GameId;

                // Save to database
                Mongo.Matches.Save(nextMatch);

                // New match hook
                NewMatch(nextMatch);
            }
        }

        /// <summary>
        /// Checks whether the match that was just played has consequences for the structure of the rest of the tournament. If so, push these consequences into the database.
        /// </summary>
        /// <param name="finishedMatch">The match that was just finished.</param>
        private static void NewMatch(Match finishedMatch)
        {
            var finishedMatchWinner = finishedMatch.Winner;

            if (finishedMatch.Phase == Phase.Pool)
            {
                var pool = finishedMatch.BlueTeam.Pool;
                
                // Check whether the pool is finished
                if (PoolFinished(pool, Mongo.Matches))
                {
                    // Pool is finished
                    // To create brackets, one of the other pools also needs to be finished.
                    // Pools are coupled as follows: 1,2 | 3,4 | etc.

                    int otherPool;

                    // If this is an even pool
                    if (pool%2 == 0)
                        otherPool = pool + 1;
                    else
                        otherPool = pool - 1;

                    // If coupled pool is also finished
                    if (PoolFinished(otherPool, Mongo.Matches))
                    {
                        // Get ranking for pools
                        var poolARanking = GetPoolRanking(pool, Mongo.Teams);
                        var poolBRanking = GetPoolRanking(otherPool, Mongo.Teams);

                        // We can now create the brackets for these pools
                        // Winner bracket
                        var match = new Match {BlueTeamId = poolARanking[0].Id, PurpleTeamId = poolBRanking[1].Id, Phase = Phase.WinnerBracket, Priority = Math.Min(pool, otherPool)};
                        var match2 = new Match { BlueTeamId = poolBRanking[0].Id, PurpleTeamId = poolARanking[1].Id, Phase = Phase.WinnerBracket, Priority = Math.Max(pool, otherPool)};
                        Mongo.Matches.Save(match);
                        Mongo.Matches.Save(match2);

                        // Loser bracket
                        var match3 = new Match { BlueTeamId = poolARanking[2].Id, PurpleTeamId = poolBRanking[3].Id, Phase = Phase.LoserBracket, Priority = Math.Min(pool, otherPool) };
                        var match4 = new Match { BlueTeamId = poolBRanking[2].Id, PurpleTeamId = poolARanking[3].Id, Phase = Phase.LoserBracket, Priority = Math.Max(pool, otherPool) };
                        Mongo.Matches.Save(match3);
                        Mongo.Matches.Save(match4);

                        // Set the teams to the correct phase
                        poolARanking[0].Phase = Phase.WinnerBracket;
                        poolARanking[0].OnHold = false;
                        poolARanking[1].Phase = Phase.WinnerBracket;
                        poolARanking[1].OnHold = false;
                        poolARanking[2].Phase = Phase.LoserBracket;
                        poolARanking[2].OnHold = false;
                        poolARanking[3].Phase = Phase.LoserBracket;
                        poolARanking[3].OnHold = false;

                        poolBRanking[0].Phase = Phase.WinnerBracket;
                        poolBRanking[0].OnHold = false;
                        poolBRanking[1].Phase = Phase.WinnerBracket;
                        poolBRanking[1].OnHold = false;
                        poolBRanking[2].Phase = Phase.LoserBracket;
                        poolBRanking[2].OnHold = false;
                        poolBRanking[3].Phase = Phase.LoserBracket;
                        poolBRanking[3].OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(poolARanking[0]);
                        Mongo.Teams.Save(poolARanking[1]);
                        Mongo.Teams.Save(poolARanking[2]);
                        Mongo.Teams.Save(poolARanking[3]);

                        Mongo.Teams.Save(poolBRanking[0]);
                        Mongo.Teams.Save(poolBRanking[1]);
                        Mongo.Teams.Save(poolBRanking[2]);
                        Mongo.Teams.Save(poolBRanking[3]);
                    }
                    else
                    {
                        // Set all teams in pool to Hold
                        var teams = Mongo.Teams.Find(Query<Team>.Where(x => x.Pool == pool));
                        foreach (var t in teams)
                        {
                            t.OnHold = true;
                            Mongo.Teams.Save(t);
                        }
                    }
                }
                // If this was the final match in the pool for this team
                else if (finishedMatch.Priority == 2)
                {
                    // Set match teams on hold since they don't have any matches left
                    Team[] teams = {finishedMatch.BlueTeam, finishedMatch.PurpleTeam};
                    foreach (var t in teams)
                    {
                        t.OnHold = true;
                        Mongo.Teams.Save(t);
                    }
                }
            }
            else if (finishedMatch.Phase == Phase.WinnerBracket || finishedMatch.Phase == Phase.LoserBracket)
            {
                // First phase
                if (finishedMatch.Priority <= 7)
                {
                    int[] couples =
                    {
                        4,
                        5,
                        6,
                        7,
                        0,
                        1,
                        2,
                        3
                    };

                    // Check if other match is also finished
                    var otherPrio = couples[finishedMatch.Priority];
                    var matchFinished =
                        Mongo.Matches.Find(
                            Query<Match>.Where(
                                x => x.Finished && x.Phase == finishedMatch.Phase && x.Priority == otherPrio));

                    // It's finished, add new bracket match
                    if (matchFinished.Count() == 1)
                    {
                        var otherTeam = matchFinished.First().Winner;

                        var match = new Match
                        {
                            BlueTeamId = finishedMatch.WinnerId,
                            PurpleTeamId = otherTeam.Id,
                            Phase = finishedMatch.Phase,
                            Priority = Math.Min(finishedMatch.Priority, otherPrio) + 8
                        };

                        Mongo.Matches.Save(match);

                        // Set OnHold = false for teams
                        finishedMatchWinner.OnHold = false;
                        otherTeam.OnHold = false;

                        Mongo.Teams.Save(finishedMatch.Winner);
                        Mongo.Teams.Save(otherTeam);
                    }
                    else
                    {
                        // Not finished, put team on hold
                        finishedMatchWinner.OnHold = true;
                        Mongo.Teams.Save(finishedMatchWinner);
                    }
                }
                // Second phase
                else if (finishedMatch.Priority >= 8 && finishedMatch.Priority <= 11)
                {
                    var couples = new Dictionary<int, int>
                    {
                        {8, 10},
                        {10, 8},
                        {9, 11},
                        {11, 9}
                    };

                    // Check if other match is also finished
                    var otherPrio = couples[finishedMatch.Priority];
                    var matchFinished =
                        Mongo.Matches.Find(
                            Query<Match>.Where(
                                x => x.Finished && x.Phase == finishedMatch.Phase && x.Priority == otherPrio));

                    // It's finished, add new bracket match
                    if (matchFinished.Count() == 1)
                    {
                        var otherTeam = matchFinished.First().Winner;

                        var match = new Match
                        {
                            BlueTeamId = finishedMatch.WinnerId,
                            PurpleTeamId = matchFinished.First().WinnerId,
                            Phase = finishedMatch.Phase,
                            Priority = Math.Min(finishedMatch.Priority, otherPrio) + 4
                        };

                        Mongo.Matches.Save(match);

                        // Set OnHold = false for teams
                        finishedMatchWinner.OnHold = false;
                        otherTeam.OnHold = false;

                        Mongo.Teams.Save(finishedMatch.Winner);
                        Mongo.Teams.Save(otherTeam);
                    }
                    else
                    {
                        // Not finished, put team on hold
                        finishedMatchWinner.OnHold = true;
                        Mongo.Teams.Save(finishedMatchWinner);
                    }
                }
                // Third phase, don't do this for loser bracket
                else if ((finishedMatch.Priority == 12 || finishedMatch.Priority == 13) && finishedMatch.Phase == Phase.WinnerBracket)
                {
                    // Check if other match is also finished
                    var otherPrio = finishedMatch.Priority == 12 ? 13 : 12;
                    var matchFinished =
                        Mongo.Matches.Find(
                            Query<Match>.Where(
                                x => x.Finished && x.Phase == Phase.WinnerBracket && x.Priority == otherPrio));

                    // It's finished, set finale match
                    if (matchFinished.Count() == 1)
                    {
                        var match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 0)).First();
                        match.BlueTeamId = finishedMatch.WinnerId;
                        match.PurpleTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Switch sides
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 1)).First();
                        match.BlueTeamId = matchFinished.First().WinnerId;
                        match.PurpleTeamId = finishedMatch.WinnerId;
                        Mongo.Matches.Save(match);

                        // Switch sides again
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 2)).First();
                        match.BlueTeamId = finishedMatch.WinnerId;
                        match.PurpleTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Set team phases
                        var blueTeam = match.BlueTeam;
                        var purpleTeam = match.PurpleTeam;

                        blueTeam.Phase = Phase.Finale;
                        blueTeam.OnHold = false;
                        purpleTeam.Phase = Phase.Finale;
                        purpleTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(purpleTeam);

                        // Also set bronze finale match
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.BronzeFinale)).First();
                        match.BlueTeamId = finishedMatch.BlueTeamId == finishedMatch.WinnerId ? finishedMatch.PurpleTeamId : finishedMatch.BlueTeamId;
                        match.PurpleTeamId = matchFinished.First().BlueTeamId == matchFinished.First().WinnerId ? matchFinished.First().PurpleTeamId : matchFinished.First().BlueTeamId;
                        Mongo.Matches.Save(match);

                        // And set their teams to the correct phase
                        blueTeam = match.BlueTeam;
                        purpleTeam = match.PurpleTeam;

                        blueTeam.Phase = Phase.BronzeFinale;
                        blueTeam.OnHold = false;
                        purpleTeam.Phase = Phase.BronzeFinale;
                        purpleTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(purpleTeam);
                    }
                    else
                    {
                        // Not finished, put team on hold
                        finishedMatchWinner.OnHold = true;
                        var loser = finishedMatchWinner.Id == finishedMatch.BlueTeamId
                            ? finishedMatch.PurpleTeam
                            : finishedMatch.BlueTeam;
                        loser.OnHold = true;
                        Mongo.Teams.Save(finishedMatchWinner);
                        Mongo.Teams.Save(loser);
                    }
                }
                // Set up loser finale
                else if ((finishedMatch.Priority == 12 || finishedMatch.Priority == 13) && finishedMatch.Phase == Phase.LoserBracket)
                {
                    // Check if other match is also finished
                    var otherPrio = finishedMatch.Priority == 12 ? 13 : 12;
                    var matchFinished =
                        Mongo.Matches.Find(
                            Query<Match>.Where(
                                x => x.Finished && x.Phase == Phase.LoserBracket && x.Priority == otherPrio));

                    // It's finished, set loser finale match
                    if (matchFinished.Count() == 1)
                    {
                        var match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.LoserFinale && x.Priority == 0)).First();
                        match.BlueTeamId = finishedMatch.WinnerId;
                        match.PurpleTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Set team phases
                        var blueTeam = match.BlueTeam;
                        var purpleTeam = match.PurpleTeam;

                        blueTeam.Phase = Phase.LoserFinale;
                        blueTeam.OnHold = false;
                        purpleTeam.Phase = Phase.LoserFinale;
                        purpleTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(purpleTeam);
                    }
                    else
                    {
                        // Not finished, put team on hold
                        finishedMatchWinner.OnHold = true;
                        var loser = finishedMatchWinner.Id == finishedMatch.BlueTeamId
                            ? finishedMatch.PurpleTeam
                            : finishedMatch.BlueTeam;
                        loser.OnHold = true;
                        Mongo.Teams.Save(finishedMatchWinner);
                        Mongo.Teams.Save(loser);
                    }
                }
            }
            else if (finishedMatch.Phase == Phase.BronzeFinale)
            {
                // Add third and fourth place to database
                var winner = finishedMatch.Winner;
                var loser = finishedMatch.BlueTeamId == winner.Id ? finishedMatch.PurpleTeam : finishedMatch.BlueTeam;
                winner.FinalRanking = 3;
                loser.FinalRanking = 4;

                Mongo.Teams.Save(winner);
                Mongo.Teams.Save(loser);
            }
            else if (finishedMatch.Phase == Phase.Finale)
            {
                if (finishedMatch.Priority == 2)
                {
                    // We have played all three matches, add final rankings to database
                    var winsBlueTeam = Mongo.Matches.Count(Query<Match>.Where(x => x.Phase == Phase.Finale && x.WinnerId == finishedMatch.BlueTeamId));
                    var winner = winsBlueTeam >= 2 ? finishedMatch.BlueTeam : finishedMatch.PurpleTeam;
                    var loser = winner.Id == finishedMatch.BlueTeamId ? finishedMatch.PurpleTeam : finishedMatch.BlueTeam;

                    winner.FinalRanking = 1;
                    loser.FinalRanking = 2;

                    Mongo.Teams.Save(winner);
                    Mongo.Teams.Save(loser);
                }
                else if (finishedMatch.Priority == 1)
                {
                    // We have played two matches so far
                    var winsBlueTeam = Mongo.Matches.Count(Query<Match>.Where(x => x.Phase == Phase.Finale && x.WinnerId == finishedMatch.BlueTeamId));
                    if (winsBlueTeam == 0 || winsBlueTeam == 2)
                    {
                        // It's already decided, scratch third match
                        Mongo.Matches.Remove(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 2));

                        var winner = finishedMatch.Winner; // winner is always also the winner of the previous match
                        var loser = winner.Id == finishedMatch.BlueTeamId ? finishedMatch.PurpleTeam : finishedMatch.BlueTeam;

                        winner.FinalRanking = 1;
                        loser.FinalRanking = 2;

                        Mongo.Teams.Save(winner);
                        Mongo.Teams.Save(loser);
                    }
                }
                else if (finishedMatch.Priority == 0)
                {
                    // This was the first match, do nothing
                }
            }
        }

        /// <summary>
        /// Whether the given pool is finished with all their matches.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="matchCol"></param>
        /// <returns></returns>
        private static bool PoolFinished(int pool, MongoCollection<Match> matchCol)
        {
            // It doesn't matter which side we look at since they are both in the same pool
            var notFinished = matchCol.FindAll().Where(x => x.BlueTeam != null && x.BlueTeam.Pool == pool && x.Phase == Phase.Pool && !x.Finished);

            // Check whether we have no unfinished matches in the pool
            return !notFinished.Any();
        }

        /// <summary>
        /// Returns the pool ranking, with the first element being the pool winner and the last being the loser.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="teamCol"></param>
        /// <param name="matchCol"></param>
        /// <returns></returns>
        private static List<Team> GetPoolRanking(int pool, MongoCollection<Team> teamCol)
        {
            return teamCol.Find(Query<Team>.Where(x => x.Pool == pool)).OrderBy(x => x.PoolRank).ToList();
        }

        /// <summary>
        /// Scrapes the summoner statistics for all participants from the Riot API.
        /// </summary>
        /// <param name="arg">Unused</param>
        private void ScrapeSummoners(object arg)
        {
            // Fix time creation date and match statistics
            TimeFixer.FixMatchCreationDates();

            // Disable during tournament to save API calls
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            var tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            if (DateTime.Now >= tournamentStart && DateTime.Now <= tournamentStart + TimeSpan.FromHours(12))
                // assuming tournament lasts for a maximum of 12 hours
                return;

            var participants = Mongo.Participants.FindAll();

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
                    var previousSeason = _api.GetMatchHistory(Region.euw, s.Id, 0, 1, null,
                        new List<Queue> {Queue.RankedSolo5x5});

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
                    var winLoss = _api.GetStatsSummaries(Region.euw, s.Id, Season.Season2014);
                    var winLossSoloQueue =
                        winLoss.Single(x => x.PlayerStatSummaryType == PlayerStatsSummaryType.RankedSolo5x5);
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

                Mongo.Participants.Save(p);
            }
        }

        /// <summary>
        /// Converts a Roman numeral between 1 and 5 to an integer.
        /// </summary>
        /// <param name="numeral">A roman numeral between 1 and 5, inclusive.</param>
        /// <returns>An integer between 1 and 5. 0 if invalid argument.</returns>
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