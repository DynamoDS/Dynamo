using System;
using System.Collections.Generic;
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

            switch (richTextBox.Name)
            {
                case "BeforePanel":
                    ShowLineDiffs(richTextBox, model, richTextBox.Name);
                    break;
                case "AfterPanel":
                    ShowLineDiffs(richTextBox, model, richTextBox.Name);
                    break;
                case "InlinePanel":
                    ShowLineDiffs(richTextBox, model, richTextBox.Name);
                    break;
                default:
                    break;
            }
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
                        paragraphs.Add(CreateParagraph(line.Text ?? string.Empty, line.Position.ToString() ?? " ", ADDED_SIGN, GetAfterModifiedBrush()));
                        break;
                    case ChangeType.Deleted:
                        paragraphs.Add(CreateParagraph(line.Text ?? string.Empty, line.Position.ToString() ?? " ", REMOVED_SIGN, GetBeforeModifiedBrush()));
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
            var paragraph = CreateParagraph(string.Empty, line.Position.ToString() ?? " ",
                isBeforeText ? REMOVED_SIGN : ADDED_SIGN,
                isBeforeText ? GetBeforeModifiedBrush() : GetAfterModifiedBrush());

            var subPieces = line.SubPieces;
            foreach (var piece in subPieces)
            {
                switch (piece.Type)
                {
                    case ChangeType.Unchanged:
                        // if unchanged add the text with no coloring to the paragraph
                        paragraph.Content.Inlines.Add(NewRun(piece.Text));
                        break;
                    case ChangeType.Deleted:
                        paragraph.Content.Inlines.Add(NewRun(piece.Text, GetDeletedRunBrush()));
                        break;
                    case ChangeType.Inserted:
                        paragraph.Content.Inlines.Add(NewRun(piece.Text, GetModifiedRunBrush()));
                        break;
                    case ChangeType.Imaginary:
                        break;
                    case ChangeType.Modified:
                        break;
                    default:
                        break;
                }
            }
            return paragraph;
        }

        private static void AppendParagraphsToPanelTable(RichTextBox textBox, List<DiffParagraph> paragraphs)
        {
            var table = new Table();
            AddTableColumn(table, 2);
            table.Columns[0].Width = new GridLength(50);
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

        private static DiffParagraph CreateParagraph(string text, string lineNum, string operationSign, Brush background = null)
        {
            var opertaionParagraph = new Paragraph(new Run(string.Format("{0} {1}", lineNum, operationSign)))
            {
                LineHeight = 0.5,
                TextAlignment = TextAlignment.Right
            };
            var contetParagraph = new Paragraph(new Run(text))
            {
                LineHeight = 0.5,
            };

            return new DiffParagraph() { Operation = opertaionParagraph, Content = contetParagraph, ParagraphBackground = background ?? Brushes.Transparent };
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
            var brush = new SolidColorBrush();
            brush.Color = (Color)ColorConverter.ConvertFromString("#FF0008");
            return brush;
        }

        private static SolidColorBrush GetBeforeModifiedBrush()
        {
            var brush = new SolidColorBrush();
            brush.Color = (Color)ColorConverter.ConvertFromString("#EE9597");
            return brush;
        }

        private static SolidColorBrush GetAfterModifiedBrush()
        {
            var brush = new SolidColorBrush();
            brush.Color = (Color)ColorConverter.ConvertFromString("#95CCA4");
            return brush;
        }

        private static SolidColorBrush GetModifiedRunBrush()
        {
            var brush = new SolidColorBrush();
            brush.Color = (Color)ColorConverter.ConvertFromString("#009F2C");
            return brush;
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
