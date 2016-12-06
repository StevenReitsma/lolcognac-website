using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
// Leave this using statement
using LoLTournament.Helpers;

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

#if !DEBUG
            //var scrape = new RiotApiScrapeJob();
            //scrape.StartTimer();

            //var matchScrape = new MatchScraper();
            //matchScrape.StartTimer();
#endif


            //Remove and JsonValueProviderFactory and add JsonDotNetValueProviderFactory
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new JsonDotNetValueProviderFactory());
        }
    }
}
