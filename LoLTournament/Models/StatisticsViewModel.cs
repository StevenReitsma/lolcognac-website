using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class StatisticsViewModel
    {

        public long AvgKills { get; set; }
        public long AvgDeaths { get; set; }
        public long AvgAssists { get; set; }
        public long TotalGames { get; set; }
        public TimeSpan AvgMatchDuration { get; set; }

        public StatisticsViewModel()
        {
            TotalGames = Mongo.Matches.Count(Query<Match>.Where(x => x.Finished));

            AvgKills = Mongo.Matches.FindAll().Sum(x => x.KillsBlueTeam + x.KillsPurpleTeam);

            if (TotalGames > 0)
                AvgKills /= TotalGames;
            else
                AvgKills = 0;

            AvgDeaths = Mongo.Matches.FindAll().Sum(x => x.DeathsBlueTeam + x.DeathsPurpleTeam);

            if (TotalGames > 0)
                AvgDeaths /= TotalGames;
            else
                AvgDeaths = 0;

            AvgAssists = Mongo.Matches.FindAll().Sum(x => x.AssistsBlueTeam + x.AssistsPurpleTeam);

            if (TotalGames > 0)
                AvgAssists /= TotalGames;
            else
                AvgAssists = 0;

            AvgMatchDuration = TimeSpan.FromSeconds(Mongo.Matches.FindAll().Average(x => x.Duration.TotalSeconds));
        }
    }
}
