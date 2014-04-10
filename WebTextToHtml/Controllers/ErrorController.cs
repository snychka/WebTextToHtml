using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTextToHtml.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View("Error");
        }
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        public ActionResult Internal()
        {
            Response.StatusCode = 500;
            return View("Internal");
        }
    }
}
