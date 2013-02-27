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
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;

namespace Dynamo.Nodes
{
    [NodeName("XYZ")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates an XYZ from three numbers.")]
    public class dynXYZ : dynNode
    {
        public dynXYZ()
        {
            InPortData.Add(new PortData("X", "X", typeof(double)));
            InPortData.Add(new PortData("Y", "Y", typeof(double)));
            InPortData.Add(new PortData("Z", "Z", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("xyz", "XYZ", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y, z;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;
            z = ((Value.Number)args[2]).Item;

            return Value.NewContainer(new XYZ(x, y, z));
        }
    }

    [NodeName("XYZ From Ref Point")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Extracts an XYZ from a Reference Point.")]
    public class dynXYZFromReferencePoint : dynNode
    {
        public dynXYZFromReferencePoint()
        {
            InPortData.Add(new PortData("pt", "Reference Point", typeof(object)));
            outPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ReferencePoint point;
            point = (ReferencePoint)((Value.Container)args[0]).Item;

            return Value.NewContainer(point.Position);
        }
    }

    [NodeName("XYZ -> X")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Fetches the X value of the given XYZ")]
    public class dynXYZGetX : dynNode
    { 
        public dynXYZGetX()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            outPortData = new PortData("X", "X value of given XYZ", typeof(double));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).X);
        }
    }

    [NodeName("XYZ -> Y")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Fetches the Y value of the given XYZ")]
    public class dynXYZGetY : dynNode
    {
        public dynXYZGetY()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            outPortData = new PortData("Y", "Y value of given XYZ", typeof(double));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).Y);
        }
    }

    [NodeName("XYZ -> Z")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Fetches the Z value of the given XYZ")]
    public class dynXYZGetZ : dynNode
    {
        public dynXYZGetZ()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            outPortData = new PortData("Z", "Z value of given XYZ", typeof(double));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).Z);
        }
    }

    [NodeName("XYZ Zero")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates an XYZ at the origin (0,0,0).")]
    public class dynXYZZero : dynNode
    {
        public dynXYZZero()
        {
            outPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(XYZ.Zero);
        }
    }

    [NodeName("XYZ BasisX")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates an XYZ representing the X basis (1,0,0).")]
    public class dynXYZBasisX : dynNode
    {
        public dynXYZBasisX()
        {
            outPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(XYZ.BasisX);
        }
    }

    [NodeName("XYZ BasisY")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates an XYZ representing the Y basis (0,1,0).")]
    public class dynXYZBasisY : dynNode
    {
        public dynXYZBasisY()
        {
            outPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(XYZ.BasisY);
        }
    }

    [NodeName("XYZ BasisZ")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates an XYZ representing the Z basis (0,0,1).")]
    public class dynXYZBasisZ : dynNode
    {
        public dynXYZBasisZ()
        {
            outPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(XYZ.BasisZ);
        }
    }

    [NodeName("XYZ Scale")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Multiplies each component of an XYZ by a number.")]
    public class dynXYZScale : dynNode
    {
        public dynXYZScale()
        {
            InPortData.Add(new PortData("XYZ", "XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("n", "Scale value.", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("xyz", "XYZ", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyz = (XYZ)((Value.Container)args[0]).Item;
            double n = ((Value.Number)args[1]).Item;

            return Value.NewContainer(xyz.Multiply(n));
        }
    }

    [NodeName("XYZ Add")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Adds the components of two XYZs.")]
    public class dynXYZAdd : dynNode
    {
        public dynXYZAdd()
        {
            InPortData.Add(new PortData("XYZa", "XYZ a", typeof(XYZ)));
            InPortData.Add(new PortData("XYZb", "XYZ b", typeof(XYZ)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("xyz", "XYZ", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyza = (XYZ)((Value.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewContainer(xyza + xyzb);
        }
    }

    [NodeName("UV Grid")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a grid of UVs from a domain.")]
    public class dynUVGrid : dynNode
    {
        public dynUVGrid()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(object)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(double)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(double)));
            outPortData = new PortData("UVs", "List of UVs in the grid", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> domain;
            double ui, vi;
            
            domain = ((Value.List)args[0]).Item;
            ui = ((Value.Number)args[1]).Item;
            vi = ((Value.Number)args[2]).Item;
            double us = ((Value.Number)domain[2]).Item / ui;
            double vs = ((Value.Number)domain[3]).Item / vi;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            UV min = ((Value.Container)domain[0]).Item as UV;
            UV max = ((Value.Container)domain[1]).Item as UV;

            for (double u = min.U; u <= max.U; u+=us)
            {
                for (double v = min.V; v <= max.V; v+=vs)
                {
                    result = FSharpList<Value>.Cons(
                        Value.NewContainer(new UV(u,v)),
                        result
                    );
                }
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("UV Random Distribution")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a grid of UVs froma domain.")]
    public class dynUVRandom : dynNode
    {
        public dynUVRandom()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(object)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(double)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(double)));
            outPortData = new PortData("UVs", "List of UVs in the grid", typeof(XYZ));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> domain;
            double ui, vi;

            domain = ((Value.List)args[0]).Item;
            ui = ((Value.Number)args[1]).Item;
            vi = ((Value.Number)args[2]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            UV min = ((Value.Container)domain[0]).Item as UV;
            UV max = ((Value.Container)domain[1]).Item as UV;
            
            Random r = new Random();
            double uSpan = max.U-min.U;
            double vSpan = max.V-min.V;

            for (int i = 0; i < ui; i++)
            {
                for (int j = 0; j < vi; j++)
                {
                    result = FSharpList<Value>.Cons(
                        Value.NewContainer(new UV(min.U + r.NextDouble()*uSpan, min.V + r.NextDouble()*vSpan)),
                        result
                    );
                }
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("XYZ Grid")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a grid of XYZs.")]
    public class dynReferencePtGrid : dynNode
    {
        public dynReferencePtGrid()
        {
            InPortData.Add(new PortData("x-count", "Number in the X direction.", typeof(double)));
            InPortData.Add(new PortData("y-count", "Number in the Y direction.", typeof(double)));
            InPortData.Add(new PortData("z-count", "Number in the Z direction.", typeof(double)));
            InPortData.Add(new PortData("x0", "Starting X Coordinate", typeof(double)));
            InPortData.Add(new PortData("y0", "Starting Y Coordinate", typeof(double)));
            InPortData.Add(new PortData("z0", "Starting Z Coordinate", typeof(double)));
            InPortData.Add(new PortData("x-space", "The X spacing.", typeof(double)));
            InPortData.Add(new PortData("y-space", "The Y spacing.", typeof(double)));
            InPortData.Add(new PortData("z-space", "The Z spacing.", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("XYZs", "List of XYZs in the grid", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double xi, yi, zi, x0, y0, z0, xs, ys, zs;

            xi = ((Value.Number)args[0]).Item;
            yi = ((Value.Number)args[1]).Item;
            zi = ((Value.Number)args[2]).Item;
            x0 = ((Value.Number)args[3]).Item;
            y0 = ((Value.Number)args[4]).Item;
            z0 = ((Value.Number)args[5]).Item;
            xs = ((Value.Number)args[6]).Item;
            ys = ((Value.Number)args[7]).Item;
            zs = ((Value.Number)args[8]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            double z = z0;
            for (int zCount = 0; zCount < zi; zCount++)
            {
                double y = y0;
                for (int yCount = 0; yCount < yi; yCount++)
                {
                    double x = x0;
                    for (int xCount = 0; xCount < xi; xCount++)
                    {
                        result = FSharpList<Value>.Cons(
                           Value.NewContainer(new XYZ(x, y, z)),
                           result
                        );
                        x += xs;
                    }
                    y += ys;
                }
                z += zs;
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("XYZ Array Along Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a list of XYZs along a curve.")]
    public class dynXYZArrayAlongCurve : dynNode
    {
        public dynXYZArrayAlongCurve()
        {
            InPortData.Add(new PortData("curve", "Curve", typeof(CurveElement)));
            InPortData.Add(new PortData("count", "Number", typeof(double))); // just divide equally for now, dont worry about spacing and starting point
            //InPortData.Add(new PortData("x0", "Starting Coordinate", typeof(double)));
            //InPortData.Add(new PortData("spacing", "The spacing.", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("XYZs", "List of XYZs in the array", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveElement c = (CurveElement)((Value.Container)args[0]).Item; // Curve 

            double xi;//, x0, xs;
            xi = ((Value.Number)args[1]).Item;// Number
            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing


            FSharpList<Value> result = FSharpList<Value>.Empty;

            //double x = x0;
            Curve crvRef = c.GeometryCurve;
            double t = 0;

            for (int xCount = 0; xCount < xi; xCount++)
            {
                t = xCount / xi; // create normalized curve param by dividing current number by total number
                result = FSharpList<Value>.Cons(
                    Value.NewContainer(
                        crvRef.Evaluate(t, true) // pass in parameter on curve and the bool to say yes this is normalized, Curve.Evaluate passes back out an XYZ that we store in this list
                    ),
                    result
                );
                //x += xs;
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("Plane")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric plane.")]
    public class dynPlane : dynNode
    {
        public dynPlane()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(XYZ)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(XYZ)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("P", "Plane", typeof(Plane));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ ptA = (XYZ)((Value.Container)args[0]).Item;
            XYZ ptB = (XYZ)((Value.Container)args[1]).Item;

            var plane = dynSettings.Instance.Doc.Application.Application.Create.NewPlane(
               ptA, ptB
            );

            return Value.NewContainer(plane);
        }
    }

    [NodeName("Sketch Plane")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric sketch plane.")]
    public class dynSketchPlane : dynRevitNode
    {
        public dynSketchPlane()
        {
            InPortData.Add(new PortData("plane", "The plane in which to define the sketch.", typeof(object))); // SketchPlane can accept Plane, Reference or PlanarFace
            outPortData = new PortData("sp", "SketchPlane", typeof(dynSketchPlane));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("SP", "SketchPlane", typeof(dynSketchPlane));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //TODO: If possible, update to handle mutation rather than deletion...
            foreach (var e in this.Elements)
                this.DeleteElement(e);

            if (input.IsList)
            {
                var planeList = (input as Value.List).Item;

                var result = Utils.SequenceToFSharpList(
                   planeList.Select(
                      delegate(Value x)
                      {
                          SketchPlane sp = null;

                          //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                          if (x is Plane) //TODO: ensure this is correctly casting and testing.
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Plane)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Plane)((Value.Container)x).Item
                              );
                          }
                          else if (x is Reference)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Reference)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Reference)((Value.Container)x).Item
                              );
                          }
                          else if (x is PlanarFace)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (PlanarFace)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (PlanarFace)((Value.Container)x).Item
                              );
                          }


                          this.Elements.Add(sp.Id);
                          return Value.NewContainer(sp);
                      }
                   )
                );

                return Value.NewList(result);
            }
            else
            {
                var x = ((Value.Container)input).Item;
                SketchPlane sp = null;

                //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                if (x is Plane)
                {
                    Plane p = x as Plane;
                    sp  = (this.UIDocument.Document.IsFamilyDocument)
                       ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                       : this.UIDocument.Document.Create.NewSketchPlane(p);
                }
                else if (x is Reference)
                {
                    Reference r = x as Reference;
                    sp  = (this.UIDocument.Document.IsFamilyDocument)
                       ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(r)
                       : this.UIDocument.Document.Create.NewSketchPlane(r);
                } else if (x is PlanarFace)
                {
                    PlanarFace p = x as PlanarFace;
                    sp = (this.UIDocument.Document.IsFamilyDocument)
                       ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                       : this.UIDocument.Document.Create.NewSketchPlane(p);
                }

                this.Elements.Add(sp.Id);

                return Value.NewContainer(sp);
            }
        }
    }

    [NodeName("Line")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric line.")]
    public class dynLineBound : dynNode
    {
        public dynLineBound()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(XYZ)));
            //InPortData.Add(new PortData("bound?", "Boolean: Is this line bounded?", typeof(bool)));

            outPortData = new PortData("line", "Line", typeof(Line));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("line", "Line", typeof(Line));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var ptB = ((Value.Container)args[1]).Item;

            Line line = null;

            if (ptA is XYZ)
            {

                line = dynSettings.Instance.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)ptA, (XYZ)ptB
                  );


            }
            else if (ptA is ReferencePoint)
            {
                line = dynSettings.Instance.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position
               );

            }

            return Value.NewContainer(line);
        }
    }

    [NodeName("Arc by Start Middle End")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric arc given start, middle and end points in XYZ.")]
    public class dynArcStartMiddleEnd : dynNode
    {
        public dynArcStartMiddleEnd()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("mid", "XYZ on Curve", typeof(XYZ)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(XYZ)));
            outPortData = new PortData("arc", "Arc", typeof(Arc));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            Arc a = null;

            var ptA = ((Value.Container)args[0]).Item;//start
            var ptB = ((Value.Container)args[1]).Item;//middle
            var ptC = ((Value.Container)args[2]).Item;//end

            if (ptA is XYZ)
            {

                a = dynSettings.Instance.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, (XYZ)ptC, (XYZ)ptB //start, end, middle 
                );


            }else if (ptA is ReferencePoint)
            {
                a = dynSettings.Instance.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position, (XYZ)((ReferencePoint)ptC).Position //start, end, middle 
                );

            }
            return Value.NewContainer(a);
        }
    }

    [NodeName("Arc by Center Point")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians")]
    public class dynArcCenter : dynNode
    {
        public dynArcCenter()
        {
            InPortData.Add(new PortData("center", "Center XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("radius", "Radius", typeof(double)));
            InPortData.Add(new PortData("start", "Start Param", typeof(double)));
            InPortData.Add(new PortData("end", "End Param", typeof(double)));
            outPortData = new PortData("arc", "Arc", typeof(Arc));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;
            var start = (double)((Value.Number)args[2]).Item;
            var end = (double)((Value.Number)args[3]).Item;

            Arc a = null;

            if (ptA is XYZ)
            {
                a = dynSettings.Instance.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is ReferencePoint)
            {
                a = dynSettings.Instance.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }

            return Value.NewContainer(a);
        }
    }

    [NodeName("Curve Transformed")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Returns the curve (c) transformed by the transform (t).")]
    public class dynCurveTransformed : dynNode
    {
        public dynCurveTransformed()
        {
            InPortData.Add(new PortData("cv", "Curve(Curve)", typeof(object)));
            InPortData.Add(new PortData("t", "Transform(Transform)", typeof(object)));
            outPortData = new PortData("circle", "Circle CurveLoop", typeof(Curve));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var curve = (Curve)((Value.Container)args[0]).Item;
            var trans = (Transform)((Value.Container)args[1]).Item;

            return Value.NewContainer(curve.get_Transformed(trans));
        }
    }

    [NodeName("Circle")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric circle.")]
    public class dynCircle : dynNode
    {
        public dynCircle()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("rad", "Radius", typeof(double)));
            outPortData = new PortData("circle", "Circle CurveLoop", typeof(CurveLoop));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;

            Curve circle = null;

            if (ptA is XYZ)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynSettings.Instance.Doc.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);

            }
            else if (ptA is ReferencePoint)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynSettings.Instance.Doc.Application.Application.Create.NewArc((XYZ)((ReferencePoint)ptA).Position, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);
            }

            return Value.NewContainer(circle);
        }
    }

    [NodeName("Ellipse")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric ellipse.")]
    public class dynEllipse : dynNode
    {
        public dynEllipse()
        {
            InPortData.Add(new PortData("center", "Center XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(double)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(double)));
            outPortData = new PortData("ell", "Ellipse", typeof(Ellipse));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;

            Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynSettings.Instance.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynSettings.Instance.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
                );
            }

            return Value.NewContainer(ell);
        }
    }

    [NodeName("Elliptical Arc")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric elliptical arc. Start and End Values may be between 0 and 2*PI in Radians")]
    public class dynEllipticalArc : dynNode
    {
        public dynEllipticalArc()
        {
            InPortData.Add(new PortData("center", "Center XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(double)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(double)));
            InPortData.Add(new PortData("start", "Start Param", typeof(double)));
            InPortData.Add(new PortData("end", "End Param", typeof(double)));
            outPortData = new PortData("ell", "Ellipse", typeof(Ellipse));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;
            var start = (double)((Value.Number)args[3]).Item;
            var end = (double)((Value.Number)args[4]).Item;

            Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynSettings.Instance.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynSettings.Instance.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
                );
            }
            return Value.NewContainer(ell);
        }
    }

    [NodeName("UV")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a UV from two double values.")]
    public class dynUV : dynNode
    {
        public dynUV()
        {
            InPortData.Add(new PortData("U", "U", typeof(double)));
            InPortData.Add(new PortData("V", "V", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double u, v;
            u = ((Value.Number)args[0]).Item;
            v = ((Value.Number)args[1]).Item;


            return FScheme.Value.NewContainer(new UV(u, v));
        }
    }

    [NodeName("Line Vector")]
    [NodeCategory(BuiltinNodeCategories.REVIT_XYZ_UV_VECTOR)]
    [NodeDescription("Creates a line in the direction of an XYZ normal.")]
    public class dynLineVectorfromXYZ : dynNode
    {
        public dynLineVectorfromXYZ()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(XYZ)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(XYZ)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("C", "Curve", typeof(CurveElement));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = (XYZ)((Value.Container)args[0]).Item;
            var ptB = (XYZ)((Value.Container)args[1]).Item;

            // CurveElement c = MakeLine(this.UIDocument.Document, ptA, ptB);
            CurveElement c = MakeLineCBP(dynSettings.Instance.Doc.Document, ptA, ptB);

            return FScheme.Value.NewContainer(c);
        }


        public ModelCurve MakeLine(Document doc, XYZ ptA, XYZ ptB)
        {
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            // Create plane by the points
            Line line = app.Create.NewLine(ptA, ptB, true);
            XYZ norm = ptA.CrossProduct(ptB);
            double length = norm.GetLength();
            if (length == 0) norm = XYZ.BasisZ;
            Plane plane = app.Create.NewPlane(norm, ptB);
            SketchPlane skplane = doc.FamilyCreate.NewSketchPlane(plane);
            // Create line here
            ModelCurve modelcurve = doc.FamilyCreate.NewModelCurve(line, skplane);
            return modelcurve;
        }

        public CurveByPoints MakeLineCBP(Document doc, XYZ ptA, XYZ ptB)
        {
            ReferencePoint sunRP = doc.FamilyCreate.NewReferencePoint(ptA);
            ReferencePoint originRP = doc.FamilyCreate.NewReferencePoint(ptB);
            ReferencePointArray sunRPArray = new ReferencePointArray();
            sunRPArray.Append(sunRP);
            sunRPArray.Append(originRP);
            CurveByPoints sunPath = doc.FamilyCreate.NewCurveByPoints(sunRPArray);
            return sunPath;
        }
    }

    [NodeName("Hermite Spline")]
    [NodeCategory(BuiltinNodeCategories.REVIT_GEOM)]
    [NodeDescription("Creates a geometric hermite spline.")]
    public class dynHermiteSpline : dynNode
    {
        HermiteSpline hs;

        public dynHermiteSpline()
        {
            InPortData.Add(new PortData("xyzs", "List of pts.(List XYZ)", typeof(object)));
            outPortData = new PortData("ell", "Ellipse", typeof(HermiteSpline));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item;

            hs = null;

            FSharpList<Value> containers = Utils.SequenceToFSharpList(pts);

            List<XYZ> ctrlPts = new List<XYZ>();
            foreach (Value e in containers)
            {
                if (e.IsContainer)
                {
                    XYZ pt = (XYZ)((Value.Container)(e)).Item;
                    ctrlPts.Add(pt);
                }
            }
            if (pts.Count() > 0)
            {
                hs = dynSettings.Instance.Doc.Application.Application.Create.NewHermiteSpline(ctrlPts, false);
            }

            return Value.NewContainer(hs);
        }
    }
}
