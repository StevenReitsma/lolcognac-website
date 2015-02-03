using System.Web.Mvc;
using LoLTournament.Models;

namespace LoLTournament.Controllers
{
    public class TimetableController : Controller
    {
        public ActionResult Index()
        {
            return View(new TimetableIndexViewModel());
        }
        public ActionResult Statistics()
        {
            return View();
        }
    }
}
