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

using Value = Dynamo.FScheme.Value;
using System.Threading;

namespace Dynamo.Nodes
{
    [NodeName("Pause")]
    [NodeDescription("Pauses execution for a given amount of time.")]
    [NodeCategory(BuiltinNodeCategories.CORE_TIME)]
    public class dynPause : dynNodeWithOneOutput
    {
        public dynPause()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(Value.Number)));
            OutPortData.Add(new PortData("", "Success", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            int ms = (int)((Value.Number)args[0]).Item;

            Thread.Sleep(ms);

            return Value.NewNumber(1);
        }
    }

    [NodeName("Execution Interval")]
    [NodeDescription("Forces an Execution after every interval")]
    [NodeCategory(BuiltinNodeCategories.CORE_TIME)]
    public class dynExecuteInterval : dynNodeWithOneOutput
    {
        public dynExecuteInterval()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(Value.Number)));
            OutPortData.Add(new PortData("", "Success?", typeof(Value.Number)));

            RegisterAllPorts();
        }

        Thread delayThread;

        //protected override void OnRunCancelled()
        //{
        //    if (delayThread != null && delayThread.IsAlive)
        //        delayThread.Abort();
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {
            int delay = (int)((Value.Number)args[0]).Item;

            if (delayThread == null || !delayThread.IsAlive)
            {
                delayThread = new Thread(new ThreadStart(
                    delegate
                    {
                        Thread.Sleep(delay);

                        if (Controller.RunCancelled)
                            return;

                        while (Controller.Running)
                        {
                            Thread.Sleep(1);
                            if (Controller.RunCancelled)
                                return;
                        }

                        this.RequiresRecalc = true;
                    }
                ));

                delayThread.Start();
            }

            return Value.NewNumber(1);
        }
    }

}
