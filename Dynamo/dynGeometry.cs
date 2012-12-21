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

using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
    [ElementName("XYZ")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an XYZ from three numbers.")]
    [RequiresTransaction(false)]
    public class dynXYZ : dynNode
    {
        public dynXYZ()
        {
            InPortData.Add(new PortData("X", "X", typeof(double)));
            InPortData.Add(new PortData("Y", "Y", typeof(double)));
            InPortData.Add(new PortData("Z", "Z", typeof(double)));

            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double x, y, z;
            x = ((Expression.Number)args[0]).Item;
            y = ((Expression.Number)args[1]).Item;
            z = ((Expression.Number)args[2]).Item;

            return Expression.NewContainer(new XYZ(x, y, z));
        }
    }

    [ElementName("XYZ -> X")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Fetches the X value of the given XYZ")]
    [RequiresTransaction(false)]
    public class dynXYZGetX : dynNode
    { 
        public dynXYZGetX()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            OutPortData = new PortData("X", "X value of given XYZ", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(((XYZ)((Expression.Container)args[0]).Item).X);
        }
    }

    [ElementName("XYZ -> Y")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Fetches the Y value of the given XYZ")]
    [RequiresTransaction(false)]
    public class dynXYZGetY : dynNode
    {
        public dynXYZGetY()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            OutPortData = new PortData("Y", "Y value of given XYZ", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(((XYZ)((Expression.Container)args[0]).Item).Y);
        }
    }

    [ElementName("XYZ -> Z")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Fetches the Z value of the given XYZ")]
    [RequiresTransaction(false)]
    public class dynXYZGetZ : dynNode
    {
        public dynXYZGetZ()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(XYZ)));
            OutPortData = new PortData("Z", "Z value of given XYZ", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(((XYZ)((Expression.Container)args[0]).Item).Z);
        }
    }

    [ElementName("XYZ Zero")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an XYZ at the origin (0,0,0).")]
    [RequiresTransaction(false)]
    public class dynXYZZero : dynNode
    {
        public dynXYZZero()
        {
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            return Expression.NewContainer(XYZ.Zero);
        }
    }

    [ElementName("XYZ BasisX")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an XYZ representing the X basis (1,0,0).")]
    [RequiresTransaction(false)]
    public class dynXYZBasisX : dynNode
    {
        public dynXYZBasisX()
        {
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            return Expression.NewContainer(XYZ.BasisX);
        }
    }

    [ElementName("XYZ BasisY")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an XYZ representing the Y basis (0,1,0).")]
    [RequiresTransaction(false)]
    public class dynXYZBasisY : dynNode
    {
        public dynXYZBasisY()
        {
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            return Expression.NewContainer(XYZ.BasisY);
        }
    }

    [ElementName("XYZ BasisZ")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates an XYZ representing the Z basis (0,0,1).")]
    [RequiresTransaction(false)]
    public class dynXYZBasisZ : dynNode
    {
        public dynXYZBasisZ()
        {
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            return Expression.NewContainer(XYZ.BasisZ);
        }
    }

    [ElementName("XYZ Scale")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Multiplies each component of an XYZ by a number.")]
    [RequiresTransaction(false)]
    public class dynXYZScale : dynNode
    {
        public dynXYZScale()
        {
            InPortData.Add(new PortData("XYZ", "XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("n", "Scale value.", typeof(double)));
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            XYZ xyz = (XYZ)((Expression.Container)args[0]).Item;
            double n = ((Expression.Number)args[1]).Item;

            return Expression.NewContainer(xyz.Multiply(n));
        }
    }

    [ElementName("XYZ Add")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Adds the components of two XYZs.")]
    [RequiresTransaction(false)]
    public class dynXYZAdd : dynNode
    {
        public dynXYZAdd()
        {
            InPortData.Add(new PortData("XYZa", "XYZ a", typeof(XYZ)));
            InPortData.Add(new PortData("XYZb", "XYZ b", typeof(XYZ)));
            OutPortData = new PortData("xyz", "XYZ", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            XYZ xyza = (XYZ)((Expression.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((Expression.Container)args[1]).Item;

            return Expression.NewContainer(xyza + xyzb);
        }
    }

    [ElementName("XYZ Grid")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a grid of XYZs.")]
    [RequiresTransaction(false)]
    public class 
        dynReferencePtGrid : dynNode
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

            OutPortData = new PortData("XYZs", "List of XYZs in the grid", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double xi, yi, zi, x0, y0, z0, xs, ys, zs;

            xi = ((Expression.Number)args[0]).Item;
            yi = ((Expression.Number)args[1]).Item;
            zi = ((Expression.Number)args[2]).Item;
            x0 = ((Expression.Number)args[3]).Item;
            y0 = ((Expression.Number)args[4]).Item;
            z0 = ((Expression.Number)args[5]).Item;
            xs = ((Expression.Number)args[6]).Item;
            ys = ((Expression.Number)args[7]).Item;
            zs = ((Expression.Number)args[8]).Item;

            FSharpList<Expression> result = FSharpList<Expression>.Empty;

            double z = z0;
            for (int zCount = 0; zCount < zi; zCount++)
            {
                double y = y0;
                for (int yCount = 0; yCount < yi; yCount++)
                {
                    double x = x0;
                    for (int xCount = 0; xCount < xi; xCount++)
                    {
                        result = FSharpList<Expression>.Cons(
                           Expression.NewContainer(new XYZ(x, y, z)),
                           result
                        );
                        x += xs;
                    }
                    y += ys;
                }
                z += zs;
            }

            return Expression.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [ElementName("XYZ Array Along Curve")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a list of XYZs along a curve.")]
    [RequiresTransaction(false)]
    public class dynXYZArrayAlongCurve : dynNode
    {
        public dynXYZArrayAlongCurve()
        {
            InPortData.Add(new PortData("curve", "Curve", typeof(CurveElement)));
            InPortData.Add(new PortData("count", "Number", typeof(double))); // just divide equally for now, dont worry about spacing and starting point
            //InPortData.Add(new PortData("x0", "Starting Coordinate", typeof(double)));
            //InPortData.Add(new PortData("spacing", "The spacing.", typeof(double)));

            OutPortData = new PortData("XYZs", "List of XYZs in the array", typeof(XYZ));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            CurveElement c = (CurveElement)((Expression.Container)args[0]).Item; // Curve 

            double xi;//, x0, xs;
            xi = ((Expression.Number)args[1]).Item;// Number
            //x0 = ((Expression.Number)args[2]).Item;// Starting Coord
            //xs = ((Expression.Number)args[3]).Item;// Spacing


            FSharpList<Expression> result = FSharpList<Expression>.Empty;

            //double x = x0;
            Curve crvRef = c.GeometryCurve;
            double t = 0;

            for (int xCount = 0; xCount < xi; xCount++)
            {
                t = xCount / xi; // create normalized curve param by dividing current number by total number
                result = FSharpList<Expression>.Cons(
                    Expression.NewContainer(
                        crvRef.Evaluate(t, true) // pass in parameter on curve and the bool to say yes this is normalized, Curve.Evaluate passes back out an XYZ that we store in this list
                    ),
                    result
                );
                //x += xs;
            }

            return Expression.NewList(
               ListModule.Reverse(result)
            );
        }
    }


    [ElementName("Plane")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a geometric plane.")]
    [RequiresTransaction(false)]
    public class dynPlane : dynNode
    {
        public dynPlane()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(XYZ)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(XYZ)));
            OutPortData = new PortData("P", "Plane", typeof(Plane));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            XYZ ptA = (XYZ)((Expression.Container)args[0]).Item;
            XYZ ptB = (XYZ)((Expression.Container)args[1]).Item;

            var plane = this.UIDocument.Application.Application.Create.NewPlane(
               ptA, ptB
            );

            return Expression.NewContainer(plane);
        }
    }

    [ElementName("Sketch Plane")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a geometric sketch plane.")]
    [RequiresTransaction(true)]
    public class dynSketchPlane : dynNode
    {
        public dynSketchPlane()
        {
            InPortData.Add(new PortData("plane", "The plane in which to define the sketch.", typeof(dynPlane)));
            OutPortData = new PortData("SP", "SketchPlane", typeof(dynSketchPlane));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            //TODO: If possible, update to handle mutation rather than deletion...
            foreach (var e in this.Elements)
                this.DeleteElement(e);

            if (input.IsList)
            {
                var planeList = (input as Expression.List).Item;

                var result = Utils.convertSequence(
                   planeList.Select(
                      delegate(Expression x)
                      {
                          SketchPlane p = this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                             (Plane)((Expression.Container)x).Item
                          );

                          this.Elements.Add(p.Id);
                          return Expression.NewContainer(p);
                      }
                   )
                );

                return Expression.NewList(result);
            }
            else
            {
                Plane p = (Plane)((Expression.Container)input).Item;

                SketchPlane sp = (this.UIDocument.Document.IsFamilyDocument)
                   ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                   : this.UIDocument.Document.Create.NewSketchPlane(p);

                this.Elements.Add(sp.Id);

                return Expression.NewContainer(sp);
            }
        }
    }

    [ElementName("Line")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a geometric line.")]
    [RequiresTransaction(false)]
    public class dynLineBound : dynNode
    {
        public dynLineBound()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(XYZ)));
            InPortData.Add(new PortData("bound?", "Boolean: Is this line bounded?", typeof(bool)));
            OutPortData = new PortData("line", "Line", typeof(Line));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var ptA = (XYZ)((Expression.Container)args[0]).Item;
            var ptB = (XYZ)((Expression.Container)args[1]).Item;
            var bound = ((Expression.Number)args[2]).Item == 1;

            return Expression.NewContainer(
               this.UIDocument.Application.Application.Create.NewLine(
                  ptA, ptB, bound
               )
            );
        }
    }

    [ElementName("UV")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a UV from two double values.")]
    [RequiresTransaction(false)]
    public class dynUV : dynNode
    {
        public dynUV()
        {
            InPortData.Add(new PortData("U", "U", typeof(double)));
            InPortData.Add(new PortData("V", "V", typeof(double)));

            OutPortData = new PortData("uv", "UV", typeof(UV));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double u, v;
            u = ((Expression.Number)args[0]).Item;
            v = ((Expression.Number)args[1]).Item;


            return FScheme.Expression.NewContainer(new UV(u, v));
        }
    }

    [ElementName("Line Vector ")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a line in the direction of an XYZ normal.")]
    [RequiresTransaction(true)]
    public class dynLineVectorfromXYZ : dynNode
    {
        public dynLineVectorfromXYZ()
        {

            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(XYZ)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(XYZ)));
            OutPortData = new PortData("C", "Curve", typeof(CurveElement));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var ptA = (XYZ)((Expression.Container)args[0]).Item;
            var ptB = (XYZ)((Expression.Container)args[1]).Item;

            // CurveElement c = MakeLine(this.UIDocument.Document, ptA, ptB);
            CurveElement c = MakeLineCBP(this.UIDocument.Document, ptA, ptB);

            return FScheme.Expression.NewContainer(c);
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

}
