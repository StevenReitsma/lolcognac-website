using System;
using System.Collections.Generic;
using System.Linq;
using RiotSharp;
using RiotSharp.LeagueEndpoint;
using RiotSharp.SummonerEndpoint;

namespace LoLTournament.Models
{
    public class SummonerRegisterInfoModel
    {
        public SummonerRegisterInfoModel(Summoner s)
        {
            Summoner = s;
        }

        public SummonerRegisterInfoModel(bool riotApiOffline)
        {
            if (!riotApiOffline)
                throw new Exception("Method can only be called with riotApiOffline = true, otherwise, give a summoner.");

            RiotApiOffline = true;
        }

        public Summoner Summoner { get; set; }
        public bool RiotApiOffline { get; set; }

        public string SoloLeague
        {
            get
            {
                if (RiotApiOffline)
                    return null;

                List<League> leagues;
                try
                {
                    leagues = Summoner.GetLeagues();
                }
                catch (Exception)
                {
                    // Unranked
                    return "Unranked";
                }

                if (leagues == null || leagues.Count == 0)
                    return "Unranked";

                var solo = leagues.SingleOrDefault(x => x.Queue == Queue.RankedSolo5x5);

                if (solo == null)
                    return "Unranked";

                var tier = solo.Tier;
                var division = solo.Entries[0].Division;

                return tier + " " + division;
            }
        }
    }
}