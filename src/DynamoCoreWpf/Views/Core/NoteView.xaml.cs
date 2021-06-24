using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Nodes
{
    public partial class NoteView : IViewModelView<NoteViewModel>
    {
        private const double MINIMUM_ZOOM_DIRECT_NODE_EDIT = 0.5;
        public NoteViewModel ViewModel { get; private set; }

        public NoteView()
        {
            InitializeComponent();

            // update the size of the element when the text changes
            noteText.SizeChanged += (sender, args) =>
                {
                    if (ViewModel != null)
                        ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
                };
            noteText.PreviewMouseDown += OnNoteTextPreviewMouseDown;

            Loaded += OnNoteViewLoaded;
            Unloaded += OnNoteViewUnloaded;
        }

        void OnNoteViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as NoteViewModel;
            ViewModel.RequestsSelection += OnViewModelRequestsSelection;

            // NoteModel has default dimension of 100x100 which will not be ideal in 
            // most cases. Here we update the model according to the size of the view.
            // At this point the view (a TextBlock) would have already been updated 
            // with the bound data, so its size is up-to-date, here we make a call to 
            // update the corresponding model.
            // 
            ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
        }

        void OnNoteViewUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RequestsSelection -= OnViewModelRequestsSelection;
        }

        void OnViewModelRequestsSelection(object sender, EventArgs e)
        {
            if (!ViewModel.Model.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                DynamoSelection.Instance.Selection.AddUnique(ViewModel.Model);

            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.Model);
                }
            }
        }

        void OnNoteTextPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid noteGuid = this.ViewModel.Model.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(noteGuid, Keyboard.Modifiers.AsDynamoType()));
            BringToFront();
           
        }

        private void OnEditItemClick(object sender, RoutedEventArgs e)
        {

            if (ViewModel.WorkspaceViewModel.Zoom > MINIMUM_ZOOM_DIRECT_NODE_EDIT)
            {
                Panel.SetZIndex(noteTextBox, 1);
                ViewModel.IsOnEditMode = true;
                noteTextBox.Focus();
                noteTextBox.SelectAll();
                return;
            }

            // Setup a binding with the edit window's text field
            var dynamoViewModel = ViewModel.WorkspaceViewModel.DynamoViewModel;
            var editWindow = new EditWindow(dynamoViewModel, true)
            {
                Title = Dynamo.Wpf.Properties.Resources.EditNoteWindowTitle
            };

            editWindow.EditTextBoxPreviewKeyDown += noteTextBox_PreviewKeyDown;

            editWindow.BindToProperty(DataContext, new Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as NoteViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        private void OnDeleteItemClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void OnNoteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                OnEditItemClick(this, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Sets ZIndex of the particular note to be the highest in the workspace
        /// This brings the note to the forefront of the workspace when clicked
        /// </summary>
        private void BringToFront()
        {
            if (NoteViewModel.StaticZIndex == int.MaxValue)
            {
                PrepareZIndex();
            }
            
            ViewModel.ZIndex = ++NoteViewModel.StaticZIndex;
        }


        /// <summary>
        /// If ZIndex is more then max value of int, it should be set back to 0 for all elements.
        /// </summary>
        private void PrepareZIndex()
        {
            NoteViewModel.StaticZIndex = Configurations.NodeStartZIndex;

            var parent = TemplatedParent as ContentPresenter;
            if (parent == null) return;

            // reset the ZIndex for all Notes
            foreach (var child in parent.ChildrenOfType<NoteView>())
            {
                child.ViewModel.ZIndex = Configurations.NodeStartZIndex;
            }
            
            // reset the ZIndex for all Nodes
            foreach(var child in parent.ChildrenOfType<Controls.NodeView>())
            {
                child.ViewModel.ZIndex = Configurations.NodeStartZIndex;
            }
        }

        private void noteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(noteTextBox, 0);
        }

        private void noteTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key[] specialKeys = { Key.OemMinus, Key.Tab, Key.Enter };
            if (!specialKeys.Contains(e.Key))
            {
                return;
            }
                
            e.Handled = true;
            var textBox = sender as TextBox;
            
            var text = SpaceToTabConversion(textBox.Text);
            var caretIndex = textBox.CaretIndex + (text.Length - textBox.Text.Length);

            switch (e.Key)
            {
                case Key.OemMinus:
                    textBox.Text = BulletDashHandler(text, caretIndex);
                    break;
                case Key.Tab:
                    textBox.Text = BulletTabHandler(text, caretIndex);
                    break;
                case Key.Enter:
                    textBox.Text = BulletEnterHandler(text, caretIndex);
                    break;
            }

            textBox.Text = TabToSpaceConversion(textBox.Text);

            textBox.CaretIndex = caretIndex + (textBox.Text.Length - text.Length);
        }

        #region Bullet point support

        /// <summary>
        /// List of bullets characters to use in sequence of indentation
        /// </summary>
        private static readonly char[] BULLETS_CHARS = { '\u2022', '\u25E6', '\u2023' };

        /// <summary>
        /// Amount of spaces that represent a tab
        /// </summary>
        private static int TAB_SPACING_SIZE = 4;

        /// <summary>
        /// Handles text when user types Key.OemMinus (DASH)
        /// It converts the DASH into its appropriate BULLET Character
        /// by counting the amount of TAB spaces before the bullet.  
        /// Conversion only happens when dash is at the beginning of a line.
        /// </summary>
        /// <param name="text"> text before typing DASH </param>
        /// <param name="caretIndex"> caret index where user typed DASH </param>
        /// <returns></returns>
        private string BulletDashHandler( string text, int caretIndex)
        {
            // Get the line where DASH was typed 
            var lineNumber = GetLineNumberAtCaretsIndex(text, caretIndex);
            var line = GetLineTextAtCaretsIndex(text, caretIndex);
            var caretAtLine = GetCaretIndexRelativeToLine(text, caretIndex);

            // If there is text before caret dont convert it to bullet
            var textBeforeCaret = line.Substring(0, caretAtLine);
            if (!StringIsEmptyOrTab(textBeforeCaret))
            {
                line = line.Insert(caretAtLine, "-");
                return ReplaceLineOfText(text, lineNumber, line);
            }

            var tabsBeforeCaret = textBeforeCaret.Count(t => t == '\t');
            var bulletIndex = tabsBeforeCaret % BULLETS_CHARS.Length;
            line = line.Insert(caretAtLine, BULLETS_CHARS[bulletIndex]+" ");

            return ReplaceLineOfText(text, lineNumber, line);
        }

        /// <summary>
        /// Handles text when user types Key.Tab (TAB)
        /// If TAB is pressed before a bullet or right after the bullet
        /// it also updates the bullet to its appropriate bullet character
        /// </summary>
        /// <param name="text">  text before typing TAB </param>
        /// <param name="caretIndex"> caret index where user typed TAB </param>
        /// <returns></returns>
        private string BulletTabHandler(string text, int caretIndex)
        {
            // Get the line where TAB was typed 
            var lineNumber = GetLineNumberAtCaretsIndex(text, caretIndex);
            var line =  GetLineTextAtCaretsIndex(text,caretIndex);
            var caretAtLine = GetCaretIndexRelativeToLine(text, caretIndex);

            // If TAB was pressed just after a bullet insert it before the bullet
            // so that you can easily indent
            var caretIsRightAfterBullet = IsCaretRightAfterBullet(line, caretAtLine);
            if (caretIsRightAfterBullet)
            {
                line = line.Insert(caretAtLine - 2, "\t");
            }
                
            else
            {
                line = line.Insert(caretAtLine, "\t");
                caretAtLine++;
            }

            // Also update the bullet character if CaretIndex is right after bullet or before bullet
            if (IsCaretBeforeABullet(line, caretAtLine) || caretIsRightAfterBullet)
            {
                line = UpdateBulletAccordingToIndentation(line);
            }
                
            return ReplaceLineOfText(text, lineNumber, line);
        }

        /// <summary>
        /// Handles text when user types Key.Enter (ENTER)
        /// When ENTER is pressed, if previous line had a bullet copy it to next line
        /// </summary>
        /// <param name="text">  text before typing ENTER </param>
        /// <param name="caretIndex"> caret index where user typed ENTER </param>
        /// <returns></returns>
        private string BulletEnterHandler(string text, int caretIndex)
        {
            var lineNumber = GetLineNumberAtCaretsIndex(text, caretIndex);
            var line = GetLineTextAtCaretsIndex(text, caretIndex);
            var caretAtLine = GetCaretIndexRelativeToLine(text, caretIndex);

            // Check if line has any bullets, and copy it to next line
            var bulletsInLine = BULLETS_CHARS.Where(b => line.Contains(b));
            if (bulletsInLine.Count() == 0)
            {
                return text = text.Insert(caretIndex, "\n");
            }
            
            text = text.Insert(caretIndex, "\n");
            caretIndex++;

            var tabsBeforeBullet = line.Split(BULLETS_CHARS)[0].Count(c => c=='\t');
            for (int i = 0; i < tabsBeforeBullet; i++)
            {
                text = text.Insert(caretIndex, "\t");
                caretIndex++;
            }
            return text.Insert(caretIndex, bulletsInLine.First()+" ");
        }

        private bool StringIsEmptyOrTab(string text)
        {
            return !text.Any(c => !(c == ' ' || c == '\t'));
        }

        private bool IsCaretRightAfterBullet(string text, int caretIndex)
        {
            if (caretIndex == 0 || caretIndex ==1 )
            {
                return false;
            }
            // We need to get the character 2 indices before the caretIndex 
            // because when we place the bullet we add a space as well for indentation
            return BULLETS_CHARS.Contains(text[caretIndex - 2]);
        }

        private bool IsCaretBeforeABullet(string text, int caretIndex)
        {
           var charactersBeforeBullet = text.Split(BULLETS_CHARS).FirstOrDefault();
           return caretIndex <= charactersBeforeBullet.Length;
        }

        private int GetCaretIndexRelativeToLine(string text, int caretIndex)
        {
            // In order to get the caretIndex relative to the caret line:
            // Get the text untill the caretIndex
            // Split the text into lines
            // The caret index relative to the caret line will be equal to
            // the last line's length
            var textUntillCaretIndex = text.Substring(0, caretIndex);
            var textInLines = BreakTextIntoLines(textUntillCaretIndex);
            var caretRelativeToLine = textInLines.Last().Length;
            return caretRelativeToLine;
        }
        
        private int GetLineNumberAtCaretsIndex(string text, int caretIndex)
        {
            var textBeforeCaret = text.Substring(0, caretIndex);
            var textInLines = BreakTextIntoLines(textBeforeCaret);
            return textInLines.Length-1;
        }
        private string GetLineTextAtCaretsIndex(string text, int caretIndex)
        {
            var textInLines = BreakTextIntoLines(text);
            var caretsLineIndex = GetLineNumberAtCaretsIndex(text, caretIndex);
            return textInLines[caretsLineIndex];
        }

        private string ReplaceLineOfText(string text, int lineNumber, string newLine)
        {
            var textInLines = BreakTextIntoLines(text);
            textInLines[lineNumber] = newLine;
            return String.Join("\n", textInLines);
        }

        private string UpdateBulletAccordingToIndentation(string text, bool reverse = false)
        {
            for (int i = 0; i < BULLETS_CHARS.Length; i++)
            {
                int nextBulletIndex = (i + 1) % BULLETS_CHARS.Length;
                if (reverse)
                {
                    nextBulletIndex = (i + BULLETS_CHARS.Length - 1) % BULLETS_CHARS.Length;
                }

                var newText = text.Replace(BULLETS_CHARS[i], BULLETS_CHARS[nextBulletIndex]);
                if (!String.Equals(text, newText))
                {
                    text = newText;
                    break;
                }
            }
            return text;
        }

        /// <summary>
        /// Following suggestions from stackoverflow,
        /// A reliable method for breaking text into lines
        /// using Regex is used.
        /// https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
        /// </summary>
        /// <param name="text"> text to break into lines</param>
        /// <returns> text lines </returns>
        private string[] BreakTextIntoLines(string text)
        {
            return Regex.Split(text, "\r\n|\r|\n");
        }

        private string TabToSpaceConversion(string text)
        {
            return text.Replace("\t", new String(' ', TAB_SPACING_SIZE));
        }

        private string SpaceToTabConversion(string text)
        {
            return text.Replace(new String(' ', TAB_SPACING_SIZE), "\t");
        }
        #endregion
    }
}
