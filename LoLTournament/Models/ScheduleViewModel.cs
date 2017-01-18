using System.Collections.Generic;
using System.Linq;
using LoLTournament.Helpers;
using LoLTournament.Models.Financial;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class ScheduleViewModel
    {
        public List<Team> Teams { get; set; }

        public ScheduleViewModel()
        {
            Teams = Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled && !x.Name.Contains("TESTCOGNAC")))
                   .OrderBy(x => x.Pool)
                   .ThenBy(x => x.Name)
                   .ToList();

            if (Teams.Count != 32 || Teams.Any(t => t.Pool == 0))
                Teams = new List<Team>();
        }
    }
}
