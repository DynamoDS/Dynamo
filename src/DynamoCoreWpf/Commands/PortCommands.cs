using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel
    {
        private DelegateCommand connectCommand;
        private DelegateCommand autoCompleteCommand;
        private DelegateCommand portMouseEnterCommand;
        private DelegateCommand portMouseLeaveCommand;
        private DelegateCommand portMouseLeftButtonCommand;
        private DelegateCommand portMouseLeftButtonOnLevelCommand;
        private DelegateCommand portMouseLeftUseLevelCommand;
        private DelegateCommand nodeInPortContextMenuCommand;
        private DelegateCommand nodeOutPortContextMenuCommand;

        public DelegateCommand ConnectCommand
        {
            get
            {
                if(connectCommand == null)
                    connectCommand = new DelegateCommand(Connect, CanConnect);

                return connectCommand;
            }
        }

        /// <summary>
        /// Command to trigger Node Auto Complete from node port interaction
        /// </summary>
        public DelegateCommand NodeAutoCompleteCommand
        {
            get
            {
                if (autoCompleteCommand == null)
                    autoCompleteCommand = new DelegateCommand(AutoComplete, CanAutoComplete);

                return autoCompleteCommand;
            }
        }

        /// <summary>
        /// Command to open an InPort's Context Menu popup
        /// </summary>
        public DelegateCommand NodeInPortContextMenuCommand
        {
            get
            {
                if (nodeInPortContextMenuCommand == null)
                {
                    nodeInPortContextMenuCommand = new DelegateCommand(NodeInPortContextMenu);
                }
                return nodeInPortContextMenuCommand;
            }
        }

        /// <summary>
        /// Command to open an OutPort's Context Menu popup
        /// </summary>
        public DelegateCommand NodeOutPortContextMenuCommand
        {
            get
            {
                if (nodeOutPortContextMenuCommand == null)
                {
                    nodeOutPortContextMenuCommand = new DelegateCommand(NodeOutPortContextMenu);
                }
                return nodeOutPortContextMenuCommand;
            }
        }

        public DelegateCommand MouseEnterCommand
        {
            get
            {
                if (portMouseEnterCommand == null)
                    portMouseEnterCommand = new DelegateCommand(OnRectangleMouseEnter, CanConnect);

                return portMouseEnterCommand;
            }
        }

        public DelegateCommand MouseLeaveCommand
        {
            get
            {
                if (portMouseLeaveCommand == null)
                    portMouseLeaveCommand = new DelegateCommand(OnRectangleMouseLeave, CanConnect);

                return portMouseLeaveCommand;
            }
        }

        public DelegateCommand MouseLeftButtonDownCommand
        {
            get
            {
                if (portMouseLeftButtonCommand == null)
                    portMouseLeftButtonCommand = new DelegateCommand(OnRectangleMouseLeftButtonDown, CanConnect);

                return portMouseLeftButtonCommand;
            }
        }

        public DelegateCommand MouseLeftButtonDownOnLevelCommand
        {
            get
            {
                if (portMouseLeftButtonOnLevelCommand == null)
                    portMouseLeftButtonOnLevelCommand = new DelegateCommand(OnMouseLeftButtonDownOnLevel, CanConnect);

                return portMouseLeftButtonOnLevelCommand;
            }
        }

        public DelegateCommand MouseLeftUseLevelCommand
        {
            get
            {
                if (portMouseLeftUseLevelCommand == null)
                    portMouseLeftUseLevelCommand = new DelegateCommand(OnMouseLeftUseLevel, CanConnect);

                return portMouseLeftUseLevelCommand;
            }
        }
    }
}
