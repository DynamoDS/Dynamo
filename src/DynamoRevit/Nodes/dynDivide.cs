//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
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
using Dynamo.Revit;


namespace Dynamo.Nodes
{
    [NodeName("Divided Path")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Divides curves or edges and makes a DividedPath.")]
    public class dynDividedPath : dynRevitTransactionNodeWithOneOutput
    {
        public dynDividedPath()
        {
            InPortData.Add(new PortData("refs", "Ref", typeof(Value.Container)));//TODO make this a ref, but how to handle tracking persistance
            InPortData.Add(new PortData("count", "Number", typeof(Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            //InPortData.Add(new PortData("x0", "Starting Coordinate", typeof(double)));
            //InPortData.Add(new PortData("spacing", "The spacing.", typeof(double)));

            OutPortData.Add(new PortData("dc ", "the divided path element", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            double xi;//, x0, xs;
            xi = ((Value.Number)args[1]).Item;// Number
            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing

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

                var curveList = (input as Value.List).Item;

                //Counter to keep track of how many references and divided path. We'll use this to delete old
                //elements later.
                int count = 0;


                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   curveList.Select(
                    //..taking each curve in the list and...
                      delegate(Value x)
                      {
                          //Reference r;
                          CurveElement c;

                          //...check to see if we already have a divided node made by this curve in a previous run
                          if (this.Elements.Count > count)
                          {
                              Element e;

                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count],typeof(DividedPath), out e))
                              {
                                  //...if we find a divided path and if we're successful matching it to the doc, update it's properties... 
                                  divPath = e as DividedPath;

                                  if (divPath != null)
                                  {
                                      divPath.FixedNumberOfPoints = (int)xi;

                                  }
                                  else
                                  {
                                      //...otherwise, we can make a new divided path and replace it in the list of
                                      //previously created divided paths.
                                      //...we extract a curve element from the container.
                                      c = (CurveElement)((Value.Container)x).Item;
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
                                  c = (CurveElement)((Value.Container)x).Item;
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
                              c = (CurveElement)((Value.Container)x).Item;
                              //...we create a new curve ref
                              Curve crvRef = c.GeometryCurve;
                              refList.Add(crvRef.Reference);
                              divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                              divPath.FixedNumberOfPoints = (int)xi;
                              //...and store the element in the element list for future runs.
                              this.Elements.Add(divPath.Id);
                              refList.Clear();
                          }
                          //Finally, we update the counter, and return a new Value containing the reference list.

                          count++;
                          return Value.NewContainer(divPath);
                      }
                   )
                );

                //Now that we've added all the divided paths from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var eid in this.Elements.Skip(count))
                {

                    this.DeleteElement(eid); // remove unused divided paths
                }


                return Value.NewList(result);
            }

            //If we're not receiving a list, we will just assume we received one curve.
            else
            {
                refList.Clear();

                CurveElement c = (CurveElement)((Value.Container)input).Item;


                FSharpList<Value> result = FSharpList<Value>.Empty;

                //double x = x0;
                Curve crvRef = c.GeometryCurve;

                refList.Add(crvRef.Reference);

                //If we've made any elements previously...
                if (this.Elements.Any())
                {
                    Element e;
                    //...try to get the first one...
                    if (dynUtils.TryGetElement(this.Elements[0],typeof(DividedPath), out e))
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
                return Value.NewContainer(divPath);
            }
        }
    }

    [NodeName("Divided Surface")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("An element which divides surfaces into patterns and faces")]
    public class dynDividedSurface : dynRevitTransactionNodeWithOneOutput
    {
        public dynDividedSurface()
        {
            InPortData.Add(new PortData("face", "Ref", typeof(Value.Container)));//Ref to a face of a form
            InPortData.Add(new PortData("U", "Spacing on face in U direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("V", "Spacing on face in U direction", typeof(Value.Number)));

            OutPortData.Add(new PortData("ds ", "the divided surface element", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            double xi;//, x0, xs;
            xi = ((Value.Number)args[1]).Item;// Number
            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing

            DividedSurface divSurf;
            List<Reference> refList = new List<Reference>();

            // this node can take one or more face refs and create one or more divided surface elements


            //if (input.IsList)
            //{
            //    refList.Clear();

            //    var curveList = (input as Value.List).Item;

            //    //Counter to keep track of how many references and divided path. We'll use this to delete old
            //    //elements later.
            //    int count = 0;


            //    //We create our output by...
            //    var result = Utils.convertSequence(
            //       curveList.Select(
            //        //..taking each curve in the list and...
            //          delegate(Value x)
            //          {
            //              Reference r;
            //              CurveElement c;

            //              //...check to see if we already have a divided node made by this curve in a previous run
            //              if (this.Elements.Count > count)
            //              {
            //                  Element e;

            //                  //...we attempt to fetch it from the document...
            //                  if (dynUtils.TryGetElement(this.Elements[count], out e))
            //                  {
            //                      //...if we find a divided path and if we're successful matching it to the doc, update it's properties... 
            //                      divPath = e as DividedPath;

            //                      if (divPath != null)
            //                      {
            //                          divPath.FixedNumberOfPoints = (int)xi;

            //                      }
            //                      else
            //                      {
            //                          //...otherwise, we can make a new divided path and replace it in the list of
            //                          //previously created divided paths.
            //                          //...we extract a curve element from the container.
            //                          c = (CurveElement)((Value.Container)x).Item;
            //                          //...we create a new curve ref
            //                          Curve crvRef = c.GeometryCurve;
            //                          refList.Add(crvRef.Reference);
            //                          divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
            //                          divPath.FixedNumberOfPoints = (int)xi;
            //                          this.Elements[count] = divPath.Id;
            //                          refList.Clear();

            //                      }
            //                  }
            //                  else
            //                  {
            //                      //...otherwise, we can make a new divided path and replace it in the list of
            //                      //previously created divided paths.
            //                      //...we extract a curve element from the container.
            //                      c = (CurveElement)((Value.Container)x).Item;
            //                      //...we create a new curve ref
            //                      Curve crvRef = c.GeometryCurve;
            //                      refList.Add(crvRef.Reference);
            //                      divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
            //                      divPath.FixedNumberOfPoints = (int)xi;
            //                      this.Elements[count] = divPath.Id;
            //                      refList.Clear();
            //                  }

            //              }
            //              //...otherwise...
            //              else
            //              {
            //                  //...we extract a curve element from the container.
            //                  c = (CurveElement)((Value.Container)x).Item;
            //                  //...we create a new curve ref
            //                  Curve crvRef = c.GeometryCurve;
            //                  refList.Add(crvRef.Reference);
            //                  divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
            //                  divPath.FixedNumberOfPoints = (int)xi;
            //                  //...and store the element in the element list for future runs.
            //                  this.Elements.Add(divPath.Id);
            //                  refList.Clear();
            //              }
            //              //Finally, we update the counter, and return a new Value containing the reference list.

            //              count++;
            //              return Value.NewContainer(divPath);
            //          }
            //       )
            //    );

            //    //Now that we've added all the divided paths from this run, we delete all of the
            //    //extra ones from the previous run.
            //    foreach (var eid in this.Elements.Skip(count))
            //    {

            //        this.DeleteElement(eid); // remove unused divided paths
            //    }


            //    return Value.NewList(result);
            //}

            ////If we're not receiving a list, we will just assume we received one curve.
            //else
            //{
                refList.Clear();

                Reference r = (Reference)((Value.Container)input).Item;


                FSharpList<Value> result = FSharpList<Value>.Empty;


                refList.Add(r);

                //If we've made any elements previously...
                if (this.Elements.Any())
                {
                    Element e;
                    //...try to get the first one...
                    if (dynUtils.TryGetElement(this.Elements[0],typeof(DividedSurface), out e))
                    {
                        //..and if we do, update it's data.
                        divSurf = e as DividedSurface;
                        //divSurf. = (int)xi;
                    }
                    else
                    {
                        //...otherwise, just make a new one and replace it in the list.

                        divSurf = this.UIDocument.Document.FamilyCreate.NewDividedSurface(r);
                        
                        //divSurf.FixedNumberOfPoints = (int)xi;
                        this.Elements[0] = divSurf.Id;
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
                    divSurf = this.UIDocument.Document.FamilyCreate.NewDividedSurface(r);
                    //divSurf.FixedNumberOfPoints = (int)xi;
                    this.Elements.Add(divSurf.Id);
                }
                refList.Clear();

                //Fin
                return Value.NewContainer(divSurf);
            }
        //}
    }
}
