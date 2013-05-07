using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using Dynamo.Utilities;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    public class dynFunctionWithRevit : dynFunction
    {
        internal ElementsContainer ElementsContainer = new ElementsContainer();

        public dynFunctionWithRevit(IEnumerable<string> inputs, IEnumerable<string> outputs, FunctionDefinition functionDefinition)
            : base(inputs, outputs, functionDefinition)
        { }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            dynRevitSettings.ElementsContainers.Push(ElementsContainer);
            var result = base.Evaluate(args);
            dynRevitSettings.ElementsContainers.Pop();
            return result;
        }

        public override void Destroy()
        {
            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   dynRevitSettings.Controller.InitTransaction();
                   try
                   {
                       ElementsContainer.DestroyAll();
                   }
                   catch (Exception ex)
                   {
                       dynSettings.Controller.DynamoViewModel.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message
                       );
                   }
                   dynRevitSettings.Controller.EndTransaction();
                   WorkSpace.Modified();
               },
               true
            );
        }
    }
}
