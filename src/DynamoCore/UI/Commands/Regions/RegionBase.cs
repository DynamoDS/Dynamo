using System;
using Dynamo.UI.Commands;

namespace Dynamo.Search.Regions
{
    public abstract class RegionBase : DelegateCommand
    {
        public bool Loaded { get; set; }

        protected RegionBase(Action<object> executeMethod, System.Predicate<object> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            Loaded = false;
        }

    }
}
