using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;

using Autodesk.Revit.DB;

using Dynamo.Connectors;

using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
    [ElementName("Element ID")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Returns the ID of the given Element.")]
    [RequiresTransaction(false)]
    public class dynElementId : dynNode
    {
        public dynElementId()
        {
            this.InPortData.Add(new PortData("e", "A Revit Element", typeof(Element)));
            this.OutPortData = new PortData("id", "Element ID", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)
        {
            Element e = (Element)((Expression.Container)args[0]).Item;
            return Expression.NewNumber(e.Id.IntegerValue);
        }
    }
}
