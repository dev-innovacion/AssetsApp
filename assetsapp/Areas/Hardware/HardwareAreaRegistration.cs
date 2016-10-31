using System.Web.Mvc;

namespace RivkaAreas.Hardware
{
    public class HardwareAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Hardware";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Hardware_default",
                "Hardware/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
