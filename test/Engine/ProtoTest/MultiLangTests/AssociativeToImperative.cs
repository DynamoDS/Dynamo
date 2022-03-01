using System;using NUnit.Framework;namespace ProtoTest.MultiLangTests{    [TestFixture]    class AssociativeToImperative : ProtoTestBase    {        [Test]        public void EmbeddedTest001()        {            String code =@"x;[Associative]{	x = 0;    [Imperative]    {        x = x + 5;    }}";            thisTest.RunScriptSource(code);            thisTest.Verify("x", 0);        }        [Test]        public void NestedLangBlock_WhileLoop()
        {
            var code = @"
a = 1;
b = 0..4;

i = [Imperative]
{
	c=0;
    while(a < 2)
    {

        [Associative]
        {
            c = a*b;
        }
        a = a+0.5;
    }
    return [c,a];
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("i", new object[] { new[] { 0, 1.5, 3, 4.5, 6 }, 2 });

        }        [Test]        public void NestedLangBlock_MultipleLevels()
        {
            var code = @"
a = [Associative]
{
	return [Imperative]
	{
		b = 0..4;
		return [Associative]
		{

			v = [Imperative]
		    {
				return 3;
		    }
		    return v*b;
		}
	}
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("a", new[] { 0, 3, 6, 9, 12 });

        }        [Test]        public void NestedLangBlock_MultipleLevels_WhileLoop()
        {
            var code = @"
a = 1;
i = [Imperative]
{

	c = 0;
	b = 0..4;

    while(a < 2)
    {
        [Associative]
        {
        	v = [Imperative]
            {
				z = 3;
				return z;
            }
            c = v*b;

        }
        a = a+0.5;
    }
    return [c,a];
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("i", new object[] { new[] { 0, 3, 6, 9, 12 }, 2 });

        }        [Test]        public void NestedLangBlock_ForLoop()
        {
            var code = @"
a = 1;

c = [Imperative]
{
    x = [Associative]
    {
    	b = 0..3;
        return 2*b;
    }
    for(i in x)
    {
    	a = a+i;
    }
    return a;
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("c", 13);

        }        [Test]        public void NestedLangBlock_AssocUpdate()
        {
            var code = @"
c = 323;
a = 2+c;
i = [Imperative]
{
	x = c;
	c = [Associative]
	{
		c = 209;
		return c+1;
	};
	return [x,c];
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("c", 209);
            thisTest.Verify("a", 211);
            thisTest.Verify("i", new[] { 323, 210 });
        }        [Test]        public void NestedLangBlock_WhileLoop2()
        {
            var code = @"
a = 1;
b = 2;
i = [Imperative]
{
	c = 0;
	b = 0..4;

    while(a < 2)
    {

        [Associative]
        {

            c = 2*b;
        }
        a = a+0.2;
    }
    return [c,a];
};";
            thisTest.RunScriptSource(code);            thisTest.Verify("b", 2);
            thisTest.Verify("a", 1);
            thisTest.Verify("i", new object[] { new[] { 0, 2, 4, 6, 8 }, 2.2 });
        }

        [Test]
        public void NestedLangBlock_WhileLoop3()
        {
            var code = @"
a = 1;
x = 3;
i = [Imperative]
{
	c = 0;
	b = 0..4;

    while(a < 2)
    {
        [Associative]
        {
            c = x*b;
        }
        a = a+0.2;
    }
    return [c,a];
};";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("i", new object[] { new[] { 0, 3, 6, 9, 12 }, 2.2 });
        }

        [Test]
        public void NestedLangBlock_WhileLoop4()
        {
            var code = @"
a = 1;
x = 3;
i = [Imperative]
{
	c = 0;
	b = 0..4;

    while(a < 2)
    {
        [Associative]
        {
        	v = [Imperative]
        	{
            	return a;
            }
            c = v*b;
        }
        a = a+0.2;
    }
    return [c,a];
};";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] { new[] { 0, 2, 4, 6, 8 }, 2.2 });
        }
    }}