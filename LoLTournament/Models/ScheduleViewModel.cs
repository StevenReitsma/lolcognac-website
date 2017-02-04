using System;
using System.Collections.Generic;
using System.Linq;
using LoLTournament.Helpers;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class ScheduleViewModel
    {
        public List<Team> Teams { get; set; }

        /// <summary>
        /// An 2D array where the inner array contains the matchups for the first round of the knockout phase
        /// </summary>
        public string[][] KnockOutTeams { get; set; }

        /// <summary>
        /// An array with shape (n_rounds, n_games, 2) of the scores of each match.
        /// This depends on the order in KnockOutTeams
        /// </summary>
        public int?[][][] KnockOutResults { get; set; }

        private const int NumPools = 8; // number of initial rounds in knockout phase

        public ScheduleViewModel()
        {
            Teams = Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled && !x.Name.Contains("TESTCOGNAC")))
                   .OrderBy(x => x.Pool)
                   .ThenBy(x => x.Name)
                   .ToList();

            if (Teams.Count != 32 || Mongo.Matches.Count() == 0)
            {
                Teams = new List<Team>();
                return;
            }
            
            KnockOutTeams = new string[NumPools][];
            int[] pools = { 0, 1, 4, 5, 7, 6, 3, 2};
            for (int i = 0; i < NumPools; i += 2)
            {
                // Couple pool 1,2 | 3,4 etc.
                var pool = pools[i];
                var otherPool = pools[i + 1];
                // Teams from the same pool should be on the other side of the bracket. 1,5 | 2,6 etc.
                var matchIndex = i/2;
                var otherBracket = matchIndex + NumPools/2; 
                KnockOutTeams[matchIndex] = new string[2];
                KnockOutTeams[otherBracket] = new string[2];
                if (BracketHelper.PoolFinished(pool))
                {
                    var ranks = BracketHelper.GetPoolRanking(pool);
                    KnockOutTeams[matchIndex][0] = ranks[0].Name;
                    KnockOutTeams[otherBracket][1] = ranks[1].Name;
                }
                else
                {
                    KnockOutTeams[matchIndex][0] = "#1 pool " + (pool+1);
                    KnockOutTeams[otherBracket][1] = "#2 pool " + (otherPool+1);
                }

                if (BracketHelper.PoolFinished(otherPool))
                {
                    var ranks = BracketHelper.GetPoolRanking(otherPool);
                    KnockOutTeams[matchIndex][1] = ranks[1].Name;
                    KnockOutTeams[otherBracket][0] = ranks[0].Name;
                }
                else
                {
                    KnockOutTeams[matchIndex][1] = "#2 pool " + (otherPool+1);
                    KnockOutTeams[otherBracket][0] = "#1 pool " + (pool+1);
                }
            }

            int numRounds = (int) Math.Round(Math.Log(NumPools*2, 2));
            KnockOutResults = new int?[numRounds][][];
            var teamsLeft = new ObjectId[NumPools*2];
            for (int i = 0; i < NumPools; i++)
            {
                var team1 = Teams.FirstOrDefault(t => t.Name == KnockOutTeams[i][0]);
                var team2 = Teams.FirstOrDefault(t => t.Name == KnockOutTeams[i][1]);
                teamsLeft[i * 2] = team1?.Id ?? ObjectId.Empty;
                teamsLeft[i * 2 + 1] = team2?.Id ?? ObjectId.Empty;
            }
            for (int i = 0; i < numRounds; i++)
            {
                int numGames = (int) Math.Round(NumPools/Math.Pow(2, i));
                var advancingTeams = new ObjectId[numGames];
                KnockOutResults[i] = new int?[numGames == 1 ? 2 : numGames][]; // Also count bronze final
                for (int j = 0; j < teamsLeft.Length; j += 2)
                {
                    var team1 = teamsLeft[j];
                    var team2 = teamsLeft[j + 1];
                    if (team1 == ObjectId.Empty || team2 == ObjectId.Empty)
                    {
                        KnockOutResults[i][j / 2] = new int?[] { null, null };
                        continue;
                    }
                    // Find the match in the winner's bracket where the two teams played eachother
                    var match = Mongo.Matches.FindOne(Query<Match>.Where(m => 
                                m.Phase == Phase.WinnerBracket && 
                                (
                                    (m.BlueTeamId == team1 && m.RedTeamId == team2) ||
                                    (m.RedTeamId == team1 && m.BlueTeamId == team2)
                                )));
                    if (match != null && match.Finished)
                    {
                        KnockOutResults[i][j/2] = match.WinnerId == team1 ? new int?[] {1, 0} : new int?[] {0, 1};

                        advancingTeams[j/2] = match.WinnerId;
                    }
                    else
                    {
                        KnockOutResults[i][j/2] = new int?[] { null, null };
                    }
                }
                if (numGames == 1)
                {
                    // Add the bronze final
                    KnockOutResults[i][1] = new int?[] { null, null };
                    var bronzeMatch = Mongo.Matches.Find(Query<Match>.Where(x => x.Phase == Phase.BronzeFinale)).First();
                    if (bronzeMatch != null && bronzeMatch.Finished)
                    {
                        var teamNames = KnockOutTeams.SelectMany(t => t).ToArray();
                        KnockOutResults[i][1] = Array.IndexOf(teamNames, bronzeMatch.Winner.Name) < NumPools ? new int?[] { 1, 0 } : new int?[] { 0, 1 };
                    }
                }
                teamsLeft = advancingTeams;

            }

        }
    }
}
