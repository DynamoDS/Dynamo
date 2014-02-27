using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

namespace Dynamo.Nodes
{

    public class ExcelInterop {

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
        public static bool ShowOnStartup { 
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
                excel = (Excel.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch (COMException e)
            {
                // 0x800401E3 - the excel process simply was not running, we continue if we
                // encounter this exception

                if ( !e.ToString().Contains("0x800401E3") )
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
            get { return _app != null;  }
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

    [NodeName("New Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Create a new Excel Workbook object.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class NewExcelWorkbook : NodeWithOneOutput
    {

        public NewExcelWorkbook()
        {
            OutPortData.Add(new PortData("workbook", "The new Excel Workbook ", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = ExcelInterop.App.Workbooks.Add();
            workbook.BeforeClose += WorkbookOnBeforeClose;

            return FScheme.Value.NewContainer(workbook);
        }

        private void WorkbookOnBeforeClose(ref bool cancel)
        {
            this.isDirty = true;
            this.MarkDirty();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.NewExcelWorkbook",
                "Excel.NewExcelWorkbook");
        }
    }

    [NodeName("Open Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Opens an Excel file and returns the Workbook inside.  If the filename does not exist, returns null.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class ReadExcelFile : FileReaderBase
    {

        public ReadExcelFile()
        {
            OutPortData.Add(new PortData("workbook", "The workbook opened from the file", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            storedPath = ((FScheme.Value.String)args[0]).Item;

            var workbookOpen = 
                ExcelInterop.App.Workbooks.Cast<Workbook>().FirstOrDefault(e => e.FullName == storedPath);

            if (workbookOpen != null)
            {
                workbookOpen.BeforeClose += WorkbookOnBeforeClose;
                return FScheme.Value.NewContainer(workbookOpen);
            }

            if (File.Exists(storedPath))
            {
                var workbook = ExcelInterop.App.Workbooks.Open(storedPath, true, false);
                workbook.BeforeClose += WorkbookOnBeforeClose;
                return FScheme.Value.NewContainer(workbook);
            }

            return FScheme.Value.NewContainer(null);
        }

        private void WorkbookOnBeforeClose(ref bool cancel)
        {
            this.isDirty = true;
            this.MarkDirty();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.ReadExcelFile",
                "Excel.ReadExcelFile@string");
        }
    }

    [NodeName("Get Worksheets From Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Get the list of Worksheets from an Excel Workbook.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class GetWorksheetsFromExcelWorkbook : NodeWithOneOutput
    {

        public GetWorksheetsFromExcelWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The excel workbook", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("worksheets", "A list of worksheets", typeof(FScheme.Value.List)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = (Workbook)((FScheme.Value.Container)args[0]).Item;
            var l = workbook.Worksheets.Cast<Worksheet>().Select(FScheme.Value.NewContainer);

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(l));
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetWorksheetsFromExcelWorkbook",
                "Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    [NodeName("Get Excel Worksheet By Name")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Gets the first Worksheet in an Excel Workbook with the given name.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class GetExcelWorksheetByName : NodeWithOneOutput
    {

        public GetExcelWorksheetByName()
        {
            InPortData.Add(new PortData("workbook", "The excel workbook", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("name", "Name of the worksheet to get", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("worksheet", "The worksheet with the given name", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;
            var sheet = workbook.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().FirstOrDefault(ws => ws.Name == name);

            if (sheet == null)
            {
                throw new Exception("Could not find a worksheet in the workbook with that name.");
            }

            return FScheme.Value.NewContainer(sheet);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetExcelWorksheetByName",
                "Excel.GetExcelWorksheetByName@var,string");
        }
    }

    [NodeName("Get Data From Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Get the non-empty range of Cell data from an Excel Worksheet.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class GetDataFromExcelWorksheet : NodeWithOneOutput
    {

        public GetDataFromExcelWorksheet()
        {
            InPortData.Add(new PortData("worksheet", "The excel workbook", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("worksheet", "The worksheet with the given name", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)((FScheme.Value.Container)args[0]).Item;

            var vals = worksheet.UsedRange.get_Value();
            var rowData = new List<FScheme.Value>();

            // rowData can potentially return a single value rather than an array
            if (!(vals is object[,]))
            {
                var row = new List<FScheme.Value>() { TryParseCell(vals) };
                rowData.Add(FScheme.Value.NewList(Utils.SequenceToFSharpList(row)));
                return FScheme.Value.NewList(Utils.SequenceToFSharpList(rowData));
            }

            int rows = vals.GetLength(0);
            int cols = vals.GetLength(1);

            // transform into 2d FScheme.Value array
            for (int r = 1; r <= rows; r++)
            {
                var row = new List<FScheme.Value>();

                for (int c = 1; c <= cols; c++)
                {
                    row.Add(TryParseCell(vals[r, c]));
                }

                rowData.Add(FScheme.Value.NewList(Utils.SequenceToFSharpList(row)));
            }

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(rowData));
        }

        public static FScheme.Value TryParseCell(object element)
        {
            if (element == null )
            {
                return FScheme.Value.NewContainer(null);
            }
                
            double val;
            return double.TryParse(element.ToString(), out val)
                ? FScheme.Value.NewNumber(val)
                : FScheme.Value.NewString(element.ToString());
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetDataFromExcelWorksheet",
                "Excel.GetDataFromExcelWorksheet@var");
        }
    }

    [NodeName("Write Data To Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Write data to a Cell of an Excel Worksheet.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class WriteDataToExcelWorksheet : NodeWithOneOutput
    {

        public WriteDataToExcelWorksheet()
        {
            InPortData.Add(new PortData("worksheet", "The Excel Worksheet to write to.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("start row", "Row index to insert data.", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("start column", "Column index to insert data.", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("data", "A single item, a 1d list, or a 2d list to write to the worksheet", typeof(FScheme.Value.List)));

            OutPortData.Add(new PortData("worksheet", "The modified excel worksheet", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        #region Helper methods

        public List<List<FScheme.Value>> ConvertTo2DList(FScheme.Value v)
        {
            if (v.IsList)
            {
                return ((FScheme.Value.List)v).Item.Select(ConvertRow).ToList();
            }

            var list = new List<List<FScheme.Value>>();
            list.Add(ConvertRow(v));
            return list;
        }

        public static List<FScheme.Value> ConvertRow(FScheme.Value v)
        {
            if (v.IsString || v.IsNumber)
            {
                return new List<FScheme.Value>() {v};
            }
            else if (v.IsList)
            {
                return ((FScheme.Value.List) v).Item.Select(ConvertAsAtom).ToList();
            }
            else
            {
                return new List<FScheme.Value>() {ConvertAsAtom(v)};
            }
        }

        public static FScheme.Value ConvertAsAtom(FScheme.Value v)
        {
            if (v == null)
            {
                return FScheme.Value.NewString("");
            } 
            else if (v.IsString || v.IsNumber)
            {
                return v;
            }
            else
            {
                return FScheme.Value.NewString(v.ToString());
            }
        }

        public static void GetObjectList(List<List<FScheme.Value>> input, out object[,] output, out int numRows,
                                   out int numCols)
        {
            numRows = input.Count;
            numCols = input.Select(x => x.Count).Max();
            output = new object[numRows, numCols];
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    if (j >= input[i].Count )
                    {
                        output[i, j] = "";
                    }
                    else if (input[i][j] is FScheme.Value.String)
                    {
                        output[i, j] = ((FScheme.Value.String)input[i][j]).Item;
                    }
                    else if (input[i][j] is FScheme.Value.Number)
                    {
                        output[i, j] = ((FScheme.Value.Number)input[i][j]).Item;
                    }
                }
            }
        }

        #endregion

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var worksheet = (Worksheet)((FScheme.Value.Container)args[0]).Item;

            // clear existing elements for dynamic update
            var usedRange = worksheet.UsedRange;
            usedRange.Cells.ClearContents();

            // excel does not use zero based indexing (ugh)
            // I would like to hide this from the user, though.
            var rowStart = Math.Max(1, (int) Math.Round(((FScheme.Value.Number) args[1]).Item) + 1);
            var colStart = Math.Max(1, (int) Math.Round(((FScheme.Value.Number) args[2]).Item) + 1);

            var data = ConvertTo2DList(args[3]);
            object[,] rangeData;
            int rowCount, colCount;
            GetObjectList(data, out rangeData, out rowCount, out colCount);

            var c1 = (Excel.Range)worksheet.Cells[rowStart, colStart];
            var c2 = (Excel.Range)worksheet.Cells[rowStart + rowCount - 1, colStart + colCount - 1];
            var range = worksheet.get_Range(c1, c2);
            range.Value = rangeData;

            return FScheme.Value.NewContainer(worksheet);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.WriteDataToExcelWorksheet",
                "Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");
        }
    }

    [NodeName("Add Excel Worksheet To Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Add a new Worksheet to a Workbook with a given name.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class AddExcelWorksheetToWorkbook : NodeWithOneOutput
    {

        public AddExcelWorksheetToWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The Excel Worksheet to write to", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("name", "Name of new Worksheet to add", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("worksheet", "The Worksheet newly added to the Workbook", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var wb = (Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;

            var worksheet = wb.Worksheets.Add();
            worksheet.Name = name;

            return FScheme.Value.NewContainer(worksheet);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.AddExcelWorksheetToWorkbook",
                "Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    [NodeName("Save Excel Workbook As")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("Write an Excel Workbook to a file with the given filename.  \n\nThis node requires Microsoft Excel to be installed.")]
    public class SaveAsExcelWorkbook : NodeWithOneOutput
    {

        public SaveAsExcelWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The Excel Workbook to save", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("filename", "Filename to save the Workbook to", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("workbook", "The Excel Workbook", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get { return true; }
            set
            {

            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var wb = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;

            if (wb.FullName == name)
            {
                wb.Save();
            }
            else
            {
                wb.SaveAs(name);
            }
            
            return FScheme.Value.NewContainer(wb);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.SaveAsExcelWorkbook",
                "Excel.SaveAsExcelWorkbook@var,string");
        }
    }
}
