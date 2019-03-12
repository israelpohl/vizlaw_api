﻿using System;
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
        public nodeData Get(int id)
        {
            if (CitationNetwork.Citations == null || CitationNetwork.Citations.Count() == 0)
                CitationNetwork.LoadCitationNetwork();

            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            return CitationNetwork.getNodeData(id.ToString());
        }


        [Route("api/values/search/{text}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public IEnumerable<CourtDecision> Search(string text)
        {


            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            return OpenLegalDb.searchDecisions(text);
        }

        [Route("api/values/decision/{DecisionId}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public CourtDecision GetDecision(string DecisionId)
        {


            //int count = CitationNetwork.Citations.Where(ct =>ct.from_type != "Law" && ct.to_type != "Law").Count();

            //return CitationNetwork.Citations.Where(c => c.to_id==id.ToString());
            return OpenLegalDb.getCourtDecision(DecisionId);
        }


    }
}
