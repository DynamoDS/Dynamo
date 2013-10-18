using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class ConnectorViewModel
    {
        private DelegateCommand redrawCommand;
        private DelegateCommand highlightCommand;
        private DelegateCommand unHighlightCommand;

        public DelegateCommand RedrawCommand
        {
            get
            {
                if (redrawCommand == null)
                    redrawCommand = new DelegateCommand(Redraw, CanRedraw);

                return redrawCommand;
            }
        }

        public DelegateCommand HighlightCommand
        {
            get
            {
                if (highlightCommand == null)
                    highlightCommand = new DelegateCommand(Highlight, CanHighlight);

                return highlightCommand;
            }
        }

        public DelegateCommand UnHighlightCommand
        {
            get
            {
                if (unHighlightCommand == null)
                    unHighlightCommand = new DelegateCommand(Unhighlight, CanUnHighlight);

                return unHighlightCommand;
            }
        }
    }
}
