using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using VizLaw_api.Data;

namespace VizLaw_api.DataAccess
{
    //[Serializable]
    public class Citation
    {
        public string from_case_court_chamber { get; set; }
        public string from_case_court_city { get; set; }
        public string from_case_court_id { get; set; }
        public string from_case_court_jurisdiction { get; set; }
        public string from_case_court_level_of_appeal { get; set; }
        public string from_case_court_name { get; set; }
        public string from_case_court_state { get; set; }
        public string from_case_date { get; set; }
        public string from_case_file_number { get; set; }
        public string from_case_private { get; set; }
        public string from_case_source_name { get; set; }
        public string from_case_type { get; set; }
        public string from_id { get; set; }
        public string from_type { get; set; }
        public string to_case_court_jurisdiction { get; set; }
        public string to_case_court_level_of_appeal { get; set; }
        public string to_case_court_name { get; set; }
        public string to_id { get; set; }
        public string to_law_book_code { get; set; }
        public string to_law_section { get; set; }
        public string to_law_title { get; set; }
        public string to_type { get; set; } 

        public Citation(DataRow sourceDataRow)
        {
            from_case_court_chamber = sourceDataRow["from_case_court_chamber"].ToString();
            from_case_court_city  = sourceDataRow["from_case_court_city"].ToString();
            from_case_court_id  = sourceDataRow["from_case_court_id"].ToString();
            from_case_court_jurisdiction  = sourceDataRow["from_case_court_jurisdiction"].ToString();
            from_case_court_level_of_appeal  = sourceDataRow["from_case_court_level_of_appeal"].ToString();
            from_case_court_name  = sourceDataRow["from_case_court_name"].ToString();
            from_case_court_state  = sourceDataRow["from_case_court_state"].ToString();
            from_case_date  = sourceDataRow["from_case_date"].ToString();
            from_case_file_number  = sourceDataRow["from_case_file_number"].ToString();
            from_case_private  = sourceDataRow["from_case_private"].ToString();
            from_case_source_name  = sourceDataRow["from_case_source_name"].ToString();
            from_case_type  = sourceDataRow["from_case_type"].ToString();
            from_id  = sourceDataRow["from_id"].ToString();
            from_type  = sourceDataRow["from_type"].ToString();
            to_case_court_jurisdiction  = sourceDataRow["to_case_court_jurisdiction"].ToString();
            to_case_court_level_of_appeal  = sourceDataRow["to_case_court_level_of_appeal"].ToString();
            to_case_court_name  = sourceDataRow["to_case_court_name"].ToString();
            to_id  = sourceDataRow["to_id"].ToString();
            to_law_book_code  = sourceDataRow["to_law_book_code"].ToString();
            to_law_section  = sourceDataRow["to_law_section"].ToString();
            to_law_title  = sourceDataRow["to_law_title"].ToString();
            to_type  = sourceDataRow["to_type"].ToString();
        }

        public void UpdateToDatabase(SqlConnector con)
        {
            if (con.GetSqlAsInt($"SELECT COUNT(*) FROM dbo.citations WHERE from_id = {from_id} AND to_id={to_id}") == 0)
            {
                //INSERT
                string sql = $@"INSERT INTO dbo.courts(
                                id,
                                chamber,
                                city,
                                jurisdiction,
                                level_of_appeal,
                                name,
                                state
                                ) VALUES (
                                '{from_case_court_id.Replace("'", "''")}',
                                '{from_case_court_chamber.Replace("'", "''")}',
                                '{from_case_court_city.Replace("'", "''")}',
                                '{from_case_court_jurisdiction.Replace("'", "''")}',
                                '{from_case_court_level_of_appeal.Replace("'", "''")}',
                                '{from_case_court_name.Replace("'", "''")}',
                                '{from_case_court_state.Replace("'", "''")}'
                                )";

                if (from_case_court_id != null && from_case_court_id.Length > 0 && con.GetSqlAsInt($"SELECT COUNT(*) FROM dbo.courts WHERE id = {from_case_court_id}") == 0)
                {
                    con.ExecuteSql(sql);
                }

                sql = $@"INSERT INTO dbo.citations(
                                from_id,
                                to_id,
                                from_case_court_id,
                                from_case_date,
                                from_case_file_number,
                                from_case_private,
                                from_case_source_name,
                                from_case_type,
                                from_type,
                                to_case_court_jurisdiction,
                                to_case_court_level_of_appeal,
                                to_case_court_name,
                                to_law_book_code,
                                to_law_section,
                                to_law_title,
                                to_type
                                ) VALUES(
                                '{from_id.Replace("'", "''")}',
                                '{to_id.Replace("'", "''")}',
                                '{from_case_court_id.Replace("'", "''")}',
                                CONVERT(datetime, '{from_case_date.Replace("'", "''")}', 126),
                                '{from_case_file_number.Replace("'", "''")}',
                                '{from_case_private.Replace("'", "''")}',
                                '{from_case_source_name.Replace("'", "''")}',
                                '{from_case_type.Replace("'", "''")}',
                                '{from_type.Replace("'", "''")}',
                                '{to_case_court_jurisdiction.Replace("'", "''")}',
                                '{to_case_court_level_of_appeal.Replace("'", "''")}',
                                '{to_case_court_name.Replace("'", "''")}',
                                '{to_law_book_code.Replace("'", "''")}',
                                '{to_law_section.Replace("'", "''")}',
                                '{to_law_title.Replace("'", "''")}',
                                '{(from_type.ToUpper() == "LAW" ? "1" : "2")}'
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