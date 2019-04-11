using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.MultiLangTests
{
    class CollectionAssgnmt : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void Collection_Assignment_1()
        {
            string code = @"
i = [Imperative]
{
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
	e = b[0];
    return [c, d, e];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };
            thisTest.Verify("i", new object[] { -2, expectedResult2, expectedResult3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void Collection_Assignment_2()
        {
            string code = @"
def foo: int[]( a: int,b: int )
{
	return [ a,b ];
}
c = foo( 1, 2 );
d = [Imperative]
{
	return foo( 3 , -4 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 3, -4 };
            thisTest.Verify("c", expectedResult2, 0);
            thisTest.Verify("d", expectedResult3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Collection_Assignment_3()
        {
            string code = @"
def foo: int[]( a: int,b: int )
{
	return [ a+1,b-2 ];
}
c = foo( 1, 2 );
d = [Imperative]
{
	return foo( 2+1 , -3-1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 2, 0 };
            object[] expectedResult3 = { 4, -6 };
            thisTest.Verify("c", expectedResult2, 0);
            thisTest.Verify("d", expectedResult3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Collection_Assignment_4()
        {
            string code = @"
	def collectioninc: int[]( a : int[] )
	{
        return = [Imperative]
        {
		    b = a;
		    j = 0;
	
		    for( i in b )
		    {
			    a[j] = a[j] + 1;
			    j = j + 1;
		    }
		    return = a;
        }
	}
c = [Imperative]
{
	d = [ 1,2,3 ];
	return collectioninc( d );
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 3, 4 };
            thisTest.Verify("c", expectedResult, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Collection_Assignment_5()
        {
            string code = @"
def foo: int[] ( a : int[], b: int, c:int )
{
	a[b] = c;
	return = a;
}
d = [ 1,2,2 ];
b = foo( d,2,3 );
i = [Imperative]
{
	e = [ -2,1,2 ];
	c = foo( e,0,0 );
    return [e, c];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1, 2, 2 };
            object[] expectedResult2 = { 1, 2, 3 };
            object[] expectedResult3 = { 0, 1, 2 };
            object[] expectedResult4 = { -2, 1, 2 };
            thisTest.Verify("b", expectedResult2);
            thisTest.Verify("d", expectedResult1);
            thisTest.Verify("i", new object[] { expectedResult4, expectedResult3 });
        }
    }
}
