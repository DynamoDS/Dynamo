using System;
using Dynamo;
using NUnit.Framework;

namespace ProtoTest
{
    class WarningCompatibilityTest : UnitTestBase
    {
        [Test]
        [Description("Tests the backwards compatibility of the RuntimeStatus.Warnings property")]
        public void TestWarningCompatibility()
        {
            TestWarningsBackwardComp.Test.TestWarningBackwardsCompatibility();
        }
    }
}
