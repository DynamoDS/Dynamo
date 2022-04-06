using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Associative
{
    class ReplicationGuide : ProtoTestBase
    {
        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
test = foo( [0,1]<1>,[2,3]<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_a()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
f = [0,1];
g = [2,3];
test = foo( f<1>, g<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_2()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
x = 0..1;
y = 2..3;
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_3()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_4()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
test = foo( (0..1)<1>,(2..3)<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_5()
        {
            String code =
@"
def foo(a,b)
{
    return = a + b;
}
test = foo( [0..1]<1>,[2..3]<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { new Object[] { 2, 4 } } }); // extra bracket is known issue for now
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_6()
        {
            String code =
@"
def foo(a:var,b:var)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_7()
        {
            String code =
@"
def foo(a:var,b:var)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y<1> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { 2, 4 });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_8()
        {
            String code =
@"
def foo(a:int,b:double)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2.0, 3.0 }, new Object[] { 3.0, 4.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_9()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo(a:TestObjectA, b:TestObjectA)
{
    return = a.a + b.a;
}
x = TestObjectA.TestObjectA([0,1]);
y = TestObjectA.TestObjectA([2,3]);
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")] // negative testing
        public void T0001_Replication_Guide_Function_With_2_Arg_10()
        {
            String code =
@"
def foo(a:int,b:double)
{
    return = a + b;
}
x = [[0,1],[2,3]];
y = [[0,1],[2,3]];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } };
            // verification : unknown
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_11()
        {
            String code =
@"
def foo(a:int,b:double)
{
    return = a + b;
}
x = [[0,1]];
y = [[2,3]];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { 2.0, 4.0 } };
            thisTest.Verify("test", new Object[] { x1 });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_12()
        {
            String code =
@"
def foo(a:var[],b:var[])
{
    return = a + b;
}
x = [[0,1]];
y = [[2,3]];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            // verification : unknown
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_14()
        {
            String code =
@"
def foo(a,b:int)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_15()
        {
            String code =
@"
def foo(a:int,b:int)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x<1>,y );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            // verification : clarify with new spec
            thisTest.Verify("test", new Object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test] //post R1
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_16()
        {
            String code =
@"
def foo(a:int,b:int)
{
    return = a + b;
}
x = [0,1];
y = [2,3];
test = foo( x,y<1> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            // verification : clarify with new spec
            thisTest.Verify("test", new Object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_17()
        {
            String code =
@"
def foo(a:int,b:int)
{
    return = a + b;
}
x = [0,1];
y = [2,3,4];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3, 4 }, new Object[] { 3, 4, 5 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_18()
        {
            String code =
@"
def foo(a:int,b:int)
{
    return = a + b;
}
x = [0,1,3];
y = [4,5];
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 4, 5 }, new Object[] { 5, 6 }, new Object[] { 7, 8 } });
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_2_Arg_19()
        {
            String code =
@"
def foo(a:int,b:int)
{
    return = a + b;
}
x = [0,1];
y = 4;
test = foo( x<1>,y<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { 4, 5});
        }

        [Test]
        [Category("Replication")]
        public void T0001_Replication_Guide_Function_With_3_Args()
        {
            String code =
                @"
def foo: var[]..[](x : string, y : string, z : string)
{
return = x + y + z;
}
x = [[""1""]];
y = ""2"";
z = [""3"", ""4""];
a = foo(x<1>, y<2>, z<3>);
b = foo(x<1>, y<2>, z<3><4>);
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] {new object[] {new object[] {"123"}, new object[] {"124"}}});
            thisTest.Verify("b", new object[] {new object[] {new object[] {"123"}, new object[] {"124"}}});
        }

        [Test]
        [Category("Replication")]
        public void T0002_Replication_Guide_Function_With_3_Arg()
        {
            String code =
@"
def foo(a:int,b:int,c)
{
    return = a + b +c;
}
x = [0,1];
y = [2,3];
z = [4,5];
test = foo( x<1>,y<2>,z<3> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } }, new Object[] { new Object[] { 7, 8 }, new Object[] { 8, 9 } } };
            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("Replication")]
        public void T0002_Replication_Guide_Function_With_3_Arg_2()
        {
            String code =
@"
def foo(a:int,b:int,c:int)
{
    return = a + b +c;
}
x = [0,1];
y = [2,3];
z = [4,5];
test = foo( x<1>,y<2>,z<1> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { 6, 7 }, new Object[] { 8, 9 } };

            thisTest.Verify("test", x1);
        }

        [Test] // post R1
        [Category("Replication")]
        public void T0002_Replication_Guide_Function_With_3_Arg_3()
        {
            String code =
@"
def foo(a:int,b:int,c:int)
{
    return = a + b +c;
}
x = [0,1];
y = [2,3];
z = [4,5];
test = foo( x<1>,y,z<2> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } }, new Object[] { new Object[] { 7, 8 }, new Object[] { 8, 9 } } };
            // verification : clarify with new spec
            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("Replication")]
        public void T0002_Replication_Guide_Function_With_3_Arg_4()
        {
            String code =
@"
def foo(a,b,c)
{
    return = a + b +c;
}
x = [0,1];
y = [2,3];
z = [4,5];
test = foo( x<1>,y<2>,z<3> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } }, new Object[] { new Object[] { 7, 8 }, new Object[] { 8, 9 } } };
            thisTest.Verify("test", x1);

        }

        [Test]
        [Category("Replication")]
        public void T0002_Replication_Guide_Function_With_3_Arg_5()
        {
            String code =
@"
def foo(a,b,c)
{
    return = a + b +c;
}
x = [0,1];
y = [2,3,4];
z = [5,6,7,8];
test = foo( x<1>,y<2>,z<3> );
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { new Object[] { 7, 8, 9, 10 }, new Object[] { 8, 9, 10, 11 }, new Object[] { 9, 10, 11, 12 } }, new Object[] { new Object[] { 8, 9, 10, 11 }, new Object[] { 9, 10, 11, 12 }, new Object[] { 10, 11, 12, 13 } } };
            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0003_Replication_Guide_Class_Constructor_With_2_Arg_1()
        {
            String code =
@"
import(""FFITarget.dll"");
test = TestObjectC.TestObjectC([0,1]<1>,[2,3]<2>).z;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0003_Replication_Guide_Class_Constructor_With_2_Arg_2()
        {
            String code =
@"
import(""FFITarget.dll"");
test = TestObjectC.TestObjectC((0..1)<1>,(2..3)<2>).z;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0003_Replication_Guide_Class_Constructor_With_2_Arg_3()
        {
            String code =
@"
import(""FFITarget.dll"");
x = [0,1];
y = [2,3];
test = TestObjectC.TestObjectC(x<1>,y<2>).z;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test] //post R1
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0003_Replication_Guide_Class_Constructor_With_2_Arg_4()
        {
            String code =
@"
import(""FFITarget.dll"");
x = [0,1];
y = 2;
test = TestObjectC.TestObjectC(x<1>,y<2>).z;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467459 NotImplemented Exception occurs when replication guides are used on a combination of collection and singleton";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] {  2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0004_Replication_Guide_Class_Constructor_With_3_Arg()
        {
            String code =
@"
import(""FFITarget.dll"");
x = [0,1];
y = [2,3];
z = [4,5];
test = TestObjectD.TestObjectD(x<1>,y<2>,z<3>).t;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } }, new Object[] { new Object[] { 7, 8 }, new Object[] { 8, 9 } } };
            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0004_Replication_Guide_Class_Constructor_With_3_Arg_2()
        {
            String code =
@"
import(""FFITarget.dll"");
x = [0,1];
y = [2,3];
z = [4,5];
test = TestObjectD.TestObjectD(x<1>,y<2>,z<1>).t;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new Object[] { 6, 7 }, new Object[] { 8, 9 } };

            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T0004_Replication_Guide_Class_Constructor_With_3_Arg_3()
        {
            String code =
@"
import(""FFITarget.dll"");
x = [0,1];
y = [2,3];
z = [4,5];
test = TestObjectD.TestObjectD(x<1>,y<2>,z).t;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x1 = new Object[] { new object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } }, new object[] { new object[] { 7, 8 }, new object[] { 8, 9 } } };
            //Vericiation : clarify with new spec
            thisTest.Verify("test", x1);
        }

        [Test]
        [Category("Replication")]
        public void T032_replicationguide_usecase()
        {
            String code =
@"
a = 0..10;
b = a;
b[2] = 100;
c = a;
d = b[0..(Count(b) - 1)..2];
a_singleton = 10;
b_1DArray = 10..100..10;
//c_2D_Array = (10..100..10) < 1 > + (10..100..10) < 2 >;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] a = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Object[] b = { 0, 1, 100, 3, 4, 5, 6, 7, 8, 9, 10 };
            Object[] d = { 0, 100, 4, 6, 8, 10 };
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", a);
            thisTest.Verify("d", d);
            thisTest.Verify("a_singleton", 10);
            thisTest.Verify("b_1DArray", new object[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 });

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T033_Replication_Guide_1467383()
        {
            String code =
@"
import(""FFITarget.dll"");
def ByPoints(pts : DummyPoint2D)
{
    return = pts;
}
p = DummyPoint2D.ByCoordinates((1..2..1)<1>, (3..4..1)<2> );
test = ByPoints(p).X;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 1.0, 1.0 }, new Object[] { 2.0, 2.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T033_Replication_Guide_1467382()
        {
            String code =
                @"
import(""FFITarget.dll"");
def ByStartPointEndPoint(p1:DummyPoint, p2:DummyPoint)
{
    return = p1;
}
height = 5;
p1 = DummyPoint.ByCoordinates((0..1)<1>, (0..1)<2>,1);
p2 = DummyPoint.ByCoordinates((0..1)<1>, (0..1)<2>,height);
l = ByStartPointEndPoint(p1, p2);
test = l.X;                ";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 0.0, 0.0 }, new Object[] { 1.0, 1.0 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments()
        {
            String code =
                @"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = 1..2;
y = 3..4;
z = 1;
test = sum ( z, x<1>, y<2> ); ";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_a()
        {
            String code =
                @"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = 1..2;
y = 3..4;
z = [1, 1];
test = sum ( z<1>, x<1>, y<2> ); ";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_b()
        {
            String code =
                @"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = 1..2;
y = 3..4;
z = 1;
test = sum ( x<1>, y<2>, z ); ";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_c()
        {
            String code =
                @"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = [1,2];
y = [3,4];
z = 1;
test = sum ( z, x<1>, y<2> ); ";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_2()
        {
            String code =
@"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = 1..2;
y = 3..4;
z = 1;
test = sum ( z, x<1>, y<2> );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_3()
        {
            String code =
@"def sum ( a, b, c)
{
    return = a + b + c ;
}
x = 1..2;
y = 3..4;
z = 1;
test = sum ( x<1>, z, y<2> );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            //thisTest.Verify("test", new Object[] { new Object[] { 5, 6 }, new Object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_4()
        {
            String code =
@"def sum ( a, b)
{
    return = a + b  ;
}
x = 1..2;
z = 1;
test = sum ( x<1>, z );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { 2, 3 });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_5()
        {
            String code =
@"def sum ( a, b)
{
    return = a + b  ;
}
x = 1..2;
z = 1;
test = sum ( z, x<1> );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test", new Object[] { 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_6()
        {
            String code =
@"
import(""FFITarget.dll"");
x1 = 1..2;
y1 = 1;
a = DummyPoint2D.ByCoordinates(y1, x1<1>);
test = a.Y;";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { 1, 2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_7()
        {
            String code =
@"
def foo ( a, b)
{
    x = a;
    y = b;
    return = x + y;
}
x1 = 1..2;
y1 = 1;
dummy = foo(y1, x1);
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";//DNL-1467386 Rev 4247 : WARNING: Replication unbox requested on Singleton warning coming from using replication guides on only some, not all arguments of a function gives incorrect output";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("dummy", new Object[] { 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_8()
        {
            String code =
@"
import(""FFITarget.dll"");
a = (0..1..#2);
cs = DummyPoint.ByCoordinates(1, a<1>, a<2>); 
test = cs.Y;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("test", new Object[] { new Object[] { 0.0, 0.0 }, new Object[] { 1.0, 1.0 } });
        }

        [Test]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_9()
        {
            String code =
@"
import(""FFITarget.dll"");
def sum (a, b, c)
{
    return = a + b + c;
}
temp1 = (DummyMath.Sin(0..180..#2) * 2);
temp2 = (DummyMath.Sin(0..180..#3) * 1);
zArray = temp1 < 1 > +temp2 < 2 >;
            zArray1 = zArray + 1;
            ceilingPoints = sum((0..10..#2)<1>, (0..15..#3)<2>, zArray1 );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "MAGN-1707 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("ceilingPoints", new object[] { new object[] { new object[] { new object[] { 1.0, 91.0, 181.0 }, new object[] { 361.0, 451.0, 541.0 } }, new object[] { new object[] { 8.5, 98.5, 188.5 }, new object[] { 368.5, 458.5, 548.5 } }, new object[] { new object[] { 16.0, 106.0, 196.0 }, new object[] { 376.0, 466.0, 556.0 } } }, new object[] { new object[] { new object[] { 11.0, 101.0, 191.0 }, new object[] { 371.0, 461.0, 551.0 } }, new object[] { new object[] { 18.5, 108.5, 198.5 }, new object[] { 378.5, 468.5, 558.5 } }, new object[] { new object[] { 26.0, 116.0, 206.0 }, new object[] { 386.0, 476.0, 566.0 } } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T034_Replication_Guides_Not_On_All_Arguments_8_a()
        {

            //Analysis: The Rep Guides are being resolved to C0C1, rather than C1C2. This needs to
            //have the fix applied for function calls applied to ctors as well.
            String code =
@"
import(""FFITarget.dll"");
a = (0..1..#2);
b = [ 0, 1]; // fails with this as well
cs = DummyPoint.ByCoordinates(1, a<1>, b<2>); 
test = cs.Y;";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("test", new Object[] { new Object[] { 0.0, 0.0 }, new Object[] { 1.0, 1.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T035_Defect_1467317_Replication_Guide_On_Instances()
        {
            String code =
@"
import(""FFITarget.dll"");
a = ArrayMember.Ctor([1,2]);
b = ArrayMember.Ctor([1,2]);
test = a.X<1> + b.X<2>;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T035_Defect_1467317_Replication_Guide_On_Instances_2()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    a = ArrayMember.Ctor([1,2]);
    b = ArrayMember.Ctor([1,2]);
    test = a.X<1> + b.X<2>;
    return = test;
}
test = foo();
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T035_Defect_1467317_Replication_Guide_On_Instances_3()
        {
            String code =
@"
import(""FFITarget.dll"");
test = [ [ ] ];
test2 = [Associative]
{
    test2 = [ ] ;
    [Imperative]
    {
        a = ArrayMember.Ctor([1,2]);
        b = ArrayMember.Ctor([1,2]);
        [Associative]
        {
            test = a.X<1> + b.X<2>;
            test2 = a.X + b.X;
        }
    }
    return = test2;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
            thisTest.Verify("test2", new Object[] { 2, 4 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T036_Defect_1467383_Replication_Guide_On_Collection()
        {
            String code =
@"
import(""FFITarget.dll"");
p = DummyPoint2D.ByCoordinates((1..2..1)<1>, (3..4..1)<2> ).X;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("p", new Object[] { new Object[] { 1.0, 1.0 }, new Object[] { 2.0, 2.0 } });

        }


        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuideAfterArray()
        {
            string code = @"
def sum ( a, b, c)
{
    return = a + b + c ;
}
y = 3..4;
r = sum(1, [ 1, 2]<1>, y<2>);";
            string errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328()
        {
            string code = @"
            xHeight = 2.0;
            yHeight = 1.0;
            zArray1 = ((1..2..1)* xHeight)<1> + ((1..2..1)* yHeight)<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("zArray1", new object[] { new object[] { 3.000000, 4.000000 }, new object[] { 5.000000, 6.000000 } });
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_2()
        {
            string code = @"
            a = 0..10;
            b = a;
            b[2] = 100;
            c = a;  
            e = b[0..(Count(b) - 1)..2]<1>+b[0..(Count(b) - 1)..2]<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e", new object[] { new object[] { 0, 100, 4, 6, 8, 10 }, new object[] { 100, 200, 104, 106, 108, 110 }, new object[] { 4, 104, 8, 10, 12, 14 }, new object[] { 6, 106, 10, 12, 14, 16 }, new object[] { 8, 108, 12, 14, 16, 18 }, new object[] { 10, 110, 14, 16, 18, 20 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_3()
        {
            string code = @"
import(""FFITarget.dll"");
x = (TestObjectA.TestObjectA(1..3))[0];
x = (TestObjectA.TestObjectA(1..3))[0..2].a<1> +(TestObjectA.TestObjectA(1..3))[0..2].a<2> ;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new object[] { new object[] { 2, 3, 4 }, new object[] { 3, 4, 5 }, new object[] { 4, 5, 6 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_4()
        {
            string code = @"
import(""FFITarget.dll"");
            x = (TestObjectA.TestObjectA(1..3))[0];
            x = (TestObjectA.TestObjectA(1..3))[0..2].a<1> +(TestObjectA.TestObjectA(1..3))[0..2].a<2> ;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new object[] { new object[] { 2, 3, 4 }, new object[] { 3, 4, 5 }, new object[] { 4, 5, 6 } });
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_5()
        {
            string code = @"
            def foo : int(a : int,b:int)
            {
                return = a * b;
            }
            list1 = [1,2];
            list2 = [1,2];
            list3 = foo(foo(foo(list1<1>, list2<2>)<1>, list2<2>)<1>, list2<2>);";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("list3", new object[] { new object[] { new object[] { new object[] { 1 }, new object[] { 2 } }, new object[] { new object[] { 2 }, new object[] { 4 } } }  } );
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_6()
        {
            string code = @"
def sum ( a, b)
{ 
    return = a + b;
}
test2 = sum ( (0..1)<1>, (2..3)<2>);";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_7()
        {
            string code = @"
test2 = (1..5..1) < 1 > + (1..5..1) < 2 >;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 2, 3, 4, 5, 6 }, new object[] { 3, 4, 5, 6, 7 }, new object[] { 4, 5, 6, 7, 8 }, new object[] { 5, 6, 7, 8, 9 }, new object[] { 6, 7, 8, 9, 10 } }
);
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_8()
        {
            string code = @"
xHeight = 2.0;
yHeight = 1.0;
test2 = ((1..2..1)* xHeight)<1> + ((1..2..1)* yHeight)<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 3.0, 4.0 }, new object[] { 5.0, 6.0 } }
);
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_9()
        {
            string code = @"
def foo(x:var[])
{
    return = x;
}
test2 = foo([2,3])<1> + ([5,7])<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 7, 9 }, new object[] { 8, 10 } }
);
        }

        [Test]
        [Category("Replication")]
        public void T037_ReplicationGuidebrackets_1467328_10()
        {
            string code = @"
def foo(x:var[])
{
    return = x;
}
x = [1,2,3,4,5,6,7,8,9];
test2 = x[foo([1,2])]<1> + ([5, 7])<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 7, 9 }, new object[] { 8, 10 } }
);
        }

        [Test]
        public void T037_ReplicationGuidebrackets_1467328_11()
        {
            string code = @"
def foo(x:var[])
{
    return = x;
}
test2 = foo([2,3]<1> + ([5,7])<2>);";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 7, 9 }, new object[] { 8, 10 } });
        }

        [Test]
        public void T037_ReplicationGuidebrackets_1467328_12()
        {
            string code = @"
b = 0..5;
t1 = 0..(Count(b) - 1)..2;
test2 = b[t1]<1>+b[t1]<2>;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 0, 2, 4 }, new object[] { 2, 4, 6 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T038_ReplicationGuide_Not_In_Sequence()
        {
            string code =
@"
def foo(x1,y1)
{
    return = x1 + y1;
}
x = [0,1];
y = [2, 3];
test = foo(x<1>,y<3>);
";
            string errmsg = "DNL-1467460 NotImplemented Exception occurs when replication guides are not in sequence";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_1()
        {
            string code =
@"def sum ( a, b, c)
{
    return = a + b + c ;
}
y = 3..4;
test = sum(1, [ 1, 2]<1>, y<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_2()
        {
            string code =
@"
def sum ( a, b, c)
{
    return = a + b + c ;
}
y = 3..4;
test = sum(1, [ 1, 2]<1>, y<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_3()
        {
            string code =
@"
def sum ( a, b, c)
{
    return = a + b + c ;
}
test = [Associative]
{
    y = 3..4;
    return = sum(1, [ 1, 2]<1>, y<2>);
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_4()
        {
            string code =
@"
def sum ( a, b, c)
{
    return = a + b + c ;
}
test = [Associative]
{
    y = 3..4;
    return = sum(1, [ 1, 2]<1>, y<2>);
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_5()
        {
            string code =
@"
def sum ( a, b, c)
{
    return = a + b + c ;
}
test = [Associative]
{
    y = 3..4;
    return = Count ( sum(1, [ 1, 2]<1>, y<2>) ) > 1 ? sum(1, [ 1, 2]<1>, y<2>) : 0;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_6()
        {
            string code =
@"
def foo ( a, b, c)
{
    x = a + b + c ;
    return = x;
}
y = 3..4;
test2 = 0..foo(1, [ 1, 2]<1>, y<2>);
test = test2[1][1];
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        }

        [Test]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_7()
        {
            string code =
@"
def foo ( a, b, c)
{
    x = a + b + c ;
    return = x;
}
y = 3..4;
test = foo(1, (1..2)<1>, y<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_8()
        {
            string code =
@"
def foo ( a, b, c)
{
    x = a + b + c ;
    return = x;
}
y = 3..4;
test = 1 + foo(1, (1..2)<1>, y<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_9()
        {
            string code =
@"
def foo ( a, b, c)
{
    return = a + b + c;
}
test = [Associative]
{
    y = 3..4;
    return = Count ( foo(1, (1..2..1)<1>, y<2>) ) > 1 ? foo(1, (1..2)<1>, y<2>) : 0;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[]{5, 6}, new object[] {6, 7}});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_10()
        {
            string code =
@"
def foo ( a, b, c)
{
    return = a + b + c;
}
test = [Associative]
{
    y = 3..4;
    return = Count (foo(1, (1..2..1)<1>, y<2>) ) > 1 ? foo(1, (1..2..#2)<1>, y<2>) : 0;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] {5, 6}, new object[]{6, 7} } );
        }

        [Test]
        [Category("Replication")]
        public void T039_1467423_replication_guide_on_array_11()
        {
            string code =
@"
test;
[Associative]
{
   test = 1 + (1..2..#2)<2> + [3,4]<2>;
}
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 5, 6 }, new object[] { 6, 7 } });
        }

        [Test]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_1()
        {
            string code =
@"
b = 0..5;
t1 = 0..(Count(b) - 1)..2;
test = b[t1]<1>+b[t1]<2>;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 0, 2, 4 }, new object[] { 2, 4, 6 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_2()
        {
            string code =
@"
b = 0..3;
test = b[0..1]<1>+b[[2,3]]<2>;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_3()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}
b = 0..3;
test = foo ( b[0..1]<1>+b[[2,3]]<2> );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_4()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}
b = 0..3;
test = foo( b[0..1]<1>+b[[2,3]]<2> );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_5()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}

b = [ [ 0, 1], [ 2, 3] ];
test1;
test2;
i=[Imperative]
{
    test1 = foo ( b[0][0..1] );
    test2 = foo ( b[1][[0, 1]] );
    return [test1,test2];
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("i", new[] {new[] {0, 1}, new[] {2, 3}});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        [Category("Failure")]
        public void T040_1467488_replication_guide_on_array_slices_6()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}
b = [ [ 0, 1], [ 2, 3] ];
test1;
test2;
[Associative]
{
    test1 = foo ( b[ [0, 0] ][ 0..1 ] );
    test2 = foo ( b[ [ 1, 1] ][ [0, 1] ] );
}
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { 0, 1 });
            thisTest.Verify("test2", new object[] { 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T040_1467488_replication_guide_on_array_slices_7()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}
b = [ [ 0, 1], [ 2, 3] ];
test = [Associative]
{
    return = foo ( b[ [0, 1]<1> ][( 0..1 )<2>] );
}
";
            string errmsg = "DNL-1467298 rev 4245 :  replication guides with partial array indexing is not giving the expected output";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { 0, 1 }, new object[] { 2, 3 } });

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        [Category("Failure")]
        public void T040_1467488_replication_guide_on_array_slices_8()
        {
            string code =
@"
def foo ( a )
{
    return = a ;
}

b = [ [ 0, 1], [ 2, 3] ];
def foo2 ( )
{
    t = foo ( b[ [0, 1] ][( 0..1 ) ] );
    return = t;
}
test = foo2();
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { 0, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_01()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [0,1];
y = [2, 3];
test = foo(x<1>,y<3>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_02()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<1>,y<3>,z); // expect this to be treated as :  foo(x<1>,y<2>,z<1>);
";
            string errmsg = "MAGN-1707 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { new Object[] { 2, 2 }, new Object[] { 3, 3 } }, new object[] { new object[] { 2, 2 }, new object[] { 3, 3 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_03()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x, y<2>, z<3>); // expect this to be treated as :  foo(x<1>,y<2>,z<3>);
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { new Object[] { 2, 2 }, new Object[] { 2, 2 } }, new object[] { new object[] { 3, 3 }, new object[] { 3, 3 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_04()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<1>, y, z<3>); // expect this to be treated as :  foo(x<1>,y<1>,z<2>);
";
            string errmsg = "MAGN-1707 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { new Object[] { 2, 3 }, new Object[] { 2, 3 } }, new object[] { new object[] { 2, 3 }, new object[] { 2, 3 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_05()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<1>, y<1>, z<3>); // expect this to be treated as :  foo(x<1>,y<1>,z<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new Object[] { 2, 2 }, new Object[] { 3, 3 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_06()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<3>, y<1>, z<5>); // expect this to be treated as :  foo(x<2>,y<1>,z<3>); 
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new object[] { new object[] { new object[] { 2, 2 }, new object[] { 2, 2 } }, new object[] { new object[] { 3, 3 }, new object[] { 3, 3 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_07()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = z1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<4>, y<3>, z<1>); // expect this to be treated as :  foo(x<3>,y<2>,z<1>); 
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] t1 = new Object[] { new Object[] { 4, 4 }, new Object[] { 4, 4 } };
            Object[] t2 = new Object[] { new Object[] { 5, 5 }, new Object[] { 5, 5 } };
            thisTest.Verify("test", new object[] { t1, t2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_08()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<7>, y<3>, z<6>); // expect this to be treated as :  foo(x<3>,y<1>,z<2>); 
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] t1 = new Object[] { new Object[] { 2, 2 }, new Object[] { 2, 2 } };
            Object[] t2 = new Object[] { new Object[] { 3, 3 }, new Object[] { 3, 3 } };
            thisTest.Verify("test", new object[] { t1, t2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T041_1467460_replication_guide_not_in_sequence_09()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<7>, y<3>, z<6>); // expect this to be treated as :  foo(x<3>,y<1>,z<2>); 
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] t1 = new Object[] { new Object[] { 2, 2 }, new Object[] { 2, 2 } };
            Object[] t2 = new Object[] { new Object[] { 3, 3 }, new Object[] { 3, 3 } };
            thisTest.Verify("test", new object[] { t1, t2 });
        }

        [Test]
        public void T041_1467460_replication_guide_not_in_sequence_010()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test = foo(x<7>, y<3>, z<6>); // expect this to be treated as :  foo(x<3>,y<1>,z<2>); 
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] t1 = new Object[] { new Object[] { 2, 2 }, new Object[] { 2, 2 } };
            Object[] t2 = new Object[] { new Object[] { 3, 3 }, new Object[] { 3, 3 } };
            thisTest.Verify("test", new object[] { t1, t2 });
        }

        [Test]
        public void T041_1467460_replication_guide_not_in_sequence_011()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
test=
[Imperative]
{
    x = [0,1];
    y = [2,3];
    z = [4,5 ];
    test = [Associative]
    {
        return = foo(x<7>, y<3>, z<6>); // expect this to be treated as :  foo(x<3>,y<1>,z<2>); 
    }
    return test;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] t1 = new Object[] { new Object[] { 2, 2 }, new Object[] { 2, 2 } };
            Object[] t2 = new Object[] { new Object[] { 3, 3 }, new Object[] { 3, 3 } };
            thisTest.Verify("test", new object[] { t1, t2 });
        }

        [Test]
        public void T041_1467460_replication_guide_not_in_sequence_013()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = y1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test1 = foo(x<2>, y<2>, z<3>) ; // expect this to be treated as :  foo(x<1>,y<1>,z<2>);
test2 = foo(x<1>, y<3>, z<3>) ; // expect this to be treated as :  foo(x<1>,y<2>,z<2>);
            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new Object[] { 2, 2, }, new Object[] { 3, 3 } });
            thisTest.Verify("test2", new object[] { new Object[] { 2, 3, }, new Object[] { 2, 3 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]
        public void T042_1467555_cartesion_product_in_dot_operation_1()
        {
            string code =
@"
import (""FFITarget.dll"");
x = [0,1];
y = [2,3];
z = [4,5];
aa = [ ReplicationTestA.ReplicationTestA(0,0,0), ReplicationTestA.ReplicationTestA(0,0,0) ];
test1 = aa.gety(x<2>, y<2>, z<3>) ; // expect this to be treated as :  foo(x<1>,y<1>,z<2>);
test2 = aa.gety(x<1>, y<3>, z<3>) ; // expect this to be treated as :  foo(x<1>,y<2>,z<2>);
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new Object[] { 2, 2, }, new Object[] { 2, 2 } }, new object[] { new Object[] { 3, 3, }, new Object[] { 3, 3 } } });
            thisTest.Verify("test2", new object[] { new object[] { new Object[] { 2, 2, }, new Object[] { 3, 3 } }, new object[] { new Object[] { 2, 2, }, new Object[] { 3, 3 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0100_FuncCall_Int_AllGuides()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = x1+y1+z1;
}
x = [0,1];
y = [2,3];
z = [4,5 ];
test1 = foo(x<1>, y<2>, z<3>) ; 

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 7, 8 }, new object[] { 8, 9 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0101_FuncCall_Double_SomeGuides()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = x1+y1+z1;
}
x = [0.0,1.0];
y = [2.0,3.0];
z = [4.0,5.0 ];
test1 = foo(x, y<2>, z<3>) ;       
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 6.0, 7.0 }, new object[] { 7.0, 8.0 } }, new object[] { new object[] { 7.0, 8.0 }, new object[] { 8.0, 9.0 } } });
        }

        [Test]
        public void T0102_FuncCall_Double_SomeGuides()
        {
            string code =
@"
import (""FFITarget.dll"");
x = [0.0,1.0];
y = [2.0,3.0];
z = [4.0,5.0 ];
t1 = ReplicationTestA.ReplicationTestA(x, y<2>, z);  
test2 = t1.A;
test = ReplicationTestA.ReplicationTestA(0,0,0);
test3 = test.bar(x, y<2>, z); 
test4 = ReplicationTestA.foo(x, y<2>, z);      
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new object[] { new object[] { 6.0, 8.0 }, new object[] { 7.0, 9.0 } });
            thisTest.Verify("test3", new object[] { new object[] { 6.0, 8.0 }, new object[] { 7.0, 9.0 } });
            thisTest.Verify("test4", new object[] { new object[] { 6.0, 8.0 }, new object[] { 7.0, 9.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0103_FuncCall_Bool_NotInSeq()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = x1 == true ? y1 : z1;
}
x = [true, false];
y = [false, true];
z = [true, true];
test1 = foo(x<2>, y<4>, z<5>) ;          
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { false, false }, new object[] { true, true } }, new object[] { new object[] { true, true }, new object[] { true, true } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0104_FuncCall_Bool_NotInSeq()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = x1 == true ? y1 : z1;
}
x = [true, false];
y = [false, true];
z = [true, true];
test1 = foo(x<4>, y<3>, z<5>) ;   
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { false, false }, new object[] { true, true } }, new object[] { new object[] { true, true }, new object[] { true, true } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0105_FuncCall_Int_NotAllGuides_NotInSeq()
        {
            string code =
@"
def foo (x1,y1,z1)
{
    return = x1 + y1 + z1;
}
x = [0, 1];
y = [2, 3];
z = [4, 5];
test1 = foo(x, y<2>, z<2>) ;       
";
            string errmsg = "DNL-1467580 IndexOutOfRange Exception when replication guides are not applied on all arguments";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { 6, 7 }, new object[] { 8, 9 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0106_FuncCall_Int_MultipleGuides()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<1><2>, y<3><4>) ;          
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0107_FuncCall_Int_MultipleGuides_NotAllInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<1><1>, y<2><3>) ;         
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0108_FuncCall_Int_MultipleGuides_NotAllInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<1><3>, y<4><5>) ;        
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        [Category("Replication")]
        public void T0109_FuncCall_Int_MultipleGuides_NotAllInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<2><4>, y<3><1>) ;            
";
            string errmsg = "MAGN-1708 NotImplemented Exception when multiple non-sequential replication guides are used on multidimensional arrays";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        public void T0110_FuncCall_Int_MultipleGuides_NotAllInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<4><8>, y<7><3>) ;            
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        public void T0111_FuncCall_Int_MultipleGuides_NotAllInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];
test1 = foo(x<3><5>, y<4><1>) ;            
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } }, new object[] { new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } }, new object[] { new object[] { 8, 9 }, new object[] { 9, 10 } } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0112_FuncCall_Int_SingleAndMultipleGuides()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [0,1];
y = [[4,5],[6,7]];
test1 = foo(x<1>, y<2><3>) ;        
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 4, 5 }, new object[] { 6, 7 } }, new object[] { new object[] { 5, 6 }, new object[] { 7, 8 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0113_FuncCall_Int_SingleAndMultipleGuides_NotInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [0,1];
y = [[4,5],[6,7]];
test1 = foo(x, y<2><3>) ;        
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } }, new object[] { new object[] { 6, 7 }, new object[] { 7, 8 } } });
        }

        [Test]
        public void T0114_FuncCall_Int_SingleAndMultipleGuides_NotInSeq()
        {
            string code =
@"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [0,1];
y = [[4,5],[6,7]];
test1 = foo(x<1>, y<1><3>) ;            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { 4, 5 }, new object[] { 7, 8 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0115_FuncCall_HeterogenousInput()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:int,y1:TestObjectA)
{
    return = x1 + y1.a;
}
x = [0,1];
y = [TestObjectA.TestObjectA(2), TestObjectA.TestObjectA(3)];
test1 = foo(x<1>, y<3>) ;            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0116_FuncCall_HeterogenousInput_MultipleGuides()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:double,y1:TestObjectA)
{
    return = x1 + y1.a;
}
x = [[0.0,1.0], [2.0, 3.0] ];
y = [ TestObjectA.TestObjectA(2), TestObjectA.TestObjectA(3)];
test1 = foo(x<1><2>, y<1>) ;            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { 2.0, 3.0 }, new object[] { 5.0, 6.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0117_FuncCall_HeterogenousInput_MultipleGuides()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:double,y1:TestObjectA)
{
    return = x1 + y1.a;
}
x = [[0.0,1.0], [2.0, 3.0] ];
y = [ [TestObjectA.TestObjectA(4), TestObjectA.TestObjectA(5)], [ TestObjectA.TestObjectA(6), TestObjectA.TestObjectA(7) ] ];
test1 = foo(x<1><2>, y<3><4>) ;            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { new object[] { 4.0, 5.0 }, new object[] { 5.0, 6.0 } }, new object[] { new object[] { 6.0, 7.0 }, new object[] { 7.0, 8.0 } } }, new object[] { new object[] { new object[] { 6.0, 7.0 }, new object[] { 7.0, 8.0 } }, new object[] { new object[] { 8.0, 9.0 }, new object[] { 9.0, 10.0 } } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0118_FuncCall_HeterogenousInput_SingleGuides()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:double,y1:TestObjectA)
{
    return = x1 + y1.a;
}
x = [0.0,1.0 ];
y = [ TestObjectA.TestObjectA(2), TestObjectA.TestObjectA(3) ];
test1 = foo(x<1>, y) ;            
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { 2.0, 3.0 }, new object[] { 3.0, 4.0 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0119_FuncCall_HeterogenousInput_SingleGuides()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:double,y1:TestObjectA, z : int)
{
    return = x1 + y1.a + z;
}
x = [ 0.0,1.0 ];
y = [ TestObjectA.TestObjectA(2), TestObjectA.TestObjectA(3) ];
z = [4, 5];
test1 = foo(x<1>, y, z<2>) ;            
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] {
                new object[] { new object[] { 6.0, 7.0 }, new object[] { 7.0, 8.0 } } ,
                new object[] { new object[] { 7.0, 8.0 }, new object[] { 8.0, 9.0 } } } );
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0120_FuncCall_HeterogenousInput_jagged_SingleGuides()
        {
            string code =
@" 
import(""FFITarget.dll"");
def foo (x1:double,y1:TestObjectA, z : int)
{
    return = x1 + y1.a + z;
}
x = [ 0.0 ];
y = [ TestObjectA.TestObjectA(2), TestObjectA.TestObjectA(3) ];
z = [4, 5];
test1 = foo(x<1>, y<2>, z<3>) ;            
";
            // This is just a sample test case of replication guides on jagged array. Replication on jagged array is not yet defined properly
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 6.0, 7.0 }, new object[] { 7.0, 8.0 } } });
        }

        [Test]
        [Category("Replication")]
        [Category("DSDefinedClass_Ported")]
        public void T0121_InstanceCall_Int_SingleGuides()
        {
            string code =
@" 
import (""FFITarget.dll"");
x = [ 1, 2 ];
y = [ 3, 4 ];
z = [ 5, 6 ];
test1 = ReplicationTestA.ReplicationTestA(x<1>, y<2>, z<3>).A ;            
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new object[] { new object[] { new object[] { 9, 10 }, new object[] { 10, 11 } }, new object[] { new object[] { 10, 11 }, new object[] { 11, 12 } } });
        }

        [Test]
        public void T0123_Replication_BuiltinMethods()
        {
            string code =
@" 
    import(""BuiltIn.ds"");
    x = [0,1,2,3];
    y = [0,1];
    z = [ ""int"", ""double"" ];
    test1 = Contains ( x, y);
    test2 = IndexOf ( x, y) ;
    test3 = Remove ( x, y) ;
    test5 = NormalizeDepth ( x, y) ; 
    test6 = List.RemoveIfNot ( x, z) ; 
    test7 = SortIndexByValue ( x, y) ; 
    test8 = Map ( [1,2], [3,4], [2,3]) ;   
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("test1", false);
            thisTest.Verify("test2", -1);
            thisTest.Verify("test3", new Object[] { new Object[] { 1, 2, 3 }, new Object[] { 0, 2, 3 } });
            thisTest.Verify("test5", new Object[] { null, new Object[] { 0, 1, 2, 3 } });
            thisTest.Verify("test6", new Object[] { new Object[] { 0, 1, 2, 3 }, new Object[] { } });
            thisTest.Verify("test7", new Object[] { new Object[] { 3, 2, 1, 0 }, new Object[] { 0, 1, 2, 3 } });
            thisTest.Verify("test8", new Object[] { 0.5, 0.5 });

        }

        [Test]
        public void T0124_ReplicationGuides_BuiltinMethods()
        {
            string code =
@" 
import(""BuiltIn.ds"");
test1;
test2;
test3;
test4;
test5;
test6;
test7;
test8;
[Associative]
{
    x = [[0,1],[2,3]];
    y = [0,1];
    z = [ ""int"", ""double"" ];
    test1 = Contains ( x<1>, y<2>);
    test2 = IndexOf ( x<1>, y<2>) ;
    test3 = Remove ( x<1>, y<2>) ; 
    test4 = Insert ( x<1>, y<2>, y<2>) ;
    test5 = NormalizeDepth ( x<1>, y<2>) ; 
    test6 = List.RemoveIfNot ( x<1>, z<2>) ; 
    test7 = SortIndexByValue ( x<1>, y<2>) ; 
    test8 = Map ( [1,2]<1>, [5,6]<2>, [2,3]<2>) ; 
    
}    
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new Object[] { new Object[] { true, true }, new Object[] { false, false } });
            thisTest.Verify("test2", new Object[] { new Object[] { 0, 1 }, new Object[] { -1, -1 } });
            thisTest.Verify("test3", new Object[] { new Object[] { new Object[] { 1 }, new Object[] { 0 } }, new Object[] { new Object[] { 3 }, new Object[] { 2 } } });
            thisTest.Verify("test4", new Object[] { new Object[] { new Object[] { 0, 0, 1 }, new Object[] { 0, 1, 1 } }, new Object[] { new Object[] { 0, 2, 3 }, new Object[] { 2, 1, 3 } } });
            thisTest.Verify("test5", new Object[] { new Object[] { null, new Object[] { 0, 1 } }, new Object[] { null, new Object[] { 2, 3 } } });
            thisTest.Verify("test6", new Object[] { new Object[] { new Object[] { 0, 1 }, new Object[] { } }, new Object[] { new Object[] { 2, 3 }, new Object[] { } } });
            thisTest.Verify("test7", new Object[] { new Object[] { new Object[] { 1, 0 }, new Object[] { 0, 1 } }, new Object[] { new Object[] { 1, 0 }, new Object[] { 0, 1 } } });
            thisTest.Verify("test8", new Object[] { new Object[] { 0.25, 0.4 }, new Object[] { 0.0, 0.25 } });
        }

        [Test]
        public void T0129_ReplicationGudes_InlineCondition()
        {
            string code =
@" 
def foo1(x,y)
{
    return = x + y;
}
def foo2()
{
    return = [1, 2];
}
def foo3()
{
    return = [3, 4];
}
def foo ( x )
{
    return = x;
}
x = 2;
t1 = foo ( x > 1 ? foo1(foo2()<1>,foo3()<2>) : 0);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { new Object[] { 4, 5 }, new Object[] { 5, 6 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0130_ReplicationGudes_InlineCondition()
        {
            string code =
@" 
def foo1(x,y)
{
    return = x + y;
}
def foo2()
{
    return = [1, 2];
}
def foo3()
{
    return = [3, 4];
}
def foo ( x )
{
    return = x;
}
x = 2;
t1 = foo ( x > 1 ? foo1(foo2()<1>,foo3()<2>) : 0);
";
            string errmsg = "DNL-1467591 replication guides in class instantiation is not giving expected output";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new [] { new object[] {4, 5}, new object[]{5, 6} });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0131_ReplicationGudes_InlineCondition()
        {
            string code =
@" 
def foo1(x,y)
{
    return = x + y;
}
def foo2()
{
    return = [1, 2];
}
def foo3()
{
    return = [3, 4];
}
def fooo ( x )
{
    return = x;
}
def foo ( x )
{
    return = fooo ( x > 1 ? foo1(foo2()<1>,foo3()<2>) : 0);
}
x = 2;
t1 = foo ( x );
";
            string errmsg = "DNL-1467591 replication guides in class instantiation is not giving expected output";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { new object[]{4, 5}, new object[] {5, 6}});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0132_ReplicationGudes_InlineCondition()
        {
            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}
x = 2;
t1 = x > 2 ? 0 : foo([1,2]<1>,[3,4]<5>, [5,6]<3>);
t2 = x > 2 ? 0 : foo([1,2]<1>,[3,4]<3>, x);
t3 = x > 2 ? 0 : foo([1,2]<1>,[3,4]<5>, [5,6]<3>);
t4 = [Associative]
{
     return = [Imperative]
     {
          return = [Associative]
          {
               return = x > 2 ? 0 : foo([1,2]<1>,[3,4]<3>, x);
          }
     }
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { new Object[] { new Object[] { 9, 10 }, new Object[] { 10, 11 } }, new Object[] { new Object[] { 10, 11 }, new Object[] { 11, 12 } } });
            thisTest.Verify("t2", new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } });
            thisTest.Verify("t3", new Object[] { new Object[] { new Object[] { 9, 10 }, new Object[] { 10, 11 } }, new Object[] { new Object[] { 10, 11 }, new Object[] { 11, 12 } } });
            thisTest.Verify("t4", new Object[] { new Object[] { 6, 7 }, new Object[] { 7, 8 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0133_ReplicationGudes_RangeExpr()
        {
            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}
x = 2;
t1 = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));
t2 = Count(foo([1,2]<1>,[3,4]<3>, x))..4..#3;
t3 = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));
t4 = Count(foo([1,2]<1>,[3,4]<3>, x))..4..#3;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, 1, 2 });
            thisTest.Verify("t2", new Object[] { 2, 3, 4 });
            thisTest.Verify("t3", new Object[] { 0, 1, 2 });
            thisTest.Verify("t4", new Object[] { 2, 3, 4 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0134_ReplicationGudes_RangeExpr()
        {
            string code =
@" 
def foo()
{
    return = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));
}
    
def foo (x,y,z)
{
    return = x + y + z;
}
def foo2 ()
{
    return = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));
}
x = 2;
t1 = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));
t2 = foo();
t3 = foo2();
t4 = [Associative]
{
    return = [Imperative]
    {
         return = [Associative]
         {
              return = 0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>));              
         }
    }
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, 1, 2 });
            thisTest.Verify("t2", new Object[] { 0, 1, 2 });
            thisTest.Verify("t3", new Object[] { 0, 1, 2 });
            thisTest.Verify("t4", new Object[] { 0, 1, 2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T0135_ReplicationGudes_ArraySlicingScope()
        {
            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}
def foo2 : int[] (a:var[]..[])
{
    return = a[0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>))];
}
def foo: int[] (a:var[]..[])
{
    return = a[0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>))];
}
a = [1,2,3,4];
c = foo2(a);
d = foo(a);
b;
f = [Associative]
{
    b = a[0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>))];
    return = [Imperative]
    {
         return = [Associative]
         {
              return = a[0..Count(foo([1,2]<1>,[3,4]<5>, 5))];              
         }
    }
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new Object[] { 1, 2, 3 });
            thisTest.Verify("c", new Object[] { 1, 2, 3 });
            thisTest.Verify("d", new Object[] { 1, 2, 3 });
            thisTest.Verify("f", new Object[] { 1, 2, 3 });
        }

        [Test]
        [Category("Failure")]
        public void T0136_ReplicationGudes_ArraySlicingScope()
        {
            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}
a = [[1],[2,3],[4,5,6]];
b = a[0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>))][0..Count(foo([1,2]<1>,[3,4]<5>, [5,6]<3>))];
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new Object[] { 1, 3, 6 });
        }

        [Test]
        public void T0137_ReplicationGudes_RelationalOperators()
        {
            string code =
@" 
a = 0..1;
b = 2..3;
c1 = a<1> > b<2> ? 1 : 0;
c2 = a<1> <= b<2>;
c3 = a<1> == b<2> ? 1 : 0;
c4 = a<1> != b<2> ? 1 : 0;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c1", new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } });
            thisTest.Verify("c2", new Object[] { new Object[] { true, true }, new Object[] { true, true } });
            thisTest.Verify("c3", new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } });
            thisTest.Verify("c4", new Object[] { new Object[] { 1, 1 }, new Object[] { 1, 1 } });
        }

        [Test]
        public void T0138_ReplicationGudes_LogicalOperators()
        {
            string code =
@" 
a1 = [ true, false];
b1 = [false, false];
c1 = a1<1> && b1<2>;
c2 = (!a1)<1> || (!b1)<1>;
//c3 = (~a1)<1> && (~b1)<1>;
//c4 = {0,0}<1> | {1,1}<2>;
//c5 = {0,1}<1> & {0,1}<2>;
//c6 = {1,2}<1> ^ {1,2}<2>;
";
            string errmsg = "DNL-1467593 Support for some logical operators like | & ^ ~ missing"; // because of this defect, the above lines are commented out

            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c1", new Object[] { new Object[] { false, false }, new Object[] { false, false } });
            thisTest.Verify("c2", new Object[] { true, true });
        }

        [Test]
        public void T0139_ReplicationGudes_MathematicalOperators()
        {
            string code =
@"
def foo (x,y,z)
{
    return = x + y + z;
}
a = [[0,1],[2,3]]; 
b = [[4,5],[6,7]];
c1 = a<1><2> + b<1><2>;
c2 = a<1><2> - b<3><4>;
c3 = a<1> / b<1>;
c4 = a<1> * b<2>;
c5 = a<1> % b<1>;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c1", new Object[] { new Object[] { 4, 6 }, new Object[] { 8, 10 } });
            thisTest.Verify("c2", new Object[] { new Object[] { new Object[] { new Object[] { -4, -5 }, new Object[] { -3, -4 } }, new Object[] { new Object[] { -6, -7 }, new Object[] { -5, -6 } } }, new Object[] { new Object[] { new Object[] { -2, -3 }, new Object[] { -1, -2 } }, new Object[] { new Object[] { -4, -5 }, new Object[] { -3, -4 } } } });
            thisTest.Verify("c3", new Object[] { new Object[] { 0.0, 0.2 }, new Object[] { 0.33333333333333331, 0.42857142857142855 } });
            thisTest.Verify("c4", new Object[] { new Object[] { new Object[] { 0, 5 }, new Object[] { 0, 7 } }, new Object[] { new Object[] { 8, 15 }, new Object[] { 12, 21 } } });
            thisTest.Verify("c5", new Object[] { new Object[] { 0, 1 }, new Object[] { 2, 3 } });
        }

        [Test]
        public void T0140_ReplicationGudes_StringConcat()
        {
            string code =
@"
a = [ ""1"", ""2""];
b = [""3"", ""4""];
c = a<1> + b<2>;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", new Object[] { new Object[] { "13", "14" }, new Object[] { "23", "24" } });
        }

        [Test]
        public void T0141_ReplicationGudes_LogicalOperators()
        {
            string code =
@"
def foo (x,y,z)
{
    return = x + y + z;
}
a = [[0,1],[2,3]]; 
b = [[4,5],[6,7]];
c = a<1><2> > b<1><2> ? 1 : 0;
d = a > b ? 1 : 0;
f = a<1><2> > b<3><4> ? 1 : 0;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } });
            thisTest.Verify("d", new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } });
            thisTest.Verify("f", new Object[] { new Object[] { new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } }, new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } } }, new Object[] { new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } }, new Object[] { new Object[] { 0, 0 }, new Object[] { 0, 0 } } } });
        }

        [Test]
        [Category("Replication")]
        [Category("DSDefinedClass_Ported")]
        public void T0142_ReplicationGudes_On_Both_Instance_And_Method_Call()
        {
            string code =
@"
import (""FFITarget.dll"");
x = 0..1;
y = 2..3;
b1 = ReplicationY.ReplicationY(x, y);
t2 = b1.sum(x, y);
t3 = b1<1>.sum(x<2>, y<2>);
t4 = b1<1>.sum(x<1>, y<1>);
t5 = b1.foo(x);
t6 = b1<1>.foo(x<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t2", new Object[] { 2, 4 });
            thisTest.Verify("t3", new Object[] { new Object[] { 2, 4 }, new Object[] { 2, 4 } });
            thisTest.Verify("t4", new Object[] { 2, 4 });
            thisTest.Verify("t5", new Object[] { 0, 1 });
            thisTest.Verify("t6", new Object[] { new Object[] { 0, 1 }, new Object[] { 0, 1 } });
        }

        [Test]
        [Category("Replication")]
        [Category("DSDefinedClass_Ported")]
        public void T0143_ReplicationGudes_On_Both_Instance_And_Method_Call()
        {
            string code =
@"
import (""FFITarget.dll"");
x = 0..1;
y = 2..3;
b1 = ReplicationY.ReplicationY(x, y);
t2 = b1.sum(0..1, 2..3);
t3 = b1.sum((0..1)<1>, (2..3)<2>);
t4 = b1.sum([0,1]<1>, [2,3]<2>);
t5 = b1<1>.sum([0,1]<1>, [2,3]<2>);
t6 = b1<1>.sum((0..1)<1>, (2..3)<2>);
t7 = b1<1>.sum((0..1)<2>, (2..3)<2>);
t8 = b1<1>.sum([0,1]<1>, [2,3]<1>);
";
            string errmsg = "MAGN-4113[Design] - spec for rep guides when skip a guide";
            thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("t2", new Object[] { 2, 4 });
            thisTest.Verify("t3", new Object[] { new object[] { new Object[] { 2, 2 }, new Object[] { 3, 3 } }, new object[] { new Object[] { 3, 3 }, new Object[] { 4, 4 } } });
            thisTest.Verify("t4", new Object[] { new object[] { new Object[] { 2, 2 }, new Object[] { 3, 3 } }, new object[] { new Object[] { 3, 3 }, new Object[] { 4, 4 } } });
            thisTest.Verify("t5", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
            thisTest.Verify("t6", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
            thisTest.Verify("t7", new Object[] { new Object[] { 2, 4 }, new Object[] { 2, 4 } });
            thisTest.Verify("t8", new Object[] { 2, 4 });
        }
 
    
        [Test]
        public void TO144_ReplicationGuidesForceArrayPromotionShortest()
        {

            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}

t1 = foo((0..1)<1>, (0..2)<1>, (0..2)<1>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, 3 });

        }

        [Test]
        public void TO144_ReplicationGuidesForceArrayPromotionShortestSingleton()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = 0;
b = 0..1;

t1 = foo(a<1>, b<2>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] {0, 1});

        }

        [Test]
        public void TO144_ReplicationGuidesForceArrayPromotionShortestSingletonManual()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = [0];
