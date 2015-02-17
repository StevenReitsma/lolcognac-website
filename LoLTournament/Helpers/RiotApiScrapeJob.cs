using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using MongoDB.Driver;
using RiotSharp;
using RiotSharp.LeagueEndpoint;
using RiotSharp.StatsEndpoint;
using RiotSharp.SummonerEndpoint;
using Participant = LoLTournament.Models.Participant;
using Season = RiotSharp.StatsEndpoint.Season;
using System.Diagnostics;
using System.Globalization;

namespace LoLTournament.Helpers
{
    public class RiotApiScrapeJob
    {
        private readonly RiotApi _api;
        private static DateTime tournamentStart;

        public RiotApiScrapeJob()
        {
            var key = WebConfigurationManager.AppSettings["RiotApiKey"];
            _api = RiotApi.GetInstance(key, 3000, 180000);

            var timeSetting = WebConfigurationManager.AppSettings["TournamentStart"];

            tournamentStart = DateTime.ParseExact(timeSetting, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);


            // For summoner statistics, always 1 hour
            var intervalSummoners = new TimeSpan(1, 0, 0);
            var intervalMatches = new TimeSpan(1, 0, 0);

            //new Timer(ScrapeSummoners, null, new TimeSpan(0, 0, 0, 0, 0), intervalSummoners);
            //new Timer(ScrapeMatches, null, new TimeSpan(0, 0, 0, 0, 0), intervalMatches);
        }

        private void ScrapeMatches(object arg)
        {
            Debug.WriteLine("Scraping Matches");

            Debug.WriteLine(DateTime.Now.ToString());
            Debug.WriteLine(tournamentStart);

            



            // TODO
            // 0 Check if event has started
            // 1   Get list of team captains
            // 2   Get match history for each team captain
            // 3   Check if a match has finished in the last 5 minutes
            // 4   Check if the match is against the correct opponent according to the schedule
            // 5   Enter score in database

/*            var teamCaptains = new List<Participant>();

            foreach (var tc in teamCaptains)
            {
                var matchHistory = tc.Summoner.GetMatchHistory(0, 9);
                var matchesFinishedInLastFiveMinutes = from m in matchHistory
                    where DateTime.Now - m.MatchCreation < new TimeSpan(0, 0, 5, 0)
                    select m;

                // TODO Get match that team is supposed to be playing
                Match matchShouldBePlaying = null;

                var participantsShould = matchShouldBePlaying.Participants.Select(p => p.Summoner.Id);
                var match =
                    matchesFinishedInLastFiveMinutes.SingleOrDefault(
                        m => m.Participants.All(n => participantsShould.Contains(n.ParticipantId)));

                if (match == null)
                    continue;

                // Match is finished! TODO Add to DB
                var matchObject = new Match(); // TODO add parameters
            }*/
        }

        private bool IsAfterTournamentStart(Models.Match match)
        {
            //TODO Determine match finish (match.FinishTime?), see if it is after tournamentStart static var
            Debug.WriteLine(match.FinishTime.ToString());
            
            return false;
        }

        private void ScrapeSummoners(object arg)
        {
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Participant>("Participants");

            var participants = col.FindAll();

            // Loop through participants and retrieve their summoner and league information
            foreach (var p in participants)
            {
                // Get summoner information
                Summoner s;
                try
                {
                    s = _api.GetSummoner(Region.euw, p.SummonerName);
                }
                catch (Exception)
                {
                    // Summoner does not exist or Riot API offline
                    continue;
                }

                // Get Season 4 league
                var season4Tier = Tier.Unranked;
                try
                {
                    var previousSeason = _api.GetMatchHistory(Region.euw, s.Id, 0, 1, null, new List<Queue> {Queue.RankedSolo5x5});

                    if (previousSeason != null)
                        season4Tier = previousSeason[0].Participants[0].HighestAchievedSeasonTier;
                }
                catch (Exception)
                {
                    // Unranked in season 4
                }

                // Get wins and losses in season 4
                int wins;
                int losses;
                try
                {
                    var winLoss = _api.GetStatsSummaries(Region.euw, s.Id, Season.Season4);
                    var winLossSoloQueue = winLoss.Single(x => x.PlayerStatSummaryType == PlayerStatsSummaryType.RankedSolo5x5);
                    wins = winLossSoloQueue.Wins;
                    losses = winLossSoloQueue.Losses;
                }
                catch (Exception)
                {
                    // No matches played in season 4
                    wins = 0;
                    losses = 0;
                }

                // Get Season 5 league
                Tier tier;
                int divisionInt;
                try
                {
                    var season5League = _api.GetLeagues(Region.euw, new List<int> {(int) s.Id});
                    var season5LeagueSoloQueue = season5League.First().Value.Single(x => x.Queue == Queue.RankedSolo5x5);
                    tier = season5LeagueSoloQueue.Tier;
                    var division =
                        season5LeagueSoloQueue.Entries.Single(x => x.PlayerOrTeamId == s.Id.ToString()).Division;

                    switch (division)
                    {
                        case "I":
                            divisionInt = 1;
                            break;
                        case "II":
                            divisionInt = 2;
                            break;
                        case "III":
                            divisionInt = 3;
                            break;
                        case "IV":
                            divisionInt = 4;
                            break;
                        case "V":
                            divisionInt = 5;
                            break;
                        default:
                            divisionInt = 0;
                            break;
                    }
                }
                catch (Exception)
                {
                    // Unranked in season 5
                    tier = Tier.Unranked;
                    divisionInt = 0;
                }

                // Set values and update DB
                p.Summoner = s;
                p.Season4Wins = wins;
                p.Season4Losses = losses;
                p.Season4Tier = season4Tier;
                p.Season5Tier = tier;
                p.Season5Division = divisionInt;
                p.LastUpdateTime = DateTime.Now;

                col.Save(p);
            }
        }
    }
}