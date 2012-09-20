//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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
using Dynamo.Controls;

namespace Dynamo.Elements
{
   [ElementName("Transaction")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Executes Expression inside of a Revit API transaction")]
   [RequiresTransaction(false)]
   public class dynTransaction : dynNode
   {
      public dynTransaction()
      {
         InPortData.Add(new PortData("expr", "Expression to run in a transaction.", typeof(object)));
         OutPortData = new PortData("result", "Result of the expression.", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override bool RequiresManualTransaction()
      {
         return true;
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         ExternMacro m = new ExternMacro(
            delegate(FSharpList<Expression> args, ExecutionEnvironment environment)
            {
               if (this.Bench.RunCancelled)
                  throw new CancelEvaluationException(false);

               var arg = args[0]; //Get the only argument

               if (dynElementSettings.SharedInstance.Bench.RunInDebug)
               {
                  return environment.Evaluate(arg);
               }

               var result = IdlePromise<Expression>.ExecuteOnIdle(
                  delegate
                  {
                     this.Bench.InIdleThread = true;
                     dynElementSettings.SharedInstance.Bench.InitTransaction();

                     try
                     {
                        var exp = environment.Evaluate(arg);

                        UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
                        Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

                        dynElementSettings.SharedInstance.Bench.EndTransaction();

                        this.ValidateConnections();

                        return exp;
                     }
                     catch (CancelEvaluationException ex)
                     {
                        throw ex;
                     }
                     catch (Exception ex)
                     {
                        this.Dispatcher.Invoke(new Action(
                           delegate
                           {
                              Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                              this.Bench.Log(ex);
                              this.Bench.ShowElement(this);

                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.Message);
                              dynElementSettings.SharedInstance.Writer.WriteLine(ex.StackTrace);
                           }
                        ));

                        this.Error(ex.Message);
                        return null;
                     }
                  }
               );

               this.Bench.InIdleThread = false;

               return result;
            }
         );

         return new ExternalMacroNode(m, portNames);
      }
   }
}
