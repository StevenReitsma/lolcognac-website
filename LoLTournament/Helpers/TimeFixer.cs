using System;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using RiotSharp;
using RiotSharp.MatchEndpoint;

namespace LoLTournament.Helpers
{
    public class TimeFixer
    {
        public static void FixMatchCreationDates()
        {
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            var api = RiotApi.GetInstance(key, rateLimit1, rateLimit2);

            foreach (var m in Mongo.Matches.FindAll())
            {
                MatchDetail game;
                try
                {
                    game = api.GetMatch(Region.euw, m.RiotMatchId);
                }
                catch (Exception)
                {
                    File.AppendAllText("C:\\Users\\Steven\\Desktop\\test.txt", m.BlueTeam.Name + " vs. " + m.RedTeam.Name + " (" + m.Phase + "." + m.Priority + ")\n");
                    continue;
                }

                m.AssistsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Assists);
                m.DeathsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Deaths);
                m.KillsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Kills);

                m.AssistsRedTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Assists);
                m.DeathsRedTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Deaths);
                m.KillsRedTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Kills);

                m.ChampionIds = game.Participants.Select(x => x.ChampionId).ToArray();
                m.BanIds = game.Teams.Where(x => x.Bans != null).SelectMany(x => x.Bans).Select(x => x.ChampionId).ToArray();

                m.Duration = game.MatchDuration;

                Mongo.Matches.Save(m);
            }
        }
    }
}