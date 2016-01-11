using Newtonsoft.Json;

namespace LoLTournament.Models.Financial
{
    public class PaymentTemplate
    {
        /// <summary>
        /// (Required) The exact amount you want to charge your buyer in Euro's. If you want to charge € 99,95 provide 99.95 as the value.
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// (Required) The description for this payment. This will be shown to your buyer in our payment screens and on their bank statement if the payment method supports that.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// (Required) The URL that you want us to send your buyer to after he completes the payment. It's important to include some kind of unique identifier to this URL - like an order ID - so that you can directly show the right screen to your buyer.
        /// </summary>
        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// (Optional) Use this parameter to directly select a payment method to use. Your buyer will then skip the payment method selection screen and will be sent directly to the payment method.
        /// </summary>
        [JsonProperty("method")]
        public PaymentMethod? Method { get; set; }

        /// <summary>
        /// (Optional) Use this parameter to store your own custom parameters with the payment. When you retrieve the payment details, you'll get these custom parameters back as well. You can provide around 1 KB of data.
        /// </summary>
        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        /// <summary>
        /// (Optional) If you pass a valid URL as this parameter, we will use this URL as the web hook instead of the web hook that is set in the web site profile.
        /// </summary>
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Valid values: nl, fr, de, en, es
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; }

        //(Optional) Creditcard and/or paypal parameters. Countries must be specified in ISO 3166-1 alpha-2 format.
        [JsonProperty("billingAddress")]
        public string BillingAddress { get; set; }
        [JsonProperty("billingCity")]
        public string BillingCity { get; set; }
        [JsonProperty("billingRegion")]
        public string BillingRegion { get; set; }
        [JsonProperty("billingPostal")]
        public string BillingPostal { get; set; }
        [JsonProperty("billingCountry")]
        public string BillingCountry { get; set; }
        [JsonProperty("shippingAddress")]
        public string ShippingAddress { get; set; }
        [JsonProperty("shippingCity")]
        public string ShippingCity { get; set; }
        [JsonProperty("shippingRegion")]
        public string ShippingRegion { get; set; }
        [JsonProperty("shippingPostal")]
        public string SshippingPostal { get; set; }
        [JsonProperty("shippingCountry")]
        public string ShippingCountry { get; set; }

        /// <summary>
        /// (Optional) Bank transfer parameter: Email address of the customer where he/she will receive the bank transfer details.
        /// </summary>
        [JsonProperty("billingEmail")]
        public string BillingEmail { get; set; }

        /// <summary>
        /// (Optional) Date that the payment automatically expires. Format YYYY-MM-DD.
        /// </summary>
        [JsonProperty("dueDate")]
        public string DueDate { get; set; }

        /// <summary>
        /// (Optional) Paysafecard parameter. Used for identifying the customer. For example the remote IP address of the customer.
        /// </summary>
        [JsonProperty("customerReference")]
        public string CustomerReference { get; set; }

        /// <summary>
        /// (Optional) iDEAL issuer id. The id could for example be ideal_INGBNL2A. The returned paymentUrl will then directly point to the ING web site.
        /// </summary>
        [JsonProperty("issuer")]
        public string Issuer { get; set; }

    }
}