b = 0..1;

t1 = foo(a<1>, b<2>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[]
                {
                    new Object[] { 0, 1}
                });
        }

        [Test]
        public void TO144_ReplicationGuidesForceArrayPromotionShortestSingletonManual2()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = [0];
b = 0..1;

t1 = foo(a<1>, b<1>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0 });
        }


        [Test]
        public void TO145_ReplicationGuidesLongest()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = 0;
b = 0..1;

t1 = foo(a<1L>, b<1L>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, 1 });
        }

        [Test]
        public void TO145_ReplicationGuidesLongest2()
        {

            string code =
@" 
def foo (x,y,z)
{
    return = x + y + z;
}

a = 0;
b = 0..1;
c = 0..2;

t1 = foo(a<1L>, b<1L>, c<1L>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, 2, 3  });
        }

        [Test]
        public void TO146_ReplicationGuidesCartesianPromoteAllSingles()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = 1;
b = 2;

t1 = foo(a<1>, b<2>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", 3);
        }

        [Test]
        public void TO146_ReplicationGuidesCartesianPromoteAllSingles_Shortest()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = 1;
b = 2;

t1 = foo(a<1>, b<1>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", 3);
        }

        [Test]
        public void TO146_ReplicationGuidesCartesianPromoteAllSingles_Longest()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = 1;
