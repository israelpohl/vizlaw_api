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


        public ActionResult GetDemoData()
        {
            if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();

            List<Citation> demoCitations = CitationNetwork.Citations.Where(c => c.to_type != "Law").Take(20).ToList();


            return PartialView("_CitationPartial", demoCitations);
        }
    }
}
