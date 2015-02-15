using System.Linq;
using MongoDB.Driver;

namespace LoLTournament.Models
{
    public class TimetableIndexViewModel
    {
        public IOrderedEnumerable<Team> Teams
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<Team>("Teams");

                return col.FindAll().OrderByDescending(x => x.AmountOfRuStudents).ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks));
            }
        }

        public MongoCursor<TeambuilderParticipant> TeambuilderParticipants
        {
            get
            {
                var client = new MongoClient();
                var server = client.GetServer();
                var db = server.GetDatabase("CLT");
                var col = db.GetCollection<TeambuilderParticipant>("TeamBuilderParticipants");

                return col.FindAll();
            }
        }
    }
}