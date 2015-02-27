using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Experimental
{
    public static class _CSVExport
    {
        public static void ExportToCSV(double[][] data, String filePath)
        {
            System.IO.StreamWriter writer = new StreamWriter(filePath);

            for (int i = 0; i < data.GetLength(0); i++ )
            {
                StringBuilder line = new StringBuilder();

                for (int j = 0; j < data[i].Length; j++)
                {
                    line.Append(data[i][j]);
                    line.Append(", ");

                }

                string lineOut = line.ToString();
                lineOut = lineOut.Substring(0, lineOut.LastIndexOf(','));
                writer.WriteLine(lineOut);

            }
            
            writer.Flush();
            writer.Close();

        }


    }
}
