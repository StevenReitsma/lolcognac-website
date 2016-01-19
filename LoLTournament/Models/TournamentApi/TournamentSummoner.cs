using Newtonsoft.Json;

namespace LoLTournament.Models.TournamentApi
{
    public class TournamentSummoner
    {
        [JsonProperty("summonerId")]
        public long SummonerId { get; set; }

        [JsonProperty("summonerName")]
        public string SummonerName { get; set; }
    }
}
