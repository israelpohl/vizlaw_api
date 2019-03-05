using System;
using System.Collections.Generic;
using System.Data;

using System.Reflection;

using System.IO;


namespace VizLaw_api.Data
{
    

    public class SqlConnector : GenericConnector
    {
        IniReader iniFile
        {
            get
            {
                return new IniReader(AppDomain.CurrentDomain.BaseDirectory + @"Data\Settings.ini") { Section = "Database" };
            }
        }

        

        public override List<string> SqlHistoryLog { get; set; }

        public override string ConnectionString
        {
            get
            {
                string constr = @"Data Source={0}\{1};Initial Catalog={2};User Id={3};Password={4};Connection Timeout = 5;Trusted_Connection={5};";
                DBUser = Username;
                return string.Format(constr, Server, Instance, Database, Username, Password, TrustedConnection);
            }
        }

        public String Server
        {
            get { return iniFile.ReadString("tbServer"); }

            set { iniFile.Write("tbServer", value); }
        }

        public String Instance
        {
            get { return iniFile.ReadString("tbInstance"); }

            set { iniFile.Write("tbInstance", value); }
        }

        public String Database
        {
            get { return iniFile.ReadString("tbDatabase"); }

            set { iniFile.Write("tbDatabase", value); }
        }

        public String Username
        {
            get { return iniFile.ReadString("tbUsername"); }

            set { iniFile.Write("tbUsername", value); }
        }

        public String Password
        {
            get { return iniFile.ReadString("tbPassword"); }

            set { iniFile.Write("tbPassword", value); }
        }

        public bool TrustedConnection
        {
            get { return iniFile.ReadString("cbTrustedConnection") == "True"; }

            set { iniFile.Write("cbTrustedConnection", value.ToString()); }
        }

        public new List<String> Databases
        {

            get
            {
                if (Connection.State != ConnectionState.Open) Connection.Open();
                List<string> sources = new List<string>();
                foreach (DataRow item in Connection.GetSchema("Databases").Rows)
                    sources.Add(item[0].ToString());
                return sources;

            }
        }

        public SqlConnector()
        {
            
            Assembly EntryAssembly = System.Reflection.Assembly.GetEntryAssembly();

            //iniFile = new IniReader((new FileInfo(EntryAssembly.FullName)).Directory + "\\Settings.ini");
  
            //iniFile.Section = "Database";
        }

    }

   
}
