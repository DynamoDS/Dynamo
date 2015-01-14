using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using Migrations;

namespace Dynamo.Nodes
{
    public class Identity : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Function.Identity", "Function.Identity@var");
        }
    }

    public class IsNull : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Object.IsNull", "Object.IsNull@var");
        }
    }

    public class ComposeFunctions : MigrationNode
    { 
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement composeNode = MigrationManager.CloneAndChangeName(oldNode,
                "DSCoreNodesUI.HigherOrder.ComposeFunctions", "Compose Function");
            composeNode.SetAttribute("inputcount", "2");
            migratedData.AppendNode(composeNode);

            return migratedData;
        }
    }

    public class Reverse : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Reverse",
                "List.Reverse@var[]..[]");
        }
    }

    public class NewList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement element = MigrationManager.CloneAndChangeName(oldNode,
                "DSCoreNodesUI.CreateList", "Create List");
            migrationData.AppendNode(element);

            int childNumber = oldNode.ChildNodes.Count;
            string childNumberString = childNumber.ToString();
            element.SetAttribute("inputcount", childNumberString);

            return migrationData;
        }
    }

    public class SortWith : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                "SortByComparsion", "SortByComparsion@var[]..[],_FunctionObject");

            migratedData.AppendNode(dsListNode);
            string dsListNodeId = MigrationManager.GetGuidFromXmlElement(dsListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsListNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class SortBy : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string nodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            PortId inPort0 = new PortId(nodeId, 0, PortType.Input);
            PortId inPort1 = new PortId(nodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(inPort0);
            XmlElement connector1 = data.FindFirstConnector(inPort1);

            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            migrationData.AppendNode(newNode);

            if (connector0 == null)
            {
                // If there is no key, migrate to List.Sort
                MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                    "List.Sort", "List.Sort@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds SortByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                "SortByKey", "SortByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData; 
        }
    }

    public class Sort : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Sort", "List.Sort@var[]..[]");
        }
    }

    public class ListMin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string nodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            PortId inPort0 = new PortId(nodeId, 0, PortType.Input);
            PortId inPort1 = new PortId(nodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(inPort0);
            XmlElement connector1 = data.FindFirstConnector(inPort1);

            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            migrationData.AppendNode(newNode);

            if (connector0 == null)
            {
                // If there is no key, migrate to List.MinimumItem
                MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                    "List.MinimumItem", "List.MinimumItem@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds MinimumItemByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                "MinimumItemByKey", "MinimumItemByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData;
        }
    }

    public class ListMax : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string nodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            PortId inPort0 = new PortId(nodeId, 0, PortType.Input);
            PortId inPort1 = new PortId(nodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(inPort0);
            XmlElement connector1 = data.FindFirstConnector(inPort1);

            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            migrationData.AppendNode(newNode);

            if (connector0 == null)
            {
                // If there is no key, migrate to List.MaximumItem
                MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                    "List.MaximumItem", "List.MaximumItem@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds MaximumItemByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                "MaximumItemByKey", "MaximumItemByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData;
        }
    }

  
    public class Fold : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Reduce", "Reduce");
            newNode.SetAttribute("inputcount", "3");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class FilterMask : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.FilterByBoolMask",
                "List.FilterByBoolMask@var[]..[],var[]..[]");
        }
    }

    public class FilterInAndOut : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Filter", "Filter");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Filter : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Filter", "Filter");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class FilterOut : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Filter", "Filter");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            PortId oldOutputPort = new PortId(oldNodeId, 0, PortType.Output);
            PortId newOutputPort = new PortId(newNodeId, 1, PortType.Output);

            if (data.FindConnectors(oldOutputPort) != null)
                foreach (XmlElement connector in data.FindConnectors(oldOutputPort))
                    data.ReconnectToPort(connector, newOutputPort);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    [AlsoKnownAs("Dynamo.Nodes.dynBuildSeq", "Dynamo.Nodes.BuildSeq")]
    public class NumberRange : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSCoreNodesUI.NumberRange", "Number Range");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class NumberSeq : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSCoreNodesUI.NumberSeq", "Number Sequence");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Combine : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Combine", "List.Combine");
            newNode.RemoveAttribute("inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute("inputs")) + 1;
            newNode.SetAttribute("inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class CartProd : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.CartesianProduct", "List.CartesianProduct");
            newNode.RemoveAttribute("inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute("inputs")) + 1;
            newNode.SetAttribute("inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class LaceShortest : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.LaceShortest", "List.LaceShortest");
            newNode.RemoveAttribute("inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute("inputs")) + 1;
            newNode.SetAttribute("inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class LaceLongest : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.LaceLongest", "List.Longest");
            newNode.RemoveAttribute("inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute("inputs")) + 1;
            newNode.SetAttribute("inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Map : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Map", "List.Map");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class ForEach : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                "ForEach", "__ForEach@_FunctionObject,var[]..[]");

            migratedData.AppendNode(dsListNode);
            string dsListNodeId = MigrationManager.GetGuidFromXmlElement(dsListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsListNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class AndMap : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                "TrueForAll", "TrueForAll@var[]..[],_FunctionObject");

            migratedData.AppendNode(dsListNode);
            string dsListNodeId = MigrationManager.GetGuidFromXmlElement(dsListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsListNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class OrMap : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                "TrueForAny", "TrueForAny@var[]..[],_FunctionObject");

            migratedData.AppendNode(dsListNode);
            string dsListNodeId = MigrationManager.GetGuidFromXmlElement(dsListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsListNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class DeCons : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Deconstruct",
                "List.Deconstruct@var[]..[]");
        }
    }

    public class List : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.AddItemToFront",
                "List.AddItemToFront@var,var[]..[]");
        }
    }

    public class TakeList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.TakeItems", "List.TakeItems@var[]..[],int");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class DropList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.DropItems", "List.DropItems@var[]..[],int");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class ShiftList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.ShiftIndices", "List.ShiftIndices@var[]..[],int");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class GetFromList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.GetItemAtIndex", "List.GetItemAtIndex@var[]..[],int");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class Shuffle : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Shuffle",
                "List.Shuffle@var[]..[]");
        }
    }

    public class GroupBy : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                "GroupByKey", "GroupByKey@var[]..[],_FunctionObject");

            migratedData.AppendNode(dsListNode);
            string dsListNodeId = MigrationManager.GetGuidFromXmlElement(dsListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsListNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class SliceList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            // Create nodes
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "List.Slice", "List.Slice@var[]..[],int,int,int");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement plus = MigrationManager.CreateFunctionNode(data.Document,
                oldNode, 0, "", "+", "+@,");
            migrationData.AppendNode(plus);
            string plusId = MigrationManager.GetGuidFromXmlElement(plus);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId plusInPort1 = new PortId(plusId, 1, PortType.Input);

            if (connector0 != null)
            {
                string startId = connector0.GetAttribute("start");
                data.CreateConnectorFromId(startId, 0, plusId, 0);
            }

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, plusInPort1);
            data.ReconnectToPort(connector2, newInPort0);
            data.CreateConnector(plus, 0, newNode, 2);

            // Add default value
            XmlElement defaultValue = data.Document.CreateElement("PortInfo");
            defaultValue.SetAttribute("index", "3");
            defaultValue.SetAttribute("default", "True");
            newNode.AppendChild(defaultValue);

            return migrationData;
        }
    }

    public class RemoveFromList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.RemoveItemAtIndex", "List.RemoveItemAtIndex@var[]..[],int[]");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class RemoveEveryNth : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.DropEveryNthItem", "List.DropEveryNthItem@var[]..[],int,int");

            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                dsCoreNode.AppendChild(child.Clone());

            return migratedData;
        }
    }

    public class TakeEveryNth : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.TakeEveryNthItem", "List.TakeEveryNthItem@var[]..[],int,int");

            foreach (XmlNode child in oldNode.ChildNodes)
            {
                dsCoreNode.AppendChild(child.CloneNode(true));
            }
            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsCoreNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsCoreNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                dsCoreNode.AppendChild(child.Clone());

            return migratedData;
        }
    }

    public class Empty : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Empty", "List.Empty");
        }
    }

    public class IsEmpty : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.IsEmpty",
                "List.IsEmpty@var[]..[]");
        }
    }

    public class Length : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Count",
                "List.Count@var[]..[]");
        }
    }

    public class Append : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement listJoinNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(listJoinNode, "DSCoreNodes.dll",
                "List.Join", "List.Join@var[]..[]");
            migratedData.AppendNode(listJoinNode);
            
            listJoinNode.SetAttribute("inputcount", "2");

            return migratedData;
        }
    }

    public class First : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.FirstItem",
                "List.FirstItem@var[]..[]");
        }
    }

    public class Last : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.LastItem",
                "List.LastItem@var[]..[]");
        }
    }

    public class Rest : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.RestOfItems",
                "List.RestOfItems@var[]..[]");
        }
    }

    public class Slice : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Chop",
                "List.Chop@var[]..[],int");
        }
    }

    public class DiagonalRightList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.DiagonalRight",
                "List.DiagonalRight@var[]..[],int");
        }
    }

    public class DiagonalLeftList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "List.DiagonalLeft", "List.DiagonalLeft@var[]..[],int");
            newNode.SetAttribute("lacing", "shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class Transpose : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Transpose", "List.Transpose@var[]..[]");
        }
    }
    
    public class Sublists : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "List.Sublists", "List.Sublists@var[]..[],var[]..[],int");
            newNode.SetAttribute("lacing", "shortest");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create code block node
            string rangesString = "{0}";
            foreach (XmlNode childNode in oldNode.ChildNodes)
            {
                if (childNode.Name.Equals(typeof(string).FullName))
                    rangesString = "{" + childNode.Attributes[0].Value + "};";
            }

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0, rangesString);
            migrationData.AppendNode(codeBlockNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            // Update connectors
            for (int idx = 0; true; idx++)
            {
                PortId oldInPort = new PortId(newNodeId, idx + 2, PortType.Input);
                PortId newInPort = new PortId(codeBlockNodeId, idx, PortType.Input);
                XmlElement connector = data.FindFirstConnector(oldInPort);

                if (connector == null)
                    break;

                data.ReconnectToPort(connector, newInPort);
            }

            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort2);
            data.CreateConnector(codeBlockNode, 0, newNode, 1);

            return migrationData;
        }
    }

    public class Repeat : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", "DSCoreNodes.dll");
            element.SetAttribute("nickname", "List.OfRepeatedItem");
            element.SetAttribute("function", "List.OfRepeatedItem@var[]..[],int");
            element.SetAttribute("lacing", "Shortest");

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    public class FlattenList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", "Flatten", "Flatten@var[]..[]");
        }
    }

    public class FlattenListAmt : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "List.Flatten", "List.Flatten@var[]..[],int");
        }
    }

    public class LessThan : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Compare.LessThan", "Compare.LessThan@var,var");
        }
    }

    public class LessThanEquals : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Compare.LessThanOrEqual", "Compare.LessThanOrEqual@var,var");
        }
    }

    public class GreaterThan : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Compare.GreaterThan", "Compare.GreaterThan@var,var");
        }
    }

    public class GreaterThanEquals : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Compare.GreaterThanOrEqual", "Compare.GreaterThanOrEqual@var,var");
        }
    }

    public class Equal : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "==", "==@,");
        }
    }

    public class And : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Logic.And", "And");
            newNode.SetAttribute("inputcount", "2");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Or : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCore.Logic.Or", "Or");
            newNode.SetAttribute("inputcount", "2");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Xor : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "Logic.Xor", "Logic.Xor@bool,bool");
        }
    }

    public class Not : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "Not", "Not@,");
        }
    }

    public class Addition : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "+", "+@,");
        }
    }

    public class Subtraction : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "-", "-@,");
        }
    }

    public class Multiplication : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "*", "*@,");
        }
    }

    public class Division : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "/", "/@,");
        }
    }

    public class Modulo : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "%", "%@,");
        }
    }

    public class Pow : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Pow", "Math.Pow@double,double");
        }
    }

    public class Round : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Round", "Math.Round@double");
        }
    }

    public class Floor : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Floor", "Math.Floor@double");
        }
    }

    public class Ceiling : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Ceiling", "Math.Ceiling@double");
        }
    }

    public class RandomSeed : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Random", "Math.Random@int");
        }
    }

    public class Random : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Rand", "Math.Rand");
        }
    }

    public class RandomList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.RandomList", "Math.RandomList@int");
        }
    }

    public class EConstant : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.E", "Math.E");
        }
    }

    public class Pi : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.PI", "Math.PI");
        }
    }

    [AlsoKnownAs("Dynamo.Nodes.2Pi", "Dynamo.Nodes.dyn2Pi")]
    public class PiTimes2 : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.PiTimes2", "Math.PiTimes2");
        }
    }

    public class Sin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "Math.Sin", "Math.Sin@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPortCBN = new PortId(converterNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortCBN);
            data.CreateConnector(converterNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class Cos : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "Math.Cos", "Math.Cos@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPortCBN = new PortId(converterNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortCBN);
            data.CreateConnector(converterNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class Tan : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "Math.Tan", "Math.Tan@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPortCBN = new PortId(converterNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortCBN);
            data.CreateConnector(converterNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class Asin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, "DSCoreNodes.dll",
                "Math.DegreesToRadians", "Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement asinNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.Asin", "Math.Asin@double");
            migrationData.AppendNode(asinNode);
            string asinNodeId = MigrationManager.GetGuidFromXmlElement(asinNode);

            // Update connectors
            PortId oldInPort0 = new PortId(converterNodeId, 0, PortType.Input);
            PortId newInPortAsin = new PortId(asinNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortAsin);
            data.CreateConnector(asinNode, 0, converterNode, 0);

            return migrationData;
        }
    }

    public class Acos : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, "DSCoreNodes.dll",
                "Math.DegreesToRadians", "Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement acosNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.Acos", "Math.Acos@double");
            migrationData.AppendNode(acosNode);
            string acosNodeId = MigrationManager.GetGuidFromXmlElement(acosNode);

            // Update connectors
            PortId oldInPort0 = new PortId(converterNodeId, 0, PortType.Input);
            PortId newInPortAcos = new PortId(acosNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortAcos);
            data.CreateConnector(acosNode, 0, converterNode, 0);

            return migrationData;
        }
    }

    public class Atan : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, "DSCoreNodes.dll",
                "Math.DegreesToRadians", "Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement atanNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "DSCoreNodes.dll",
                "Math.Atan", "Math.Atan@double");
            migrationData.AppendNode(atanNode);
            string atanNodeId = MigrationManager.GetGuidFromXmlElement(atanNode);

            // Update connectors
            PortId oldInPort0 = new PortId(converterNodeId, 0, PortType.Input);
            PortId newInPortAtan = new PortId(atanNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPortAtan);
            data.CreateConnector(atanNode, 0, converterNode, 0);

            return migrationData;
        }
    }

    public class Average : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Math.Average", "Math.Average@var[]");
        }
    }

    public class Smooth : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class Begin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            int inputCount = oldNode.ChildNodes.Count;
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, inputCount, 1);

            migrationData.AppendNode(dummyNode);
            return migrationData;
        }
    }

    public class ApplyList : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", "__ApplyList", "ApplyList@_FunctionObject,var[]..[]");
        }
    }

    public class Apply1 : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement applyNode = MigrationManager.CloneAndChangeName(oldNode, 
                "DSCoreNodesUI.HigherOrder.ApplyFunction","Apply Function");           

            int numberOfArgs = oldNode.ChildNodes.Count + 1;
            string numberOfArgsString = numberOfArgs.ToString();
            applyNode.SetAttribute("inputcount", numberOfArgsString);
            migratedData.AppendNode(applyNode);

            return migratedData;
        }
    }

    public class Conditional : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSCoreNodesUI.Logic.If", "If"));

            return migrationData;
        }
    }
    
    public class Breakpoint : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "DSCoreNodes.dll",
                "Object.Identity",
                "Object.Identity@var[]..[]");
        }
    }



    public class AngleInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "Math.DegreesToRadians", "Math.DegreesToRadians@double");
            migrationData.AppendNode(newNode);
          
            XmlElement numberNode = MigrationManager.CreateNode(data.Document, 
                oldNode, 0, "Dynamo.Nodes.DoubleInput", "Number");

            // Get attributes from old child node
            XmlElement newChild = data.Document.CreateElement("System.Double");

            foreach (XmlNode attr in oldNode.FirstChild.Attributes)
            {
                if (attr.Name.Equals("value"))
                    newChild.SetAttribute("value", attr.Value);
            }

            numberNode.AppendChild(newChild);
            migrationData.AppendNode(numberNode);

            data.CreateConnector(numberNode, 0, newNode, 0);
            return migrationData;
        }
    }

    public class DoubleSliderInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "Dynamo.Nodes.DoubleSlider", "Double Slider");

            // Get attributes from old child node
            XmlElement newChild1 = data.Document.CreateElement("System.Double");
            XmlElement newChild2 = data.Document.CreateElement("Range");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals("value"))
                        newChild1.InnerText = attr.Value;
                    else
                        newChild2.SetAttribute(attr.Name, attr.Value);
                }
            }

            newNode.AppendChild(newChild1);
            newNode.AppendChild(newChild2);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class IntegerSliderInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "Dynamo.Nodes.IntegerSlider", "Integer Slider");

            // Get attributes from old child node
            XmlElement newChild1 = data.Document.CreateElement("System.Int32");
            XmlElement newChild2 = data.Document.CreateElement("Range");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals("value"))
                        newChild1.InnerText = attr.Value;
                    else
                        newChild2.SetAttribute(attr.Name, attr.Value);
                }
            }

            newNode.AppendChild(newChild1);
            newNode.AppendChild(newChild2);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public partial class BoolSelector : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCoreNodesUI.BoolSelector", "Boolean");

            // Get attribute from old child node
            XmlElement newChild = data.Document.CreateElement("System.Boolean");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals("value"))
                        newChild.InnerText = attr.Value;
                }
            }

            newNode.AppendChild(newChild);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public partial class StringDirectory : StringFilename
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement original = data.MigratedNodes.ElementAt(0);
            var cloned = MigrationManager.CloneAndChangeName(original, "DSCore.File.Directory", "Directory Path");

            var document = original.OwnerDocument;
            foreach (XmlNode childNode in original.ChildNodes)
            {
                if (childNode.Name.Equals(typeof(string).FullName))
                {
                    var childElement = document.CreateElement(typeof(string).FullName);
                    childElement.InnerText = childNode.Attributes[0].Value;
                    cloned.AppendChild(childElement);
                }
            }

            migrationData.AppendNode(cloned);
            return migrationData;            
        }
    }

    public partial class StringFilename : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement original = data.MigratedNodes.ElementAt(0);
            var cloned = MigrationManager.CloneAndChangeName(original, "DSCore.File.Filename", "File Path");

            var document = original.OwnerDocument;
            foreach (XmlNode childNode in original.ChildNodes)
            {
                if (childNode.Name.Equals(typeof(string).FullName))
                {
                    var childElement = document.CreateElement(typeof(string).FullName);
                    childElement.InnerText = childNode.Attributes[0].Value;
                    cloned.AppendChild(childElement);
                }
            }

            migrationData.AppendNode(cloned);
            return migrationData;
        }
    }

    public class ConcatStrings : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement stringNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(stringNode, "DSCoreNodes.dll",
                "String.Concat", "String.Concat@string[]");
            migratedData.AppendNode(stringNode);

            int numberOfArgs = oldNode.ChildNodes.Count + 2;
            string numberOfArgsString = numberOfArgs.ToString();

            stringNode.SetAttribute("inputcount", numberOfArgsString);

            return migratedData;
        }
    }

    public class String2Num : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.ToNumber", "String.ToNumber@string");
        }
    }

    public class Num2String : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.FromObject", "String.FromObject@var");
        }
    }

    public class StringLen : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.Length", "String.Length@string");
        }
    }

    public class ToString : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.FromObject", "String.FromObject@var");
        }
    }

    public class SplitString : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "String.Split", "String.Split@string,string[]");
            newNode.SetAttribute("inputcount", "2");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                if (newChild.GetAttribute("index") == "1")
                    newChild.SetAttribute("index", "0");

                newNode.AppendChild(newChild);
            }

            return migrationData;
        }
    }

    public class JoinStrings : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, "DSCoreNodes.dll",
                "List.GetItemAtIndex", "List.GetItemAtIndex@var[]..[],int");
            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);


            XmlElement stringJoinNode = MigrationManager.CreateVarArgFunctionNode(
                data.Document, oldNode, 1, "DSCoreNodes.dll",
                "String.Join", "String.Join@string,string[]", "2");
            migratedData.AppendNode(stringJoinNode);
            string stringJoinNodeId = MigrationManager.GetGuidFromXmlElement(stringJoinNode);


            XmlElement cbn = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 2, "0");
            migratedData.AppendNode(cbn);
            string cbnId = MigrationManager.GetGuidFromXmlElement(cbn);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(stringJoinNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(stringJoinNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);
            data.CreateConnector(stringJoinNode, 0, dsCoreNode, 0);
            data.CreateConnector(cbn, 0, dsCoreNode, 1);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                if (newChild.GetAttribute("index") == "1")
                    newChild.SetAttribute("index", "0");

                stringJoinNode.AppendChild(newChild);
            }

            return migratedData;           
        }
    }

    public class StringCase : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.ChangeCase", "String.ChangeCase@string,bool");
        }
    }

    public class Substring : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "String.Substring", "String.Substring@string,int,int");
        }
    }
}
