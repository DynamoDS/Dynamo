using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Expression = Dynamo.FScheme.Expression;
using Autodesk.Revit.DB;
using System.Diagnostics;

namespace Dynamo.Elements
{
   [ElementName("Transaction")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Executes Expression inside of a Revit API transaction")]
   [RequiresTransaction(false)]
   public class dynTransaction : dynElement
   {
      public dynTransaction()
      {
         InPortData.Add(new PortData("expr", "Expression to run in a transaction.", typeof(object)));
         OutPortData = new PortData("result", "Result of the expression.", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         ExternMacro m = new ExternMacro(
            delegate(FSharpList<Expression> args, ExecutionEnvironment environment)
            {
               var arg = args[0]; //Get the only argument

               if (dynElementSettings.SharedInstance.Bench.RunInDebug)
               {
                  return environment.Evaluate(arg);
               }

               var result = IdlePromise<Expression>.ExecuteOnIdle(
                  delegate
                  {
                     dynElementSettings.SharedInstance.Bench.InitTransaction();

                     try
                     {
                        var exp = environment.Evaluate(arg);

                        UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                        Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                        dynElementSettings.SharedInstance.Bench.EndTransaction();

                        return exp;
                     }
                     catch (Exception ex)
                     {
                        this.Dispatcher.Invoke(new Action(
                           delegate
                           {
                              Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                              dynElementSettings.SharedInstance.Bench.Log(ex.Message);
                              dynElementSettings.SharedInstance.Bench.ShowElement(this);
                           }
                        ));

                        SetToolTipDelegate sttd = new SetToolTipDelegate(SetTooltip);
                        Dispatcher.Invoke(sttd, System.Windows.Threading.DispatcherPriority.Background,
                              new object[] { ex.Message });

                        MarkConnectionStateDelegate mcsd = new MarkConnectionStateDelegate(MarkConnectionState);
                        Dispatcher.Invoke(mcsd, System.Windows.Threading.DispatcherPriority.Background,
                              new object[] { true });

                        this.Dispatcher.Invoke(new Action(
                           delegate
                           {
                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                           }
                        ));

                        return null;
                     }
                  }
               );

               return result;
            }
         );

         return new ExternalMacroNode(m, portNames);
      }
   }
}
