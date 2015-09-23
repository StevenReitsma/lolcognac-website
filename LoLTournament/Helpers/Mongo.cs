using LoLTournament.Models;
using MongoDB.Driver;

namespace LoLTournament.Helpers
{
    public static class Mongo
    {
        public static MongoCollection<Team> Teams;
        public static MongoCollection<Match> Matches;
        public static MongoCollection<Participant> Participants;
        public static MongoCollection<TeambuilderParticipant> TeamBuilderParticipants;

        static Mongo()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");

            Teams = db.GetCollection<Team>("Teams");
            Matches = db.GetCollection<Match>("Matches");
            Participants = db.GetCollection<Participant>("Participants");
            TeamBuilderParticipants = db.GetCollection<TeambuilderParticipant>("TeamBuilderParticipants");
        }
    }
}
