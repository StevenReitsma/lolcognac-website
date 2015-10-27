using System.Web.Mvc;
using LoLTournament.Models;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson;

namespace LoLTournament.Controllers
{
    public class ScheduleController : Controller
    {
        public ActionResult Index()
        {
            return View(new ScheduleIndexViewModel());
        }

        public ActionResult Schedule()
        {
            return View(new ScheduleViewModel());
        }

        public ActionResult Team(string teamId)
        {
            if (teamId.IsNullOrWhiteSpace())
                return View((object)null);

            var teamObjectId = ObjectId.Parse(teamId);
            var model = new TeamViewModel(teamObjectId);

            return View(model);
        }
    }
}
