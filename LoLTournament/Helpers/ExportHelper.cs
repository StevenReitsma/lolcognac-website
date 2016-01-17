using System;
using System.IO;
using System.Text;
using System.Web;
using LoLTournament.Models;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Driver.Builders;

namespace LoLTournament.Helpers
{
    public class ExportHelper
    {
        /// <summary>
        /// Exports a list that can be used by Joris' automatic badge creator.
        /// </summary>
        public static void ExportBadgeList()
        {
            var csv = new StringBuilder();
            csv.AppendLine("SummonerName, CurrentSeasonTier, CurrentSeasonDivision, TeamName");

            var validTeams = Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled))
                .OrderByDescending(x => x.AmountOfRuStudents)
                .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                .Take(32)
                .OrderBy(x => x.Name);

            foreach (var p in Mongo.Participants.FindAll().OrderBy(x => x.Summoner.Name))
            {
                if (validTeams.Any(x => x.Id == p.TeamId))
                {
                    var str = string.Format("{0},{1},{2},{3}", p.Summoner.Name, p.CurrentSeasonTier, p.CurrentSeasonDivision,
                        p.Team.Name);
                    csv.AppendLine(str);
                }
            }

            File.WriteAllText(HttpRuntime.AppDomainAppPath + "/badge_list.csv", csv.ToString(), new UTF8Encoding(false)); // no BOM
        }

        /// <summary>
        /// Exports a CSV file that can be used at the event checkin.
        /// </summary>
        public static void ExportEntryList()
        {
            var csv = new StringBuilder();
            csv.AppendLine("SummonerName, FullName, TeamName, StudyProgram, Captain, RU");

            var validTeams = Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled))
                .OrderByDescending(x => x.AmountOfRuStudents)
                .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                .Take(32)
                .OrderBy(x => x.Name);

            foreach (var p in Mongo.Participants.FindAll().OrderBy(x => x.Team.Name))
            {
                if (validTeams.Any(x => x.Id == p.TeamId))
                {
                    var str = string.Format("{0},{1},{2},{3},{4},{5}", p.Summoner.Name, p.FullName, p.Team.Name, p.StudyProgram, p.IsCaptain, p.RuStudent);
                    csv.AppendLine(str);
                }
            }

            File.WriteAllText(HttpRuntime.AppDomainAppPath + "/checkin_list.csv", csv.ToString(), new UTF8Encoding(false)); // no BOM
        }
    }
}