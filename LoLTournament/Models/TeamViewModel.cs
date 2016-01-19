using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp.TeamEndpoint;
using LoLTournament.Helpers;

namespace LoLTournament.Models
{
    public class TeamViewModel
    {
        public Team Team { get; set; }
        public Match NextMatch { get; set; }
        public List<Match> MatchHistory { get; set; }
        public bool OtherTeamReady { get; set; }
        public bool OtherTeamDefined { get; set; }
        public double AvgKills { get; set; }
        public double AvgAssists { get; set; }
        public double AvgDeaths { get; set; }
        public double WinPercentage { get; set; }
        public TimeSpan AvgPlayTime { get; set; }

        public TeamViewModel()
        {
            OtherTeamDefined = true;
        }

        public TeamViewModel(ObjectId teamId) : this()
        {
            // Initialize team
            Team = Mongo.Teams.Find(Query<Team>.Where(x => x.Id == teamId && !x.Cancelled)).SingleOrDefault();

            if (Team == null)
                return;

            // Initialize next match
            NextMatch = Team.GetNextMatch();

            if (NextMatch == null && Team.OnHold)
                OtherTeamDefined = false;

            // Initialize match history
            MatchHistory =
                Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && (x.BlueTeamId == Team.Id || x.RedTeamId == Team.Id))).OrderByDescending(x => x.CreationTime)
                    .ToList();

            // Initialize statistics
            AvgKills = Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.KillsBlueTeam) + Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.RedTeamId == Team.Id)).Sum(x => x.KillsRedTeam);

            if (MatchHistory.Count > 0)
                AvgKills /= MatchHistory.Count;
            else
                AvgKills = 0;

            AvgDeaths = Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.DeathsBlueTeam) + Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.RedTeamId == Team.Id)).Sum(x => x.DeathsRedTeam);

            if (MatchHistory.Count > 0)
                AvgDeaths /= MatchHistory.Count;
            else
                AvgDeaths = 0;

            AvgAssists = Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.AssistsBlueTeam) + Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.RedTeamId == Team.Id)).Sum(x => x.AssistsRedTeam);

            if (MatchHistory.Count > 0)
                AvgAssists /= MatchHistory.Count;
            else
                AvgAssists = 0;

            var losses = Team.Losses;
            if (losses == 0)
                WinPercentage = 100;
            else
                WinPercentage = Team.Wins / ((double)Team.Losses + Team.Wins) * 100;

            AvgPlayTime = Team.TotalPlayTime;

            if (MatchHistory.Count > 0)
                AvgPlayTime = TimeSpan.FromSeconds(AvgPlayTime.TotalSeconds / MatchHistory.Count);
            else
                AvgPlayTime = new TimeSpan(0, 0, 0);

            // Check if other team is ready
            if (NextMatch != null)
            {
                var otherId = NextMatch.BlueTeamId == Team.Id ? NextMatch.RedTeamId : NextMatch.BlueTeamId;
                var otherTeam = Mongo.Teams.Find(Query<Team>.Where(x => x.Id == otherId)).SingleOrDefault();
                if (otherTeam == null)
                {
                    OtherTeamDefined = false;
                    return;
                }

                var otherTeamNextMatch = otherTeam.GetNextMatch();
                OtherTeamReady = otherTeamNextMatch.BlueTeamId == NextMatch.BlueTeamId &&
                                 otherTeamNextMatch.RedTeamId == NextMatch.RedTeamId;
            }
        }
    }
}
