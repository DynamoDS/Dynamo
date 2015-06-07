using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using DynamoServices;
using Autodesk.DesignScript.Runtime;
using ProtoCore.DSASM;
using ProtoCore.Properties;

namespace DSOffice
{
    internal class ExcelCloseEventArgs : EventArgs
    {
        public ExcelCloseEventArgs(bool saveWorkbooks = true)
        {
            this.SaveWorkbooks = saveWorkbooks;
        }

        public bool SaveWorkbooks { get; private set; }
    }

    internal static class ExcelInterop
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
                if (_showOnStartup)
                    _app.Visible = true;

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
                throw new Exception(Resources.ExcelNotInstalled);

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
                    throw new Exception(Resources.ExcelCommunicationError);
                }
            }
            catch (Exception)
            {
                // An exception "The URI prefix is not recognized" will be
                // thrown out for the first run, no idea why that happen, so
                // just swallow this exception and try to create an new excel
                // instance.

                // Sometimes a FileNotFoundException occurs with DynamoCore.XmlSerializers
                // which is not clear why. This exception also makes Excel tests very flaky
                // It is found that just swallowing these exceptions and creating a new excel instance 
                // below causes the test to continue and successfuly pass
            }

            if (excel == null)
            {
                excel = new Microsoft.Office.Interop.Excel.Application();
            }

            // KILLDYNSETTINGS - is this safe
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            excel.Visible = ShowOnStartup;
            excel.DisplayAlerts = false;

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
        private static void TryQuitAndCleanup(bool saveWorkbooks)
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

                _app = null;
            }
        }

        internal static void OnProcessExit(object sender, EventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                var args = eventArgs as ExcelCloseEventArgs;
                if (args != null)
                {
                    TryQuitAndCleanup(args.SaveWorkbooks);
                }
            }
            else
                TryQuitAndCleanup(true);
        }
    }

    public static class Excel
    {
        /// <summary>
        /// Reads the given Excel file and returns a workbook
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static WorkBook ReadExcelFile(FileInfo file)
        {
            return WorkBook.ReadExcelFile(file.FullName);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static WorkBook ReadExcelFile(string file)
        {
            return WorkBook.ReadExcelFile(file);
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
            WorkSheet worksheet, int startRow = 0, int startColumn = 0, object[][] data = null)
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
        /// <param name="file">File representing the Microsoft Excel spreadsheet.</param>
        /// <param name="sheetName">Name of the worksheet containing the data.</param>
        /// <param name="readAsStrings">toggle to switch between reading Excel file as strings only or not</param>
        /// <returns name="data">Rows of data from the Excel worksheet.</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] ReadFromFile(FileInfo file, string sheetName, bool readAsStrings = false)
        {
            WorkBook wb = WorkBook.ReadExcelFile(file.FullName);
            WorkSheet ws = wb.GetWorksheetByName(sheetName);
            if (readAsStrings)
                return ws.GetData(true);

            return ws.Data;
        }

        [Obsolete("Use File.FromPath -> Excel.ReadFromFile node instead.")]
        public static object[][] Read(string filePath, string sheetName)
        {
            return ReadFromFile(new FileInfo(filePath), sheetName);
        }

        /// <summary>
        ///     Write data to a Microsoft Excel spreadsheet. Data is written by row
        ///     with sublists to be written in successive rows. Rows and columns are
        ///     zero-indexed; for example, the value in the data list at [0,0] will
        ///     be written to cell A1. Null values and empty lists are written to Excel 
        ///     as empty cells. This node requires Microsoft Excel to be installed. 
        /// </summary>
        /// <param name="filePath">File path to the Microsoft Excel spreadsheet.</param>
        /// <param name="sheetName">Name of the workseet to write data to.</param>
        /// <param name="startRow">Start row for writing data. Enter 0 for A, 1 for B, etc.</param>
        /// <param name="startCol">
        ///     Start column for writing data. Enter 0 for col 1, 1 for column 2, ect.
        /// </param>
        /// <param name="data">Data to write to the spreadsheet.</param>
        /// <param name="overWrite"></param>
        /// <returns name="data">Data written to the spreadsheet.</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] WriteToFile(string filePath, string sheetName, int startRow, int startCol, object[][] data, bool overWrite = false)
        {
            WorkBook wb = new WorkBook(filePath);
            WorkSheet ws = new WorkSheet(wb, sheetName, overWrite);
            ws = ws.WriteData(startRow, startCol, data);
            return ws.Data;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class WorkSheet
    {
        #region Helper methods

        private static object[][] ConvertToJaggedArray(object[,] input, bool convertToString = false)
        {
            int rows = input.GetUpperBound(0);
            int cols = input.GetUpperBound(1);

            object[][] output = new object[rows][];

            for (int i = 0; i < rows; i++)
            {
                output[i] = new object[cols];

                for (int j = 0; j < cols; j++)
                {
                    if (convertToString)
                    {
                        if (input[i + 1, j + 1] == null)
                            output[i][j] = null;
                        else
                            output[i][j] = input[i + 1, j + 1].ToString();
                    }
                    else
                        output[i][j] = input[i + 1, j + 1];
                }
            }

            return output;
        }

        private static object[,] ConvertToDimensionalArray(object[][] input, out int rows, out int cols)
        {
            if (input == null)
            {
                rows = cols = 1;
                return new object[,] { { "" } };
            }

            rows = input.GetUpperBound(0) + 1;
            cols = 0;
            for (int i = 0; i < rows; i++)
            {
                if (input[i] != null)
                    cols = Math.Max(cols, input[i].GetUpperBound(0) + 1);
            }

            // if the input data is an empty list or a list of nested empty lists
            // return an empty cell
            if(rows == 0 || cols == 0)
            {
                rows = cols = 1;
                return new object[,] { { "" } };
            }

            object[,] output = new object[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (input[i] == null || j > input[i].GetUpperBound(0))
                        output[i, j] = "";
                    else
                    {
                        var item = input[i][j];

                        if (item is double)
                        {
                            output[i, j] = ((double)item).ToString(CultureInfo.InvariantCulture);
                        }
                        else if (item is float)
                        {
                            output[i, j] = ((float)item).ToString(CultureInfo.InvariantCulture);
                        }
                        else if (item is DateTime)
                        {
                            output[i, j] = ((DateTime)item).ToString(CultureInfo.InvariantCulture);
                        }
                        else if (item == null)
                        {
                            output[i, j] = "";
                        }
                        else if (item is StackValue)
                        {
                            if (((StackValue)item).IsPointer)
                            {
                                string message = string.Format(Resources.kMethodResolutionFailureWithTypes,
                                    "Excel.WriteToFile", "_SingleFunctionObject");
                                LogWarningMessageEvents.OnLogWarningMessage(message);
                                return null;
                            }

                            output[i, j] = item.ToString();
                        }
                        else
                        {
                            output[i, j] = item.ToString();
                        }
                    }

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
                return GetData();
            }
        }

        internal object[][] GetData(bool convertToString = false)
        {
            var vals = ws.UsedRange.get_Value();

            // if worksheet is empty
            if (vals == null)
                return new object[0][];

            // if worksheet contains a single value
            if (!vals.GetType().IsArray)
                return new object[][] { new object[] { vals } };

            return ConvertToJaggedArray((object[,])vals, convertToString);
        }

        private WorkBook wb = null;
        private Worksheet ws = null;

        /// <summary>
        /// create new worksheet from given workbook and name (AddExcelWorksheetToWorkbook node)
        /// </summary>
        /// <param name="wbook"></param>
        /// <param name="sheetName"></param>
        /// <param name="overWrite"></param>
        internal WorkSheet(WorkBook wbook, string sheetName, bool overWrite = false)
        {
            wb = wbook;

            // Look for an existing worksheet
            WorkSheet[] worksheets = wbook.WorkSheets;
            WorkSheet wSheet = worksheets.FirstOrDefault(n => n.ws.Name == sheetName);

            if (wSheet == null)
            {
                // If you don't find one, create one.
                ws = (Worksheet) wb.Add();
                ws.Name = sheetName;
                wb.Save();
                return;
            }
            
            // If you find one, then use it.
            if (overWrite)
            {
                // if there is only one worksheet, we need to add one more
                // before we can delete the first one
                ws = (Worksheet) wb.Add();
                wSheet.ws.Delete();
                ws.Name = sheetName;
                wb.Save();

            }
            else
                ws = wSheet.ws;
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

            if (rangeData == null)
                return this;

            var c1 = (Range)ws.Cells[startRow + 1, startColumn + 1];
            var c2 = (Range)ws.Cells[startRow + numRows, startColumn + numColumns];
            var range = ws.Range[c1, c2];
            range.Value = rangeData;

            wb.Save();
            return this;
        }

    }

    [IsVisibleInDynamoLibrary(false)]
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
            if (!String.IsNullOrEmpty(wb.Path))
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
                    // Look for an existing open workbook
                    var workbookOpen = ExcelInterop.App.Workbooks.Cast<Workbook>()
                        .FirstOrDefault(e => e.FullName == filePath);

                    // Use the existing workbook.
                    if (workbookOpen != null)
                    {
                        wb = workbookOpen;
                    }
                    // If you can't find an existing workbook at
                    // the specified path, then create a new one.
                    else
                    {
                        Workbook workbook = ExcelInterop.App.Workbooks.Open(filePath);
                        wb = workbook;
                        wb.Save();
                    }
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
