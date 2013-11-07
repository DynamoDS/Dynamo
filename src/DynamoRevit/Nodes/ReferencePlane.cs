using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Reference Plane")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a reference plane")]
    public class ReferencePlane : RevitTransactionNodeWithOneOutput
    {
        public ReferencePlane()
        {
            InPortData.Add(new PortData("l", "Geometry Line.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("ref", "Reference Plane", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var input = args[0];

            //If we are receiving a list, we must create reg planes for each curve in the list.
            if (input.IsList)
            {
                var curveList = (input as FScheme.Value.List).Item;

                //Counter to keep track of how many ref planes we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var result = Utils.SequenceToFSharpList(
                   curveList.Select(
                    //..taking each element in the list and...
                      delegate(FScheme.Value x)
                      {
                          Autodesk.Revit.DB.ReferencePlane refPlane;
                          Line line;
                          XYZ bubbleEnd;
                          XYZ freeEnd;
                          //...if we already have elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out refPlane))
                              {
                                  //...and if we're successful, update it's position (well for now make a new one with the same name)... 
                                  string name = refPlane.Name;
                                  this.UIDocument.Document.Delete(refPlane.Id);//delete old one for now

                                  //refPlane.Reference = (Line)((Value.Container)x).Item;// these are all readonly, how to modify exising grid then?

                                  //then make a new one using new line and old name 
                                  line = (Line)((FScheme.Value.Container)x).Item;
                                  bubbleEnd = line.get_EndPoint(0);
                                  freeEnd = line.get_EndPoint(1);

                                  refPlane = this.UIDocument.Document.IsFamilyDocument
                                    ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView)
                                    : this.UIDocument.Document.Create.NewReferencePlane(
                                        bubbleEnd,
                                        freeEnd,
                                        XYZ.BasisZ,
                                        this.UIDocument.ActiveView);

                                  refPlane.Name = name;
                              }
                              else
                              {
                                  //...otherwise, we can make a new ref plane and replace it in the list of
                                  //previously created ref planes.
                                  line = (Line)((FScheme.Value.Container)x).Item;
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
                              line = (Line)((FScheme.Value.Container)x).Item;
                              bubbleEnd = line.get_EndPoint(0);
                              freeEnd = line.get_EndPoint(1);

                              refPlane = this.UIDocument.Document.IsFamilyDocument
                                  ? this.UIDocument.Document.FamilyCreate.NewReferencePlane(
                                      bubbleEnd,
                                      freeEnd,
                                      XYZ.BasisZ,
                                      this.UIDocument.ActiveView)
                                  : this.UIDocument.Document.Create.NewReferencePlane(
                                      bubbleEnd,
                                      freeEnd,
                                      XYZ.BasisZ,
                                      this.UIDocument.ActiveView);

                              //...and store it in the element list for future runs.
                              this.Elements.Add(refPlane.Id);

                          }
                          //Finally, we update the counter, and return a new Value containing the level.
                          //This Value will be placed in the Value.List that will be passed downstream from this
                          //node.
                          count++;
                          return FScheme.Value.NewContainer(refPlane);
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
                return FScheme.Value.NewList(result);
            }
            //If we're not receiving a list, we will just assume we received one curve.
            else
            {
                //Ref plane elements take in one geometry curve 
                Line c = (Line)((FScheme.Value.Container)args[0]).Item;

                Autodesk.Revit.DB.ReferencePlane refPlane;
                Line line;
                XYZ bubbleEnd;
                XYZ freeEnd;
                string name;

                if (this.Elements.Any())
                {
                    if (dynUtils.TryGetElement(this.Elements[0], out refPlane))
                    {
                        //...and if we're successful, update it's position (well for now make a new one with the same name)... 
                        name = refPlane.Name;

                        XYZ oldBubbleEnd = refPlane.BubbleEnd;
                        XYZ oldFreeEnd = refPlane.FreeEnd;
                        XYZ midPointOld = 0.5 * (oldBubbleEnd + oldFreeEnd);

                        //refPlane.Reference = (Line)((Value.Container)x).Item;// these are all readonly, how to modify exising grid then?

                        //then make a new one using new line and old name 
                        line = (Line)c;
                        bubbleEnd = line.get_EndPoint(0);
                        freeEnd = line.get_EndPoint(1);

                        XYZ midPoint = 0.5 * (bubbleEnd + freeEnd);
                        XYZ moveVec = XYZ.BasisZ.DotProduct(midPoint - midPointOld) * XYZ.BasisZ;
                        bool didByMove = true;
                        try
                        {
                            ElementTransformUtils.MoveElement(this.UIDocument.Document, refPlane.Id, moveVec);
                            refPlane.BubbleEnd = bubbleEnd;
                            refPlane.FreeEnd = freeEnd;
                        }
                        catch
                        {
                            didByMove = false;
                        }
                        if (!didByMove)
                        {
                            this.UIDocument.Document.Delete(refPlane.Id);//delete old one for now

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

                return FScheme.Value.NewContainer(refPlane);
            }
        }
    }
}
