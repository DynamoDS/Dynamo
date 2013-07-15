using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Open Excel Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Opens an Excel file and returns the Workbook inside.  If the filename does not exist, returns null.")]
    public class dynReadExcelFile : dynFileReaderBase
    {

        private static Microsoft.Office.Interop.Excel.Application _excel;

        public dynReadExcelFile()
        {
            _excel = new Microsoft.Office.Interop.Excel.Application();
            OutPortData.Add(new PortData("workbook", "The workbook opened from the file", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            storedPath = ((FScheme.Value.String)args[0]).Item;

            if (File.Exists(storedPath))
            {
                var workbook = _excel.Workbooks.Open(storedPath);
                return FScheme.Value.NewContainer(workbook);
            }

            return FScheme.Value.NewContainer(null);
        }

    }

    [NodeName("Get Worksheets From Excel Workbook")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Get the list of Worksheets from an Excel Workbook.")]
    public class dynGetWorksheetsFromWorkbook : dynNodeWithOneOutput
    {

        public dynGetWorksheetsFromWorkbook()
        {
            InPortData.Add(new PortData("workbook", "The excel workbook", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("index", "Index of the worksheet to get", typeof(FScheme.Value.Number)));
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

    [NodeName("Get Excel Worksheet By Index")]
    [NodeCategory(BuiltinNodeCategories.IO_FILE)]
    [NodeDescription("Get a particular Worksheet from an Excel Workbook by index.")]
    public class dynGetExcelWorksheetByIndex : dynNodeWithOneOutput
    {

        public dynGetExcelWorksheetByIndex()
        {
            InPortData.Add(new PortData("workbook", "The excel workbook", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("index", "Index of the worksheet to get", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("worksheet", "The worksheet at the given index", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var workbook = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var index = (int)Math.Round(((FScheme.Value.Number)args[1]).Item);
            var sheet = workbook.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().ElementAtOrDefault(index);

            return FScheme.Value.NewContainer(sheet);
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
            InPortData.Add(new PortData("name", "Name of the worksheet to get", typeof(FScheme.Value.Number)));
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
                    row.Add(FScheme.Value.NewContainer(range.Cells[r, c].Value2));
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
            InPortData.Add(new PortData("worksheet", "The Excel Worksheet to write to", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("row", "Row index to insert data", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("col", "Column index to insert data", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("value", "Data to add", typeof(FScheme.Value.Container)));

            OutPortData.Add(new PortData("worksheet", "The modified excel worksheet", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)((FScheme.Value.Container)args[0]).Item;
            var row = (int)Math.Round(((FScheme.Value.Number)args[1]).Item);
            var col = (int)Math.Round(((FScheme.Value.Number)args[2]).Item);
            var data = ((FScheme.Value.Container)args[3]).Item;

            worksheet.Cells[row, col] = data;

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
            InPortData.Add(new PortData("name", "Name of new worksheet to add", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("workbook", "The excel Workbook after adding the Worksheet", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var wb = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;

            var ws = new Microsoft.Office.Interop.Excel.Worksheet();
            ws.Name = name;
            wb.Worksheets.Add(ws);

            return FScheme.Value.NewContainer(wb);
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

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var wb = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[0]).Item;
            var name = (Microsoft.Office.Interop.Excel.Workbook)((FScheme.Value.Container)args[1]).Item;
            wb.SaveAs(name);
            return FScheme.Value.NewContainer(wb);
        }

    }
}
