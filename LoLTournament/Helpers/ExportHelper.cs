using System.IO;
using System.Text;
using LoLTournament.Models;
using MongoDB.Driver;
using System.Linq;

namespace LoLTournament.Helpers
{
    public class ExportHelper
    {
        public static void ExportBadgeList()
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Participant>("Participants");
            var teamCol = db.GetCollection<Team>("Teams");

            var csv = new StringBuilder();
            csv.AppendLine("SummonerName, Season5Tier, Season5Division, TeamName");

            var validTeams = teamCol.FindAll()
                .OrderByDescending(x => x.AmountOfRuStudents)
                .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                .Take(32)
                .OrderBy(x => x.Name);

            foreach (var p in col.FindAll().OrderBy(x => x.Summoner.Name))
            {
                if (validTeams.Any(x => x.Id == p.TeamId))
                {
                    var str = string.Format("{0},{1},{2},{3}", p.Summoner.Name, p.Season5Tier, p.Season5Division,
                        p.Team.Name);
                    csv.AppendLine(str);
                }
            }

            File.WriteAllText("C:\\Users\\Steven\\Desktop\\badge_list.csv", csv.ToString(), Encoding.UTF8);
        }
    }
}