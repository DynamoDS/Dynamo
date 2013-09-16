using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Model Text")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Creates a model text object.")]
    public class ModelText : RevitTransactionNodeWithOneOutput
    {
        public ModelText()
        {
            InPortData.Add(new PortData("text", "The text to create.", typeof (FScheme.Value.String)));
            InPortData.Add(new PortData("position", "The position of the model text object.",
                                        typeof (FScheme.Value.String)));
            InPortData.Add(new PortData("normal", "The orientation of the model text object.",
                                        typeof (FScheme.Value.String)));
            InPortData.Add(new PortData("up", "The up axis of the model text.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("depth", "The depth of the model text object.", typeof (FScheme.Value.String)));
            InPortData.Add(new PortData("text type name", "The name of the model text type to use.", typeof(FScheme.Value.String)));
            OutPortData.Add(new PortData("model text", "The model text object(s).", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args)
        {
            var text = ((FScheme.Value.String) args[0]).Item;
            var position = (XYZ) ((FScheme.Value.Container) args[1]).Item;
            var normal = (XYZ) ((FScheme.Value.Container) args[2]).Item;
            var up = (XYZ) ((FScheme.Value.Container) args[3]).Item;
            var depth = ((FScheme.Value.Number) args[4]).Item;
            var textTypeName = ((FScheme.Value.String) args[5]).Item;

            //find a text type in the document to use
            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ModelTextType));
            ModelTextType mtt;
            if (fec.ToElements().Cast<ModelTextType>().Any(x => x.Name == textTypeName))
            {
                mtt = fec.ToElements().First() as ModelTextType;
            }
            else
            {
                throw new Exception(string.Format("A model text type named {0} could not be found in the document.", textTypeName));
            }

            Autodesk.Revit.DB.ModelText mt = null;

            if (Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(Elements[0], typeof (Autodesk.Revit.DB.ModelText), out e))
                {
                    mt = (Autodesk.Revit.DB.ModelText) e;

                    //if the position or normal are different
                    //we have to recreate
                    var currPos = mt.Location as LocationPoint;
                    if (!position.IsAlmostEqualTo(currPos.Point))
                    {
                        dynRevitSettings.Doc.Document.Delete(Elements[0]);
                        mt = CreateModelText(normal, position, -up, text, mtt, depth);
                        Elements.Add(mt.Id);
                    }
                    else
                    {
                        //reset the text and the depth
                        mt.Text = text;
                        mt.Depth = depth;
                        Elements[0] = mt.Id;
                    }

                }
            }
            else
            {
                mt = CreateModelText(normal, position, -up, text, mtt, depth);
                Elements.Add(mt.Id);
            }

            return FScheme.Value.NewContainer(mt);
        }

        private static Autodesk.Revit.DB.ModelText CreateModelText(XYZ normal, XYZ position, XYZ up, string text, ModelTextType mtt,
                                                 double depth)
        {
            Autodesk.Revit.DB.ModelText mt = null;
            var xAxis = normal.CrossProduct(up).Normalize();
            var yAxis = normal.CrossProduct(xAxis).Normalize();
            var plane = new Autodesk.Revit.DB.Plane(xAxis, yAxis, position);
           
            var sp = Autodesk.Revit.DB.SketchPlane.Create(dynRevitSettings.Doc.Document, plane);
            mt = dynRevitSettings.Doc.Document.FamilyCreate.NewModelText(text, mtt, sp, position, HorizontalAlign.Left, depth);
            return mt;
        }
    }
}
