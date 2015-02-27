using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class TextFxTests : ProtoTestBase
    {
        [Test]
        [Category("TextFx Tests")]
        public void BasicRunAndVerify()
        {
            String code =
@"	class f	{		fx : var;		fy : var;		constructor f()		{			fx = 123;			fy = 345;		}	}// Construct class 'f'	cf = f.f();	x = cf.fx;	y = cf.fy;";
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
