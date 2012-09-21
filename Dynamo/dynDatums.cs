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
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
    [ElementName("Level")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a level datum.")]
    [RequiresTransaction(true)]
    public class dynLevel : dynNode
    {
        public dynLevel()
        {
            InPortData.Add(new PortData("h", "Height.", typeof(double)));
            OutPortData = new PortData("l", "Level", typeof(Level));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            var input = args[0];

            //If we are receiving a list, we must create levels for each double in the list.
            if (input.IsList)
            {
                var doubleList = (input as Expression.List).Item;

                //Counter to keep track of how many levels we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.convertSequence(
                   doubleList.Select(
                    //..taking each element in the list and...
                      delegate(Expression x)
                      {
                          Level lev;
                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out e))
                              {
                                  //...and if we're successful, update it's position... 
                                  lev = e as Level;
                                  lev.Elevation = (double)((Expression.Number)x).Item;
                              }
                              else
                              {
                                  //...otherwise, we can make a new level and replace it in the list of
                                  //previously created level.
                                  lev = this.UIDocument.Document.IsFamilyDocument
                                  ? this.UIDocument.Document.FamilyCreate.NewLevel(
                                     (double)((Expression.Number)x).Item
                                  )
                                  : this.UIDocument.Document.Create.NewLevel(
                                     (double)((Expression.Number)x).Item
                                  );
                                  this.Elements[0] = lev.Id;

                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new level...
                              lev = this.UIDocument.Document.FamilyCreate.NewLevel(
                                 (double)((Expression.Number)x).Item
                              );
                              //...and store it in the element list for future runs.
                              this.Elements.Add(lev.Id);

                          }
                          //Finally, we update the counter, and return a new Expression containing the level.
                          //This Expression will be placed in the Expression.List that will be passed downstream from this
                          //node.
                          count++;
                          return Expression.NewContainer(lev);
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
                return Expression.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one double height.
            else
            {
                //Level elements take in one double for the z elevation (height)f
                double h = (double)((Expression.Number)args[0]).Item;

                Level lev;

                if (this.Elements.Any())
                {
                    Element e;
                    if (dynUtils.TryGetElement(this.Elements[0], out e))
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

                //Now that we've created this single Level from this run, we delete all of the
                // potential extra ones from the previous run.
                // this is to handle going from a list down to a simgle element.
                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return Expression.NewContainer(lev);
            }
        }
    }
}