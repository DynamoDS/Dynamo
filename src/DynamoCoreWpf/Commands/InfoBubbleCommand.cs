using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class InfoBubbleViewModel : ViewModelBase
    {
        private DelegateCommand updateContentCommand;
        private DelegateCommand updatePositionCommand;
        private DelegateCommand changeInfoBubbleStateCommand;
        private DelegateCommand openDocumentationLinkCommand;
        private DelegateCommand dismissWarningCommand;

        public DelegateCommand UpdateContentCommand
        {
            get
            {
                if (updateContentCommand == null)
                    updateContentCommand = new DelegateCommand(UpdateInfoBubbleContent, CanUpdateInfoBubbleCommand);
                return updateContentCommand;
            }
        }

        public DelegateCommand UpdatePositionCommand
        {
            get
            {
                if (updatePositionCommand == null)
                    updatePositionCommand = new DelegateCommand(UpdatePosition, CanUpdatePosition);
                return updatePositionCommand;
            }
        }
        
        public DelegateCommand ChangeInfoBubbleStateCommand
        {
            get
            {
                if (changeInfoBubbleStateCommand == null)
                {
                    changeInfoBubbleStateCommand = new DelegateCommand(ChangeInfoBubbleState, CanChangeInfoBubbleState);
                }
                return changeInfoBubbleStateCommand;
            }
        }

        public DelegateCommand OpenDocumentationLinkCommand
        {
            get
            {
                if (openDocumentationLinkCommand == null)
                {
                    openDocumentationLinkCommand = new DelegateCommand(OpenDocumentationLink, CanOpenDocumentationLink);
                }
                return openDocumentationLinkCommand;
            }
        }

        public DelegateCommand DismissWarningCommand
        {
            get
            {
                if (dismissWarningCommand == null)
                {
                    dismissWarningCommand = new DelegateCommand(DismissWarning, CanDismissWarning);
                }
                return dismissWarningCommand;
            }
        }

    }
}