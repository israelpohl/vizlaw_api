using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using VizLaw_api.DataAccess;


namespace VizLaw_api.Data
{
    public static class OpenLegalDb
    {
        private const string URL = "https://de.openlegaldata.io/api/";

        private static List<CourtDecision> courtDecisions = null;

        private static SqlConnector con = null;


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
            if (con == null)
                con = new SqlConnector();
            //OpenLegalDb.reloadOpenLegalData();

            return new CourtDecision(Convert.ToInt32(DecisionId), con);
            
        }

        /// <summary>
        /// returns of courtdecisions wich containing string
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static List<CourtDecision> searchDecisions(string search)
        {
            List<CourtDecision> result = new List<CourtDecision>();

            try
            {

                if (con == null)
                    con = new SqlConnector();

                

                search = search.Replace("'", "''");

                //Alle Ergebnisse laden
                DataTable results = con.GetSqlAsDataTable($"SELECT TOP 50 d.id, slug, type, file_number, CONVERT(nvarchar(max), create_date, 110) as create_date, CONVERT(nvarchar(max), date, 110) as date, {(false ? "content" : "'' as content")}, court_id, (SELECT COUNT(*) FROM dbo.citations ct WHERE ct.to_id = d.id) countCitated , c.chamber court_chamber, c.city court_city, c.jurisdiction court_jurisdiction, c.level_of_appeal court_level_of_appeal, c.name court_name, c.state court_state FROM dbo.courtdecisions d  LEFT JOIN dbo.courts c on c.id = d.court_id WHERE contains(content, '\"{search}\"')  OR contains(file_number, '\"{search}\"')");

                foreach (DataRow row in results.Rows)
                {
                    result.Add(new CourtDecision(row));
                }

                string ids = string.Empty;

                foreach (CourtDecision c in result.Where(cd => !string.IsNullOrEmpty(cd.id)))
                {
                    ids += c.id + ", ";
                }

                //IDs für die Abfrage vorbereiten
                if (ids.EndsWith(", "))
                {
                    ids = ids.Substring(0, ids.Length - 2);
                }

                results = con.GetSqlAsDataTable("SELECT DISTINCT ct.*, c.chamber from_case_court_chamber, c.city from_case_court_city, c.jurisdiction from_case_court_jurisdiction, c.level_of_appeal from_case_court_level_of_appeal, c.name from_case_court_name, c.state from_case_court_state FROM dbo.citations ct LEFT JOIN dbo.courts c ON c.id = ct.from_case_court_id where from_id in (" + ids + ") OR to_id IN (" + ids + ")");

                foreach (DataRow row in results.Rows)
                {
                    foreach (CourtDecision d in result.Where(c => c.id == row["from_id"].ToString() || c.id == row["to_id"].ToString()))
                    {
                        d.AddCitation(new Citation(row));
                    }

                }
            }
            catch
            {
                result = new List<CourtDecision>();
            }



            return result;
        }

        public static void reloadOpenLegalData()
        {
            if (con == null)
                con = new SqlConnector();
            //Abrufen der Dateien vom WebServer
            //URL https://static.openlegaldata.io/dumps/de/
            List<string> availableFiles = GetDirectoryListingRegexForUrl("https://static.openlegaldata.io/dumps/de/");
            foreach (string file in availableFiles.Where(f => f.Contains("/refs.csv.gz")))
            {
                string gzFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\refs.csv.gz";
                new WebClient().DownloadFile(file, gzFile);
                OpenLegalDb.Decompress(new FileInfo(gzFile));
                UpdateCitationNetwork();
            }

            //INSERT OR UPDATE CourtDecisions
            foreach (string file in availableFiles.Where(f => f.Contains("_oldp_cases.json.gz")))
            {
                string gzFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\cases.json.gz";
                new WebClient().DownloadFile(file, gzFile);
                OpenLegalDb.Decompress(new FileInfo(gzFile));
                UpdateDecisions();
                File.Delete(gzFile);
            }

            //INSERT OR UPDATE Citations
        }

        private static void UpdateDecisions()
        {
            using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"Data\cases.json" , Encoding.UTF8))
            {
                string query = reader.ReadLine();
                int i = 0;
                while (!reader.EndOfStream)
                {

                    if (i % 500 == 0)
                        {
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + i + " rows checked");

                        }
                    JsonValue v = JsonValue.Parse(reader.ReadLine());
                    CourtDecision dec = new CourtDecision(v);
                    dec.UpdateToDatabase(con);
                    i++;
                }
            }
        }

        private static void UpdateCitationNetwork()
        {

            //Load Data from Source File and stor it in sourceData 
            DataAccess.CsvHelper.CsvReader csvFile = new DataAccess.CsvHelper.CsvReader(AppDomain.CurrentDomain.BaseDirectory + @"\Data\refs.csv") { cSeperator = ',', cDelimiter = '"', HasHeaderRow = true };
            DataTable sourceData = csvFile.ReadIntoDataTable();
           

            csvFile.Dispose();
            DataTable existingData = con.GetSqlAsDataTable("Select * FROM dbo.citations");
            //Load Citations to List

            int e = 0;
            sourceData.Columns.Add("COMPAREKEY");
            foreach (DataRow r in sourceData.Rows)
            {
                if (e % 50000 == 0)
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + e + " keys added (s)");
                }
                r["COMPAREKEY"] = r["from_id"] + "-" + r["to_id"];
                e++;
            }
            e = 0;
            existingData.Columns.Add("COMPAREKEY");
            foreach (DataRow r in existingData.Rows)
            {
                if (e % 50000 == 0)
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + e + " keys added (e)");
                }
                r["COMPAREKEY"] = r["from_id"] + "-" + r["to_id"];
                e++;
            }


            var set = new HashSet<string>(existingData.AsEnumerable().Select(r => (string)r["COMPAREKEY"]));
            List<DataRow> rowsToInsert = sourceData.AsEnumerable().Where(r => !set.Contains((string)r["COMPAREKEY"])).ToList();
            
            int i = 0;
            foreach (DataRow row in rowsToInsert)
            {
                if(i%500==0)
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + i + " rows checked");

                }
                if(!set.Contains((string)row["COMPAREKEY"]))
                {
                    Citation cit = new Citation(row);
                    cit.UpdateToDatabase(con);
                    set.Add((string)row["COMPAREKEY"]);
                }

                i++;
            }
        }

        /// <summary>
        /// Returns a List of available Files on Server
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<string> GetDirectoryListingRegexForUrl(string url)
        {
            //Url normieren
            if (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);

            List<string> result = new List<string>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex("a href=\".*\">(?<name>.*)</a>");
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                if(match.Groups["name"].Value.EndsWith("/") && !match.Groups["name"].Value.StartsWith("."))
                                {
                                    result.AddRange(GetDirectoryListingRegexForUrl(url + "/" + match.Groups["name"].Value));
                                }
                                else if(!match.Groups["name"].Value.StartsWith("."))
                                {
                                    result.Add(url + "/" + match.Groups["name"].Value);
                                }
                            }
                        }
                    }
                }
            }

            return result;

        }

        private static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }
    }
}