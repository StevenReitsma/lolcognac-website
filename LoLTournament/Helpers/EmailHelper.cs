using System.Net;
using System.Net.Mail;
using System.Web.Configuration;
using LoLTournament.Models.Financial;

namespace LoLTournament.Helpers
{
    public static class EmailHelper
    {
        public static void SendPaymentSuccess(string toMail, string toName)
        {
            SendMail(toMail, toName, "Payment successful", "Hi team captain!\n\nWe have successfully received your payment for the CognAC League of Legends Tournament 2016.\n\nBest, CognAC League of Legends Committee");
        }

        public static void SendOfficialLoLRegistrationReminder(string toMail, string toName)
        {
            SendMail(toMail, toName, "Registration complete", "Hi team captain!\n\nWe have received your registration. We would like to remind you that to complete your registration you and all your team members should also register at the official League of Legends website. You will receive an email on how to do this later.\n\nBest, CognAC League of Legends Committee");
        }

        public static void SendPaymentFailure(string toMail, string toName, PaymentStatus? status)
        {
            SendMail(toMail, toName, "Payment failed or refunded", "Hi team captain!\n\nYour payment for the CognAC League of Legends Tournament 2016 failed (status: " + status + "). Your team registration has been cancelled. You are welcome to try registering again.\n\nBest, CognAC League of Legends Committee");
        }

        private static void SendMail(string toMail, string toName, string subject, string body)
        {
            var fromAddress = new MailAddress("lol@svcognac.nl", "CognAC League of Legends Tournament");
            var toAddress = new MailAddress(toMail, toName);
            var password = WebConfigurationManager.AppSettings["EmailPassword"];

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, password)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
