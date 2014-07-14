using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTextToHtml.Models;

namespace WebTextToHtml.Controllers
{
    public class ListContentsController : Controller
    {
        //
        // GET: /ListContents/

        Text2HtmlDb context = new Text2HtmlDb();

        public ActionResult Index()
        {
            var model = this.context.Files.ToList();
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.context != null)
            {
                this.context.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
