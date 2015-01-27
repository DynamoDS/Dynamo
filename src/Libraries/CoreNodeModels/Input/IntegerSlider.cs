using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;

using Dynamo.Models;
using Dynamo.Nodes;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI.Input
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("IntegerSliderNodeDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : SliderBase<int>
    {
        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 0;
            ShouldDisplayPreviewCore = false;
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "Min":
                case "MinText":
                    Min = ConvertStringToInt(value);
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = ConvertStringToInt(value);
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = ConvertStringToInt(value);
                    return true; // UpdateValueCore handled.
                case "Step":
                case "StepText":
                    Step = ConvertStringToInt(value);
                    return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] {assignment};
        }

        protected override int DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var xmlDocument = element.OwnerDocument;
            var subNode = xmlDocument.CreateElement("Range");
            subNode.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("step", Step.ToString(CultureInfo.InvariantCulture));
            element.AppendChild(subNode);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.

            foreach (XmlNode subNode in element.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;
                if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                    continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "min":
                            Min = ConvertStringToInt(attr.Value);
                            break;
                        case "max":
                            Max = ConvertStringToInt(attr.Value);
                            break;
                        case "step":
                            Step = ConvertStringToInt(attr.Value);
                            break;
                        default:
                            Log(string.Format("{0} attribute could not be deserialized for {1}", attr.Name, GetType()));
                            break;
                    }
                }

                break;
            }
        }

        #endregion
    }
}

namespace Dynamo.Nodes
{
    public class IntegerSlider
    {
        [NodeMigration(@from: "0.7.5.0")]
        public static NodeMigrationData Migrate_0750(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCoreNodesUI.Input.IntegerSlider", "Integer Slider", true);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}
