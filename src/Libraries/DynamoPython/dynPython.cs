using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using IronPython.Modules;

namespace Dynamo.Nodes
{
    [NodeName("LEGACY Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs an embedded IronPython script")]
    public class Python : NodeModel
    {
        public Python(WorkspaceModel workspaceModel) : base(workspaceModel) { }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", "1");
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN\b", "IN[0]");
                element.AppendChild(node);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    // not sure why this is still around...
    public abstract class VariableInput : NodeModel
    {
        public VariableInput(WorkspaceModel ws)
            : base(ws)
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        protected internal virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx));
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, ""));
                }
            }
            RegisterAllPorts();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, ""));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion
    }

    [NodeName("LEGACY Python Script With Variable Number of Inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs an embedded IronPython script")]
    public class PythonVarIn : VariableInput
    {
        // implement methods from variableinput
        public PythonVarIn(WorkspaceModel ws) : base(ws) { }

        protected override string GetInputRootName()
        {
            return "IN";
        }

        protected override string GetTooltipRootName()
        {
            return "Input";
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", xmlNode.GetAttribute("inputs"));
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {

                
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN[0-9]+\b", delegate(Match m)
                {
                    return "IN[" + m.ToString().Substring(2) + "]";
                });
                element.AppendChild(subNode.Clone());
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName("LEGACY Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs a IronPython script from a string")]
    public class PythonString : NodeModel
    {
        public PythonString(WorkspaceModel workspaceModel) : base(workspaceModel) { }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonStringNode", "Python Script From String");
            element.SetAttribute("inputcount", "2");

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
