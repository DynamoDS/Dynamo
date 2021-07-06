using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using DynamoUnits;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UnitsUI
{
    /// <summary>
    /// Class defining the behaviour of a nodemodel which has no inputs, but rather uses wpf comboboxes to select items from collections.
    /// It then outputs a formatted string from the selections made.
    /// </summary>
    public abstract class MeasurementInputBase : NodeModel
    {
        [JsonIgnore]
        public SIUnit Measure { get; protected set; }
        /// <summary>
        /// Numerical value entered into the textbox (to be formatted).
        /// </summary>
        public double Value
        {
            get
            {
                return Measure.Value;
            }
            set
            {
                Measure.Value = value;
                RaisePropertyChanged(nameof(Value));
            }
        }

        public MeasurementInputBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public MeasurementInputBase() : base() { }

        internal void ForceValueRaisePropertyChanged()
        {
            RaisePropertyChanged(nameof(Value));
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                // this node now stores a double, having previously stored a measure type
                // by checking for the measure type as well we allow for loading of older files.
                if (subNode.Name.Equals(typeof(double).FullName) || subNode.Name.Equals("Dynamo.Measure.Foot"))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        protected double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == nameof(Value))
            {
                var converter = new MeasureConverter();
                this.Value = ((double)converter.ConvertBack(value, typeof(double), Measure, null));
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }

    }
}
