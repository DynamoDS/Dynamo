//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Ref Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a reference point.")]
    public class dynReferencePointByXYZ : dynRevitTransactionNodeWithOneOutput
    {
        public dynReferencePointByXYZ()
        {
            InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "The Reference Point(s) created from this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //If we are receiving a list, we must create reference points for each XYZ in the list.
            if (input.IsList)
            {
                var xyzList = (input as Value.List).Item;

                //Counter to keep track of how many ref points we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   xyzList.Select(
                    //..taking each element in the list and...
                      delegate(Value x)
                      {
                          ReferencePoint pt;
                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out e))
                              {
                                  //...and if we're successful, update it's position... 
                                  pt = e as ReferencePoint;
                                  pt.Position = (XYZ)((Value.Container)x).Item;
                              }
                              else
                              {
                                  //...otherwise, we can make a new reference point and replace it in the list of
                                  //previously created points.
                                  pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                                     (XYZ)((Value.Container)x).Item
                                  );
                                  this.Elements[count] = pt.Id;
                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new point...
                              pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                                 (XYZ)((Value.Container)x).Item
                              );
                              //...and store it in the element list for future runs.
                              this.Elements.Add(pt.Id);
                          }
                          //Finally, we update the counter, and return a new Value containing the reference point.
                          //This Value will be placed in the Value.List that will be passed downstream from this
                          //node.
                          count++;
                          return Value.NewContainer(pt);
                      }
                   )
                );

                //Now that we've created all the Reference Points from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                //Fin
                return Value.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one XYZ.
            else
            {
                XYZ xyz = (XYZ)((Value.Container)input).Item;

                ReferencePoint pt;

                //If we've made any elements previously...
                if (this.Elements.Any())
                {
                    Element e;
                    //...try to get the first one...
                    if (dynUtils.TryGetElement(this.Elements[0], out e))
                    {
                        //..and if we do, update it's position.
                        pt = e as ReferencePoint;
                        pt.Position = xyz;
                    }
                    else
                    {
                        //...otherwise, just make a new one and replace it in the list.
                        pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                        this.Elements[0] = pt.Id;
                    }

                    //We still delete all extra elements, since in the previous run we might have received a list.
                    foreach (var el in this.Elements.Skip(1))
                    {
                        this.DeleteElement(el);
                    }
                }
                //...otherwise...
                else
                {
                    //...just make a point and store it.
                    pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                    this.Elements.Add(pt.Id);
                }

                //Fin
                return Value.NewContainer(pt);
            }
        }
    }

    [NodeName("Ref Point Dist")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Measures a distance between point(s).")]
    public class dynDistanceBetweenPoints: dynNodeWithOneOutput
    {
        public dynDistanceBetweenPoints()
        {
            InPortData.Add(new PortData("ptA", "Element to measure to.", typeof(Value.Container)));
            InPortData.Add(new PortData("ptB", "A Reference point.", typeof(Value.Container)));

            OutPortData.Add(new PortData("dist", "Distance between points.", typeof(Value.Number)));

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
            return Value.NewNumber(ptA.DistanceTo(ptB));
        }
    }

    [NodeName("Ref Point On Edge")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an element which owns a reference point on a selected edge.")]
    public class dynPointOnEdge : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointOnEdge()
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
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], out e))
                {
                    p = e as ReferencePoint;
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
    }

    [NodeName("Ref Point On Face by UV")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an element which owns a reference point on a selected face.")]
    public class dynPointOnFaceUV : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointOnFaceUV()
        {
            InPortData.Add(new PortData("face", "ModelFace", typeof(Value.Container)));
            InPortData.Add(new PortData("UV", "UV Parameter on face.", typeof(Value.Container)));
            OutPortData.Add(new PortData("pt", "PointOnFace", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            object arg0 = ((Value.Container)args[0]).Item;
            if (arg0 is Reference)
            {

                Reference r = arg0 as Reference;

                UV uv = ((Value.Container)args[1]).Item as UV;

                PointElementReference facePoint = this.UIDocument.Application.Application.Create.NewPointOnFace(r, uv);

                ReferencePoint pt = null;

                if (this.Elements.Any())
                {
                    Element e;
                    if (dynUtils.TryGetElement(this.Elements[0], out e))
                    {
                        pt = e as ReferencePoint;
                        pt.SetPointElementReference(facePoint);
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
            else
            {
                throw new Exception("Cannot cast first argument to Face.");
            }
        }
    }

    [NodeName("Ref Point By Normal")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Owns a reference point which is projected from a point by normal and distance.")]
    public class dynPointNormalDistance : dynRevitTransactionNodeWithOneOutput
    {
        public dynPointNormalDistance()
        {
            InPortData.Add(new PortData("pt", "The point to reference", typeof(Value.Container)));
            InPortData.Add(new PortData("norm", "The normal", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "The offset distance", typeof(Value.Number)));
            OutPortData.Add(new PortData("pt", "Point", typeof(Value.Container)));

            RegisterAllPorts();
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
            ReferencePoint p = null;

            ReferencePoint pt = ((Value.Container)args[0]).Item as ReferencePoint;
            XYZ norm = ((Value.Container)args[1]).Item as XYZ;
            double dist = ((Value.Number)args[2]).Item;

            XYZ location = null;
            PointElementReference per = pt.GetPointElementReference();

            if (pt.GetPointElementReference().GetType() == typeof(PointOnFace))
            {
                //gather information about the point
                PointOnFace pof = per as PointOnFace;
                Reference faceRef = pof.GetFaceReference();
                Face f = this.UIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
                location = f.Evaluate(pof.UV);

                p = this.UIDocument.Document.FamilyCreate.NewReferencePoint(location + norm.Normalize().Multiply(dist));
                this.Elements.Add(p.Id);

            }

            return Value.NewContainer(p);
        }

    }

    [NodeName("Plane from Ref Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Extracts one of the primary Reference Planes from a Reference Point.")]
    public class dynPlaneFromRefPoint : dynRevitTransactionNodeWithOneOutput
    {
        ComboBox combo;

        public dynPlaneFromRefPoint()
        {
            InPortData.Add(new PortData("pt", "The point to extract the plane from", typeof(Value.Container)));
            OutPortData.Add(new PortData("r", "Reference", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
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

            foreach (var plane in Enum.GetValues(typeof(RefPointReferencePlanes)))
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

}
