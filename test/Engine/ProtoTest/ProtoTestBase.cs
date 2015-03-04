using ProtoTestFx.TD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProtoTest
{
    abstract class ProtoTestBase
    {
        protected ProtoCore.Core core;
        protected ProtoCore.RuntimeCore runtimeCore;
        protected TestFrameWork thisTest = new TestFrameWork();

        [SetUp]
        public virtual void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));

            // This is set when a test is executed 
            runtimeCore = null;
        }

        private void CleanupRuntimeCore()
        {
            // Executing a test can either come from DebugRunner or TestFrameWork
            // We want the generated runtimecore from either of these frameworks to be properly disposed.
            // The DebugRunner should handle setting runtimeCore in its TearDown function
            // As a fallback, if the runtimeCore is null, it is assumed that the test was run from the TestFramework
            if (runtimeCore == null)
            {
                runtimeCore = thisTest.GetTestRuntimeCore();
            }
            runtimeCore.Cleanup();
        }

        [TearDown]
        public virtual void TearDown()
        {
            CleanupRuntimeCore();
            thisTest.CleanUp();
        }
    }
}
