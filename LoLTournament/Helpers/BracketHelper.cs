using System;
using System.Collections.Generic;
using System.Linq;
using LoLTournament.Models;
using MongoDB.Driver;

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
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Team>("Teams");
            var matchCol = db.GetCollection<Match>("Matches");

            // Get competing teams
            var validTeams = col.FindAll()
                .OrderByDescending(x => x.AmountOfRuStudents)
                .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                .Take(32)
                .OrderByDescending(x => x.MMR).ToList();

            // For each pool
            for (int i = 0; i < validTeams.Count() / 4; i++)
            {
                Team[] teams = {validTeams[i], validTeams[validTeams.Count()/2 - i - 1], validTeams[validTeams.Count()/2 + i], validTeams[(int)(validTeams.Count()*0.75) + i]};
                List<int>[] availabilityLists = { new List<int> { 0, 1, 2 }, new List<int> { 0, 1, 2 }, new List<int> { 0, 1, 2 }, new List<int> { 0, 1, 2 } };

                // For each team
                for (int j = 0; j < 4; j++)
                {
                    // Set pool in team objects
                    teams[j].Pool = i;
                    col.Save(teams[j]);

                    // For each team below team j
                    for (int k = j + 1; k < 4; k++)
                    {
                        // Determine priority
                        var intersect = availabilityLists[j].Intersect(availabilityLists[k]);
                        var p = intersect.Min();

                        // Remove chosen priority from team availability lists
                        availabilityLists[j].Remove(p);
                        availabilityLists[k].Remove(p);

                        // Determine sides (lowest MMR = blue)
                        var blue = teams[j].MMR < teams[k].MMR ? teams[j].Id : teams[k].Id;
                        var purple = teams[j].MMR >= teams[k].MMR ? teams[j].Id : teams[k].Id;

                        // Create match and add to database
                        var match = new Match { BlueTeamId = blue, PurpleTeamId = purple, Phase = Phase.Pool, Priority = p };
                        matchCol.Save(match);
                    }
                }
                
            }
        }

        /// <summary>
        /// This creates a best-out-of-three match for the Finale and a solo match for the Bronze Finale.
        /// </summary>
        public static void CreateFinaleStructure()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var matchCol = db.GetCollection<Match>("Matches");

            // Finale
            for (int p = 0; p < 3; p++)
            {
                var match = new Match {Phase = Phase.Finale, Priority = p};
                matchCol.Save(match);
            }

            // Losers' finale
            var losers = new Match {Phase = Phase.LoserFinale};
            matchCol.Save(losers);

            // Bronze finale
            var bronze = new Match { Phase = Phase.BronzeFinale };
            matchCol.Save(bronze);
        }
    }
}