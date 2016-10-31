using System.Web.Mvc;

namespace RivkaAreas.Assignments
{
    public class AssignmentsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Assignments";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Assignments_default",
                "Assignments/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
