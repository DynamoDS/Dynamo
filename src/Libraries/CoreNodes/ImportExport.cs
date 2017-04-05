#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace DSCore
{
    /// <summary>
    ///     Methods for Import/Export category.
    /// </summary>
    public static class ImportExport
    {
        /// <summary>
        ///     Write a list of lists into a file using a comma-separated values 
        ///     format. Outer list represents rows, inner lists represent columns. 
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="data">List of lists to write into CSV</param>
        /// <returns name="str">Contents of the text file.</returns>
        /// <search>write,text,file</search>
        public static void ExportCSV(string filePath, object[][] data)
        {
            using (var writer = new StreamWriter(DSCore.IO.File.AbsolutePath(filePath)))
            {
                foreach (var line in data)
                {
                    int count = 0;
                    foreach (var entry in line)
                    {
                        writer.Write(entry);
                        if (++count < line.Length)
                            writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        ///     Imports data from a CSV (comma separated values) file, put the items into a list and 
        ///     transpose it if needed.
        /// </summary>
        /// <param name="filePath">The CSV file to be converted into a list.</param>
        /// <param name="transpose">Whether the resulting list should be transposed.</param>
        /// <returns name="list">The list containing the items in the CSV file.</returns>
        /// <search>import,csv,comma,file,list,separate,transpose</search>
        public static IList ImportCSV(string filePath, bool transpose = false)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                // File not existing.
                throw new FileNotFoundException();
            }
            // Open the file to read from.
            List<object[]> CSVdatalist = new List<object[]>();
            int colNum = 0;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using (var sr = new StreamReader(fileStream))
            {
                while (!sr.EndOfStream)
                {
                    string[] lineStr = sr.ReadLine().Split(',');
                    int count = 0;

                    // Convert each line into an array of objects (int, double, null or string)
                    // and append them into CSVdatalist. 
                    object[] line = new object[lineStr.Length];
                    foreach (string elementStr in lineStr)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(elementStr) || string.IsNullOrWhiteSpace(elementStr))
                                line[count] = null;
                            else if (elementStr.Contains("."))
                                line[count] = Double.Parse(elementStr);
                            else line[count] = Int32.Parse(elementStr);
                        }
                        catch (Exception)
                        {
                            line[count] = elementStr;
                        }
                        count++;
                    }
                    colNum = System.Math.Max(colNum, line.Length);
                    CSVdatalist.Add(line);
                }
            }

            // The length of all arrays in CSVdatalist must be the same. If the length of the array
            // is less than colNum, null is appended to the array to achieve the required length.  
            for (int row = 0; row < CSVdatalist.Count(); row++)
            {
                int count = CSVdatalist[row].Count();
                if (count < colNum)
                {
                    object[] newRow = new object[colNum];
                    Array.Copy(CSVdatalist[row], newRow, count);
                    for (int j = count; j < colNum; j++)
                    {
                        newRow[j] = null;
                    }
                    CSVdatalist[row] = newRow;
                }
            }

            // Judge whether the array needed to be transposed (when transpose is false) or not (when transpose is true)
            if (transpose) return CSVdatalist;
            else return List.Transpose(CSVdatalist);
        }
    }
}
