using System;
using NUnit.Framework;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class TextFxTests : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("TextFx Tests")]
        public void BasicRunAndVerify()
        {
            String code =
@"	fx = 123;	fy = 345;	x = fx;	y = fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 123);
            thisTest.Verify("y", 345);
        }
        [Test]
        [Category("TextFx Tests")]
        public void NullVerify()
        {
            String code =
@"	a = null;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            //Null is not the same as anything else
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", 0.0));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", 0));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", -1));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", -1.0));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", false));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", true));
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => thisTest.Verify("a", ""));
        }
    }
}
