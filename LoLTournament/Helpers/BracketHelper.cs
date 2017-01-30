using System;
using System.Collections.Generic;
using System.Linq;
using LoLTournament.Models;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace LoLTournament.Helpers
{
    public class BracketHelper
    {
        /// <summary>
        /// This method creates the pool structure and adds it to the database.
        /// Teams are divided into groups of 4, based on their MMR.
        /// 
        /// Only works when there are 2^N with N >= 4 teams.
        /// </summary>
        public static void CreatePoolStructure()
        {
            // Get competing teams
            var validTeams = Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled))
                .OrderByDescending(x => x.MMR).ToList();

            // For each pool
            for (int i = 0; i < validTeams.Count() / 4; i++)
            {
                Team[] teams =
                {
                    validTeams[i],
                    validTeams[validTeams.Count/2 - i - 1],
                    validTeams[validTeams.Count/2 + i],
                    validTeams[(int) (validTeams.Count*0.75) + i]
                };

                List<int>[] availabilityLists =
                {
                    new List<int> {0, 1, 2},
                    new List<int> {0, 1, 2},
                    new List<int> {0, 1, 2},
                    new List<int> {0, 1, 2}
                };

                // For each team
                for (int j = 0; j < 4; j++)
                {
                    // Set pool in team objects
                    teams[j].Pool = i;
                    Mongo.Teams.Save(teams[j]);

                    // For each team below team j
                    for (int k = j + 1; k < 4; k++)
                    {
                        // Determine priority
                        var intersect = availabilityLists[j].Intersect(availabilityLists[k]);
                        var p = intersect.Min();

                        // Remove chosen priority from team availability lists
                        availabilityLists[j].Remove(p);
                        availabilityLists[k].Remove(p);

                        // Determine sides at random
                        var sides = ShuffleTeamIds(teams[j].Id, teams[k].Id);
                        var blue = sides.Item1;
                        var red = sides.Item2;

                        // Create match and add to database
                        var allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == blue || team.Id == red))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var match = new Match
                        {
                            BlueTeamId = blue,
                            RedTeamId = red,
                            Phase = Phase.Pool,
                            Priority = p,
                            TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners),
                            TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners)
                        };
                        Mongo.Matches.Save(match);
                    }
                }
                
            }
        }

        /// <summary>
        /// This creates a best-out-of-three match for the Finale and a solo match for the Bronze Finale.
        /// This method does not create the necessary tournament codes yet, since the allowed summoners are not yet known.
        /// </summary>
        public static void CreateFinaleStructure()
        {
            // Finale
            for (int p = 0; p < 3; p++)
            {
                var match = new Match {Phase = Phase.Finale, Priority = p};
                Mongo.Matches.Save(match);
            }

            // Losers' finale
            // DISABLED IN 2016 TOURNAMENT
            // var losers = new Match {Phase = Phase.LoserFinale};
            // Mongo.Matches.Save(losers);

            // Bronze finale
            var bronze = new Match { Phase = Phase.BronzeFinale };
            Mongo.Matches.Save(bronze);
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
                    if (pool % 2 == 0)
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
                        var allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == poolARanking[0].Id || team.Id == poolBRanking[1].Id))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var sides = ShuffleTeamIds(poolARanking[0].Id, poolBRanking[1].Id);
                        var match = new Match
                        {
                            BlueTeamId = sides.Item1,
                            RedTeamId = sides.Item2,
                            Phase = Phase.WinnerBracket,
                            Priority = Math.Min(pool, otherPool),
                            TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners),
                            TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners)
                        };

                        allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == poolBRanking[0].Id || team.Id == poolARanking[1].Id))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var sides2 = ShuffleTeamIds(poolBRanking[0].Id, poolARanking[1].Id);
                        var match2 = new Match
                        {
                            BlueTeamId = sides2.Item1,
                            RedTeamId = sides2.Item2,
                            Phase = Phase.WinnerBracket,
                            Priority = Math.Max(pool, otherPool),
                            TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners),
                            TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners)
                        };

                        Mongo.Matches.Save(match);
                        Mongo.Matches.Save(match2);

                        // Loser bracket
                        // DISABLED FOR 2016 TOURNAMENT
                        // var sides3 = ShuffleTeamIds(poolARanking[2].Id, poolBRanking[3].Id);
                        // var sides4 = ShuffleTeamIds(poolBRanking[2].Id, poolARanking[3].Id);
                        // var match3 = new Match { BlueTeamId = sides3.Item1, RedTeamId = sides3.Item2, Phase = Phase.LoserBracket, Priority = Math.Min(pool, otherPool) };
                        // var match4 = new Match { BlueTeamId = sides4.Item1, RedTeamId = sides4.Item2, Phase = Phase.LoserBracket, Priority = Math.Max(pool, otherPool) };
                        // Mongo.Matches.Save(match3);
                        // Mongo.Matches.Save(match4);

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
                    Team[] teams = { finishedMatch.BlueTeam, finishedMatch.RedTeam };
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

                        var allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == finishedMatch.WinnerId || team.Id == otherTeam.Id))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var sides = ShuffleTeamIds(finishedMatch.WinnerId, otherTeam.Id);
                        var match = new Match
                        {
                            BlueTeamId = sides.Item1,
                            RedTeamId = sides.Item2,
                            Phase = finishedMatch.Phase,
                            Priority = Math.Min(finishedMatch.Priority, otherPrio) + 8,
                            TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners),
                            TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners)
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

                        var allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == finishedMatch.WinnerId || team.Id == otherTeam.Id))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var sides = ShuffleTeamIds(finishedMatch.WinnerId, otherTeam.Id);
                        var match = new Match
                        {
                            BlueTeamId = sides.Item1,
                            RedTeamId = sides.Item2,
                            Phase = finishedMatch.Phase,
                            Priority = Math.Min(finishedMatch.Priority, otherPrio) + 4,
                            TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners),
                            TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners)
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
                        var otherId = matchFinished.First().WinnerId;
                        var allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == finishedMatch.WinnerId || team.Id == otherId))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        var sides = ShuffleTeamIds(finishedMatch.WinnerId, matchFinished.First().WinnerId);
                        var match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 0)).First();
                        match.BlueTeamId = sides.Item1;
                        match.RedTeamId = sides.Item2;
                        match.TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners);
                        match.TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners);
                        Mongo.Matches.Save(match);

                        // Switch sides
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 1)).First();
                        match.BlueTeamId = sides.Item2;
                        match.RedTeamId = sides.Item1;
                        match.TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners);
                        match.TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners);
                        Mongo.Matches.Save(match);

                        // Switch sides again
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.Finale && x.Priority == 2)).First();
                        match.BlueTeamId = sides.Item1;
                        match.RedTeamId = sides.Item2;
                        match.TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners);
                        match.TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners);
                        Mongo.Matches.Save(match);

                        // Set team phases
                        var blueTeam = match.BlueTeam;
                        var redTeam = match.RedTeam;

                        blueTeam.Phase = Phase.Finale;
                        blueTeam.OnHold = false;
                        redTeam.Phase = Phase.Finale;
                        redTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(redTeam);

                        // Also set bronze finale match
                        match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.BronzeFinale)).First();
                        var team1 = finishedMatch.BlueTeamId == finishedMatch.WinnerId ? finishedMatch.RedTeamId : finishedMatch.BlueTeamId;
                        var team2 = matchFinished.First().BlueTeamId == matchFinished.First().WinnerId ? matchFinished.First().RedTeamId : matchFinished.First().BlueTeamId;
                        var sidesBronze = ShuffleTeamIds(team1, team2);
                        match.BlueTeamId = sidesBronze.Item1;
                        match.RedTeamId = sidesBronze.Item2;

                        allowedSummoners =
                            Mongo.Teams.Find(Query<Team>.Where(team => team.Id == match.BlueTeamId || team.Id == match.RedTeamId))
                                .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                                .ToList();

                        match.TournamentCode = TournamentCodeFactory.GetTournamentCode(allowedSummoners);
                        match.TournamentCodeBlind = TournamentCodeFactory.GetTournamentCodeBlind(allowedSummoners);
                        Mongo.Matches.Save(match);

                        // And set their teams to the correct phase
                        blueTeam = match.BlueTeam;
                        redTeam = match.RedTeam;

                        blueTeam.Phase = Phase.BronzeFinale;
                        blueTeam.OnHold = false;
                        redTeam.Phase = Phase.BronzeFinale;
                        redTeam.OnHold = false;

                        // Save all to database
                        Mongo.Teams.Save(blueTeam);
                        Mongo.Teams.Save(redTeam);
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
                // DISABLED FOR 2016 TOURNAMENT
                /*else if ((finishedMatch.Priority == 12 || finishedMatch.Priority == 13) && finishedMatch.Phase == Phase.LoserBracket)
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
                        var sides = ShuffleTeamIds(finishedMatch.WinnerId, matchFinished.First().WinnerId);
                        var match = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.LoserFinale && x.Priority == 0)).First();
                        match.BlueTeamId = sides.Item1;
                        match.RedTeamId = sides.Item2;
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
                }*/
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
        /// Returns a tuple of the two team ids in a random order, so both teams have an equal change of playing either side
        /// </summary>
        /// <param name="team1">Team id 1</param>
        /// <param name="team2">Team id 2</param>
        /// <returns></returns>
        private static Tuple<ObjectId, ObjectId> ShuffleTeamIds(ObjectId team1, ObjectId team2)
        {
            var rng = new Random().Next(2) == 1;
            return rng ? new Tuple<ObjectId, ObjectId>(team1, team2) : new Tuple<ObjectId, ObjectId>(team2, team1);
        }
    }
}
