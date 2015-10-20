using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LoLTournament.Models
{
    public class RegisterViewModel
    {
        [DisplayName("Team name")]
        [Required(ErrorMessage="Required.")]
        public string TeamName { get; set; }
        [DisplayName("Team captain summoner name")]
        [Required(ErrorMessage="Required.")]
        public string TeamCaptainName { get; set; }
        [DisplayName("Team captain real name")]
        [Required(ErrorMessage = "Required.")]
        public string TeamCaptainRealName { get; set; }
        [DisplayName("Team captain email address")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage="This is not a valid email address.")]
        public string TeamCaptainEmail { get; set; }
        [DisplayName("Team captain university and study program (e.g. RU Artificial Intelligence, UvA English, HAN Electrical Engineering, etc.)")]
        [Required(ErrorMessage = "Required.")]
        public string TeamCaptainStudy { get; set; }

        [DisplayName("Player 2 summoner name")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner2Name { get; set; }
        [DisplayName("Player 2 real name")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner2RealName { get; set; }
        [DisplayName("Player 2 email address")]
        [Required(ErrorMessage = "Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner2Email { get; set; }
        [DisplayName("Player 2 university and study program")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner2Study { get; set; }

        [DisplayName("Player 3 summoner name")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner3Name { get; set; }
        [DisplayName("Player 3 real name")]
        [Required(ErrorMessage = "Required.")]
        public string Summoner3RealName { get; set; }
        [DisplayName("Player 3 email address")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner3Email { get; set; }
        [DisplayName("Player 3 university and study program")]
        [Required(ErrorMessage="Required.")]
        public string Summoner3Study { get; set; }

        [DisplayName("Player 4 summoner name")]
        [Required(ErrorMessage="Required.")]
        public string Summoner4Name { get; set; }
        [DisplayName("Player 4 real name")]
        [Required(ErrorMessage="Required.")]
        public string Summoner4RealName { get; set; }
        [DisplayName("Player 4 email address")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner4Email { get; set; }
        [DisplayName("Player 4 university and study program")]
        [Required(ErrorMessage="Required.")]
        public string Summoner4Study { get; set; }

        [DisplayName("Player 5 summoner name")]
        [Required(ErrorMessage="Required.")]
        public string Summoner5Name { get; set; }
        [DisplayName("Player 5 real name")]
        [Required(ErrorMessage="Required.")]
        public string Summoner5RealName { get; set; }
        [DisplayName("Player 5 email address")]
        [Required(ErrorMessage="Required.")]
        [EmailAddress(ErrorMessage = "This is not a valid email address.")]
        public string Summoner5Email { get; set; }
        [DisplayName("Player 5 university and study program")]
        [Required(ErrorMessage="Required.")]
        public string Summoner5Study { get; set; }
    }
}