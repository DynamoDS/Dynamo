using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for ShortcutToolbar.xaml
    /// </summary>
    public partial class ShortcutToolbar : UserControl
    {
        private ObservableCollection<ShortcutBarItem> shortcutBarItems;
        public ObservableCollection<ShortcutBarItem> ShortcutBarItems
        {
            get { return shortcutBarItems; }
        }

        public ShortcutToolbar()
        {            
            shortcutBarItems = new ObservableCollection<ShortcutBarItem>();

            DynamoViewModel viewModel = dynSettings.Controller.DynamoViewModel;

            ShortcutBarItem newScriptButton = new ShortcutBarItem();
            newScriptButton.ShortcutToolTip = "New [Ctrl + N]";
            newScriptButton.ShortcutCommand = viewModel.NewHomeWorkspaceCommand;
            newScriptButton.ShortcutCommandParameter = null;
            newScriptButton.ImgNormalSource = "/DynamoCore;component/UI/Images/new_normal.png";
            newScriptButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/new_disabled.png";
            newScriptButton.ImgHoverSource = "/DynamoCore;component/UI/Images/new_hover.png";

            ShortcutBarItem openScriptButton = new ShortcutBarItem();
            openScriptButton.ShortcutToolTip = "Open [Ctrl + O]";
            openScriptButton.ShortcutCommand = viewModel.ShowOpenDialogAndOpenResultCommand;
            openScriptButton.ShortcutCommandParameter = null;
            openScriptButton.ImgNormalSource = "/DynamoCore;component/UI/Images/open_normal.png";
            openScriptButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/open_disabled.png";
            openScriptButton.ImgHoverSource = "/DynamoCore;component/UI/Images/open_hover.png";

            ShortcutBarItem saveButton = new ShortcutBarItem();
            saveButton.ShortcutToolTip = "Save [Ctrl + S]";
            saveButton.ShortcutCommand = viewModel.ShowSaveDialogIfNeededAndSaveResultCommand;
            saveButton.ShortcutCommandParameter = null;
            saveButton.ImgNormalSource = "/DynamoCore;component/UI/Images/save_normal.png";
            saveButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/save_disabled.png";
            saveButton.ImgHoverSource = "/DynamoCore;component/UI/Images/save_hover.png";

            // PLACEHOLDER FOR FUTURE SHORTCUTS
            //ShortcutBarItem undoButton = new ShortcutBarItem();
            //undoButton.ShortcutToolTip = "Undo [Ctrl + Z]";
            ////undoButton.ShortcutCommand = viewModel.; // Function implementation in progress
            //undoButton.ShortcutCommandParameter = null;
            //undoButton.ImgNormalSource = "/DynamoCore;component/UI/Images/undo_normal.png";
            //undoButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/undo_disabled.png";
            //undoButton.ImgHoverSource = "/DynamoCore;component/UI/Images/undo_hover.png";

            //ShortcutBarItem redoButton = new ShortcutBarItem();
            //redoButton.ShortcutToolTip = "Redo [Ctrl + Y]";
            ////redoButton.ShortcutCommand = viewModel.; // Function implementation in progress
            //redoButton.ShortcutCommandParameter = null;
            //redoButton.ImgNormalSource = "/DynamoCore;component/UI/Images/redo_normal.png";
            //redoButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/redo_disabled.png";
            //redoButton.ImgHoverSource = "/DynamoCore;component/UI/Images/redo_hover.png";

            //ShortcutBarItem runButton = new ShortcutBarItem();
            //runButton.ShortcutToolTip = "Run [Ctrl + R]";
            ////runButton.ShortcutCommand = viewModel.RunExpressionCommand; // Function implementation in progress
            //runButton.ShortcutCommandParameter = null;
            //runButton.ImgNormalSource = "/DynamoCore;component/UI/Images/run_normal.png";
            //runButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/run_disabled.png";
            //runButton.ImgHoverSource = "/DynamoCore;component/UI/Images/run_hover.png";

            shortcutBarItems.Add(newScriptButton);
            shortcutBarItems.Add(openScriptButton);
            shortcutBarItems.Add(saveButton);
            //shortcutBarItems.Add(undoButton);
            //shortcutBarItems.Add(redoButton);
            //shortcutBarItems.Add(runButton);

            InitializeComponent();
        }
    }

    public partial class ShortcutBarItem
    {
        private string shortcutToolTip;
        private string imgNormalSource;
        private string imgHoverSource;
        private string imgDisabledSource;
        private DelegateCommand shortcutCommand;
        private string shortcutCommandParameter;

        public string ShortcutCommandParameter
        {
            get { return shortcutCommandParameter; }
            set { shortcutCommandParameter = value; }
        }

        public DelegateCommand ShortcutCommand
        {
            get { return shortcutCommand; }
            set { shortcutCommand = value; }
        }

        public string ImgDisabledSource
        {
            get { return imgDisabledSource; }
            set { imgDisabledSource = value; }
        }

        public string ImgHoverSource
        {
            get { return imgHoverSource; }
            set { imgHoverSource = value; }
        }

        public string ImgNormalSource
        {
            get { return imgNormalSource; }
            set { imgNormalSource = value; }
        }

        public string ShortcutToolTip
        {
            get { return shortcutToolTip; }
            set { shortcutToolTip = value; }
        }

        public bool IsEnabled
        {
            get
            {
                if (this.shortcutCommand != null)
                    return this.shortcutCommand.CanExecute(null);

                return false;
            }
        }
    }
}
