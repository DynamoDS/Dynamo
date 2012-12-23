//Copyright 2012 Ian Keough

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

using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
    [ElementName("Reference Point")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a reference point.")]
    [RequiresTransaction(true)]
    public class dynReferencePointByXYZ : dynNode
    {
        public dynReferencePointByXYZ()
        {
            InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(XYZ)));
            OutPortData = new PortData("pt", "The Reference Point(s) created from this operation.", typeof(ReferencePoint));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            //If we are receiving a list, we must create reference points for each XYZ in the list.
            if (input.IsList)
            {
                var xyzList = (input as Expression.List).Item;

                //Counter to keep track of how many ref points we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.convertSequence(
                   xyzList.Select(
                    //..taking each element in the list and...
                      delegate(Expression x)
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
                                  pt.Position = (XYZ)((Expression.Container)x).Item;
                              }
                              else
                              {
                                  //...otherwise, we can make a new reference point and replace it in the list of
                                  //previously created points.
                                  pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                                     (XYZ)((Expression.Container)x).Item
                                  );
                                  this.Elements[count] = pt.Id;
                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new point...
                              pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                                 (XYZ)((Expression.Container)x).Item
                              );
                              //...and store it in the element list for future runs.
                              this.Elements.Add(pt.Id);
                          }
                          //Finally, we update the counter, and return a new Expression containing the reference point.
                          //This Expression will be placed in the Expression.List that will be passed downstream from this
                          //node.
                          count++;
                          return Expression.NewContainer(pt);
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
                return Expression.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one XYZ.
            else
            {
                XYZ xyz = (XYZ)((Expression.Container)input).Item;

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
                return Expression.NewContainer(pt);
            }
        }
    }

    [ElementName("Reference Point Distance")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Measures a distance between point(s).")]
    [RequiresTransaction(false)]
    public class dynDistanceBetweenPoints : dynNode
    {
        public dynDistanceBetweenPoints()
        {
            InPortData.Add(new PortData("ptA", "Element to measure to.", typeof(object)));
            InPortData.Add(new PortData("ptB", "A Reference point.", typeof(object)));

            OutPortData = new PortData("dist", "Distance between points.", typeof(double));

            base.RegisterInputsAndOutputs();
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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            //Grab our inputs and turn them into XYZs.
            XYZ ptA = this.getXYZ(((Expression.Container)args[0]).Item);
            XYZ ptB = this.getXYZ(((Expression.Container)args[1]).Item);

            //Return the calculated distance.
            return Expression.NewNumber(ptA.DistanceTo(ptB));
        }
    }

    [ElementName("Reference Point On Edge")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an element which owns a reference point on a selected edge.")]
    [RequiresTransaction(true)]
    public class dynPointOnEdge : dynNode
    {
        public dynPointOnEdge()
        {
            InPortData.Add(new PortData("curve", "ModelCurve", typeof(Element)));
            InPortData.Add(new PortData("t", "Parameter on edge.", typeof(double)));
            OutPortData = new PortData("pt", "PointOnEdge", typeof(ReferencePoint));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            Reference r = ((CurveElement)((Expression.Container)args[0]).Item).GeometryCurve.Reference;

            double t = ((Expression.Number)args[1]).Item;
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

            return Expression.NewContainer(p);
        }
    }

    [ElementName("Reference Point On Face by UV components")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an element which owns a reference point on a selected face.")]
    [RequiresTransaction(true)]
    public class dynPointOnFace : dynNode
    {
        public dynPointOnFace()
        {
            InPortData.Add(new PortData("face", "ModelFace", typeof(Reference)));
            InPortData.Add(new PortData("u", "U Parameter on face.", typeof(double)));
            InPortData.Add(new PortData("v", "V Parameter on face.", typeof(double)));
            OutPortData = new PortData("pt", "PointOnFace", typeof(ReferencePoint));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            object arg0 = ((Expression.Container)args[0]).Item;
            if (arg0 is Reference)
            {
                // MDJ TODO - this is really hacky. I want to just use the face but evaluating the ref fails later on in pointOnSurface, the ref just returns void, not sure why.

                //Face f = ((Face)((FScheme.Expression.Container)args[0]).Item).Reference; // MDJ TODO this returns null but should not, figure out why and then change selection code to just pass back face not ref
                Reference r = arg0 as Reference;

                double u = ((Expression.Number)args[1]).Item;
                double v = ((Expression.Number)args[2]).Item;

                //Autodesk.Revit.DB..::.PointElementReference
                //Autodesk.Revit.DB..::.PointOnEdge
                //Autodesk.Revit.DB..::.PointOnEdgeEdgeIntersection
                //Autodesk.Revit.DB..::.PointOnEdgeFaceIntersection
                //Autodesk.Revit.DB..::.PointOnFace
                //Autodesk.Revit.DB..::.PointOnPlane

                PointElementReference facePoint = this.UIDocument.Application.Application.Create.NewPointOnFace(r, new UV(u, v));

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

                return Expression.NewContainer(pt);
            }
            else
            {
                throw new Exception("Cannot cast first argument to Face.");
            }
        }
    }

    [ElementName("Reference Point On Face by UV")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an element which owns a reference point on a selected face.")]
    [RequiresTransaction(true)]
    public class dynPointOnFaceUV : dynNode
    {
        public dynPointOnFaceUV()
        {
            InPortData.Add(new PortData("face", "ModelFace", typeof(Reference)));
            InPortData.Add(new PortData("UV", "U Parameter on face.", typeof(object)));
            OutPortData = new PortData("pt", "PointOnFace", typeof(ReferencePoint));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            object arg0 = ((Expression.Container)args[0]).Item;
            if (arg0 is Reference)
            {
               
                Reference r = arg0 as Reference;

                UV uv = ((Expression.Container)args[1]).Item as UV;

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

                return Expression.NewContainer(pt);
            }
            else
            {
                throw new Exception("Cannot cast first argument to Face.");
            }
        }
    }

    [ElementName("Reference Point By Normal")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an element which owns a reference point which is projected from a point by normal and distance.")]
    [RequiresTransaction(true)]
    public class dynPointNormalDistance : dynNode
    {
        public dynPointNormalDistance()
        {
            InPortData.Add(new PortData("pt", "The point to reference", typeof(object)));
            InPortData.Add(new PortData("norm", "The normal", typeof(object)));
            InPortData.Add(new PortData("d", "The offset distance", typeof(object)));
            OutPortData = new PortData("pt", "Point", typeof(ReferencePoint));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
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

            ReferencePoint pt = ((Expression.Container)args[0]).Item as ReferencePoint;
            XYZ norm = ((Expression.Container)args[1]).Item as XYZ;
            double dist = ((Expression.Number)args[2]).Item;

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

            return Expression.NewContainer(p);
        }

    }
    
    [ElementName("Plane from Reference Point")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Extracts one of the primary Reference Planes from a Reference Point.")]
    [RequiresTransaction(true)]
    public class dynPlaneFromRefPoint : dynNode
    {
        ComboBox combo;

        public dynPlaneFromRefPoint()
        {
            
            InPortData.Add(new PortData("pt", "The point to extract the plane from", typeof(object)));
            OutPortData = new PortData("r", "Reference", typeof(Reference));

            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.IsDirty = true;
            };

            PopulateComboBox();

            base.RegisterInputsAndOutputs();
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


        public override Expression Evaluate(FSharpList<Expression> args)
        {
            foreach (ElementId el in this.Elements)
            {
                Element e;
                if (dynUtils.TryGetElement(el, out e))
                {
                    this.UIDocument.Document.Delete(el);
                }
            }
            Plane p = null;
            Reference r = null;
            ReferencePoint pt = ((Expression.Container)args[0]).Item as ReferencePoint;
            //Transform t = pt.GetCoordinateSystem();
            //XYZ norm = t.BasisZ;
            //XYZ origin = TransformPoint(XYZ.Zero, t); // origin in 'local' coordinates to handle point element orientation 

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
            
            
            return Expression.NewContainer(r);
        }

    }

}

