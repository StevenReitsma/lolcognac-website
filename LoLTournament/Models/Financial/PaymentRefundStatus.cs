using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LoLTournament.Models.Financial
{
    public class PaymentRefundStatus
    {
        /// <summary>
        /// The refund's unique identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The original payment.
        /// </summary>
        [JsonProperty("payment")]
        public Payment Payment { get; set; }

        /// <summary>
        /// The amount refunded to the consumer with this refund.
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Since refunds may be delayed for certain payment methods, the refund carries a status field.
        /// </summary>
        [JsonProperty("status")]
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// The date and time the refund was issued, in ISO 8601 format.
        /// </summary>
        [JsonProperty("refundedDateTime")]
        public DateTime? RefundedDateTime { get; set; }
    }
}
