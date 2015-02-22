namespace LoLTournament.Models
{
    public enum Phase
    {
        Pool,
        WinnerBracket,
        LoserBracket,
        Finale,
        BronzeFinale
    }

    public static class EnumExtension
    {
        public static string GetDisplayString(this Phase phase)
        {
            switch (phase)
            {
                case Phase.Pool:
                    return "Pool";
                case Phase.WinnerBracket:
                    return "Elimination";
                case Phase.LoserBracket:
                    return "Losers' bracket";
                case Phase.Finale:
                    return "Finale";
                case Phase.BronzeFinale:
                    return "Bronze finale";
            }

            return "";
        }
    }
}
