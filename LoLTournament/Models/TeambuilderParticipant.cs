using System;
using MongoDB.Bson;
using RiotSharp.SummonerEndpoint;

namespace LoLTournament.Models
{
    public class TeambuilderParticipant
    {
        public ObjectId Id { get; set; }
        public Summoner Summoner { get; set; }
        public string SummonerName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public ObjectId TeamId { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string StudyProgram { get; set; }
        public bool IsCaptain { get; set; }
        public string Roles { get; set; }
    }
}