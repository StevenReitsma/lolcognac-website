using System.Linq;
using LoLTournament.Helpers;

namespace LoLTournament.Models.Admin
{
    public class AdminParticipantsViewModel
    {
        public IOrderedEnumerable<Participant> Participants
        {
            get
            {
                return Mongo.Participants.FindAll().Where(x => !x.Team.Cancelled && x.Summoner != null).OrderBy(x => x.RegisterTime);
            }
        }
    }
}
