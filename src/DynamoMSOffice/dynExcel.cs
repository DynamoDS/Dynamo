using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

namespace Dynamo.Nodes
{

    public class ExcelInterop {

        private static Microsoft.Office.Interop.Excel.Application _excelApp;
        public static Microsoft.Office.Interop.Excel.Application ExcelApp
        {
            get
            {
                _excelApp = RegisterAndGetApp();
                return _excelApp;
            }
        }

        public static Application RegisterAndGetApp()
        {
            Application excel = null;

            try
            {
                excel = (Excel.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch (COMException)
            {
            }
            if (excel == null) excel = new Microsoft.Office.Interop.Excel.Application();
            if (excel == null)
            {
                throw new Exception("Excel could not be opened.");
            }

            dynSettings.Controller.DynamoModel.CleaningUp += DynamoModelOnCleaningUp;

            excel.Visible = true;

            return excel;
        }

        private static void DynamoModelOnCleaningUp(object sender, EventArgs eventArgs)
        {
            _excelApp.Workbooks.Cast<Workbook>().ToList().ForEach((wb) => wb.Close());
            _excelApp.Quit();
            Marshal.ReleaseComObject(_excelApp);
        }

    }

    [NodeName("Open Excel Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Opens an Excel file and returns the Workbook inside.  If the filename does not exist, returns null.")]
    public class dynReadExcelFile : dynFileReaderBase
    {

        public dynReadExcelFile()
        {
            OutPortData.Add(new PortData("workbook", "The workbook opened from the file", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            storedPath = ((FScheme.Value.String)args[0]).Item;

            var workbookOpen =
                ExcelInterop.ExcelApp.Workbooks.Cast<Microsoft.Office.Interop.Excel.Workbook>().FirstOrDefault(e => e.FullName == storedPath);

            if (workbookOpen != null)
            {
                return FScheme.Value.NewContainer(workbookOpen);
            }

            if (File.Exists(storedPath))
            {
                var workbook = ExcelInterop.ExcelApp.Workbooks.Open(storedPath, true, false);
                return FScheme.Value.NewContainer(workbook);
            }

            return FScheme.Value.NewContainer(null);
        }

    }

    [NodeName("Get Worksheets From Excel Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Get the list of Worksheets from an Excel Workbook.")]
    public class dynGetWorksheetsFromExcelWorkbook : dynNodeWithOneOutput
    {

        public dynGetWorksheetsFromExcelWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The excel workbook", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("worksheets", "A list of worksheets", typeof(FScheme.Value.List)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var l = workbook.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(FScheme.Value.NewContainer);

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(l));
        }

    }

    [NodeName("Get Excel Worksheet By Name")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Gets the first Worksheet in an Excel Workbook with the given name.")]
    public class dynGetExcelWorksheetByName : dynNodeWithOneOutput
    {

        public dynGetExcelWorksheetByName()
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

            return FScheme.Value.NewContainer(sheet);
        }

    }

    [NodeName("Get Data From Excel Worksheet")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Get the non-empty range of Cell data from an Excel Worksheet.")]
    public class dynGetDataFromExcelWorksheet : dynNodeWithOneOutput
    {

        public dynGetDataFromExcelWorksheet()
        {
            InPortData.Add(new PortData("worksheet", "The excel workbook", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("worksheet", "The worksheet with the given name", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)((FScheme.Value.Container)args[0]).Item;

            Microsoft.Office.Interop.Excel.Range range = worksheet.UsedRange;
            
            int rows = range.Rows.Count;
            int cols = range.Columns.Count;

            var rowData = new List<FScheme.Value>();

            for (int r = 1; r <= rows; r++)
            {
                var row = new List<FScheme.Value>();

                for (int c = 1; c <= cols; c++)
                {
                    // try parsing the numbers as doubles
                    // if that doesn't work, send out their string rep.
                    double val;
                    row.Add(double.TryParse(range.Cells[r, c].Value2.ToString(), out val)
                                ? FScheme.Value.NewNumber(val)
                                : FScheme.Value.NewString(range.Cells[r, c].Value2.ToString()));
                }

                rowData.Add(FScheme.Value.NewList(Utils.SequenceToFSharpList(row)));
            }

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(rowData));
        }

    }

    [NodeName("Write Data To Excel Worksheet")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Write data to a Cell of an Excel Worksheet.")]
    public class dynWriteDataToExcelWorksheet : dynNodeWithOneOutput
    {

        public dynWriteDataToExcelWorksheet()
        {
            InPortData.Add(new PortData("worksheet", "The Excel Worksheet to write to.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("start row", "Row index to insert data.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("start column", "Column index to insert data.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("data", "A list of data to add.", typeof(FScheme.Value.List)));

            OutPortData.Add(new PortData("worksheet", "The modified excel worksheet", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)((FScheme.Value.Container)args[0]).Item;
            var rowStart = (int)Math.Round(((FScheme.Value.Number)args[1]).Item);
            var colStart = (int)Math.Round(((FScheme.Value.Number)args[2]).Item);
            //object data;

            if(!args[3].IsList)
                throw new Exception("A list of data must be provided to set a range in Excel.");

            // assume a list of lists
            // get the dimension of the first object

            var data = ((FScheme.Value.List) args[3]).Item;
            var rowCount = data.Count();
            var colCount = 0;

            if (!data[0].IsList)
                colCount = 1;
            else
            {
                var firstList = ((FScheme.Value.List)data[0]).Item;
                colCount = firstList.Count();
            }
            
            var rangeData = new object[rowCount,colCount];
            for (int i = 0; i < rowCount; i++)
            {
                var row = ((FScheme.Value.List)data[i]).Item;

                for (int j = 0; j < colCount; j++)
                {
                    if (row[j] is FScheme.Value.String)
                    {
                        rangeData[i, j] = ((FScheme.Value.String)row[j]).Item;
                    }
                    else if (row[j] is FScheme.Value.Number)
                    {
                        rangeData[i, j] = ((FScheme.Value.Number) row[j]).Item;
                    }
                    else
                    {
                        // it's not a string or a number, we don't know what
                        // to do with it.
                        rangeData[i, j] = "";
                    }
                }
            }

            var range = worksheet.Range[worksheet.Cells[rowStart, colStart], worksheet.Cells[rowStart + rowCount-1, colStart + colCount-1]];

            //worksheet.Cells[row, col] = data;
            range.Value = rangeData;

            return FScheme.Value.NewContainer(worksheet);
        }

    }

    [NodeName("Add Excel Worksheet To Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Add a new Worksheet to a Workbook with a given name.")]
    public class dynAddExcelWorksheetToWorkbook : dynNodeWithOneOutput
    {

        public dynAddExcelWorksheetToWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The Excel Worksheet to write to", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("name", "Name of new Worksheet to add", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("worksheet", "The Worksheet newly added to the Workbook", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var wb = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;

            var worksheet = wb.Worksheets.Add();
            worksheet.Name = name;

            return FScheme.Value.NewContainer(wb);
        }

    }

    [NodeName("New Excel Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Create a new Excel Workbook object.")]
    public class dynNewExcelWorkbook : dynNodeWithOneOutput
    {

        public dynNewExcelWorkbook()
        {
            OutPortData.Add(new PortData("workbook", "The new Excel Workbook ", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = ExcelInterop.ExcelApp.Workbooks.Add();
            return FScheme.Value.NewContainer(workbook);
        }

    }

    [NodeName("Save Excel Workbook As")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Write an Excel Workbook to a file with the given filename.")]
    public class dynSaveAsExcelWorkbook : dynNodeWithOneOutput
    {

        public dynSaveAsExcelWorkbook()
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

    }
}
