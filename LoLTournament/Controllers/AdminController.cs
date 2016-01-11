using System.Web.Mvc;
using LoLTournament.Models.Admin;

namespace LoLTournament.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View(new AdminDashboardViewModel());
        }

        // GET: Admin/Teams
        public ActionResult Teams()
        {
            return View(new AdminTeamsViewModel());
        }

        // GET: Admin/Participants
        public ActionResult Participants()
        {
            return View();
        }

        // GET: Admin/Finance
        public ActionResult Finance()
        {
            return View(new AdminFinanceViewModel());
        }
    }
}
