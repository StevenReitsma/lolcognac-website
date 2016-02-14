using System.Collections.Generic;
using System.Web.Configuration;
using RiotSharp;
using RiotSharp.TournamentEndpoint;

namespace LoLTournament.Helpers
{
    public static class TournamentCodeFactory
    {
        private static readonly TournamentRiotApi _tournamentApi;

        static TournamentCodeFactory()
        {
            var tournamentKey = WebConfigurationManager.AppSettings["RiotTournamentApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            _tournamentApi = TournamentRiotApi.GetInstance(tournamentKey, rateLimit1, rateLimit2);
        }

        public static string GetTournamentCode(int tournamentId)
        {
            var tc = _tournamentApi.CreateTournamentCode(tournamentId, 5, new List<long> { 24689119, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, TournamentSpectatorType.All,
                TournamentPickType.TournamentDraft, TournamentMapType.SummonersRift, string.Empty);

            return tc;
        }

        public static string GetTournamentCodeBlind()
        {
            return string.Empty;
        }

        public static Tournament GenerateTournament()
        {
            var provider = _tournamentApi.CreateProvider(Region.euw, "http://home.properchaos.nl/Match/Callback");
            var tournament = _tournamentApi.CreateTournament(provider.Id, "CognAC Test Tournament (DEV)");

            return tournament;
        }
    }
}
