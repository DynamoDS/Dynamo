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
    public class ReplicatorTest
    {
        ProtoCore.Core core;
        DebugRunner fsr;
        ProtoScript.Config.RunConfiguration runnerConfig;
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }


        [Test]
        //Test "SomeNulls()"
        public void ComputeReducedParams()
        {
            string code =
                           @"                      a = {1,2};                      b = {3,4};                  ";

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
