using System;
using System.Collections.Generic;
using LoLTournament.Helpers;
using System.Linq;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class ChampionsBannedViewModel
    {
        public Dictionary<string, int> ChampionsBanned { get; set; }

        public ChampionsBannedViewModel()
        {

            var matches = Mongo.Matches.FindAll();

            // Flatten all champion IDs
            var bannedChampions = matches.Where(match => match.BanIds != null).SelectMany(match => match.BanIds);

            var counts = from id in bannedChampions
                group id by id
                into g
                where g.Any()
                orderby g.Count() descending
                select new {g.Key, Count = g.Count()};

            ChampionsBanned = counts.Take(5).ToDictionary(c => Mongo.Champions.Find(Query<Champion>.Where(champion => champion.ChampionId == c.Key)).First().Name, c => c.Count);
        } 
    }
}