namespace VizLaw_api.DataAccess
{
    public class Edge
    {
        public string sourceId { get; set; }

        public string targetId { get; set; }

        public Edge(string source, string target)
        {
            sourceId = source;
            targetId = target;
        }
    }
}