using System.Web.Mvc;

namespace RivkaAreas.Processes
{
    public class ProcessesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Processes";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Processes_default",
                "Processes/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