b = 2;

t1 = foo(a<1L>, b<1L>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", 3);
        }

        [Test]
        public void TO147_ReplicationGuidesCartesian()
        {

            string code =
@" 
def foo (x,y)
{
    return = x + y;
}

a = [ 1 ];
b = [ 2 ];

t1 = foo(a<1>, b<2>);

";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[]
                {  new Object[] {
                    3
                }
                });
        }

        [Test]
        public void RegressMagn4853_1()
        {
            string code =
            @" 
def foo(x)
{
}

x = foo(""xyz"");
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);

            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RegressMagn4853_2()
        {
            // Test replication on singleton
            string code =
            @" 
import(""FFITarget.dll"");
t = TestObjectA.TestObjectA(1);
r1 = t.Set(1);
r2 = t<1>.Set(1);
r3 = t<1L>.Set(1);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RegressMagn4853_3()
        {
            // Test replication on singleton 
            string code =
            @" 
import(""FFITarget.dll"");
t = TestObjectA.TestObjectA(1);
v = 42;
r1 = t.Set(v);
r2 = t<1>.Set(v<1>);
r3 = t<1L>.Set(v<1L>);
r4 = t<1>.Set(v<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RegressMagn4853_4()
        {
            // Test replication on LHS
            string code =
            @" 
import(""FFITarget.dll"");
ts = [TestObjectA.TestObjectA(), TestObjectA.TestObjectA()];
r1 = ts.Set(1);
r2 = ts<1>.Set(1);
r3 = ts<1L>.Set(1);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RegressMagn4853_5()
        {
            // Test replication on LHS
            string code =
            @" 
import(""FFITarget.dll"");
ts = [TestObjectA.TestObjectA(), TestObjectA.TestObjectA()];
vs = [42, 43];
r1 = ts.Set(vs);
r2 = ts<1>.Set(vs<1>);
r3 = ts<1L>.Set(vs<1L>);
r4 = ts<1>.Set(vs<2>);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RegressMagn4853_6()
        {
            // Test replication on LHS
            string code =
            @" 
import(""FFITarget.dll"");
t = TestObjectA.TestObjectA(42);
r1 = t.a;
ts = [TestObjectA.TestObjectA(42), TestObjectA.TestObjectA(42)];
r2 = ts.a;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            // Should get clear after running
            Assert.AreEqual(0, thisTest.GetTestRuntimeCore().ReplicationGuides.Count);
        }

        [Test]
        [Category("ReplicationGuide")]
        public void ZachExample()
        {
            string code = @"
def firstItem(list: var[]..[])
{
    return=list[0];
}

xs = [[[1, 2, 3], [4, 5, 6]],[[7, 8, 9], [10, 11, 12]]];
result = firstItem(xs<1><1>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("result", new object[] { new object[] { 1, 4 }, new object[] { 7, 10 } });
        }

        [Test]
        [Category("Replication")]
        public void TestZipFirstReplication()
        {
            string code = @"
def foo(x:var, y:var)
{
    return = x + y;
}
xs = [[""a"", ""b""], [""c"", ""d""]];
ys = [1, 2];
zs = foo(xs, ys);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("zs", new object[] { new object[] { "a1", "b1" }, new object[] { "c2", "d2" } });
        }

        [Test]
        [Category("Replication")]
        public void RegressMAGN1708()
        {
            string code = @"
def foo (x1,y1)
{
    return = x1 + y1;
}
x = [[0, 1],[2,3]];
y = [[4,5],[6,7]];

rs = foo(x<2><4>, y<3><1>);";  
            // it is equivalent to foo(x<1><2>, y<2><1>) =  
            // {{foo({0, 1}<2>, {4, 5}<1>), foo({0, 1}<2>, {6, 7}<2>}, {foo({2, 3}<2>, {4, 5}<1>), foo({2, 3}<2>, {6, 7}<2>}} = 
            // {{{{4, 5}, {5, 6}}, {{6, 7}, {7, 8}}}, {{{6, 7}, {7, 8}}, {{8, 9}, {9, 10}}}}
            thisTest.RunScriptSource(code);
            thisTest.Verify("rs", new object[] 
            { 
                new object[]
                {
                    new object[] { new object[] {4, 5}, new object[] {5, 6}},
                    new object[] { new object[] {6, 7}, new object[] {7, 8}},
                },
                new object[]
                {
                    new object[] { new object[] {6, 7}, new object[] {7, 8}},
                    new object[] { new object[] {8, 9}, new object[] {9, 10}}
                }
            });
        }

        [Test]
        [Category("Replication")]
        public void RegressMAGN1556_01()
        {
            string code = @"
import (""FFITarget.dll"");
zero = 0;
            Var2 = [ 0, 1 ];
            Var10 = DummyPoint.ByCoordinates(Var2 < 1 >, Var2 < 2 >, zero < 1 >); // if 'zero' does not have any replications guide, then am getting the expected value
            rs = Var10.Z;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("rs", new object[] { new object[] { 0.0, 0.0 } } );
        }

        [Test]
        [Category("Replication")]
        public void RegressMAGN1556_02()
        {
            string code = @"
import (""FFITarget.dll"");
zero = 0;
            Var2 = [ 0, 1 ];
            Var10 = DummyPoint.ByCoordinates(Var2 < 1L >, Var2 < 2 >, zero < 1 >); // if 'zero' does not have any replications guide, then am getting the expected value
            rs = Var10.Z;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("rs", new object[] { new object[] { 0.0, 0.0 }, new object[] { 0.0, 0.0 } }) ;
        }

        [Test]
        [Category("Replication")]
        public void RegressMAGN1705()
        {
            string code = @"
val = [ 0.5 , 0.5 ];
def foo( x : var, y : var ) { return = (x+y)/2.0 ; }
answer=foo([ val ][0]<1> , [ val ][0]<2>)[0]; // expected : { 0.5, 0.5 }
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("answer", new object[] { 0.5, 0.5 });
        }

        [Test]
        [Category("Replication")]
        public void TestReplicationGuideTakesPriotry()
        {
            // Verify it always replicates on the first parameter
            string code = @"
def foo(xs:var[], ys:var, zs:var)
{
    return = Sum(xs) + ys + zs;
};
xs = [1, 2, 3];
ys = ""-foo"";
zs = ""-bar"";
r = foo(xs<1>, ys, zs);
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { "1-foo-bar", "2-foo-bar", "3-foo-bar" });
        }

        [Test]
        [Category("Replication")]
        public void TestReplicationGuideTakesPriotry2()
        {
            // Verify it always replicates on the first parameter
            string code = @"
def foo(xs:var[], ys:var, zs:var)
{
    return = Sum(xs) + ys + zs;
};
xs = [1, 2, 3];
ys = ""-foo"";
zs = ""-bar"";
r = foo(xs<1L>, ys<1L>, zs<1L>);
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { "1-foo-bar", "2-foo-bar", "3-foo-bar" });
        }
    }
}
