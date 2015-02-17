using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class Match
    {
        public ObjectId Id { get; set; }

        public ObjectId BlueTeamId { get; set; }
        public ObjectId PurpleTeamId { get; set; }
        public Phase Phase { get; set; }
        public int Priority { get; set; } // used to indicate order for Pool and Finale (best out of three) phase

        public bool Finished { get; set; }

        public ObjectId WinnerId { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime FinishTime { get; set; }

        public int KillsWinningTeam { get; set; }
        public int DeathsWinningTeam { get; set; }
        public int AssistsWinningTeam { get; set; }

        public int KillsLosingTeam { get; set; }
        public int DeathsLosingTeam { get; set; }
        public int AssistsLosingTeam { get; set; }

        [BsonIgnore]
        public Team BlueTeam {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");

                return col.Find(Query<Team>.Where(x => x.Id == BlueTeamId)).FirstOrDefault();
            }
        }

        [BsonIgnore]
        public Team PurpleTeam
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");

                return col.Find(Query<Team>.Where(x => x.Id == PurpleTeamId)).FirstOrDefault();
            }
        }
    }
}