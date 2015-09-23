using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RiotSharp;
using RiotSharp.GameEndpoint;
using RiotSharp.MatchEndpoint;
using Player = RiotSharp.GameEndpoint.Player;
using Team = LoLTournament.Models.Team;

namespace LoLTournament.Helpers
{
    public class RiotApiMocker
    {
        /// <summary>
        /// Simulates a finished match between two teams.
        /// </summary>
        /// <param name="teamId1"></param>
        /// <param name="teamId2"></param>
        /// <returns></returns>
        public static List<Game> MockRecentGames(string teamId1, string teamId2)
        {
            var team1 = Mongo.Teams.Find(Query<Team>.Where(x => x.Id == ObjectId.Parse(teamId1))).First();
            var team2 = Mongo.Teams.Find(Query<Team>.Where(x => x.Id == ObjectId.Parse(teamId2))).First();

            var participantFilter = team2.Participants.Where(x => !x.IsCaptain).ToList();

            var fellowPlayers = new List<Player>
            {
                new Player {SummonerId = team1.Participants[0].Summoner.Id, TeamId = 100},
                new Player {SummonerId = team1.Participants[1].Summoner.Id, TeamId = 100},
                new Player {SummonerId = team1.Participants[2].Summoner.Id, TeamId = 100},
                new Player {SummonerId = team1.Participants[3].Summoner.Id, TeamId = 100},
                new Player {SummonerId = team1.Participants[4].Summoner.Id, TeamId = 100},

                new Player {SummonerId = participantFilter[0].Summoner.Id, TeamId = 200},
                new Player {SummonerId = participantFilter[1].Summoner.Id, TeamId = 200},
                new Player {SummonerId = participantFilter[2].Summoner.Id, TeamId = 200},
                new Player {SummonerId = participantFilter[3].Summoner.Id, TeamId = 200},
            };

            var game = new Game
            {
                GameMode = GameMode.Classic,
                GameType = GameType.CustomGame,
                SubType = GameSubType.None,
                MapType = MapType.SummonersRiftCurrent,
                CreateDate = new DateTime(2015, 2, 26, 19, 02, 18),
                FellowPlayers = fellowPlayers,
                Invalid = false,
            };

            return new List<Game> {game};
        }

        /// <summary>
        /// Simulates match details. Blue side always wins.
        /// </summary>
        /// <param name="region">Unused.</param>
        /// <param name="gameId">Unused.</param>
        /// <returns></returns>
        public static MatchDetail MockMatch(Region region, long gameId)
        {
            var participants = new List<Participant>
            {
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = true, WardsPlaced = 10}, TeamId = 100},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = true, WardsPlaced = 10}, TeamId = 100},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = true, WardsPlaced = 10}, TeamId = 100},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = true, WardsPlaced = 10}, TeamId = 100},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = true, WardsPlaced = 10}, TeamId = 100},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = false, WardsPlaced = 10}, TeamId = 200},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = false, WardsPlaced = 10}, TeamId = 200},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = false, WardsPlaced = 10}, TeamId = 200},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = false, WardsPlaced = 10}, TeamId = 200},
                new Participant {Stats = new ParticipantStats {Assists = 10, Deaths = 3, Kills = 8, Winner = false, WardsPlaced = 10}, TeamId = 200}
            };


            return new MatchDetail
            {
                Participants = participants,
                MatchDuration = new TimeSpan(0, 32, 12),
                MatchCreation = new DateTime(2015, 2, 26, 19, 02, 18)
            };
        }
    }
}