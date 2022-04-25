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
                Padding = new Thickness(5, 0, 0, 0)
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
