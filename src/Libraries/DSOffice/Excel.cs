using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Dynamo.Utilities;

namespace DSOffice
{
    class ExcelInterop
    {
        private static Microsoft.Office.Interop.Excel.Application _app;
        public static Microsoft.Office.Interop.Excel.Application App
        {
            get
            {
                if (_app == null || !ExcelProcessRunning)
                {
                    _app = RegisterAndGetApp();
                }
                if (_showOnStartup) _app.Visible = true;
                return _app;
            }
        }

        public static bool _showOnStartup = true;
        public static bool ShowOnStartup
        {
            get { return _showOnStartup; }
            set { _showOnStartup = value; }
        }

        public static Application RegisterAndGetApp()
        {
            Application excel = null;

            // get excel, throw exception if it is not
            var officeType = Type.GetTypeFromProgID("Excel.Application");
            if (officeType == null)
                throw new Exception("Excel is not installed.");

            try
            {
                excel = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch (COMException e)
            {
                // 0x800401E3 - the excel process simply was not running, we continue if we
                // encounter this exception

                if (!e.ToString().Contains("0x800401E3"))
                {
                    throw new Exception("Error setting up communication with Excel.  Try closing any open Excel instances.");
                }
            }

            if (excel == null) excel = new Microsoft.Office.Interop.Excel.Application();

            if (excel == null)
            {
                throw new Exception("Excel could not be opened.");
            }

            dynSettings.Controller.DynamoModel.CleaningUp += DynamoModelOnCleaningUp;

            excel.Visible = ShowOnStartup;

            return excel;
        }

        /// <summary>
        /// Check if the excel process is running
        /// </summary>
        public static bool ExcelProcessRunning
        {
            get
            {
                return Process.GetProcessesByName("EXCEL").Length != 0;
            }
        }

        /// <summary>
        /// Check if this object holds a reference to Excel
        /// </summary>
        public static bool HasValidExcelReference
        {
            get { return _app != null; }
        }

        /// <summary>
        /// Close all Excel workbooks and provide SaveAs dialog if needed.  Also, perform
        /// garbage collection and remove references to Excel App
        /// </summary>
        public static void TryQuitAndCleanup(bool saveWorkbooks)
        {
            if (HasValidExcelReference)
            {
                if (ExcelProcessRunning)
                {
                    App.Workbooks.Cast<Workbook>().ToList().ForEach((wb) => wb.Close(saveWorkbooks));
                    App.Quit();
                }

                while (Marshal.ReleaseComObject(_app) > 0)
                {

                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                _app = null;
            }
        }

        private static void DynamoModelOnCleaningUp(object sender, EventArgs eventArgs)
        {
            TryQuitAndCleanup(true);
        }
    }

    public class Excel
    {
        #region Helper methods

        private static object[][] ConvertToJaggedArray(object[,] input)
        {
            int rows = input.GetUpperBound(0) + 1;
            int cols = input.GetUpperBound(1) + 1;

            object[][] output = new object[rows][];

            for (int i = 0; i < rows; i++)
            {
                output[i] = new object[cols];

                for (int j = 0; j < cols; j++)
                {
                    output[i][j] = input[i, j];
                }
            }

            return output;
        }

        public static object[,] ConvertToDimensionalArray(object[][] input, out int rows, out int cols)
        {
            rows = input.GetUpperBound(0) + 1;
            cols = 0;
            for (int i = 0; i < rows; i++)
                cols = Math.Max(cols, input[i].GetUpperBound(0) + 1);

            object[,] output = new object[rows, cols];
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (j > input[i].GetUpperBound(0))
                        output[i, j] = "";
                    else
                        output[i, j] = input[i][j];
                }
            }

            return output;
        }

        #endregion

        public static Workbook ReadExcelFile(string path)
        {
            var workbook =
                ExcelInterop.App.Workbooks.Cast<Workbook>().FirstOrDefault(e => e.FullName == path);

            if (workbook == null && File.Exists(path))
            {
                workbook = ExcelInterop.App.Workbooks.Open(path, true, false);
            }

            return workbook;
        }

        public static Worksheet[] GetWorksheetsFromExcelWorkbook(Workbook workbook)
        {
            return workbook.Worksheets.Cast<Worksheet>().ToArray();
        }

        public static Worksheet GetExcelWorksheetByName(Workbook workbook, string name)
        {
            var sheet = workbook.Worksheets.Cast<Worksheet>().FirstOrDefault(ws => ws.Name == name);

            if (sheet == null)
            {
                throw new Exception("Could not find a worksheet in the workbook with that name.");
            }

            return sheet;
        }

        public static object[][] GetDataFromExcelWorksheet(Worksheet worksheet)
        {
            var vals = worksheet.UsedRange.get_Value();

            return ConvertToJaggedArray(vals);
        }

        public static Worksheet WriteDataToExcelWorksheet(
            Worksheet worksheet, int startRow, int startColumn, object data)
        {
            // clear existing elements for dynamic update
            var usedRange = worksheet.UsedRange;
            usedRange.Cells.ClearContents();

            object[][] data2D;

            if (!data.GetType().IsArray)
            {
                // single-valued data
                data2D = new object[][]
                {
                    new object[] { data }
                };
            }
            else if (!((object[])data)[0].GetType().IsArray)
            {
                // one-dimensional data
                data2D = new object[][]
                {
                    (object[])data
                };
            }
            else
            {
                // two-dimensional data
                data2D = (object[][])data;
            }

            startRow = Math.Max(0, startRow);
            startColumn = Math.Max(0, startColumn);
            
            int numRows, numColumns;
            
            object[,] rangeData = ConvertToDimensionalArray(data2D, out numRows, out numColumns);

            var c1 = (Range)worksheet.Cells[startRow + 1, startColumn + 1];
            var c2 = (Range)worksheet.Cells[startRow + numRows, startColumn + numColumns];
            var range = worksheet.get_Range(c1, c2);
            range.Value = rangeData;
            
            return worksheet;
        }
        
        public static Worksheet AddExcelWorksheetToWorkbook(Workbook workbook, string name)
        {
            var worksheet = workbook.Worksheets.Add();
            worksheet.Name = name;

            return worksheet;
        }

        public static Workbook NewExcelWorkbook()
        {
            return ExcelInterop.App.Workbooks.Add();
        }

        public static Workbook SaveAsExcelWorkbook(Workbook workbook, string filename)
        {
            if (workbook.FullName == filename)
            {
                workbook.Save();
            }
            else
            {
                workbook.SaveAs(filename);
            }

            return workbook;
        }
    }
}
