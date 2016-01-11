using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using LoLTournament.Helpers;
using LoLTournament.Models.Financial;
using MongoDB.Driver.Builders;

namespace LoLTournament.Controllers
{
    public class PaymentController : Controller
    {
        // POST: Payment/ChangeStatus
        [HttpPost]
        public ActionResult ChangeStatus(string id)
        {
            // Get payment from db
            var payment = Mongo.Payments.FindOne(Query<Payment>.Where(x => x.MollieId == id));

            // Check on iDeal payment
            var key = WebConfigurationManager.AppSettings["MollieTestKey"];
            var client = new MollieClient { ApiKey = key };

            var status = client.GetStatus(id);
            status.TeamId = payment.TeamId;
            status.Id = payment.Id;

            Mongo.Payments.Save(payment);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
