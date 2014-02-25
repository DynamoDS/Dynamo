using System;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;
using RevitServices.Persistence;
using System.Xml;

namespace Dynamo.Nodes
{
    [NodeName("Reference Point")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates a reference point.")]
    [NodeSearchTags("pt","ref")]
    [Obsolete("Use ReferencePoint.ByPt")]
    public class ReferencePointByXyz : RevitTransactionNodeWithOneOutput
    {
        public ReferencePointByXyz()
        {
            InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "The Reference Point(s) created from this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var xyz = (XYZ)((Value.Container)input).Item;

            ReferencePoint pt;

            if (Elements.Any())
            {
                if (dynUtils.TryGetElement(Elements[0], out pt))
                {
                    pt.Position = xyz;
                }
                else
                {
                    pt = UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                    Elements[0] = pt.Id;
                }
            }
            else
            {
                pt = UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                Elements.Add(pt.Id);
            }

            return Value.NewContainer(pt);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "ReferencePoint.ByPoint", "ReferencePoint.ByPoint@Point");
        }
    }

    [NodeName("Reference Point on Edge")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates an element which owns a reference point on a selected edge.")]
    [NodeSearchTags("ref", "pt")]
    [Obsolete("Use ReferencePoint.ByPtOnEdge")]
    public class PointOnEdge : RevitTransactionNodeWithOneOutput
    {
        public PointOnEdge()
        {
            InPortData.Add(new PortData("curve", "ModelCurve", typeof(Value.Container)));
            InPortData.Add(new PortData("t", "Parameter on edge.", typeof(Value.Number)));
            OutPortData.Add(new PortData("pt", "PointOnEdge", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference r = ((CurveElement)((Value.Container)args[0]).Item).GeometryCurve.Reference;

            double t = ((Value.Number)args[1]).Item;
            //Autodesk.Revit.DB..::.PointElementReference
            //Autodesk.Revit.DB..::.PointOnEdge
            //Autodesk.Revit.DB..::.PointOnEdgeEdgeIntersection
            //Autodesk.Revit.DB..::.PointOnEdgeFaceIntersection
            //Autodesk.Revit.DB..::.PointOnFace
            //Autodesk.Revit.DB..::.PointOnPlane
            PointLocationOnCurve plc = new PointLocationOnCurve(PointOnCurveMeasurementType.NormalizedCurveParameter, t, PointOnCurveMeasureFrom.Beginning);
            PointElementReference edgePoint = this.UIDocument.Application.Application.Create.NewPointOnEdge(r, plc);

            ReferencePoint p;

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(Elements[0], out p))
                {
                    p.SetPointElementReference(edgePoint);
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                    this.Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                this.Elements.Add(p.Id);
            }
            
            return Value.NewContainer(p);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "ReferencePoint.ByParameterOnCurveReference",
                "ReferencePoint.ByParameterOnCurveReference@CurveReference,double");
        }
    }

    [NodeName("Reference Point on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates an element which owns a reference point on a selected face.")]
    [NodeSearchTags("ref", "pt")]
    [Obsolete("Use ReferencePoint.ByPtOnFace")]
    public class PointOnFaceUv : RevitTransactionNodeWithOneOutput
    {
        public PointOnFaceUv()
        {
            InPortData.Add(new PortData("face", "ModelFace", typeof(Value.Container)));
            InPortData.Add(new PortData("UV", "UV Parameter on face.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "PointOnFace", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            UV uv = ((Value.Container)args[1]).Item as UV;

            object arg0 = ((Value.Container)args[0]).Item;

            Face f;
            var r = arg0 as Reference;
            if (r != null)
            {
                var document = DocumentManager.GetInstance().CurrentUIDocument.Document;
                f = (Face)document.GetElement(r.ElementId).GetGeometryObjectFromReference(r);
            }
            else
                f = (Face)arg0;

            var facePoint = f.Evaluate(uv);

            ReferencePoint pt = null;

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out pt))
                {
                    pt.Position = facePoint;
                }
                else
                {
                    if (this.UIDocument.Document.IsFamilyDocument)
                    {
                        pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(facePoint);
                        this.Elements[0] = pt.Id;
                    }
                }
            }
            else
            {
                if (this.UIDocument.Document.IsFamilyDocument)
                {
                    pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(facePoint);
                    this.Elements.Add(pt.Id);
                }
            }

            return Value.NewContainer(pt);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsReferencePoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsReferencePoint, "DSRevitNodes.dll",
                "ReferencePoint.ByParametersOnFaceReference",
                "ReferencePoint.ByParametersOnFaceReference@FaceReference,double,double");

            migratedData.AppendNode(dsReferencePoint);
            string dsReferencePointId = MigrationManager.GetGuidFromXmlElement(dsReferencePoint);

            XmlElement uvU = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "UV.U", "UV.U");
            migratedData.AppendNode(uvU);
            string uvUId = MigrationManager.GetGuidFromXmlElement(uvU);

