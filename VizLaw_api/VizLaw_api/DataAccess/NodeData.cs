using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VizLaw_api.DataAccess
{
    public class nodeData
    {
        public List<Node> nodes;

        public List<Edge> edges;

        public nodeData()
        {
            nodes = new List<Node>();
            edges = new List<Edge>();
        }

    }
}