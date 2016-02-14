using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LoLTournament.Helpers;
using LoLTournament.Models;
using MongoDB.Driver.Builders;

namespace LoLTournament
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var scrape = new RiotApiScrapeJob();
#if !DEBUG
            scrape.StartTimer();
#endif

            //TournamentCodeFactory.GetTournamentCode(8551);

            //ExportHelper.ExportBadgeList();
            //ExportHelper.ExportEntryList();
            //BracketHelper.CreatePoolStructure();
            //BracketHelper.CreateFinaleStructure();

            //Remove and JsonValueProviderFactory and add JsonDotNetValueProviderFactory
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new JsonDotNetValueProviderFactory());
        }
    }
}
