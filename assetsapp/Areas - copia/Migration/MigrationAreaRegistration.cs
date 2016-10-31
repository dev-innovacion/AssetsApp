using System.Web.Mvc;

namespace RivkaAreas.Migration
{
    public class MigrationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Migration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Migration_default",
                "Migration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
