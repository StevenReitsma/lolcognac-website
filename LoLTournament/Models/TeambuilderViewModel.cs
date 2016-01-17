using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Configuration;

namespace LoLTournament.Models
{
    public class TeambuilderViewModel
    {
        [DisplayName("Summoner name*")]
        [Required(ErrorMessage = "Required.")]
        public string Name { get; set; }
        [DisplayName("Real name*")]
        [Required(ErrorMessage = "Required.")]
        public string RealName { get; set; }
        [DisplayName("Email address*")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Email { get; set; }
        [DisplayName("Study program (e.g. B Artificial Intelligence, M English, B Computer Science, etc.)")]
        public string Study { get; set; }
        [DisplayName("Student number (only if student of RU)")]
        public string StudentNumber { get; set; }

        [DisplayName("You are a RU student (will be verified)")]
        public bool RUStudent { get; set; }
        [DisplayName("You are a member or benefactor of CognAC (will be verified)")]
        public bool CognAC { get; set; }
        [DisplayName("You are a member or benefactor of Dorans (will be verified)")]
        public bool Dorans { get; set; }

        [DisplayName("Preferred roles (i.e. AP, AD, support, top, jungle)*")]
        [Required(ErrorMessage = "Required.")]
        public string Role { get; set; }

        public DateTime RegistrationCloseDate
        {
            get
            {
                var timeSetting = WebConfigurationManager.AppSettings["RegistrationClose"];
                var registrationClose = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                return registrationClose;
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