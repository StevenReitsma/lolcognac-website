using RiotSharp.LeagueEndpoint.Enums;

namespace LoLTournament.Helpers
{
    public static class LeagueMath
    {
        public static int GetLeagueMultiplier(Tier t)
        {
            switch (t)
            {
                case Tier.Unranked:
                    return 7;
                case Tier.Bronze:
                    return 6;
                case Tier.Silver:
                    return 5;
                case Tier.Gold:
                    return 4;
                case Tier.Platinum:
                    return 3;
                case Tier.Diamond:
                    return 2;
                case Tier.Master:
                    return 1;
                case Tier.Challenger:
                    return 0;
                default:
                    return 7;
            }
        }
    }
}
