using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Creates a Revit topography instance from a list of points
    /// </summary>
    /// <search>topography,topo,points,pts</search>
    [NodeName("Topography From Points")]
    [NodeDescription("Creates a Revit topography surface from a list of points")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    class TopographyFromPoints:RevitTransactionNodeWithOneOutput
    {
        public TopographyFromPoints()
        {
            InPortData.Add(new PortData("points", "A list of points from which to create a topography.", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("topography", "The topography surface.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("The input is not a list of points.");

            var newPts = ((Value.List)args[0]).Item.Select(x => (XYZ)((Value.Container)x).Item).ToList();

            if(!newPts.Any())
                throw new Exception("There are no points in the list.");

            TopographySurface topo = null;

            if (Elements.Any())
            {
                if (dynUtils.TryGetElement(Elements[0], out topo))
                {
                    //In Revit 2014, a transaction edit scope is required to edit a topo surface
                    //we can not currently handle this as it generates a transaction group
                    //so just delete the existing surface and remake.
                    DeleteElement(Elements[0]);

                    topo = CreateTopographySurface(newPts);
                    Elements[0] = topo.Id;
                }
            }
            else
            {
                topo = CreateTopographySurface(newPts);
                Elements.Add(topo.Id);
            }

            if(topo == null)
                throw new ArgumentException("A topography surface could not be created.");

            return Value.NewContainer(topo);
        }

        private TopographySurface CreateTopographySurface(List<XYZ> points)
        {
            var document = DocumentManager.Instance.CurrentDBDocument;
            return TopographySurface.Create(document, points);
        }

        [NodeMigration(from:"0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            //Migrate the topography node from 0.6.3 to 0.7.0
            //No connectors need to be altered as the new version of this node
            //will have the same number of input and outputs connectors.

            //Get the executing assembly location to build the assembly path
            //for the node library. This will need work as we might be loading
            //the assemblies from a package. How do we convey this?
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPath = Path.Combine(assemblyDir, "RevitNodes.dll");

            var oldNodeElement = data.MigratedNodes.ElementAt(0);
            var newNodeElement = MigrationManager.CreateFunctionNodeFrom(oldNodeElement);
            MigrationManager.SetFunctionSignature(newNodeElement, assemblyPath,
                "DSTopography.ByPoints", "DSTopography.ByPoints@double[]..[]");

            NodeMigrationData migrated = new NodeMigrationData(data.Document);
            migrated.AppendNode(newNodeElement);
            return migrated;
        }

        private static XmlElement CreateNewFunctionNodeElement(
            XmlDocument doc, 
            Dictionary<string, string> oldAttribs, 
            string assemblyDir,
            string methodName,
            string assemblyName, 
            string signature)
        {
            var newNodeElement = doc.CreateElement("Dynamo.Nodes.DSFunction");
            newNodeElement.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            newNodeElement.SetAttribute("guid", oldAttribs["guid"]);
            newNodeElement.SetAttribute("nickname", methodName);
            newNodeElement.SetAttribute("x", oldAttribs["x"]);
            newNodeElement.SetAttribute("y", oldAttribs["y"]);
            newNodeElement.SetAttribute("isVisible", oldAttribs["isVisible"]);
            newNodeElement.SetAttribute("isUpstreamVisible", oldAttribs["isUpstreamVisible"]);
            newNodeElement.SetAttribute("lacing", oldAttribs["lacing"]);
            newNodeElement.SetAttribute("assembly", Path.Combine(assemblyDir, assemblyName));
            newNodeElement.SetAttribute("function", signature);
            return newNodeElement;
        }

        private static Dictionary<string,string> GetOldAttributesToCarryOver(XmlElement oldNodeElement)
        {
            var oldAttribs = new Dictionary<string, string>();
            oldAttribs["guid"] = oldNodeElement.Attributes["guid"].Value;
            oldAttribs["isVisible"] = oldNodeElement.Attributes["isVisible"].Value;
            oldAttribs["isUpstreamVisible"] = oldNodeElement.Attributes["isUpstreamVisible"].Value;
            oldAttribs["lacing"] = oldNodeElement.Attributes["lacing"].Value;
            oldAttribs["x"] = oldNodeElement.Attributes["x"].Value;
            oldAttribs["y"] = oldNodeElement.Attributes["y"].Value;

            return oldAttribs;
        }
    }

    /// <summary>
    /// Extracts a list of points from a Revit topography instance.
    /// </summary>
    /// <search>topography,topo,points,pts</search>
    [NodeName("Points from Topography")]
    [NodeDescription("Extracts a list of points from a Revit topograhy surface")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    class PointsFromTopography : NodeWithOneOutput
    {
        public PointsFromTopography()
        {
            InPortData.Add(new PortData("topography", "The topography surface.", typeof(Value.Container)));
            OutPortData.Add(new PortData("points", "A list of points from the topography.", typeof(Value.List)));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var topo = (TopographySurface) ((Value.Container) args[0]).Item;
            
            if (!topo.GetPoints().Any())
                throw new Exception("There are no points in the topography surface.");

            var pts = topo.GetPoints().Select(Value.NewContainer);
            
            return Value.NewList(Utils.SequenceToFSharpList(pts));
        }

        [NodeMigration(from:"0.7.0")]
        public static XmlElement Migrate(XmlElement element)
        {
            //DSRevitNodes.DSTopography.Points
            return element;
        }

    }


}
