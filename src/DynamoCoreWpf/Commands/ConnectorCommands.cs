using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class ConnectorViewModel
    {
        private DelegateCommand redrawCommand;

        public DelegateCommand RedrawCommand
        {
            get
            {
                if (redrawCommand == null)
                    redrawCommand = new DelegateCommand(Redraw, CanRedraw);

                return redrawCommand;
            }
        }
    }
}
