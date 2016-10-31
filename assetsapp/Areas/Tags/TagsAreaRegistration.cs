using System.Web.Mvc;

namespace RivkaAreas.Tags
{
    public class TagsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Tags";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Tags_default",
                "Tags/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
