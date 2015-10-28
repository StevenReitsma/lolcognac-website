using System;
using System.Collections.Generic;
using LoLTournament.Helpers;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Globalization;

namespace LoLTournament.Models
{
    public class KillsOverTimeViewModel
    {
        public Dictionary<string, long> Kills { get; set; }

        public KillsOverTimeViewModel()
        {
            Kills = new Dictionary<string, long>();
            var matches = Mongo.Matches.FindAll().OrderBy(match => match.CreationTime);
            
            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];
            DateTime tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime tournamentEnd = matches.Last().FinishDate;

            for (DateTime currentTime = tournamentStart; currentTime <= tournamentEnd; currentTime += new TimeSpan(0, 5, 0))
            {
                long killsSoFar = matches.Where(match => match.FinishDate < currentTime).Sum(match => match.KillsBlueTeam + match.KillsPurpleTeam);
                Kills.Add(currentTime.ToString("HH:mm"), killsSoFar);
            }
        }

    }
}