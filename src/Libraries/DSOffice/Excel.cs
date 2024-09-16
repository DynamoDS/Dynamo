using System;
using System.IO;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace DSOffice
{
    /// <summary>
    /// Methods for interacting with Excel 
    /// </summary>
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
        /// <param name="showExcel">toggle to switch between showing and hiding the main Excel window</param>
        /// <returns name="data">Rows of data from the Excel worksheet.</returns>
        /// <search>office,excel,spreadsheet,ifequalreturnindex</search>
        [IsVisibleInDynamoLibrary(false)]
        public static object[][] ReadFromFile(FileInfo file, string sheetName, bool readAsStrings = false, bool showExcel = true)
        {
            object[][] data;

            if (!showExcel)
            {
                ExcelInterop.ShowOnStartup = false;
            }
            WorkBook wb = WorkBook.ReadExcelFile(file.FullName);
            WorkSheet ws = wb.GetWorksheetByName(sheetName);
            if (readAsStrings)
            {
                data = ws.GetData(true);
            }
            else
            {
                data = ws.Data;
            }
            if (!showExcel)
            {
                wb.CloseHidden();
                ExcelInterop.ShowOnStartup = true;
            }

            return data;
        }

        [NodeObsolete("ReadObsolete", typeof(Properties.Resources))]
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
        /// <param name="startRow">Start row for writing data. Enter 0 for Row 1, 1 for Row 2, etc.</param>
        /// <param name="startCol">
        ///     Start column for writing data. Enter 0 for Column A, 1 for Column B, etc.
        /// </param>
        /// <param name="data">Data to write to the spreadsheet.</param>
        /// <param name="overWrite"></param>
        /// <returns name="data">Data written to the spreadsheet.</returns>
        /// <search>office,excel,spreadsheet</search>
        [IsVisibleInDynamoLibrary(false)]
        public static object[][] WriteToFile(string filePath, string sheetName, int startRow, int startCol, object[][] data, bool overWrite = false)
        {
            return ExcelWriteUtils.WriteData(filePath, sheetName, startRow, startCol, data, overWrite);
        }
    }

    /// <summary>
    ///     Methods for Import/Export category.
    /// </summary>
    public static partial class Data
    {
        /// <summary>
        ///     Read data from a Microsoft Excel spreadsheet. Data is read by row and
        ///     returned in a series of lists by row. Rows and columns are zero-indexed;
        ///     for example, the value in cell A1 will appear in the data list at [0,0].
        ///     This node requires Microsoft Excel to be installed.
        /// </summary>
        /// <param name="file">File representing the Excel workbook</param>
        /// <param name="sheetName">Name of the worksheet containing data</param>
        /// <param name="readAsStrings">Toggle to read cells as strings</param>
        /// <param name="showExcel">Toggle to show excel's main window</param>
        /// <returns name="data">Rows of data from the Excel worksheet</returns>
        /// <search>office,excel,spreadsheet,ifequalreturnindex</search>
        public static object[][] ImportExcel(FileInfo file, string sheetName, bool readAsStrings = false, bool showExcel = true)
        {
            return Excel.ReadFromFile(file, sheetName, readAsStrings, showExcel);
        }

        /// <summary>
        ///     Write data to a Microsoft Excel spreadsheet. Data is written by row
        ///     with sublists to be written in successive rows. Rows and columns are
        ///     zero-indexed; for example, the value in the data list at [0,0] will
        ///     be written to cell A1. Null values and empty lists are written to Excel 
        ///     as empty cells. This node requires Microsoft Excel to be installed. 
        /// </summary>
        /// <param name="filePath">File path to the Microsoft Excel spreadsheet</param>
        /// <param name="sheetName">Name of the workseet to write data to</param>
        /// <param name="startRow">Start row for writing data. Enter 0 for Row 1, 1 for Row 2, etc.</param>
        /// <param name="startColumn">
        ///     Start column for writing data. Enter 0 for Column A, 1 for Column B, etc.
        /// </param>
        /// <param name="data">Data to write to the spreadsheet</param>
        /// <param name="overWrite">True to overwrite file, false not to overwrite</param>
        /// <returns name="data">Data written to the spreadsheet</returns>
        /// <search>office,excel,spreadsheet</search>
        [Obsolete("Use ExportToExcel instead.")] 
        public static object[][] ExportExcel(string filePath, string sheetName, int startRow, int startColumn, object[][] data, bool overWrite = false)
        {
            return ExcelWriteUtils.WriteData(filePath, sheetName, startRow, startColumn, data, overWrite);
        }

        /// <summary>
        ///     Write data to a Microsoft Excel spreadsheet. Data is written by row
        ///     with sublists to be written in successive rows. Rows and columns are
        ///     zero-indexed; for example, the value in the data list at [0,0] will
        ///     be written to cell A1. Null values and empty lists are written to Excel 
        ///     as empty cells. This node requires Microsoft Excel to be installed. 
        /// </summary>
        /// <param name="filePath">File representing the Excel workbook</param>
        /// <param name="sheetName">Name of the worksheet containing data</param>
        /// <param name="startRow">Start row for writing data. Enter 0 for Row 1, 1 for Row 2, etc.</param>
        /// <param name="startColumn">
        ///     Start column for writing data. Enter 0 for Column A, 1 for Column B, etc.
        /// </param>
        /// <param name="data">Data to write to the spreadsheet</param>
        /// <param name="overWrite"> Toggle to clear spreadsheet before writing</param>
        /// <param name="writeAsString">Toggle to switch between writing Excel file as strings</param>
        /// <returns name="data">Rows of data from the Excel worksheet</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] ExportToExcel(string filePath, string sheetName, int startRow, int startColumn, object[][] data, bool overWrite = false, bool writeAsString = false)
        {
            return ExcelWriteUtils.WriteData(filePath, sheetName, startRow, startColumn, data, overWrite, writeAsString);
        }

        /// <summary>
        /// Read data from a Microsoft Excel spreadsheet by using the Open XML standard.
        /// Data is read by row and returned in a series of lists by row.
        /// Rows and columns are zero-indexed; for example, the value in cell A1 will
        /// appear in the data list at [0,0].
        /// </summary>
        /// <param name="filePath">File representing the Excel workbook</param>
        /// <param name="sheetName">Name of the worksheet containing data</param>
        /// <param name="startRow">Start row for reading data. Enter 0 for Row 1, 1 for Row 2, etc.</param>
        /// <param name="startColumn">Start column for reading data. Enter 0 for Column A, 1 for Column B, etc.</param>
        /// <param name="readAsString">Toggle to read cells as strings</param>
        /// <returns name="data">Rows of data from the Excel worksheet</returns>
        /// <search>office,excel,spreadsheet</search>
        public static object[][] OpenXMLImportExcel(string filePath, string sheetName, int startRow = 0, int startColumn = 0, bool readAsString = false)
        {
            return OpenXmlHelper.Read(filePath, sheetName, startRow, startColumn, readAsString);
        }

        /// <summary>
        /// Write data to a Microsoft Excel spreadsheet by using the Open XML standard.
        /// Data is written by row with sublists to be written in successive rows.
        /// Rows and columns are zero-indexed; for example, the value in the data list at [0,0] will
        /// be written to cell A1. Null values and empty lists are written as empty cells.
        /// </summary>
        /// <param name="filePath">File representing the Excel workbook</param>
        /// <param name="sheetName">Name of the worksheet containing data</param>
        /// <param name="startRow">Start row for writing data. Enter 0 for Row 1, 1 for Row 2, etc.</param>
        /// <param name="startColumn">Start column for writing data. Enter 0 for Column A, 1 for Column B, etc.</param>
        /// <param name="data">Data to write to the spreadsheet</param>
        /// <param name="overWrite"> Toggle to clear spreadsheet before writing</param>
        /// <param name="writeAsString">Toggle to switch between writing cell values as strings</param>
        /// <search>office,excel,spreadsheet</search>
        /// <returns>Boolean indicating if writing to spreadsheet is successful.</returns>
        public static bool OpenXMLExportExcel(string filePath, string sheetName, object[][] data, int startRow = 0, int startColumn = 0, bool overWrite = false, bool writeAsString = false)
        {
            return OpenXmlHelper.Write(filePath, sheetName, data, startRow, startColumn, overWrite, writeAsString);
        }
    }
}
