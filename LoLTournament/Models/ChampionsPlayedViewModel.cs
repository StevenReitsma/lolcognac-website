using System;
using System.Collections.Generic;
using LoLTournament.Helpers;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using RiotSharp;

namespace LoLTournament.Models
{
    public class ChampionsPlayedViewModel
    {
        public Dictionary<string, int> ChampionsPlayed { get; set; }

        public ChampionsPlayedViewModel()
        {
            var matches = Mongo.Matches.FindAll();

            int[] championsIdPlayed = new int[1000];

            foreach(var match in matches) 
            {
                foreach(var id in match.ChampionIds) 
                {
                    championsIdPlayed[id]++;
                }
            }

            // Maybe the static api can be scraped once and relevant information can be saved to the database
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            var api = StaticRiotApi.GetInstance(key);

            ChampionsPlayed = championsIdPlayed
                .Select((s, i) => new { Id = i, Count = s })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToDictionary(c => api.GetChampion(Region.euw, c.Id).Name, c => c.Count);   
        }
    }
}