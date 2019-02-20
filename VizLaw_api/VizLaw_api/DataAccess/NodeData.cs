using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VizLaw_api.DataAccess
{
    public class NodeData
    {
        public List<Node> Nodes;

        public List<Edge> Edges;

        public NodeData()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

    }
}