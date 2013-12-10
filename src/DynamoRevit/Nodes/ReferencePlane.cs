using System;
using System.Linq;
using Autodesk.Revit.DB;
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
            InPortData.Add(new PortData("name", "The name of the reference plane.", typeof(FScheme.Value.String)));
            OutPortData.Add(new PortData("ref", "Reference Plane", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //Ref plane elements take in one geometry curve 
            var c = (Line)((FScheme.Value.Container)args[0]).Item;
            var name = ((FScheme.Value.String) args[1]).Item;

            //give the plane a unique name temporarily
            //it will be renamed after evaluation when the element
            //name store is flushed
            var tmpName = Guid.NewGuid().ToString();

            Autodesk.Revit.DB.ReferencePlane refPlane;
            Line line;
            XYZ bubbleEnd;
            XYZ freeEnd;

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out refPlane))
                {
                    //...and if we're successful, update it's position (well for now make a new one with the same name)... 
                    //name = refPlane.Name;

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
                        refPlane.Name = tmpName;
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
                        refPlane.Name = tmpName;
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
                    refPlane.Name = tmpName;
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
                refPlane.Name = tmpName;
            }

            if (string.IsNullOrEmpty(name))
            {
                dynRevitSettings.Controller.ElementNameStore.Add(refPlane.Id, "ReferencePlane_"+tmpName);
            }
            else
            {
                dynRevitSettings.Controller.ElementNameStore.Add(refPlane.Id, name);
            }
            

            return FScheme.Value.NewContainer(refPlane);  
        }
    }
}
