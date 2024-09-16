using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DynamoServices;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("MessageLog")]
    class MessageLogTests : DynamoModelTestBase
    {
        private Exception addWarningEx;
        private int waitHandle1 = 0;
        private int waitHandle2 = 0;

        public void AddWarning()
        {
            try
            {
                ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
                var warnings = runtimeCore.RuntimeStatus.Warnings.Where(x => x.Message.Contains("Test"));
                Assert.AreEqual(3, warnings.Count());
                foreach (var warning in warnings)
                {
                    Interlocked.Increment(ref waitHandle2);
                    while (waitHandle1 > 0)// wait for the main thread to add another warning
                    {
                        Thread.Sleep(100);
                    }
                }
                Assert.AreEqual(3, warnings.Count());
            }
            catch(Exception e)
            {
                addWarningEx = e;
            }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestWarningMessageLog()
        {
            string openPath = Path.Combine(TestDirectory, @"core\messagelog\testwarningmessage.dyn");
            RunModel(openPath);

            ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.WarningCount);

            ProtoCore.Runtime.WarningEntry warningEntry = runtimeCore.RuntimeStatus.Warnings.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.WarningID.Default, warningEntry.ID);
        }

        [Test]
        public void TestInfoMessageLog()
        {
            string openPath = Path.Combine(TestDirectory, @"core\messagelog\ScaleInfo.dyn");
            RunModel(openPath);

            ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            Assert.AreEqual(1, runtimeCore.RuntimeStatus.InfosCount);

            ProtoCore.Runtime.InfoEntry infoEntry = runtimeCore.RuntimeStatus.Infos.ElementAt(0);
            Assert.AreEqual(ProtoCore.Runtime.InfoID.Default, infoEntry.ID);
        }

        [Test]
        public void MultiThreadWarnings()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 3; i++)
                    LogWarningMessageEvents.OnLogWarningMessage($"Test{i}");

                waitHandle1 = 1;// make thread1 wait until waitHandle is 0

                Thread thread1 = new Thread(AddWarning);
                thread1.Start();

                while (waitHandle2 == 0 && thread1.IsAlive)// wait for thread1 to reach the Warning iteration
                {
                    Thread.Sleep(100);
                }

                LogWarningMessageEvents.OnLogWarningMessage($"Test3");

                Interlocked.Decrement(ref waitHandle1);
           
                thread1.Join();

                if (addWarningEx != null) throw addWarningEx;

                ProtoCore.RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
                Assert.AreEqual(4, runtimeCore.RuntimeStatus.WarningCount);
            });
        }
    }
}
