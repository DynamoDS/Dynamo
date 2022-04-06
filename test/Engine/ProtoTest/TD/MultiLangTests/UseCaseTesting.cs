using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.MultiLangTests
{
    class UseCaseTesting : ProtoTestBase
    {
        string testPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("SmokeTest")]
        public void T001_implicit_programming_Robert()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
// no paradigm specified, so assume associative
// some associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some imperative code ....
if (a>10) 	// implicit switch to imperative paradigm
{
	c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
	b=b/2;	// is executed before the statement on line 14 [as would be expected]
}
else
{
	[Associative] 	// explicit switch to associative paradigm [overrides the imperative paradigm]
	{
		c = b;    	// c references the final state of b, therefore [because we are in an associative paradigm] 
		b = b*2;	// the statement on line 21 is executed before the statement on line 20
	}
}
// some more associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a imperative block is nested within an associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the associative graph before the imperative block
			//			the imperative block
			//			the part of the associative graph after the imperative block
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                thisTest.Verify("a", 13);
                thisTest.Verify("b", 26);
                thisTest.Verify("c", 22);
            });
        }

        [Test]

        public void T001_implicit_programming_Robert_2()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case ");

            string code = @"
a = 10;
b = a*2;
a = a +1;

c = 0;
i = [Imperative]
{
	if (a>10) 	// explicit switch to imperative paradigm
	{
		c = b;
 	
		b=b/2;
	
	}
	else
	{
		[Associative] 	// explicit switch to associative paradigm 
		{
			c = b;
    	
			b = b*2;
	
		}
	}
	return [b,c];
}
 
a = a + 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 13);
            thisTest.Verify("i", new[] {13.0, 26});

        }

        [Test]
        [Category("Replication")]
        public void T002_limits_to_replication_1_Robert()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections ");

            string code = @"
a = 0..10..2; 
b = a>5? 0:1; 
[Imperative]
{
	c = a * 2; // replication within an imperative block [OK?]
	d = a > 5 ? 0:1; // in-line conditional.. operates on a collection [inside an imperative block, OK?]
	if( c[2] > 4 ) x = 10; // if statement evaluates a single term [OK]
	
	if( c > 4 ) // but... replication within a regular 'if..else' any support for this?
	{
		y = 1;
	}
	else
	{
		y = -1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1, 1, 0, 0, 0 };
            thisTest.Verify("b", v1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T004_simple_order_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
b1 = 20;        // =1
a2 = a1 + b1;   // =3
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5
a  = a2 + b;    // 6";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 10);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b2", 50);
            thisTest.Verify("b", 52);
            thisTest.Verify("a", 82);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_grouped_1_Robert()
        {
            string code = @"
a1 = 10;        // =1
a2 = a1 + b1;   // =3
a  = a2 + b;    // 6    
    
b1 = 20;        // =1
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 82);
            thisTest.Verify("b", 52);
            thisTest.Verify("a2", 30);
            thisTest.Verify("b1", 20);
            thisTest.Verify("a1", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_imperative_if_inside_for_loop_1_Robert()
        {
            string code = @"
x = [Imperative]
{
	x = 0;
	
	for ( i in 1..10..2)
	{
		x = i;
		if(i>5) x = i*2; // tis is ignored
		// if(i<5) x = i*2; // this causes a crash
	}
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 18);
        }

        [Test]
        public void T014_Robert_2012_09_14_MultipleNestedLanguage()
        {
            string code =
    @"
            
         def foo  ()
    {   
        t = [Imperative]
        {
              t1 = [Associative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    def foo2  ()
    {   
        t = [Associative]
        {
              t1 = [Imperative]
             {                    
                    t2 = 6;   
                    return = t2; 
            }     
           return = t1;                
        }
        return = t;   
    }
    p1 = foo(); // expected 6, got null
    p2 = foo2();// expected 6, got 6
";
            thisTest.RunScriptSource(code,"", testPath);

            thisTest.Verify("p1", 6);
            thisTest.Verify("p2", 6);
            //thisTest.Verify("totalLength", 2.0 ); // this needs to be verified after the defect is fixed
        }
    }
}
