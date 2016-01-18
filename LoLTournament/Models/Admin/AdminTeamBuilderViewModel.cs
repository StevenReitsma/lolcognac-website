using System.Linq;
using LoLTournament.Helpers;

namespace LoLTournament.Models.Admin
{
    public class AdminTeamBuilderViewModel
    {
        public IOrderedEnumerable<TeambuilderParticipant> TeamBuilders
        {
            get { return Mongo.TeamBuilderParticipants.FindAll().OrderBy(x => x.RegisterTime); }
        }
    }
}
