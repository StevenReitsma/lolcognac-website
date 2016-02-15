using System;
using System.Collections.Generic;
using System.Linq;
using LoLTournament.Helpers;
using RiotSharp.LeagueEndpoint;

namespace LoLTournament.Models
{
    public class LeagueDataViewModel
    {
        public Dictionary<string, long> LeagueData5 { get; set; }
        public Dictionary<string, long> LeagueData6 { get; set; }

        public LeagueDataViewModel()
        {
            LeagueData5 = new Dictionary<string, long>();
            LeagueData6 = new Dictionary<string, long>();

            var enumValues = new List<Tier> {Tier.Unranked, Tier.Bronze, Tier.Silver, Tier.Gold, Tier.Platinum, Tier.Diamond, Tier.Master, Tier.Challenger };

            foreach (Tier t in enumValues)
            {
                // Gotta catch 'em all because of easy Cancelled checking
                var participants = Mongo.Participants.FindAll();
                var count5 =
                    participants.Count(
                        x =>
                            x.PreviousSeasonTier == t && !x.Cancelled);

                var count6 =
                    participants.Count(
                        x =>
                            x.CurrentSeasonTier == t && !x.Cancelled);

                LeagueData5[t.ToString()] = count5;
                LeagueData6[t.ToString()] = count6;
            }
        }
    }
}
