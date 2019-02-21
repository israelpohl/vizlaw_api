using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VizLaw_api.DataAccess
{
    public class CourtDecision
    {
        public Court court { get; set; }

        public string id { get; set; }
        public string slug { get; set; }
        public string file_number { get; set; }
        public string date { get; set; }
        public string create_date { get; set; }
        public string update_date { get; set; }
        public string type { get; set; }
        public string exil { get; set; }
        public string content { get; set; }

        public CourtDecision()
        {

        }
    }
}