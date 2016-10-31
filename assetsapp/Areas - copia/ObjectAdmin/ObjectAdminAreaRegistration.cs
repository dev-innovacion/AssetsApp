using System.Web.Mvc;

namespace RivkaAreas.ObjectAdmin
{
    public class ObjectAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ObjectAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ObjectAdmin_default",
                "ObjectAdmin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
