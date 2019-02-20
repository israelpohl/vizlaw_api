using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace VizLaw_api.DataAccess
{
    public static class CitationNetwork
    {
        private static DataTable sourceData;

        //List aof all available Citations
        public static List<Citation> Citations;

        public static void LoadCitationNetwork()
        {
            //Load Data from Source File and stor it in sourceData 
            sourceData = new CsvHelper.CsvReader(AppDomain.CurrentDomain.BaseDirectory + @"\Data\refs.csv") { cSeperator = ',' , cDelimiter='"', HasHeaderRow=true}.ReadIntoDataTable();

            //Initialize CitationList
            Citations = new List<Citation>();

            //Load Citations to List
            foreach(DataRow row in sourceData.Rows)
            {
                Citations.Add(new Citation(row));
            }
        }

        public static NodeData getNodeData(string nodeId)
        {
            NodeData result = new NodeData();

            foreach(Citation cit in Citations.Where(c => c.from_id == nodeId))
            {
                //add only if currently not in List
                if(result.Nodes.Count(n => n.NodeId == cit.to_id) == 0)
                    result.Nodes.Add(new Node(cit.to_id, cit.from_case_file_number, cit.from_case_date, cit.to_case_court_jurisdiction));

                result.Edges.Add(new Edge(cit.from_id, cit.to_id));
            }

            foreach (Citation cit in Citations.Where(c => c.to_id == nodeId))
            {
                //add only if currently not in List
                if (result.Nodes.Count(n => n.NodeId == cit.from_id) == 0)
                    result.Nodes.Add(new Node(cit.from_id, cit.from_case_file_number, cit.from_case_date, cit.to_case_court_jurisdiction));

                result.Edges.Add(new Edge(cit.from_id, cit.to_id));
            }

            //edges hinzufügen



            return result;
        }
    }
}