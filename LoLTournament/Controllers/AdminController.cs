using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoLTournament.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Teams
        public ActionResult Teams()
        {
            return View();
        }

        // GET: Admin/Participants
        public ActionResult Participants()
        {
            return View();
        }

        // GET: Admin/Finance
        public ActionResult Finance()
        {
            return View();
        }
    }
}
