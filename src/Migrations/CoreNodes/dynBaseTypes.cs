﻿using System;
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Function.Identity", /*NXLT*/"Function.Identity@var");
        }
    }

    public class IsNull : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Object.IsNull", /*NXLT*/"Object.IsNull@var");
        }
    }

    public class ComposeFunctions : MigrationNode
    { 
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement composeNode = MigrationManager.CloneAndChangeName(oldNode,
                /*NXLT*/"DSCoreNodesUI.HigherOrder.ComposeFunctions", /*NXLT*/"Compose Function");
            composeNode.SetAttribute(/*NXLT*/"inputcount", /*NXLT*/"2");
            migratedData.AppendNode(composeNode);

            return migratedData;
        }
    }

    public class Reverse : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Reverse",
                /*NXLT*/"List.Reverse@var[]..[]");
        }
    }

    public class NewList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement element = MigrationManager.CloneAndChangeName(oldNode,
                /*NXLT*/"DSCoreNodesUI.CreateList", /*NXLT*/"Create List");
            migrationData.AppendNode(element);

            int childNumber = oldNode.ChildNodes.Count;
            string childNumberString = childNumber.ToString();
            element.SetAttribute(/*NXLT*/"inputcount", childNumberString);

            return migrationData;
        }
    }

    public class SortWith : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                /*NXLT*/"SortByComparsion", /*NXLT*/"SortByComparsion@var[]..[],_FunctionObject");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
                MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                    /*NXLT*/"List.Sort", /*NXLT*/"List.Sort@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds SortByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                /*NXLT*/"SortByKey", /*NXLT*/"SortByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData; 
        }
    }

    public class Sort : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Sort", /*NXLT*/"List.Sort@var[]..[]");
        }
    }

    public class ListMin : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
                MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                    /*NXLT*/"List.MinimumItem", /*NXLT*/"List.MinimumItem@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds MinimumItemByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                /*NXLT*/"MinimumItemByKey", /*NXLT*/"MinimumItemByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData;
        }
    }

    public class ListMax : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
                MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                    /*NXLT*/"List.MaximumItem", /*NXLT*/"List.MaximumItem@var[]..[]");

                PortId newInPort1 = new PortId(nodeId, 1, PortType.Input);
                data.ReconnectToPort(connector1, inPort0);

                return migrationData;
            }

            // If there is key, migrate to FunctionObject.ds MaximumItemByKey
            MigrationManager.SetFunctionSignature(newNode, "",
                /*NXLT*/"MaximumItemByKey", /*NXLT*/"MaximumItemByKey@var[]..[],_FunctionObject");

            // Update connectors
            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort0);

            return migrationData;
        }
    }

  
    public class Fold : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Reduce", /*NXLT*/"Reduce");
            newNode.SetAttribute(/*NXLT*/"inputcount", /*NXLT*/"3");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class FilterMask : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",/*NXLT*/ "List.FilterByBoolMask",
                /*NXLT*/"List.FilterByBoolMask@var[]..[],var[]..[]");
        }
    }

    public class FilterInAndOut : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Filter", /*NXLT*/"Filter");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Filter", /*NXLT*/"Filter");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Filter", /*NXLT*/"Filter");
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

    [AlsoKnownAs(/*NXLT*/"Dynamo.Nodes.dynBuildSeq", /*NXLT*/"Dynamo.Nodes.BuildSeq")]
    public class NumberRange : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, /*NXLT*/"DSCoreNodesUI.NumberRange", /*NXLT*/"Number Range");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class NumberSeq : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, /*NXLT*/"DSCoreNodesUI.NumberSeq", /*NXLT*/"Number Sequence");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Combine : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Combine", /*NXLT*/"List.Combine");
            newNode.RemoveAttribute(/*NXLT*/"inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute(/*NXLT*/"inputs")) + 1;
            newNode.SetAttribute(/*NXLT*/"inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class CartProd : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.CartesianProduct", /*NXLT*/"List.CartesianProduct");
            newNode.RemoveAttribute(/*NXLT*/"inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute(/*NXLT*/"inputs")) + 1;
            newNode.SetAttribute(/*NXLT*/"inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class LaceShortest : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.LaceShortest", /*NXLT*/"List.LaceShortest");
            newNode.RemoveAttribute(/*NXLT*/"inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute("inputs")) + 1;
            newNode.SetAttribute(/*NXLT*/"inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class LaceLongest : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.LaceLongest", /*NXLT*/"List.Longest");
            newNode.RemoveAttribute(/*NXLT*/"inputs");
            int numberOfInputs = Convert.ToInt32(oldNode.GetAttribute(/*NXLT*/"inputs")) + 1;
            newNode.SetAttribute(/*NXLT*/"inputcount", Convert.ToString(numberOfInputs));

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Map : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Map", /*NXLT*/"List.Map");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                /*NXLT*/"ForEach", /*NXLT*/"__ForEach@_FunctionObject,var[]..[]");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                /*NXLT*/"TrueForAll", /*NXLT*/"TrueForAll@var[]..[],_FunctionObject");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                /*NXLT*/"TrueForAny", /*NXLT*/"TrueForAny@var[]..[],_FunctionObject");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Deconstruct",
                /*NXLT*/"List.Deconstruct@var[]..[]");
        }
    }

    public class List : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.AddItemToFront",
                /*NXLT*/"List.AddItemToFront@var,var[]..[]");
        }
    }

    public class TakeList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.TakeItems", /*NXLT*/"List.TakeItems@var[]..[],int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.DropItems", /*NXLT*/"List.DropItems@var[]..[],int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.ShiftIndices", /*NXLT*/"List.ShiftIndices@var[]..[],int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.GetItemAtIndex", /*NXLT*/"List.GetItemAtIndex@var[]..[],int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Shuffle",
                /*NXLT*/"List.Shuffle@var[]..[]");
        }
    }

    public class GroupBy : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsListNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsListNode, "",
                /*NXLT*/"GroupByKey", /*NXLT*/"GroupByKey@var[]..[],_FunctionObject");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            // Create nodes
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.Slice", /*NXLT*/"List.Slice@var[]..[],int,int,int");
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
                string startId = connector0.GetAttribute(/*NXLT*/"start");
                data.CreateConnectorFromId(startId, 0, plusId, 0);
            }

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, plusInPort1);
            data.ReconnectToPort(connector2, newInPort0);
            data.CreateConnector(plus, 0, newNode, 2);

            // Add default value
            XmlElement defaultValue = data.Document.CreateElement(/*NXLT*/"PortInfo");
            defaultValue.SetAttribute(/*NXLT*/"index", /*NXLT*/"3");
            defaultValue.SetAttribute(/*NXLT*/"default", /*NXLT*/"True");
            newNode.AppendChild(defaultValue);

            return migrationData;
        }
    }

    public class RemoveFromList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.RemoveItemAtIndex", /*NXLT*/"List.RemoveItemAtIndex@var[]..[],int[]");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.DropEveryNthItem", /*NXLT*/"List.DropEveryNthItem@var[]..[],int,int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.TakeEveryNthItem", /*NXLT*/"List.TakeEveryNthItem@var[]..[],int,int");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Empty", /*NXLT*/"List.Empty");
        }
    }

    public class IsEmpty : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.IsEmpty",
                /*NXLT*/"List.IsEmpty@var[]..[]");
        }
    }

    public class Length : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Count",
                /*NXLT*/"List.Count@var[]..[]");
        }
    }

    public class Append : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement listJoinNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(listJoinNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.Join", /*NXLT*/"List.Join@var[]..[]");
            migratedData.AppendNode(listJoinNode);

            listJoinNode.SetAttribute(/*NXLT*/"inputcount", /*NXLT*/"2");

            return migratedData;
        }
    }

    public class First : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.FirstItem",
                /*NXLT*/"List.FirstItem@var[]..[]");
        }
    }

    public class Last : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.LastItem",
                /*NXLT*/"List.LastItem@var[]..[]");
        }
    }

    public class Rest : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.RestOfItems",
                /*NXLT*/"List.RestOfItems@var[]..[]");
        }
    }

    public class Slice : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Chop",
                /*NXLT*/"List.Chop@var[]..[],int");
        }
    }

    public class DiagonalRightList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.DiagonalRight",
                /*NXLT*/"List.DiagonalRight@var[]..[],int");
        }
    }

    public class DiagonalLeftList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.DiagonalLeft", /*NXLT*/"List.DiagonalLeft@var[]..[],int");
            newNode.SetAttribute(/*NXLT*/"lacing", /*NXLT*/"shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class Transpose : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Transpose", /*NXLT*/"List.Transpose@var[]..[]");
        }
    }
    
    public class Sublists : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.Sublists", /*NXLT*/"List.Sublists@var[]..[],var[]..[],int");
            newNode.SetAttribute(/*NXLT*/"lacing", /*NXLT*/"shortest");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create code block node
            string rangesString = /*NXLT*/"{0}";
            foreach (XmlNode childNode in oldNode.ChildNodes)
            {
                if (childNode.Name.Equals(typeof(string).FullName))
                    rangesString = /*NXLT*/"{" + childNode.Attributes[0].Value + /*NXLT*/"};";
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute(/*NXLT*/"assembly", /*NXLT*/"DSCoreNodes.dll");
            element.SetAttribute(/*NXLT*/"nickname", /*NXLT*/"List.OfRepeatedItem");
            element.SetAttribute(/*NXLT*/"function", /*NXLT*/"List.OfRepeatedItem@var[]..[],int");
            element.SetAttribute(/*NXLT*/"lacing", /*NXLT*/"Shortest");

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    public class FlattenList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", /*NXLT*/"Flatten", /*NXLT*/"Flatten@var[]..[]");
        }
    }

    public class FlattenListAmt : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"List.Flatten", /*NXLT*/"List.Flatten@var[]..[],int");
        }
    }

    public class LessThan : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Compare.LessThan", /*NXLT*/"Compare.LessThan@var,var");
        }
    }

    public class LessThanEquals : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Compare.LessThanOrEqual", /*NXLT*/"Compare.LessThanOrEqual@var,var");
        }
    }

    public class GreaterThan : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Compare.GreaterThan", /*NXLT*/"Compare.GreaterThan@var,var");
        }
    }

    public class GreaterThanEquals : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Compare.GreaterThanOrEqual", /*NXLT*/"Compare.GreaterThanOrEqual@var,var");
        }
    }

    public class Equal : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"==", /*NXLT*/"==@,");
        }
    }

    public class And : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Logic.And", /*NXLT*/"And");
            newNode.SetAttribute(/*NXLT*/"inputcount", /*NXLT*/"2");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Or : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, /*NXLT*/"DSCore.Logic.Or", /*NXLT*/"Or");
            newNode.SetAttribute(/*NXLT*/"inputcount", /*NXLT*/"2");
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    public class Xor : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"Logic.Xor", /*NXLT*/"Logic.Xor@bool,bool");
        }
    }

    public class Not : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"Not", /*NXLT*/"Not@,");
        }
    }

    public class Addition : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"+", /*NXLT*/"+@,");
        }
    }

    public class Subtraction : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"-",/*NXLT*/ "-@,");
        }
    }

    public class Multiplication : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"*", /*NXLT*/"*@,");
        }
    }

    public class Division : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"/",/*NXLT*/"/@,");
        }
    }

    public class Modulo : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"%", /*NXLT*/"%@,");
        }
    }

    public class Pow : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Pow", /*NXLT*/"Math.Pow@double,double");
        }
    }

    public class Round : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Round", /*NXLT*/"Math.Round@double");
        }
    }

    public class Floor : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Floor", /*NXLT*/"Math.Floor@double");
        }
    }

    public class Ceiling : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Ceiling", /*NXLT*/"Math.Ceiling@double");
        }
    }

    public class RandomSeed : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Random", /*NXLT*/"Math.Random@int");
        }
    }

    public class Random : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.Rand", /*NXLT*/"Math.Rand");
        }
    }

    public class RandomList : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.RandomList", /*NXLT*/"Math.RandomList@int");
        }
    }

    public class EConstant : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.E", /*NXLT*/"Math.E");
        }
    }

    public class Pi : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.PI", /*NXLT*/"Math.PI");
        }
    }

    [AlsoKnownAs(/*NXLT*/"Dynamo.Nodes.2Pi", /*NXLT*/"Dynamo.Nodes.dyn2Pi")]
    public class PiTimes2 : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll", /*NXLT*/"Math.PiTimes2", /*NXLT*/"Math.PiTimes2");
        }
    }

    public class Sin : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Sin", /*NXLT*/"Math.Sin@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees", /*NXLT*/"Math.RadiansToDegrees@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Cos", /*NXLT*/"Math.Cos@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees",   /*NXLT*/"Math.RadiansToDegrees@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Tan",   /*NXLT*/"Math.Tan@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees",   /*NXLT*/"Math.RadiansToDegrees@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.DegreesToRadians",   /*NXLT*/"Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement asinNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Asin",   /*NXLT*/"Math.Asin@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.DegreesToRadians",   /*NXLT*/"Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement acosNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Acos",   /*NXLT*/"Math.Acos@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var converterNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(converterNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.DegreesToRadians",   /*NXLT*/"Math.DegreesToRadians@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Create new node
            XmlElement atanNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.Atan",   /*NXLT*/"Math.Atan@double");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"Math.Average",   /*NXLT*/"Math.Average@var[]");
        }
    }

    public class Smooth : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"",   /*NXLT*/"__ApplyList",   /*NXLT*/"ApplyList@_FunctionObject,var[]..[]");
        }
    }

    public class Apply1 : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement applyNode = MigrationManager.CloneAndChangeName(oldNode,
                /*NXLT*/"DSCoreNodesUI.HigherOrder.ApplyFunction",   /*NXLT*/"Apply Function");           

            int numberOfArgs = oldNode.ChildNodes.Count + 1;
            string numberOfArgsString = numberOfArgs.ToString();
            applyNode.SetAttribute(  /*NXLT*/"inputcount", numberOfArgsString);
            migratedData.AppendNode(applyNode);

            return migratedData;
        }
    }

    public class Conditional : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0),   /*NXLT*/"DSCoreNodesUI.Logic.If", "If"));

            return migrationData;
        }
    }
    
    public class Breakpoint : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Object.Identity",
                /*NXLT*/"Object.Identity@var[]..[]");
        }
    }



    public class AngleInput : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.DegreesToRadians",   /*NXLT*/"Math.DegreesToRadians@double");
            migrationData.AppendNode(newNode);
          
            XmlElement numberNode = MigrationManager.CreateNode(data.Document,
                oldNode, 0,   /*NXLT*/"Dynamo.Nodes.DoubleInput",   /*NXLT*/"Number");

            // Get attributes from old child node
            XmlElement newChild = data.Document.CreateElement(  /*NXLT*/"System.Double");

            foreach (XmlNode attr in oldNode.FirstChild.Attributes)
            {
                if (attr.Name.Equals(  /*NXLT*/"value"))
                    newChild.SetAttribute(  /*NXLT*/"value", attr.Value);
            }

            numberNode.AppendChild(newChild);
            migrationData.AppendNode(numberNode);

            data.CreateConnector(numberNode, 0, newNode, 0);
            return migrationData;
        }
    }

    public class DoubleSliderInput : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode,   /*NXLT*/"Dynamo.Nodes.DoubleSlider",   /*NXLT*/"Double Slider");

            // Get attributes from old child node
            XmlElement newChild1 = data.Document.CreateElement(  /*NXLT*/"System.Double");
            XmlElement newChild2 = data.Document.CreateElement(  /*NXLT*/"Range");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals(  /*NXLT*/"value"))
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode,   /*NXLT*/"Dynamo.Nodes.IntegerSlider",   /*NXLT*/"Integer Slider");

            // Get attributes from old child node
            XmlElement newChild1 = data.Document.CreateElement(  /*NXLT*/"System.Int32");
            XmlElement newChild2 = data.Document.CreateElement(  /*NXLT*/"Range");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals(  /*NXLT*/"value"))
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode,   /*NXLT*/"DSCoreNodesUI.BoolSelector",   /*NXLT*/"Boolean");

            // Get attribute from old child node
            XmlElement newChild = data.Document.CreateElement(  /*NXLT*/"System.Boolean");

            foreach (XmlNode subNode in oldNode.ChildNodes)
            {
                foreach (XmlNode attr in subNode.Attributes)
                {
                    if (attr.Name.Equals(  /*NXLT*/"value"))
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement original = data.MigratedNodes.ElementAt(0);
            var cloned = MigrationManager.CloneAndChangeName(original,   /*NXLT*/"DSCore.File.Directory",   /*NXLT*/"Directory Path");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement original = data.MigratedNodes.ElementAt(0);
            var cloned = MigrationManager.CloneAndChangeName(original,   /*NXLT*/"DSCore.File.Filename",   /*NXLT*/"File Path");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement stringNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(stringNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"String.Concat",   /*NXLT*/"String.Concat@string[]");
            migratedData.AppendNode(stringNode);

            int numberOfArgs = oldNode.ChildNodes.Count + 2;
            string numberOfArgsString = numberOfArgs.ToString();

            stringNode.SetAttribute(  /*NXLT*/"inputcount", numberOfArgsString);

            return migratedData;
        }
    }

    public class String2Num : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"String.ToNumber",  /*NXLT*/"String.ToNumber@string");
        }
    }

    public class Num2String : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"String.FromObject",   /*NXLT*/"String.FromObject@var");
        }
    }

    public class StringLen : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"String.Length",   /*NXLT*/"String.Length@string");
        }
    }

    public class ToString : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"String.FromObject",   /*NXLT*/"String.FromObject@var");
        }
    }

    public class SplitString : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateVarArgFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"String.Split",   /*NXLT*/"String.Split@string,string[]");
            newNode.SetAttribute(  /*NXLT*/"inputcount",   /*NXLT*/"2");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                if (newChild.GetAttribute(  /*NXLT*/"index") ==   /*NXLT*/"1")
                    newChild.SetAttribute(  /*NXLT*/"index",   /*NXLT*/"0");

                newNode.AppendChild(newChild);
            }

            return migrationData;
        }
    }

    public class JoinStrings : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsCoreNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsCoreNode, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.GetItemAtIndex",   /*NXLT*/"List.GetItemAtIndex@var[]..[],int");
            migratedData.AppendNode(dsCoreNode);
            string dsCoreNodeId = MigrationManager.GetGuidFromXmlElement(dsCoreNode);


            XmlElement stringJoinNode = MigrationManager.CreateVarArgFunctionNode(
                data.Document, oldNode, 1, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"String.Join",   /*NXLT*/"String.Join@string,string[]", "2");
            migratedData.AppendNode(stringJoinNode);
            string stringJoinNodeId = MigrationManager.GetGuidFromXmlElement(stringJoinNode);


            XmlElement cbn = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 2,  /*NXLT*/"0");
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

                if (newChild.GetAttribute(  /*NXLT*/"index") == "1")
                    newChild.SetAttribute(  /*NXLT*/"index", "0");

                stringJoinNode.AppendChild(newChild);
            }

            return migratedData;           
        }
    }

    public class StringCase : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",  /*NXLT*/"String.ChangeCase",   /*NXLT*/"String.ChangeCase@string,bool");
        }
    }

    public class Substring : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",   /*NXLT*/"String.Substring",   /*NXLT*/"String.Substring@string,int,int");
        }
    }
}
