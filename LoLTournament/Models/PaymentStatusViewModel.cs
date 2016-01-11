using LoLTournament.Models.Financial;

namespace LoLTournament.Models
{
    public class PaymentStatusViewModel
    {
        public PaymentStatus? Status { get; set; }
        public string TeamName { get; set; }
        public string PaymentUrl { get; set; }
    }
}
