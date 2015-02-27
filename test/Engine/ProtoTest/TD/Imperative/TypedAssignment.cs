using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class TypedAssignment : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_TestVariousTypes()
        {

            string code = @"
class A
{
    i : int;
	constructor A ( a : int)
	{
	     x : int  = 2;
		 i = x + a ;
	}
	
	def foo : int ( )
	{
	    x : int = 2;
		t3 = x + i;
        return  = t3;		
	}
	
}
def foo : int ( a : int )
{
    x : int = 2;
	return = x + a ;
}
i1;i2;
d1;d2;
isTrue1;isTrue2;
isFalse1;isFalse2;
x1;x2;
x11;x12;
b1;b2;
y1;y2;
[Imperative]
{
    i1 : int = 5;
    d1 : double = 5.2;
    isTrue1 : bool = true;
    isFalse1 :bool = false;
	x1 = foo(1);
	a1 = A.A(1);
	b1 = a1.foo();
	x11:int = 2.3;
	y1:double = 2;
    
}
[Associative]
{
    i2 : int = 5;
    d2 : double = 5.2;
    isTrue2 : bool = true;
    isFalse2 :bool = false;
	x2 = foo(1);
	a2 = A.A(1);
	b2 = a2.foo();
	x12:int = 2.3;
	y2:double = 2;
    
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i1", 5);
            thisTest.Verify("d1", 5.2);
            thisTest.Verify("isTrue1", true);
            thisTest.Verify("isFalse1", false);
            thisTest.Verify("x1", 3);
            thisTest.Verify("b1", 5);
            thisTest.Verify("x11", 2);
            thisTest.Verify("y1", 2.0);
            thisTest.Verify("i2", 5);
            thisTest.Verify("d2", 5.2);
            thisTest.Verify("isTrue2", true);
            thisTest.Verify("isFalse2", false);
            thisTest.Verify("x2", 3);
            thisTest.Verify("b2", 5);
            thisTest.Verify("x12", 2);
            thisTest.Verify("y2", 2.0);
        }
    }
}
