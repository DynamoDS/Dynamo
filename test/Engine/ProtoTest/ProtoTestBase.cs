using NUnit.Framework;
using ProtoTestFx.TD;

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
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

            // This is set when a test is executed 
            runtimeCore = null;
        }

        private void CleanupRuntimeCore()
        {
            // If a runtimeCore was used for the test, call its Cleanup
            if (runtimeCore != null)
            {
                runtimeCore.Cleanup();
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            CleanupRuntimeCore();
            thisTest.CleanUp();
        }
    }
}
