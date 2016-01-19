using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using LoLTournament.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp;
using RiotSharp.LeagueEndpoint;
using RiotSharp.StatsEndpoint;
using RiotSharp.SummonerEndpoint;
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

#if !DEBUG
            new Timer(ScrapeSummoners, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
            new Timer(ScrapeStatic, null, TimeSpan.Zero, TimeSpan.FromDays(1.0));
#endif
        }

        private void ScrapeStatic(object arg)
        {
            Mongo.Champions.DropAllIndexes();
            var champions = _staticApi.GetChampions(Region.euw).Champions;
            foreach(var champion in champions.Keys)
            {
                Mongo.Champions.Save(new Champion { Name = champions[champion].Name, ChampionId = champions[champion].Id });
            }
        }

        /// <summary>
        /// Checks whether the match that was just played has consequences for the structure of the rest of the tournament. If so, push these consequences into the database.
        /// </summary>
        /// <param name="finishedMatch">The match that was just finished.</param>
        public static void NewMatch(Match finishedMatch)
        {
            var finishedMatchWinner = finishedMatch.Winner;

            if (finishedMatch.Phase == Phase.Pool)
            {
                var pool = finishedMatch.BlueTeam.Pool;
                
                // Check whether the pool is finished
                if (PoolFinished(pool))
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
                    if (PoolFinished(otherPool))
                    {
                        // Get ranking for pools
                        var poolARanking = GetPoolRanking(pool);
                        var poolBRanking = GetPoolRanking(otherPool);

                        // We can now create the brackets for these pools
                        // Winner bracket
                        var match = new Match {BlueTeamId = poolARanking[0].Id, RedTeamId = poolBRanking[1].Id, Phase = Phase.WinnerBracket, Priority = Math.Min(pool, otherPool)};
                        var match2 = new Match { BlueTeamId = poolBRanking[0].Id, RedTeamId = poolARanking[1].Id, Phase = Phase.WinnerBracket, Priority = Math.Max(pool, otherPool)};
                        Mongo.Matches.Save(match);
                        Mongo.Matches.Save(match2);

                        // Loser bracket
                        var match3 = new Match { BlueTeamId = poolARanking[2].Id, RedTeamId = poolBRanking[3].Id, Phase = Phase.LoserBracket, Priority = Math.Min(pool, otherPool) };
                        var match4 = new Match { BlueTeamId = poolBRanking[2].Id, RedTeamId = poolARanking[3].Id, Phase = Phase.LoserBracket, Priority = Math.Max(pool, otherPool) };
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
                        var teams = Mongo.Teams.Find(Query<Team>.Where(x => x.Pool == pool && !x.Cancelled));
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
                    Team[] teams = {finishedMatch.BlueTeam, finishedMatch.RedTeam};
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
                            RedTeamId = otherTeam.Id,
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
                            RedTeamId = matchFinished.First().WinnerId,
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
                        match.RedTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Switch sides
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 1)).First();
                        match.BlueTeamId = matchFinished.First().WinnerId;
                        match.RedTeamId = finishedMatch.WinnerId;
                        Mongo.Matches.Save(match);

                        // Switch sides again
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 2)).First();
                        match.BlueTeamId = finishedMatch.WinnerId;
                        match.RedTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Set team phases
                        var blueTeam = match.BlueTeam;
                        var purpleTeam = match.RedTeam;

                        blueTeam.Phase = Phase.Finale;
                        blueTeam.OnHold = false;
                        purpleTeam.Phase = Phase.Finale;
                        purpleTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(purpleTeam);

                        // Also set bronze finale match
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.BronzeFinale)).First();
                        match.BlueTeamId = finishedMatch.BlueTeamId == finishedMatch.WinnerId ? finishedMatch.RedTeamId : finishedMatch.BlueTeamId;
                        match.RedTeamId = matchFinished.First().BlueTeamId == matchFinished.First().WinnerId ? matchFinished.First().RedTeamId : matchFinished.First().BlueTeamId;
                        Mongo.Matches.Save(match);

                        // And set their teams to the correct phase
                        blueTeam = match.BlueTeam;
                        purpleTeam = match.RedTeam;

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
                            ? finishedMatch.RedTeam
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
                        match.RedTeamId = matchFinished.First().WinnerId;
                        Mongo.Matches.Save(match);

                        // Set team phases
                        var blueTeam = match.BlueTeam;
                        var purpleTeam = match.RedTeam;

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
                            ? finishedMatch.RedTeam
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
                var loser = finishedMatch.BlueTeamId == winner.Id ? finishedMatch.RedTeam : finishedMatch.BlueTeam;
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
                    var winner = winsBlueTeam >= 2 ? finishedMatch.BlueTeam : finishedMatch.RedTeam;
                    var loser = winner.Id == finishedMatch.BlueTeamId ? finishedMatch.RedTeam : finishedMatch.BlueTeam;

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
                        var loser = winner.Id == finishedMatch.BlueTeamId ? finishedMatch.RedTeam : finishedMatch.BlueTeam;

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
        private static bool PoolFinished(int pool)
        {
            // It doesn't matter which side we look at since they are both in the same pool
            var notFinished = Mongo.Matches.FindAll().Where(x => x.BlueTeam != null && x.BlueTeam.Pool == pool && x.Phase == Phase.Pool && !x.Finished);

            // Check whether we have no unfinished matches in the pool
            return !notFinished.Any();
        }

        /// <summary>
        /// Returns the pool ranking, with the first element being the pool winner and the last being the loser.
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        private static List<Team> GetPoolRanking(int pool)
        {
            return Mongo.Teams.Find(Query<Team>.Where(x => x.Pool == pool && !x.Cancelled)).OrderBy(x => x.PoolRank).ToList();
        }

        /// <summary>
        /// Scrapes the summoner statistics for all participants from the Riot API.
        /// </summary>
        /// <param name="arg">Unused</param>
        private void ScrapeSummoners(object arg)
        {
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

                // Get previous season league
                var previousTier = Tier.Unranked;
                try
                {
                    var rankedGames = _api.GetMatchList(Region.euw, s.Id, null, new List<Queue> {Queue.RankedSolo5x5});

                    if (rankedGames != null && rankedGames.Matches.Count > 0)
                    {
                        var id = rankedGames.Matches.First().MatchID;
                        var match = _api.GetMatch(Region.euw, id);
                        if (match != null)
                            previousTier = match.Participants.Single(x => x.ParticipantId == match.ParticipantIdentities.Single(y => y.Player.SummonerId == s.Id).ParticipantId).HighestAchievedSeasonTier;
                    }
                }
                catch (Exception ex)
                {
                    // Unranked in previous season
                }

                // Get wins and losses in previous season
                int wins;
                int losses;
                try
                {
                    var winLoss = _api.GetStatsSummaries(Region.euw, s.Id, Season.Season2015); // change each year
                    var winLossSoloQueue =
                        winLoss.Single(x => x.PlayerStatSummaryType == PlayerStatsSummaryType.RankedSolo5x5);
                    wins = winLossSoloQueue.Wins;
                    losses = winLossSoloQueue.Losses;
                }
                catch (Exception ex)
                {
                    // No matches played in previous season
                    wins = 0;
                    losses = 0;
                }

                // Get current league
                Tier tier;
                int divisionInt;
                try
                {
                    var currentLeague = _api.GetLeagues(Region.euw, new List<int> {(int) s.Id});
                    var currentLeagueSoloQueue = currentLeague.First().Value.Single(x => x.Queue == Queue.RankedSolo5x5);
                    tier = currentLeagueSoloQueue.Tier;
                    var division =
                        currentLeagueSoloQueue.Entries.Single(x => x.PlayerOrTeamId == s.Id.ToString()).Division;

                    divisionInt = RomanToInt(division);
                }
                catch (Exception)
                {
                    // Unranked in current season
                    tier = Tier.Unranked;
                    divisionInt = 0;
                }

                // Set values and update DB
                p.Summoner = s;
                p.PreviousSeasonWins = wins;
                p.PreviousSeasonLosses = losses;
                p.PreviousSeasonTier = previousTier;
                p.CurrentSeasonTier = tier;
                p.CurrentSeasonDivision = divisionInt;
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