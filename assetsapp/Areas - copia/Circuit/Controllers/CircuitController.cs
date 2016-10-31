using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RivkaAreas.Circuit.Controllers
{
    [Authorize]
    public class CircuitController : Controller
    {
        //
        // GET: /Circuit/Circuit/

        public ActionResult Index()
        {
            return View();
        }

    }
}
