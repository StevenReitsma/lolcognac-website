using System.Linq;
using MongoDB.Driver;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class TimetableIndexViewModel
    {
        public IOrderedEnumerable<Team> Teams
        {
            get
            {
                return Mongo.Teams.FindAll().OrderByDescending(x => x.AmountOfRuStudents).ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks));
            }
        }

        public MongoCursor<TeambuilderParticipant> TeambuilderParticipants
        {
            get
            {
                return Mongo.TeamBuilderParticipants.FindAll();
            }
        }
    }
}
