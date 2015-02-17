using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models
{
    public class TeamViewModel
    {
        public Team Team { get; set; }
        public Match NextMatch { get; set; }
        public List<Match> MatchHistory { get; set; }
        public bool OtherTeamReady { get; set; }

        public TeamViewModel()
        {
        }

        public TeamViewModel(ObjectId teamId)
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
            NextMatch = GetNextMatch(matchCol, Team);

            // Initialize match history
            MatchHistory =
                matchCol.Find(Query<Match>.Where(x => x.Finished && (x.BlueTeamId == Team.Id || x.PurpleTeamId == Team.Id)))
                    .ToList();

            // Check if other team is ready
            if (NextMatch != null)
            {
                var otherId = NextMatch.BlueTeamId == Team.Id ? NextMatch.PurpleTeamId : NextMatch.BlueTeamId;
                var otherTeam = col.Find(Query<Team>.Where(x => x.Id == otherId)).SingleOrDefault();
                if (otherTeam == null)
                    return;
                var otherTeamNextMatch = GetNextMatch(matchCol, otherTeam);
                OtherTeamReady = otherTeamNextMatch.BlueTeamId == NextMatch.BlueTeamId &&
                                 otherTeamNextMatch.PurpleTeamId == NextMatch.PurpleTeamId;
            }
        }

        private static Match GetNextMatch(MongoCollection<Match> matchCol, Team team)
        {
            if (team.Phase == Phase.Pool)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Pool &&
                                (x.BlueTeamId == team.Id || x.PurpleTeamId == team.Id)))
                        .OrderBy(x => x.Priority)
                        .SingleOrDefault();
            }
            else if (team.Phase == Phase.Finale)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Finale &&
                                (x.BlueTeamId == team.Id || x.PurpleTeamId == team.Id)))
                        .OrderBy(x => x.Priority)
                        .SingleOrDefault();
            }
            else if (team.Phase == Phase.WinnerBracket)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.WinnerBracket &&
                                (x.BlueTeamId == team.Id || x.PurpleTeamId == team.Id))).SingleOrDefault();
            }
            else if (team.Phase == Phase.LoserBracket)
            {
                return
                    matchCol.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserBracket &&
                                (x.BlueTeamId == team.Id || x.PurpleTeamId == team.Id))).SingleOrDefault();
            }

            return null;
        }
    }
}
