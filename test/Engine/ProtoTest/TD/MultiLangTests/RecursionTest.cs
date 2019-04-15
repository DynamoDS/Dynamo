using System;
using NUnit.Framework;
namespace ProtoTest.TD.MultiLangTests
{

    class RecursionTest : ProtoTestBase
    {
        public void RecursionImperative01()
        {
            String code =
@"
def f(x)
{
    return = [Imperative]
    {
        if(x <= 1)
        {
            return = 1;
        }
        else
        {
            return = f(x - 1) + x;   
        }
    }
}

a = f(3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6);
        }
        

        public void RecursionNestedLanguageBlockTest01()
        {
            String code =
@"
def f(x)
{
    return = [Imperative]
    {
        if(x <= 1)
        {
            return = [Associative]
            {
                return = 1;
            }
        }
        else
        {
            return = [Associative]
            {
                return = f(x - 1) + x;   
            }
        }
    }
}

a = f(3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6);
        }

        [Test]
        public void Test()
        {
            String code =
@"
c = 3.0..5.0;//3.0,4.0,5.0
";
            //Assert.Fail("StackOverflow");
            thisTest.RunScriptSource(code);

        }
    }
}
