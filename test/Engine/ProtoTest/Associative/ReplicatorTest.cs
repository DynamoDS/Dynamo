using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class ReplicatorTest : ProtoTestBase
    {
        DebugRunner fsr;
        ProtoScript.Config.RunConfiguration runnerConfig;

        public override void Setup()
        {
            // Specify some of the requirements of IDE.
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }

        [Test]
        [Category("Failure")]
        //Test "SomeNulls()"
        public void ComputeReducedParams()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4115
            string code =
                           @"
                      a = {1,2};
                      b = {3,4};
                  ";

            //Run
            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = null;
            vms = fsr.Run();
            var mirror = vms.mirror;
            List<StackValue> args = new List<StackValue>();
            args.Add(mirror.GetRawFirstValue("a"));
            args.Add(mirror.GetRawFirstValue("b"));
            List<ReplicationInstruction> ris = new List<ReplicationInstruction>();
            ris.Add(
                new ReplicationInstruction()
                    {
                        ZipIndecies = new List<int> { 0, 1 },
                        Zipped = true
                    }
                );
            List<List<StackValue>> combin = ProtoCore.Lang.Replication.Replicator.ComputeAllReducedParams(args, ris, core);
            Assert.IsTrue(combin[0][0].opdata == 1);
            Assert.IsTrue(combin[0][1].opdata == 3);
            Assert.IsTrue(combin[1][0].opdata == 2);
            Assert.IsTrue(combin[1][1].opdata == 4);
        }
    }
}
