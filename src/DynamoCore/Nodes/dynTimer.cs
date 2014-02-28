using Dynamo.Models;
using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using System.Threading;
using System.Linq;
using System.Xml;

namespace Dynamo.Nodes
{
    [NodeName("Pause")]
    [NodeDescription("Pauses execution for a given amount of time.")]
    [NodeCategory(BuiltinNodeCategories.CORE_TIME)]
    public class Pause : NodeWithOneOutput
    {
        public Pause()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(Value.Number)));
            OutPortData.Add(new PortData("", "Success", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc 
        {
            get { return true; }
            set { }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            int ms = (int)((Value.Number)args[0]).Item;

            Thread.Sleep(ms);

            return Value.NewDummy("pause");
        }
    }

    [NodeName("Execution Interval")]
    [NodeDescription("Forces an Execution after every interval")]
    [NodeCategory(BuiltinNodeCategories.CORE_TIME)]
    public class ExecuteInterval : NodeWithOneOutput
    {
        private Thread delayThread;

        public ExecuteInterval()
        {
            InPortData.Add(new PortData("ms", "Delay in milliseconds", typeof(Value.Number)));
            OutPortData.Add(new PortData("", "Success?", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            int delay = (int)((Value.Number)args[0]).Item;

            if (delayThread == null || !delayThread.IsAlive)
            {
                delayThread = new Thread(new ThreadStart(
                    delegate
                    {
                        Thread.Sleep(delay);

                        if (Controller == null || Controller.RunCancelled)
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

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

}
