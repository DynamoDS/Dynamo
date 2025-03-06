using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Graph.Notes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
using ProtoCore.Utils;

namespace Dynamo.Nodes
{
    public partial class NoteView : IViewModelView<NoteViewModel>
    {
        private EditWindow editWindow;

        /// <summary>
        /// Special keys definition in note
        /// </summary>
        internal Key[] specialKeys = { Key.OemMinus, Key.Subtract, Key.Tab, Key.Enter };

        public NoteViewModel ViewModel { get; private set; }

        public NoteView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);

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
            if (ViewModel != null)
            {
                ViewModel.RequestsSelection -= OnViewModelRequestsSelection;
            }
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

           if (this.ViewModel.Model.PinnedNode != null)
            {
            var nodeGuid = this.ViewModel.Model.PinnedNode.GUID;
                //We have to use AddUnique due that otherwise the node will be added several times when we click right over the note
                DynamoSelection.Instance.Selection.AddUnique(ViewModel.Model.PinnedNode);
            }
            BringToFront();

        }

        private void OnEditItemClick(object sender, RoutedEventArgs e)
        {

            if (ViewModel.WorkspaceViewModel.Zoom > Configurations.ZoomDirectEditThreshold)
            {
                Panel.SetZIndex(noteTextBox, 1);
                ViewModel.IsOnEditMode = true;
                noteTextBox.Focus();
                noteTextBox.SelectAll();
                return;
            }

            // Setup a binding with the edit window's text field
            var dynamoViewModel = ViewModel.WorkspaceViewModel.DynamoViewModel;
            editWindow = new EditWindow(dynamoViewModel, true)
            {
                Title = Wpf.Properties.Resources.EditNoteWindowTitle
            };

            editWindow.EditTextBoxPreviewKeyDown += noteTextBox_PreviewKeyDown;
            editWindow.Closed += EditWindow_Closed;

            editWindow.BindToProperty(DataContext, new Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as NoteViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
            editWindow.TitleTextBlock.Text = Wpf.Properties.Resources.EditNoteWindowTitle;

            editWindow.ShowDialog();

        }
       
        private void EditWindow_Closed(object sender, EventArgs e)
        {
            editWindow.EditTextBoxPreviewKeyDown -= noteTextBox_PreviewKeyDown;
            editWindow.Closed -= EditWindow_Closed;
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
            ViewModel.IsOnEditMode = false;
            
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    System.Guid.Empty, ViewModel.Model.GUID, nameof(NoteModel.Text), noteTextBox.Text));
        }

        private void noteTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!specialKeys.Contains(e.Key) || Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift)
            {
                return;
            }
            e.Handled = true;

            if (sender is TextBox textBox)
            {
                var originalText = textBox.Text;
                try
                {
                    // Remove selected text
                    textBox.Text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);

                    var text = StringUtils.SpaceToTabConversion(textBox.Text, TAB_SPACING_SIZE);
                    var caretIndex = textBox.CaretIndex + (text.Length - textBox.Text.Length);

                    switch (e.Key)
                    {
                        case Key.OemMinus:
                            textBox.Text = BulletDashHandler(text, caretIndex);
                            break;
                        case Key.Subtract:
                            textBox.Text = BulletDashHandler(text, caretIndex);
                            break;
                        case Key.Tab:
                            textBox.Text = BulletTabHandler(text, caretIndex);
                            break;
                        case Key.Enter:
                            textBox.Text = BulletEnterHandler(text, caretIndex);
                            break;
                    }

                    textBox.Text = StringUtils.TabToSpaceConversion(textBox.Text, TAB_SPACING_SIZE);

                    textBox.CaretIndex = caretIndex + (textBox.Text.Length - text.Length);
                }
                catch (Exception)
                {
                    // Restore original text if something went wrong
                    textBox.Text = originalText;
                }
            }
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
        /// Amount of spaces inserted after bullet
        /// </summary>
        private static int SPACING_AFTER_BULLET = 2;

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

            // If there is text before caret
            // or another bullet in line
            // dont convert it to bullet
            bool lineContainsBullet = BULLETS_CHARS.Where(b => line.Contains(b)).Any();
            var textBeforeCaret = line.Substring(0, caretAtLine);
            if (!StringUtils.IsStringSpacesWithTabs(textBeforeCaret) || lineContainsBullet)
            {
                line = line.Insert(caretAtLine, "-");
                return StringUtils.ReplaceLineOfText(text, lineNumber, line);
            }

            var tabsBeforeCaret = textBeforeCaret.Count(t => t == '\t');
            var bulletIndex = tabsBeforeCaret % BULLETS_CHARS.Length;
            var bulletCharAndRightSpacing = BULLETS_CHARS[bulletIndex] + new String(' ', SPACING_AFTER_BULLET);
            line = line.Insert(caretAtLine, bulletCharAndRightSpacing);

            return StringUtils.ReplaceLineOfText(text, lineNumber, line);
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
            var distanceFromBulletToCaret = SPACING_AFTER_BULLET + 1;
            var caretIsRightAfterBullet = IsCaretRightAfterBullet(line, caretAtLine, distanceFromBulletToCaret);
            if (caretIsRightAfterBullet)
            {
                line = line.Insert(caretAtLine - distanceFromBulletToCaret, "\t");
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
                
            return StringUtils.ReplaceLineOfText(text, lineNumber, line);
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
            return text.Insert(caretIndex, bulletsInLine.First()+ new String(' ', SPACING_AFTER_BULLET));
        }

        private bool IsCaretRightAfterBullet(string text, int caretIndex, int distanceFromBulletToCaret)
        {
           
            if (caretIndex < distanceFromBulletToCaret)
            {
                return false;
            }
            // We need to get the character 2 indices before the caretIndex 
            // because when we place the bullet we add a space as well for indentation
            return BULLETS_CHARS.Contains(text[caretIndex - distanceFromBulletToCaret]);
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
            var textInLines = StringUtils.BreakTextIntoLines(textUntillCaretIndex);
            var caretRelativeToLine = textInLines.Last().Length;
            return caretRelativeToLine;
        }
        
        private int GetLineNumberAtCaretsIndex(string text, int caretIndex)
        {
            var textBeforeCaret = text.Substring(0, caretIndex);
            var textInLines = StringUtils.BreakTextIntoLines(textBeforeCaret);
            return textInLines.Length-1;
        }
        private string GetLineTextAtCaretsIndex(string text, int caretIndex)
        {
            var textInLines = StringUtils.BreakTextIntoLines(text);
            var caretsLineIndex = GetLineNumberAtCaretsIndex(text, caretIndex);
            return textInLines[caretsLineIndex];
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

        #endregion
    }
}
