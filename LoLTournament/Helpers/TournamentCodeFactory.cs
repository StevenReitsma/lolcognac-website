using System;
using System.Collections.Generic;
using System.Web.Configuration;
using RiotSharp;
using RiotSharp.TournamentEndpoint;
using RiotSharp.TournamentEndpoint.Enums;

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
            var tc = TournamentApi.CreateTournamentCode(TournamentId, 5, allowedSummoners, TournamentSpectatorType.All,
                TournamentPickType.TournamentDraft, TournamentMapType.SummonersRift, string.Empty);

            return tc;
        }

        public static string GetTournamentCodeBlind(List<long> allowedSummoners)
        {
            var tc = TournamentApi.CreateTournamentCode(TournamentId, 5, allowedSummoners, TournamentSpectatorType.All,
                TournamentPickType.BlindPick, TournamentMapType.SummonersRift, string.Empty);

            return tc;
        }

        public static Tuple<TournamentProvider, Tournament> GenerateTournament()
        {
            var provider = TournamentApi.CreateProvider(Region.euw, "http://lolcognac.nl/Match/Callback");
            var tournament = TournamentApi.CreateTournament(provider.Id, "LCN 2017");

            return new Tuple<TournamentProvider, Tournament>(provider, tournament);
        }

        public static bool UpdateTournamentCodePlayers(string code, List<long> allowedsummoners)
        {
            return TournamentApi.UpdateTournamentCode(code, allowedsummoners, TournamentSpectatorType.All,
                TournamentPickType.TournamentDraft, TournamentMapType.SummonersRift);
        }

        public static bool UpdateTournamentCodePlayersBlind(string code, List<long> allowedsummoners)
        {
            return TournamentApi.UpdateTournamentCode(code, allowedsummoners, TournamentSpectatorType.All,
                TournamentPickType.BlindPick, TournamentMapType.SummonersRift);
        }
    }
}
