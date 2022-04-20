using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.UI.Commands;

namespace Dynamo.GraphNodeManager.ViewModels
{
    public class FilterViewModel : NotificationObject
    {
        private bool isFilterOn = false;

        public DelegateCommand ToggleCommand { get; set; }
        public string Name { get; internal set; }

        public bool IsFilterOn
        {
            get
            {
                return isFilterOn;
            }
            internal set
            {
                isFilterOn = value;
                RaisePropertyChanged(nameof(IsFilterOn));
            }
        }

        public FilterViewModel()
        {
            ToggleCommand = new DelegateCommand(Toggle);
        }

        private void Toggle(object obj)
        {
            IsFilterOn = !IsFilterOn;
        }
    }
}
