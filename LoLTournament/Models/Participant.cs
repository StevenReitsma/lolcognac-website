using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp.LeagueEndpoint;
using RiotSharp.SummonerEndpoint;

namespace LoLTournament.Models
{
    public class Participant
    {
        public ObjectId Id { get; set; }
        public Summoner Summoner { get; set; }
        public Tier Season4Tier { get; set; }
        public Tier Season5Tier { get; set; }
        public int Season5Division { get; set; }
        public int Season4Wins { get; set; }
        public int Season4Losses { get; set; }
        public string SummonerName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public ObjectId TeamId { get; set; }

        /// <summary>
        /// Returns the Team this participant is competing with. Null if no team.
        /// </summary>
        [BsonIgnore]
        public Team Team
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");

                return col.Find(Query<Team>.Where(x => x.Id == TeamId)).FirstOrDefault();
            }
        }

        public DateTime RegisterTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string StudyProgram { get; set; }
        public bool IsCaptain { get; set; }
        public bool RuStudent { get; set; }

        /// <summary>
        /// Returns the MMR for the participant, based on win/loss ratio in season 4, season 4 tier and season 5 tier and division.
        /// </summary>
        [BsonIgnore]
        public double MMR
        {
            get
            {
                // MMR is based on win/loss ratio, season 4 tier, and season 5 tier and division in the following ratio:
                // Win/Loss    Squared times 35, but only if at least 40 games were played
                // Season 4    Tier level times 10
                // Season 5    Tier level times 2 plus division times 0.4 (this gives a linear function from Bronze V to Challenger).

                double winLossRatio = 0;
                if (Season4Losses != 0)
                    winLossRatio = (double)Season4Wins/Season4Losses;

                double winLossPart = 0;
                if (Season4Losses + Season4Wins > 40)
                    winLossPart = Math.Pow(winLossRatio,2) * 35;

                var s4Part = (int)Season4Tier*10;
                var s5Part = (int)Season5Tier*2 + (5 - Season5Division)*0.4;

                return winLossPart + s4Part + s5Part;
            }
        }
    }
}