using Newtonsoft.Json;

namespace LoLTournament.Models.Financial
{
    public class PaymentLinks
    {
        /// <summary>
        /// The URL the consumer should visit to make the payment. This is where you redirect the consumer to.
        /// </summary>
        [JsonProperty("paymentUrl")]
        public string PaymentUrl { get; set; }

        /// <summary>
        /// The URL the consumer will be redirected to after completing or cancelling the payment process.
        /// </summary>
        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// The URL Mollie will call as soon an important status change takes place.
        /// </summary>
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }
    }
}
