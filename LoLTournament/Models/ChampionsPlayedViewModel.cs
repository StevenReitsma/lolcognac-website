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

            // Flatten all champion id's
            var playedChampions = matches.Where(x => x.ChampionIds != null).SelectMany(match => match.ChampionIds);

            // Create groups, count contents, check for 0-count, order by count and put in dictionary
            var counts = from id in playedChampions
                group id by id
                into g
                where g.Any()
                orderby g.Count() descending 
                select new {g.Key, Count = g.Count()};

            // Take top 5 and convert id -> name
            ChampionsPlayed = counts
                .Take(10)
                .ToDictionary(
                    c =>
                        Mongo.Champions.Find(Query<Champion>.Where(champion => champion.ChampionId == c.Key))
                            .First()
                            .Name, c => c.Count);
        }
    }
}
