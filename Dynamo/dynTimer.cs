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
using Dynamo.Connectors;

using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;
using System.Threading;

namespace Dynamo.Elements
{
    [ElementName("Pause")]
    [ElementDescription("Pauses execution for a given amount of time.")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynPause : dynNode
    {
        public dynPause()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(double)));
            OutPortData = new PortData("", "Success", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)
        {
            int ms = (int)((Expression.Number)args[0]).Item;

            Thread.Sleep(ms);

            return Expression.NewNumber(1);
        }
    }

    [ElementName("Execution Interval")]
    [ElementDescription("Forces an Execution after every interval")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynExecuteInterval : dynNode
    {
        public dynExecuteInterval()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(double)));
            OutPortData = new PortData("", "Success?", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        Thread delayThread;

        //protected override void OnRunCancelled()
        //{
        //    if (delayThread != null && delayThread.IsAlive)
        //        delayThread.Abort();
        //}

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            int delay = (int)((Expression.Number)args[0]).Item;

            if (delayThread == null || !delayThread.IsAlive)
            {
                delayThread = new Thread(new ThreadStart(
                    delegate
                    {
                        Thread.Sleep(delay);

                        if (this.Bench.RunCancelled)
                            return;

                        while (this.Bench.Running)
                        {
                            Thread.Sleep(1);
                            if (this.Bench.RunCancelled)
                                return;
                        }

                        this.IsDirty = true;
                    }
                ));

                delayThread.Start();
            }

            return Expression.NewNumber(1);
        }
    }

}
