using System.Web.Mvc;

namespace RivkaAreas.Rule
{
    public class RuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Rule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Rule_default",
                "Rule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
