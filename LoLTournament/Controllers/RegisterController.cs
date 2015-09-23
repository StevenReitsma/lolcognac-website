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
            if (DateTime.Now >= new DateTime(2015, 2, 20))
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
                    SummonerName = m.TeamCaptainName
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
                    SummonerName = m.Summoner2Name
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
                    SummonerName = m.Summoner3Name
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
                    SummonerName = m.Summoner4Name
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
                    SummonerName = m.Summoner5Name
                };

                // TODO create database wrapper + mongoclient singleton so we don't have to reconnect constantly
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

            SummonerRegisterInfoModel summoner;
            try
            {
                // TODO fix cringy magic numbers (rate limits)
                summoner = new SummonerRegisterInfoModel(RiotApi.GetInstance(key, 3000, 180000).GetSummoner(Region.euw, name));
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