using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LoLTournament.Models.Financial
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentStatus
    {
        /// <summary>
        /// The payment has been created, but no other status has been reached yet.
        /// </summary>
        [JsonProperty("open")]
        Open,

        /// <summary>
        /// Your customer has cancelled the payment.
        /// </summary>
        [JsonProperty("cancelled")]
        Cancelled,

        /// <summary>
        /// The payment has been started but not yet complete.
        /// </summary>
        [JsonProperty("pending")]
        Pending,

        /// <summary>
        /// The payment has been paid for.
        /// </summary>
        [JsonProperty("paid")]
        Paid,

        /// <summary>
        /// The payment has been paid for and we have transferred the sum to your bank account.
        /// </summary>
        [JsonProperty("paidout")]
        Paidout,

        /// <summary>
        /// The payment has been refunded.
        /// </summary>
        [JsonProperty("refunded")]
        Refunded,

        /// <summary>
        /// The payment has expired, for example, your customer has closed the payment screen.
        /// </summary>
        [JsonProperty("expired")]
        Expired,
    }
}
