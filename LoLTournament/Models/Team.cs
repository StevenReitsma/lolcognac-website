using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class Team
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public List<ObjectId> ParticipantsIds { get; set; }
        public int Pool { get; set; }
        public Phase Phase { get; set; }
        public int FinalRanking { get; set; }
        public bool OnHold { get; set; }

        [BsonIgnore]
        public List<Participant> Participants
        {
            get
            {
                return Mongo.Participants.Find(Query<Participant>.Where(x => x.TeamId == Id)).ToList();
            }
        }

        public ObjectId CaptainId { get; set; }
        
        [BsonIgnore]
        public Participant Captain {
            get
            {
                return Mongo.Participants.Find(Query<Participant>.Where(x => x.TeamId == Id && x.IsCaptain)).First();
            }
        }

        [BsonIgnore]
        public double MMR
        {
            get { return Participants.Sum(x => x.MMR); }
        }

        [BsonIgnore]
        public int AmountOfRuStudents
        {
            get { return Participants.Count(x => x.RuStudent); }
        }

        [BsonIgnore]
        public long Wins
        {
            get
            {
                return Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId == Id)).Count();
            }
        }

        [BsonIgnore]
        public long Losses
        {
            get
            {
                return Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId != Id && (x.BlueTeamId == Id || x.PurpleTeamId == Id))).Count();
            }
        }

        [BsonIgnore]
        public TimeSpan TotalPlayTime
        {
            get
            {
                return
                    TimeSpan.FromSeconds(Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && (x.BlueTeamId == Id || x.PurpleTeamId == Id)))
                        .Sum(x => x.Duration.TotalSeconds));
            }
        }

        [BsonIgnore]
        public TimeSpan TotalWinsPlayTime
        {
            get
            {
                return
                    TimeSpan.FromSeconds(Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId == Id && (x.BlueTeamId == Id || x.PurpleTeamId == Id)))
                        .Sum(x => x.Duration.TotalSeconds));
            }
        }

        [BsonIgnore]
        public int PoolRank
        {
            get
            {
                return Mongo.Teams.Find(Query<Team>.Where(x => x.Pool == Pool)).OrderByDescending(x => x.Wins).ThenBy(x => x.TotalWinsPlayTime).ToList().FindIndex(x => x.Id == Id);
            }
        }

        public Match GetNextMatch()
        {
            if (Phase == Phase.Pool)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Pool &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id)))
                        .OrderBy(x => x.Priority)
                        .FirstOrDefault();
            }
            if (Phase == Phase.Finale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Finale &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id)))
                        .OrderBy(x => x.Priority)
                        .FirstOrDefault();
            }
            if (Phase == Phase.BronzeFinale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.BronzeFinale &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.WinnerBracket)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.WinnerBracket &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.LoserBracket)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserBracket &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.LoserFinale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserFinale &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }

            return null;
        }
    }
}