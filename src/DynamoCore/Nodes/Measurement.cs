using System;
using System.Globalization;
using System.Xml;
using Dynamo.Units;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    public abstract partial class MeasurementInputBase : NodeWithOneOutput
    {
        protected SIUnit _measure;

        public double Value
        {
            get
            {
                return _measure.Value;
            }
            set
            {
                _measure.Value = value;
                RaisePropertyChanged("Value");
            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewContainer(_measure);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
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
    }

    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Enter a length.")]
    [NodeSearchTags("Imperial", "Metric", "Length", "Project", "units")]
    public class LengthInput : MeasurementInputBase
    {
        public LengthInput()
        {
            _measure = new Units.Length(0.0, dynSettings.Controller.UnitsManager);
            OutPortData.Add(new PortData("length", "The length. Stored internally as decimal meters.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        [NodeMigration(from:"0.6.2")]
        public void MigrateLengthFromFeetToMeters(XmlNode node)
        {
            //length values were previously stored as decimal feet
            //convert them internally to SI meters.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "System.Double")
                {
                    if (child.Attributes != null && child.Attributes.Count > 0)
                    {
                        var valueAttrib = child.Attributes["value"];
                        valueAttrib.Value = (double.Parse(valueAttrib.Value)/SIUnit.ToFoot).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }

    [NodeName("Area")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Enter an area.")]
    [NodeSearchTags("Imperial", "Metric", "Area", "Project", "units")]
    public class AreaInput : MeasurementInputBase
    {
        public AreaInput()
        {
            _measure = new Area(0.0, dynSettings.Controller.UnitsManager);
            OutPortData.Add(new PortData("area", "The area. Stored internally as decimal meters squared.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }
    }

    [NodeName("Volume")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Enter a volume.")]
    [NodeSearchTags("Imperial", "Metric", "volume", "Project", "units")]
    public class VolumeInput : MeasurementInputBase
    {
        public VolumeInput()
        {
            _measure = new Volume(0.0, dynSettings.Controller.UnitsManager);
            OutPortData.Add(new PortData("volume", "The volume. Stored internally as decimal meters cubed.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }
    }

    public abstract class UnitFromNumberBase : NodeWithOneOutput
    {
        protected UnitFromNumberBase()
        {
            InPortData.Add(new PortData("value", "A number to be converted to a unit.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("unit", "The unit. Stored internally as SI units.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;

            ((PreferenceSettings)dynSettings.Controller.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;
        }

        public virtual void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return
                FScheme.Value.NewContainer(
                    ConvertToMeasurement(((FScheme.Value.Number)args[0]).Item));
        }

        protected abstract SIUnit ConvertToMeasurement(double value);
    }

    [NodeName("Length from Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Create a length unit from a number.")]
    [NodeSearchTags("Imperial", "Metric", "Length", "Project", "units")]
    public class LengthFromNumber : UnitFromNumberBase
    {
        protected override SIUnit ConvertToMeasurement(double value)
        {
            return new Units.Length(value / dynSettings.Controller.UnitsManager.UiLengthConversion, dynSettings.Controller.UnitsManager);
        }

        public override void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LengthUnit")
            {
                RequiresRecalc = true;
            }
        }
    }

    [NodeName("Area from Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Create an area unit from a number.")]
    [NodeSearchTags("Imperial", "Metric", "Area", "Project", "units")]
    public class AreaFromNumber : UnitFromNumberBase
    {
        protected override SIUnit ConvertToMeasurement(double value)
        {
            return new Area(value / dynSettings.Controller.UnitsManager.UiAreaConversion, dynSettings.Controller.UnitsManager);
        }

        public override void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AreaUnit")
            {
                RequiresRecalc = true;
            }
        }
    }

    [NodeName("Volume from Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Create a volume unit from a number.")]
    [NodeSearchTags("Imperial", "Metric", "Volume", "Project", "units")]
    public class VolumeFromNumber : UnitFromNumberBase
    {
        protected override SIUnit ConvertToMeasurement(double value)
        {
            return new Volume(value / dynSettings.Controller.UnitsManager.UiVolumeConversion, dynSettings.Controller.UnitsManager);
        }

        public override void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "VolumeUnit")
            {
                RequiresRecalc = true;
            }
        }
    }
}

