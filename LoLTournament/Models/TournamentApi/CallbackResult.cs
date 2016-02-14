using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RiotSharp;

namespace LoLTournament.Models.TournamentApi
{
    public class CallbackResult
    {
        [JsonProperty("startTime")]
        [JsonConverter(typeof(DateTimeConverterFromLong))]
        public DateTime StartTime { get; set; }

        [JsonProperty("winningTeam")]
        public List<TournamentSummoner> WinningTeam;

        [JsonProperty("losingTeam")]
        public List<TournamentSummoner> LosingTeam;

        [JsonProperty("shortCode")]
        public string TournamentCode;

        [JsonProperty("metaData")]
        public string Metadata;

        [JsonProperty("gameId")]
        public long GameId;

        [JsonProperty("gameName")]
        public string GameName;

        [JsonProperty("gameType")]
        public string GameType;

        [JsonProperty("gameMap")]
        public MapType GameMap;

        [JsonProperty("gameMode")]
        public GameMode GameMode;

        [JsonProperty("region")]
        public Platform Region;
    }
}
