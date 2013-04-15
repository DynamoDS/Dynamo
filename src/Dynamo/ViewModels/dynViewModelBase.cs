using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    abstract class dynViewModelBase : NotificationObject
    {
        private string _name;
        private Guid _guid;

        /// <summary>
        /// The name of the view model object.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(("Name"));
            }
        }

        /// <summary>
        /// The unique identifier of the view model object.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                RaisePropertyChanged(("Guid"));
            }
        }

        protected dynViewModelBase(string name)
        {
            _name = name;
            _guid = Guid.NewGuid();
        }
    }
}
