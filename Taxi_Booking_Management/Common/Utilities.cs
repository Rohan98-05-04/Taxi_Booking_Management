using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace Taxi_Booking_Management.Common
{
    public static class Utilities
    {
      

        public static string IsActive(this IHtmlHelper html,
                                  string control,
                                  string action)
        {

            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

  
            var returnActive = control == routeControl &&
                               action == routeAction;

            return returnActive ? "sideBarListColor" : "";
        }

    }
}
