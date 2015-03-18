using System.Web.Mvc;
using LoLTournament.Models;

namespace LoLTournament.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Photos()
        {
            return View(new PhotosViewModel());
        }

        public ActionResult Information()
        {
            return View();
        }
    }
}