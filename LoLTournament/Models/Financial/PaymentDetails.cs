using Newtonsoft.Json;

namespace LoLTournament.Models.Financial
{
    public class PaymentDetails
    {
        /// <summary>
        /// The consumer's name.
        /// </summary>
        [JsonProperty("consumerName")]
        public string ConsumerName { get; set; }

        /// <summary>
        /// The consumer's IBAN.
        /// </summary>
        [JsonProperty("consumerAccount")]
        public string ConsumerAccount { get; set; }

        /// <summary>
        /// The consumer's bank's BIC.
        /// </summary>
        [JsonProperty("consumerBic")]
        public string ConsumerBic { get; set; }

        /// <summary>
        /// The card holder's name.
        /// </summary>
        [JsonProperty("cardHolder")]
        public string CardHolder { get; set; }

        /// <summary>
        /// The last four digits of the card number.
        /// </summary>
        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        /// <summary>
        /// The card's security type.
        /// </summary>
        [JsonProperty("cardSecurity")]
        public CardSecurityType CardSecurity { get; set; }

        /// <summary>
        /// The name of the bank the consumer should wire the amount to.
        /// </summary>
        [JsonProperty("bankName")]
        public string BankName { get; set; }

        /// <summary>
        /// The IBAN the consumer should wire the amount to.
        /// </summary>
        [JsonProperty("bankAccount")]
        public string BankAccount { get; set; }

        /// <summary>
        /// The BIC of the bank the consumer should wire the amount to.
        /// </summary>
        [JsonProperty("bankBic")]
        public string BankBic { get; set; }

        /// <summary>
        /// The reference the consumer should use when wiring the amount. Note you should not apply any formatting here; show it to the consumer as-is.
        /// </summary>
        [JsonProperty("transferReference")]
        public string TransferReference { get; set; }

        /// <summary>
        /// The bitcoin address the bitcoins were transferred to.
        /// </summary>
        [JsonProperty("bitcoinAddress")]
        public string BitcoinAddress { get; set; }

        /// <summary>
        /// The amount transferred in BTC.
        /// </summary>
        [JsonProperty("bitcoinAmount")]
        public string BitcoinAmount { get; set; }

        /// <summary>
        /// The BTC to EUR exchange rate applied to the payment.
        /// </summary>
        [JsonProperty("bitcoinRate")]
        public string BitcoinRate { get; set; }

        /// <summary>
        /// A URI that's understood by Bitcoin wallet clients and will cause such clients to prepare a transaction.
        /// </summary>
        [JsonProperty("bitcoinUri")]
        public string BitcoinUri { get; set; }

        /// <summary>
        /// PayPal's reference for the transaction.
        /// </summary>
        [JsonProperty("paypalReference")]
        public string PayPalReference { get; set; }

        /// <summary>
        /// The consumer identification supplied when the payment was created.
        /// </summary>
        [JsonProperty("customerReference")]
        public string CustomerReference { get; set; }
    }
}
