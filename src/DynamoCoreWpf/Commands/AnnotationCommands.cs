using System;
using System.Dynamic;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class AnnotationViewModel
    {
        private DelegateCommand _saveCommand;
        private DelegateCommand _bringToFrontCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new DelegateCommand(SaveTextboxValue, CanSaveTextboxValue);
                return _saveCommand;
            }
        }

        public DelegateCommand BringToFrontCommand
        {
            get
            {
                if (_bringToFrontCommand == null)
                    _bringToFrontCommand = new DelegateCommand(BringToFront, CanBringToFront);
                return _bringToFrontCommand;
            }
        }
    }
}
