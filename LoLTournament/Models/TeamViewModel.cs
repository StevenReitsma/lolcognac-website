using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp.TeamEndpoint;

namespace LoLTournament.Models
{
    public class TeamViewModel
    {
        public Team Team { get; set; }
        public Match NextMatch { get; set; }
        public List<Match> MatchHistory { get; set; }
        public bool OtherTeamReady { get; set; }
        public bool OtherTeamDefined { get; set; }
        public long AvgKills { get; set; }
        public long AvgAssists { get; set; }
        public long AvgDeaths { get; set; }
        public double WinPercentage { get; set; }
        public TimeSpan AvgPlayTime { get; set; }

        public TeamViewModel()
        {
            OtherTeamDefined = true;
        }

        public TeamViewModel(ObjectId teamId) : this()
        {
            // Initialize team
            var client = new MongoClient();
            var server = client.GetServer();
            var db = server.GetDatabase("CLT");
            var col = db.GetCollection<Team>("Teams");

            Team = col.Find(Query<Team>.Where(x => x.Id == teamId)).SingleOrDefault();

            if (Team == null)
                return;

            // Initialize next match
            var matchCol = db.GetCollection<Match>("Matches");
            NextMatch = Team.GetNextMatch();

            if (NextMatch == null && Team.OnHold)
                OtherTeamDefined = false;

            // Initialize match history
            MatchHistory =
                matchCol.Find(Query<Match>.Where(x => x.Finished && (x.BlueTeamId == Team.Id || x.PurpleTeamId == Team.Id))).OrderByDescending(x => x.CreationTime)
                    .ToList();

            // Initialize statistics
            AvgKills = matchCol.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.KillsBlueTeam) + matchCol.Find(Query<Match>.Where(x => x.Finished && x.PurpleTeamId == Team.Id)).Sum(x => x.KillsPurpleTeam);

            if (MatchHistory.Count > 0)
                AvgKills /= MatchHistory.Count;
            else
                AvgKills = 0;

            AvgDeaths = matchCol.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.DeathsBlueTeam) + matchCol.Find(Query<Match>.Where(x => x.Finished && x.PurpleTeamId == Team.Id)).Sum(x => x.DeathsPurpleTeam);

            if (MatchHistory.Count > 0)
                AvgDeaths /= MatchHistory.Count;
            else
                AvgDeaths = 0;

            AvgAssists = matchCol.Find(Query<Match>.Where(x => x.Finished && x.BlueTeamId == Team.Id)).Sum(x => x.AssistsBlueTeam) + matchCol.Find(Query<Match>.Where(x => x.Finished && x.PurpleTeamId == Team.Id)).Sum(x => x.AssistsPurpleTeam);

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
                var otherId = NextMatch.BlueTeamId == Team.Id ? NextMatch.PurpleTeamId : NextMatch.BlueTeamId;
                var otherTeam = col.Find(Query<Team>.Where(x => x.Id == otherId)).SingleOrDefault();
                if (otherTeam == null)
                {
                    OtherTeamDefined = false;
                    return;
                }

                var otherTeamNextMatch = otherTeam.GetNextMatch();
                OtherTeamReady = otherTeamNextMatch.BlueTeamId == NextMatch.BlueTeamId &&
                                 otherTeamNextMatch.PurpleTeamId == NextMatch.PurpleTeamId;
            }
        }
    }
}
