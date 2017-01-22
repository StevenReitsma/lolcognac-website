using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using LoLTournament.Helpers;
using LoLTournament.Models;
using LoLTournament.Models.TournamentApi;
using MongoDB.Driver.Builders;
using RiotSharp;
using RiotSharp.MatchEndpoint;

namespace LoLTournament.Controllers
{
    public class MatchController : Controller
    {
        private readonly TournamentRiotApi _tournamentApi;

        public MatchController()
        {
            var tournamentKey = WebConfigurationManager.AppSettings["RiotTournamentApiKey"];
            var rateLimit1 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Seconds"]);
            var rateLimit2 = int.Parse(WebConfigurationManager.AppSettings["RateLimitPer10Minutes"]);

            _tournamentApi = TournamentRiotApi.GetInstance(tournamentKey, rateLimit1, rateLimit2);
        }

        private static void Log(string msg)
        {
            try
            {
#if DEBUG
                var path = "D:/Development/LoLTournament/match.txt";
#else
                var path = "/var/www/lolcognac.nl/match.txt";
#endif

                using (var f = new FileStream(path, FileMode.Append))
                {
                    var bytes = Encoding.UTF8.GetBytes(msg + "\n");
                    f.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception)
            {
            }
        }

        // GET: Match/Callback
        [HttpGet]
        public ActionResult Callback()
        {
            return Content("POST only.");
        }

        // POST: Match/Callback
        [HttpPost]
        public ActionResult Callback(CallbackResult obj)
        {
            if (obj == null)
            {
                Log("obj == null");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Log("obj != null");
            // Get the match from the database by looking up the tournament code
            var match = Mongo.Matches.FindOne(Query<Match>.Where(x => x.TournamentCode == obj.TournamentCode || x.TournamentCodeBlind == obj.TournamentCode));

            Log("tournament code = " + obj.TournamentCode);

            if (match == null)
            {
                Log("match == null");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Tournament code not found");
            }

            Log("match != null");

            // Check which side won
            var winningTeam = obj.WinningTeam.Select(y => y.SummonerId);
            var losingTeam = obj.LosingTeam.Select(y => y.SummonerId);

            Log("defined winningTeam, losingTeam");

            var blueSideWon = match.BlueTeam.Participants.All(x => winningTeam.Contains(x.Summoner.Id));
            var redSideWon = match.RedTeam.Participants.All(x => winningTeam.Contains(x.Summoner.Id));

            Log("defined blueSideWon, redSideWon");

            var blueSideLost = match.BlueTeam.Participants.All(x => losingTeam.Contains(x.Summoner.Id));
            var redSideLost = match.RedTeam.Participants.All(x => losingTeam.Contains(x.Summoner.Id));

            Log("defined blueSideLost, redSideLost");

            // Check if the results are sane
            if (blueSideWon && redSideLost || redSideWon && blueSideLost)
            {
                Log("result sane");

                // Set winner ID to blue if blue side won.
                match.WinnerId = blueSideWon ? match.BlueTeamId : match.RedTeamId;
                match.StartTime = obj.StartTime;
                match.FinishDate = DateTime.UtcNow;
                match.RiotMatchId = obj.GameId;
                match.Finished = true;
                match.Status = Status.Finished;

                // Save to database
                Mongo.Matches.Save(match);

                Log("saved preliminary stats");

                MatchDetail matchDetails;

                // Get more match details
                try
                {
                    matchDetails = _tournamentApi.GetTournamentMatch(Region.euw, obj.GameId, match.TournamentCode, false);
                }
                catch (Exception)
                {
                    Log("match not found, deferring to scraper");

                    match.Invalid = true;
                    match.InvalidReason = "MATCH_NOT_FOUND";

                    // Save to database
                    Mongo.Matches.Save(match);
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Match ID not found");
                }
                
                // Get the players that were supposed to be on blue side
                var supposedToBeBluePlayers = matchDetails.Participants.Where(
                    matchDetailParticipant =>
                        match.BlueTeam.Participants.Select(
                            localMatchParticipant => localMatchParticipant.Summoner.Name.ToLower())
                            .Contains(
                                matchDetails.ParticipantIdentities.Single(
                                    identity => identity.ParticipantId == matchDetailParticipant.ParticipantId)
                                    .Player.SummonerName.ToLower())).ToList();

                if (supposedToBeBluePlayers.Count == 0)
                {
                    Log("supposedToBeBluePlayers.Count == 0");

                    // Teams did not have correct summoners playing the match. Match invalid.
                    match.Invalid = true;
                    match.InvalidReason = "INCORRECT_SUMMONER_COUNT_BLUE_TEAM";

                    // Save to database
                    Mongo.Matches.Save(match);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }

                // Set the team id to the side they actually played
                var blueTeamId = supposedToBeBluePlayers.First().TeamId;

                // Get the players that were supposed to be on red side
                var supposedToBeRedPlayers = matchDetails.Participants.Where(
                    matchDetailParticipant =>
                        match.RedTeam.Participants.Select(
                            localMatchParticipant => localMatchParticipant.Summoner.Name.ToLower())
                            .Contains(
                                matchDetails.ParticipantIdentities.Single(
                                    identity => identity.ParticipantId == matchDetailParticipant.ParticipantId)
                                    .Player.SummonerName.ToLower())).ToList();

                if (supposedToBeRedPlayers.Count == 0)
                {
                    Log("supposedToBeRedPlayers.Count == 0");

                    // Teams did not have correct summoners playing the match. Match invalid.
                    match.Invalid = true;
                    match.InvalidReason = "INCORRECT_SUMMONER_COUNT_RED_TEAM";

                    // Save to database
                    Mongo.Matches.Save(match);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                
                // Set the team id to the side they actually played
                var redTeamId = supposedToBeRedPlayers.First().TeamId;

                // Set statistics
                match.Duration = matchDetails.MatchDuration;
                match.CreationTime = matchDetails.MatchCreation;
                match.RiotMatchId = matchDetails.MatchId;
                
                match.ChampionIds = matchDetails.Participants.Select(x => x.ChampionId).ToArray();

                // Exclude null bans for blind pick and for teams that forgot all their bans
                match.BanIds = matchDetails.Teams.Where(x => x.Bans != null).SelectMany(x => x.Bans).Select(x => x.ChampionId).ToArray();

                match.AssistsBlueTeam = matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Assists);
                match.KillsBlueTeam = matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Kills);
                match.DeathsBlueTeam = matchDetails.Participants.Where(x => x.TeamId == blueTeamId).Sum(x => x.Stats.Deaths);

                match.AssistsRedTeam = matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Assists);
                match.KillsRedTeam = matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Kills);
                match.DeathsRedTeam = matchDetails.Participants.Where(x => x.TeamId == redTeamId).Sum(x => x.Stats.Deaths);

                match.Invalid = false;

                Log("set all stats");

                if (blueTeamId == 200)
                    match.PlayedWrongSide = true;

                // Save to database
                Mongo.Matches.Save(match);

                Log("executing NewMatch hook");
                // Call the new match hook
                BracketHelper.NewMatch(match);
                Log("========================done========================");
            }
            else
            {
                Log("incorrect summoners");

                // Teams did not have correct summoners playing the match. Match invalid.
                match.Invalid = true;
                match.InvalidReason = "INCORRECT_SUMMONERS";

                // Save to database
                Mongo.Matches.Save(match);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
