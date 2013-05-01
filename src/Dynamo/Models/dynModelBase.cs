using System;
using System.Windows;
using Dynamo.Selection;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynModelBase : NotificationObject, ISelectable, ILocatable
    {
        private Guid _guid;
        private bool _isSelected = false;
        private double x = 0.0;
        private double y = 0.0;
        private double height = 100;
        private double width = 100;

        /// <summary>
        /// The X coordinate of the node in canvas space.
        /// </summary>
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        /// The Y coordinate of the node in canvas space.
        /// </summary>
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                RaisePropertyChanged("Y");
            }
        }

        /// <summary>
        /// The height of the node.
        /// </summary>
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        /// The width of the node.
        /// </summary>
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        public Rect Rect
        {
            get{return new Rect(x,y,width,height);}
        }

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

    public interface ILocatable
    {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        Rect Rect { get; }
    }
}