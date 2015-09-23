using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class ScheduleViewModel
    {
        public List<Team> Teams { get; set; }

        public ScheduleViewModel()
        {
            Teams = Mongo.Teams.FindAll()
                   .OrderByDescending(x => x.AmountOfRuStudents)
                   .ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks))
                   .Take(32)
                   .OrderBy(x => x.Pool)
                   .ThenBy(x => x.Name)
                   .ToList();
        }
    }
}
