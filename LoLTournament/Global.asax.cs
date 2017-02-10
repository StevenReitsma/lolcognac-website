using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LoLTournament.Helpers;

namespace LoLTournament
{
    public class MvcApplication : HttpApplication
    {
        // Keep reference to avoid GC
        private MatchScraper _matchScraper;
        private RiotApiScrapeJob _riotApiScrapeJob;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Test code for debugging (Joris + Steven summoner ID's)
            // var code = TournamentCodeFactory.GetTournamentCode(new List<long> {24689119, 26230426, 20893030, 20122308, 19308883, 39609774, 40776930, 21148960, 19977657, 20013452});

#if !DEBUG
            _riotApiScrapeJob = new RiotApiScrapeJob();
            _riotApiScrapeJob.StartTimer();

            _matchScraper = new MatchScraper();
            _matchScraper.StartTimer();
#endif


            //Remove and JsonValueProviderFactory and add JsonDotNetValueProviderFactory
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new JsonDotNetValueProviderFactory());
        }
    }
}
