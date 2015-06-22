using System;
using System.Xml;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Selection;
using Dynamo.Utilities;
using ProtoCore.Namespace;

namespace Dynamo.Models
{
    public enum SaveContext { File, Copy, Undo };

    /// <summary>
    /// This class encapsulates the input parameters that need to be passed into nodes
    /// when they are updated in the UI.
    /// </summary>
    public class UpdateValueParams
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName">Name of the property whose value is to be updated.
        /// This parameter cannot be empty or null.</param>
        /// <param name="propertyValue">Value of the named property whose value is to be 
        /// updated. This parameter can either be null or empty if the targeted property 
        /// allows such values.This value comes directly
        /// from DynamoTextBox after user commits it. Overridden methods then use 
        /// a specific IValueConverter to turn this string into another data type 
        /// that it expects</param>
        /// <param name="elementResolver">responsible for resolving class namespaces</param>
        public UpdateValueParams(string propertyName, string propertyValue, ElementResolver elementResolver = null)
        {
            ElementResolver = elementResolver;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public string PropertyName { get; private set; }
        public string PropertyValue { get; private set; }
        public ElementResolver ElementResolver { get; private set; }
    }

    public abstract class ModelBase : NotificationObject, ISelectable, ILocatable, ILogSource
    {
        /// <summary>
        /// Fired when this Model is disposed.
        /// </summary>
        public event Action<ModelBase> Disposed;

        private Guid guid;
        private bool isSelected;
        private double x;
        private double y;
        private double height = 100;
        private double width = 100;
       
        public double CenterX
        {
            get { return X + Width / 2; }
            set { 
                X = value - Width/2;
            }
        }

        public double CenterY
        {
            get { return Y + Height / 2; }
            set
            {
                Y = value - Height / 2;
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
        public virtual double Height
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
        public virtual double Width
        {
            get { return width; }
            set

            {
                width = value;
                //RaisePropertyChanged("Width");
            }
        }

        public virtual Rect2D Rect
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
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public Guid GUID
        {
            get
            {
                if (guid == null)
                {
                    throw new Exception("GUID on model must never be null");
                }
                return guid;
            }
            set
            {
                guid = value;
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

        public virtual void Dispose()
        {
            var handler = Disposed;
            if (handler != null)
                handler(this);
        }

        #region Command Framework Supporting Methods

        public bool UpdateValue(UpdateValueParams updateValueParams)
        {
            return UpdateValueCore(updateValueParams);
        }

        /// <summary>
        /// This method is currently used as a way to send an event to ModelBase 
        /// derived objects. Its primary use is in DynamoNodeButton class, which 
        /// sends this event when clicked.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="recorder"></param>
        /// <returns>Returns true if the call has been handled, or false otherwise.
        /// </returns>
        public bool HandleModelEvent(string eventName, UndoRedoRecorder recorder)
        {
            return HandleModelEventCore(eventName, recorder);
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
        /// <param name="updateValueParams">Please see UpdateValueParams for details.</param>
        /// <returns>Returns true if the call has been handled, or false otherwise.
        /// </returns>
        protected virtual bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            return false; // Base class does not handle this.
        }

        protected virtual bool HandleModelEventCore(string eventName, UndoRedoRecorder recorder)
        {
            return false; // Base class does not handle this.
        }

        #endregion

        #region Serialization/Deserialization Methods

        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            var element = CreateElement(xmlDocument, context);
            SerializeCore(element, context);
            return element;
        }

        public void Deserialize(XmlElement element, SaveContext context)
        {
            DeserializeCore(element, context);
        }

        protected virtual XmlElement CreateElement(XmlDocument xmlDocument, SaveContext context)
        {
            string typeName = GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            return element;
        }
        
        protected abstract void SerializeCore(XmlElement element, SaveContext context);
        protected abstract void DeserializeCore(XmlElement nodeElement, SaveContext context);

        #endregion

        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }
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