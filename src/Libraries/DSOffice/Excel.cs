using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Dynamo.Utilities;
using Autodesk.DesignScript.Runtime;

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
            if(eventArgs != null)
            {
                Dynamo.Nodes.ExcelCloseEventArgs args = eventArgs as Dynamo.Nodes.ExcelCloseEventArgs;
                if (args != null)
                {
                    TryQuitAndCleanup(args.SaveWorkbooks);
                }
            }
            else
                TryQuitAndCleanup(true);
        }
    }

    public class Excel
    {

        private Excel()
        {

        }

        /// <summary>
        /// Reads the given Excel file and returns a workbook
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkBook ReadExcelFile(string path)
        {
            return WorkBook.ReadExcelFile(path);
        }

        /// <summary>
        /// Returns a list of all the worksheets present in the given Excel workbook
        /// </summary>
        /// <param name="workbook"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkSheet[] GetWorksheetsFromExcelWorkbook(WorkBook workbook)
        {
            return workbook.WorkSheets;
        }

        /// <summary>
        /// Returns the worksheet in the given workbook by its name
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkSheet GetExcelWorksheetByName(WorkBook workbook, string name)
        {
            return workbook.GetWorksheetByName(name);
        }

        /// <summary>
        /// Reads and retrieves the data from the given Excel worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object[][] GetDataFromExcelWorksheet(WorkSheet worksheet)
        {
            return worksheet.Data;
        }

        /// <summary>
        /// Writes the given data at the specified row and column no. (base 0) in the given worksheet
        /// and returns the worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="startRow"></param>
        /// <param name="startColumn"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkSheet WriteDataToExcelWorksheet(
            WorkSheet worksheet, int startRow=0, int startColumn=0, object[][] data=null)
        {
            if (data == null)
                return worksheet;
            else
                return worksheet.WriteData(startRow, startColumn, data);
        }

        /// <summary>
        /// Adds a new Excel worksheet with the given name to the given workbook        
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkSheet AddExcelWorksheetToWorkbook(WorkBook workbook, string name)
        {
            return new WorkSheet(workbook, name);
        }

        /// <summary>
        /// Creates a new temporary Excel workbook
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkBook NewExcelWorkbook()
        {
            return new WorkBook("");
        }

        /// <summary>
        /// Saves the given Excel workbook to the specified file path and returns it
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkBook SaveAsExcelWorkbook(WorkBook workbook, string filename)
        {
            return new WorkBook(workbook, filename);
        }

        /// <summary>
        ///     Read data from a Microsoft Excel spreadsheet. Data is read by row and
        ///     returned in a series of lists by row. Rows and columns are zero-indexed;
        ///     for example, the value in cell A1 will appear in the data list at [0,0].
        ///     This node requires Microsoft Excel to be installed.
        /// </summary>
        /// <param name="filePath">File path to the Microsoft Excel spreadsheet.</param>
        /// <param name="sheetName">Name of the worksheet containing the data.</param>
        /// <returns name="data">Rows of data from the Excel worksheet.</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] Read(string filePath, string sheetName)
        {
            WorkBook wb = WorkBook.ReadExcelFile(filePath);
            WorkSheet ws = wb.GetWorksheetByName(sheetName);
            return ws.Data;
        }

        /// <summary>
        ///     Write data to a Microsoft Excel spreadsheet. Data is written by row
        ///     with sublists to be written in successive rows. Rows and columns are
        ///     zero-indexed; for example, the value in the data list at [0,0] will
        ///     be written to cell A1. This node requires Microsoft Excel to be
        ///     installed.
        /// </summary>
        /// <param name="filePath">File path to the Microsoft Excel spreadsheet.</param>
        /// <param name="sheetName">Name of the workseet to write data to.</param>
        /// <param name="startRow">Start row for writing data. Enter 0 for A, 1 for B, etc.</param>
        /// <param name="startCol">
        ///     Start column for writing data. Enter 0 for col 1, 1 for column 2, ect.
        /// </param>
        /// <param name="data">Data to write to the spreadsheet.</param>
        /// <returns name="data">Data written to the spreadsheet.</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] Write(string filePath, string sheetName, int startRow, int startCol, object[][] data)
        {
            WorkBook wb = new WorkBook(filePath);
            WorkSheet ws = new WorkSheet (wb, sheetName);
            ws = ws.WriteData(startRow, startCol, data);
            return ws.Data;
        }
    }

    public class WorkSheet
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
                        output[i, j] = input[i][j].ToString();
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
        internal WorkSheet (WorkBook wbook, string sheetName)
        {
            wb = wbook;
            WorkSheet wSheet = wbook.WorkSheets.FirstOrDefault(n => n.ws.Name == sheetName);
            
            if (wSheet != null)
            {
                // Overwrite sheet
                DSOffice.ExcelInterop.App.DisplayAlerts = false;
                wSheet.ws.Delete();
                DSOffice.ExcelInterop.App.DisplayAlerts = true;
            }
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

    public class WorkBook
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
        /// Creates a new Workbook with filepath and sheet name as input
        /// </summary>
        internal WorkBook(string filePath)
        {            
            Name = filePath;

            if (!String.IsNullOrEmpty(filePath))
            {
                try
                {
                    Workbook workbook = ExcelInterop.App.Workbooks.Open(filePath);
                    wb = workbook;
                    wb.Save();
                    
                }
                catch (Exception)
                {
                    // Exception is thrown when there is no existing workbook with the given filepath
                    wb = ExcelInterop.App.Workbooks.Add();
                    wb.SaveAs(filePath);
                }
            }
            else
                wb = ExcelInterop.App.Workbooks.Add();
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
                try
                {
                    Workbook workbook = ExcelInterop.App.Workbooks.Open(filename);
                    workbook.Close(false);
                }
                catch (Exception)
                {   
                }
                
                wb.SaveAs(filename);
            }

        }
        
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
