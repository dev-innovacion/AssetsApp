using System.Web.Mvc;

namespace RivkaAreas.User
{
    public class UserAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "User_default",
                "User/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            //context.MapRoute(
            //    "Login",
            //    "Login",
            //    new { AreaName = "User", controller = "Login", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
