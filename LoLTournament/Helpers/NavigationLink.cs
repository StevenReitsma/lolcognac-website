using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace LoLTournament.Helpers
{
    public static class NavigationExtension
    {
        public static string NavigationLink(
            this HtmlHelper html,
            string actionName,
            string controllerName)
        {
            string contextAction = (string) html.ViewContext.RouteData.Values["action"];
            string contextController = (string) html.ViewContext.RouteData.Values["controller"];

            bool isCurrent =
                string.Equals(contextAction, actionName, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(contextController, controllerName, StringComparison.CurrentCultureIgnoreCase);

            return isCurrent ? "active" : "";
        }
    }
}