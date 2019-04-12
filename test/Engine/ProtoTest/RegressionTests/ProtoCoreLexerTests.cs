using System;
using NUnit.Framework;
namespace ProtoTest.RegressionTests
{
    [TestFixture]
    class LexerRegressionTests : ProtoTestBase
    {
        [Test]
        public void PreClarifyPreParseBracket001()
        {
            String code =
@"x;
[Associative]
{
 a = [1001,1002];    
 // This is failing the pre-parse. 
 // Probably has somthing to do with convertingthe language blocks into binary exprs
     x = a[0];  
}";

            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1001);
        }
    }
}
