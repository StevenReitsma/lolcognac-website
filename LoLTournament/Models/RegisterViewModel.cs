using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Configuration;
using LoLTournament.Models.Financial;

namespace LoLTournament.Models
{
    public class RegisterViewModel
    {
        [DisplayName("Team name*")]
        [Required(ErrorMessage="Required.")]
        public string TeamName { get; set; }
        [DisplayName("Team captain summoner name*")]
        [Required(ErrorMessage="Required.")]
        public string TeamCaptainName { get; set; }
        [DisplayName("Team captain real name*")]
        [Required(ErrorMessage = "Required.")]
        public string TeamCaptainRealName { get; set; }
        [DisplayName("Team captain email address*")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage="This is not a valid email address.")]
        public string TeamCaptainEmail { get; set; }
        [DisplayName("Team captain study program (e.g. B Artificial Intelligence, M English, B Computer Science, etc.)")]
        public string TeamCaptainStudy { get; set; }
        [DisplayName("Team captain is a RU student (will be verified)")]
        public bool TeamCaptainRUStudent { get; set; }
        [DisplayName("Team captain is a member or benefactor of CognAC (will be verified)")]
        public bool TeamCaptainCognAC { get; set; }
        [DisplayName("Team captain is a member or benefactor of Dorans (will be verified)")]
        public bool TeamCaptainDorans { get; set; }
        [DisplayName("Team captain student number (only if student of RU)")]
        public string TeamCaptainStudentNumber { get; set; }

        [DisplayName("Player 2 summoner name*")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner2Name { get; set; }
        [DisplayName("Player 2 real name*")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner2RealName { get; set; }
        [DisplayName("Player 2 email address*")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner2Email { get; set; }
        [DisplayName("Player 2 study program")]
        public string Summoner2Study { get; set; }
        [DisplayName("Player 2 is a RU student (will be verified)")]
        public bool Summoner2RUStudent { get; set; }
        [DisplayName("Player 2 is a member or benefactor of CognAC (will be verified)")]
        public bool Summoner2CognAC { get; set; }
        [DisplayName("Player 2 is a member or benefactor of Dorans (will be verified)")]
        public bool Summoner2Dorans { get; set; }
        [DisplayName("Player 2 student number (only if student of RU)")]
        public string Summoner2StudentNumber { get; set; }

        [DisplayName("Player 3 summoner name*")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner3Name { get; set; }
        [DisplayName("Player 3 real name*")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner3RealName { get; set; }
        [DisplayName("Player 3 email address*")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner3Email { get; set; }
        [DisplayName("Player 3 study program")]
        public string Summoner3Study { get; set; }
        [DisplayName("Player 3 is a RU student (will be verified)")]
        public bool Summoner3RUStudent { get; set; }
        [DisplayName("Player 3 is a member or benefactor of CognAC (will be verified)")]
        public bool Summoner3CognAC { get; set; }
        [DisplayName("Player 3 is a member or benefactor of Dorans (will be verified)")]
        public bool Summoner3Dorans { get; set; }
        [DisplayName("Player 3 student number (only if student of RU)")]
        public string Summoner3StudentNumber { get; set; }

        [DisplayName("Player 4 summoner name*")]
        [Required(ErrorMessage="Required.")]
        public string Summoner4Name { get; set; }
        [DisplayName("Player 4 real name*")]
        [Required(ErrorMessage="Required.")]
        public string Summoner4RealName { get; set; }
        [DisplayName("Player 4 email address*")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner4Email { get; set; }
        [DisplayName("Player 4 study program")]
        public string Summoner4Study { get; set; }
        [DisplayName("Player 4 is a RU student (will be verified)")]
        public bool Summoner4RUStudent { get; set; }
        [DisplayName("Player 4 is a member or benefactor of CognAC (will be verified)")]
        public bool Summoner4CognAC { get; set; }
        [DisplayName("Player 4 is a member or benefactor of Dorans (will be verified)")]
        public bool Summoner4Dorans { get; set; }
        [DisplayName("Player 4 student number (only if student of RU)")]
        public string Summoner4StudentNumber { get; set; }

        [DisplayName("Player 5 summoner name*")]
        [Required(ErrorMessage="Required.")]
        public string Summoner5Name { get; set; }
        [DisplayName("Player 5 real name*")]
        [Required(ErrorMessage="Required.")]
        public string Summoner5RealName { get; set; }
        [DisplayName("Player 5 email address*")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner5Email { get; set; }
        [DisplayName("Player 5 study program")]
        public string Summoner5Study { get; set; }
        [DisplayName("Player 5 is a RU student (will be verified)")]
        public bool Summoner5RUStudent { get; set; }
        [DisplayName("Player 5 is a member or benefactor of CognAC (will be verified)")]
        public bool Summoner5CognAC { get; set; }
        [DisplayName("Player 5 is a member or benefactor of Dorans (will be verified)")]
        public bool Summoner5Dorans { get; set; }
        [DisplayName("Player 5 student number (only if student of RU)")]
        public string Summoner5StudentNumber { get; set; }
        [DisplayName("Payment method")]
        public PaymentMethod PaymentMethod { get; set; }

        public string PaymentUrl { get; set; }

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

        public DateTime RegistrationStartDateEarlyBird
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