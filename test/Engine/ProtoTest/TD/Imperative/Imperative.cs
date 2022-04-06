using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Imperative
{
    class Imperative : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T004_array_TypeConversion()
        {
            String code =
             @"
                a = [Imperative]
                {
                    a[0] = 0;
                    a[1] = ""dummy"";  
                    return a;                
                }
                t1 = a;
             ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", new Object[] { 0, "dummy" });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T005_ClassConstructorNestedScope_TypeConversion()
        {
            String code =
             @"
import (""FFITarget.dll"");
a = ClassFunctionality.ClassFunctionality(true);";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T009_ClassConstructorNestedScope_LogicalOperators()
        {
            String code =
             @"
			 import(""FFITarget.dll"");
def foo ()
{
    return = false;
}
  
    def test (a1, b1) 
    {
		c1=0;
		c2=0;
		
        d=[Imperative]
        {
		  c = 0..3;
          b=  [Associative]
            {
                a=[Imperative]
                {
                   
                   
                    if( !a1 )  
                    {
                     e=a1;
                     f=b1;
                        t=ClassFunctionality.ClassFunctionality(0);
						c1 = a1 && b1  && !foo();
                        c2 = !a1 || !b1;                    
                    }
					return=[c1,c2];
                }
				return=a;
            }
			return=b;
        }
		return=d;
    }


result= test(false,true);


";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
		thisTest.Verify("result", new object [] {false,true});
        
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T010_ClassConstructorNestedScope_RelationalOperators()
        {
            String code =
             @"
			 import(""FFITarget.dll"");
def foo ()
 
 {
 
     return = 1;
 
 }
    
 ClassFunctionality.StaticProp = 0;

 
     def test (x) 
      
     {
      c1;
         d=[Imperative]
 
         {
 
            b= [Associative]
 
             {
 
              a= [Imperative]
 
                 {
 
                     if( x > 1 )  
 
                     {
 
                         c1 = ClassFunctionality.StaticFunction() > x ? ClassFunctionality.StaticFunction() : foo();                                         
 
                    }
 
                     else if ( x < 1 )
 
                     {
 
                         c1 = ClassFunctionality.StaticFunction() != foo() ? foo() : ClassFunctionality.StaticFunction();                        
 
                     }  
 
                     else 
 
                     {
 
                        c1 = ClassFunctionality.StaticFunction() == foo() ? foo() : ClassFunctionality.StaticFunction();                        
 
                     }            
                    return=c1;
                 }
            return=a;
 
             }
            return =b;
         }
return=d;
 
     }
 

 
 a1 = test(1);
 
 a2 = test(0);
 
 a3 = test(2); 

";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 0);
            thisTest.Verify("a2", 1);
            thisTest.Verify("a3", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T011_ClassConstructorNestedScope_MathematicalOperators()
        {
            String code =
             @"
			 import(""FFITarget.dll"");
def foo ()
{
    return = 1;
}


    def test () 
    {
        d=[Imperative]
        {
          b=  [Associative]
            {
                a=[Imperative]
                {
					t=ClassFunctionality.ClassFunctionality(0);
                    c1 =  t.IntVal + foo() / 5 * 5 %2;
					return=c1;
                }
				return=a;
            }
			return=b;
        }
		return=d;
    }

a1 = test();
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4082
            string errmsg = "MAGN-4082: Using the 'mod' operator on double value yields null in imperative scope and an unexpected warning message in associative scope";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1.0);
        }

        [Test]
        public void TestNamespaceQualifiedStaticProperty()
        {
            String code =
             @"
			 import(""FFITarget.dll"");

                a=[Imperative]
                {
                    ClassFunctionality.StaticProp = 1133;
                    return FFITarget.ClassFunctionality.StaticProp;
                };
            ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("a", 1133);
        }

        [Test]
        public void TestNamespaceQualifiedStaticPropertyChaining()
        {
            String code =
             @"
			 import(""FFITarget.dll"");

                a=[Imperative]
                {
                    ClassFunctionality.StaticProp = 1133;
                    return [FFITarget.ClassFunctionality.Instance.StaticFunction(), FFITarget.ClassFunctionality.Instance.IntVal];
                };
            ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("a", new double[] { 1133, 2349 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T012_ClassConstructorNestedScope_ImplicitTypeConversion()
        {
            String code =
             @"
def foo (x:int, y:int)
{
    return = x+y;
}
def foo2(x : double)
{
    return = x;
}

    def test () 
    {
        d=[Imperative]
        {
          b=  [Associative]
            {
                a=[Imperative]
                {
                    c1 = foo(foo2(1.0),2);
					return=c1;
                }
				return=a;
            }
			return=b;
        }
		return=d;
    }

a1 = test();
a2 = foo(foo2(1.0),2);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", 3);
        }
    }
}
