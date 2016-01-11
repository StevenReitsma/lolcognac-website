using System.Net;
using System.Web.Mvc;
using LoLTournament.Helpers;
using LoLTournament.Models;
using LoLTournament.Models.Financial;
using MongoDB.Bson;
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
            payment.Update();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult Status(string id)
        {
            var team = Mongo.Teams.FindOne(Query<Team>.Where(x => x.Id == ObjectId.Parse(id)));
            var payment = Mongo.Payments.FindOne(Query<Payment>.Where(x => x.TeamId == team.Id));
            payment.Update();

            var model = new PaymentStatusViewModel { TeamName = team.Name, Status = payment.Status, PaymentUrl = payment.Links.PaymentUrl };

            return View(model);
        }
    }
}
