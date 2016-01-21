using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using LoLTournament.Helpers;
using LoLTournament.Models;
using LoLTournament.Models.Admin;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace LoLTournament.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        [Authorize]
        public ActionResult Index()
        {
            
            return View(new AdminDashboardViewModel());
        }

        // GET: Admin/Teams
        [Authorize]
        public ActionResult Teams()
        {
            return View(new AdminTeamsViewModel());
        }

        [Authorize]
        public ActionResult Team()
        {
            var id = HttpContext.Request.QueryString["teamId"];
            return View(Mongo.Teams.FindOne(Query<Team>.Where(x => x.Id == ObjectId.Parse(id))));
        }

        // GET: Admin/Participants
        [Authorize]
        public ActionResult Participants()
        {
            return View(new AdminParticipantsViewModel());
        }

        // GET: Admin/Finance
        [Authorize]
        public ActionResult Finance()
        {
            if (User.IsInRole("Financial"))
                return View(new AdminFinanceViewModel());

            return View("AuthenticationError");
        }

        // GET: Admin/Login
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return Logout();

            return View(new AdminLoginViewModel());
        }

        // POST: Admin/Login
        [HttpPost]
        public ActionResult Login(AdminLoginViewModel m)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(m.Username, m.Password))
                {
                    // Login the user, don't use cookies to remember
                    FormsAuthentication.RedirectFromLoginPage(m.Username, false);
                }
                else
                {
                    // Wrong username or password
                    ModelState.AddModelError("", "");
                }
            }

            return View(m);
        }

        // GET: Admin/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Admin");
        }

        // GET: Admin/TeamBuilder
        public ActionResult TeamBuilder()
        {
            return View(new AdminTeamBuilderViewModel());
        }

        // GET: Admin/UpdateSummoners
        public ActionResult UpdateSummoners()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            // Update summoners on separate thread
            new Thread(() => new RiotApiScrapeJob().ScrapeSummoners(null)).Start();

            return RedirectToAction("Index", "Admin");
        }
    }
}
