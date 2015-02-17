using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LoLTournament.Models
{
    public enum Phase
    {
        Pool,
        WinnerBracket,
        LoserBracket,
        Finale
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
                    return "Winner bracket";
                case Phase.LoserBracket:
                    return "Loser bracket";
                case Phase.Finale:
                    return "Finale";
            }

            return "";
        }
    }
}
