using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LoLTournament.Helpers;
using LoLTournament.Models.Financial;
using MongoDB.Driver.Builders;

namespace LoLTournament.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public long TeamCount
        {
            get { return Mongo.Teams.Count(Query<Team>.Where(x => !x.Cancelled)); }
        }

        public long Teambuilders
        {
            get { return Mongo.TeamBuilderParticipants.Count(); }
        }

        public decimal PaymentsReceived
        {
            get { return Mongo.Payments.Find(Query<Payment>.Where(x => x.Status == PaymentStatus.Paid || x.Status == PaymentStatus.Paidout)).Sum(x => x.Amount); }
        }

        public long PendingPayments
        {
            get { return Mongo.Payments.Count(Query<Payment>.Where(x => x.Status == PaymentStatus.Open || x.Status == PaymentStatus.Pending)); }
        }

        public long DoransNoRUCount
        {
            get
            {
                return Mongo.Participants.FindAll().Count(x => !x.Cancelled && x.Dorans && !x.RuStudent);
            }
        }

        public long NoDoransNoRUCount
        {
            get
            {
                return Mongo.Participants.FindAll().Count(x => !x.Cancelled && !x.Dorans && !x.RuStudent);
            }
        }

        public long CognACCount
        {
            get
            {
                return Mongo.Participants.FindAll().Count(x => !x.Cancelled && x.CognAC);
            }
        }
    }
}
