using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo;
using Dynamo.Logging;
using Dynamo.Python;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class CodeCompletionTests : UnitTestBase
    {
        [Test]
        public void SharedCoreCanFindFirstLoadedIfNotMatch()
        {
            var provider = new SharedCompletionProvider(PythonNodeModels.PythonEngineVersion.CPython3, "");
            Assert.IsNotNull(provider);
        }
    }
}
