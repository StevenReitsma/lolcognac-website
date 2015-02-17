using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class Team
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public List<ObjectId> ParticipantsIds { get; set; }
        public int Pool { get; set; }
        public Phase Phase { get; set; }

        [BsonIgnore]
        public List<Participant> Participants
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Participant>("Participants");

                return col.Find(Query<Participant>.Where(x => x.TeamId == Id)).ToList();
            }
        }

        public ObjectId CaptainId { get; set; }
        
        [BsonIgnore]
        public Participant Captain {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Participant>("Participants");

                return col.Find(Query<Participant>.Where(x => x.TeamId == Id && x.IsCaptain)).First();
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
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Match>("Matches");

                return col.Find(Query<Match>.Where(x => x.Finished && x.WinnerId == Id)).Count();
            }
        }

        [BsonIgnore]
        public long Losses
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Match>("Matches");

                return col.Find(Query<Match>.Where(x => x.Finished && x.WinnerId != Id && (x.BlueTeamId == Id || x.PurpleTeamId == Id))).Count();
            }
        }

        public Match GetNextMatch()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var matchCol = db.GetCollection<Match>("Matches");

            if (Phase == Phase.Pool)
            {
                return
                    matchCol.Find(
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
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Finale &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id)))
                        .OrderBy(x => x.Priority)
                        .FirstOrDefault();
            }
            if (Phase == Phase.WinnerBracket)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.WinnerBracket &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.LoserBracket)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserBracket &&
                                (x.BlueTeamId == Id || x.PurpleTeamId == Id))).FirstOrDefault();
            }

            return null;
        }
    }
}