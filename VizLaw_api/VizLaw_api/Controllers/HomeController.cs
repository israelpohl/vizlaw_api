using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VizLaw_api.DataAccess;

namespace VizLaw_api.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            //CitationNetwork net = new CitationNetwork();

            return View();
        }
    }
}
