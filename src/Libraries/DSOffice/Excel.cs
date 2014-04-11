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

        private static bool _showOnStartup = true;
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

        private Excel()
        {

        }

        public static object ReadExcelFile(string path)
        {
            
            return WorkBook.ReadExcelFile(path);
        }

        public static object[] GetWorksheetsFromExcelWorkbook(object workbook)
        {
            WorkBook wb = (WorkBook)workbook;
            return wb.WorkSheets;
        }

        public static object GetExcelWorksheetByName(object workbook, string name)
        {
            WorkBook wb = (WorkBook)workbook;
            return wb.GetWorksheetByName(name);
        }

        public static object[][] GetDataFromExcelWorksheet(object worksheet)
        {
            WorkSheet ws = (WorkSheet)worksheet;
            return ws.Data;
        }

        public static object WriteDataToExcelWorksheet(
            object worksheet, int startRow, int startColumn, object[][] data)
        {
            WorkSheet ws = (WorkSheet)worksheet;
            return ws.WriteData(startRow, startColumn, data);
        }

        public static object AddExcelWorksheetToWorkbook(object workbook, string name)
        {
            WorkBook wb = (WorkBook)workbook;
            return new WorkSheet(wb, name);
        }

        public static object NewExcelWorkbook()
        {
            return new WorkBook("");
        }

        public static object NewExcelWorkbook(string filePath)
        {
            return new WorkBook(filePath);
        }

        public static object SaveAsExcelWorkbook(object workbook, string filename)
        {
            WorkBook wb = (WorkBook)workbook;
            return new WorkBook(wb, filename);
        }
    }

    internal class WorkSheet
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
        /// <summary>
        /// return data from given worksheet (GetDataFromExcelWorksheet node)
        /// </summary>
        internal object[][] Data 
        { 
            get
            {
                var vals = ws.UsedRange.get_Value();

                // if worksheet is empty
                if (vals == null)
                    return new object[0][];

                // if worksheet contains a single value
                if (!vals.GetType().IsArray)
                    return new object[][] { new object[] { vals } };

                return ConvertToJaggedArray((object[,])vals);
            }
        }  

        private WorkBook wb = null;        
        private Worksheet ws = null;

        /// <summary>
        /// create new worksheet from given workbook and name (AddExcelWorksheetToWorkbook node)
        /// </summary>
        /// <param name="wbook"></param>
        /// <param name="sheetName"></param>
        internal WorkSheet(WorkBook wbook, string sheetName)
        {
            wb = wbook;
            ws = (Worksheet)wb.Add();
            ws.Name = sheetName;

            wb.Save();
        }

        internal WorkSheet(Worksheet ws, WorkBook wb)
        {
            this.ws = ws;
            this.wb = wb;
        }
          
        /// <summary>
        /// instance method, write data to existing worksheet, (WriteDataToExcelWorksheet node)
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal WorkSheet WriteData(int startRow, int startColumn, object[][] data)
        {
            startRow = Math.Max(0, startRow);
            startColumn = Math.Max(0, startColumn);
            int numRows, numColumns;

            object[,] rangeData = ConvertToDimensionalArray(data, out numRows, out numColumns);

            var c1 = (Range)ws.Cells[startRow + 1, startColumn + 1];
            var c2 = (Range)ws.Cells[startRow + numRows, startColumn + numColumns];
            var range = ws.get_Range(c1, c2);
            range.Value = rangeData;

            wb.Save();
            return this;
        }

    }

    internal class WorkBook
    {
        /// <summary>
        /// 
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// (GetWorksheetsFromExcelWorkbook node)
        /// </summary>
        internal WorkSheet[] WorkSheets 
        {
            get
            {
                return wb.Worksheets.Cast<Worksheet>().Select(n => new WorkSheet(n, this)).ToArray();
            }
        }    
   
        private Workbook wb = null;

        internal object Add()
        {
            return wb.Worksheets.Add();
        }

        internal void Save()
        {
            if(!String.IsNullOrEmpty(wb.Path))
                wb.Save();
        }
 
        /// <summary>
        /// Creates a new Workbook with filepath as input
        /// </summary>
        internal WorkBook(string filePath)
        {
            wb = ExcelInterop.App.Workbooks.Add();
            Name = filePath;

            if (!String.IsNullOrEmpty(filePath))
            {                
                wb.SaveAs(filePath);
            }
        }

        /// <summary>
        /// (SaveAsExcelWorkbook node)
        /// </summary>
        /// <param name="wbook"></param>
        /// <param name="filename"></param>
        internal WorkBook(WorkBook wbook, string filename)
        {
            Name = filename;
            wb = wbook.wb;

            if (wb.FullName == filename)
                wb.Save();
            else
            {
                wb.SaveAs(filename);
            }

        }
        
        //public WorkBook(string wbName)
        //{
        //    Name = wbName;
        //}

        private WorkBook(Workbook wb, string filePath)
        {
            this.wb = wb;
            Name = filePath;
        }

        /// <summary>
        /// (ReadExcelFile node)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static WorkBook ReadExcelFile(string path)
        {
            var workbookOpen = ExcelInterop.App.Workbooks.Cast<Workbook>()
                .FirstOrDefault(e => e.FullName == path);

            if (workbookOpen != null)
                return new WorkBook(workbookOpen, path);

            if (File.Exists(path))
                return new WorkBook(ExcelInterop.App.Workbooks.Open(path, true, false), path);

            throw new FileNotFoundException("File path not found.", path);
        }

        /// <summary>
        /// instance method, (GetExcelWorksheetByName node)
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        internal WorkSheet GetWorksheetByName(string sheetName)
        {
            var ws = wb.Worksheets.Cast<Worksheet>().FirstOrDefault(sheet => sheet.Name == sheetName);

            if (ws == null)
                throw new ArgumentException("No worksheet matches the given string.", "sheetName");

            return new WorkSheet(ws, this);
        }             

    }
}
