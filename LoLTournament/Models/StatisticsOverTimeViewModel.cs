using System;
using System.Collections.Generic;
using LoLTournament.Helpers;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Globalization;

namespace LoLTournament.Models
{
    public class StatisticsOverTimeViewModel
    {
        public Dictionary<string, long> Kills { get; set; }
        public Dictionary<string, long> Assists { get; set; }

        public StatisticsOverTimeViewModel()
        {
            Kills = new Dictionary<string, long>();
            Assists = new Dictionary<string, long>();
            var matches = Mongo.Matches.FindAll().OrderBy(match => match.CreationTime);
            
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            DateTime tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            var lastMatch = matches.LastOrDefault();

            if (lastMatch == null)
                return;

            DateTime tournamentEnd = lastMatch.FinishDate;

            for (DateTime currentTime = tournamentStart; currentTime <= tournamentEnd; currentTime += new TimeSpan(0, 5, 0))
            {
                var matchesSoFar = matches.Where(match => match.FinishDate < currentTime);
                long killsSoFar = matchesSoFar.Sum(match => match.KillsBlueTeam + match.KillsRedTeam);
                long assistsSoFar = matchesSoFar.Sum(match => match.AssistsBlueTeam + match.AssistsRedTeam);
                string time = currentTime.ToString("HH:mm");
                Kills.Add(time, killsSoFar);
                Assists.Add(time, assistsSoFar);
            }
        }

    }
}