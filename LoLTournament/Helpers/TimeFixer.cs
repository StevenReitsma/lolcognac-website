using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using LoLTournament.Models;
using MongoDB.Driver;
using RiotSharp;
using RiotSharp.GameEndpoint;
using RiotSharp.MatchEndpoint;

namespace LoLTournament.Helpers
{
    public class TimeFixer
    {
        public static void FixMatchCreationDates()
        {
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            var api = RiotApi.GetInstance(key, 3000, 180000);

            foreach (var m in Mongo.Matches.FindAll())
            {
                MatchDetail game;
                try
                {
                    game = api.GetMatch(Region.euw, m.RiotMatchId);
                }
                catch (Exception)
                {
                    File.AppendAllText("C:\\Users\\Steven\\Desktop\\test.txt", m.BlueTeam.Name + " vs. " + m.PurpleTeam.Name + " (" + m.Phase + "." + m.Priority + ")\n");
                    continue;
                }

                m.AssistsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Assists);
                m.DeathsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Deaths);
                m.KillsBlueTeam = game.Participants.Where(x => x.TeamId == 100).Sum(x => x.Stats.Kills);

                m.AssistsPurpleTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Assists);
                m.DeathsPurpleTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Deaths);
                m.KillsPurpleTeam = game.Participants.Where(x => x.TeamId == 200).Sum(x => x.Stats.Kills);

                m.CreationTime = game.MatchCreation + new TimeSpan(0, 2, 10, 0) + game.MatchDuration;
                Mongo.Matches.Save(m);
            }
        }
    }
}