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
using LoLTournament.Models.Financial;
using MongoDB.Driver.Builders;

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
            timeSetting = WebConfigurationManager.AppSettings["RegistrationStart"];
            var registrationStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            timeSetting = WebConfigurationManager.AppSettings["RegistrationStartEarlyBird"];
            var registrationStartEarlyBird = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            // Check registration date, throw 403 otherwise.
            if (DateTime.Now >= registrationClose || DateTime.Now < registrationStartEarlyBird)
            {
                Response.StatusCode = 403;
                return PartialView();
            }

            // Get amount of RU students, CognAC and Dorans
            var permitCount = Convert.ToInt16(m.TeamCaptainRUStudent || m.TeamCaptainCognAC || m.TeamCaptainDorans) +
                              Convert.ToInt16(m.Summoner2RUStudent || m.Summoner2CognAC || m.Summoner2Dorans) +
                              Convert.ToInt16(m.Summoner3RUStudent || m.Summoner3CognAC || m.Summoner3Dorans) +
                              Convert.ToInt16(m.Summoner4RUStudent || m.Summoner4CognAC || m.Summoner4Dorans) +
                              Convert.ToInt16(m.Summoner5RUStudent || m.Summoner5CognAC || m.Summoner5Dorans);
            var earlyBirdCount = Convert.ToInt16(m.TeamCaptainCognAC || m.TeamCaptainDorans) +
                                 Convert.ToInt16(m.Summoner2CognAC || m.Summoner2Dorans) +
                                 Convert.ToInt16(m.Summoner3CognAC || m.Summoner3Dorans) +
                                 Convert.ToInt16(m.Summoner4CognAC || m.Summoner4Dorans) +
                                 Convert.ToInt16(m.Summoner5CognAC || m.Summoner5Dorans);

            // Check for at least 4 permitted clients
            if (permitCount < 4)
                ModelState.AddModelError("permitCount", "The team does not exist of at least 4 RU students, CognAC members, or Dorans members.");

            // Check for early-bird access
            if (DateTime.Now >= registrationStartEarlyBird && DateTime.Now < registrationStart)
            {
                if (earlyBirdCount < 5)
                    ModelState.AddModelError("earlyBirdAccess",
                        "Registrations are currently only open for teams that have at least five CognAC and/or Dorans members.");

                if (Mongo.Teams.Count(Query<Team>.Where(x => !x.Cancelled)) >= 12)
                    ModelState.AddModelError("earlyBirdFull",
                        "All 12 early-bird spots are now taken. You can retry registering after the tournament registration officially opens for all students.");
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
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.TeamCaptainStudy,
                    TeamId = teamId,
                    SummonerName = m.TeamCaptainName,
                    RuStudent = m.TeamCaptainRUStudent,
                    CognAC = m.TeamCaptainCognAC,
                    Dorans = m.TeamCaptainDorans,
                    StudentNumber = m.TeamCaptainStudentNumber
                };

                var summoner2 = new Participant
                {
                    Email = m.Summoner2Email,
                    FullName = m.Summoner2RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.Summoner2Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner2Name,
                    RuStudent = m.Summoner2RUStudent,
                    CognAC = m.Summoner2CognAC,
                    Dorans = m.Summoner2Dorans,
                    StudentNumber = m.Summoner2StudentNumber
                };

                var summoner3 = new Participant
                {
                    Email = m.Summoner3Email,
                    FullName = m.Summoner3RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.Summoner3Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner3Name,
                    RuStudent = m.Summoner3RUStudent,
                    CognAC = m.Summoner3CognAC,
                    Dorans = m.Summoner3Dorans,
                    StudentNumber = m.Summoner3StudentNumber
                };

                var summoner4 = new Participant
                {
                    Email = m.Summoner4Email,
                    FullName = m.Summoner4RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.Summoner4Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner4Name,
                    RuStudent = m.Summoner4RUStudent,
                    CognAC = m.Summoner4CognAC,
                    Dorans = m.Summoner4Dorans,
                    StudentNumber = m.Summoner4StudentNumber
                };

                var summoner5 = new Participant
                {
                    Email = m.Summoner5Email,
                    FullName = m.Summoner5RealName,
                    IsCaptain = false,
                    LastUpdateTime = new DateTime(1900, 1, 1),
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.Summoner5Study,
                    TeamId = teamId,
                    SummonerName = m.Summoner5Name,
                    RuStudent = m.Summoner5RUStudent,
                    CognAC = m.Summoner5CognAC,
                    Dorans = m.Summoner5Dorans,
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
                    ParticipantsIds = listParticipantIds,
                    Cancelled = false,
                };

                // Add team to database
                Mongo.Teams.Insert(team);

                // Create iDeal payment
                var key = WebConfigurationManager.AppSettings["MollieLiveKey"];
                var client = new MollieClient {ApiKey = key};

                var template = new PaymentTemplate {Amount = team.Price, Description = "CognAC League of Legends Tournament 2016", RedirectUrl = "https://lolcognac.nl/Payment/Status/" + team.Id, Method = m.PaymentMethod };
                var status = client.StartPayment(template);
                m.PaymentUrl = status.Links.PaymentUrl;
                m.Price = status.Amount;

                status.TeamId = team.Id;

                Mongo.Payments.Insert(status);

                // Send email
                EmailHelper.SendOfficialLoLRegistrationReminder(captain.Email, captain.FullName);

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
                    RegisterTime = DateTime.UtcNow,
                    StudyProgram = m.Study,
                    SummonerName = m.Name,
                    Roles = m.Role,
                    StudentNumber = m.StudentNumber,
                    RUStudent = m.RUStudent,
                    CognAC = m.CognAC,
                    Dorans = m.Dorans
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