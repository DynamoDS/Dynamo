using System;
using System.Globalization;
using System.Xml;
using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : DSCoreNodesUI.Integer
    {
        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Value = 0;

            ShouldDisplayPreviewCore = false;
        }

        //Value property has to be changed for over flow condition
        //So instead of having it in modelbase, this property is defined here.
        private int _value;
        public new int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RequiresRecalc = true;
                RaisePropertyChanged("Value");
            }
        }
            

        private int _max;
        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;

                if (_max < Value)
                    Value = _max;

                RaisePropertyChanged("Max");
            }
        }

        private int _min;
        public int Min
        {
            get { return _min; }
            set
            {
                _min = value;

                if (_min > Value)
                    Value = _min;

                RaisePropertyChanged("Min");
            }
        }

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            var converter = new IntegerDisplay();
            switch (name)
            {
                case "Value":
                    Value = ((int)converter.ConvertBack(value, typeof(int), null, null));
                     if (Value >= Max)
                    {
                        this.Max = Value;
                    }
                    if (Value <= Min)
                    {
                        this.Min = Value;
                    }
                    return true; // UpdateValueCore handled.
                case "Max":
                    Max = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true; // UpdateValueCore handled.
                case "Min":
                    Min = ((int)converter.ConvertBack(value, typeof(int), null, null));
                    return true;
            }

            return base.UpdateValueCore(name, value, recorder);
        }
        
        #region Serialization/Deserialization Methods

<<<<<<< HEAD

        #region Load/Save

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
=======
        protected override void SerializeCore(XmlElement element, SaveContext context)
>>>>>>> f8be37756a6f9a1412ce19b1b141e3935df6efad
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            XmlElement outEl = element.OwnerDocument.CreateElement("Range");
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            element.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); // Base implementation must be called.

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;

                int min = Min;
                int max = Max;

                if (subNode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("value"))
                            Value = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }
                }

                Min = min;
                Max = max;
            }
        }

        #endregion
<<<<<<< HEAD

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            if (context == SaveContext.Undo)
            {
                var xmlDocument = element.OwnerDocument;
                XmlElement subNode = xmlDocument.CreateElement("Range");
                subNode.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
                subNode.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); // Base implementation must be called.

            if (context == SaveContext.Undo)
            {
                foreach (XmlNode subNode in element.ChildNodes)
                {
                    if (!subNode.Name.Equals("Range"))
                        continue;
                    if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                        continue;

                    int min = Min;
                    int max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("min"))
                            min = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                        else if (attr.Name.Equals("max"))
                            max = Convert.ToInt32(attr.Value, CultureInfo.InvariantCulture);
                    }

                    Min = min;
                    Max = max;
                }
            }
        }

        #endregion

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }

=======
>>>>>>> f8be37756a6f9a1412ce19b1b141e3935df6efad
    }
}
