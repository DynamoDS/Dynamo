using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Column Grid")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a column grid datum")]
    public class ColumnGrid : RevitTransactionNodeWithOneOutput
    {
        public ColumnGrid()
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
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out grid))
                              {
                                  //...and if we're successful, update it's position... 
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
                    if (dynUtils.TryGetElement(this.Elements[0], out grid))
                    {
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

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "Grid.ByLine", "Grid.ByLine@Line");
        }
    }

}