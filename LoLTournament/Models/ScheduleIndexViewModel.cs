using System.Linq;
using MongoDB.Driver;
using LoLTournament.Helpers;
using System.Web.Configuration;
using System;
using System.Globalization;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class ScheduleIndexViewModel
    {
        public IOrderedEnumerable<Team> Teams
        {
            get
            {
                return Mongo.Teams.Find(Query<Team>.Where(x => !x.Cancelled)).OrderBy(x => x.Participants.Sum(y => y.RegisterTime.Ticks));
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

        public DateTime RegistrationStartDate
        {
            get
            {
                var timeSetting = WebConfigurationManager.AppSettings["RegistrationStart"];
                var registrationStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                return registrationStart;
            }
        }

        public DateTime RegistrationStartEarlyBird
        {
            get
            {
                var timeSetting = WebConfigurationManager.AppSettings["RegistrationStartEarlyBird"];
                var registrationStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                return registrationStart;
            }
        }
    }
}
