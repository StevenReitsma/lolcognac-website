using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoLTournament.Models
{
    public class Champion
    {
        public ObjectId Id { get; set; }
        public int ChampionId { get; set; }
        public string Name { get; set; }
    }
}