using System;
using System.Collections.Generic;
using System.Data;
using System.Json;
using System.Linq;
using System.Web;
using VizLaw_api.Data;

namespace VizLaw_api.DataAccess
{
    public class CourtDecision
    {
        private SqlConnector con;
        private int citated;
        private List<Citation> citations;

        public Court court { get; set; }

        public string id { get; set; }
        public string slug { get; set; }
        public string file_number { get; set; }
        public string date { get; set; }
        public string create_date { get; set; }
        public string update_date { get; set; }
        public string type { get; set; }
        public string exil { get; set; }
        public string content { get; set; }

        public CourtDecision()
        {

        }

        public CourtDecision(int Id, SqlConnector connection)
        {
            try
            {
                con = connection;
                DataTable result = con.GetSqlAsDataTable("SELECT d.id, slug, type, file_number, CONVERT(nvarchar(max), create_date, 110) as create_date, CONVERT(nvarchar(max), date, 110) as date, content, court_id, (SELECT COUNT(*) FROM dbo.citations ct WHERE ct.to_id = d.id) countCitated , c.chamber court_chamber, c.city court_city, c.jurisdiction court_jurisdiction, c.level_of_appeal court_level_of_appeal, c.name court_name, c.state court_state FROM dbo.courtdecisions d  LEFT JOIN dbo.courts c on c.id = d.court_id WHERE d.id =" + Id);

                foreach (DataRow row in result.Rows)
                {
                    id = row["id"].ToString();
                    slug = row["slug"].ToString();
                    file_number = row["file_number"].ToString();
                    date = row["date"].ToString();
                    create_date = row["create_date"].ToString();
                    update_date = "";
                    type = row["type"].ToString();
                    exil = "";
                    content = row["content"].ToString();
                    citated = (int) row["countCitated"] ;

                    if (row["court_id"] != null && row["court_id"].ToString().Length > 0)
                    {
                        Court c = new Court();
                        c.id = row["court_id"].ToString();
                        c.jurisdiction = row["court_jurisdiction"].ToString();
                        c.level_of_appeal = row["court_level_of_appeal"].ToString();
                        c.name = row["court_name"].ToString();
                        c.slug = "";
                        c.state = row["court_state"].ToString();

                        court = c;
                    }
                }

                
                citations = new List<Citation>();

                foreach (DataRow row in con.GetSqlAsDataTable("SELECT DISTINCT ct.*, c.chamber from_case_court_chamber, c.city from_case_court_city, c.jurisdiction from_case_court_jurisdiction, c.level_of_appeal from_case_court_level_of_appeal, c.name from_case_court_name, c.state from_case_court_state FROM dbo.citations ct LEFT JOIN dbo.courts c ON c.id = ct.from_case_court_id where from_id = " + id + " OR to_id = " + id).Rows)
                {
                    citations.Add(new Citation(row));
                }
            }
            catch(Exception ex)
            {
                string exc = ex.ToString();
            }

        }

        public List<Citation> getCitations()
        {
            return citations;
        }

        public int getCitatedCount()
        {
            return citated;
        }

        public CourtDecision(JsonValue v)
        {
            id = v["id"].ToString();
            slug = v["slug"];
            type = v["type"];
            file_number = v["file_number"];
            create_date = v["created_date"];
            date = v["date"];
            content = v["content"];
            if (v["court"] != null && v["court"]["id"] != null)
            {
                court = new Court();
                court.id = v["court"]["id"].ToString();
                court.jurisdiction = v["court"]["jurisdiction"];
                court.level_of_appeal = v["court"]["level_of_appeal"];
                court.name = v["court"]["name"];
                court.slug = v["court"]["slug"];
                court.state = v["court"]["state"] != null ? v["court"]["state"].ToString() : null;

            }
        }

        public void UpdateToDatabase(SqlConnector con)
        {
            if (con.GetSqlAsInt($"SELECT COUNT(*) FROM dbo.courtdecisions WHERE id = {id}") == 0)
            {
                //INSERT
                string sql = "";

                if (court != null && court.id.Length > 0 && con.GetSqlAsInt($"SELECT COUNT(*) FROM dbo.courts WHERE id = {court.id}") == 0)
                {
                    sql = $@"INSERT INTO dbo.courts(
                                id,
                                chamber,
                                city,
                                jurisdiction,
                                level_of_appeal,
                                name,
                                state
                                ) VALUES (
                                '{court.id.Replace("'", "''")}',
                                '',
                                '{(court.city == null ? string.Empty : court.city.Replace("'", "''"))}',
                                '{(court.jurisdiction == null ? string.Empty : court.jurisdiction.Replace("'", "''"))}',
                                '{(court.level_of_appeal == null ? string.Empty : court.level_of_appeal.Replace("'", "''"))}',
                                '{(court.name == null ? string.Empty : court.name.Replace("'", "''"))}',
                                '{(court.state == null ? string.Empty : court.state.Replace("'", "''"))}'
                                )";
                    con.ExecuteSql(sql);
                }

                sql = $@"INSERT INTO dbo.courtdecisions(
                            id,
                            slug,
                            type,
                            file_number,
                            create_date,
                            date,
                            content,
                            court_id
                            ) VALUES (
                            '{id}',
                            '{slug.Replace("'", "''")}',
                            '{type.Replace("'", "''")}',
                            '{file_number.Replace("'", "''")}',
                            '{create_date}',
                            '{date}',
                            '{content.Replace("'", "''")}',
                            {(court != null && court.id.Length > 0 ? "'" + court.id + "'" : "NULL")}
                            )";

                con.ExecuteSql(sql);
            }
            else
            {
                //UPDATE ersteinmal nicht ggf. zuviele unnötige updates
            }
        }
    }
}