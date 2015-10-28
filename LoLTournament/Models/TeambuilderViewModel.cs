using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Configuration;

namespace LoLTournament.Models
{
    public class TeambuilderViewModel
    {
        [DisplayName("Summoner name")]
        [Required(ErrorMessage = "Required.")]
        public string Name { get; set; }
        [DisplayName("Real name")]
        [Required(ErrorMessage = "Required.")]
        public string RealName { get; set; }
        [DisplayName("Email address")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Email { get; set; }
        [DisplayName("Study program (e.g. Artificial Intelligence, English, Law, etc.)")]
        [Required(ErrorMessage = "Required.")]
        public string Study { get; set; }

        [DisplayName("Preferred roles (i.e. AP, AD, support, top, jungle)")]
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
    }
}