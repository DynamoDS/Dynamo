using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using System.Xml;

namespace Dynamo.Nodes
{
    [NodeName("Arc by Start, Middle, End")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given start, middle and end points in XYZ.")]
    [NodeSearchTags("arc", "circle", "start", "middle", "end", "3 point", "three")]
    public class ArcStartMiddleEnd : GeometryBase
    {
        public ArcStartMiddleEnd()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("mid", "XYZ on Curve", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {

            Arc a = null;

            var ptA = ((FScheme.Value.Container)args[0]).Item;//start
            var ptB = ((FScheme.Value.Container)args[1]).Item;//middle
            var ptC = ((FScheme.Value.Container)args[2]).Item;//end

            if (ptA is XYZ)
            {

                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)ptA, (XYZ)ptC, (XYZ)ptB //start, end, middle 
                );


            }
            else if (ptA is ReferencePoint)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position, (XYZ)((ReferencePoint)ptC).Position //start, end, middle 
                );

            }

            return FScheme.Value.NewContainer(a);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Arc.ByThreePoints",
                "Arc.ByThreePoints@Point,Point,Point");
        }
    }

    [NodeName("Arc by Center, Radius, Parameters")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians")]
    [NodeSearchTags("arc", "circle", "center", "radius")]
    public class ArcCenter : GeometryBase
    {
        public ArcCenter()
        {
            InPortData.Add(new PortData("center", "center xyz or transform", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("arc", "Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var radius = (double)((FScheme.Value.Number)args[1]).Item;
            var start = (double)((FScheme.Value.Number)args[2]).Item;
            var end = (double)((FScheme.Value.Number)args[3]).Item;

            Arc a = null;

            if (ptA is XYZ)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)ptA, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is ReferencePoint)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                             center, radius, start, end, trf.BasisX, trf.BasisY
                );
            }

            return FScheme.Value.NewContainer(a);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            // This migration assumes that the first input of the old node is
            // always an XYZ and never a Transform.

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Arc.ByCenterPointRadiusAngle", "Arc.ByCenterPointRadiusAngle@Point,double,double,double,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);
            string identityCoordinateSystemId = MigrationManager.GetGuidFromXmlElement(identityCoordinateSystem);

            XmlElement zAxisNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "CoordinateSystem.ZAxis",
                "CoordinateSystem.ZAxis");
            migrationData.AppendNode(zAxisNode);
            string zAxisNodeId = MigrationManager.GetGuidFromXmlElement(zAxisNode);

            XmlElement toDegreeNodeStart = MigrationManager.CreateFunctionNode(
                data.Document, "DSCoreNodes.dll", 
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeStart);
            string toDegreeNodeStartId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeStart);

            XmlElement toDegreeNodeEnd = MigrationManager.CreateFunctionNode(
                data.Document, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeEnd);
            string toDegreeNodeEndId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeEnd);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId toDegreeNodeStartPort = new PortId(toDegreeNodeStartId, 0, PortType.INPUT);
            PortId toDegreeNodeEndPort = new PortId(toDegreeNodeEndId, 0, PortType.INPUT);

            // Update connectors
            data.ReconnectToPort(connector2, toDegreeNodeStartPort);
            data.ReconnectToPort(connector3, toDegreeNodeEndPort);
            data.CreateConnector(toDegreeNodeStart, 0, newNode, 2);
            data.CreateConnector(toDegreeNodeEnd, 0, newNode, 3);
            data.CreateConnector(zAxisNode, 0, newNode, 4);
            data.CreateConnector(identityCoordinateSystem, 0, zAxisNode, 0);

            return migrationData;
        }
    }


    [NodeName("Best Fit Arc")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Creates best fit arc through points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class BestFitArc : RevitTransactionNodeWithOneOutput
    {
        public BestFitArc()
        {
            InPortData.Add(new PortData("points", "Points to Fit Arc Through", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("arc", "Best Fit Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            List<XYZ> xyzList = new List<XYZ>();

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)args[0]).Item;
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((FScheme.Value.Container)vals[ii]).Item;

                if (item is ReferencePoint)
                {
                    ReferencePoint refPoint = (ReferencePoint)item;
                    XYZ thisXYZ = refPoint.GetCoordinateSystem().Origin;
                    xyzList.Add(thisXYZ);
                }
                else if (item is XYZ)
                {
                    XYZ thisXYZ = (XYZ)item;
                    xyzList.Add(thisXYZ);
                }
            }

            if (xyzList.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }


            Type ArcType = typeof(Autodesk.Revit.DB.Arc);

            MethodInfo[] arcStaticMethods = ArcType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreateByFit = "CreateByFit";
            Arc result = null;

            foreach (MethodInfo m in arcStaticMethods)
            {
                if (m.Name == nameOfMethodCreateByFit)
                {
                    object[] argsM = new object[1];
                    argsM[0] = xyzList;

                    result = (Arc)m.Invoke(null, argsM);

                    break;
                }
            }

            return FScheme.Value.NewContainer(result);
        }
    }
}
