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
        }

        public static void SendOfficialLoLRegistrationReminder(string toMail, string toName)
        {
        }

        public static void SendPaymentFailure(string toMail, string toName, PaymentStatus? status)
        {
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
