using System;
using Dynamo.Selection;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynModelBase : NotificationObject, ISelectable
    {
        private Guid _guid;
        private bool _isSelected = false;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public Guid GUID
        {
            get
            {
                if (_guid == null)
                {
                    throw new Exception("GUID on model must never be null");
                }
                return _guid;
            }
            set
            {
                _guid = value;
                RaisePropertyChanged("GUID");
            }
        }

        protected dynModelBase()
        {
            GUID = Guid.NewGuid();
        }

        public virtual void Select()
        {
            IsSelected = true;
        }

        public virtual void Deselect()
        {
            IsSelected = false;
        }
    }
}