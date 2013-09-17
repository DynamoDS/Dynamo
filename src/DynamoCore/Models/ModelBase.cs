using System;
using System.Windows;
using System.Xml;
using Dynamo.Selection;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Models
{
    public enum SaveContext { File, Copy, Undo };

    public abstract class ModelBase : NotificationObject, ISelectable, ILocatable
    {
        private Guid _guid;
        private bool _isSelected = false;
        private double x = 0.0;
        private double y = 0.0;
        private double height = 100;
        private double width = 100;
        
        public double CenterX
        {
            get { return X + Width / 2; }
            set { 
                this.X = value - this.Width/2;
            }
        }

        public double CenterY
        {
            get { return Y + Height / 2; }
            set
            {
                this.Y = value - this.Height / 2;
            }
        }

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

        public event EventHandler Updated; 
        public void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
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

        protected ModelBase()
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

        #region Serialization/Deserialization Methods

        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            string typeName = this.GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            this.SerializeCore(element, context);
            return element;
        }

        public void Deserialize(XmlElement element, SaveContext context)
        {
            this.DeserializeCore(element, context);
        }

        protected abstract void SerializeCore(XmlElement element, SaveContext context);
        protected abstract void DeserializeCore(XmlElement element, SaveContext context);

        #endregion

    }

    public interface ILocatable
    {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        Rect Rect { get; }
        double CenterX { get; set; }
        double CenterY { get; set; }
    }
}