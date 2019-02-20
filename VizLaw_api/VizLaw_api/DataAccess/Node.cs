namespace VizLaw_api.DataAccess
{
    public class Node
    {

        public string NodeId { get ; set; }
        public string Name { get; set; }

        public string Date { get; set; }

        public string Court { get; set; }

        public Node(string nodid, string name, string date, string court)
        {
            NodeId = nodid;
            Name = name;
            Date = date;
            Court = court;
        }

    }
}