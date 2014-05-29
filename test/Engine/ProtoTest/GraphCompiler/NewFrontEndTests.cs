using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProtoTest.GraphCompiler
{
    [TestFixture]
    public class NewFrontEndTests
    {
        [Test]
        public void ReproMAGN3603()
        {
            List<String> outStrings;
            GraphToDSCompiler.GraphUtilities.CompileExpression(
@"a = 1 + (2 * 3);
b = (1 + 2) * 3;
c = 1 + 2 * 3;
", out  outStrings);


            Assert.IsTrue(outStrings[0].Trim() == "a = 1 + (2 * 3);");
            Assert.IsTrue(outStrings[1].Trim() == "b = (1 + 2) * 3;");
            Assert.IsTrue(outStrings[2].Trim() == "c = 1 + (2 * 3);");



        }


    }
}
