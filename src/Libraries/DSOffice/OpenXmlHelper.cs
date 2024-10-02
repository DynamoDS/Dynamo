using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DSOffice
{
    /// <summary>
    /// Deals with reading and writing Excel files using the Open XML SDK.
    /// </summary>
    internal class OpenXmlHelper
    {
        /// <summary>
        /// Reads data from an Excel spreadsheet using the Open XML SDK.
        /// </summary>
        /// <param name="filePath">Path to the Excel workbook file</param>
        /// <param name="sheetName">Name of the sheet to read from inside the workbook</param>
        /// <param name="startRow">Row where to begin reading from (zero-based)</param>
        /// <param name="startColumn">Column where to begin reading from (zero-based)</param>
        /// <param name="readAsString">Whether to read cell values as strings or not</param>
        /// <returns>Cell values read from the spreadsheet</returns>
        internal static object[][] Read(string filePath, string sheetName, int startRow, int startColumn, bool readAsString)
        {
            // While SpreadsheetDocument.Open handles this, the error message it throws is not localized.
            if (!File.Exists(filePath))
            {
                throw new ArgumentException(string.Format(Properties.Resources.WorkbookNotFound, filePath));
            }

            using (var document = SpreadsheetDocument.Open(filePath, false))
            {
                var sheet = GetWorksheetPart(document, sheetName);
                if (sheet == null)
                {
                    throw new ArgumentException(string.Format(Properties.Resources.WorksheetNotFound, sheetName));
                }
                var stylesheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
                var sharedStringTable = document.WorkbookPart.SharedStringTablePart?.SharedStringTable;
                var rowsData = new List<object[]>();

                // Takes into account holes at the beginning or in between rows and cells
                var currentRowIndex = startRow + 1;
                var columnCount = GetLargestColumnIndex(sheet.Worksheet.GetFirstChild<SheetData>()) - startColumn;
                foreach (var row in sheet.Worksheet.GetFirstChild<SheetData>().Elements<Row>())
                {
                    if (row.RowIndex.Value < currentRowIndex)
                    {
                        // Skip rows until we get to where we should start
                        continue;
                    }

                    while (row.RowIndex.Value > currentRowIndex)
                    {
                        // Add empty rows to fill gap. The column count used is the largest one found.
                        rowsData.Add(new object[columnCount]);
                        currentRowIndex++;
                    }

                    var rowData = new List<object>();
                    var currentColumnIndex = startColumn + 1;
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        var columnIndex = GetColumnIndex(cell.CellReference.Value);

                        if (columnIndex < currentColumnIndex)
                        {
                            // Skip columns until we get to where we should start
                            continue;
                        }

                        while (columnIndex > currentColumnIndex)
                        {
                            // Add empty cells to fill gap
                            rowData.Add(null);
                            currentColumnIndex++;
                        }

                        object value = GetCellValue(cell, sharedStringTable, stylesheet, readAsString);
                        rowData.Add(value);
                        currentColumnIndex++;
                    }

                    // Fill remaining columns with empty cells
                    while (currentColumnIndex <= columnCount + startColumn)
                    {
                        rowData.Add(null);
                        currentColumnIndex++;
                    }

                    rowsData.Add(rowData.ToArray());
                    currentRowIndex++;
                }
                return rowsData.ToArray();
            }
        }

        /// <summary>
        /// Writes data to an Excel spreadsheet using the Open XML SDK.
        /// </summary>
        /// <param name="filePath">Path to the Excel workbook file. If it does not exist, a new workbook will be created.</param>
        /// <param name="sheetName">Name of the sheet to write to inside the workbook</param>
        /// <param name="data">Data values to be written to the sheet's cells</param>
        /// <param name="startRow">Row where to begin writing from (zero-based)</param>
        /// <param name="startColumn">Column where to begin writing from (zero-based)</param>
        /// <param name="overWrite">Whether the sheet should be re-created before writing</param>
        /// <param name="writeAsString">Whether data values should be written as strings or not</param>
        /// <returns>Boolean indicating if writing to spreadsheet is successful.</returns>
        internal static bool Write(string filePath, string sheetName, object[][] data, int startRow, int startColumn, bool overWrite, bool writeAsString)
        {
            using (var document = OpenOrCreate(filePath, sheetName))
            {
                var sheet = GetWorksheetPart(document, sheetName);
                if (sheet == null)
                {
                    sheet = AddWorksheetPart(document, sheetName);
                }
                else if (overWrite)
                {
                    RemoveWorksheetPart(document, sheet);
                    sheet = AddWorksheetPart(document, sheetName);
                }

                var stylesheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
                // Add a shared string table if one does not exist
                if (document.WorkbookPart.SharedStringTablePart == null)
                {
                    document.WorkbookPart.AddNewPart<SharedStringTablePart>();
                    document.WorkbookPart.SharedStringTablePart.SharedStringTable = new SharedStringTable() { Count = 0, UniqueCount = 0 };
                }
                var sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                // Special case: When data is an empty list or list of empty lists
                // then it is treated as a single empty value.
                if (data.Length == 0 || data.Select(r => r == null ? 0 : r.Length).Max() == 0)
                {
                    data = new object[][] { new object[] { null } };
                }

                // Special case: When data contains StackValue a warning is raised
                // and data is treated as null
                if (ContainsFunction(data))
                {
                    string message = string.Format(Properties.Resources.kMethodResolutionFailureWithTypes,
                        "Data.OpenXMLExportExcel", "Function");
                    DynamoServices.LogWarningMessageEvents.OnLogWarningMessage(message);
                    data = new object[0][];
                }

                var currentRowIndex = (uint)startRow + 1;
                var sheetData = sheet.Worksheet.GetFirstChild<SheetData>();
                var rows = new List<Row>(sheetData.Elements<Row>());
                var rowsIndex = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    // Skip rows until the start row
                    while (rowsIndex < rows.Count && rows[rowsIndex].RowIndex < currentRowIndex)
                    {
                        rowsIndex++;
                    }

                    Row row;
                    if (rowsIndex >= rows.Count)
                    {
                        // Add a new row to the end
                        row = new Row() { RowIndex = currentRowIndex };
                        sheetData.AppendChild(row);
                    }
                    else if (rows[rowsIndex].RowIndex > currentRowIndex)
                    {
                        // Add a new row before this one
                        row = new Row() { RowIndex = currentRowIndex };
                        sheetData.InsertBefore(rows[rowsIndex], row);
                    }
                    else
                    {
                        // Overwrite the existing row
                        row = rows[rowsIndex];
                        rowsIndex++;
                    }

                    if (data[i] == null || data[i].Length == 0)
                    {
                        // Special case: when there is no row data, delete the entire row
                        sheetData.RemoveChild(row);
                        currentRowIndex++;
                        continue;
                    }

                    var currentColumnIndex = startColumn + 1;
                    var cells = new List<Cell>(row.Elements<Cell>());
                    var cellsIndex = 0;
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        // Skip cells until the start column
                        while (cellsIndex < cells.Count && GetColumnIndex(cells[cellsIndex].CellReference) < currentColumnIndex)
                        {
                            cellsIndex++;
                        }

                        Cell cell;
                        if (cellsIndex >= cells.Count)
                        {
                            // Add a new cell to the end
                            cell = new Cell() { CellReference = GetCellReference(currentColumnIndex, currentRowIndex) };
                            row.AppendChild(cell);
                        }
                        else if (GetColumnIndex(cells[cellsIndex].CellReference) > currentColumnIndex)
                        {
                            // Add a new cell before this one
                            cell = new Cell() { CellReference = GetCellReference(currentColumnIndex, currentRowIndex) };
                            row.InsertBefore(cells[cellsIndex], cell);
                        }
                        else
                        {
                            // Overwrite the existing cell
                            cell = cells[cellsIndex];
                            cellsIndex++;
                        }

                        if (data[i][j] == null)
                        {
                            // Special case: when there is no cell data, delete the cell
                            row.RemoveChild(cell);
                            currentColumnIndex++;
                            continue;
                        }
                  
                        SetCellValue(data[i][j], cell, sharedStringTable, stylesheet, writeAsString);
                        currentColumnIndex++;
                    }

                    // Delete the row in case it turns out to be empty
                    if (row.Elements<Cell>().Count() == 0)
                    {
                        sheetData.RemoveChild(row);
                    }

                    currentRowIndex++;
                }
                return true;
            }
        }

        /// <summary>
        /// Given a worksheet's data it returns the largest column index for any rows.
        /// </summary>
        /// <param name="sheetData">Worksheet's data</param>
        /// <returns>The largest column index found</returns>
        private static int GetLargestColumnIndex(SheetData sheetData)
        {
            var result = 1;
            foreach (var row in sheetData.Elements<Row>())
            {
                var lastCell = row.LastChild as Cell;
                var current = GetColumnIndex(lastCell.CellReference);
                if (current > result)
                {
                    result = current;
                }
            }

            return result;
        }

        /// <summary>
        /// Given the column and row indices, like 3 and 7, it returns the cell reference, C7 in this case.
        /// </summary>
        /// <param name="columnIndex">Column index (1-based)</param>
        /// <param name="rowIndex">Row index (1-based)</param>
        /// <returns>The cell reference that matches the provided column and index</returns>
        private static string GetCellReference(int columnIndex, uint rowIndex)
        {
            var letters = new Stack<char>();
            do
            {
                letters.Push((char)('A' + ((columnIndex - 1) % 26)));
                columnIndex = (columnIndex - 1) / 26;
            }
            while (columnIndex > 0);

            return new string(letters.ToArray()) + rowIndex;
        }

        /// <summary>
        /// Given a cell reference, like C7, it returns the column index, C = 3 in this case.
        /// </summary>
        /// <param name="cellReference">A cell reference like C7</param>
        /// <returns>The column index (1-based) equivalent to the letter sequence identifying the cell column</returns>
        private static int GetColumnIndex(string cellReference)
        {
            var result = 0;
            cellReference = cellReference.ToUpper();
            Func<char, bool> isLetter = c => c >= 'A' && c <= 'Z';
            for (var i = 0; i < cellReference.Length && isLetter(cellReference[i]); i++)
            {
                result = result * 26 + cellReference[i] - 'A' + 1;
            }
            return result;
        }

        /// <summary>
        /// Gets the value from the cell, optionally converting it to a string.
        /// </summary>
        /// <param name="cell">Cell to get the value from</param>
        /// <param name="sharedStringTable">Structure of the spreadsheet that contains the actual string values</param>
        /// <param name="stylesheet">Style section of the spreadsheet, containing formatting information</param>
        /// <param name="readAsString">When true, a string will be returned rather than the actual cell value</param>
        /// <returns>The value inside of the cell, possibly converted to a string</returns>
        private static object GetCellValue(Cell cell, SharedStringTable sharedStringTable, Stylesheet stylesheet, bool readAsString)
        {
            if (cell != null && !string.IsNullOrEmpty(cell.InnerText))
            {
                if (cell.DataType == null)
                {
                    // For numbers and dates Office sets the data type as null.
                    // In this case, the type needs to be determined by inspecting the cell format.
                    var numberFormatId = 0;
                    if (cell.StyleIndex != null)
                    {
                        var cellFormat = (CellFormat)stylesheet.CellFormats.ElementAt((int)cell.StyleIndex.Value);
                        numberFormatId = (int)cellFormat.NumberFormatId.Value;
                    }

                    if (14 <= numberFormatId && numberFormatId <= 22 ||
                        45 <= numberFormatId && numberFormatId <= 47)
                    {
                        // This is a date
                        if (cell.CellValue.TryGetDouble(out var number))
                        {
                            var value = DateTime.FromOADate(number);
                            if (readAsString)
                            {
                                return value.ToString();
                            }

                            return value;
                        }

                        return cell.InnerText;
                    }
                    else
                    {
                        // This is a number
                        if (cell.CellValue.TryGetDouble(out var value))
                        {
                            if (readAsString)
                            {
                                if (numberFormatId != 0)
                                {
                                    var numberingFormat = (NumberingFormat)stylesheet.NumberingFormats.ElementAt(numberFormatId);
                                    var formatted = value.ToString(numberingFormat.FormatCode);
                                    return formatted;
                                }

                                return value.ToString();
                            }

                            return value;
                        }

                        return cell.InnerText;
                    }
                }
                else if (cell.DataType.Value == CellValues.SharedString)
                {
                    // In this case, the value is actually an index on the shared string table.
                    // This is the representation used for strings by Office.
                    if (sharedStringTable != null)
                    {
                        if (cell.CellValue.TryGetInt(out var index))
                        {
                            return sharedStringTable.ElementAt(index).InnerText;
                        }
                    }

                    return cell.InnerText;
                }
                else if (cell.DataType.Value == CellValues.Boolean)
                {
                    // For booleans, values 0 or 1 can be converted directly.
                    if (cell.CellValue.TryGetBoolean(out var value))
                    {
                        if (readAsString)
                        {
                            return value.ToString();
                        }

                        return value;
                    }

                    return cell.InnerText;
                }
                else
                {
                    if (cell.CellFormula == null)
                    {
                        // Default to raw string value for inline strings and unknown types.
                        return cell.InnerText;
                    }
                    else
                    {
                        return cell.CellValue.Text;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the worksheet with the specified name from the document.
        /// </summary>
        /// <param name="document">Document where to obtain the worksheet from</param>
        /// <param name="sheetName">Name of the sheet to obtain</param>
        /// <returns>The sheet with the specified name or null if it was not found</returns>
        private static WorksheetPart GetWorksheetPart(SpreadsheetDocument document, string sheetName)
        {
            var sheet = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().FirstOrDefault(s => s.Name == sheetName);
            if (sheet == null)
            {
                return null;
            }
            return document.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart;
        }

        /// <summary>
        /// Checks whether the passed in data contains a function.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool ContainsFunction(object[][] data)
        {
            foreach (var row in data)
            {
                if (row == null)
                {
                    continue;
                }

                foreach (var value in row)
                {
                    if (value is ProtoCore.DSASM.StackValue stackValue && stackValue.IsPointer)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Opens an existing workbook or creates one from scratch on the specified file path.
        /// </summary>
        /// <param name="filePath">File path where the workbook is or should be created</param>
        /// <param name="sheetName">In case a new workbook is created, the sheet will use this name</param>
        /// <returns>A spreadsheet document</returns>
        private static SpreadsheetDocument OpenOrCreate(string filePath, string sheetName)
        {
            if (File.Exists(filePath))
            {
                return SpreadsheetDocument.Open(filePath, true);
            }
            else
            {
                var document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = sheetName
                };
                sheets.Append(sheet);
                
                var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet = new Stylesheet();
                AddDefaultStyles(workbookStylesPart.Stylesheet);
                return document;
            }
        }

        /// <summary>
        /// Adds default styles to the stylesheet so that Office does not identify the spreadsheet as corrupt.
        /// </summary>
        /// <param name="stylesheet">Stylesheet section of a new document</param>
        private static void AddDefaultStyles(Stylesheet stylesheet)
        {
            stylesheet.Fonts = new Fonts() { Count = 1 };
            stylesheet.Fonts.AppendChild(new Font()
            {
                FontName = new FontName() { Val = "Calibri" },
                FontSize = new FontSize() { Val = 11 }
            });

            stylesheet.Fills = new Fills() { Count = 1 };
            stylesheet.Fills.AppendChild(new Fill() { PatternFill = new PatternFill() { PatternType = PatternValues.None } });

            stylesheet.Borders = new Borders() { Count = 1 };
            stylesheet.Borders.AppendChild(new Border()
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            });

            stylesheet.CellStyleFormats = new CellStyleFormats() { Count = 1 };
            stylesheet.CellStyleFormats.AppendChild(new CellFormat()
            {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
            });

            stylesheet.CellFormats = new CellFormats() { Count = 2 };
            stylesheet.CellFormats.AppendChild(new CellFormat()
            {
                FormatId = 0,
                NumberFormatId = 0
            });
            stylesheet.CellFormats.AppendChild(new CellFormat()
            {
                FormatId = 0,
                NumberFormatId = 22,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyNumberFormat = true
            });
        }

        /// <summary>
        /// Removes a worksheet from the document.
        /// </summary>
        /// <param name="document">Document to remove the worksheet from</param>
        /// <param name="worksheetPart">Worksheet to be removed</param>
        private static void RemoveWorksheetPart(SpreadsheetDocument document, WorksheetPart worksheetPart)
        {
            var relId = document.WorkbookPart.GetIdOfPart(worksheetPart);
            document.WorkbookPart.DeletePart(worksheetPart);

            var sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            var sheet = sheets.FirstOrDefault(s => (s as Sheet).Id == relId);
            sheets.RemoveChild(sheet);
        }

        /// <summary>
        /// Adds a worksheet to the document.
        /// </summary>
        /// <param name="document">Document to add the wroksheet to</param>
        /// <param name="sheetName">Name of the worksheet to be added</param>
        /// <returns></returns>
        private static WorksheetPart AddWorksheetPart(SpreadsheetDocument document, string sheetName)
        {
            var worksheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            var relationshipId = document.WorkbookPart.GetIdOfPart(worksheetPart);
            uint sheetId = 1;
            if (sheets.Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            var sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);

            return worksheetPart;
        }

        /// <summary>
        /// Sets the value of a cell, optionally converting it to a string first.
        /// </summary>
        /// <param name="value">Value to set in the cell</param>
        /// <param name="cell">Cell where to set the value</param>
        /// <param name="sharedStringTable">Structure of the spreadsheet that contains the actual string values</param>
        /// <param name="stylesheet">Style section of the spreadsheet, containing formatting information</param>
        /// <param name="writeAsString">When true, the value will be converted to a string before writing to the cell</param>
        private static void SetCellValue(object value, Cell cell, SharedStringTable sharedStringTable, Stylesheet stylesheet, bool writeAsString)
        {
            if (writeAsString)
            {
                value = Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            if (value is string)
            {
                // For strings, we put them in the shared string table and reference by index, just like Office would
                var tuple = sharedStringTable.Select((v, i) => System.Tuple.Create(v, i)).FirstOrDefault(t => t.Item1.InnerText.Equals(value));
                int index;
                if (tuple == null)
                {
                    // The string is not in the table, so we add it
                    sharedStringTable.AppendChild(new SharedStringItem(new Text((string)value)));
                    index = (int)sharedStringTable.Count.Value;
                    // Yes, you need to update these manually
                    sharedStringTable.Count++;
                    sharedStringTable.UniqueCount++;
                }
                else
                {
                    index = tuple.Item2;
                }

                cell.CellValue = new CellValue(index.ToString(CultureInfo.InvariantCulture));
                cell.DataType = CellValues.SharedString;
            }
            else if (value is bool)
            {
                cell.CellValue = new CellValue(Convert.ToInt32((bool)value).ToString(CultureInfo.InvariantCulture));
                cell.DataType = CellValues.Boolean;
            }
            else if (value is DateTime)
            {
                const int DateTimeFormatId = 22;
                // Search for a style using the DateTime format id
                var tuple = stylesheet.CellFormats.Select((v, i) => System.Tuple.Create((CellFormat)v, i)).FirstOrDefault(p => p.Item1.NumberFormatId != null && p.Item1.NumberFormatId.Value == DateTimeFormatId);
                uint index;
                if (tuple == null)
                {
                    // No such style exists so we create one
                    stylesheet.CellFormats.AppendChild(new CellFormat() { NumberFormatId = DateTimeFormatId, FormatId = 0 });
                    index = stylesheet.CellFormats.Count.Value;
                    // We update this for the sake of consistency
                    stylesheet.CellFormats.Count++;
                }
                else
                {
                    index = (uint)tuple.Item2;
                }

                cell.StyleIndex = index;
                cell.CellValue = new CellValue(((DateTime)value).ToOADate());
                cell.DataType = null;
            }
            else
            {
                // This is for long and double. Also acts as the default behavior for things we do not specially handle.
                cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                cell.DataType = null;
            }
        }
    }
}
