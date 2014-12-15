using System;
using System.Xml;

using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Utilities;

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
        /// A position defined by the x and y components.
        /// Used for notification in situations where you don't
        /// want to have property notifications for X and Y
        /// </summary>
        public Point2D Position
        {
            get{return new Point2D(x,y);}
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
                //RaisePropertyChanged("Height");
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
                //RaisePropertyChanged("Width");
            }
        }

        public Rect2D Rect
        {
            get{return new Rect2D(x,y,width,height);}
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

        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        /// <summary>
        /// Set the width and the height of the node model
        /// and report once.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public void SetSize(double w, double h)
        {
            width = w;
            height = h;
            RaisePropertyChanged("Position");
        }

        #region Command Framework Supporting Methods

        public bool UpdateValue(string name, string value)
        {
            return this.UpdateValueCore(name, value);
        }

        /// <summary>
        /// This method is currently used as a way to send an event to ModelBase 
        /// derived objects. Its primary use is in DynamoNodeButton class, which 
        /// sends this event when clicked.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>Returns true if the call has been handled, or false otherwise.
        /// </returns>
        /// 
        public bool HandleModelEvent(string eventName)
        {
            return this.HandleModelEventCore(eventName);
        }

        /// <summary>
        /// This method is supplied as a generic way for command framework to update
        /// a given named-value in a ModelBase (which has to work under both user 
        /// and playback scenarios). During playback, the command framework issues 
        /// pre-recorded UpdateModelValueCommand that targets a model. Since there
        /// is no data-binding at play here, there will not be IValueConverter. This
        /// method takes only string input (the way they appear in DynamoTextBox),
        /// which overridden method can use for value conversion.
        /// </summary>
        /// <param name="name">The name of a property/value to update.</param>
        /// <param name="value">The new value to be set. This value comes directly
        /// from DynamoTextBox after user commits it. Overridden methods then use 
        /// a specific IValueConverter to turn this string into another data type 
        /// that it expects.</param>
        /// <returns>Returns true if the call has been handled, or false otherwise.
        /// </returns>
        /// 
        protected virtual bool UpdateValueCore(string name, string value)
        {
            return false; // Base class does not handle this.
        }

        protected virtual bool HandleModelEventCore(string eventName)
        {
            return false; // Base class does not handle this.
        }

        #endregion

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
        Rect2D Rect { get; }
        double CenterX { get; set; }
        double CenterY { get; set; }
        void ReportPosition();
    }
}