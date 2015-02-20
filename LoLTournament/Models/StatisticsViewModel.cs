using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class StatisticsViewModel
    {

        public List<Team> Teams { get; set; }
        public long TotalKills { get; set; }
        public long TotalDeaths { get; set; }
        public long TotalAssists { get; set; }
        public long TotalGames { get; set; }

        public StatisticsViewModel()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Team>("Teams");
            var matchCol = db.GetCollection<Match>("Matches");

            Teams = col.FindAll()
                    .OrderByDescending(x => x.AmountOfRuStudents)
                    .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                    .Take(32)
                    .OrderBy(x => x.Name)
                    .ToList();

            TotalKills = matchCol.FindAll().Sum(x => x.KillsBlueTeam + x.KillsPurpleTeam);
            TotalDeaths = matchCol.FindAll().Sum(x => x.DeathsBlueTeam + x.DeathsPurpleTeam);
            TotalAssists = matchCol.FindAll().Sum(x => x.AssistsBlueTeam + x.AssistsPurpleTeam);
            TotalGames = matchCol.Count(Query<Match>.Where(x => x.Finished));
        }
    }
}