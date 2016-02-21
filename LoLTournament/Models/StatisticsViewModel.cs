using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class StatisticsViewModel
    {

        public double AvgKills { get; set; }
        public double AvgDeaths { get; set; }
        public double AvgAssists { get; set; }
        public double TotalGames { get; set; }
        public double BlueSideWinPercentage { get; set; }
        public TimeSpan AvgMatchDuration { get; set; }

        public StatisticsViewModel()
        {
            TotalGames = Mongo.Matches.Count(Query<Match>.Where(x => x.Finished));
            var matches = Mongo.Matches.FindAll();

            if(matches.Count(x => x.Finished) > 0)
            {
                AvgKills = matches.Sum(x => x.KillsBlueTeam + x.KillsRedTeam) / TotalGames;
                AvgDeaths = matches.Sum(x => x.DeathsBlueTeam + x.DeathsRedTeam) / TotalGames;
                AvgAssists = matches.Sum(x => x.AssistsBlueTeam + x.AssistsRedTeam) / TotalGames;
                AvgMatchDuration = TimeSpan.FromSeconds(matches.Average(x => x.Duration.TotalSeconds));
                BlueSideWinPercentage = matches.Where(match => match.Finished).Count(match => match.BlueTeamId == match.WinnerId) / TotalGames;
            }
            else
            {
                AvgKills = 0;
                AvgDeaths = 0;
                AvgAssists = 0;
                BlueSideWinPercentage = 0;
                AvgMatchDuration = TimeSpan.Zero;
            }
            
        }
    }
}
