using System;
using System.Reflection;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Revit;


using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Base class for all auto-generated Revit API nodes.
    /// </summary>
    public abstract class dynAPIMethodNode : dynRevitTransactionNodeWithOneOutput
    {
        protected Type base_type;
        protected Type return_type;
        protected MethodBase mi;
        protected ParameterInfo[] pi;

        ///<summary>
        ///Default constructor
        ///</summary>
        public dynAPIMethodNode()
        {

        }

        ///<summary>
        ///Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        ///</summary>
        public override Value Evaluate(FSharpList<Value> args)
        {
            foreach (var e in this.Elements)
            {
                this.DeleteElement(e);
            }

            Value result = dynRevitUtils.InvokeAPIMethod(this, args, base_type, pi, mi, return_type);

            return result;
        }

    }

    /// <summary>
    /// Base class for wrapped properties. Does not create a transaction.
    /// </summary>
    public abstract class dynAPIPropertyNode : dynNodeWithOneOutput
    {
        protected Type base_type;
        protected Type return_type;
        protected PropertyInfo pi;

        ///<summary>
        ///Default constructor
        ///</summary>
        public dynAPIPropertyNode()
        {

        }

        ///<summary>
        ///Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        ///</summary>
        public override Value Evaluate(FSharpList<Value> args)
        {
            return dynRevitUtils.GetAPIPropertyValue(args, base_type, pi, return_type);
        }
    }

    /// <summary>
    /// Revit Document node. Returns the active Revit Document.
    /// </summary>
    [NodeName("Revit Document")]
    [NodeSearchTags("document", "active")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Gets the active Revit document.")]
    public class dynRevitDocument : dynRevitTransactionNodeWithOneOutput
    {
        public dynRevitDocument()
        {
            OutPortData.Add(new PortData("doc", "The active Revit doc.", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(dynRevitSettings.Doc.Document);
        }
    }
}
