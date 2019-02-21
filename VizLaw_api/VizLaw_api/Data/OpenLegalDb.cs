using System;
using System.Collections.Generic;
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


        static JsonValue getApiCall(string urlParameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            JsonValue dataObjects = null;
            //string accessToken = "9ac38da25c98106b3a6f056614b09ea738a02928";
            client.DefaultRequestHeaders.Add("Authorization", "Token 9ac38da25c98106b3a6f056614b09ea738a02928");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", accessToken);
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
                //9ac38da25c98106b3a6f056614b09ea738a02928


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

        public static CourtDecision getCourtDecision(string DecisionId)
        {
            CourtDecision result = new CourtDecision();


            JsonValue decision = getApiCall("cases/" +  DecisionId);
            if(decision == null)
            {
                result.id = "error";
                result.slug = "error - api request to openlegaldata failed";
                return result;
            }
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