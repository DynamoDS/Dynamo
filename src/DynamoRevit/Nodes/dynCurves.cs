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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Model Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a model curve.")]
    public class dynModelCurve : dynRevitTransactionNodeWithOneOutput
    {
        public dynModelCurve()
        {
            InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(Value.Container)));
            InPortData.Add(new PortData("sp", "The Sketch Plane.", typeof(Value.Container)));
            OutPortData.Add(new PortData("mc", "Model Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve c = (Curve)((Value.Container)args[0]).Item;
            SketchPlane sp = (SketchPlane)((Value.Container)args[1]).Item;


            ModelCurve mc;
            XYZ spOrigin = sp.Plane.Origin;
            XYZ modelOrigin = XYZ.Zero;
            Transform trf = Transform.get_Translation(spOrigin);
            //trf =  trf.Multiply(Transform.get_Rotation(spOrigin,XYZ.BasisZ,spOrigin.AngleOnPlaneTo(XYZ.BasisY,spOrigin)));
            //Curve ct = c.get_Transformed(trf);


            // http://wikihelp.autodesk.com/Revit/enu/2013/Help/00006-API_Developer's_Guide/0074-Revit_Ge74/0114-Sketchin114/0117-ModelCur117
            // The SetPlaneAndCurve() method and the Curve and SketchPlane property setters are used in different situations.
            // When the new Curve lies in the same SketchPlane, or the new SketchPlane lies on the same planar face with the old SketchPlane, use the Curve or SketchPlane property setters.
            // If new Curve does not lay in the same SketchPlane, or the new SketchPlane does not lay on the same planar face with the old SketchPlane, you must simultaneously change the Curve value and the SketchPlane value using SetPlaneAndCurve() to avoid internal data inconsistency.


            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], out e))
                {
                    mc = e as ModelCurve;
                    mc.SketchPlane = sp;
                    var loc = mc.Location as LocationCurve;
                    loc.Curve = c;

                }
                else
                {
                    mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                    this.Elements[0] = mc.Id;
                    mc.SketchPlane = sp;


                }
            }
            else
            {
                mc = this.UIDocument.Document.IsFamilyDocument
                   ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                   : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                this.Elements.Add(mc.Id);
                mc.SketchPlane = sp;
            }

            return Value.NewContainer(mc);
        }
    }

    [NodeName("Curve By Pts")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Create a new Curve by Points by passing in a list of Reference Points")]
    public class dynCurveByPoints : dynRevitTransactionNodeWithOneOutput
    {
        //Our eventual output.
        CurveByPoints c;

        public dynCurveByPoints()
        {
            InPortData.Add(new PortData("refPts", "List of reference points", typeof(Value.List)));
            OutPortData.Add(new PortData("curve", "Curve from ref points", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            
            //Build a sequence that unwraps the input list from it's Value form.
            IEnumerable<ReferencePoint> refPts = ((Value.List)args[0]).Item.Select(
               x => (ReferencePoint)((Value.Container)x).Item
            );

            //Add all of the elements in the sequence to a ReferencePointArray.
            ReferencePointArray refPtArr = new ReferencePointArray();
            foreach (var refPt in refPts)
            {
                refPtArr.Append(refPt);
            }

            //Standard logic for updating an old result, if it exists.
            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], out e))
                {
                    c = e as CurveByPoints;
                    c.SetPoints(refPtArr);
                }
                else
                {
                    //TODO: This method of handling bad elements may cause problems. Instead of overwriting
                    //      index in Elements, might be better to just add it the Elements and then do
                    //      this.DeleteElement(id, true) on the old index.
                    c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                    this.Elements[0] = c.Id;
                }
            }
            else
            {
                c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                this.Elements.Add(c.Id);
            }

            return Value.NewContainer(c);
        }
    }

    [NodeName("Curve By Points By Line")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Create a new Curve by Points by passing in a geometry line in 3d space")]
    public class dynCurveByPointsByLine : dynRevitTransactionNodeWithOneOutput
    {
        public dynCurveByPointsByLine()
        {
            InPortData.Add(new PortData("curve", "geometry curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve from ref points", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Our eventual output.
            CurveByPoints c;

            var input = args[0];


            //If we are receiving a list, we must create a curve by points (CBPs) for each curve in the list.
            if (input.IsList)
            {
                var curveList = (input as Value.List).Item;

                //Counter to keep track of how many CBPs we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   curveList.Select(
                    //..taking each element in the list and...
                      delegate(Value x)
                      {
                          Curve gc = (Curve)((Value.Container)x).Item;
                          //Add the geometry curves start and end points to a ReferencePointArray.
                          ReferencePointArray refPtArr = new ReferencePointArray();
                          if (gc.GetType() == typeof(Line))
                          {
                              XYZ start = gc.get_EndPoint(0);
                              XYZ end = gc.get_EndPoint(1);

                              ReferencePoint refPointStart = this.UIDocument.Document.FamilyCreate.NewReferencePoint(start);
                              ReferencePoint refPointEnd = this.UIDocument.Document.FamilyCreate.NewReferencePoint(end);
                              refPtArr.Append(refPointStart);
                              refPtArr.Append(refPointEnd);
                          }
                          //only lines supported at this point

                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out e))
                              {
                                  //...and if we're successful, update it's position... 
                                  c = e as CurveByPoints;
                                  //c.SetPoints(refPtArr);
                                  //c.GetPoints().get_Item(0).Position.X = gc.get_EndPoint(0).X;
                              }
                              else
                              {
                                  //...otherwise, we can make a new CBP and replace it in the list of
                                  //previously created CBPs.
                                  c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                                  this.Elements[count] = c.Id;
                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new point...
                              c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                              //...and store it in the element list for future runs.
                              this.Elements.Add(c.Id);
                          }
                          //Finally, we update the counter, and return a new Value containing the CBP.
                          //This Value will be placed in the Value.List that will be passed downstream from this
                          //node.
                          count++;
                          return Value.NewContainer(c);
                      }
                   )
                );

                //Now that we've created all the CBPs from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                //Fin
                return Value.NewList(result);
            }

            else
            {
                //If we're not receiving a list, we will just assume we received one geometry curve.

                Curve gc = (Curve)((Value.Container)args[0]).Item;
                //Add the geometry curves start and end points to a ReferencePointArray.
                ReferencePointArray refPtArr = new ReferencePointArray();
                if (gc.GetType() == typeof(Line))
                {
                    XYZ start = gc.get_EndPoint(0);
                    XYZ end = gc.get_EndPoint(1);

                    ReferencePoint refPointStart = this.UIDocument.Document.FamilyCreate.NewReferencePoint(start);
                    ReferencePoint refPointEnd = this.UIDocument.Document.FamilyCreate.NewReferencePoint(end);
                    refPtArr.Append(refPointStart);
                    refPtArr.Append(refPointEnd);
                }

                //If we've made any elements previously...
                if (this.Elements.Any())
                {
                    Element e;
                    //...try to get the first one...
                    if (dynUtils.TryGetElement(this.Elements[0], out e))
                    {
                        //..and if we do, update it's position.
                        c = e as CurveByPoints;
                        c.SetPoints(refPtArr);
                    }
                    else
                    {
                        c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                        this.Elements[0] = c.Id;
                    }
                }
                //...otherwise...
                else
                {
                    c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
                    this.Elements.Add(c.Id);
                }
            }

            return Value.NewContainer(c);
        }
    }

    [NodeName("Curve Element Ref")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Takes in a Model Curve or Geometry Curve, returns a Curve Reference")]
    public class dynCurveRef : dynRevitTransactionNodeWithOneOutput
    {
        public dynCurveRef()
        {
            InPortData.Add(new PortData("curve", "Model Curve Element or Geometry Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("curveRef", "Curve Reference", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value makeCurveRef(object c, int count)
        {
            Reference r = c is CurveElement
               ? (c as CurveElement).GeometryCurve.Reference // curve element
               : (c as Curve).Reference; // geometry curve

            return Value.NewContainer(r);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                int count = 0;
                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      (input as Value.List).Item.Select(
                         x =>
                                this.makeCurveRef(
                                ((Value.Container)x).Item,
                                count++
                            )
                      )
                   )
                );
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeCurveRef(
                       ((Value.Container)input).Item,
                       0

                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

    }

    [NodeName("Curve From Curve Ele")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Takes in a Model Curve and Extracts Geometry Curve")]
    public class dynCurveFromModelCurve : dynRevitTransactionNodeWithOneOutput
    {
        public dynCurveFromModelCurve()
        {
            InPortData.Add(new PortData("mc", "Model Curve Element", typeof(Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private Value extractCurve(object c, int count)
        {
            Curve curve = ((CurveElement)c).GeometryCurve;

            return Value.NewContainer(curve);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                int count = 0;
                var result = Value.NewList(
                   Utils.SequenceToFSharpList(
                      (input as Value.List).Item.Select(
                         x =>
                                this.extractCurve(
                                ((Value.Container)x).Item,
                                count++
                            )
                      )
                   )
                );
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.extractCurve(
                       ((Value.Container)input).Item,
                       0

                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

    }

    [NodeName("Planar Nurb Spline")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Node to create a planar model curve.")]
    public class dynModelCurveNurbSpline : dynRevitTransactionNodeWithOneOutput
    {
        public dynModelCurveNurbSpline()
        {
            InPortData.Add(new PortData("pts", "The points from which to create the nurbs curve", typeof(Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline model curve created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item.Select(
               x => ((ReferencePoint)((Value.Container)x).Item).Position
            ).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = UIDocument.Application.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            ModelNurbSpline c;
            Element e;

            if (Elements.Any() && dynUtils.TryGetElement(Elements[0], out e))
            {
                c = e as ModelNurbSpline;

                c.GeometryCurve = ns;
            }
            else
            {
                Elements.Clear();

                double rawParam = ns.ComputeRawParameter(.5);
                Transform t = ns.ComputeDerivatives(rawParam, false);

                XYZ norm = t.BasisZ;

                if (norm.GetLength() == 0)
                {
                    norm = XYZ.BasisZ;
                }

                Plane p = new Plane(norm, t.Origin);
                SketchPlane sp = this.UIDocument.Document.FamilyCreate.NewSketchPlane(p);
                //sps.Add(sp);

                c = UIDocument.Document.FamilyCreate.NewModelCurve(ns, sp) as ModelNurbSpline;

                Elements.Add(c.Id);
            }

            return Value.NewContainer(c);
        }
    }
}

