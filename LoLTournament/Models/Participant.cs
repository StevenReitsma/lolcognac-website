using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
        public int CurrentSeasonWins { get; set; }
        public int CurrentSeasonLosses { get; set; }
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
                var previousSeason = 10 * ((7 - LeagueMath.GetLeagueMultiplier(PreviousSeasonTier))*5 + 3);
                var currentSeason = 10 * ((7 - LeagueMath.GetLeagueMultiplier(CurrentSeasonTier))*5 + (5 - CurrentSeasonDivision));

                if (PreviousSeasonTier == Tier.Unranked)
                    return currentSeason;
                if (CurrentSeasonTier == Tier.Unranked)
                    return previousSeason;

                return previousSeason * DynamicRatio + currentSeason * (1 - DynamicRatio);
            }
        }

        [BsonIgnore]
        public double DynamicRatio
        {
            get
            {
                if (PreviousSeasonMMRUncertainty == 0)
                    return 0;
                if (CurrentSeasonMMRUncertainty == 0)
                    return 1;

                var sum = (1/PreviousSeasonMMRUncertainty) + (1/CurrentSeasonMMRUncertainty);
                var expandPrevious = (1/PreviousSeasonMMRUncertainty)/sum;

                return expandPrevious;
            }
        }

        [BsonIgnore]
        public double PreviousSeasonMMRUncertainty
        {
            get
            {
                if (PreviousSeasonLosses == 0)
                    return 0;
                if (PreviousSeasonTier == Tier.Unranked)
                    return 0;

                var winrate = PreviousSeasonWins / (double)PreviousSeasonLosses;

                var winrateDeviation = Math.Abs(winrate - 1) + 0.08;
                var matchCountDeviation = 1 / (PreviousSeasonWins + (double) PreviousSeasonLosses);

                return winrateDeviation * matchCountDeviation * 1000 + 2; // add constant because we don't have division
            }
        }

        [BsonIgnore]
        public double CurrentSeasonMMRUncertainty
        {
            get
            {
                if (CurrentSeasonLosses == 0)
                    return 0;
                if (CurrentSeasonTier == Tier.Unranked)
                    return 0;

                var winrate = CurrentSeasonWins / (double)CurrentSeasonLosses;

                var winrateDeviation = Math.Abs(winrate - 1) + 0.08;
                var matchCountDeviation = 1 / (CurrentSeasonWins + (double)CurrentSeasonLosses);

                return winrateDeviation * matchCountDeviation * 1000;
            }
        }

        [BsonIgnore]
        public double MMRUncertainty
        {
            get
            {
                var uncertainty = DynamicRatio*PreviousSeasonMMRUncertainty + (1 - DynamicRatio)*CurrentSeasonMMRUncertainty;
                return uncertainty > 5 ? 5 : uncertainty;
            }
        }

        [BsonIgnore]
        public bool Cancelled
        {
            get
            {
                return Team == null ? true : Team.Cancelled;
            }
        }
    }
}
