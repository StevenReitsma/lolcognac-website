using LoLTournament.Models;
using System.Web.Mvc;

namespace LoLTournament.Controllers
{
    public class StatisticsController : Controller
    {
        public ActionResult Index()
        {
            return View(new StatisticsViewModel());
        }

        public JsonResult StudyProgrammeData()
        {
            return Json(new StudyProgrammeViewModel());
        }

        public JsonResult StatisticsOverTimeData()
        {
            return Json(new StatisticsOverTimeViewModel());
        }

        public JsonResult ChampionsPlayedData()
        {
            return Json(new ChampionsPlayedViewModel());
        }

        public JsonResult ChampionsBannedData()
        {
            return Json(new ChampionsBannedViewModel());
        }

        public JsonResult LeagueData()
        {
            return Json(new LeagueDataViewModel());
        }
    }
}
