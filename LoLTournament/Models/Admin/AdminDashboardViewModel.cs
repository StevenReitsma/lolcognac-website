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
            get { return Mongo.Payments.FindAll().Sum(x => x.Amount); }
        }

        public long PendingPayments
        {
            get { return Mongo.Payments.Count(Query<Payment>.Where(x => x.Status == PaymentStatus.Open || x.Status == PaymentStatus.Pending)); }
        }
    }
}