using System.Web.Mvc;
using System.Web.Security;
using LoLTournament.Models.Admin;

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

        // GET: Admin/Participants
        [Authorize]
        public ActionResult Participants()
        {
            return View();
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
    }
}
