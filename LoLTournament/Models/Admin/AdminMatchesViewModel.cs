using System.Linq;
using LoLTournament.Helpers;

namespace LoLTournament.Models.Admin
{
    public class AdminMatchesViewModel
    {
        public IOrderedEnumerable<Match> LiveMatches
        {
            get { return Mongo.Matches.FindAll().Where(x => !x.Finished && x.SpectateKey != null).OrderByDescending(x => x.Duration); }
        }

        public IOrderedEnumerable<Match> UnfinishedMatches
        {
            get { return Mongo.Matches.FindAll().Where(x => !x.Finished && x.SpectateKey == null).OrderBy(x => x.Phase).ThenBy(x => x.Priority); }
        }

        public IOrderedEnumerable<Match> FinishedMatches
        {
            get { return Mongo.Matches.FindAll().Where(x => x.Finished).OrderBy(x => x.FinishDate); }
        }
    }
}
