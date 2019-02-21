using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using VizLaw_api.DataAccess;

namespace VizLaw_api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SearchController : ApiController
    {
        // GET api/values
        
       

        // GET api/values/5
        public nodeData Get(string text)
        {
            if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();

            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            return CitationNetwork.getNodeData(text);
        }

        public nodeData Get(int id)
        {
            if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();

            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            return CitationNetwork.getNodeData(id.ToString());
        }


    }
}
