using ProtoCore.AST.AssociativeAST;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.DSASM;
using Dynamo.Models;
using DynCmd = Dynamo.Models.DynamoModel;
using ProtoCore.Mirror;
using Dynamo.DSEngine;
using ProtoCore.Utils;
using Dynamo.DSEngine.CodeCompletion;
using System.IO;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class UnicodeTest : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void TestUnicodeIdentifierInCBN()
        {
            RunModel(@"core\unicode_test\unicodeInCBN.dyn");
            AssertPreviewValue("b0e39eef-abd2-4da2-9c5e-bc45129210f4", 48);
            AssertPreviewValue("dfcf8646-fa78-443f-b708-e06d713ca21e", 12);
            AssertPreviewValue("87c8b0a6-5e3b-4436-a8b4-d459d0937337", 27);
        }
    }
}
