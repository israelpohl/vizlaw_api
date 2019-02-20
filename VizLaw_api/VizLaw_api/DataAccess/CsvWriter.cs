using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace VizLaw_api.DataAccess.CsvHelper
{
    /// <summary>
    /// Class to write data to a csv file
    /// </summary>
    public sealed class CsvWriter : IDisposable
    {

        #region Members

        private StreamWriter _streamWriter;
        private bool _replaceCarriageReturnsAndLineFeedsFromFieldValues = true;
        private string _carriageReturnAndLineFeedReplacement = ",";
        private bool _writeColumnHeaders = true;

        #endregion Members

        #region Properties

        /// <summary>
        /// Gets or sets whether carriage returns and line feeds should be removed from 
        /// field values, the default is true 
        /// </summary>
        public bool ReplaceCarriageReturnsAndLineFeedsFromFieldValues
        {
            get
            {
                return _replaceCarriageReturnsAndLineFeedsFromFieldValues;
            }
            set
            {
                _replaceCarriageReturnsAndLineFeedsFromFieldValues = value;
            }
        }

        /// <summary>
        /// Gets or sets what the carriage return and line feed replacement characters should be
        /// </summary>
        public string CarriageReturnAndLineFeedReplacement
        {
            get
            {
                return _carriageReturnAndLineFeedReplacement;   
            }
            set
            {
                _carriageReturnAndLineFeedReplacement = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the column headers will be written to the csv file.
        /// </summary>
        public bool WriteColumnHeaders
        {
            get
            {
                return _writeColumnHeaders;
            }
            set
            {
                _writeColumnHeaders = value;
            }
        }

        /// <summary>
        /// Set the Delimiter between Fields. Default is ','
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Get or Set whether '"' should be always be used as text qualifier.
        /// </summary>
        public bool TextqualifierForced { get; set; }

        #endregion Properties

        public CsvWriter()
        {
            Delimiter = ",";
            TextqualifierForced = false;
        }

        #region Methods

        #region CsvFile write methods

        /// <summary>
        /// Writes csv content to a file
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="filePath">File path</param>
        public void WriteCsv(CsvFile csvFile, string filePath)
        {
            WriteCsv(csvFile, filePath, null);
        }

        /// <summary>
        /// Writes csv content to a file
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="filePath">File path</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(CsvFile csvFile, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (StreamWriter writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
            {
                WriteToStream(csvFile, writer);
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Writes csv content to a stream
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="stream">Stream</param>
        public void WriteCsv(CsvFile csvFile, Stream stream)
        {
            WriteCsv(csvFile, stream, null);
        }

        /// <summary>
        /// Writes csv content to a stream
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(CsvFile csvFile, Stream stream, Encoding encoding)
        {
            stream.Position = 0;
            _streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
            WriteToStream(csvFile, _streamWriter);
            _streamWriter.Flush();
            stream.Position = 0;
        }

        /// <summary>
        /// Writes csv content to a string
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Csv content in a string</returns>
        public string WriteCsv(CsvFile csvFile, Encoding encoding)
        {
            string content = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default))
                {
                    WriteToStream(csvFile, writer);
                    writer.Flush();
                    memoryStream.Position = 0;

                    using (StreamReader reader = new StreamReader(memoryStream, encoding ?? Encoding.Default))
                    {
                        content = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        memoryStream.Close();
                    }
                }
            }

            return content;
        }

        #endregion CsvFile write methods

        #region DataTable write methods

        /// <summary>
        /// Writes a DataTable to a file
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="filePath">File path</param>
        public void WriteCsv(DataTable dataTable, string filePath)
        {
            WriteCsv(dataTable, filePath, null);
        }

        /// <summary>
        /// Writes a DataTable to a file
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="filePath">File path</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(DataTable dataTable, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (StreamWriter writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
            {
                WriteToStream(dataTable, writer);
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Writes a DataTable to a stream
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="stream">Stream</param>
        public void WriteCsv(DataTable dataTable, Stream stream)
        {
            WriteCsv(dataTable, stream, null);
        }

        /// <summary>
        /// Writes a DataTable to a stream
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(DataTable dataTable, Stream stream, Encoding encoding)
        {
            stream.Position = 0;
            _streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
            WriteToStream(dataTable, _streamWriter);
            _streamWriter.Flush();
            stream.Position = 0;
        }

        /// <summary>
        /// Writes the DataTable to a string
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Csv content in a string</returns>
        public string WriteCsv(DataTable dataTable, Encoding encoding)
        {
            string content = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, encoding ?? Encoding.Default))
                {
                    WriteToStream(dataTable, writer);
                    writer.Flush();
                    memoryStream.Position = 0;

                    using (StreamReader reader = new StreamReader(memoryStream, encoding ?? Encoding.Default))
                    {
                        content = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        memoryStream.Close();
                    }
                }
            }

            return content;
        }

        #endregion DataTable write methods

        /// <summary>
        /// Writes the Csv File
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="writer">TextWriter</param>
        private void WriteToStream(CsvFile csvFile, TextWriter writer)
        {
            if (csvFile.Headers.Count > 0)
                WriteRecord(csvFile.Headers, writer);

            csvFile.Records.ForEach(record => WriteRecord(record.Fields, writer));
        }

        /// <summary>
        /// Writes the Csv File
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="writer">TextWriter</param>
        private void WriteToStream(DataTable dataTable, TextWriter writer)
        {
            List<string> fields = (from DataColumn column in dataTable.Columns select column.ColumnName).ToList();
            if (WriteColumnHeaders)
                WriteRecord(fields, writer);

            foreach (DataRow row in dataTable.Rows)
            {
                fields.Clear();
                fields.AddRange(row.ItemArray.Select(o => o.ToString()));
                WriteRecord(fields, writer);
            }
        }

        /// <summary>
        /// Writes the record to the underlying stream
        /// </summary>
        /// <param name="fields">Fields</param>
        /// <param name="writer">TextWriter</param>
        private void WriteRecord(IList<string> fields, TextWriter writer)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                bool quotesRequired = fields[i].Contains(Delimiter);
                if (!quotesRequired)
                    quotesRequired = TextqualifierForced;
                bool escapeQuotes = fields[i].Contains("\"");
                string fieldValue = (escapeQuotes ? fields[i].Replace("\"", "\"\"") : fields[i]);

                if (fieldValue.Contains("\r") || fieldValue.Contains("\n"))
                {
                    quotesRequired = true;
                }
                if (ReplaceCarriageReturnsAndLineFeedsFromFieldValues && (fieldValue.Contains("\r") || fieldValue.Contains("\n")))
                {
                    //quotesRequired = true;
                    fieldValue = fieldValue.Replace("\r\n", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\r", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\n", CarriageReturnAndLineFeedReplacement);
                }

                writer.Write(string.Format("{0}{1}{0}{2}", 
                    (quotesRequired || escapeQuotes ? "\"" : string.Empty),
                    fieldValue, 
                    ( i < (fields.Count - 1) ? Delimiter : string.Empty)));
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Disposes of all unmanaged resources
        /// </summary>
        public void Dispose()
        {
            if (_streamWriter == null) 
                return;

            _streamWriter.Close();
            _streamWriter.Dispose();
        }

        #endregion Methods

    }
}
