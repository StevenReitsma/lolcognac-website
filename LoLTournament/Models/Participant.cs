using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp.LeagueEndpoint;
using RiotSharp.SummonerEndpoint;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class Participant
    {
        [BsonIgnore]
        private const double PreviousRatio = 0.67;

        public ObjectId Id { get; set; }
        public Summoner Summoner { get; set; }
        public Tier PreviousSeasonTier { get; set; }
        public Tier CurrentSeasonTier { get; set; }
        public int CurrentSeasonDivision { get; set; }
        public int PreviousSeasonWins { get; set; }
        public int PreviousSeasonLosses { get; set; }
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
                return Mongo.Teams.Find(Query<Team>.Where(x => x.Id == TeamId)).FirstOrDefault();
            }
        }

        public DateTime RegisterTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string StudyProgram { get; set; }
        public bool IsCaptain { get; set; }
        public bool RuStudent { get; set; }
        public bool CognAC { get; set; }
        public bool Dorans { get; set; }
        public string StudentNumber { get; set; }

        /// <summary>
        /// Returns the MMR for the participant, based on season 4 tier and season 5 tier and division.
        /// </summary>
        [BsonIgnore]
        public double MMR
        {
            get
            {
                double tierRanking = 0;

                if (PreviousSeasonTier != Tier.Unranked)
                    tierRanking += (int)PreviousSeasonTier * 5 + 3; // the +3 is because this is the average division

                // If also ranked in current season, take weighted average (previous = 0.67, current = 0.33)
                if (CurrentSeasonTier != Tier.Unranked)
                {
                    tierRanking *= PreviousRatio;
                    tierRanking += (1 - PreviousRatio) * ((int)CurrentSeasonTier * 5 + 5 - CurrentSeasonDivision);
                    if (PreviousSeasonTier != Tier.Unranked)
                        tierRanking /= (1 - PreviousRatio); // undo ratio multiplication
                }

                return tierRanking * 3;
            }
        }
    }
}