using System;
using System.Xml;
using Dynamo.Graph.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Graph
{
    /// <summary>
    /// Connector Pin Model
    /// </summary>
    public class ConnectorPinModel : ModelBase
    {
        private readonly double height = 30;

        /// <summary>
        /// Connector Id
        /// </summary>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// Required when needing to pull a reliable value for objects of this
        /// type when there is no instances of this object type.
        /// </summary>
        public static double StaticWidth { get; } = 30;

        /// <summary>
        /// Static height of pin
        /// </summary>
        public override double Height => height;

        /// <summary>
        /// Static width of pin
        /// </summary>
        public override double Width { get => StaticWidth; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="id"></param>
        /// <param name="connectorId"></param>
        public ConnectorPinModel(double x, double y, Guid id, Guid connectorId)
        {
            X = x;
            Y = y;
            GUID = id;
            ConnectorId = connectorId;
        }


        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            return base.UpdateValueCore(updateValueParams);
        }
        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("connectorId", ConnectorId);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);
            GUID = helper.ReadGuid("guid", GUID);
            ConnectorId = helper.ReadGuid("connectorId");
            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);

            // Notify listeners that the position of the note has changed, 
            // then parent group will also redraw itself.
            ReportPosition();
        }

        #endregion
    }
}
