using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FamilyInstanceCreatorSelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementSelection", "Select Model Element");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class DividedSurfaceBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSDividedSurfaceFamiliesSelection", "Select Divided Surface Families");
            migrationData.AppendNode(newNode);

            // DO NOT clone the sub nodes. The behavior of this node changed from 0.6.3 to 0.7.0
            // and we want to ensure that the user will need to reselect.

            //foreach (XmlElement subNode in oldNode.ChildNodes)
            //    newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class FormElementBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSFaceSelection", "Select Face");
            migrationData.AppendNode(newNode);

            XmlElement newChild = data.Document.CreateElement("instance");
            newChild.SetAttribute("id", oldNode.GetAttribute("faceRef"));
            newNode.AppendChild(newChild);

            return migrationData;
        }
    }

    public class EdgeOnElementBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSEdgeSelection", "Select Edge");
            migrationData.AppendNode(newNode);

            XmlElement newChild = data.Document.CreateElement("instance");
            newChild.SetAttribute("id", oldNode.GetAttribute("edgeRef"));
            newNode.AppendChild(newChild);

            return migrationData;
        }
    }

    public class CurvesBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementSelection", "Select Model Element");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class MultipleCurvesBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementsSelection", "Select Model Elements");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class PointBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementSelection", "Select Model Element");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class LevelBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementSelection", "Select Model Element");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class ModelElementSelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSModelElementSelection", "Select Model Element");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class XyzBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Dynamo.Nodes.DSPointOnElementSelection", "Select Point on Face");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class Categories : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.Categories", "Categories");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class AllElementsOfCategory : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.ElementsOfCategory", "All Elements of Category");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class ElementTypes : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.ElementTypes", "Element Types");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class AllElementsOfType : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.ElementsOfType", "All Elements of Type");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }
}