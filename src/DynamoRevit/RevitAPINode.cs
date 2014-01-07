using System;
using System.Reflection;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Revit;


using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Base class for all auto-generated Revit API nodes.
    /// </summary>
    public abstract class ApiMethodNode : RevitTransactionNodeWithOneOutput
    {
        protected Type base_type;
        protected Type return_type;
        protected MethodBase mi;
        protected ParameterInfo[] pi;

        ///<summary>
        ///Default constructor
        ///</summary>
        public ApiMethodNode()
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
    public abstract class ApiPropertyNode : NodeWithOneOutput
    {
        protected Type base_type;
        protected Type return_type;
        protected PropertyInfo pi;

        ///<summary>
        ///Default constructor
        ///</summary>
        public ApiPropertyNode()
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
    public class RevitDocument : RevitTransactionNodeWithOneOutput
    {
        public RevitDocument()
        {
            OutPortData.Add(new PortData("doc", "The active Revit doc.", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(DocumentManager.GetInstance().CurrentUIDocument.Document);
        }
    }
}
