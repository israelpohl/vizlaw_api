namespace VizLaw_api.DataAccess
{
    public class Edge
    {
        public string SourceId { get; set; }

        public string TargetId { get; set; }

        public Edge(string source, string target)
        {
            SourceId = source;
            TargetId = target;
        }
    }
}