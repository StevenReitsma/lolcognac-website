﻿using System.Linq;
using LoLTournament.Helpers;

namespace LoLTournament.Models.Admin
{
    public class AdminTeamsViewModel
    {
        public IOrderedEnumerable<Team> Teams
        {
            get
            {
                return Mongo.Teams.FindAll().OrderBy(x => x.Cancelled).ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks));
            }
        }
    }
}
