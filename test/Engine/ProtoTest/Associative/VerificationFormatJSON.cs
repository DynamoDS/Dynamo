using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class VerificationFormatJSON : ProtoTestBase
    {
        [Test]
        public void TestAssignment01()
        {

            String code = @"a = 1;";
            string verification =
@" 
    {""a"": 1}
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestAssignment02()
        {
            string code =
@"a = 1;b = 2;";
            string verification =
@" 
    {
        ""a"": 1,
        ""b"": 2
    }
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestNullAssignment01()
        {
            string code =
@"a = null;";
            string verification =
@" 
    {
        ""a"": null,
    }
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestNullAssignment02()
        {
            string code =
@"a = null;b = a;";
            string verification =
@" 
    {
        ""a"": null,
        ""b"": null,
    }
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestFunctionCall01()
        {
            string code =
@"def f(){    return = 1;}x = f();";
            string verification =
@" 
    {""x"": 1}
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestDouble01()
        {
            string code =
@"a = 1.0;";
            string verification =
@" 
    {""a"": 1.0}
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestDouble02()
        {
            string code =
@"pi = 3.14;e = 2.71828;";
            string verification =
@" 
    {        ""pi"" : 3.14,        ""e"" : 2.71828,
    }
";
            thisTest.RunAndVerify(code, verification);

        }

        [Test]
        public void TestDouble03()
        {
            string code =
@"a = 1.1;b = 2.2;c = 3.3;";
            string verification =
@" 
    {        ""a"" : 1.1,        ""b"" : 2.2,        ""c"" : 3.3,
    }
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestArrayAssignment01()
        {
            string code =
@"a = [1,2,3];";
            string verification =
@" 
    {""a"": [1,2,3]}
";
            thisTest.RunAndVerify(code, verification);
        }

        [Test]
        public void TestArrayAssignment02()
        {
            string code =
@"i = 2;a = [1, 2, 3 + i];";
            string verification =
@" 
{
    ""i"": 2,
    ""a"": [1,2,5]
}
";
            thisTest.RunAndVerify(code, verification);
        }
      
        [Test]
        public void TestNestedArrayAssignment01()
        {
            string code =
@"a = [1,2,[3,4]];";
            string verification =
@" 
{
    ""a"": [1,2,[3,4]]
}
";
            thisTest.RunAndVerify(code, verification);
        }

    }
}