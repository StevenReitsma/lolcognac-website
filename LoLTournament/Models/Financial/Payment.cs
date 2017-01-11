using System;
using System.Linq;
using System.Web.Configuration;
using LoLTournament.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

namespace LoLTournament.Models.Financial
{
    public class Payment
    {
        public ObjectId Id { get; set; }
        public ObjectId TeamId { get; set; }

        /// <summary>
        /// The transaction ID for the payment
        /// </summary>
        [JsonProperty("id")]
        public string MollieId { get; set; }

        /// <summary>
        /// test or live, This depends on what API key you used in creating the payment
        /// </summary>
        [JsonProperty("mode")]
        public PaymentMode Mode { get; set; }

        /// <summary>
        /// The exact date and time the payment was created, in ISO-8601 format. All dates and times are in the GMT time zone.
        /// </summary>
        [JsonProperty("createdDatetime")]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// The current status of the payment.
        /// </summary>
        [JsonProperty("status")]
        public PaymentStatus? Status { get; set; }

        /// <summary>
        /// The exact date and time the payment was marked as paid, in ISO-8601 format.
        /// </summary>
        [JsonProperty("paidDatetime")]
        public DateTime? PaidDateTime { get; set; }

        /// <summary>
        /// The exact date and time the payment was cancelled, in ISO-8601 format.
        /// </summary>
        [JsonProperty("cancelledDatetime")]
        public DateTime? CancelledDateTime { get; set; }

        /// <summary>
        /// The exact date and time the payment expired, in ISO-8601 format.
        /// </summary>
        [JsonProperty("expiredDatetime")]
        public DateTime? ExpiredDateTime { get; set; }

        /// <summary>
        /// The time until a payment will expire in ISO 8601 duration format.
        /// </summary>
        [JsonProperty("expiryPeriod")]
        public string ExpiryPeriod { get; set; }

        /// <summary>
        /// The amount you specified for this payment in Euro's.
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Only available when refunds are available for this payment - The total amount in EURO that is already refunded. For some payment methods, this amount may be higher than the payment amount, for example to allow reimbursement of the costs for a return shipment to the consumer.
        /// </summary>
        [JsonProperty("amountRefunded")]
        public decimal AmountRefunded { get; set;}

        /// <summary>
        /// Only available when refunds are available for this payment - The remaining amount in EURO that can be refunded.
        /// </summary>
        [JsonProperty("amountRemaining")]
        public decimal AmountRemaining { get; set; }

        /// <summary>
        /// The description of the payment.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The payment method that was ultimately used to complete this payment or null when no payment method was selected yet.
        /// </summary>
        [JsonProperty("method")]
        public PaymentMethod? Method { get; set; }

        /// <summary>
        /// Any details the specific payment method provided after completing the payment. In case of iDEAL this will contain the bank account number (IBAN).
        /// </summary>
        [JsonProperty("details")]
        public PaymentDetails Details { get; set; }

        /// <summary>
        /// Contains various links relevant to the payment. The most important one is paymentUrl, after creating your payment you should send your buyer to this URL. Warning: you cannot use the paymentUrl in an iframe.
        /// </summary>
        [JsonProperty("links")]
        public PaymentLinks Links { get; set; }

        /// <summary>
        /// Contains any metadata you've provided.
        /// </summary>
        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        /// <summary>
        /// The consumer's locale, either forced on creation by specifying the locale parameter, or detected by us during checkout.
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// The identifier referring to the profile this payment was created on.
        /// </summary>
        [JsonProperty("profileId")]
        public string ProfileId { get; set; }
    
        /// <summary>
        /// Updates the Mollie payment in the database.
        /// </summary>
        public void Update()
        {
            var key = WebConfigurationManager.AppSettings["MollieLiveKey"];

            var client = new MollieClient { ApiKey = key };
            var updatedPayment = client.GetStatus(MollieId);

            var oldStatus = Status;

            Status = updatedPayment.Status;
            AmountRefunded = updatedPayment.AmountRefunded;
            AmountRemaining = updatedPayment.AmountRemaining;
            CancelledDateTime = updatedPayment.CancelledDateTime;
            CreatedDateTime = updatedPayment.CreatedDateTime;
            Details = updatedPayment.Details;
            PaidDateTime = updatedPayment.PaidDateTime;
            ExpiredDateTime = updatedPayment.ExpiredDateTime;
            Locale = updatedPayment.Locale;

            Mongo.Payments.Save(this);

            var team = Mongo.Teams.FindOne(Query<Team>.Where(x => x.Id == TeamId));

            // If payment status did not change, return
            if (oldStatus == Status) return;

            // Cancel team in database
            if (Status == PaymentStatus.Cancelled || Status == PaymentStatus.Expired)
            {
                team.Cancelled = true;

                Mongo.Teams.Save(team);

                // Send email
                var captain = team.Participants.Single(x => x.IsCaptain);
                EmailHelper.SendPaymentFailure(captain.Email, captain.FullName, Status);
            }
            else if (Status == PaymentStatus.Refunded)
            {
                // Send email
                var captain = team.Participants.Single(x => x.IsCaptain);
                EmailHelper.SendPaymentPartialRefund(captain.Email, captain.FullName);

                Mongo.Teams.Save(team);
            }
            else if (Status == PaymentStatus.Paid)
            {
                // Send email
                var captain = team.Participants.Single(x => x.IsCaptain);
                EmailHelper.SendPaymentSuccess(captain.Email, captain.FullName);

                // Force summoner scrape
                try
                {
                    new RiotApiScrapeJob().ScrapeSummoners(null);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Returns the name of the team associated with this payment.
        /// </summary>
        [BsonIgnore]
        public string TeamName
        {
            get { return Mongo.Teams.FindOne(Query<Team>.Where(x => x.Id == TeamId)).Name; }
        }
    }
}
