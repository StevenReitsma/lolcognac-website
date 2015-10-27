using LoLTournament.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoLTournament.Models
{
    public class StudyProgrammeViewModel
    {

        public Dictionary<string, int> Programmes { get; set; }

        public StudyProgrammeViewModel()
        {
            var participants = Mongo.Participants.FindAll();
            Programmes = participants.GroupBy(p => p.StudyProgram)
                .OrderByDescending(group => group.Count())
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}