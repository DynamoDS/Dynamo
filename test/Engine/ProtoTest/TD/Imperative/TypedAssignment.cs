using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Imperative
{
    class TypedAssignment : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T01_TestVariousTypes()
        {

            string code = @"
def Create ( a : int)
{
	x : int  = 2;
	return = x + a ;
}
	
def foo : int ( )
{
	x : int = 2;
	t3 = x + i;
    return  = t3;		
}
	
def foo : int ( a : int )
{
    x : int = 2;
	return = x + a ;
}

i = [Imperative]
{
    i1 : int = 5;
    d1 : double = 5.2;
    isTrue1 : bool = true;
    isFalse1 :bool = false;
	x1 = foo(1);
	a1 = Create(1);
	b1 = foo(a1);
	x11:int = 2.3;
	y1:double = 2;
    return [i1, d1, isTrue1, isFalse1, x1, a1, b1, x11, y1];
}
a = [Associative]
{
    i2 : int = 5;
    d2 : double = 5.2;
    isTrue2 : bool = true;
    isFalse2 :bool = false;
	x2 = foo(1);
	a2 = Create(1);
	b2 = foo(a2);
	x12:int = 2.3;
	y2:double = 2;
    return [i2, d2, isTrue2, isFalse2, x2, a2, b2, x12, y2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] {5, 5.2, true, false, 3, 3, 5, 2, 2.0});

            thisTest.Verify("a", new object[] {5, 5.2, true, false, 3, 3, 5, 2, 2.0});
        }
    }
}
