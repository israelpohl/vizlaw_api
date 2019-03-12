using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using VizLaw_api.Data;
using VizLaw_api.DataAccess;

namespace VizLaw_api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SearchController : ApiController
    {
        // GET api/values
        
       

        // GET api/values/5
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage searchDecisions(string searchQuery)
        {
            //if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
            //    CitationNetwork.LoadCitationNetwork();
            //82520

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            var response = Request.CreateResponse(HttpStatusCode.OK, OpenLegalDb.searchDecisions(searchQuery));
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;

        }
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public CourtDecision getCourtDecision(string DecisionId)
        {
            //if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
            //    CitationNetwork.LoadCitationNetwork();
            
            return OpenLegalDb.getCourtDecision(DecisionId);
        }

        [HttpGet]
        public string updateDatabase()
        {
            OpenLegalDb.reloadOpenLegalData();
            return "ok";
        }

    }
}
