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

        [Test]
        public void TestUnicodeInStringNode()
        {
            RunModel(@"core\unicode_test\unicodeInStringNode.dyn");
            AssertPreviewValue("2ac4c6a7-83a1-4775-b8f4-7fa9001d33f7", "<\"äö&üß\">");
        }

        [Test]
        public void TestUnicodeInIronPython()
        {
            RunModel(@"core\unicode_test\unicodeInIronPython.dyn");
            AssertPreviewValue("5b91e128-0f9e-411d-9b8f-76ed6f9aa85c", "中文字符123");
            AssertPreviewValue("4192b7c7-c4f8-42ce-9144-2f058e68be6b", 7);
        }
    }
}
