using System.Web.Mvc;

namespace RivkaAreas.Semaphore
{
    public class SemaphoreAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Semaphore";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Semaphore_default",
                "Semaphore/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
