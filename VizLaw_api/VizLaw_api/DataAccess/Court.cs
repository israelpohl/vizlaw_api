using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VizLaw_api.DataAccess
{
    public class Court
    {
        public string id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string jurisdiction { get; set; }
        public string level_of_appeal { get; set; }

        public Court()
        {

        }
    }
}