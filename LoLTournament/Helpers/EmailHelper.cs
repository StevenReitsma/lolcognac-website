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
            SendMail(toMail, toName, "Payment successful", "Hi team captain!\n\nWe have successfully received your payment for the LCN 2017. Your registration is now final.\n\nBest,\nLCN Committee");
        }

        public static void SendOfficialLoLRegistrationReminder(string toMail, string toName)
        {
            SendMail(toMail, toName, "Registration complete", "Hi team captain!\n\nWe have received your registration. We would like to remind you that to complete your registration you and all your team members should also register at the official League of Legends website. You can do this through the following link:\n\nhttp://events.euw.leagueoflegends.com/events/233255\n\nPlease make sure to mention your team name as well.\n\nBest,\nLCN Committee");
        }

        public static void SendPaymentFailure(string toMail, string toName, PaymentStatus? status)
        {
            SendMail(toMail, toName, "Payment failed or refunded", "Hi team captain!\n\nYour payment for the LCN 2017 failed (status: " + status + "). Your team registration has been cancelled. You are welcome to try registering again.\n\nBest,\nLCN Committee");
        }

        private static void SendMail(string toMail, string toName, string subject, string body)
        {
            var fromAddress = new MailAddress("lol@svcognac.nl", "League of Legends Championship Nijmegen");
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
