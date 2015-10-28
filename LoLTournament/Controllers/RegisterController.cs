using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using LoLTournament.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using RiotSharp;
using LoLTournament.Helpers;
using System.Globalization;

namespace LoLTournament.Controllers
{
    public class RegisterController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new RegisterViewModel());
        }

        [HttpGet]
        public ActionResult Teambuilder()
        {
            return View(new TeambuilderViewModel());
        }

        [HttpPost]
        public PartialViewResult Index(RegisterViewModel m)
        {
            var timeSetting = WebConfigurationManager.AppSettings["RegistrationClose"];
            var registrationClose = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            if (DateTime.Now >= registrationClose) // if registration date has passed
            {
                Response.StatusCode = 403;
                return PartialView();
            }

            if (ModelState.IsValid)
            {
                // Note: we don't actually retrieve stuff from the Riot servers here. We do this in a separate cron thread asynchronously every hour (outside event) or every minute (during event).

                var teamId = ObjectId.GenerateNewId();

                var captain = new Participant
                {
                    Email = m.TeamCaptainEmail,
                    FullName = m.TeamCaptainRealName,
                    IsCaptain = true,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.TeamCaptainStudy,
                    TeamId = teamId,
                    SummonerName = m.TeamCaptainName,
                    RuStudent = m.TeamCaptainRUStudent,
                    CognACDorans = m.TeamCaptainCognACDorans,
                    StudentNumber = m.TeamCaptainStudentNumber
                };

                var summoner2 = new Participant
                {
                    Email = m.Summoner2Email,
                    FullName = m.Summoner2RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.Summoner2Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner2Name,
                    RuStudent = m.Summoner2RUStudent,
                    CognACDorans = m.Summoner2CognACDorans,
                    StudentNumber = m.Summoner2StudentNumber
                };

                var summoner3 = new Participant
                {
                    Email = m.Summoner3Email,
                    FullName = m.Summoner3RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.Summoner3Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner3Name,
                    RuStudent = m.Summoner3RUStudent,
                    CognACDorans = m.Summoner3CognACDorans,
                    StudentNumber = m.Summoner3StudentNumber
                };

                var summoner4 = new Participant
                {
                    Email = m.Summoner4Email,
                    FullName = m.Summoner4RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.Summoner4Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner4Name,
                    RuStudent = m.Summoner4RUStudent,
                    CognACDorans = m.Summoner4CognACDorans,
                    StudentNumber = m.Summoner4StudentNumber
                };

                var summoner5 = new Participant
                {
                    Email = m.Summoner5Email,
                    FullName = m.Summoner5RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.Summoner5Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner5Name,
                    RuStudent = m.Summoner5RUStudent,
                    CognACDorans = m.Summoner5CognACDorans,
                    StudentNumber = m.Summoner5StudentNumber
                };

                Mongo.Participants.Insert(captain);
                Mongo.Participants.Insert(summoner2);
                Mongo.Participants.Insert(summoner3);
                Mongo.Participants.Insert(summoner4);
                Mongo.Participants.Insert(summoner5);
  
                var listParticipantIds = new List<ObjectId> {captain.Id, summoner2.Id, summoner3.Id, summoner4.Id, summoner5.Id};

                var team = new Team
                {
                    CaptainId = captain.Id,
                    Id = teamId,
                    Name = m.TeamName,
                    ParticipantsIds = listParticipantIds
                };

                // Add team to database
                Mongo.Teams.Insert(team);

                return PartialView("OK", m);
            }

            // Model state is invalid, return form.
            return PartialView("Form", m);
        }

        [HttpPost]
        public PartialViewResult Teambuilder(TeambuilderViewModel m)
        {
            if (ModelState.IsValid)
            {
                // Note: we don't actually retrieve stuff from the Riot servers here. We do this in a separate cron thread asynchronously every hour (outside event) or every minute (during event).

                var buildee = new TeambuilderParticipant
                {
                    Email = m.Email,
                    FullName = m.RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.Now,
                    StudyProgram = m.Study,
                    SummonerName = m.Name,
                    Roles = m.Role
                };

                Mongo.TeamBuilderParticipants.Insert(buildee);

                // Model is valid and participant has been added to DB, return OK message
                return PartialView("TeambuilderOK", m);
            }

            // Model is not valid, return form
            return PartialView("TeambuilderForm", m);
        }

        [HttpPost]
        public PartialViewResult GetSummonerInfo(string name)
        {
            if (string.IsNullOrEmpty(name.Trim()))
                return null;

            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            SummonerRegisterInfoModel summoner;
            try
            {
                summoner = new SummonerRegisterInfoModel(RiotApi.GetInstance(key, rateLimit1, rateLimit2).GetSummoner(Region.euw, name));
            }
            catch (Exception e)
            {
                // Summoner does not exist or riot API offline
                if (e.Message != "404, Resource not found")
                    return PartialView("SummonerInfo", new SummonerRegisterInfoModel(true));

                return PartialView("SummonerInfo", null);
            }

            return PartialView("SummonerInfo", summoner);
        }
    }
}