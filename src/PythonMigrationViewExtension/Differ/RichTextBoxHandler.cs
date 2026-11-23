using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DiffPlex.DiffBuilder.Model;

namespace Dynamo.PythonMigration.Differ
{
    class RichTextBoxHandler : DependencyObject
    {
        private const string REMOVED_SIGN = "-";
        private const string ADDED_SIGN = "+";
        private const string UNCHANGED_SIGN = " ";
        private const double ContentPaddingLeft = 5;
        private const double LineNumberSlack = 10;
        private const double NoWrapSlack = 40;
        private const string TabAsSpaces = "        ";

        public static DiffPaneModel GetUpdate(DependencyObject obj)
        {
            return (DiffPaneModel)obj.GetValue(UpdateProperty);
        }

        public static void SetUpdate(DependencyObject obj, DiffPaneModel value)
        {
            obj.SetValue(UpdateProperty, value);
        }

        public static readonly DependencyProperty UpdateProperty = DependencyProperty.RegisterAttached(
            "Update",
            typeof(DiffPaneModel),
            typeof(RichTextBoxHandler),
            new PropertyMetadata(null, new PropertyChangedCallback(DiffPanelsChange)));

        private static void DiffPanelsChange(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (RichTextBox)dep;
            var model = (DiffPaneModel)e.NewValue;

            // possible panel names : AfterPanel, BeforePanel, InlinePanel
            ShowLineDiffs(richTextBox, model, richTextBox.Name);
        }

        private static void ShowLineDiffs(RichTextBox diffBox, DiffPaneModel model, string senderPanel)
        {
            var isBeforePanel = senderPanel == "BeforePanel";
            diffBox.Document.Blocks.Clear();
            var paragraphs = new List<DiffParagraph>();
            foreach (var line in model.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Unchanged:
                        paragraphs.Add(CreateParagraph(line.Text ?? string.Empty, line.Position.ToString() ?? " ", UNCHANGED_SIGN));
                        break;
                    case ChangeType.Inserted:
                        paragraphs.Add(ShowSubPieceDiffs(line, false));
                        break;
                    case ChangeType.Deleted:
                        paragraphs.Add(ShowSubPieceDiffs(line, true));
                        break;
                    case ChangeType.Imaginary:
                        // Spacer so this row stays opposite the real line in the other pane.
                        paragraphs.Add(CreateParagraph(string.Empty, " ", " "));
                        break;
                    case ChangeType.Modified:
                        paragraphs.Add(ShowSubPieceDiffs(line, isBeforePanel));
                        break;
                    default: throw new ArgumentException();
                }
            }
            AppendParagraphsToPanelTable(diffBox, paragraphs);
        }

        private static DiffParagraph ShowSubPieceDiffs(DiffPiece line, bool isBeforeText)
        {
            var paragraph = CreateParagraph(
                string.Empty,
                line.Position.ToString() ?? " ",
                isBeforeText ? REMOVED_SIGN : ADDED_SIGN,
                isBeforeText ? GetBeforeModifiedBrush() : GetAfterModifiedBrush()
                );

            var wrotePiece = false;
            var subPieces = line.SubPieces;
            if (subPieces != null)
            {
                foreach (var piece in subPieces)
                {
                    switch (piece.Type)
                    {
                        case ChangeType.Unchanged:
                            paragraph.Content.Inlines.Add(NewRun(piece.Text));
                            wrotePiece = true;
                            break;
                        case ChangeType.Deleted:
                            paragraph.Content.Inlines.Add(NewRun(piece.Text, GetDeletedRunBrush()));
                            wrotePiece = true;
                            break;
                        case ChangeType.Inserted:
                            paragraph.Content.Inlines.Add(NewRun(piece.Text, GetModifiedRunBrush()));
                            wrotePiece = true;
                            break;
                        case ChangeType.Imaginary:
                            break;
                        case ChangeType.Modified:
                            break;
                        default:
                            break;
                    }
                }
            }

            // A whole inserted or deleted line often has no character pieces.
            // The words are on the line itself.
            if (!wrotePiece && !string.IsNullOrEmpty(line.Text))
            {
                var brush = isBeforeText ? GetDeletedRunBrush() : GetModifiedRunBrush();
                paragraph.Content.Inlines.Add(NewRun(line.Text, brush));
            }

            return paragraph;
        }

        private static void AppendParagraphsToPanelTable(RichTextBox textBox, List<DiffParagraph> paragraphs)
        {
            const int lineNumbersWidth = 30;

            var table = new Table();
            AddTableColumn(table, 2);
            table.Columns[0].Width = new GridLength(lineNumbersWidth);
            table.Columns[1].Width = GridLength.Auto;
            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            foreach (var item in paragraphs)
            {
                var currentRow = AddTableRow(group, item.ParagraphBackground);
                AddCellToRow(currentRow, item.Operation);
                AddCellToRow(currentRow, item.Content);
            }

            textBox.Document.Blocks.Add(table);
        }

        /// <summary>
        /// The rows of the diff table in <paramref name="box"/>, one per line of the diff, in order.
        /// Empty when the table has not been built.
        /// </summary>
        internal static IList<TableRow> GetRows(RichTextBox box)
        {
            var table = GetTable(box);
            if (table == null || table.RowGroups.Count == 0)
            {
                return Array.Empty<TableRow>();
            }

            return table.RowGroups[0].Rows;
        }

