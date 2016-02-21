using System.Collections.Generic;
using System.Web.Configuration;
using RiotSharp;
using RiotSharp.TournamentEndpoint;

namespace LoLTournament.Helpers
{
    public static class TournamentCodeFactory
    {
        private static readonly TournamentRiotApi TournamentApi;
        private static readonly int TournamentId;

        static TournamentCodeFactory()
        {
            TournamentId = int.Parse(WebConfigurationManager.AppSettings["TournamentId"]);

            var tournamentKey = WebConfigurationManager.AppSettings["RiotTournamentApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            TournamentApi = TournamentRiotApi.GetInstance(tournamentKey, rateLimit1, rateLimit2);
        }

        public static string GetTournamentCode(List<long> allowedSummoners)
        {
#if DEBUG
            return string.Empty;
#endif

            var tc = TournamentApi.CreateTournamentCode(TournamentId, 5, allowedSummoners, TournamentSpectatorType.All,
                TournamentPickType.TournamentDraft, TournamentMapType.SummonersRift, string.Empty);

            return tc;
        }

        public static string GetTournamentCodeBlind(List<long> allowedSummoners)
        {
#if DEBUG
            return string.Empty;
#endif

            var tc = TournamentApi.CreateTournamentCode(TournamentId, 5, allowedSummoners, TournamentSpectatorType.All,
                TournamentPickType.BlindPick, TournamentMapType.SummonersRift, string.Empty);

            return tc;
        }

        public static Tournament GenerateTournament()
        {
            var provider = TournamentApi.CreateProvider(Region.euw, "https://lolcognac.nl/Match/Callback");
            var tournament = TournamentApi.CreateTournament(provider.Id, "CognAC League of Legends Tournament 2016");

            return tournament;
        }
    }
}
