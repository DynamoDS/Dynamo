using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Dynamo.Connectors;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Select Structural Framing Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Create structural framing.")]
    public class dynStructuralFramingSelector: dynDropDrownBase
    {
        public dynStructuralFramingSelector()
        {
            OutPortData.Add(new PortData("type", "The selected structural framing type.", typeof(Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            base.PopulateItems();

            Items.Clear();

            //find all the structural framing family types in the project
            var collector = new FilteredElementCollector(dynRevitSettings.Doc.Document);

            var catFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            collector.OfClass(typeof(FamilySymbol)).WherePasses(catFilter);

            foreach (var e in collector.ToElements())
                Items.Add(new DynamoDropDownItem(e.Name, e));
        }

    }

    [NodeName("Structural Framing")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Create structural framing.")]
    public class dynStructuralFraming : dynRevitTransactionNodeWithOneOutput
    {
        public dynStructuralFraming()
        {
            InPortData.Add(new PortData("type", "The framing type.", typeof(Value.Container)));
            InPortData.Add(new PortData("curves", "The curve(s) to be used as center lines for your framing elements.", typeof(Value.Container)));
            InPortData.Add(new PortData("normal", "A vector to act as a target for the rotation of the framing element.", typeof(Value.Container)));
            OutPortData.Add(new PortData("framing", "The structural framing instance(s) created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var symbol = (FamilySymbol)((Value.Container)args[0]).Item;
            var curve = (Curve) ((Value.Container) args[1]).Item;
            var target = (XYZ)((Value.Container) args[2]).Item;

            //calculate the desired rotation
            //we do this by finding the angle between the z axis
            //and vector between the start of the beam and the target point
            //both projected onto the start plane of the beam.

            XYZ zAxis = new XYZ(0,0,1);
            XYZ yAxis = new XYZ(0, 1, 0);

            //flatten the beam line onto the XZ plane
            //using the start's z coordinate
            XYZ start = curve.get_EndPoint(0);
            XYZ end = curve.get_EndPoint(1);
            XYZ newEnd = new XYZ(end.X, end.Y, start.Z); //drop end point to plane

            ////use the x axis of the curve's transform 
            ////as the normal of the start plane
            //XYZ planeNormal = (curve.get_EndPoint(0) - curve.get_EndPoint(1)).Normalize();

            //catch the case where the end is directly above
            //the start, creating a normal with zero length
            //in that case, use the Z axis
            XYZ planeNormal = newEnd.IsAlmostEqualTo(start) ? zAxis : (newEnd - start).Normalize();

            XYZ target_project = target - target.DotProduct(planeNormal) * planeNormal;
            XYZ z_project = zAxis - zAxis.DotProduct(planeNormal) * planeNormal;

            //double gamma = target_project.AngleTo(z_project);
            double gamma = target.AngleOnPlaneTo(zAxis.IsAlmostEqualTo(planeNormal) ? yAxis : zAxis, planeNormal);

            Debug.WriteLine(gamma);

            FamilyInstance instance = null;
            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(FamilyInstance), out e))
                {
                    instance = e as FamilyInstance;

                    //update the curve
                    var locCurve = instance.Location as LocationCurve;
                    locCurve.Curve = curve;
                }
                else
                {
                    instance = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(curve, symbol,
                                                                                  dynRevitSettings.DefaultLevel,
                                                                                  StructuralType.Beam);
                    this.Elements[0] = instance.Id;
                }
            }
            else
            {
                instance = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(curve, symbol,
                                                                                  dynRevitSettings.DefaultLevel,
                                                                                  StructuralType.Beam);
                this.Elements.Add(instance.Id);
            }

            //TODO:update the gamma
            Parameter p = instance.get_Parameter("Cross-Section Rotation");
            if (p != null)
            {
                if(gamma != p.AsDouble())
                    p.Set(gamma);
            }

            return Value.NewContainer(instance);
        }
        
    }
}
