using System;
using System.Collections.Generic;
using System.Data;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using VizLaw_api.DataAccess;

namespace VizLaw_api.Data
{
    public static class OpenLegalDb
    {
        private const string URL = "https://de.openlegaldata.io/api/";

        private static List<CourtDecision> courtDecisions = null;


        static JsonValue getApiCall(string urlParameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            JsonValue dataObjects = null; ;

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                using (HttpContent content = response.Content)
                {
                    // ... Read the string.
                    Task<string> result = content.ReadAsStringAsync();
                    dataObjects = JsonValue.Parse(result.Result);
                }
                
                

            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            //Make any other calls using HttpClient here.

            //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();
            return dataObjects;
        }

        private static List<CourtDecision> getDecisions()
        {
            if(courtDecisions == null)
            {
                //Load Data from Source File and stor it in sourceData 
                DataTable sourceData = new DataAccess.CsvHelper.CsvReader(AppDomain.CurrentDomain.BaseDirectory + @"\Data\CourtData.csv") { cSeperator = '|', cDelimiter = '"', HasHeaderRow = false }.ReadIntoDataTable();

                //Initialize CitationList
                courtDecisions = new List<CourtDecision>();

                //Load Citations to List
                foreach (DataRow row in sourceData.Rows)
                {
                    CourtDecision dec = new CourtDecision();
                    dec.id = row[0].ToString();
                    dec.file_number = row[1].ToString();
                    dec.date = row[2].ToString();
                    dec.court = new Court();
                    dec.court.level_of_appeal = row[3].ToString();

                    courtDecisions.Add(dec);
                }
            }

            return courtDecisions;
        }

        public static CourtDecision getCourtDecision(string DecisionId)
        {
            try
            {
                return getDecisions().Where(d => d.id == DecisionId).First();
            }
            catch(Exception ex)
            {
                return new CourtDecision() { id = DecisionId, file_number = "12 U 123/45", date = "2016-01-01", court = new Court() { id = "0", level_of_appeal = "" } } ;
            }

            CourtDecision result = new CourtDecision();

            JsonValue decision = getApiCall("cases/" +  DecisionId);

            result.id = decision["id"].ToString();
            result.slug = decision["slug"].ToString();
            result.file_number = decision["file_number"];
            result.date = decision["date"];
            result.create_date = decision["created_date"];
            result.update_date = decision["updated_date"];
            result.type = decision["type"];

            result.court = new Court();
            result.court.id = decision["court"]["id"].ToString();
            result.court.name = decision["court"]["name"].ToString();
            result.court.slug = decision["court"]["slug"];
            result.court.city = decision["court"]["city"] != null ? decision["court"]["city"].ToString() : "";
            result.court.state = decision["court"]["state"].ToString();
            result.court.jurisdiction = decision["court"]["jurisdiction"];
            result.court.level_of_appeal = decision["court"]["level_of_appeal"];

            return result;
        }
    }
}