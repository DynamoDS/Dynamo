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

        private static bool _showOnStartup = false;
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
            int rows = input.GetUpperBound(0);
            int cols = input.GetUpperBound(1);

            object[][] output = new object[rows][];

            for (int i = 0; i < rows; i++)
            {
                output[i] = new object[cols];

                for (int j = 0; j < cols; j++)
                {
                    output[i][j] = input[i + 1, j + 1];
                }
            }

            return output;
        }

        private static object[,] ConvertToDimensionalArray(object[][] input, out int rows, out int cols)
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

        public static object ReadExcelFile(string path)
        {
            var workbookOpen = ExcelInterop.App.Workbooks.Cast<Workbook>()
                .FirstOrDefault(e => e.FullName == path);

            if (workbookOpen != null)
                return workbookOpen;
            
            if (File.Exists(path))
                return ExcelInterop.App.Workbooks.Open(path, true, false);

            return null;
        }

        public static object[] GetWorksheetsFromExcelWorkbook(object workbook)
        {
            Workbook wb = (Workbook)workbook;

            return wb.Worksheets.Cast<Worksheet>().ToArray();
        }

        public static object GetExcelWorksheetByName(object workbook, string name)
        {
            Workbook wb = (Workbook)workbook;

            return wb.Worksheets.Cast<Worksheet>().FirstOrDefault(ws => ws.Name == name);
        }

        public static object[][] GetDataFromExcelWorksheet(object worksheet)
        {
            Worksheet ws = (Worksheet)worksheet;
            var vals = ws.UsedRange.get_Value();

            // if worksheet is empty
            if (vals == null)
                return new object[0][];

            // if worksheet contains a single value
            if (!vals.GetType().IsArray)
                return new object[][] { new object[] { vals } };

            return ConvertToJaggedArray(vals);
        }

        public static object WriteDataToExcelWorksheet(
            object worksheet, int startRow, int startColumn, object[][] data)
        {
            Worksheet ws = (Worksheet)worksheet;
            startRow = Math.Max(0, startRow);
            startColumn = Math.Max(0, startColumn);
            int numRows, numColumns;

            object[,] rangeData = ConvertToDimensionalArray(data, out numRows, out numColumns);

            var c1 = (Range)ws.Cells[startRow + 1, startColumn + 1];
            var c2 = (Range)ws.Cells[startRow + numRows, startColumn + numColumns];
            var range = ws.get_Range(c1, c2);
            range.Value = rangeData;
            
            return ws;
        }

        public static object AddExcelWorksheetToWorkbook(object workbook, string name)
        {
            Workbook wb = (Workbook)workbook;

            var worksheet = wb.Worksheets.Add();
            worksheet.Name = name;

            return wb;
        }

        public static object NewExcelWorkbook()
        {
            return ExcelInterop.App.Workbooks.Add();
        }

        public static object SaveAsExcelWorkbook(object workbook, string filename)
        {
            Workbook wb = (Workbook)workbook;
            if (wb.FullName == filename)
                wb.Save();
            else
                wb.SaveAs(filename);

            return wb;
        }
    }
}
