using System;
using NUnit.Framework;

namespace ProtoTest
{
    class WarningCompatibilityTest : ProtoTestBase
    {
        [Test]
        [Description("Tests the backwards compatibility of the RuntimeStatus.Warnings property")]
        public void TestWarningCompatibility()
        {
            TestWarningsBackwardComp.Test.TestWarningBackwardsCompatibility();
        }
    }
}
