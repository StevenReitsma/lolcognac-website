using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using LoLTournament.Helpers;
using LoLTournament.Models;
using LoLTournament.Models.Admin;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using RiotSharp;

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

        [Authorize]
        public ActionResult GetMatchDetails()
        {
            if (User.Identity.Name != "Steven")
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                var key = WebConfigurationManager.AppSettings["RiotTournamentApiKey"];
                var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
                var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

                var api = TournamentRiotApi.GetInstance(key, rateLimit1, rateLimit2);

                MatchScraper.GetMatchDetails(api, match, false);
                return RedirectToAction("Matches", "Admin");
            }

            return View(match);
        }

        [Authorize]
        public ActionResult DeleteMatch()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            if (confirmation)
            {
                Mongo.Matches.Remove(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));
                return RedirectToAction("Matches", "Admin");
            }

            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            return View(match);
        }

        [Authorize]
        public ActionResult SetMatchDuration()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;
            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                match.Duration = TimeSpan.Parse(HttpContext.Request.QueryString["confirmation"]);

                Mongo.Matches.Save(match);
                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary { { "matchId", id } });
            }

            return View(match);
        }

        [Authorize]
        public ActionResult NewCode()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var id = HttpContext.Request.QueryString["matchId"];
            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;
            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.Id == ObjectId.Parse(id)));

            if (confirmation)
            {
                var allowedSummoners = Mongo.Teams.Find(Query<Team>.Where(team => team.Id == match.BlueTeamId || team.Id == match.RedTeamId))
                    .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                    .ToList();
                TournamentCodeFactory.UpdateTournamentCodePlayers(match.TournamentCode,
                    allowedSummoners);
                TournamentCodeFactory.UpdateTournamentCodePlayersBlind(match.TournamentCodeBlind,
                    allowedSummoners);

                return RedirectToAction("MatchInfo", "Admin", new RouteValueDictionary { { "matchId", id } });
            }

            return View(match);
        }

        [Authorize]
        public ActionResult NewCodeAll()
        {
            if (!User.IsInRole("Edit"))
                return View("AuthenticationError");

            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            if (confirmation)
            { 
                // Update all tournament codes that are currently defined
                foreach (var match in Mongo.Matches.Find(Query<Match>.Where(m => m.TournamentCode != null && m.TournamentCode != "")))
                {
                    var allowedSummoners = Mongo.Teams.Find(Query<Team>.Where(team => team.Id == match.BlueTeamId || team.Id == match.RedTeamId))
                        .SelectMany(team => team.Participants.Select(participant => participant.Summoner.Id))
                        .ToList();
                    TournamentCodeFactory.UpdateTournamentCodePlayers(match.TournamentCode,
                        allowedSummoners);
                    TournamentCodeFactory.UpdateTournamentCodePlayersBlind(match.TournamentCodeBlind,
                        allowedSummoners);
                }

                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [Authorize]
        public ActionResult BuildTournamentStructure()
        {
            if (User.Identity.Name != "Steven")
                return View("AuthenticationError");

            var confirmation = HttpContext.Request.QueryString["confirmation"] != null;

            if (confirmation)
            {
                BracketHelper.CreatePoolStructure();
                BracketHelper.CreateFinaleStructure();

                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [Authorize]
        public ActionResult BadgeExport()
        {
            return new FileContentResult(ExportHelper.ExportBadgeList(), "text/csv");
        }
    }
}
