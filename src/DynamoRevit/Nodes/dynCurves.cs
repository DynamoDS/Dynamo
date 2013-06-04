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
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using System.Reflection;

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
                if (dynUtils.TryGetElement(this.Elements[0], typeof(ModelCurve), out e))
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

    [NodeName("Reference Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a model curve.")]
    public class dynReferenceCurve : dynRevitTransactionNodeWithOneOutput
    {
        public dynReferenceCurve()
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
                if (dynUtils.TryGetElement(this.Elements[0],typeof(ModelCurve), out e))
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

            mc.ChangeToReferenceLine();

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
            InPortData.Add(new PortData("isRef", "Boolean indicating whether the resulting curve is a reference curve.", typeof(Value.Number)));
            OutPortData.Add(new PortData("curve", "Curve from ref points", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            bool isRefCurve = Convert.ToBoolean(((Value.Number)args[1]).Item);

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
                if (dynUtils.TryGetElement(this.Elements[0],typeof(CurveByPoints), out e))
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

            c.IsReferenceLine = isRefCurve;

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
            CurveByPoints c = null;

            var input = args[0];

            Curve gc = (Curve)((Value.Container)args[0]).Item;
            XYZ start = gc.get_EndPoint(0);
            XYZ end = gc.get_EndPoint(1);

            //If we've made any elements previously...
            if (this.Elements.Any())
            {
                Element e;
                //...try to get the first one...
                if (dynUtils.TryGetElement(this.Elements[0], typeof(CurveByPoints), out e))
                {
                    //..and if we do, update it's position.
                    c = e as CurveByPoints;

                    ReferencePointArray existingPts = c.GetPoints();

                    //update the points on the curve to match
                    ReferencePointArrayIterator iter = existingPts.ForwardIterator();
                    existingPts.get_Item(0).Position = start;
                    existingPts.get_Item(1).Position = end;
                }
                else
                {
                    c = CreateCurveByPoints(c, gc, start, end);
                    this.Elements[0] = c.Id;
                }
            }
            else
            {    
                c = CreateCurveByPoints(c, gc, start, end);
                this.Elements.Add(c.Id);
            }

            return Value.NewContainer(c);
        }

        private CurveByPoints CreateCurveByPoints(CurveByPoints c, Curve gc, XYZ start, XYZ end)
        {
            //Add the geometry curves start and end points to a ReferencePointArray.
            ReferencePointArray refPtArr = new ReferencePointArray();
            if (gc.GetType() == typeof(Line))
            {
                ReferencePoint refPointStart = this.UIDocument.Document.FamilyCreate.NewReferencePoint(start);
                ReferencePoint refPointEnd = this.UIDocument.Document.FamilyCreate.NewReferencePoint(end);
                refPtArr.Append(refPointStart);
                refPtArr.Append(refPointEnd);
            }

            c = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(refPtArr);
            return c;
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

    [NodeName("Nurbs Spline Model Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Node to create a planar nurbs spline model curve.")]
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

            if (Elements.Any() && dynUtils.TryGetElement(Elements[0],typeof(ModelCurve), out e))
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

    [NodeName("Nurbs Spline")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Node to create a planar nurbs spline curve.")]
    public class dynGeometryCurveNurbSpline : dynCurveBase
    {
        public dynGeometryCurveNurbSpline()
        {
            InPortData.Add(new PortData("xyzs", "The xyzs from which to create the nurbs curve", typeof(Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline curve created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

         public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item.Select(
               x => ((XYZ)((Value.Container)x).Item)).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = dynRevitSettings.Revit.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            crvs.Add(ns);
            
            return Value.NewContainer(ns);
        }
    }
     
    /*
    [NodeName("Offset Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Create an offset curve from a given curve.")]
    public class dynOffsetCurve : dynCurveBase
    {
        public dynOffsetCurve()
        {
            InPortData.Add(new PortData("crv", "The curve to offset.", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "The distance to offset.", typeof(Value.Number)));
            OutPortData.Add(new PortData("crv", "The offset curve.", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve cIn = (Curve)((Value.Container)args[0]).Item;
            double dIn = ((Value.Number)args[1]).Item;

            Curve cOut = null;

            if(cIn is Arc)
            {
                Arc a = cIn as Arc;

                Transform tStart = cIn.ComputeDerivatives(0, true);
                Transform tEnd = cIn.ComputeDerivatives(1, true);
                Transform tMid = cIn.ComputeDerivatives(.5, true);

                XYZ startVec = tStart.BasisX.CrossProduct(tStart.BasisZ).Normalize();
                XYZ endVec = tEnd.BasisX.CrossProduct(tEnd.BasisZ).Normalize();
                XYZ midVec = tMid.BasisX.CrossProduct(tMid.BasisZ).Normalize();

                XYZ start = a.Evaluate(0, true) + startVec*dIn;
                XYZ end = a.Evaluate(1,true) + endVec*dIn;
                XYZ mid = a.Evaluate(.5,true) + midVec*dIn;

                cOut = dynRevitSettings.Revit.Application.Create.NewArc(start, end, mid);
            }
            else if (cIn is CylindricalHelix)
            {
                CylindricalHelix ch = cIn as CylindricalHelix;

                cOut = CylindricalHelix.Create(ch.BasePoint, ch.Radius + dIn, ch.XVector, ch.ZVector, ch.Pitch, 0, Math.PI * 2);
            }
            else if (cIn is Ellipse)
            {
                Ellipse e = cIn as Ellipse;
                cOut = dynRevitSettings.Revit.Application.Create.NewEllipse(e.Center, e.RadiusX+dIn, e.RadiusY+dIn, e.XDirection, e.YDirection, 0, Math.PI*2);
            }
            else if (cIn is HermiteSpline)
            {
                List<XYZ> newCtrlPoints = new List<XYZ>();
                HermiteSpline hs = cIn as HermiteSpline;

                double tStart = hs.get_EndParameter(0);
                double tEnd = hs.get_EndParameter(1);

                List<XYZ> newPts = new List<XYZ>();
                double domain = tEnd - tStart;

                for (double i = tStart; i <= tEnd; i += domain / 20)
                {
                    Transform t = hs.ComputeDerivatives(i, false);

                    XYZ curveNormal = t.BasisX.CrossProduct(t.BasisZ).Normalize();

                    if (t.BasisY.Normalize().GetLength() == 0)
                        continue;

                    double dot = t.BasisY.Normalize().DotProduct(curveNormal);
                    if (dot > 0)
                    {
                        newPts.Add(t.Origin + curveNormal * dIn);
                    }
                    else
                        newPts.Add(t.Origin + curveNormal * -dIn);

                    Debug.WriteLine(string.Format("{0} : {1} : {2}", t.BasisY.Normalize(), newPts.Last(), dot));
                }

                cOut = dynRevitSettings.Revit.Application.Create.NewHermiteSpline(newPts, false);
            }
            else if (cIn is Line)
            {
                Line l = cIn as Line;
                XYZ startVec = cIn.ComputeDerivatives(0, true).BasisZ.Normalize();
                XYZ endVec = cIn.ComputeDerivatives(1, true).BasisZ.Normalize();
                XYZ start = l.Evaluate(0, true) + startVec * dIn;
                XYZ end = l.Evaluate(1, true) + endVec * dIn;

                cOut = dynRevitSettings.Revit.Application.Create.NewLineBound(start, end);
            }
            else if (cIn is NurbSpline)
            {
                bool flip = false;
                double lastMagnitude = 0.0;

                List<XYZ> newCtrlPoints = new List<XYZ>();
                NurbSpline ns = cIn as NurbSpline;

                double tStart = ns.get_EndParameter(0);
                double tEnd = ns.get_EndParameter(1);

                List<XYZ> newPts = new List<XYZ>();
                double domain = tEnd - tStart;

                for (double i = tStart; i <= tEnd; i += domain / 20)
                {
                    Transform t = ns.ComputeDerivatives(i, false);

                    XYZ curveNormal = t.BasisX.CrossProduct(t.BasisZ).Normalize();

                    if (t.BasisY.Normalize().GetLength() == 0)
                        continue;

                    double deviation = t.BasisY.GetLength() - lastMagnitude;
                    if(deviation < 0 + .0000001 && deviation > 0 - .0000001)
                        continue;

                    flip = deviation < 0;

                    if (flip || i==tStart)
                    {
                        newPts.Add(t.Origin + curveNormal * dIn);
                    }
                    else
                        newPts.Add(t.Origin + curveNormal * -dIn);

                    Debug.WriteLine(string.Format("{0} : {1} : {2}", t.BasisY, newPts.Last(), t.BasisY.GetLength()));

                    lastMagnitude = t.BasisY.GetLength();
                }

                cOut = dynRevitSettings.Revit.Application.Create.NewHermiteSpline(newPts, false);
                
            }

            if (cOut != null)
                crvs.Add(cOut);

            return Value.NewContainer(cOut);
        }
    }
    */

    [NodeName("Curve Loop")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates Curve Loop")]
    public class dynCurveLoop : dynCurveBase
    {
        public dynCurveLoop()
        {
            InPortData.Add(new PortData("curves", "Geometry curves to make curve loop", typeof(Value.List)));
            OutPortData.Add(new PortData("CurveLoop", "CurveLoop", typeof(Value.Container)));
            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            var curves = ((Value.List)args[0]).Item.Select(
               x => ((Curve)((Value.Container)x).Item)).ToList();

            CurveLoop result = CurveLoop.Create(curves);

            foreach (Curve c in result)
            {
                crvs.Add(c);
            }

            return Value.NewContainer(result);
        }
    }

    [NodeName("Thicken Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates Curve Loop by thickening curve")]
    public class dynThickenCurveLoop : dynCurveBase
    {
        public dynThickenCurveLoop()
        {
            InPortData.Add(new PortData("Curve", "Curve to thicken, could not be closed.", typeof(Value.Container)));
            InPortData.Add(new PortData("Thickness", "Thickness value.", typeof(Value.Number)));
            InPortData.Add(new PortData("Normal", "The normal vector to the plane used for thickening.", typeof(Value.Container)));
            OutPortData.Add(new PortData("CurveLoop", "CurveLoop which is the result of thickening.", typeof(Value.Container)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            double thickness = ((Value.Number)args[1]).Item;
            XYZ normal = (XYZ)((Value.Container)args[2]).Item;

            CurveLoop result = CurveLoop.CreateViaThicken(curve, thickness, normal);
            if (result == null)
                throw new Exception("Could not thicken curve");

            foreach (Curve c in result)
            {
                crvs.Add(c);
            }

            return Value.NewContainer(result);
        }
    }

    [NodeName("Curve Loop List")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates list of curves in the Curve Loop")]
    public class dynListCurveLoop : dynRevitTransactionNodeWithOneOutput
    {
        public dynListCurveLoop()
        {
            InPortData.Add(new PortData("CurveLoop", "Curve to thicken.", typeof(Value.Container)));
            OutPortData.Add(new PortData("Curve List", "List of curves in the curve loop.", typeof(Value.List)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveLoop curveLoop = (CurveLoop)((Value.Container)args[0]).Item;

            CurveLoopIterator CLiter = curveLoop.GetCurveLoopIterator();

            List<Curve> listCurves = new List<Curve>();
            for (; CLiter.MoveNext(); )
            {
                listCurves.Add(CLiter.Current);
            }

            var result = FSharpList<Value>.Empty;
            for (int indexCurve = listCurves.Count - 1; indexCurve > -1; indexCurve--)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(listCurves[indexCurve]), result);
            }

            return Value.NewList(result);
        }
    }
     
    [NodeName("Offset Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates curve by offseting curve")]
    public class dynOffsetCrv : dynCurveBase
    {
        public dynOffsetCrv()
        {
            InPortData.Add(new PortData("Curve", "Curve to thicken, could not be closed.", typeof(Value.Container)));
            InPortData.Add(new PortData("Offset", "Offset value.", typeof(Value.Number)));
            InPortData.Add(new PortData("Normal", "The normal vector to the plane used for offset.", typeof(Value.Number)));
            OutPortData.Add(new PortData("Curve", "Curve which is the result of offset.", typeof(Value.Container)));
            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;

            double thickness = ((Value.Number)args[1]).Item;
            XYZ normal = (XYZ)((Value.Container)args[2]).Item;

            CurveLoop thickenLoop = CurveLoop.CreateViaThicken(curve, thickness, normal);

            if (thickenLoop == null)
                throw new Exception("Could not offset curve");

            CurveLoopIterator CLiter = thickenLoop.GetCurveLoopIterator();

            Curve result = null;

            //relying heavily on the order of curves in the resulting curve loop, based on internal implemen
            for (int index = 0; CLiter.MoveNext(); index++)
            {
                if (index == 2)
                    result = CLiter.Current;
            }

            if (result == null)
                throw new Exception("Could not offset curve");

            crvs.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Bisector Line")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates bisector of two lines")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynBisector : dynRevitTransactionNodeWithOneOutput
    {
        public dynBisector()
        {
            InPortData.Add(new PortData("line1", "First Line", typeof(Value.Container)));
            InPortData.Add(new PortData("line2", "Second Line", typeof(Value.Container)));
            OutPortData.Add(new PortData("bisector", "Bisector Line", typeof(Value.Container)));
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            Line line1 = (Line)((Value.Container)args[0]).Item;
            Line line2 = (Line)((Value.Container)args[1]).Item;

            Type LineType = typeof(Autodesk.Revit.DB.Line);

            MethodInfo[] lineInstanceMethods = LineType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            String nameOfMethodCreateBisector = "CreateBisector";
            Line result = null;

            foreach (MethodInfo m in lineInstanceMethods)
            {
                if (m.Name == nameOfMethodCreateBisector)
                {
                    object[] argsM = new object[1];
                    argsM[0] = line2;

                    result = (Line)m.Invoke(line1, argsM);

                    break;
                }
            }

            return Value.NewContainer(result);
        }
    }

    [NodeName("Best fit arc")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates best fit arc through points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynBestFitArc : dynRevitTransactionNodeWithOneOutput
    {
        public dynBestFitArc()
        {
            InPortData.Add(new PortData("points", "Points to Fit Arc Through", typeof(Value.List)));
            OutPortData.Add(new PortData("arc", "Best Fit Arc", typeof(Value.Container)));
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            List<XYZ> xyzList = new List<XYZ>();

            FSharpList<Value> vals = ((Value.List)args[0]).Item;
            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((Value.Container)vals[ii]).Item;

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

            String nameOfMethodCreateByFit = "CreateByFit";
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

            return Value.NewContainer(result);
        }
    }

    [NodeName("Approximate By Tangent Arcs")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates best fit arc through points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynApproximateByTangentArcs : dynRevitTransactionNodeWithOneOutput
    {
        public dynApproximateByTangentArcs()
        {
            InPortData.Add(new PortData("curve", "Curve to Approximate by Tangent Arcs", typeof(Value.Container)));
            OutPortData.Add(new PortData("arcs", "List of Approximating Arcs", typeof(Value.List)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve thisCurve = (Curve)((Value.Container)args[0]).Item;

            if (thisCurve == null)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }


            Type CurveType = typeof(Autodesk.Revit.DB.Curve);

            MethodInfo[] curveInstanceMethods = CurveType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            String nameOfMethodApproximateByTangentArcs = "ApproximateByTangentArcs";
            List <Curve> resultArcs = null;
            var result = FSharpList<Value>.Empty;

            foreach (MethodInfo m in curveInstanceMethods)
            {
                if (m.Name == nameOfMethodApproximateByTangentArcs)
                {
                    object[] argsM = new object[0];

                    resultArcs = (List<Curve>)m.Invoke(thisCurve, argsM);

                    break;
                }
            }
            for (int indexCurve = resultArcs.Count - 1; indexCurve > -1; indexCurve--)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(resultArcs[indexCurve]), result);   
            }

            return Value.NewList(result);
        }
    }
}

