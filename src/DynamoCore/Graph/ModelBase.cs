using System;
using System.Xml;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Selection;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.Namespace;

namespace Dynamo.Graph
{
    /// <summary>
    /// SaveContext represents several contexts, in which node can be serialized/deserialized.
    /// </summary>
    public enum SaveContext { File, Copy, Undo, Preset };

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
        /// that it expects.</param>
        /// <param name="elementResolver">Responsible for resolving class namespaces.</param>
        public UpdateValueParams(string propertyName, string propertyValue, ElementResolver elementResolver = null)
        {
            ElementResolver = elementResolver;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        /// <summary>
        /// Returns name of the property whose value needs to be updated.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Returns string representation of value to update specified node property
        /// </summary>
        public string PropertyValue { get; private set; }

        /// <summary>
        /// Returns <see cref="ElementResolver"/> object responsible for resolving class namespaces
        /// </summary>
        public ElementResolver ElementResolver { get; private set; }
    }

    /// <summary>
    /// Base class for all objects with which user can interact in Dynamo.
    /// </summary>
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

        /// <summary>
        /// X coordinate of center point.
        /// </summary>
        [JsonIgnore]
        public double CenterX
        {
            get { return X + Width / 2; }
            set
            {
                X = value - Width / 2;
            }
        }

        /// <summary>
        /// Y coordinate of center point.
        /// </summary>
        [JsonIgnore]
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
        /// Override in derived classes to specify whether
        /// to serialize the X property.
        /// </summary>
        public virtual bool ShouldSerializeX()
        {
            return false;
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
        /// Override in derived classes to specify whether
        /// to serialize the Y property.
        /// </summary>
        public virtual bool ShouldSerializeY()
        {
            return false;
        }
        /// <summary>
        /// A position defined by the x and y components.
        /// Used for notification in situations where you don't
        /// want to have property notifications for X and Y
        /// </summary>
        [JsonIgnore]
        public Point2D Position
        {
            get { return new Point2D(x, y); }
        }

        /// <summary>
        /// The height of the object.
        /// </summary>
        [JsonIgnore]
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
        /// The width of the object.
        /// </summary>
        [JsonIgnore]
        public virtual double Width
        {
            get { return width; }
            set
            {
                width = value;
                //RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// The bounds of the object.
        /// </summary>
        [JsonIgnore]
        public virtual Rect2D Rect
        {
            get { return new Rect2D(x, y, width, height); }
        }

        /// <summary>
        /// Returns true if the object is selected otherwise false.
        /// </summary>
        [JsonIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Unique ID.
        /// </summary>
        [JsonProperty("Uuid")]
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

        /// <summary>
        /// Protected constructor.
        /// </summary>
        protected ModelBase()
        {
            GUID = Guid.NewGuid();
        }

        /// <summary>
        /// This method sets the isSelected property on the object to true.
        /// E.g. when user clicks on node.
        /// </summary>
        public virtual void Select()
        {
            IsSelected = true;
        }

        /// <summary>
        /// This method sets the isSelected property on the object to false.
        /// E.g. when user clicks on canvas.
        /// </summary>
        public virtual void Deselect()
        {
            IsSelected = false;
        }

        /// <summary>
        ///  Notifies listeners that the position of the object has changed,
        ///  then all dependant objects will also redraw themselves.
        /// </summary>
        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        /// <summary>
        /// Set the width and the height of the node model
        /// and report once.
        /// </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public void SetSize(double w, double h)
        {
            width = w;
            height = h;
            RaisePropertyChanged("Position");
        }

        /// <summary>
        /// Invokes Dispose on the object.
        /// </summary>
        public virtual void Dispose()
        {
            var handler = Disposed;
            if (handler != null)
            {
                handler(this);
            }
        }

        #region Command Framework Supporting Methods

        /// <summary>
        /// Updates object properties.
        /// UpdateValueCore is overridden in derived classes.
        /// </summary>
        /// <param name="updateValueParams">Please see UpdateValueParams for details.</param>
        /// <returns>Returns true if the call has been handled, or false otherwise.</returns>
        public bool UpdateValue(UpdateValueParams updateValueParams)
        {
            return UpdateValueCore(updateValueParams);
        }

        /// <summary>
        /// This method is currently used as a way to send an event to ModelBase
        /// derived objects. Its primary use is in DynamoNodeButton class, which
        /// sends this event when clicked, to change the number of ports in a
        /// VariableInputNode.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="value">For the SetInPortCount event, the number of
        /// ports desired.  Ignored for other events.</param>
        /// <param name="recorder"></param>
        /// <returns>Returns true if the call has been handled, or false otherwise.
        /// </returns>
        internal bool HandleModelEvent(string eventName, int value, UndoRedoRecorder recorder)
        {
            return HandleModelEventCore(eventName, value, recorder);
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

        internal virtual bool HandleModelEventCore(string eventName, int value, UndoRedoRecorder recorder)
        {
            return false; // Base class does not handle this.
        }

        #endregion

        #region Serialization/Deserialization Methods

        /// <summary>
        /// Serialize model into xml node.
        /// </summary>
        /// <param name="xmlDocument">Xml document</param>
        /// <param name="context">Context in which object is saved</param>
        /// <returns>xml node</returns>
        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            var element = CreateElement(xmlDocument, context);
            SerializeCore(element, context);
            return element;
        }

        /// <summary>
        /// Deserialize model from xml node.
        /// </summary>
        /// <param name="element">Xml node</param>
        /// <param name="context">Save context. E.g. save in file, copy node etc.</param>
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

        /// <summary>
        /// This event is fired when a message is logged.
        /// </summary>
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

    /// <summary>
    /// Interface contains definitions for locatable objects.
    /// Objects such as nodes, connectors, workspaces etc.
    /// </summary>
    public interface ILocatable
    {
        /// <summary>
        /// X coordinate of locatable object.
        /// </summary>
        double X { get; set; }

        /// <summary>
        /// Y coordinate of locatable object.
        /// </summary>
        double Y { get; set; }

        /// <summary>
        /// Width of locatable object.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of locatable object.
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Bounds of locatable object.
        /// </summary>
        Rect2D Rect { get; }

        /// <summary>
        /// X coordinate of center point.
        /// </summary>
        double CenterX { get; set; }

        /// <summary>
        /// Y coordinate of center point.
        /// </summary>
        double CenterY { get; set; }

        /// <summary>
        ///  Notify listeners that the position of the object has changed,
        ///  then all dependant objects will also redraw themselves.
        /// </summary>
        void ReportPosition();
    }
}