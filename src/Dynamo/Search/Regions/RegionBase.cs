using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dynamo.Search.Regions
{
    public abstract class RegionBase : ICommand
    {
        public bool Loaded { get; set; }

        public abstract bool CanExecute(object parameter);

        public abstract event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);
    }
}
