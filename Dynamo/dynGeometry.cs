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
    [ElementDescription("An element which creates an XYZ from three double values.")]
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

    [ElementName("XYZ Scale")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which multiplies each component of an XYZ by a number.")]
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
    [ElementDescription("An element which adds the components of two XYZs.")]
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
    [ElementDescription("An element which creates a grid of reference points.")]
    [RequiresTransaction(false)]
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
    [ElementDescription("An element which creates an array of XYZs along a curve.")]
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

    [ElementName("Divided Path")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which divides curves or edges and makes a collection")]
    [RequiresTransaction(true)]
    public class dynDividedPath : dynNode
    {
        public dynDividedPath()
        {
            InPortData.Add(new PortData("refs", "Ref", typeof(CurveElement)));//TODO make this a ref, but how to handle tracking persistance
            InPortData.Add(new PortData("count", "Number", typeof(double))); // just divide equally for now, dont worry about spacing and starting point
            //InPortData.Add(new PortData("x0", "Starting Coordinate", typeof(double)));
            //InPortData.Add(new PortData("spacing", "The spacing.", typeof(double)));

            OutPortData = new PortData("dc ", "the divided path element", typeof(DividedPath));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];
            double xi;//, x0, xs;
            xi = ((Expression.Number)args[1]).Item;// Number
            //x0 = ((Expression.Number)args[2]).Item;// Starting Coord
            //xs = ((Expression.Number)args[3]).Item;// Spacing

            DividedPath divPath;
            List<Reference> refList = new List<Reference>();

            // this node can take one or more curve elements and create one or more divided path elements
            // input is one or more user-selected curves for now
            // todo: - create a utility function that can handle curve loops and convert them into lists, perhaps makes this a node or just an allowed input of this node
            //       - enhance curve by selection node to handle multiple picks
            //       - allow selection of a family instance to extract a curve loop or reference list
            //
            // process input curve elements
            // - if we pass in a single curve element, we will extract it's reference and pass that into the divided path creation method
            // - if we pass in a collection of curve elements, we want to make a divided path for each curve from that collection
            //    for each curve element in list, 
            //      manage curve list (determine whether we already had used that curve before to make this divided path, add new curves, remove old curves)
            //      extract curve refs for each curve element and create divided path
            //      update params of for the div path
            //  this.Elements should only hold divided paths not curves.
            // node should return a list of divided paths

            if (input.IsList)
            {
                refList.Clear();

                var curveList = (input as Expression.List).Item;
                
                //Counter to keep track of how many references and divided path. We'll use this to delete old
                //elements later.
                int count = 0;


                //We create our output by...
                var result = Utils.convertSequence(
                   curveList.Select(
                    //..taking each curve in the list and...
                      delegate(Expression x)
                      {
                          Reference r;
                          CurveElement c;
                          
                          //...check to see if we already have a divided node made by this curve in a previous run
                          if (this.Elements.Count > count)
                          {
                              Element e;

                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out e))
                              {
                                  //...if we find a divided path and if we're successful matching it to the doc, update it's properties... 
                                  divPath = e as DividedPath;

                                  if (divPath!= null)
                                  {
                                      divPath.FixedNumberOfPoints = (int)xi;
                                     
                                  }
                                  else
                                  {
                                      //...otherwise, we can make a new divided path and replace it in the list of
                                      //previously created divided paths.
                                      //...we extract a curve element from the container.
                                      c = (CurveElement)((Expression.Container)x).Item;
                                      //...we create a new curve ref
                                      Curve crvRef = c.GeometryCurve;
                                      refList.Add(crvRef.Reference);
                                      divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                                      divPath.FixedNumberOfPoints = (int)xi;
                                      this.Elements[count] = divPath.Id;
                                      refList.Clear();
                                      
                                  }
                              }
                              else
                              {
                                  //...otherwise, we can make a new divided path and replace it in the list of
                                  //previously created divided paths.
                                  //...we extract a curve element from the container.
                                  c = (CurveElement)((Expression.Container)x).Item;
                                  //...we create a new curve ref
                                  Curve crvRef = c.GeometryCurve;
                                  refList.Add(crvRef.Reference);
                                  divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                                  divPath.FixedNumberOfPoints = (int)xi;
                                  this.Elements[count] = divPath.Id;
                                  refList.Clear();
                              }

                          }
                          //...otherwise...
                          else
                          {
                              //...we extract a curve element from the container.
                              c = (CurveElement)((Expression.Container)x).Item;
                              //...we create a new curve ref
                              Curve crvRef = c.GeometryCurve;
                              refList.Add(crvRef.Reference);
                              divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                              divPath.FixedNumberOfPoints = (int)xi;
                              //...and store the element in the element list for future runs.
                              this.Elements.Add(divPath.Id);
                              refList.Clear();
                          }
                          //Finally, we update the counter, and return a new Expression containing the reference list.

                          count++;
                          return Expression.NewContainer(divPath);
                      }
                   )
                );

                //Now that we've added all the divided paths from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var eid in this.Elements.Skip(count))
                {
                    
                    this.DeleteElement(eid); // remove unused divided paths
                }


                return Expression.NewList(result);
            }

            //If we're not receiving a list, we will just assume we received one curve.
            else
            {
                refList.Clear();

                CurveElement c = (CurveElement)((Expression.Container)input).Item; 


                FSharpList<Expression> result = FSharpList<Expression>.Empty;

                //double x = x0;
                Curve crvRef = c.GeometryCurve;

                refList.Add(crvRef.Reference); 

                //If we've made any elements previously...
                if (this.Elements.Any())
                {
                    Element e;
                    //...try to get the first one...
                    if (dynUtils.TryGetElement(this.Elements[0], out e))
                    {
                        //..and if we do, update it's data.
                        divPath = e as DividedPath;
                        divPath.FixedNumberOfPoints = (int)xi;
                    }
                    else
                    {
                        //...otherwise, just make a new one and replace it in the list.
                        divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                        divPath.FixedNumberOfPoints = (int)xi;
                        this.Elements[0] = divPath.Id;
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
                    //...just make a divided curve and store it.
                    divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                    divPath.FixedNumberOfPoints = (int)xi;
                    this.Elements.Add(divPath.Id);
                }
                refList.Clear();

                //Fin
                return Expression.NewContainer(divPath);
            }
        }
    }

    [ElementName("Plane")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which creates a geometric plane.")]
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
    [ElementDescription("An element which creates a geometric sketch plane.")]
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
    [ElementDescription("An element which creates a geometric line.")]
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
    [ElementDescription("An element which creates a UV from two double values.")]
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
    [ElementDescription("An element which returns a line in the direction of an XYZ normal.")]
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
