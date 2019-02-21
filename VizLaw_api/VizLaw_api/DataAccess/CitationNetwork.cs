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

        public static nodeData getNodeData(string nodeId)
        {
            //Load data of origin decision
            CourtDecision sourceDecision = Data.OpenLegalDb.getCourtDecision(nodeId);

            nodeData result = new nodeData();

            result.nodes.Add(new Node(nodeId, sourceDecision.file_number , sourceDecision.date, sourceDecision.court.level_of_appeal, Citations.Count(c => c.to_id == nodeId).ToString()));


            foreach (Citation cit in Citations.Where(c => c.from_id == nodeId))
            {
                //add only if currently not in List
                if (result.nodes.Count(n => n.nodeId == cit.to_id || cit.from_case_file_number == cit.from_case_file_number) == 0)
                {

                    result.nodes.Add(new Node(cit.to_id, cit.from_case_file_number, cit.from_case_date, cit.from_case_court_level_of_appeal, Citations.Count(c => c.to_id == cit.to_id && c.to_type != "Law").ToString()));

                    if (result.edges.Count(e => e.sourceId == cit.from_id && e.targetId == cit.to_id) == 0)
                        result.edges.Add(new Edge(cit.from_id, cit.to_id));
                }
            
            }

            foreach (Citation cit in Citations.Where(c => c.to_id == nodeId))
            {
                //add only if currently not in List
                if (result.nodes.Count(n => n.nodeId == cit.from_id || cit.from_case_file_number == n.name ) == 0)
                {
                    result.nodes.Add(new Node(cit.from_id, cit.from_case_file_number, cit.from_case_date, cit.from_case_court_level_of_appeal, Citations.Count(c => c.to_id == cit.from_id).ToString()));

                    if (result.edges.Count(e => e.sourceId == cit.from_id && e.targetId == cit.to_id) == 0)
                        result.edges.Add(new Edge(cit.from_id, cit.to_id));
                }
                
            }

            //edges hinzufügen



            return result;
        }
    }
}