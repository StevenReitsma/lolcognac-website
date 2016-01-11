using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using LoLTournament.Models.Financial;
using Newtonsoft.Json;

namespace LoLTournament.Helpers
{
    public class MollieClient
    {
        private const string ApiEndpoint = "https://api.mollie.nl";
        private const string RemoteApiVersion = "v1";

        private string _apiKey;
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                var key = value.Trim();

                if (!Regex.IsMatch(key, "^(live|test)_\\w+$"))
                {
                    throw new Exception($"Invalid API key: '{key}'. An API key must start with 'test_' or 'live_'.");
                }

                _apiKey = key;
            }
        }

        public string LastRequestJson { get; private set; }
        public string LastResponseJson { get; private set; }

        /// <summary>
        /// Start a payment
        /// </summary>
        /// <param name="payment">Payment object</param>
        /// <returns></returns>
        public Payment StartPayment(PaymentTemplate payment)
        {
            var jsonData = LoadWebRequest("POST", "payments", JsonConvert.SerializeObject(payment));
            var status = JsonConvert.DeserializeObject<Payment>(jsonData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return status;
        }

        /// <summary>
        /// Get status of a payment
        /// </summary>
        /// <param name="id">The id of the payment</param>
        /// <returns></returns>
        public Payment GetStatus(string id)
        {
            var jsonData = LoadWebRequest("GET", "payments" + "/" + id, "");
            var status = JsonConvert.DeserializeObject<Payment>(jsonData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return status;
        }

        /// <summary>
        /// To refund a payment, you must have sufficient balance with Mollie for deducting the refund and its fees. You can find your current balance on the on the Mollie controlpanel.
        /// At the moment you can only process refunds for iDEAL, Bancontact/Mister Cash, SOFORT Banking, creditcard and bank transfer payments.
        /// 
        /// Refunds COMPLETE payment.
        /// </summary>
        /// <param name="id">The id of the payment</param>
        /// <returns></returns>
        public PaymentRefundStatus Refund(string id)
        {
            return Refund(id, 0);
        }

        /// <summary>
        /// To refund a payment, you must have sufficient balance with Mollie for deducting the refund and its fees. You can find your current balance on the on the Mollie controlpanel.
        /// At the moment you can only process refunds for iDEAL, Bancontact/Mister Cash, SOFORT Banking, creditcard and bank transfer payments.
        /// 
        /// Refunds only the amount you specify.
        /// </summary>
        /// <param name="id">The id of the payment</param>
        /// <param name="amount">The amount to refund</param>
        /// <returns></returns>
        public PaymentRefundStatus Refund(string id, decimal amount)
        {
            string jsonData = LoadWebRequest("POST", "payments" + "/" + id + "/refunds", (amount == 0) ? "" : JsonConvert.SerializeObject(new { amount = amount }));
            PaymentRefundStatus refundStatus = JsonConvert.DeserializeObject<PaymentRefundStatus>(jsonData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return refundStatus;
        }

        private string LoadWebRequest(string httpMethod, string resource, string postData)
        {
            LastRequestJson = postData;

            string url = ApiEndpoint + "/" + RemoteApiVersion + "/" + resource;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = httpMethod;
            request.Accept = "application/json";
            request.Headers.Add("Authorization", "Bearer " + ApiKey);
            request.UserAgent = "CLT Mollie C# Client";

            if (postData != "")
            {
                //Send the request and get the response
                request.ContentType = "application/json";
                using (StreamWriter streamOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    streamOut.Write(postData);
                    streamOut.Close();
                }
            }

            string strResponse = "";

            try
            {
                using (StreamReader streamIn = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    strResponse = streamIn.ReadToEnd();
                    streamIn.Close();
                }
            }
            catch (Exception ex)
            {
                var exception = ex as WebException;
                if (exception != null && exception.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse errResp = exception.Response;
                    using (Stream respStream = errResp.GetResponseStream())
                    {
                        using (StreamReader r = new StreamReader(respStream))
                        {
                            strResponse = r.ReadToEnd();
                            r.Close();
                        }
                        respStream.Close();
                        throw new Exception(strResponse);
                    }
                }
            }

            LastResponseJson = strResponse;

            return strResponse;
        }
    }
}
