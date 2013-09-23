using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class PopupViewModel : ViewModelBase
    {
        private DelegateCommand _updatePopupCommand;
        private DelegateCommand _fadeInCommand;
        private DelegateCommand _fadeOutCommand;

        public DelegateCommand UpdatePopupCommand
        {
            get
            {
                if (_updatePopupCommand == null)
                    _updatePopupCommand = new DelegateCommand(UpdatePopup, CanUpdatePopup);
                return _updatePopupCommand;
            }
        }

        public DelegateCommand FadeInCommand
        {
            get
            {
                if (_fadeInCommand == null)
                    _fadeInCommand = new DelegateCommand(FadeIn, CanFadeIn);
                return _fadeInCommand;
            }
        }
        
        public DelegateCommand FadeOutCommand
        {
            get
            {
                if (_fadeOutCommand == null)
                    _fadeOutCommand = new DelegateCommand(FadeOut, CanFadeOut);
                return _fadeOutCommand;
            }
        }
    }
}
