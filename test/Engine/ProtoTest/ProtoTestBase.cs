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
        protected TestFrameWork thisTest = new TestFrameWork();

        [SetUp]
        public virtual void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
        }

        [TearDown]
        public virtual void TearDown()
        {
            core.__TempCoreHostForRefactoring.Cleanup();
            thisTest.CleanUp();
        }
    }
}
