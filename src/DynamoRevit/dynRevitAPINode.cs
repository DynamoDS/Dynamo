using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Revit;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public abstract class dynRevitAPINode : dynRevitTransactionNodeWithOneOutput
    {
        protected Type base_type;
        protected Type return_type;
        protected MethodBase mi;
        protected ParameterInfo[] pi;

        ///<summary>
        ///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        ///</summary>
        public dynRevitAPINode()
        {

        }

        ///<summary>
        ///Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        ///</summary>
        public override Value Evaluate(FSharpList<Value> args)
        {
            Elements.ForEach(
            delegate(ElementId el)
            {
                Element e;
                if (dynUtils.TryGetElement(el, out e))
                {
                    DeleteElement(e.Id);
                }
            });
            Elements.Clear();

            return dynRevitUtils.InvokeAPIMethod(this, args, base_type, pi, mi, return_type);
        }
    }

    [NodeName("Revit Document")]
    [NodeSearchTags("document", "active")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Gets the active Revit document.")]
    public class dynRevitDocument : dynRevitTransactionNodeWithOneOutput
    {
        public dynRevitDocument()
        {
            OutPortData.Add(new PortData("doc", "The active Revit doc.", typeof(Autodesk.Revit.DB.Document)));
            NodeUI.RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(dynRevitSettings.Doc.Document);
        }
    }
}
