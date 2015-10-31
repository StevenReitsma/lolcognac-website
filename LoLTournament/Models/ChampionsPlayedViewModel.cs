using System;
using System.Collections.Generic;
using LoLTournament.Helpers;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using RiotSharp;
using MongoDB.Driver.Builders;

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

            ChampionsPlayed = championsIdPlayed
                .Select((s, i) => new { Id = i, Count = s })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToDictionary(c => Mongo.Champions.Find(Query<Champion>.Where(champion => champion.ChampionId == c.Id)).First().Name, c => c.Count);   
        }
    }
}