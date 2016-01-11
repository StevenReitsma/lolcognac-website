using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LoLTournament.Helpers;
using LoLTournament.Models.Financial;

namespace LoLTournament.Models.Admin
{
    public class AdminFinanceViewModel
    {
        public IOrderedEnumerable<Payment> Payments
        {
            get { return Mongo.Payments.FindAll().OrderBy(x => x.CreatedDateTime); }
        }
    }
}
