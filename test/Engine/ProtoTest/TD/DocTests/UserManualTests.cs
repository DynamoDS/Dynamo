using NUnit.Framework;
namespace ProtoTest.TD.DocTests
{
    class UserManualTests : ProtoTestBase
    {
        [Test]
        public void UM01_Print()
        {
            string code =
@"
quote = ""Less is bore."";
s = Print(quote + "" "" + quote + "" "" + quote);
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM19_Range()
        {
            string code =
@"
a = 10;
b = 1..6;
s = Print(a);
s = Print(b);
x = b[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4);
        }

        [Test]
        public void UM21_Range()
        {
            string code =
@"
a = 0..1..0.1;
s = Print(a);
x = a[5];
y = a[7];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.5);
            thisTest.Verify("y", 0.7);
        }

        [Test]
        public void UM22_Range()
        {
            string code =
@"
a = 0..7..0.75;
s = Print(a);
x = a[3];
y = a[5];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.25);
            thisTest.Verify("y", 3.75);
        }

        [Test]
        public void UM23_Range()
        {
            string code =
@"
// DesignScript will increment by 0.777 not 0.75
a = 0..7..~0.75;
s = Print(a);
x = a[3];
y = a[5];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.3333333333333335);
            thisTest.Verify("y", 3.8888888888888893);
        }

        [Test]
        public void UM24_Range()
        {
            string code =
@"
// Interpolate between 0 and 7 such that
// “a” will contain 9 elements
a = 0..7..#9;
s = Print(a);
x = a[6];
y = a[8];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5.25);
            thisTest.Verify("y", 7.0);
        }

        [Test]
        public void UM27_Collection()
        {
            string code =
@"
// generate a collection of numbers
a = 0..6;
// change several of the elements of a collection
a[2] = 100;
a[5] = 200;
s = Print(a);
x = a[2];
y = a[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 100);
            thisTest.Verify("y", 3);
        }

        [Test]
        public void UM28_Collection()
        {
            string code =
@"
// create a collection explicitly
a = [ 45, 67, 22 ];
// create an empty collection
b = [];
// change several of the elements of a collection
b[0] = 45;
b[1] = 67;
b[2] = 22;
s = Print(a);
s = Print(b);
x = a[1];
y = b[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 67);
            thisTest.Verify("y", 67);
        }

        [Test]
        public void UM29_Collection()
        {
            string code =
@"
a = 5..20;
indices = [1, 3, 5, 7];
// create a collection via a collection of indices
b = a[indices];
s = Print(a);
s = Print(b);
x = b[2];
y = b[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10);
            thisTest.Verify("y", 12);
        }

        [Test]
        public void UM30_NumberTypes()
        {
            string code =
@"
// create an integer with value exactly 1
i = 1;
// create a floating point number with approximate
// value 1
f = 1.0;
s = Print(i);
s = Print(f);
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM31_NumberTypes()
        {
            string code =
@"
// create a floating point number approximately 1.0
f1 = 1.0;
// attempt to create a floating point number beyond the
// precision of DesignScript. This number will be
// rounded. f1 and f2 become the same number
f2 = 1.000000000000000000001;
s = Print(f1);
s = Print(f2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f1", 1.0);
            thisTest.Verify("f2", 1.0);
        }

        [Test]
        public void UM33_NumberTypes()
        {
            string code =
@"
// create two seemingly different numbers
a = 1.0;
b = 0.99999;
// test if the two numbers are equal
x = a == b ? 1 : 0;

c = 1.0;
d = 0.9999;
// test if the two numbers are equal
y = c == d ? 1 : 0;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 0);
        }

        [Test]
        public void UM34_NumberTypes()
        {
            string code =
@"
// create two seemingly different numbers
a = 100000000;
b = 99999999;
// test if the two numbers are equal
s = Print(a == b ? ""Are equal"" : ""Are not equal"");
x = a == b ? 1 : 0;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0);
        }

        [Test]
        public void UM38_Functions()
        {
            string code =
@"
def getTimesTwo(arg)
{
return = arg * 2;
}
times_two = getTimesTwo(10);
s = Print(times_two);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("times_two", 20);
        }

        [Test]
        public void UM39_Functions()
        {
            string code =
@"
def getGoldenRatio()
{
return = 1.61803399;
}
gr = getGoldenRatio();
s = Print(gr);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("gr", 1.618034);
        }

        [Test]
        public void UM40_Functions()
        {
            string code =
@"
def returnTwoNumbers()
{
return = [1, 2];
}
two_nums = returnTwoNumbers();
x = two_nums[0];
y = two_nums[1];
s = Print(two_nums);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void UM44_Math()
        {
            string code =
@"
s = Print(7 % 2);
s = Print(6 % 2);
s = Print(10 % 3);
s = Print(19 % 7);
x = 7 % 2;
y = 19 % 7;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 5);
        }

        [Test]
        public void UM61_Boolean()
        {
            string code =
@"
result = 10 < 30;
s = Print(result);
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM62_Boolean()
        {
            string code =
@"
result = 15 <= 15;
s = Print(result);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        public void UM63_Boolean()
        {
            string code =
@"
result = 99 != 99;
s = Print(result);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", false);
        }

        [Test]
        public void UM64_Boolean()
        {
            string code =
@"
result = true && false;
s = Print(result);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", false);
        }

        [Test]
        public void UM65_Boolean()
        {
            string code =
@"
result = true || false;
s = Print(result);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        public void UM66_Boolean()
        {
            string code =
@"
result = !false;
s = Print(result);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", true);
        }

        [Test]
        public void UM69_Looping()
        {
            string code =
@"
geometry = [Imperative]
{
collection = 0..10;
for (i in collection)
{
s = Print(i);
}
}
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void UM75_JaggedCollection()
        {
            string code =
@"
j = [];
j[0] = 1;
j[1] = [2, 3, 4];
j[2] = 5;
j[3] = [ [6, 7], [ [8] ] ];
j[4] = 9;
s = Print(j);
x = j[1][1];
y = j[3][1][0][0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 8);

        }

        [Test]
        public void UM77_JaggedCollection()
        {
            string code =
@"
// generate a jagged collection
j = [1, [2, 3, 4], 5, [[6, 7], [[8]]], 9];
s = Print(j);
s = Print( j[0] );
s = Print( j[1][0] );
s = Print( j[1][1] );
s = Print( j[1][2] );
s = Print( j[2] );
s = Print( j[3][0][0] );
s = Print( j[3][0][1] );
s = Print( j[3][1][0][0] );
s = Print( j[4] );
x = j[1][2];
y = j[3][1][0][0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4);
            thisTest.Verify("y", 8);
        }
    }
}
