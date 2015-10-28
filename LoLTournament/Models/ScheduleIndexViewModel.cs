using System.Linq;
using MongoDB.Driver;
using LoLTournament.Helpers;
using System.Web.Configuration;
using System;
using System.Globalization;

namespace LoLTournament.Models
{
    public class ScheduleIndexViewModel
    {
        public IOrderedEnumerable<Team> Teams
        {
            get
            {
                return Mongo.Teams.FindAll().OrderByDescending(x => x.AmountOfRuStudents).ThenBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks));
            }
        }

        public MongoCursor<TeambuilderParticipant> TeambuilderParticipants
        {
            get
            {
                return Mongo.TeamBuilderParticipants.FindAll();
            }
        }

        public DateTime RegistrationCloseDate
        {
            get
            {
                var timeSetting = WebConfigurationManager.AppSettings["RegistrationClose"];
                var registrationClose = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                return registrationClose;
            }
        }
    }
}
