using System.Web.Mvc;

namespace RivkaAreas.Movement
{
    public class MovementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Movement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Movement_default",
                "Movement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
