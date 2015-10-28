using LoLTournament.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoLTournament.Controllers
{
    public class StatisticsController : Controller
    {
        public ActionResult Index()
        {
            return View(new StatisticsViewModel());
        }

        public ActionResult StudyProgrammeChart()
        {
            return View(new StudyProgrammeViewModel());
        }

        public ActionResult StatisticsOverTimeChart()
        {
            return View(new StatisticsOverTimeViewModel());
        }

        public ActionResult ChampionsPlayedChart()
        {
            return View(new ChampionsPlayedViewModel());
        }
    }
}