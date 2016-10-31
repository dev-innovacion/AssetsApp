using System.Web.Mvc;

namespace RivkaAreas.LogBook
{
    public class LogBookAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "LogBook";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "LogBook_default",
                "LogBook/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
