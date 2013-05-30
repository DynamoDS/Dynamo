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
    [NodeName("Level")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a level datum.")]
    public class dynLevel : dynRevitTransactionNodeWithOneOutput
    {
        public dynLevel()
        {
            InPortData.Add(new PortData("el", "The elevation of the level.", typeof(Value.Number)));
            OutPortData.Add(new PortData("level", "The level.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Level elements take in one double for the z elevation (height)f
            double h = (double)((Value.Number)args[0]).Item;

            Level lev;

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(Level), out e))
                {
                    lev = e as Level;
                    lev.Elevation = h;

                }
                else
                {
                    lev = this.UIDocument.Document.IsFamilyDocument
                        ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                        : this.UIDocument.Document.Create.NewLevel(h);
                    this.Elements[0] = lev.Id;
                }
            }
            else
            {
                lev = this.UIDocument.Document.IsFamilyDocument
                    ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                    : this.UIDocument.Document.Create.NewLevel(h);
                this.Elements.Add(lev.Id);
            }

            return Value.NewContainer(lev);
        }
    }

    [NodeName("Ref Plane")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a reference plane.")]
    public class dynReferencePlane : dynRevitTransactionNodeWithOneOutput
    {
        public dynReferencePlane()
        {
            InPortData.Add(new PortData("l", "Geometry Line.", typeof(Value.Container)));
            OutPortData.Add(new PortData("ref", "Reference Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //If we are receiving a list, we must create reg planes for each curve in the list.
            if (input.IsList)
            {
                var curveList = (input as Value.List).Item;

                //Counter to keep track of how many ref planes we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   curveList.Select(
                    //..taking each element in the list and...
                      delegate(Value x)
                      {
                          ReferencePlane refPlane;
                          Line line;
                          XYZ bubbleEnd;
                          XYZ freeEnd;
                          string name;
                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count],typeof(ReferencePlane), out e))
                              {
                                  //...and if we're successful, update it's position (well for now make a new one with the same name)... 
                                  refPlane = e as ReferencePlane;
                                  name = refPlane.Name;
                                  this.UIDocument.Document.Delete(refPlane.Id);//delete old one for now

                                  //refPlane.Reference = (Line)((Value.Container)x).Item;// these are all readonly, how to modify exising grid then?

                                  //then make a new one using new line and old name 
                                  line = (Line)((Value.Container)x).Item; 
                                  bubbleEnd = line.get_EndPoint(0);
                                  freeEnd = line.get_EndPoint(1);

                                  refPlane = this.UIDocument.Document.IsFamilyDocument
                                    ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView
                                    )
                                    : this.UIDocument.Document.Create.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView
                                    );
                                  refPlane.Name = name;


                              }
                              else
                              {
                                  //...otherwise, we can make a new ref plane and replace it in the list of
                                  //previously created ref planes.
                                  line = (Line)((Value.Container)x).Item;
                                  bubbleEnd = line.get_EndPoint(0);
                                  freeEnd = line.get_EndPoint(1);

                                  refPlane = this.UIDocument.Document.IsFamilyDocument
                                    ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView

                                    )
                                    : this.UIDocument.Document.Create.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView
                                    );
                                  this.Elements[0] = refPlane.Id;

                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new ref plane...
                            line = (Line)((Value.Container)x).Item;
                            bubbleEnd = line.get_EndPoint(0);
                            freeEnd = line.get_EndPoint(1);

                            refPlane = this.UIDocument.Document.IsFamilyDocument
                                ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                                    bubbleEnd,
                                    freeEnd,
                                    XYZ.BasisZ,
                                    this.UIDocument.ActiveView

                                )
                                : this.UIDocument.Document.Create.NewReferencePlane(
                                    bubbleEnd,
                                    freeEnd,
                                    XYZ.BasisZ,
                                    this.UIDocument.ActiveView
                                    );
                              //...and store it in the element list for future runs.
                              this.Elements.Add(refPlane.Id);

                          }
                          //Finally, we update the counter, and return a new Value containing the level.
                          //This Value will be placed in the Value.List that will be passed downstream from this
                          //node.
                          count++;
                          return Value.NewContainer(refPlane);
                      }
                   )
                );

                //Now that we've created all the Levels from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                //Fin
                return Value.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one curve.
            else
            {
                //Ref plane elements take in one geometry curve 
                Line c = (Line)((Value.Container)args[0]).Item;

                ReferencePlane refPlane;
                Line line;
                XYZ bubbleEnd;
                XYZ freeEnd;
                string name;

                if (this.Elements.Any())
                {
                    Element e;
                    if (dynUtils.TryGetElement(this.Elements[0],typeof(ReferencePlane), out e))
                    {
                        
                        //...and if we're successful, update it's position (well for now make a new one with the same name)... 
                        refPlane = e as ReferencePlane;
                        name = refPlane.Name;
                        this.UIDocument.Document.Delete(refPlane.Id);//delete old one for now

                        //refPlane.Reference = (Line)((Value.Container)x).Item;// these are all readonly, how to modify exising grid then?

                        //then make a new one using new line and old name 
                        line = (Line)c;
                        bubbleEnd = line.get_EndPoint(0);
                        freeEnd = line.get_EndPoint(1);

                        refPlane = this.UIDocument.Document.IsFamilyDocument
                          ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                              bubbleEnd,
                              freeEnd,
                              XYZ.BasisZ,
                              this.UIDocument.ActiveView
                          )
                          : this.UIDocument.Document.Create.NewReferencePlane(
                              bubbleEnd,
                              freeEnd,
                              XYZ.BasisZ,
                              this.UIDocument.ActiveView
                          );
                        refPlane.Name = name;

                    }
                    else
                    {
                        //then make a new one using new line and old name 
                        line = c;
                        bubbleEnd = line.get_EndPoint(0);
                        freeEnd = line.get_EndPoint(1);

                        refPlane = this.UIDocument.Document.IsFamilyDocument
                          ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                              bubbleEnd,
                              freeEnd,
                              XYZ.BasisZ,
                              this.UIDocument.ActiveView
                          )
                          : this.UIDocument.Document.Create.NewReferencePlane(
                              bubbleEnd,
                              freeEnd,
                              XYZ.BasisZ,
                              this.UIDocument.ActiveView
                          );
                        this.Elements[0] = refPlane.Id;
                    }
                }
                else
                {
                    //then make a new one using new line and old name 
                    line = c;
                    bubbleEnd = line.get_EndPoint(0);
                    freeEnd = line.get_EndPoint(1);

                    refPlane = this.UIDocument.Document.IsFamilyDocument
                      ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                          bubbleEnd,
                          freeEnd,
                          XYZ.BasisZ,
                          this.UIDocument.ActiveView
                      )
                      : this.UIDocument.Document.Create.NewReferencePlane(
                          bubbleEnd,
                          freeEnd,
                          XYZ.BasisZ,
                          this.UIDocument.ActiveView
                      );
                    this.Elements.Add(refPlane.Id);
                }

                //Now that we've created this single ref plane from this run, we delete all of the
                // potential extra ones from the previous run.
                // this is to handle going from a list down to a simgle element.
                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return Value.NewContainer(refPlane);
            }
        }
    }

    [NodeName("Column Grid")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a column grid datum.")]
    public class dynColumnGrid : dynRevitTransactionNodeWithOneOutput
    {
        public dynColumnGrid()
        {
            InPortData.Add(new PortData("line", "Geometry Line.", typeof(Value.Container))); // MDJ TODO - expand this to work with curved grids.
            OutPortData.Add(new PortData("grid", "Grid", typeof(Value.Container)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {

            var input = args[0];

            //If we are receiving a list, we must create a column grid for each curve in the list.
            if (input.IsList)
            {
                var curveList = (input as Value.List).Item;

                //Counter to keep track of how many column grids we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   curveList.Select(
                    //..taking each element in the list and...
                      delegate(Value x)
                      {
                          Grid grid;
                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count],typeof(Grid), out e))
                              {
                                  //...and if we're successful, update it's position... 
                                  grid = e as Grid;
                                  //grid.Curve = (Curve)((Value.Container)x).Item; // these are all readonly, how to modify exising grid then?
                                  //MDJ TODO - figure out how to move grid or use document.Create.NewGrid(geomLine)
                                  // hack - make a new one for now
                                  string gridNum = grid.Name;
                                  grid = this.UIDocument.Document.Create.NewGrid(
                                     (Line)((Value.Container)x).Item
                                  );
                                  grid.Name = gridNum;

                              }
                              else
                              {
                                  //...otherwise, we can make a new column grid and replace it in the list of
                                  //previously created grids.
                                  //grid = this.UIDocument.Document.IsFamilyDocument
                                  //?
                                    //this.UIDocument.Document.FamilyCreate.NewLevel(
                                    // (double)((Value.Number)x).Item
                                  //)
                                  //: 
                                  grid = this.UIDocument.Document.Create.NewGrid(
                                     (Line)((Value.Container)x).Item
                                  );
                                  this.Elements[0] = grid.Id;

                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new column grid...
                              //grid = this.UIDocument.Document.IsFamilyDocument
                              //?
                              //this.UIDocument.Document.FamilyCreate.NewLevel(
                              // (double)((Value.Number)x).Item
                              //)
                              //: 
                              grid = this.UIDocument.Document.Create.NewGrid(
                                 (Line)((Value.Container)x).Item
                              );
                              //...and store it in the element list for future runs.
                              this.Elements.Add(grid.Id);

                          }
                          //Finally, we update the counter, and return a new Value containing the grid.
                          //This Value will be placed in the Value.List that will be passed downstream from this
                          //node.
                          count++;
                          return Value.NewContainer(grid);
                      }
                   )
                );

                //Now that we've created all the column grids from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                //Fin
                return Value.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one double height.
            else
            {
                //Column grid elements take in one curve for their geometry
                Line c = (Line)((Value.Container)args[0]).Item;

                Grid grid;

                if (this.Elements.Any())
                {
                    Element e;
                    if (dynUtils.TryGetElement(this.Elements[0],typeof(Grid), out e))
                    {
                        grid = e as Grid;
                        grid = this.UIDocument.Document.Create.NewGrid(c);

                    }
                    else
                    {
                        //grid = this.UIDocument.Document.IsFamilyDocument
                        //   ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                        //   : 
                        grid = this.UIDocument.Document.Create.NewGrid(c);
                        this.Elements[0] = grid.Id;
                    }
                }
                else
                {
                    //grid = this.UIDocument.Document.IsFamilyDocument
                    //   ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                    //   : 
                    grid = this.UIDocument.Document.Create.NewGrid(c);
                    this.Elements.Add(grid.Id);
                }

                //Now that we've created this single Level from this run, we delete all of the
                // potential extra ones from the previous run.
                // this is to handle going from a list down to a simgle element.
                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return Value.NewContainer(grid);
            }
        }
    }
}