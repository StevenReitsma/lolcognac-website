using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using LoLTournament.Helpers;
using LoLTournament.Models.Financial;
using Microsoft.Ajax.Utilities;

namespace LoLTournament.Models
{
    public class Team
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public List<ObjectId> ParticipantsIds { get; set; }
        public int Pool { get; set; }
        public Phase Phase { get; set; }
        public int FinalRanking { get; set; }
        public bool OnHold { get; set; }

        /// <summary>
        /// Whether the registration has been cancelled.
        /// </summary>
        public bool Cancelled { get; set; }

        [BsonIgnore]
        public List<Participant> Participants
        {
            get
            {
                return Mongo.Participants.Find(Query<Participant>.Where(x => ParticipantsIds.Contains(x.Id))).ToList();
            }
        }

        public ObjectId CaptainId { get; set; }
        
        [BsonIgnore]
        public Participant Captain {
            get
            {
                return Mongo.Participants.Find(Query<Participant>.Where(x => x.TeamId == Id && x.IsCaptain)).First();
            }
        }

        [BsonIgnore]
        public double MMR
        {
            get { return Participants.Sum(x => x.MMR); }
        }

        [BsonIgnore]
        public double MMRUncertainty
        {
            get { return Participants.Sum(x => x.MMRUncertainty); }
        }

        [BsonIgnore]
        public int AmountOfRuStudents
        {
            get { return Participants.Count(x => x.RuStudent); }
        }

        [BsonIgnore]
        public long Wins
        {
            get
            {
                return Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId == Id)).Count();
            }
        }

        [BsonIgnore]
        public long Losses
        {
            get
            {
                return Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId != Id && (x.BlueTeamId == Id || x.RedTeamId == Id))).Count();
            }
        }

        [BsonIgnore]
        public TimeSpan TotalPlayTime
        {
            get
            {
                return
                    TimeSpan.FromSeconds(Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && (x.BlueTeamId == Id || x.RedTeamId == Id)))
                        .Sum(x => x.Duration.TotalSeconds));
            }
        }

        [BsonIgnore]
        public TimeSpan TotalWinsPlayTime
        {
            get
            {
                return
                    TimeSpan.FromSeconds(Mongo.Matches.Find(Query<Match>.Where(x => x.Finished && x.WinnerId == Id && (x.BlueTeamId == Id || x.RedTeamId == Id)))
                        .Sum(x => x.Duration.TotalSeconds));
            }
        }

        [BsonIgnore]
        public int PoolRank
        {
            get
            {
                return Mongo.Teams.Find(Query<Team>.Where(x => x.Pool == Pool && !x.Cancelled)).OrderByDescending(x => x.Wins).ThenBy(x => x.TotalWinsPlayTime).ToList().FindIndex(x => x.Id == Id);
            }
        }

        public Match GetNextMatch()
        {
            if (Phase == Phase.Pool)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Pool &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id)))
                        .OrderBy(x => x.Priority)
                        .FirstOrDefault();
            }
            if (Phase == Phase.Finale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.Finale &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id)))
                        .OrderBy(x => x.Priority)
                        .FirstOrDefault();
            }
            if (Phase == Phase.BronzeFinale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.BronzeFinale &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.WinnerBracket)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.WinnerBracket &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.LoserBracket)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserBracket &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id))).FirstOrDefault();
            }
            if (Phase == Phase.LoserFinale)
            {
                return
                    Mongo.Matches.Find(
                        Query<Match>.Where(
                            x =>
                                !x.Finished && x.Phase == Phase.LoserFinale &&
                                (x.BlueTeamId == Id || x.RedTeamId == Id))).FirstOrDefault();
            }

            return null;
        }

        [BsonIgnore]
        public int CognACCount
        {
            get
            {
                return Participants.Count(x => x.CognAC);
            }
        }

        [BsonIgnore]
        public int DoransCount
        {
            get
            {
                return Participants.Count(x => x.Dorans);
            }
        }

        /// <summary>
        /// The price this team has to pay.
        /// </summary>
        [BsonIgnore]
        public decimal Price
        {
            get
            {
                decimal price = 0m;

                foreach (var p in Participants)
                {
                    // RU + CognAC: 3.00 euro
                    if (p.RuStudent && p.CognAC)
                        price += 3.0m;
                    // CognAC: 4.50 euro
                    else if (!p.RuStudent && p.CognAC)
                        price += 4.5m;
                    // RU || Dorans: 5.00 euro
                    else if (p.RuStudent && !p.CognAC || p.Dorans && !p.CognAC)
                        price += 5.0m;
                    // None: 6.50 euro
                    else if (!p.RuStudent && !p.CognAC && !p.Dorans)
                        price += 6.5m;
                    // Baseline for oversights
                    else
                        price += 5.0m;
                }

                return price;
            }
        }

        [BsonIgnore]
        public Payment Payment
        {
            get { return Mongo.Payments.FindOne(Query<Payment>.Where(x => x.TeamId == Id)); }
        }

        public void Disqualify()
        {
            var match = GetNextMatch();
            while (match != null)
            {
                if (match.BlueTeamId == Id)
                    match.ForceRedWin();
                else
                    match.ForceBlueWin();

                match = GetNextMatch();
            }
        }
    }
}
