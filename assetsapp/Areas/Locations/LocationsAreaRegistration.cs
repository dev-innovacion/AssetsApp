using System.Web.Mvc;

namespace RivkaAreas.Locations
{
    public class LocationsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Locations";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Locations_default",
                "Locations/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
