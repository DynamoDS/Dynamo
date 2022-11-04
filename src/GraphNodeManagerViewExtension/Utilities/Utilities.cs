using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;

namespace Dynamo.GraphNodeManager.Utilities
{
    public static class Utilities
    {
        /// <summary>
        /// Exports a collection of objects to CSV file
        /// </summary>
        /// <param name="exportObject">The array of nodes to be exported.</param>
        /// <param name="promptName">The default name to be provided for saving the file.</param>
        public static void ExportToCSV(object exportObject, string promptName)
        {
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                Stream myStream;
                
                saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.Title = "Save CSV file";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = promptName;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string output = JsonConvert.SerializeObject(exportObject);

                    var csv = jsonToCSV(output);

                    //write csv file into file stream
                    File.WriteAllLines(saveFileDialog.FileName, csv);
                }
            }
        }

        /// <summary>
        /// Export a collection of objects to JSON file
        /// </summary>
        /// <param name="exportObject">The array of nodes to be exported.</param>
        /// <param name="promptName">The default name to be provided for saving the file.</param>
        public static void ExportToJson(object exportObject, string promptName)
        {
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                Stream myStream;
                
                saveFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.Title = "Save JSON file";
                saveFileDialog.DefaultExt = "json";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = promptName;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Read the contents of the file into a stream
                    var fileStream = saveFileDialog.OpenFile();

                    using (StreamWriter file = new StreamWriter(fileStream, System.Text.Encoding.Unicode))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        //serialize object directly into file stream
                        serializer.Serialize(file, exportObject);
                    }
                }
            }
        }

        /// <summary>
        /// Converts JSON to CSV 
        /// </summary>
        /// <param name="jsonContent"></param>
        /// <returns></returns>
        public static List<string> jsonToCSV(string jsonContent)
        {
            XmlNode xml = JsonConvert.DeserializeXmlNode("{records:{record:" + jsonContent + "}}");
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.LoadXml(xml.InnerXml);

            XmlReader xmlReader = new XmlNodeReader(xml);
            DataSet dataSet = new DataSet();

            dataSet.ReadXml(xmlReader);
            var dataTable = dataSet.Tables[0];

            //Datatable to CSV
            var lines = new List<string>();
            string[] columnNames = dataTable.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName).
                ToArray();

            var header = string.Join(",", columnNames);
            lines.Add(header);

            var valueLines = dataTable.AsEnumerable()
                .Select(row => string.Join(",", row.ItemArray));

            lines.AddRange(valueLines);
            return lines;
        }
    }
}
