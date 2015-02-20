using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class Match
    {
        public ObjectId Id { get; set; }
        public long RiotMatchId { get; set; }

        public ObjectId BlueTeamId { get; set; }
        public ObjectId PurpleTeamId { get; set; }
        public Phase Phase { get; set; }
        public int Priority { get; set; } // used to indicate order for Pool and Finale (best out of three) phase

        public bool Finished { get; set; }

        public ObjectId WinnerId { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime FinishTime { get; set; }

        public long KillsBlueTeam { get; set; }
        public long DeathsBlueTeam { get; set; }
        public long AssistsBlueTeam { get; set; }

        public long KillsPurpleTeam { get; set; }
        public long DeathsPurpleTeam { get; set; }
        public long AssistsPurpleTeam { get; set; }

        public bool BlueStatsFilled { get; set; }
        public bool PurpleStatsFilled { get; set; }

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

        [BsonIgnore]
        public Team Winner
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");

                return col.Find(Query<Team>.Where(x => x.Id == WinnerId)).FirstOrDefault();
            }
        }

        [BsonIgnore]
        public string TournamentCode
        {
            get
            {
                return
                    "pvpnet://lol/customgame/joinorcreate/map11/pick6/team5/specALL/" + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"name\": \"" + @BlueTeam.Name.Substring(0, 10) + "..(B)+" + @PurpleTeam.Name.Substring(0, 10) + "..(P)" + "\",\"extra\":\"\",\"password\":\"CognAC" + Id + "\",\"report\":\"\"}"));
            }
        }

        [BsonIgnore]
        public string TournamentCodeBlind
        {
            get
            {
                return
                    "pvpnet://lol/customgame/joinorcreate/map11/pick1/team5/specALL/" + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"name\": \"" + @BlueTeam.Name.Substring(0, 10) + "..(B) vs " + @PurpleTeam.Name.Substring(0, 10) + "..(P) [BP]" + "\",\"extra\":\"\",\"password\":\"CognAC" + Id + "\",\"report\":\"\"}"));
            }
        }
    }
}