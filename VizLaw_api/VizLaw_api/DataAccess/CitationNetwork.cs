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
    }
}