using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class StatisticsViewModel
    {

        public long AvgKills { get; set; }
        public long AvgDeaths { get; set; }
        public long AvgAssists { get; set; }
        public long TotalGames { get; set; }

        public StatisticsViewModel()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var matchCol = db.GetCollection<Match>("Matches");

            TotalGames = matchCol.Count(Query<Match>.Where(x => x.Finished));

            AvgKills = matchCol.FindAll().Sum(x => x.KillsBlueTeam + x.KillsPurpleTeam);

            if (TotalGames > 0)
                AvgKills /= TotalGames;
            else
                AvgKills = 0;

            AvgDeaths = matchCol.FindAll().Sum(x => x.DeathsBlueTeam + x.DeathsPurpleTeam);

            if (TotalGames > 0)
                AvgDeaths /= TotalGames;
            else
                AvgDeaths = 0;

            AvgAssists = matchCol.FindAll().Sum(x => x.AssistsBlueTeam + x.AssistsPurpleTeam);

            if (TotalGames > 0)
                AvgAssists /= TotalGames;
            else
                AvgAssists = 0;
        }
    }
}
