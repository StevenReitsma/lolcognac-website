using LoLTournament.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class StudyProgrammeViewModel
    {

        public Dictionary<string, int> Programmes { get; set; }

        public StudyProgrammeViewModel()
        {
            var participants = Mongo.Participants.Find(Query<Participant>.Where(x => x.StudyProgram != null));
            Programmes = participants.GroupBy(p => p.StudyProgram)
                .OrderByDescending(group => group.Count())
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}