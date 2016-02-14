using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
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
        [Authorize]
        public ActionResult TeamBuilder()
        {
            return View(new AdminTeamBuilderViewModel());
        }

        // GET: Admin/UpdateSummoners
        [Authorize]
        public ActionResult UpdateSummoners()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            // Update summoners on separate thread
            new Thread(() => new RiotApiScrapeJob().ScrapeSummoners(null)).Start();

            return RedirectToAction("Index", "Admin");
        }

        // GET: Admin/Matches
        [Authorize]
        public ActionResult Matches()
        {
            return View(new AdminMatchesViewModel());
        }

        // GET: Admin/MatchInfo
        [Authorize]
        public ActionResult MatchInfo()
        {
            var id = HttpContext.Request.QueryString["matchId"];

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            return View(match);
        }

        [Authorize]
        public ActionResult SwitchWin()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                match.SwitchWin();
                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary {{"matchId", id}});
            }

            return View(match);
        }

        [Authorize]
        public ActionResult RollbackMatch()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                match.Rollback();
                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary { { "matchId", id } });
            }

            return View(match);
        }

        [Authorize]
        public ActionResult ForceBlueWin()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                match.ForceBlueWin();
                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary { { "matchId", id } });
            }

            return View(match);
        }

        [Authorize]
        public ActionResult ForceRedWin()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                match.ForceRedWin();
                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary { { "matchId", id } });
            }

            return View(match);
        }

        [Authorize]
        public ActionResult DisqualifyTeam()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["teamId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var team = Mongo.Teams.FindOne(Query<Team>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                team.Disqualify();
                return RedirectToAction("Teams", "Admin");
            }

            return View(team);
        }
    }
}
