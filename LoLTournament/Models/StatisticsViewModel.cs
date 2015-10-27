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
            var matches = Mongo.Matches.FindAll();

            if(matches.Count() > 0)
            {
                AvgKills = matches.Sum(x => x.KillsBlueTeam + x.KillsPurpleTeam) / TotalGames;
                AvgDeaths = matches.Sum(x => x.DeathsBlueTeam + x.DeathsPurpleTeam) / TotalGames;
                AvgAssists = matches.Sum(x => x.AssistsBlueTeam + x.AssistsPurpleTeam) / TotalGames;
                AvgMatchDuration = TimeSpan.FromSeconds(matches.Average(x => x.Duration.TotalSeconds));
            }
            else
            {
                AvgKills = 0;
                AvgDeaths = 0;
                AvgAssists = 0;
                AvgMatchDuration = TimeSpan.Zero;
            }
            
        }
    }
}