        /// <summary>
        /// Widens the line-number column so the largest number and its +/- sign fit on one row.
        /// Every box gets the same width. Call after the boxes are loaded, so their fonts are known.
        /// </summary>
        internal static void FitLineNumberColumn(params RichTextBox[] boxes)
        {
            var width = 0.0;
            foreach (var box in boxes)
            {
                foreach (var row in GetRows(box))
                {
                    width = Math.Max(width, MeasureCell(box, row, 0));
                }
            }

            if (width <= 0)
            {
                return;
            }

            foreach (var box in boxes)
            {
                var table = GetTable(box);
                if (table != null && table.Columns.Count > 0)
                {
                    table.Columns[0].Width = new GridLength(Math.Ceiling(width + LineNumberSlack));
                }
            }
        }

        /// <summary>
        /// Stops long lines from wrapping: every box gets a page as wide as the longest line in any of them.
        /// Each line is then exactly one row high, so side-by-side panes stay level with each other and
        /// scroll the same distance. The panes scroll sideways to show long lines.
        /// Call after the boxes are loaded, so their fonts are known.
        /// </summary>
        internal static void TurnOffWrapping(params RichTextBox[] boxes)
        {
            FitLineNumberColumn(boxes);

            var textWidth = 0.0;
            var numberWidth = 0.0;
            foreach (var box in boxes)
            {
                var table = GetTable(box);
                if (table != null && table.Columns.Count > 0)
                {
                    numberWidth = Math.Max(numberWidth, table.Columns[0].Width.Value);
                }

                foreach (var row in GetRows(box))
                {
                    textWidth = Math.Max(textWidth, MeasureCell(box, row, 1));
                }
            }

            foreach (var box in boxes)
            {
                var document = box.Document;
                var padding = document.PagePadding;
                var horizontalPadding = double.IsNaN(padding.Left) || double.IsNaN(padding.Right)
                    ? 0
                    : padding.Left + padding.Right;
                document.PageWidth = Math.Ceiling(numberWidth + ContentPaddingLeft + textWidth + horizontalPadding + NoWrapSlack);

                // A wide page alone is not enough: the RichTextBox stays as wide as the pane and hides the
                // rest of the page. Make the box itself at least as wide as the page, so the pane around it
                // shows a sideways scroll bar.
                box.MinWidth = document.PageWidth
                               + box.Padding.Left + box.Padding.Right
                               + box.BorderThickness.Left + box.BorderThickness.Right;
            }
        }

        private static Table GetTable(RichTextBox box) => box?.Document?.Blocks.FirstBlock as Table;

        private static double MeasureCell(RichTextBox box, TableRow row, int column)
        {
            if (row.Cells.Count <= column)
            {
                return 0;
            }

            var block = row.Cells[column].Blocks.FirstBlock;
            if (block == null)
            {
                return 0;
            }

            var text = new TextRange(block.ContentStart, block.ContentEnd).Text.TrimEnd('\r', '\n');
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var document = box.Document;
            var formatted = new FormattedText(
                text.Replace("\t", TabAsSpaces),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(document.FontFamily, document.FontStyle, document.FontWeight, document.FontStretch),
                document.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(box).PixelsPerDip);
            return formatted.WidthIncludingTrailingWhitespace;
        }

        private static DiffParagraph CreateParagraph(string text, string lineNum, string operationSign, Brush background = null)
        {
            var operationParagraph = new Paragraph(new Run(string.Format("{0} {1}", lineNum, operationSign)))
            {
                LineHeight = 0.5,
                TextAlignment = TextAlignment.Right
            };
            var contentParagraph = new Paragraph(new Run(text))
            {
                LineHeight = 0.5,
                Padding = new Thickness(ContentPaddingLeft, 0, 0, 0)
            };

            return new DiffParagraph() { Operation = operationParagraph, Content = contentParagraph, ParagraphBackground = background ?? Brushes.Transparent };
        }

        private static void AddTableColumn(Table table, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                table.Columns.Add(new TableColumn());
            }
        }

        private static TableRow AddTableRow(TableRowGroup group, Brush background)
        {
            var row = new TableRow();
            row.Background = background;
            group.Rows.Add(row);
            return row;
        }

        private static void AddCellToRow(TableRow row, Paragraph p)
        {
            var cell = new TableCell(p);
            row.Cells.Add(cell);
        }

        private static Run NewRun(string text, Brush background = null) => new Run(text)
        {
            Background = background ?? Brushes.Transparent
        };

        private static SolidColorBrush GetDeletedRunBrush()
        {
            return new SolidColorBrush
            {
                Color = (Color)ColorConverter.ConvertFromString("#C14C49") // bright red
            };
        }

        private static SolidColorBrush GetBeforeModifiedBrush()
        {
            return new SolidColorBrush
            {
                Color = (Color)ColorConverter.ConvertFromString("#634B4A") // dark red
            };
        }

        private static SolidColorBrush GetAfterModifiedBrush()
        {
            return new SolidColorBrush
            {
                Color = (Color)ColorConverter.ConvertFromString("#495B43") // dark green
            };
        }

        private static SolidColorBrush GetModifiedRunBrush()
        {
            return new SolidColorBrush
            {
                Color = (Color)ColorConverter.ConvertFromString("#419D29") // bright green
            };
        }

        private class DiffParagraph
        {
            public Paragraph Operation { get; set; }
            public Paragraph Content { get; set; }
            public Brush ParagraphBackground { get; set; }

            public void SetBackground(SolidColorBrush colorBrush)
            {
                if (colorBrush == null)
                    return;

                Operation.Background = colorBrush;
                Content.Background = colorBrush;
            }
        }
    }
}
