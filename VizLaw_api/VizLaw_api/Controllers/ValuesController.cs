using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VizLaw_api.DataAccess;

namespace VizLaw_api.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        
        public IEnumerable<Citation> Get()
        {
            if(CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();
            
            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            return CitationNetwork.Citations.Where(c=>c.to_type != "Law").Take(20);
        }

        // GET api/values/5
        public IEnumerable<Citation> Get(int id)
        {
            if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();

            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
