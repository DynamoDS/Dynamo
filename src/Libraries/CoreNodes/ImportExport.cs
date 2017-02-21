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
        ///     Imports data from a CSV (comma separated values) file and put the items into a list.
        /// </summary>
        /// <param name="filePath">The CSV file to be converted into a list.</param>
        /// <returns name="list">The list containing the items in the CSV file.</returns>
        /// <search>import,csv,comma,file,list,separate</search>
        public static IList ImportFromCSV(string filePath)
        {
            return List.Transpose(ReadCSVFile(filePath));
        }

        /// <summary>
        ///     Imports data from a CSV (comma separated values) file, put the items into a list and 
        ///     transpose it if needed.
        /// </summary>
        /// <param name="filePath">The CSV file to be converted into a list.</param>
        /// <param name="transpose">Whether the resulting list should be transposed.</param>
        /// <returns name="list">The list containing the items in the CSV file.</returns>
        /// <search>import,csv,comma,file,list,separate,transpose</search>
        public static IList ImportFromCSV(string filePath, bool transpose)
        {
            if (transpose) return ReadCSVFile(filePath);
            else return List.Transpose(ReadCSVFile(filePath));
        }

        private static IList ReadCSVFile(string filePath)
        {
            List<object[]> newList = new List<object[]>();
            int maxLength = 0;
            if (File.Exists(filePath))
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fileStream))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] lineStr = sr.ReadLine().Split(',');
                        maxLength = (maxLength < lineStr.Count()) ? lineStr.Count() : maxLength;
                        object[] newRow = new object[maxLength];
                        for (int i = 0; i < lineStr.Count(); i++)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(lineStr[i]) || string.IsNullOrWhiteSpace(lineStr[i]))
                                    newRow[i] = null;
                                else if (lineStr[i].Contains("."))
                                    newRow[i] = Double.Parse(lineStr[i]);
                                else newRow[i] = Int32.Parse(lineStr[i]);
                            }
                            catch (Exception)
                            {
                                newRow[i] = lineStr[i];
                            }
                        }
                        newList.Add(newRow);
                    }
                }
                for (int row = 0; row < newList.Count(); row++)
                {
                    int count = newList[row].Count();
                    if (count < maxLength)
                    {
                        object[] newRow = new object[maxLength];
                        Array.Copy(newList[row], newRow, count);
                        for (int j = count; j < maxLength; j++)
                        {
                            newRow[j] = null;
                        }
                        newList[row] = newRow;
                    }
                }
            }
            return newList;
        }

    }
}