            XmlElement uvV = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "UV.V", "UV.V");
            migratedData.AppendNode(uvV);
            string uvVId = MigrationManager.GetGuidFromXmlElement(uvV);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            XmlElement connector2 = null;
            if (connector1!=null)
            {
                connector2 = MigrationManager.CreateFunctionNodeFrom(connector1);
                data.CreateConnector(connector2);
            }

            PortId newInPort = new PortId(dsReferencePointId, 0, PortType.INPUT);
            data.ReconnectToPort(connector0, newInPort);
            newInPort = new PortId(uvUId, 0, PortType.INPUT);
            data.ReconnectToPort(connector1, newInPort);

            if (connector2 != null)
            {
                newInPort = new PortId(uvVId, 0, PortType.INPUT);
                data.ReconnectToPort(connector2, newInPort);
            }

            data.CreateConnector(uvU, 0, dsReferencePoint, 1);
            data.CreateConnector(uvV, 0, dsReferencePoint, 2);

            return migratedData;           
        }
    }

    [NodeName("Reference Point by Normal")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Owns a reference point which is projected from a point by normal and distance.")]
    [NodeSearchTags("normal", "ref")]
    [Obsolete("Use ReferencePoint.ByPtVectorDistance")]
    public class PointNormalDistance : RevitTransactionNodeWithOneOutput
    {
        public PointNormalDistance()
        {
            InPortData.Add(new PortData("pt", "The point to reference", typeof(Value.Container)));
            InPortData.Add(new PortData("norm", "The normal", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "The offset distance", typeof(Value.Number)));
            OutPortData.Add(new PortData("pt", "Point", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pt = (ReferencePoint)((Value.Container)args[0]).Item;
            var norm = (XYZ)((Value.Container)args[1]).Item;
            double dist = ((Value.Number)args[2]).Item;

            ReferencePoint p;

            var newLocation = pt.Position + norm.Normalize().Multiply(dist);

            if (Elements.Any())
            {
                if (dynUtils.TryGetElement(Elements[0], out p))
                {
                    //move the point to the new offset
                    p.Position = newLocation;
                    Elements[0] = p.Id;
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(newLocation);
                    Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(newLocation);
                this.Elements.Add(p.Id);
            }

            return Value.NewContainer(p);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "ReferencePoint.ByPointVectorDistance",
                "ReferencePoint.ByPointVectorDistance@Point,Vector,double");
        }

    }

    [NodeName("Plane from Reference Point")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Extracts one of the primary Reference Planes from a Reference Point.")]
    [NodeSearchTags("ref")]
    [Obsolete("Use properties on ReferencePlane")]
    public class PlaneFromRefPoint : RevitTransactionNodeWithOneOutput
    {
        ComboBox combo;

        public PlaneFromRefPoint()
        {
            InPortData.Add(new PortData("pt", "The point to extract the plane from", typeof(Value.Container)));
            OutPortData.Add(new PortData("r", "Reference", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };

            PopulateComboBox();
        }

        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateComboBox();
        }
        public enum RefPointReferencePlanes { XY, YZ, XZ };

        private void PopulateComboBox()
        {

            combo.Items.Clear();

            foreach (var plane in System.Enum.GetValues(typeof(RefPointReferencePlanes)))
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = plane.ToString();
                combo.Items.Add(cbi);
            }

        }


        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            foreach (ElementId el in this.Elements)
            {
                Element e;
                if (dynUtils.TryGetElement(el, out e))
                {
                    this.UIDocument.Document.Delete(el);
                }
            }
            
            //Plane p = null;
            Reference r = null;
            ReferencePoint pt = ((Value.Container)args[0]).Item as ReferencePoint;

            int n = combo.SelectedIndex;
            switch (n)
            {
                case 0: //combo.SelectedValue == "XY"
                    r = pt.GetCoordinatePlaneReferenceXY();
                    break;
                case 1: //combo.SelectedValue == "XZ"
                    r = pt.GetCoordinatePlaneReferenceXZ();
                    break;
                case 2: //combo.SelectedValue == "YZ"
                    r = pt.GetCoordinatePlaneReferenceYZ();
                    break;
                default:
                    r = pt.GetCoordinatePlaneReferenceXY();
                    break;
            }
            return Value.NewContainer(r);
        }

    }

    [NodeName("Reference Point at Length")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates an ref point element on curve located by length from the start or end of the curve.")]
    [NodeSearchTags("ref", "pt", "curve")]
    public class PointOnCurveByLength : RevitTransactionNodeWithOneOutput
    {
        public PointOnCurveByLength()
        {
            InPortData.Add(new PortData("curve", "Model Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("len", "measured length or percent of overall length", typeof(Value.Number)));
            InPortData.Add(new PortData("normalized?", "if true len is the percent of overall curve length, else the actual length", typeof(Value.Container)));
            InPortData.Add(new PortData("beginning?", "if true measured from Beginning, else from End", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "PointOnCurve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var inputItem = ((Value.Container)args[0]).Item;
            Reference r = (inputItem is Reference) ?
                                (Reference)inputItem : ((CurveElement)inputItem).GeometryCurve.Reference;

            double len = ((Value.Number)args[1]).Item;

            bool isNormalized = ((Value.Number)args[2]).Item == 1;
            bool isBeginning = ((Value.Number)args[3]).Item == 1;

            PointLocationOnCurve plc = new PointLocationOnCurve(isNormalized ? PointOnCurveMeasurementType.NormalizedSegmentLength : PointOnCurveMeasurementType.SegmentLength,
                                                            len,
                                                    isBeginning ? PointOnCurveMeasureFrom.Beginning : PointOnCurveMeasureFrom.End);

            PointElementReference edgePoint = this.UIDocument.Application.Application.Create.NewPointOnEdge(r, plc);

            ReferencePoint p;

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out p))
                {
                    p.SetPointElementReference(edgePoint);
                }
                else
                {
                    p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                    this.Elements[0] = p.Id;
                }
            }
            else
            {
                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(edgePoint);
                this.Elements.Add(p.Id);
            }

            return Value.NewContainer(p);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "ReferencePoint.ByLengthOnCurveReference",
                "ReferencePoint.ByLengthOnCurveReference@CurveReference,double");
        }
    }

    [NodeName("Reference Point Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeSearchTags("norm")]
    [NodeDescription("Measures a distance between point(s).")]
    public class DistanceBetweenPoints : MeasurementBase
    {
        public DistanceBetweenPoints()
        {
            InPortData.Add(new PortData("ptA", "Element to measure to.", typeof(Value.Container)));
            InPortData.Add(new PortData("ptB", "A Reference point.", typeof(Value.Container)));

            OutPortData.Add(new PortData("dist", "Distance between points.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private XYZ getXYZ(object arg)
        {
            if (arg is ReferencePoint)
            {
                return (arg as ReferencePoint).Position;
            }
            else if (arg is FamilyInstance)
            {
                return ((arg as FamilyInstance).Location as LocationPoint).Point;
            }
            else if (arg is XYZ)
            {
                return arg as XYZ;
            }
            else
            {
                throw new Exception("Cannot cast argument to ReferencePoint or FamilyInstance or XYZ.");
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Grab our inputs and turn them into XYZs.
            XYZ ptA = this.getXYZ(((Value.Container)args[0]).Item);
            XYZ ptB = this.getXYZ(((Value.Container)args[1]).Item);

            //Return the calculated distance.
            return Value.NewContainer(Units.Length.FromFeet(ptA.DistanceTo(ptB)));
        }
    }
}
