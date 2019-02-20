namespace VizLaw_api.DataAccess
{
    public class Node
    {

        public string nodeId { get ; set; }
        public string name { get; set; }

        public string date { get; set; }

        public string court { get; set; }
        public string numberCitations { get; set; }

        public Node(string Nodid, string Name, string Date, string Court, string numbercitations)
        {
            nodeId = Nodid;
            this.name = Name;
            this.date = Date;
            this.court = Court;
            numberCitations = numbercitations;
        }

    }
}