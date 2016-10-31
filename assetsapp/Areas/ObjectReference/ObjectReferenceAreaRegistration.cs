using System.Web.Mvc;

namespace RivkaAreas.ObjectReference
{
    public class ObjectReferenceAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ObjectReference";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ObjectReference_default",
                "ObjectReference/{controller}/{action}/{id}",
                new { controller = "Object", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
