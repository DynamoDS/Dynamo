using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    [TestFixture]
    class TypeSystemTests : ProtoTestBase
    {
        [Test]
        [Category("Type System")]
        public void TS001_IntToDoubleTypeConversion()
        {
            string code =
                @"a = 1;
                b : double = a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1.0);
        }

        [Test]
        [Category("Type System")]
        public void TS001a_DoubleToIntTypeConversion()
        {
            string code =
                @"a = 1.0;
                b : int = a;
                c : int = 2.1;
                d : int = 2.5;
                e : int = 3.0;";
            thisTest.RunScriptSource(code);
            //These should convert and emit warnings
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 3);
            thisTest.Verify("e", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS002_IntToUserDefinedTypeConversion()
        {
            string code =
                @"
                class A {}
                a = 1;
                b : A = a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("Type System")]
        public void TS003IntToChar1467119()
        {
            string code =
                @"
                def foo ( x : char )
                {    
                return = x;
                }
                y = foo (1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("Type System")]
        public void TS004_IntToChar_1467119_2()
        {
            string code =
                @"
                def foo ( x : char )
                {    
                return = true;
                }
                y = foo ('1');";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", true);
        }

        [Test]
        [Category("Type System")]
        public void TS005_RetTypeArray_return_Singleton_1467196()
        {
            string code =
                @"
                   //return type class and return an array of class-
                   class A
                        {
                            X : int;
                        }
                        def length : A[] (pts : A[])
                        {
                            return = pts[0];
                        }
                        pt1 = A.A( );
                        pt2 = A.A(  );
                        pts = {pt1, pt2};
                        numpts = length(pts); 
                        a=numpts.X;
                       
                ";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 0 });
        }

        [Test]
        [Category("Type System")]
        public void TS005_RetTypeArray_return_Singleton_1467196_a()
        {
            string code =
                @"
                        def length : int[] (pts : int[])
                        {
                            return = pts[0];
                        }
                        pts = {1, 2};
                        numpts = length(pts); 
                        a=numpts;
                       
                ";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS005_RetTypeArray_return_Singleton_1467196_b()
        {
            string code =
                @"
                   //return type class and return an array of class-
                   class A
                        {
                            X : int;
                        }
                        def length : A[] (a : A)
                        {
                            return = a;
                        }
                        pt = A.A();
                        numpts = length(pt); 
                        a=numpts.X;
                       
                ";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 0 });
        }


        [Test]
        [Category("Type System")]
        public void TS006_RetTypeuserdefinedArray_return_double_1467196()
        {
            string code =
                @"
                   //return type class and return a double
                   class A
                    {
                        X : int;
                    }
                    def length : A (pts : A[])
                    {
                        return = 1.0;
                    }
                    pt1 = A.A();
                    pt2 = A.A();
                    pts = {pt1, pt2};
                    numpts = length(pts); 
                    a=numpts.X;
                ";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            //thisTest.Verify("a",); not sure what is the expected behaviour 
        }

        [Test]
        [Category("Type System")]
        public void TS007_Return_double_To_int_1467196()
        {
            string code =
                @"
                        //return type int and return a double
                        class A
                        {
                            X : int;
                        }
                        def length : double (pts : A[])
                        {
                              return = 1;
                        }
                        pt1 = A.A( );
                        pt2 = A.A( );
                        pts = {pt1, pt2};
                        numpts = length(pts); 
                         a=numpts.X;
                 
                ";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 1.0);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS008_Param_Int_IntArray_1467208()
        {
            string code =
                @"
                      def foo:int[] (x:int[])
                      {
                          return = 1;
                      }
                      r = foo(3);            
                ";
            string error = "DNL-1467208 Auto-upcasting of int -> int[] is not happening in some cases";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS009_Parameter_Int_ToBoolArray_1467182()
        {
            string code =
                @"
                       def foo(x:bool[])
                       {
                          return = x;
                       }
                       r = foo(3);            
                ";
            //int -> bool -> bool array
            string error = "1467303 -Sprint 26 - Rev 3779 - int to bool array conversion does not happen for function arguments ";

            thisTest.RunScriptSource(code, error);
            thisTest.Verify("r", new object[] { true });

        }

        [Test]
        [Category("Type System")]
        public void TS010_Parameter_Bool_ToIntArray_1467182()
        {
            string code =
                @"
                      def foo(x:int[])
                      {
                            return = 1;
                       }
                       r = foo(false);    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS011_Return_Int_ToIntArray()
        {
            string code =
                @"
                        def foo:int[](x:int)
                        {
                              return = x;
                        }
                        r = foo(3); // r = {3};
                ";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS012_Return_Int_ToBoolArray_1467182()
        {
            string code =
                @"
                    def foo:bool[]()
                    {
                            return = x;
                     }
                     r = foo(3);            // r = {null} ?
                ";
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases  ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS013_Parameter_Bool_ToIntArray()
        {
            string code =
                @"
                        def foo:int[]()
                        {
                           return = false;
                        }
                        r = foo(); // r = {null}
                ";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS014_Return_IntArray_ToInt()
        {
            string code =
                @"
                       def foo:int()
                       {
                            return = {1, 2, 3};
                       }              
                       r = foo();                              
                ";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS015_Parameter_BoolArray_ToInt()
        {
            string code =
                @"
                      def foo(x:int)
                      {
                             return = x + 1;
                      }
                      r = foo({true, false}); // method resolution failure, r= null
                              
                ";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS016_Return_BoolArray_ToInt()
        {
            string code =
                @"
                       def foo:int()
                       {
                              return = {true, false};
                       }              
                       r = foo();                             
                ";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS017_Return_BoolArray_ToInt_1467182()
        {
            string code =
                @"
                       class A
                            {
                            id:int;
                            }
                            class B
                            {
                                id:int;
                            }
                            a = {A.A(),B.B()};
                            b=a;
                            a[0].id = 100;
                            b[0].id = ""false"";
                            c=a[0].id;
                            d=b[0].id;";
            string error = "1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("c", null);// null 
        }

        [Test]
        [Category("Type System")]
        public void TS018_Param_Int_ordouble_ToBool_1467172()
        {
            string code =
                @"
b;c;d;
                      [Imperative]
                            {
                            c={};
                            def foo( a : bool )
                            {
                            c={a};
                            return = c; 
                            }
                            b = foo( 1 );
                            c = foo( 1.5 );
                            d = 0;
                            if(1.5 == true )
                            {
                            d = 3;
                            }
                            }
";
            //Assert.Fail("1467172 - sprint 25 - Rev 3146 - [Design Issue ] the type conversion between int/double to bool not allowed ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS018_Return_Int_ordouble_ToBool_1467172_2()
        {
            string code =
                @"
b;c;d;
                      [Imperative]
                            {
                        
                            def foo:bool( a  )
                            {
                            
                            return = a; 
                            }
                            b = foo( 1 );
                            c = foo( 1.5 );
                            d = 0;
                            if(1.5 == true )
                            {
                            d = 3;
                            }
                            }
";
            //Assert.Fail("1467172 - sprint 25 - Rev 3146 - [Design Issue ] the type conversion between int/double to bool not allowed ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", true);
            thisTest.Verify("c", true);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS019_conditional_cantevaluate_1467170()
        {
            string code =
                @"A;
                     [Imperative]
                        {
                        A = 1;
                        if (0)
                        A = 2;
                        else
                        A= 3;
                        }
                   
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS019_conditional_cantevaluate_1465293_2()
        {
            string code =
                @"
                     A;
                     [Imperative]
                        {
                            A = 1;
                            if (!0)
                                A = 2;
                            else
                                A= 3;
                        }
                      
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 2);
        }

        [Test]
        [Category("Type System")]
        public void TS019_conditional_cantevaluate_1465293_3()
        {
            string code =
                @"
                     test1;
                     test2;
                     test3;
                     p1 = 1;
                     test1 = p1 == null ? true : false; // expected false, received false
                     test2 = p1!= null ? true : false; // expected true,  received false
                     test3 = !p1 ? true : false;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("test1", false);
            thisTest.Verify("test2", true);
            thisTest.Verify("test3", false);
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1467170()
        {
            string code =
                @"A;
                     [Imperative]
                        {
                        A = 1;
                        B=1;
                        if (B)
                        A = 2;
                        else
                        A= 3;
                        }
                      
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 2);
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1465293()
        {
            string code =
                @"
                     a = { 1, 2 };
                        b = 0;
                        def foo(a)
                        {
                         d=   [Imperative]
                            {
                                if (a!= null)
                                {
                                    b = 1;
                                }
                                return = b;
                            }
                            return = d;
                        }
                        z;
                        [Imperative]
                        {
                            z = foo(a);
                        }
                        
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", new object[] { 1, 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1465293_2()
        {
            string code =
                @"
                    a = { 1, 2 };
b = 0;
class test
{
    def foo(a)
    {
        d = [Imperative]
        {
            if (a!= null)
            {
                b = 1;
            }
            return = b;
        }
        return = d;
    }
}
z;
[Imperative]
{
    y = test.test();
    z = y.foo(a);
}
                        
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", new object[] { 1, 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1465293_3()
        {
            string code =
                @"
                    a = { 1, 2 };
b = 0;
class test
{
    def foo(a)
    {
        d = [Imperative]
        {
            if (a!= null)
            {
                b = 1;
            }
            return = b;
        }
        return = d;
    }
}
z;
    y = test.test();
    z = y.foo(a);
                        
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", new object[] { 1, 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1465293_4()
        {
            string code =
                @"
                     a = { 1, 2 };
                        b = 0;
                        def foo(a)
                        {
                         d=   [Imperative]
                            {
                                if (a!= null)
                                {
                                    b = 1;
                                }
                                return = b;
                            }
                            return = d;
                        }
                        z;
                        
                            z = foo(a);
                        
                        
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", new object[] { 1, 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS021_OverallPrimitiveConversionTestInt()
        {
            string code =
                @"
                class A {}
                zero_var:var = 0;
                zero_int:int = 0;
                zero_double:double = 0;
                zero_bool:bool = 0;
                zero_String:string = 0;
                zero_char:char = 0;
                zero_a:A = 0;
                 one_var:var = 1;
                 one_int:int = 1;
                 one_double:double = 1;
                 one_bool:bool = 1;
                 one_String:string = 1;
                 one_char:char = 1;
                 one_a:A = 1;
                foo:int = 32.342;
                foo2:int = 32.542;
                foo3:int = 32.5;
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("zero_var", 0);
            thisTest.Verify("zero_int", 0);
            thisTest.Verify("zero_double", 0.0);
            thisTest.Verify("zero_bool", false);
            thisTest.Verify("zero_String", null);
            thisTest.Verify("zero_char", null);
            thisTest.Verify("zero_a", null);
            thisTest.Verify("one_var", 1);
            thisTest.Verify("one_int", 1);
            thisTest.Verify("one_double", 1.0);
            thisTest.Verify("one_bool", true);
            thisTest.Verify("one_String", null);
            thisTest.Verify("one_char", null);
            thisTest.Verify("one_a", null);
            thisTest.Verify("foo", 32);
            thisTest.Verify("foo2", 33);
            thisTest.Verify("foo3", 33);
        }

        [Test]
        [Category("Type System")]
        public void TS022_conditional_cantevaluate_1467170()
        {
            string code =
                @"A;
                     [Imperative]
                        {
                        A = 1;
                        B=1;
                        if (null)
                        A = 2;
                        else
                        A= 3;
                        }
                        //expected A=1;
                        //Received A=3;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS022_conditional_cantevaluate_1465293()
        {
            string code =
                @"
                     A;                   
                        [Imperative]
                        {
                        A = 1;
                        B=1;
                        if (!null)
                            A = 2;
                        else
                            A= 3;
                        }
                        //expected A=1;
                        //Received A=3;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS022_conditional_cantevaluate_1465293_3()
        {
            string code =
                @"
                     a = { 1, 2 };
                     b = 0;
                     [Imperative]
                        {
                            if (a!= null)
                            {
                                b = 1;
                            }
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084()
        {
            string code =
                @"
                     def foo:int(i:int)
                        {
                             return = i;
                        }
                        x = foo(2.5);// returning 2.5 it should return 2
                        ";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084_2()
        {
            string code =
                @"
                     def foo ( x : int )
                        {
                        return = x + 1;
                        }
                        t = foo( 1.5);
                        ";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084_3()
        {
            string code =
                @"
                     class twice
                        {
                                def twice : int []( a : double )
                                {
                         //      c=(1..a)..5..1;
                                        //return = c;
                        return = {{1,1},{1,1}};
                                }
                        }
                        d=1..4;
                        a=twice.twice();
                        d=a.twice(4);
                        ";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS024_Double_To_Int_IndexIntoArray_1467214()
        {
            string code =
                @"
                  a={1,2,3,4,5};
                    x=2.5;
                    b=a[x];
                        ";
            //     Assert.Fail("1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS025_KeyWords_Doesnotexist_1467215()
        {
            string code =
                @"
                  a:z=3;
                        ";
            string error = "1467215 - Sprint 26 - rev 3310 language is too easy on key words for typesystem , even when does not exist it passes  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS026_Double_ToInt_1467211()
        {
            string code =
                @"
                  def foo:int()
                  {
                      return = 3.5; 
                  }
                  a=foo();
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS027_Double_ToInt_1467217()
        {
            string code =
                @"
                 def foo:int[]()
                    {
                         return = {3.5}; 
                    }
                    a=foo();
                        ";
            //Assert.Fail("1467217 - Sprint 26 - Rev 3337 - Type Conversion does not happen if the function returns an array ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 4 });
        }

        [Test]
        [Category("Type System")]
        public void TS028_Double_ToInt_1467218()
        {
            string code =
                @"
                  def foo:int()
                  {
                      return = 3.5; 
                  }
                  a=foo();
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS028_Double_ToInt_1467218_1()
        {
            string code =
                @"
                  def foo:int[]()
                  {
                      return = {3.5}; 
                  }
                  a=foo()[0];
                        ";
            //Assert.Fail("1467218 - Sprint 26 - Rev 3337 - Type Conversion does not happen if the function returns and array and and index into function call ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS029_Double_ToVar_1467222()
        {
            string code =
                @"
                  class A
                    {
                        x:int;
                        def foo()
                        {
                            x : double = 3.5; // x still is int, and 3.5 converted to 4
                            return = x;
                        }
                    }
                    a = A.A();
                    b = a.foo();
 
                        ";
            //Assert.Fail("1467222 - Sprint 26 - rev 3345 - if return type is var it still does type conversion ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS029_Double_ToInt_1463268()
        {
            string code =
                @"
                  def foo ( x : int )
                    {
                    return = x + 1;
                    }
                    t = foo( 1.5);
                        ";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected");
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS030_eachtype_To_var()
        {
            string code =
                @"class A{ a=1; }
                  
                        def foo ( x )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        b = foo( 1); 
                        c = foo( ""1.5""); //char to var 
                        //a = foo( '1.5');// char to var 
                         d = foo( A.A()); // user define to var 
                        d1=d.a;
                        e = foo( false);//bool to var 
                        f = foo( null);//null to var 
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.5);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", "1.5");
            thisTest.Verify("d1", 1);
            thisTest.Verify("e", false);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS030_eachtype_To_var_2()
        {
            string code =
                @"class A{ a=1; }
                  
                        def foo ( x:var )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        b = foo( 1); 
                        c = foo( ""1.5""); //char to var 
                        //a = foo( '1.5');// char to var 
                         d = foo( A.A()); // user define to var 
                        d1=d.a;
                        e = foo( false);//bool to var 
                        f = foo( null);//null to var 
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.5);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", "1.5");
            thisTest.Verify("d1", 1);
            thisTest.Verify("e", false);
            thisTest.Verify("f", null);
        }


        [Test]
        [Category("Type System")]
        public void TS031_eachType_To_int()
        {
            string code =
                @"
                  class A{ a=1; }
                        def foo ( x:int )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        b = foo( 1); 
                        c = foo( ""1.5""); // var to int 
                        //a = foo( '1.5');// var to int
                         d = foo( A.A()); // user define to var 
                        d1=d.a;
                        e = foo( false);// var to int 
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS031_null_To_int()
        {
            string code =
                @"
            
                        def foo ( x:int )
                        {
	                   
	                        return =1;
                        }
                                              
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", 1);

        }

        [Test]
        [Category("Type System")]
        public void TS031_eachtype_To_double()
        {
            string code =
                @"
                   class A{ a=1; }
                        def foo ( x:double )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1); 
                        b = foo( 1); 
                        c = foo( ""1.5""); // var to int 
                        //a = foo( '1.5');// var to int
                         d = foo( A.A()); // user define to var 
                        d1=d.a;
                        e = foo( false);// var to int 
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1.0);
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS031_null_To_double()
        {
            string code =
                @"
                   
                        def foo ( x:double )
                        {
	               
	                        return =1.0;
                        }
                   
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", 1.0);
        }

        [Test]
        [Category("Type System")]
        public void TS032_null_To_bool()
        {
            string code =
                @"
                   
                        def foo ( x:bool )
                        {
	               
	                        return =true;
                        }
                   
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", true);
        }

        [Test]
        [Category("Type System")]
        public void TS032_eachType_To_bool()
        {
            string code =
                @"
                    class A{ a=1; }
                        a:bool= 1;//true
                        b:bool= -0.1; //true
                        c:bool=""1.5""; //true
                        c1:bool= """"; //false
                        //d:bool='1.5';
                        d:bool= A.A(); // user def to bool - > if not null true
                       
                        e:bool= true;
                        e1:bool=null;
                          ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", true);
            thisTest.Verify("b", true);
            thisTest.Verify("c", true);
            thisTest.Verify("c1", false);
            thisTest.Verify("d", true);
            thisTest.Verify("e", true);
            thisTest.Verify("e1", null);
        }

        [Test]
        [Category("Type System")]
        public void TS033_eachType_To_string()
        {
            string code =
                @"
                    class A{ a=1; }
                       def foo ( x:string)
                          {
                              b1= x ;
                              return =b1;
                          }
                          a = foo(0.00); // double to string
                          b = foo( 1); // int to  string
                          c = foo( ""1.5"");//char to string  
                          c1 = foo( '1');// char to string 
                          d = foo( A.A()); // user define to string
                          d1=d.a;
                          e = foo( false);//bool to string
                          f = foo( null);//null to string";
            string error = "1467311 - Sprint 26 - Rev 3788 - Char to String conversion not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", "1.5");
            thisTest.Verify("c1", "1");
            thisTest.Verify("d1", null);
            //   thisTest.Verify("c1", "1");//Assert.Fail("1467227 -Sprint 26 - 3329 char not convertible to string ");
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS033_null_To_string()
        {
            string code =
                @"
                   
                        def foo ( x:string )
                        {
	               
	                        return = ""test"";
                        }
                   
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", "test");
        }

        [Test]
        [Category("Type System")]
        public void TS033_null_To_char()
        {
            string code =
                @"
                   
                        def foo ( x:char )
                        {
	               
	                        return = 'c';
                        }
                   
                        f = foo( null);//null to int
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("f", 'c');
        }

        [Test]
        [Category("Type System")]
        public void TS034_eachType_To_char()
        {
            string code =
                @"
                     class A{ a=1; }
                       def foo ( x:char)
                          {
                              b1= x ;
                              return =b1;
                          }
                          a = foo(0.00); // double to char
                          b = foo( 1); // int to  char
                          c = foo( ""1.5"");//char to char
                          c1 = foo( '1');// char to char  
                          d = foo( A.A()); // user define to char
                          d1=d.a;
                          e = foo( false);//bool to char
                          f = foo( null);//null to char";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("c1", '1');
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS34_CharToString_1467227()
        {
            string code =
               @"
                a:string='1';";
            thisTest.RunScriptSource(code, "1467227 -Sprint 26 - 3329 char not convertible to string ");
            thisTest.Verify("a", "1");
        }

        [Test]
        [Category("Type System")]
        public void TS35_nullTobool_1467231()
        {
            string code =
                @"
                a:bool=null;";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467231 - Sprint 26 - Rev 3393 null to bool conversion should not be allowed ");
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS36_stringTobool_1467239()
        {
            string code =
                @"
                c:bool=""1.5""; //expected :true, received : null
                c1:bool= """"; //expected :false,received : null 
";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467239 - Sprint 26 - Rev 3425 type conversion - string to bool conversion failing  ");
            thisTest.Verify("c", true);
            thisTest.Verify("c1", false);
        }

        [Test]
        [Category("Type System")]
        public void TS37_userdefinedTobool_1467240()
        {
            string code =
                @"
                class A
                    {
	                    a:int;
	                    constructor A (b:int)
	                    {
		                    a=b;
                        }
                    }
                    d:bool=A.A(5); // user def to bool - > if not null true ";
            string error = "1467240 - Sprint 26 - Rev 3426 user defined type not convertible to bool";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("d", true);
        }

        [Test]
        [Category("Type System")]
        public void TS37_userdefinedTo_null()
        {
            string code =
                @"
                class B{b=0;}
                class A
                    {
	                    a:int;
	                    constructor A (b:B)
	                    {
		                    a=1;
                        }
                    }
                    //d:bool='1.5';
                    d=A.A(null); // user def to bool - > if not null true 
                    d1 = d.a;";

            thisTest.RunScriptSource(code);
            //Assert.Fail("1467240 - Sprint 26 - Rev 3426 user defined type not convertible to bool");
            thisTest.Verify("d1", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS038_eachType_To_Userdefined()
        {
            string code =
                @"
                    class B{ b=1; }
                            class A
                            {
	                            a:int;
	                            constructor A (b:int)
	                            {
		                            a=b;
                                }
                            }
                            a:A= 1;//
                            b:A= -0.1; //
                            c:A= ""1.5""; //false
                            d:A=true;
                            //d:bool='1.5';
                            d:A= B.B(); // user def to bool - > if not null true
                            e:A=A.A(1);
                            e1=e.a;
                            f:A= true;
                            g:A=null;
                          ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", 1);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TS038_Param_single_AlltypeTo_UserDefined()
        {
            string code =
                @"
                    class A{ a=1; }
                    class B extends A{ b=2; }
                        def foo ( x:A)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( B.B() );
                        d1 = d.b;
                        e = foo(false);
                        f = foo( null );";
            string error = "1467314 - Sprint 26 - Rev 3805 user defined type to array of user defined type does not upgrade to array ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", 2);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }


        [Test]
        [Category("Type System")]
        public void TS038_return_single_AlltypeTo_UserDefined()
        {
            string code =
                @"
                      class B extends A{ b=2; }
                        class A{ a=1; }
                        def foo :A( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( B.B() );
                        d1 = d.b;
                        e = foo(false);
                        f = foo( null );";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", 2);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS039_userdefined_covariance()
        {
            string code =
                @"
               
                    class A
                    {
	                    a:int;
	                    constructor A (b:int)
	                    {
		                    a=b;
                        }
                    }
                    class B extends A
                    {
	                    b:int;
	                    constructor B (c:int)
	                    {
		                    b=c;
                        }
                    }
                    a:A=A.A(1);
                    a1=a.a;
                    b:A=B.B(2);
                    b1=b.b;
                    b2=b.a;
                    c:B=A.A(3);
                    c1=c.b;
                    c2=c.a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1);
            thisTest.Verify("b1", 2);
            thisTest.Verify("b2", 0);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("c2", null);
        }

        [Test]
        [Category("Type System")]
        public void TS039_userdefined_covariance_2()
        {
            string code =
                @"
               
                    class B extends A{ b=2; }
                    class A{
                        a : B;
                        c : A;
                    }
                    def foo( x)
                    {
                        test = A.A();
                        test.a = x;
                        test.c = x;
                        return = test;
                    }
                    a1 = foo( B.B() );
                    b1 = a1.a.b;
                    b2 = a1.c.b;
                    c1 = foo(A.A());
                    d1= c1.a.b;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2);
            thisTest.Verify("b2", 2);
            thisTest.Verify("d1", null);
        }

        [Test]
        [Category("Type System")]
        public void TS40_null_toBool_1467231()
        {
            string code =
                @"
                a:bool=null;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS41_null_toBool_1467231_2()
        {
            string code =
                @"
                a=null; 
                def test(b:bool)
                {
                return = b;
                }
                c=test(a);"; //expected :true, received : null
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("Type System")]
        public void TS42_null_toBool_1467231_3()
        {
            string code =
                @"
                a=null; 
                def test:bool(b)
                {
                return = b;
                }
                c=test(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("Type System")]
        public void TS43_null_toBool_1467231_positive()
        {
            string code =
                @"b;
                [Imperative]
                    {
                    a=null;
                    b;
                    if (a) 
                    {
	                    b=1;
                    }
                    else
                    {
	                    b=2;
                    }
                    }"; //expected :true, received : null
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("Type System")]
        public void TS44_any_toNull()
        {
            string code =
                @"
                class test
                    {
                    
                    }
                    a:double= null; 
                    b:int =  null; 
                    c:string=null; 
                    d:char = null;
                    e:test = null;
                    f:bool = null;
                    g = null;"; //expected :true, received : null
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TS45_int_To_Double_1463268()
        {
            string code =
                @"
               def foo(x:int) 
               { 
	               return = x + 1; 
               }
               t = foo(1.5);
                t1 = foo(null);";

            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
            thisTest.Verify("t1", null);
        }

        [Test]
        [Category("Type System")]
        public void TS46_typedassignment_To_array_1467206()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[]= {1,2,3}; 
                    
                    b:int[] =  {1,2,3}; 
                    c:string[]={""a"",""b"",""c""}; 
                    d:char []= {'c','d','e'};
                    x1= test.test();
                    y1= test.test();
                    z1= test.test();
                    e:test []= {x1,y1,z1};
                    e1=e.x;
                    f:bool []= {true,false,null};
                    g []={ null,null,null};";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1.0, 2.0, 3.0 });
            thisTest.Verify("b", new object[] { 1, 2, 3 });
            thisTest.Verify("c", new object[] { "a", "b", "c" });
            thisTest.Verify("d", new object[] { 'c', 'd', 'e' });
            thisTest.Verify("e1", new object[] { 1, 1, 1 });
            thisTest.Verify("f", new object[] { true, false, null });
            thisTest.Verify("g", new object[] { null, null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS46_typedassignment_To_array_1467294_2()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[]= 1; 
                    
                    b:int[] =  1.1; 
                    c:string[]=""a""; 
                    d:char []= 'c';
                    x1= test.test();
                    e:test []= x1;
                    e1=e.x;
                    f:bool []= true;
                    g []=null;";
            
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1.0 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", new object[] { "a" });
            thisTest.Verify("d", new object[] { 'c' });
            thisTest.Verify("e1", new object[] { 1 });
            thisTest.Verify("f", new object[] { true });
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_To_array_1467294_3()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[][]= {1}; 
                    
                    b:int[][] =  {1.1}; 
                    c:string[][]={""a""}; 
                    d:char [][]= {'c'};
                    x1= test.test();
                    e:test [][]= {x1};
                    e1=e.x;
                    f:bool [][]= {true};
                    g [][]={null};";


            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3973
            string error = "MAGN-3973: {null} to array upgrdation must null out ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1.0 } });
            thisTest.Verify("b", new object[] { new object[] { 1 } });
            thisTest.Verify("c", new object[] { new object[] { "a" } });
            thisTest.Verify("d", new object[] { new object[] { 'c' } });
            thisTest.Verify("e1", new object[] { new object[] { 1 } });
            thisTest.Verify("f", new object[] { new object[] { true } });
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_To_Vararray_1467294_4()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:var[][]= 1; 
                    
                    b:var[][] =  1.1; 
                    c:var[][]=""a""; 
                    d:var[][]= 'c';
                    x1= test.test();
                    e:var[][]= x1;
                    e1=e.x;
                    f:var[][]= true;
                    g :var[][]=null;";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3974
            string error = "MAGN-3974: In typed assignment, array promotion does not occur in some cases";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1 } });
            thisTest.Verify("b", new object[] { new object[] { 1.1 } });
            thisTest.Verify("c", new object[] { new object[] { "a" } });
            thisTest.Verify("d", new object[] { new object[] { 'c' } });
            thisTest.Verify("e1", new object[] { new object[] { 1 } });
            thisTest.Verify("f", new object[] { new object[] { true } });
            thisTest.Verify("g", new object[] { new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS46_typedassignment_To_Intarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:int[]= 1; 
                    
                    b:int[] =  1.1; 
                    c:int[]=""a""; 
                    d:int[]= 'c';
                    x1= test.test();
                    e:int[]= x1;
                    e1=e.x;
                    f:int[]= true;
                    g :int[]=null;";
            string error = "1467294 - Sprint 26 - Rev 3763 - in typed assignment, array promotion does not occur in some cases";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_singleton_To_Intarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:int[][]= {1}; 
                    
                    b:int[][] =  {1.1}; 
                    c:int[][]={""a""}; 
                    d:int[][]= {'c'};
                    x1= test.test();
                    e:int[][]= {x1};
                    e1=e.x;
                    f:int[][]= {true};
                    g :int[][]={null;}";
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1 } });
            thisTest.Verify("b", new object[] { new object[] { 1 } });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        public void TS46_typedassignment_To_doublearray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[]= 1; 
                    
                    b:double[] =  1.1; 
                    c:double[]=""a""; 
                    d:double[]= 'c';
                    x1= test.test();
                    e:double[]= x1;
                    e1=e.x;
                    f:double[]= true;
                    g :double[]=null;";
            //string error = "1467294 - Sprint 26 - Rev 3763 - in typed assignment, array promotion does not occur in some cases";
            string error = "1467332  - Sprint 27 - Rev 3956 {null} to array upgrdation must null out ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.0 });
            thisTest.Verify("b", new object[] { 1.1 });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_Singleton_To_doublearray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[][]= {1}; 
                    
                    b:double[][] =  {1.1}; 
                    c:double[][]={""a""}; 
                    d:double[][]= {'c'};
                    x1= test.test();
                    e:double[][]= {x1};
                    e1=e.x;
                    f:double[][]= {true};
                    g :double[][]={null};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1.0 } });
            thisTest.Verify("b", new object[] { new object[] { 1.1 } });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TS46_typedassignment_To_boolarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:bool[]= 1; 
                    
                    b:bool[] =  1.1; 
                    c:bool[]=""a""; 
                    d:bool[]= 'c';
                    x1= test.test();
                    e:bool[]= x1;
                    e1=e.x;
                    f:bool[]= true;
                    g :bool[]=null;";
            string error = "1467295- Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";

            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true });
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("d", new object[] { true });
            thisTest.Verify("e", new object[] { true });
            thisTest.Verify("f", new object[] { true });
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        public void TS46_typedassignment_singleton_To_boolarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:bool[][]= {1}; 
                    
                    b:bool[][] =  {1.1}; 
                    c:bool[][]={""a""}; 
                    d:bool[][]= {'c'};
                    x1= test.test();
                    e:bool[][]= {x1};
                    e1=e.x;
                    f:bool[][]= {true};
                    g :bool[][]={null;}";
            //string error = "1467295- Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";
            string error = "1467332  - Sprint 27 - Rev 3956 {null} to array upgrdation must null out ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { true } });
            thisTest.Verify("b", new object[] { new object[] { true } });
            thisTest.Verify("c", new object[] { new object[] { true } });
            thisTest.Verify("d", new object[] { new object[] { true } });
            thisTest.Verify("e", new object[] { new object[] { true } });
            thisTest.Verify("f", new object[] { new object[] { true } });
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TS46_typedassignment_To_stringarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:string[]= 1; 
                    
                    b:string[] =  1.0; 
                    c:string[]=""test""; 
                    d:string[]= '1';
                    x1= test.test();
                    e:string[]= x1;
                    e1=e.x;
                    f:string[]= false;
                    g :string[]=null;";
            //string error = "1467295 - Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";
            string error = "1467332  - Sprint 27 - Rev 3956 {null} to array upgrdation must null out ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { "test" });
            thisTest.Verify("d", new object[] { "1" });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_singleton_To_stringarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:string[][]= {1}; 
                    
                    b:string[][] =  {1.0}; 
                    c:string[][]={""test""}; 
                    d:string[][]= {'1'};
                    x1= test.test();
                    e:string[][]= {x1};
                    e1=e.x;
                    f:string[][]= {false};
                    g :string[][]={null};";

            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { new object[] { "test" } });
            thisTest.Verify("d", new object[] { new object[] { "1" } });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        public void TS46_typedassignment_To_chararray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:char[]= 1; 
                    
                    b:char[] =  1.0; 
                    c:char[]=""test""; 
                    d:char[]= '1';
                    x1= test.test();
                    e:char[]= x1;
                    e1=e.x;
                    f:char[]= false;
                    g :char[]=null;";
            string error = "1467295 - Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", new object[] { '1' });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]

        [Category("Type System")]
        public void TS46_typedassignment_singleton_To_chararray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:char[][]= 1; 
                    
                    b:char[][] =  1.0; 
                    c:char[][]=""test""; 
                    d:char[][]= '1';
                    x1= test.test();
                    e:char[][]= x1;
                    e1=e.x;
                    f:char[][]= false;
                    g :char[][]=null;";
            string error = "1467295 - Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", new object[] { new object[] { '1' } });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TS047_double_To_Int_insidefunction()
        {
            string code =
                @"
              def foo(x:int) 
                {
	                x = 3.5; 
                
	                return = x; 
                }
                a=1.5;
                t = foo(a);";
            //thisTest.SetErrorMessage("1467250 Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ");
            string error = "1467250 - Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("t", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS047_double_To_Int_insidefunction_2()
        {
            string code =
                @"
              def foo(x:int )
                {
                    x:double = 3.5;
                    y = 3;
                    return = x * y;
                }
                a = 1.5;
                t = foo(a);";
            //thisTest.SetErrorMessage("1467250 Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ");
            string error = "1467250 - Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("t", 10.5);
        }

        [Test]
        [Category("Type System")]
        public void TS048_Param_eachType_To_varArray()
        {
            string code =
                @"
                  class A{ a=1; }
                        def foo ( x:var[] )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a  = foo( 1.5); 
                        b  = foo( 1); 
                        c  = foo( ""1.5""); // char to var 
                        d  = foo( A.A());   // user define to var
                        d1 = d.a;
                        e  = foo( false);   //bool to var 
                        f  = foo( null);    //null to var 
                        ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("d1", new object[] { 1 });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", null );
        }

        [Test]
        [Category("Type System")]
        public void TS048_Param_null_To_varArray()
        {
            string code =
                @"
                  def foo(x : var[])
                   {
                        return = 1;
                   }
                a = foo(null);
                        ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", 1);
        }


        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS049_Return_eachType_To_varArray()
        {
            string code =
                @"
                   class A{ a=1; }
                        def foo :var[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        b = foo( 1); 
                        c = foo( ""1.5""); 
                        //a = foo( '1.5'); 
                        d = foo( A.A()); // user define to var
                        d1=d.a;
                        e = foo( false); 
                        f = foo( null); 
                        ";


            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3964
            string error = "MAGN-3964: Type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("d1", new object[] { 1 });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS050_Return_eachType_To_intArray()
        {
            //  
            string code =
                @"
                  class A{ a=1; }
                        def foo :int[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        z:var=1.5;
                        a1=foo(z);
                        b = foo( 1); 
                        c = foo( ""1.5""); 
                        //a = foo( '1.5');
                        d = foo( A.A()); // user define to var
                        d1=d.a;
                        e = foo( false); 
                        f = foo( null); 
                        ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 2 });
            thisTest.Verify("a1", new object[] { 2 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS051_Param_eachType_To_intArray()
        {
            string code =
                @"
                 class A{ a=1; }
                        def foo ( x:int[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                        z:var=1.5;
                        a1=foo(z);
                        b = foo( 1); 
                        c = foo( ""1.5""); 
                        c1  = foo( '1');
                       d = foo( A.A()); // user define to var
                        d1=d.a;
                        e = foo( false); 
                        f = foo( null);
                        ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 2 });
            thisTest.Verify("a1", new object[] { 2 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS052_Return_AllTypeTo_doubleArray()
        {
            //  
            string code =
                @"
                  class A{ a=1; }
                        def foo :double[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                         z:var=1.5;
                        a1=foo(z);
                        b = foo( 1); 
                        c = foo( ""1.5""); 
                        c1 = foo( '1');
                        d = foo( A.A()); // user define to var
                        d1=d.a;
                        e = foo( false); 
                        f = foo( null); 
                        ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("a1", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1.0 });
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS053_Param_AlltypeTo_doubleArray()
        {
            //  
            string code =
                @"
                  class A{ a=1; }
                        def foo ( x:double[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5); 
                         z:var=1.5;
                        a1=foo(z);
                        b = foo( 1); 
                        c = foo( ""1.5"");  
                        c1 = foo( '1');
                        d = foo( A.A()); // user define to var
                        d1=d.a;
                        e = foo( false);
                        f = foo( null);
                        ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("a1", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1.0 });
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS055_Param_AlltypeTo_BoolArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo ( x:bool[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo({ 1.5, 2.5 });
                        z:var[]={ 1.5,2.5 };
                        a1=foo(z);
                        b = foo({ 1, 0 });
                        c = foo({ ""1.5"" ,""""});
                        c1 = foo( {'1','0'});
                        d = foo({ A.A(),A.A() });
                        e = foo({ false,true });
                        f = foo({ null, null });";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1664
            string error = "MAGN-1664 Sprint 26 - Rev 3781 - array of nulls to bool array , not working correctly in case of function arguments";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, true });
            thisTest.Verify("e", new object[] { false, true });
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS055_null_To_BoolArray_1467304()
        {
            string code =
                @"def func(a : bool[])
                    {
                        return = a;
                    }
                    a = func({null}); ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1664
            string error = "MAGN-1664 Sprint 26 - Rev 3781 - array of nulls to bool array , not working correctly in case of function arguments";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS055_null_To_BoolArray_1467304_2()
        {
            string code =
                @"def func(a : bool[])
                    {
                        return = a;
                    }
                    a = func({null,null}); ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1664
            string error = "MAGN-1664 Sprint 26 - Rev 3781 - array of nulls to bool array , not working correctly in case of function arguments";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS056_Return_AlltypeTo_BoolArray()
        {
            string code =
                @"
                   class A{ a=1; }
                        def foo :bool[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo({ 1.5, 2.5 });
                        z:var []={ 1.5,2.5 };
                        a1=foo(z);
                        b = foo({ 1, 0 });
                        c = foo({ ""1.5"" ,""""});
                        d = foo( {'1','0'});
                        e = d = foo({ A.A(),A.A() });
                        f = foo({ false,true });
                        g = foo({ null, null });
                                                  ";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3968
            string error = "MAGN-3968: Type conversion from var to bool array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, false });
            thisTest.Verify("e", new object[] { true, true });
            thisTest.Verify("f", new object[] { false, true });
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS056_Return_BoolArray_1467258()
        {
            string code =
                @"
                   class A{ a=1; }
                    def foo:bool[](x)
                            {
		 	                    b1= x ;
	                             return =b1;
                            }
                    a = foo({ 1.5, 2.5 });
                    z:var={ 1.5,2.5 };
                    a1=foo(z);
//                    a1 : var = foo({ 1.5,2.5 });
                    b = foo({ 1, 0 });
                    c = foo({ ""1.5"" ,""""});
                    d = foo({ '1', '0' });
                     e = d = foo({ A.A(),A.A() });
                                                  ";
            string error = "1467258 - sprint 26 - Rev 3541 if the return type is bool array , type conversion does not happen for some cases  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, false });
            thisTest.Verify("e", new object[] { true, true });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS057_Return_Array_1467305()
        {
            string code =
                @"
                   class A{ a=1; }
                    def foo:bool[](x)
                            {
		 	                    b1= x ;
	                             return =b1;
                            }
                    a = foo({ 1.5, 2.5 });
                    z:var={ 1.5,2.5 };
                    a1=foo(z);
                    b = foo({ 1, 0 });
                    c = foo({ ""1.5"" ,""""});
                    d = foo({ '1', '0' });
                                                  ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1665
            string error = "MAGN-1665 Sprint 26 - Rev 3782  adds an additonal rank while returning as array, when the rank matches";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, false });
        }

        [Test]
        [Category("Type System")]
        public void TS058_setter_Typeconversion_1467262()
        {
            string code =
                @"
                   class A
                    {
                        id : int;
                    }
                    a = A.A();
                    a.id = false;
                    c = a.id;
                ";
            string error = "1467262 - Sprint 26 - Rev 3543 , setter method does not do type conversion correctly";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.VerifyProperty(mirror, "a", "id", null, 0);
        }

        [Test]
        [Category("Type System")]
        public void TS059Double_To_int_1467203()
        {
            string code =
                @"
                   a:int=2.5;
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS060Double_To_int_1467203()
        {
            string code =
                @"
                   a:int=2.5;
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS061_typeconersion_imperative_1467213()
        {
            string code =
                @"
a;
                [Imperative]
                { 
                    a : int = 3.2;
                }
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS062_basic_upcoerce_assign()
        {
            string code =
                @"a;
                [Associative]
                { 
                    a : int[] = 3;
                }
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS063_basic_upcoerce_dispatch()
        {
            string code =
                @"a;
                [Associative]
                { 
                    def foo(i : int[])
                    { return=i; }
                    a = foo(3);
                }
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS063_basic_upcoerce_return()
        {
            string code =
                @"a;
                [Associative]
                { 
                    def foo:int[]()
                    { return=3; }
                    a = foo();
                }
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS064_bool_Conditionals_1467278()
        {
            string code =
                @"
                c = true;
                d = false;
                x = c == true;
                y = d == true;                   ";
            string error = "1467278 - Sprint 26 - Rev 3667 - type conversion fails when evaluating boolean conditionals ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("x", true);
            thisTest.Verify("y", false);
        }

        [Test]
        [Category("Type System")]
        public void TS065_doubleToInt_IndexingIntoArray_1467214()
        {
            string code =
                @"
                    a={1,2,3,4,5};
                    x=2.5;
                    b=a[x];
                    c=a[2.1];       
                    d=a[-2.1]; ";
            string error = "1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 4);

        }


        [Test]
        [Category("Type System")]
        public void TS065_doubleToInt_IndexingIntoArray_1467214_2()
        {
            string code =
                @"
                    a = { 1, { 2 }, 3, 4, 5 };
                    x=-0.1;
                    b = a[1][x];";
            string error = "1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("Type System")]
        public void TS066_Int_To_Char_1467119()
        {
            string code =
                @"
                    def foo ( x : char )
                    {
                        return = x;
                    }
                    y = foo (1);
                    ";
            string error = "1467119 - Sprint24 : rev 2807 : Type conversion issue with char  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("Type System")]
        public void TS067_string_To_Char_1467119_2()
        {
            string code =
                @"
                    def foo ( x : char )
                    {
                        return = x;
                    }
                    y = foo (""1"");
                    ";
            string error = "1467119 - Sprint24 : rev 2807 : Type conversion issue with char  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("Type System")]
        public void TS068_Param_singleton_AlltypeTo_BoolArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo ( x:bool[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo( 1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo(A.A() );
                        e = foo( false );
                        f = foo( null );";
            // string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            string error = "1467306 - Sprint 26 - Rev 3784 - string to bool conversion does not happen in function arguments ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true });
            thisTest.Verify("a1", new object[] { true });
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("d", new object[] { true });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS069_Return_singleton_AlltypeTo_BoolArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo:bool[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo( 1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo( 1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo(A.A() );
                        e = foo( false );
                        f = foo( null );";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true });
            thisTest.Verify("a1", new object[] { true });
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("c1", new object[] { true });
            thisTest.Verify("d", new object[] { true });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS070_Param_singleton_AlltypeTo_StringArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo ( x:string[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( A.A() );
                        e = foo(false);
                        f = foo( null );";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("c1", new object[] { "1" });
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS071_return_singleton_AlltypeTo_StringArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo :string[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( A.A() );
                        e = foo(false);
                        f = foo( null );";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("c1", new object[] { "1" });
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS072_Param_singleton_AlltypeTo_CharArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo ( x:char[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( A.A() );
                        e = foo(false);
                        f = foo( null );";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", new object[] { '1' });
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS073_return_singleton_AlltypeTo_CharArray()
        {
            string code =
                @"
                    class A{ a=1; }
                        def foo :char[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( A.A() );
                        e = foo(false);
                        f = foo( null );";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", new object[] { '1' });
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS074_Param_singleton_AlltypeTo_UserDefinedArray()
        {
            string code =
                @"
                    class A{ a=1; }
                    class B extends A{ b=2; }
                        def foo ( x:A[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( B.B() );
                        d1 = d.b;
                        e = foo(false);
                        f = foo( null );";
            string error = "1467314 - Sprint 26 - Rev 3805 user defined type to array of user defined type does not upgrade to array ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);

            thisTest.Verify("d1", new object[] { 2 });
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS075_return_singleton_AlltypeTo_UserDefinedArray()
        {
            string code =
                @"
                      class B extends A{ b=2; }
                        class A{ a=1; }
                        def foo :A[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( B.B() );
                        d1 = d.b;
                        e = foo(false);
                        f = foo( null );";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("d1", new object[] { 2 });
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("Type System")]
        public void TS076_UserDefinedCovariance_ArrayPromotion()
        {
            string code =
                @" class A
                    {
	                        a:int;
	                        constructor A (b:int)
	                        {
		                        a=b;
                            }
                    }
                    class B extends A
                    {
	                        b:int;
	                        constructor B (c:int)
	                        {
    		                    b=c;
                            }
                    }
                   
                   
                    a:A[]=A.A(1);
                    a1=a.a;
                    b:A[]=B.B(2);
                    b1=b.b;
                    b2=b.a;
                    c:B[]=A.A(3);
                    c1=c.b;
                    c2=c.a;
                    ";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a1", new object[] { 1 });
            thisTest.Verify("b1", new object[] { 2 });
            thisTest.Verify("b2", new object[] { 0 });
            thisTest.Verify("c", null);
            thisTest.Verify("c1", null);
            thisTest.Verify("c2", null);
        }

        [Test]
        [Category("Type System")]
        public void TS077_userdefinedTobool_1467240_Imperative()
        {
            string code =
                @"class A
                    {
	                    a:int;
	                    constructor A (b:int)
	                    {
		                    a=b;
                        }
                    }
d;
                    [Imperative]{
                         d:bool=A.A(5); // user def to bool - > if not null true
                    }";
            string error = "1467287 Sprint 26 - 3721 user defined to bool conversion does not happen in imperative ";
            thisTest.RunScriptSource(code, error);
            //Assert.Fail("1467240 - Sprint 26 - Rev 3426 user defined type not convertible to bool");
            thisTest.Verify("d", true);
        }

        [Test]
        [Category("Type System")]
        public void TS078_userdefinedToUserdefinedArray()
        {
            string code =
                @"class A
                    {
                                a:int;
                                constructor A (b:int)
                                {
                                        a=b;
                            }
                    }
                        a : A[] =  A.A(1) ;
                        a1 = a.a;";
            string error = "";
            thisTest.RunScriptSource(code, error);
            //Assert.Fail("1467240 - Sprint 26 - Rev 3426 user defined type not convertible to bool");
            thisTest.Verify("a1", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS079_typedassignment_nullTo_Anyarray_1467295()
        {
            string code =
                @"
               class A{};
                    a :double[]=null;
                    b :string[]=null;
                    c :char[]=null;
                    d :A[]=null;
                    e :bool[]=null;
                    g :int[]=null;";
            string error = "1467295 - Sprint 26 : rev 3766 null gets converted into an array of nulls (while converting into array of any type) when the conversion is not allowed ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array()
        {
            string code =
                    @"
                    class A{
                       def foo(x:int[])
                       {
                             return = x;
                       }
                       }
                    s = A.A();
                    r = s.foo(1);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_2()
        {
            string code =
                    @"
                    class A{
                    def foo(x:int[][])
                    {
                        return = x;
                    }
                    }
                    s = A.A();
                    r = s.foo(1);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { new object[] { 1 } });
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_5()
        {
            string code =
                    @"
                    class A{
                    def foo:int[][](x)
                    {
                        return = x;
                    }
                    }
                    s = A.A();
                    r = s.foo(1);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { new object[] { 1 } });
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_3()
        {
            string code =
                    @"
                    class A{
                    def foo (x:var[][])
                    {
                        return = x;
                    }
                    }
                    s = A.A();
                    r = s.foo(1);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { new object[] { 1 } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_4()
        {
            string code =
                    @"
                    class A {
                    def foo : var[](x:var[][])
                    {
                        return = x;
                    }
                    }
                    s = A.A();
                    r = s.foo(1);";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4172
            string error = "MAGN-4172: What is the proper coercion strategy for this?";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_6()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : int[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B());
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
            thisTest.Verify("t", new object[] { 1 });
            thisTest.Verify("u", new object[] { 1 });
            thisTest.Verify("v", null);
            thisTest.Verify("w", null);
            thisTest.Verify("x", null);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_7()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : var[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B()).b1;
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
            thisTest.Verify("t", new object[] { 1.0 });
            thisTest.Verify("u", new object[] { 1 });
            thisTest.Verify("v", new object[] { true });
            thisTest.Verify("w", new object[] { "1" });
            thisTest.Verify("x", new object[] { '1' });
            thisTest.Verify("y", new object[] { 0 });
            thisTest.Verify("z", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_8()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : double[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B());
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1.0 });
            thisTest.Verify("t", new object[] { 1.0 });
            thisTest.Verify("u", new object[] { 1.0 });
            thisTest.Verify("v", null);
            thisTest.Verify("w", null);
            thisTest.Verify("x", null);
            thisTest.Verify("y", null);
            thisTest.Verify("z", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_9()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : bool[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B());
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { true });
            thisTest.Verify("t", new object[] { true });
            thisTest.Verify("u", new object[] { true });
            thisTest.Verify("v", new object[] { true });
            thisTest.Verify("w", new object[] { true });
            thisTest.Verify("x", new object[] { true });
            thisTest.Verify("y", new object[] { true });
            thisTest.Verify("z", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_10()
        {
            string code =
                    @"
                        class A
                        {
                            x : var[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);";
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_11()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : string[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B());
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            thisTest.Verify("t", null);
            thisTest.Verify("u", null);
            thisTest.Verify("v", null);
            thisTest.Verify("w", new object[] { "1" });
            thisTest.Verify("x", new object[] { "1" });
            thisTest.Verify("y", null);
            thisTest.Verify("z", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_Defect_1467235_coercion_from_singleton_array_12()
        {
            string code =
                    @"
                        class B{b1=0;}
                        class A
                        {
                            x : char[];
                            def foo (y)
                            {
    	                        x = y;
                                return = x;
                            }
                        }
                        s = A.A();
                        r = s.foo(1);
                        t = s.foo(1.0);
                        u1:var= 1;
                        u = s.foo(u1);
                        v = s.foo(true);
                        w = s.foo(""1"");
                        x = s.foo('1');
                        y = s.foo(B.B());
                        z = s.foo(null);"
                    ;
            string error = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            thisTest.Verify("t", null);
            thisTest.Verify("u", null);
            thisTest.Verify("v", null);
            thisTest.Verify("w", null);
            thisTest.Verify("x", new object[] { '1' });
            thisTest.Verify("y", null);
            thisTest.Verify("z", null);
        }

        [Test]
        [Category("Type System")]
        public void TZ01_1467320_single_To_Dynamicarray()
        {
            string code =
                    @"
              class A
                    {
                        x:int[] = { };
	                    constructor A ( y : int )
	                    {
                        x =  y ;
	                    }
                    }
                    c = A.A(0);
                    d = c.x;";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("d", new object[] { 0 });
        }


        [Test]
        [Category("Type System")]
        public void TZ01_1467320_single_To_Dynamicarray_2()
        {
            string code =
                    @"
              class A
                    {
                        x = { };
	                    constructor A ( y : int )
	                    {
                        x =  {y,y+1} ;
	                    }
                    }
                    c = A.A(0);
                    d = c.x;";
            string error = "1467320 Sprint 27 - Rev 3873 ,Upgrade to array does not happen if the member property define as dynamic array and single value is assigned ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", new object[] { 0, 1 });
        }


        [Test]
        [Category("Type System")]
        public void TZ01_1467320_single_To_Dynamicarray_3()
        {
            string code =
                    @"
              class A
                    {
                        x = { };
	                    constructor A ( y : int )
	                    {
                        x =  {y,{y+1}} ;
	                    }
                    }
                    c = A.A(0);
                    d = c.x;";
            string error = "1467320 Sprint 27 - Rev 3873 ,Upgrade to array does not happen if the member property define as dynamic array and single value is assigned ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", new object[] { 0, new object[] { 1 } });
        }

        [Test]
        [Category("Type System")]
        public void TS080_string_To_Bool_1467306()
        {
            string code =
                    @"
                        def foo(x:bool)
                        {
                            b1 = x;
                            return = b1;
    
                        }
                        c = foo(""1.5"");
                        d = foo("""");";
            string error = "1467306 - Sprint 26 - Rev 3784 - string to bool conversion does not happen in function arguments ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("c", true);
            thisTest.Verify("d", false);
        }

        [Test]
        [Category("Type System")]
        public void TS081_Userdefined_To_single_1467308()
        {
            string code =
                    @"
                    class A
                    {
                        a : int;
                        constructor A(b : int[])
                        {
                            a = b;
                        }
                    }
e;
                    [Imperative]
                    {
                        d = A.A(5);
                        e = d.a;
                    }
                    ";
            string error = "1467308 - Sprint 26 - Rev 3786 - userdefined type array to singleton conversion must return null since conversion not possible ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("e", null);
        }

        [Test]
        [Category("Type System")]
        public void TS081_Userdefined_To_single_1467308_2()
        {
            string code =
                    @"
                    class A
                    {
                        a : int[];
                        constructor A(b : int)
                        {
                            a = b;
                        }
                    }
e;
                    [Imperative]
                    {
                        d = A.A(5);
                        e = d.a;
                    }
                    ";
            string error = "1467308 - Sprint 26 - Rev 3786 - userdefined type array to singleton conversion must return null since conversion not possible ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("e", new object[] { 5 });
        }

        [Test]
        [Category("Type System")]
        public void TS081_Userdefined_To_single_1467308_3()
        {
            string code =
                    @"
                    class A
                    {
                        a : int;
                        constructor A(b : int[]..[])
                        {
                            a = b;
                        }
                    }
e;
                    [Imperative]
                    {
                        d = A.A(5);
                        e = d.a;
                    }
                    ";
            string error = "1467308 - Sprint 26 - Rev 3786 - userdefined type array to singleton conversion must return null since conversion not possible ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("e", 5);
        }

        [Test]
        [Category("Type System")]
        public void TS081_Userdefined_To_single_1467308_4()
        {
            string code =
                    @"
                    class A
                    {
                        a : int;
                        constructor A(b : int[]..[])
                        {
                            a = b;
                        }
                    }
e;
                    [Imperative]
                    {
                        d = A.A(5);
                        e = d.a;
                    }
                    ";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("e", 5);
        }


        [Test]
        [Category("Type System")]
        public void TS082_return_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{ a=1; }
                        def foo:bool[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                       
                        d = foo(A.A() );
                    ";
            string error = "147310 - Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", new object[] { true });
        }

        [Test]
        [Category("Type System")]
        public void TS082_Param_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{ a=1; }
                        def foo( x:bool[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                       
                        d = foo(A.A() );
                    ";
            string error = "1467310 -Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", new object[] { true });
        }

        [Test]
        [Category("Type System")]
        public void TS082_assign_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{  }
                    v = {A.A(), A.A()};
                    b:bool[] = v;
                    ";
            string error = "147310 - Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", new object[] { true, true });
        }

        [Test]
        [Category("Type System")]
        public void TS082_assign2_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{  }
                    v = A.A();
                    b:bool[] = v;
                    ";
            string error = "147310 - Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", new object[] { true });
        }

        [Test]
        [Category("Type System")]
        public void TS082_dispatch2_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{  }
                    def foo(b : bool[]) { return = 1; }
                    v = {A.A(), A.A()};
                    o = foo(v);
                    ";
            string error = "147310 - Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("o", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS082_dispatch3_userdefined_To_BoolArray_1467310()
        {
            string code =
                    @"
                    class A{  }
                    def foo(b : bool[]) { return = 1; }
                    v = A.A();
                    o = foo(v);
                    ";
            string error = "147310 - Sprint 26 - Rev 3786 user defined to bool array - array conversion does not happen ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("o", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS083_Param_char_To_String_1467311()
        {
            string code =
                    @"
                    def foo ( x:string)
	                {
    	                b1= x ;
                        return =b1;
                    }
                    c1 = foo( '1');// char to string 
                    ";
            string error = "1467311 - Sprint 26 - Rev 3788 - Char to String conversion not happening ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("c1", "1");
        }

        [Test]
        [Category("Type System")]
        public void TS083_Param_char_To_StringArray_1467311()
        {
            string code =
                    @"
                    def foo ( x:string[])
	                {
    	                b1= x ;
                        return =b1;
                    }
                    c1 = foo( '1');// char to string 
                    ";
            string error = "1467311 - Sprint 26 - Rev 3788 - Char to String conversion not happening ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("c1", new object[] { "1" });
        }

        [Test]
        [Category("Type System")]
        public void TS084_Param_UserDefine_To_UserDefinedArray_1467314()
        {
            string code =
                    @"
                    class A{ a = 1; }
                        def foo : A[](x )
                        {
                            b1 = x;
                            return = b1;
    
                        }
                        d = foo(A.A());
                        d1 = d.a;
                    ";
            string error = "1467314 Sprint 26 - Rev 3805 user defined type to array of user defined type does not upgrade to array ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d1", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS085_return_UserDefine_To_UserDefinedArray_1467314()
        {
            string code =
                    @"
                    class A{ a = 1; }
                        def foo(x:A[] )
                        {
                            b1 = x;
                            return = b1;
    
                        }
                        d = foo(A.A());
                        d1 = d.a;
                    ";
            string error = "1467314 Sprint 26 - Rev 3805 user defined type to array of user defined type does not upgrade to array ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d1", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS086_param_null_Array_replication_1467316()
        {
            string code =
                    @"
                    def foo ( x : int[])
                        {
                            return = x;
                        }
                        a1 = { null, 5, 6.0};
                        b1 = foo ( a1 );
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1667
            string error = "MAGN-1667 Sprint 26 - Rev 3831 function arguments - if the first argument in an array is null it replicates, when not expected";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a1", new object[] { null, 5, 6 });
        }

        [Test]
        [Category("Type System")]
        public void TS087_arrayUpgrade_function_arguments_1457470()
        {
            string code =
                    @"
                    class A{
                        x : int[];
                        constructor A(i : int[])
                        {
                            x = i;
                        }
                    }
                    a1 = A.A(1);
                    b1 = a1.x;
                    ";
            string error = "1467316 - Sprint 26 - Rev 3831 function arguments - if the first argument in an array is null it replicates, when not expected";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b1", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS088_arrayUpgrade_function_return()
        {
            string code =
                    @"
                    def A : int[](y : int)
	                {
		                x = { y, { y + 1 } };
		                return = x; 
	                }
	                c = A(0);
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { 0, null });
        }

        [Test]
        [Category("Type System")]
        public void TS089_arrayUpgrade_function_return_intArray_rankmismtach()
        {
            string code =
                    @"
                    def A : int[](y : int)
	                {
		                x = { y, { y + 1 } };
		                return = x; 
	                }
	                c = A(0);
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { 0, null });
        }

        [Test]
        [Category("Type System")]
        public void TS090_arrayUpgrade_function_return_doubleArray_rankmismtach()
        {
            string code =
                    @"
                    def A : double[](y : int)
	                {
		                x = { y, { y + 1 } };
		                return = x; 
	                }
	                c = A(0);
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { 0.0, null });
        }

        [Test]
        [Category("Type System")]
        public void TS091_arrayUpgrade_function_return_boolArray_rankmismtach()
        {
            string code =
                    @"
                    def A : bool[](y : int)
	                {
		                x = { y, { y + 1 } };
		                return = x; 
	                }
	                c = A(0);
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { false, null });
        }

        [Test]
        [Category("Type System")]
        public void TS092_arrayUpgrade_function_return_stringArray_rankmismtach()
        {
            string code =
                    @"
                    def A : string[](y : int)
	                {
		                x = { y, { y + 1 } };
		                return = x; 
	                }
	                c = A(0);
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS093_Param_notypedefined_indexing_Userdefined()
        {
            string code =
                    @"
                    class A{ a = 1234;}
                    class B{
                        b : A;
                        constructor B(x : A, y : A)
                        {
                            b = x;
                        }
                    }
                    points = { A.A(), A.A() };
                    def CreateLine(points: var[] )
                    {
                        return = B.B(points[0], points[1]);
                    }
                    test = CreateLine(points);
                    z=test.b.a;
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", 1234);
        }

        [Test]
        [Category("Type System")]
        public void TS094_Param_notypedefined_single_Userdefined()
        {
            string code =
                    @"
                    class A{ a=0; }
                        class B{
                            b : A;
                            constructor B(x : A)
                            {
                                b = x;
                            }
                        }
                        points = A.A();
                        def CreateLine(points )
                        {
                            return = B.B(points);
                        }
                        test = CreateLine(points);
                        z = test.b.a;
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", 0);
        }

        [Test]
        [Category("Type System")]
        public void TS094_Class_member_single_ToDynamicArray_Userdefined_1467320()
        {
            string code =
                    @"
                    class A
                        {
                            x = { };
                                constructor A ( y : int )
                                {
                            x =  y ;
                                }
                        }
                        c = A.A(0);
                        d = c.x;
                    ";
            //string error = "1467320 - Sprint 27 - Rev 3873 ,Upgrade to array does not happen if the member property define as dynamic array and single value is assigned ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", 0);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0120_typedassignment_To_Jagged_Vararray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:var[]..[]= {1,{2,3},{{4},3}}; 
                    
                    b:var[]..[] =  {1.1,{2.2,3.3}}; 
                    c:var[]..[]={""a"",{""a""}}; 
                    d:var[]..[]= {'c',{'c'}};
                    x1= test.test();
                    e:var[]..[]= {x1,{x1}};
                    e1=e.x;
                    f:var[]..[]= {true,{true}};
                    g :var[]..[]={null,{null}};";

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1701
            string error = "MAGN-1701 Regression : Dot operation on jagged arrays is giving unexpected null";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { new object[] { 4 }, 3 } });
            thisTest.Verify("b", new object[] { 1.1, new object[] { 2.2, 3.3 } });
            thisTest.Verify("c", new object[] { "a", new object[] { "a" } });
            thisTest.Verify("d", new object[] { 'c', new object[] { 'c' } });
            thisTest.Verify("e1", new object[] { 1, new object[] { 1 } });
            thisTest.Verify("f", new object[] { true, new object[] { true } });
            thisTest.Verify("g", new object[] { null, new object[] { null } });
        }


        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0121_typedassignment_To_Jagged_Intarray()
        {
            string code =
                @"
                class test
                    {
                        x=1;
                    }
                    a:int[]..[]= {1,{2,{3}}}; 
                    
                    b:int[]..[] =  {1.1,{1.1}}; 
                    c:int[]..[]={""a"",{""a""}}; 
                    d:int[]..[]= {'c',{'c'}};
                    x1= test.test();
                    e:int[]..[]= {x1,{x1}};
                    e1=e.x;
                    f:int[]..[]= {true,{true}};
                    g :int[]..[]={null,{null}};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1701
            string error = "MAGN-1701 Regression : Dot operation on jagged arrays is giving unexpected null";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, new object[] { 3 } } });
            thisTest.Verify("b", new object[] { 1, new object[] { 1 } });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0122_typedassignment_To_Jagged_doublearray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:double[]= {1,{1}}; 
                    
                    b:double[] =  {1.1,{-3}}; 
                    c:double[]={""a"",{""ds""}}; 
                    d:double[]= {'c',{'1'}};
                    x1= test.test();
                    e:double[]= {x1,{x1}};
                    e1=e.x;
                    f:double[]= {true,{true}};
                    g :double[]={null,{null}};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";

            thisTest.VerifyRunScriptSource(code, error);
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.0, null });
            thisTest.Verify("b", new object[] { 1.1, null });
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0123_typedassignment_To_Jagged_boolarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:bool[]..[]= {1,{1}}; 
                    
                    b:bool[] ..[]=  {1.1,{1.1}}; 
                    c:bool[]..[]={""a"",{""a""}}; 
                    d:bool[]..[]= {'c',{'c'}};
                    x1= test.test();
                    e:bool[]..[]= {x1,{x1}};
                    e1=e.x;
                    f:bool[]..[]= {true,{true}};
                    g :bool[]..[]={null,{null}};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, new object[] { true } });
            thisTest.Verify("b", new object[] { true, new object[] { true } });
            thisTest.Verify("c", new object[] { true, new object[] { true } });
            thisTest.Verify("d", new object[] { true, new object[] { true } });
            thisTest.Verify("e", new object[] { true, new object[] { true } });
            thisTest.Verify("f", new object[] { true, new object[] { true } });
            thisTest.Verify("g", null);
        }


        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0124_typedassignment_To_Jagged_stringarray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:string[]..[]= {1,{1}}; 
                    
                    b:string[] ..[]=  {1.0,{1.0}}; 
                    c:string[]..[]={""test"",{""test""}}; 
                    d:string[]..[]= {'1',{'1'}};
                    x1= test.test();
                    e:string[]..[]= {x1,{x1}};
                    e1=e.x;
                    f:string[]..[]= {false,{true}};
                    g :string[]..[]={null,{null}};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { "test", new object[] { "test" } });
            thisTest.Verify("d", new object[] { "1", new object[] { "1" } });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0125_typedassignment_To_Jagged_chararray()
        {
            string code =
                @"
               class test
                    {
                        x=1;
                    }
                    a:char[]..[]= {1,{1}}; 
                    
                    b:char[]..[] =  {1.0,{1.0}}; 
                    c:char[]..[]={""test"",{""test""}}; 
                    d:char[]..[]= {'1',{'1'}};
                    x1= test.test();
                    e:char[]..[]= {x1,{x1}};
                    e1=e.x;
                    f:char[]..[]= {false,{false}};
                    g :char[]..[]={null,{null}};";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", new object[] { "1", new object[] { "1" } });
            thisTest.Verify("e1", null);
            thisTest.Verify("f", null);
            thisTest.Verify("g", null);
        }

        [Test]
        [Category("Type System")]
        public void arrayRankmismtach_function_Param_1467326()
        {
            string code =
                @"
                    def foo (x: var[][] )
                        {
                            return = x;
    
                        }
                        z = foo({  3  });
                    ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "z", new object[] { new object[] { 3 } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void arrayRankmismtach_function_Return_1467326()
        {
            string code =
                @"
                    def foo : var[][](x )
                        {
                            return = x;
    
                        }
                        z = foo({  3  });
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668: when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "z", new object[] { new object[] { 3 } });
        }

        [Test]
        [Category("Type System")]
        public void TS126_Param_eachType_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { 3 } });
            TestFrameWork.Verify(mirror, "h1", new object[] { new object[] { 0 } });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS126_Return_eachType_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : var[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                         h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { new object[] { 3 } } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { new object[] { 3.0 } } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { new object[] { "1.5" } } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { new object[] { '1' } } });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { new object[] { 3 } } });
            TestFrameWork.Verify(mirror, "h1", new object[] { new object[] { new object[] { 0 } } });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS127_Param_eachType_Array_To_IntArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: int[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 3 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { 3 } });
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS128_Return_eachType_Array_To_IntArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : int[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3.0;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668: Dot Operation on instances using replication returns single null where multiple nulls are expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { new object[] { 3 } } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { new object[] { 3 } } });
            TestFrameWork.Verify(mirror, "c", new object[] { null });
            TestFrameWork.Verify(mirror, "d", new object[] { null });
            TestFrameWork.Verify(mirror, "e", new object[] { null });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { new object[] { 3 } } });
            TestFrameWork.Verify(mirror, "h1", new object[] { null });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS129_Param_eachType_Array_To_DoubleArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: double[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3.0;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS130_Return_eachType_Array_To_DoubleArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : double[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668: Dot Operation on instances using replication returns single null where multiple nulls are expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { new object[] { 3.0 } } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { new object[] { 3.0 } } });
            TestFrameWork.Verify(mirror, "c", new object[] { null });
            TestFrameWork.Verify(mirror, "d", new object[] { null });
            TestFrameWork.Verify(mirror, "e", new object[] { null });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { new object[] { 3.0 } } });
            TestFrameWork.Verify(mirror, "h1", new object[] { null });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS131_Param_eachType_Array_To_BoolArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: bool[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3.0;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "h", new object[] { new object[] { true } });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS132_Return_eachType_Array_To_BoolArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : bool[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "f1", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "h", new object[] { new object[] { new object[] { true } } });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS133_Param_eachType_Array_To_StringArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: string[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3.0;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { "1" } });
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS134_Return_eachType_Array_To_StringArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : string[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668: Dot Operation on instances using replication returns single null where multiple nulls are expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { null });
            TestFrameWork.Verify(mirror, "b", new object[] { null });
            TestFrameWork.Verify(mirror, "c", new object[] { null });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { new object[] { "1.5" } } });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { new object[] { "1" } } });
            TestFrameWork.Verify(mirror, "f1", new object[] { null });
            TestFrameWork.Verify(mirror, "h1", new object[] { null });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        public void TS135_Param_eachType_Array_To_charArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: char[][])
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3.0;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS136_Return_eachType_Array_To_CharArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo : char[][](x)
                        {
                            return = x;
    
                        }
                        a = foo({ 3 });
                        b = foo({ 3.0 });
                        c = foo({ true });
                        d = foo({ ""1.5""});
                        e = foo({ '1' });
                        f : var = 3;
                        f1 = foo({ f });
                        h = foo({A.A()});
                        h1=h.a;
                        i = foo({ null });
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668:  Dot Operation on instances using replication returns single null where multiple nulls are expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { null });
            TestFrameWork.Verify(mirror, "b", new object[] { null });
            TestFrameWork.Verify(mirror, "c", new object[] { null });
            TestFrameWork.Verify(mirror, "d", new object[] { null });
            TestFrameWork.Verify(mirror, "e", new object[] { new object[] { new object[] { '1' } } });
            TestFrameWork.Verify(mirror, "f1", new object[] { null });
            TestFrameWork.Verify(mirror, "h1", new object[] { null });
            TestFrameWork.Verify(mirror, "i", new object[] { null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0137_Param_eachType_Array_To_Jagged_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var[]..[])
                        {
                            return = x;
    
                        }
                        a = foo({ 3,{3} });
                        b = foo({ 3.0 ,{3.0}});
                        c = foo({ true ,{false}});
                        d = foo({ ""1"",{""1.5""}});
                        e = foo({ '1',{'1'} });
                        f : var = 3;
                        f1 = foo({ f,{f} });
                        h = foo({A.A(),{A.A()}});
                        h1=h.a;
                        i = foo({ null ,{null}});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "d", new object[] { "1.5", new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { '1', new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", new object[] { 3 });
            TestFrameWork.Verify(mirror, "h1", new object[] { 0, new object[] { 0 } });
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0138_Return_eachType_Array_To_Jagged_VarArray()
        {
            string code =
                 @"
                  class A{a=0; }
                      def foo : var[]..[](x)
                      {
                          return = x;
    
                      }
                      a = foo({ 3,{3} });
                      b = foo({ 3.0 ,{3.0}});
                      c = foo({ true ,{false}});
                      d = foo({ ""1"",{""1.5""}});
                      e = foo({ '1',{'1'} });
                      f : var = 3;
                      f1 = foo({ f,{f} });
                      h = foo({A.A(),{A.A()}});
                      h1=h.a;
                      i = foo({ null ,{null}});
                  ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1671
            string error = "MAGN-1671 Sprint 27 - Rev 3966 - when return type is arbitrary rank , type conversion reduces array rank , where it is not expected to";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "d", new object[] { "1.5", new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { '1', new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", new object[] { 3 });
            TestFrameWork.Verify(mirror, "h1", new object[] { 0, new object[] { 0 } });
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS139_Param_eachType_Array_To_jagged_IntArray()
        {
            string code =
                  @"
          class A{a=0; }
                     def foo (x: int[]..[])
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""1""}});
                     e = foo({ '1' ,{'1'}});
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1671
            string error = "MAGN-1671 Sprint 27 - Rev 3966 - when return type is arbitrary rank , type conversion reduces array rank , where it is not expected to";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", null);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS140_Return_eachType_Array_To_Jagged_IntArray()
        {
            string code =
                @"
                 class A{a=0; }
                     def foo : int[]..[](x)
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""1""}});
                     e = foo({ '1' ,{'1'}});
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1671
            string error = "MAGN-1671 Sprint 27 - Rev 3966 - when return type is arbitrary rank , type conversion reduces array rank , where it is not expected to";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, new object[] { 3 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS141_Param_eachType_Array_To_jagged_DoubleArray()
        {
            string code =
                @"
                 class A{a=0; }
                     def foo (x: double[]..[])
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""1""}});
                     e = foo({ '1' ,{'1'}});
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            // string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            string error = "1467334 - Sprint 27 - Rev 3966 - when return type is arbitrary rank , type conversion reduces array rank , where it is not expected to ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS142_Return_eachType_Array_To_Jagged_DoubleArray()
        {
            string code =
                              @"
                 class A{a=0; }
                     def foo (x: double[]..[])
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""1""}});
                     e = foo({ '1' ,{'1'}});
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", null);
            TestFrameWork.Verify(mirror, "f1", new object[] { 3.0, new object[] { 3.0 } });
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS143_Param_eachType_Array_To_Jagged_BoolArray()
        {
            string code =
                @"
                 class A{a=0; }
                     def foo (x: bool[]..[])
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""""}});
                     e = foo({ '1',{'0'} });
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "b", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "c", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "d", new object[] { true, new object[] { false } });
            // TestFrameWork.Verify(mirror, "e", new object[] { true,new object[] { false } });
            TestFrameWork.Verify(mirror, "f", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "h1", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS144_Return_eachType_Array_To_Jagged_BoolArray()
        {
            string code =
                 @"
                 class A{a=0; }
                     def foo : bool[]..[](x)
                     {
                         return = x;
    
                     }
                     a = foo({ 3,{3} });
                     b = foo({ 3.0 ,{3.0}});
                     c = foo({ true ,{false}});
                     d = foo({ ""1.5"",{""""}});
                     e = foo({ '1',{'0'} });
                     f : var = 3.0;
                     f1 = foo({ f ,{f}});
                     h = foo({A.A(),{A.A()}});
                     h1=h.a;
                     i = foo({ null ,{null}});
                 ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1671
            string error = "MAGN-1671 Sprint 27 - Rev 3966 - when return type is arbitrary rank , type conversion reduces array rank , where it is not expected to";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "b", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "c", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "d", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "e", new object[] { true, new object[] { false } });
            TestFrameWork.Verify(mirror, "f", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "h1", new object[] { true, new object[] { true } });
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS147_Param_eachType_Array_To_jagged_StringArray()
        {
            string code =
                 @"
                   class A{a=0; }
                       def foo (x: string[]..[])
                       {
                           return = x;
    
                       }
                       a1 = foo({ 3,0,{3} });
                       b = foo({ 3.0,{0.0} });
                       c = foo({ true ,{true},false});
                       d = foo({ ""1.5"",{""1.5""}});
                       e = foo({ '1',{'1'} });
                       f : var = 3;
                       f1 = foo({ f,{f} });
                       h = foo({A.A(),{A.A()}});
                       h1=h.a;
                       i = foo({ null ,{null}});
                   ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a1", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { "1.5" }, new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { "1", new object[] { "1" } });
            TestFrameWork.Verify(mirror, "f", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS148_Return_eachType_Array_To_jagged_StringArray()
        {
            string code =
                @"
                   class A{a=0; }
                       def foo : string[]..[](x)
                       {
                           return = x;
    
                       }
                       a1 = foo({ 3,{3} });
                       b = foo({ 3.0,{0.0} });
                       c = foo({ true ,{true},false});
                       d = foo({ ""1.5"",{""1.5""}});
                       e = foo({ '1',{'1'} });
                       f : var = 3;
                       f1 = foo({ f,{f} });
                       h = foo({A.A(),{A.A()}});
                       h1=h.a;
                       i = foo({ null ,{null}});
                   ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1670
            string error = "MAGN-1670 Sprint 27 - Rev 3956 {null} to array upgrdation must null out";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a1", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { "1.5" }, new object[] { "1.5" } });
            TestFrameWork.Verify(mirror, "e", new object[] { "1", new object[] { "1" } });
            TestFrameWork.Verify(mirror, "f", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS149_Param_eachType_Array_To_Jagged_charArray()
        {
            string code =
                @"
                  class A{a=0; }
                      def foo (x: char[]..[])
                      {
                          return = x;
    
                      }
                      a1 = foo({ 3,{3} });
                      b = foo({ 3.0 ,{3.0},1.0});
                      c = foo({ true ,{{true},false}});
                      d = foo({ ""1.5"",{""1.5""}});
                      e = foo({ '1',{'1'} });
                      f : var = 3;
                      f1 = foo({ f ,{f}});
                      h = foo({A.A(),{A.A()}});
                      h1=h.a;
                      i = foo({ null ,{null}});
                  ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a1", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", new object[] { '1', new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS150_Return_eachType_Array_To_jagged_CharArray()
        {
            string code =
                @"
                  class A{a=0; }
                      def foo : char[]..[](x)
                      {
                          return = x;
    
                      }
                      a1 = foo({ 3,{3}});
                      b = foo({ 3.0 ,{3.0},1.0});
                      c = foo({ true,{{true},false}});
                      d = foo({ ""1.5"",{""1.5""}});
                      e = foo({ '1',{'1'} });
                      f : var = 3;
                      f1 = foo({ f,{f}});
                      h = foo({A.A(),{A.A()}});
                      h1=h.a;
                      i = foo({ null,{null}});
                  ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a1", null);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", null);
            TestFrameWork.Verify(mirror, "d", null);
            TestFrameWork.Verify(mirror, "e", new object[] { '1', new object[] { '1' } });
            TestFrameWork.Verify(mirror, "f1", null);
            TestFrameWork.Verify(mirror, "h1", null);
            TestFrameWork.Verify(mirror, "i", new object[] { null, new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS0151_Param_eachType_Heterogenous_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var[]..[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A().a});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A().a,null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3.0, true, "1", '1', 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3.0, true, "1", '1', 3, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3.0, true, "1", '1', 3, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3.0, true, "1", '1', 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3.0, true, "1", '1', 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0152_NullingOutTest()
        {
            string code =
                @"
class A{
    a : int;
    constructor A(b : int[])
    {
        a = b;
    }
}
    d = A.A(5);
e = d.a;
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", null);
        }

        [Test]
        [Category("Type System")]
        public void TS0152_NullingOutTest_a()
        {
            string code =
                @"
class A{
    a : int;
    constructor A(b : int[])
    {
        a = b;
    }
}
e;
[Imperative]
{
    d = A.A(5);
    e = d.a;
}
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", null);
        }

        [Test]
        [Category("Type System")]
        public void TS0152_NullingOutTest_b()
        {
            string code =
                @"
class A{
    a : int[];
    constructor A(b : int[])
    {
        a = b;
    }
}
e;
[Imperative]
{
    d = A.A(5);
    e = d.a;
}
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", new object[] { 5 });
        }

        [Test]
        [Category("Type System")]
        public void TS0152_NullingOutTest_c()
        {
            string code =
                @"
class A{
    a : int;
    constructor A(b : int[])
    {
        a = b;
    }
    def foo()
    {
        return=a;
    }
}
    d = A.A(5);
e = d.foo();
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", null);
        }

        [Test]
        [Category("Type System")]
        public void TS0152_Defect_Analysis()
        {
            string code =
                @"
def foo(a : var[]..[]) { return = 1; }
def foo(a : int) { return = 2; }
e = foo({5});
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS0152_Defect_Analysis_a()
        {
            string code =
                @"
def foo(a : var[]..[]) { return = 1; }
def foo(a : double) { return = 2; }
e = foo({5});
                    ";
            string error = "DNL-1467307 Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "e", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS0160_Param_eachType_Heterogenous_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A().a});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A().a,null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3.0, true, "1", '1', 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3.0, true, "1", '1', 3, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3.0, true, "1", '1', 3, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3.0, true, "1", '1', 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3.0, true, "1", '1', 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS161_Param_eachType_Heterogenous_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A().a});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A().a,null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3.0, true, "1", '1', 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3.0, true, "1", '1', 3, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3.0, true, "1", '1', 3, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3.0, true, "1", '1', 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3.0, true, "1", '1', 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0162_Param_ArrayReduction_varArray_Var()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A().a});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A().a,null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3.0, true, "1", '1', 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3.0, true, "1", '1', 3, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3.0, true, "1", '1', 3, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3.0, true, "1", '1', 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3.0, true, "1", '1', 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0163_Param_ArrayReduction_heterogenous_To_Int()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: int)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3, null, null, null, 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3, null, null, null, 3, null });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3, null, null, null, 3, null, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3, null, null, null, 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3, null, null, null, 3 });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0164_Param_ArrayReduction_heterogenous_To_double()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: double)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                        f =foo({ true,""1"",3,3.0,'1',f,null});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1672
            string error = "MAGN-1672 Sprint 27 - Rev 4011 function does not replicate when hetrogenous array with type mismatch are passed as argument";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3.0, 3.0, null, null, null, 3.0 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, 3.0, null, null, null, 3.0, null });
            TestFrameWork.Verify(mirror, "c", new object[] { 3.0, 3.0, null, null, null, 3.0, null, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3.0, 3.0, null, null, null, 3.0, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3.0, 3.0, null, null, null, 3.0 });
            TestFrameWork.Verify(mirror, "f", new object[] { null, null, 3.0, 3.0, null, 3.0, null });
        }

        [Test]
        [Category("Type System")]
        public void TS0165_Param_ArrayReduction_heterogenous_To_bool()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: bool)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "1467326 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { true, true, true, true, true, true });
            TestFrameWork.Verify(mirror, "b", new object[] { true, true, true, true, true, true, true });
            TestFrameWork.Verify(mirror, "c", new object[] { true, true, true, true, true, true, true, null });
            TestFrameWork.Verify(mirror, "d", new object[] { true, true, true, true, true, true, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, true, true, true, true, true, true });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0166_Param_ArrayReduction_heterogenous_To_UserDefined()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: A)
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        b1 = {b[0] ,b[1],b[2] ,b[3],b[4],b[5],A.A().a};
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        c1 = {c[0] ,c[1],c[2] ,c[3],c[4],c[5],A.A().a,c[7]};
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1679
            string error = "MAGN-1679 Sprint 27 - rev 4184 - when heterogenous array is passed and the type is user defined , it does not replicate unless the first item is user defined";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { null, null, null, null, null, null });
            TestFrameWork.Verify(mirror, "b", new object[] { null, null, null, null, null, null, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { null, null, null, null, null, null, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { null, null, null, null, null, null, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, null, null, null, null, null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS0170_typeconversion_replication_oneargument()
        {
            string code =
                @"
                    def foo(x : int, y : int[])
                        {
                            return = x+y;
                        }
                        b1 = foo({ 1, 2 }, { 1, 2 });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0171_typeconversion_replication_botharguments()
        {
            string code =
                @"
                    def foo(x : int, y : int[])
                        {
                            return = x+y;
                        }
                        b1 = foo({ 1, 2 }, { {1, 2 },{2,4}});
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0171_typeconversion_replication_botharguments_2()
        {
            string code =
                @"
                   def foo(x : int, y : int[])
                        {
                            return = x + y;
                        }
                        a = foo(1, { 1, 2 } );
                        b = foo({ 1, 2 }, { { 1, 2 }, { 2, 4 } });
                        c = foo({ 1, 2 }, { 1, 2 });
                        d = foo(1, { { 1, 2 }, { 2, 4 } });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 2, 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { 2, 3 }, new object[] { 3, 5 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0171_typeconversion_replication_botharguments_3()
        {
            string code =
                @"
                   class test
                    {
                        def foo:int[](x : int, y : double[])
                        {
                            return = x + y;
                        }
                    }
                        z= test.test();
                        a = z.foo(1, { 1, 2 } );
                        b = z.foo({ 1, 2 }, { { 1, 2 }, { 2, 4 } });
                        c = z.foo({ 1, 2 }, { 1, 2 });
                        d = z.foo(1, { { 1, 2 }, { 2, 4 } });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 2, 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { 2, 3 }, new object[] { 3, 5 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0171_typeconversion_replication_botharguments_4()
        {
            string code =
                @"
                   class test
                    {
                        def foo(x, y:var[] )
                        {
                            return = x + y;
                        }
                    }
                        z= test.test();
                        a = z.foo(1, { 1, 2 } );
                        b = z.foo({ 1, 2 }, { { 1, 2 }, { 2, 4 } });
                        c = z.foo({ 1, 2 }, { 1, 2 });
                        d = z.foo(1, { { 1, 2 }, { 2, 4 } });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 2, 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
            TestFrameWork.Verify(mirror, "c", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
            TestFrameWork.Verify(mirror, "d", new object[] { new object[] { 2, 3 }, new object[] { 3, 5 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0172_typeconversion_replication_Var_one_argument()
        {
            string code =
                @"
                    def foo(x : int, y : var[])
                        {
                            return = x+y;
                        }
                        b1 = foo({ 1, 2 }, { 1, 2 });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0173_typeconversion_replication_Var_botharguments()
        {
            string code =
                @"
                    def foo(x : int, y : var[])
                        {
                            return = x+y;
                        }
                        b1 = foo({ 1, 2 }, { {1, 2 },{2,4}});
                    ";
            string error = "1467355 - Sprint 27 Rev 4014 , it replicates with maximum combination where as its expected to zipped ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0174_typeconversion_replication()
        {
            string code =
                @"
                    def foo(x, y : var[])
                        {
                            return = x + y;
                        }
                        b1 = foo({ 1, 2 }, { { 1, 2 }, { 2, 4 } });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0175_typeconversion_replication()
        {
            string code =
                @"
                    def foo(x, y : var[])
                        {
                            return = x + y;
                        }
                        b1 = foo({ 1, 2 }, { { 1, 2 }, { 2, 4 } });
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0176_typeconversion_replication_userdefined()
        {
            string code =
                @"
                    class A{ a = 1; };
                    def foo(x : bool, y : A[])
                    {
                        return = { x, y.a };
                    }
                    b1 = foo({ true, false }, { { A.A(), A.A() }, { A.A(), A.A() } });
                    ";
            string error = "1467355 - Sprint 27 Rev 4014 , it replicates with maximum combination where as its expected to zipped ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { true, new object[] { 1, 1 } }, new object[] { false, new object[] { 1, 1 } } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0177_typeconversion_replication()
        {
            string code =
                @"
                    def foo(x : int)
                    {
                        return = x;
                    }
                    a1 = {  1 , 2 ,  3 , { 4 } }; 
 
                    b1 = foo(a1); // received {1,2,3,null}, expected : {  1 , 2 ,  3 , { 4 } }
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1673
            string error = "MAGN-1673 Sprint 27 - Rev4014 - function argument with jagged array  - its expected to replicate for the attached code";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { 1, 2, 3, new object[] { 4 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0178_typeconversion_replication()
        {
            string code =
                @"
                    def foo(x : var[], y : int)
                        {
                            return = x + y;
                        }
                        b1 = foo({ { 1, 2 }, { 2, 4 } },{ 1, 2 });
                    ";
            string error = "1467355 Sprint 27 Rev 4014 , it replicates with maximum combination where as its expected to zipped ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 4, 6 } });
        }

        [Test]
        [Category("Type System")]
        public void TS0179_typeconversion_replication()
        {
            string code =
                @"
                    def foo(x : var[], y : int)
                        {
                            return = x + y;
                        }
                        b1 = foo({ { 1, 2 }, { 2,3} }, 1);
                    ";

            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }


        [Test]
        [Category("Type System")]
        public void TS0180_typeconversion_replication()
        {
            string code =
                @"
              def foo(x : int[], y : int[])
                    {
                        return =  x + y ;
                    }
                    def foo(x : int, y : int)
                    {
                        return = x - y;
                    }
                    a = foo({ 1, 2 }, 1);
                    b = foo(1, { 1, 2 });
                    c = foo( { 1, 2 }, { 1, 2 } );
                    ";

            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "a", new object[] { 0, 1 });
            TestFrameWork.Verify(mirror, "b", new object[] { 0, -1 });
            TestFrameWork.Verify(mirror, "c", new object[] { 2, 4 });
        }

        [Test]
        [Category("Type System")]
        public void TS0181_typeconversion_replication_class()
        {
            string code =
                @"
              class test
                    {
    
                    def foo(x : int[], y : int[])
                    {
                        return =  x + y ;
                    }
                    def foo(x : int, y : int)
                    {
                        return = x - y;
                    }
                    }
                    z = test.test();
                    a = z.foo({ 1, 2 }, 1);
                    b = z.foo(1, { 1, 2 });
                    c = z.foo( { 1, 2 }, { 1, 2 } );
                    ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "a", new object[] { 0, 1 });
            TestFrameWork.Verify(mirror, "b", new object[] { 0, -1 });
            TestFrameWork.Verify(mirror, "c", new object[] { 2, 4 });
        }

        [Test]
        [Category("Type System")]
        public void TS0182_typeconversion_replication_arithematic()
        {
            string code =
                @"
                 a = { 1 ,2.3};
                 b = { 2 ,3};
                 c = a + b;
                        ";
            string error = "1467359 - Sprint 27 - rev 4017 arithematic operations , the type must be converted higer up in the chain , currently does based on the first item in the array";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 5.3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0183_TypeConversion()
        {
            string code =
                @"
                 a = { 1 ,2.3};
                 b = { 2 ,3};
                 c = a + b;
                        ";
            string error = "1467359 - Sprint 27 - rev 4017 arithematic operations , the type must be converted higer up in the chain , currently does based on the first item in the array";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 5.3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0184_TypeConversion()
        {
            string code =
                @"
                class B
                    {
                    constructor B()
                    {
                    }
                    def foo()
                    {
                    return = 1;
                    }
                    }
                    arr = { 0, B.B() };
                    x = arr[1];
                    test = x.foo();
                                            ";
            string error = "1467359 - Sprint 27 - rev 4017 arithematic operations , the type must be converted higer up in the chain , currently does based on the first item in the array";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "test", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS0184_TypeConversion_1467352()
        {
            string code =
                @"
                class B
                    {
                    constructor B()
                    {
                    }
                    def foo()
                    {
                    return = 1;
                    }
                    }
                    arr = { 0, B.B() };
                    x = arr[1];
                    test = x.foo();
                                            ";
            string error = "1467359 - Sprint 27 - rev 4017 arithematic operations , the type must be converted higer up in the chain , currently does based on the first item in the array";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "test", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS0184_TypeConversion_1467352_2()
        {
            string code =
                @"
               class A
                    {
                        constructor A(i)
                        {        
                        }
                        def bing()
                        {
                            return = 10;
                        }
                    }
                    class B
                    {
                        constructor B()
                        {
                        }
                        def faa()
                        {
                            return = 1;
                        }
                        def foo()
                        {
                            return  = 2;
                        }
                        static def bar()
                        {
                            return = 3;
                        }
                    }
                    arr = { A.A(), 1, { 2, B.B() }, 5 };
                    x = arr[2][1];
                    test = x.faa();
                                            ";
            string error = "1467352 - [Language] Mixed array return type is incorrect";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "test", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS0185_TypeConversion_1467291()
        {
            string code =
                @"
                    a : int[] = { 2, 2, 3 };
                    a[0] = false;
                                            ";
            string error = "1467291 - Assigning a value to a typed array doesn't respect the type ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { null, 2, 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS0186_TypeConversion_replicate()
        {
            string code =
                @"
                   def foo : int[](x : int)
                    {
                        return = x;
                    }
                    a1 = {1.1,2.0,3}; 
                    b1 = foo ( a1 );//received -  b1 = {{1},{2},{3}}
                                            ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b1", new object[] { new object[] { 1 }, new object[] { 2 }, new object[] { 3 } });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0187_TypeConversion_builtin_Sum()
        {
            string code =
                @"
                  sum2 = Sum({ { 2,1,true}, 1 });
                                            ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1675
            string error = "MAGN-1675 Sprint 27 - Rev 4048 function Sum fails if non convertible types are given as input";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "sum2", 4);
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0189_TypeConversion_conditionals()
        {
            string code =
                @"
                  x=2=={ };  
                  y={}==null;
                  z={{1}}=={1};
                  z2={{1}}== 1;
                  z3=1=={{1}};
                  z4={1}=={{1}};
                                            ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3941
            string error = "MAGN-3941 Errors with conditionals with empty arrays and ararys with different ranks";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "x", null);
            TestFrameWork.Verify(mirror, "z", false);
            TestFrameWork.Verify(mirror, "z2", false);
            TestFrameWork.Verify(mirror, "z3", false);
            TestFrameWork.Verify(mirror, "z4", false);
        }

        [Test]
        [Category("Type System")]
        public void TS0189_TypeConversion_conditionals_1465293_1()
        {
            string code =
                @"
                  a=2==null;  
                  b=2!=null;
                  c=null==null;
                  d=null!=null;
                 ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", false);
            TestFrameWork.Verify(mirror, "b", true);
            TestFrameWork.Verify(mirror, "c", true);
            TestFrameWork.Verify(mirror, "d", false);
        }

        [Test]
        [Category("Type System")]
        public void TS0189_TypeConversion_conditionals_1465293_2()
        {
            string code =
                @"
                  a=!2;  
                  b=!null;
                  c=!true;
                  d=!false;
                  e = !0;
                  f = d!= true;
                 ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", false);
            TestFrameWork.Verify(mirror, "b", null);
            TestFrameWork.Verify(mirror, "c", false);
            TestFrameWork.Verify(mirror, "d", true);
            TestFrameWork.Verify(mirror, "e", true);
            TestFrameWork.Verify(mirror, "f", false);
        }

        [Test]
        [Category("Type System")]
        public void TS190_Param_eachType_Heterogenous_Array_To_VarArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: var[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A().a});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A().a,null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            string error = "";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3.0, true, "1", '1', 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3.0, true, "1", '1', 3, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3.0, true, "1", '1', 3, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3.0, true, "1", '1', 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3.0, true, "1", '1', 3 });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0191_Param_ArrayReduction_heterogenous_To_IntArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: int[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1660
            string error = "MAGN-1660 Sprint25: rev 3352: Type conversion - method dispatch over heterogeneous array is not correct";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3, null, null, null, 3 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3, 3, null, null, null, 3, null });
            TestFrameWork.Verify(mirror, "c", new object[] { 3, 3, null, null, null, 3, null, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3, 3, null, null, null, 3, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3, 3, null, null, null, 3 });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0192_Param_ArrayReduction_heterogenous_To_doubleArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: double[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                        f =foo({ true,""1"",3,3.0,'1',f,null});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1660
            string error = "MAGN-1660 Sprint25: rev 3352: Type conversion - method dispatch over heterogeneous array is not correct"; 
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { 3.0, 3.0, null, null, null, 3.0 });
            TestFrameWork.Verify(mirror, "b", new object[] { 3.0, 3.0, null, null, null, 3.0, null });
            TestFrameWork.Verify(mirror, "c", new object[] { 3.0, 3.0, null, null, null, 3.0, null, null });
            TestFrameWork.Verify(mirror, "d", new object[] { 3.0, 3.0, null, null, null, 3.0, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, 3.0, 3.0, null, null, null, 3.0 });
            TestFrameWork.Verify(mirror, "f", new object[] { null, null, 3.0, 3.0, null, 3.0, null });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0193_Param_ArrayReduction_heterogenous_To_boolArray()
        {
            string code =
                @"
                    class A{a=0; }
                        def foo (x: bool[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1660
            string error = "MAGN-1660 Sprint25: rev 3352: Type conversion - method dispatch over heterogeneous array is not correct"; 
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { true, true, true, true, true, true });
            TestFrameWork.Verify(mirror, "b", new object[] { true, true, true, true, true, true, true });
            TestFrameWork.Verify(mirror, "c", new object[] { true, true, true, true, true, true, true, null });
            TestFrameWork.Verify(mirror, "d", new object[] { true, true, true, true, true, true, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, true, true, true, true, true, true });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0194_Param_ArrayReduction_heterogenous_To_UserDefinedArray()
        {
            string code =
                @"
                   class A{a=0; }
                        def foo (x: A[])
                        {
                            return = x;
    
                        }
                        f : var = 3;
                        a = foo({ 3,3.0,true ,""1"",'1',f});
                        b = foo({ 3,3.0,true ,""1"",'1',f,A.A()});
                        b1 = {b[0] ,b[1],b[2] ,b[3],b[4],b[5],A.A().a};
                        c = foo({ 3,3.0,true ,""1"",'1',f,A.A(),null});
                        c1 = {c[0] ,c[1],c[2] ,c[3],c[4],c[5],A.A().a,c[7]};
                        d = foo({ 3,3.0,true ,""1"",'1',f,null});
                        e = foo({ null ,3,3.0,true ,""1"",'1',f});
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1679
            string error = "MAGN-1679 Sprint 27 - rev 4184 - when heterogenous array is passed and the type is user defined , it does not replicate unless the first item is user defined";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "a", new object[] { null, null, null, null, null, null });
            TestFrameWork.Verify(mirror, "b", new object[] { null, null, null, null, null, null, 0 });
            TestFrameWork.Verify(mirror, "c", new object[] { null, null, null, null, null, null, 0, null });
            TestFrameWork.Verify(mirror, "d", new object[] { null, null, null, null, null, null, null });
            TestFrameWork.Verify(mirror, "e", new object[] { null, null, null, null, null, null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS0195_heterogenousarray_To_UserDefinedArray()
        {
            string code =
                @"
                   class A{a=0; }
                   def foo(x : A)
                        {
                            return = x;
    
                        }
                   a = foo({ 1,A.A(), A.A() });
                   b = {null,0,0};
                    ";
            string error = "1467376 - Sprint 27 - rev 4184 - when heterogenous array is passed and the type is user defined , it does not replicate unless the first item is user defined ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b", new object[] { null, 0, 0 });

        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0196_heterogenousarray_To_UserDefinedArray()
        {
            string code =
                @"
                   class A{a=0; }
                   def foo(x : A[])
                        {
                            return = x;
    
                        }
                   a = foo({ 1,A.A(), A.A() });
                   b = {null,a[1].a,a[2].a};
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1679
            string error = "MAGN-1679 Sprint 27 - rev 4184 - when heterogenous array is passed and the type is user defined , it does not replicate unless the first item is user defined";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b", new object[] { null, 0, 0 });
        }

        [Test]
        [Category("Type System")]
        [Category("Failure")]
        public void TS0197_getter_dotoperator_1467419()
        {
            string code =
                @"
                   class A 
                    {
                    x : int[];
                    constructor A( y)
                    {
                    x = y;
                    }
                    }
                    class B extends A 
                    {
                    t : int[];
                    constructor B(y : var[])
                    {
                    x = y;
                    t = x + 1;
                    }
                    }
                    a1 = { B.B(1), { A.A(2), { B.B(0), B.B(1) } } };
                    // received :{{1},null}
                    //expected:{{1},{{0},{1}}}
                    z = a1.x;
                    ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1683
            string error = "MAGN-1683 Sprint 29 - Rev 4502 - the .operator is doing rank reduction where it is expected to replicate";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "z", new object[] { new object[] { 1 }, new object[] { new object[] { 0 }, new object[] { 1 } } });
        }

        [Test]
        [Category("Type System")]
        public void TS0198_method_resolution_1467273()
        {
            string code =
                @"
                   class test{
                        def foo(x : var[]..[])
                        {
                        return = 3;
                        }
                        def foo(x:var[][])
                        {
                        return = 2;
                        }
                        def foo(x:var[])
                        {
                        return = 1;
                        }
                        def foo(x:var)
                        {
                        return = 0;
                        }
                        }
                        a = test.test();
                        u = a.foo({ 1, 2 });
                        v = a.foo({ { 1 }, 2 });
                        w = a.foo({ 1, { 2 } });
                        x = a.foo({ { 1 }, { 2 } });
                        y = a.foo({ { { 1 }, { 2 } } });
                    ";
            string error = "1467419 - Sprint 29 - Rev 4502 - the .operator is doing rank reduction where it is expected to replicate ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "u", 1);
            TestFrameWork.Verify(mirror, "v", 2);
            TestFrameWork.Verify(mirror, "w", 2);
            TestFrameWork.Verify(mirror, "x", 2);
            TestFrameWork.Verify(mirror, "y", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS0198_method_resolution_1467273_2()
        {
            string code =
                @"
                   class test{
                        def foo(x : int[]..[])
                        {
                        return = 3;
                        }
                        def foo(x:double[][])
                        {
                        return = 2;
                        }
                        def foo(x:int[])
                        {
                        return = 1;
                        }
                        def foo(x:var)
                        {
                        return = 0;
                        }
                        }
                        a = test.test();
                        u = a.foo({ 1, 2 });
                        v = a.foo({ { 1 }, 2 });
                        w = a.foo({ 1, { 2 } });
                        x = a.foo({ { 1 }, { 2 } });
                        y = a.foo({ { { 1 }, { 2 } } });
                    ";
            string error = "1467419 - Sprint 29 - Rev 4502 - the .operator is doing rank reduction where it is expected to replicate ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "u", 1);
            TestFrameWork.Verify(mirror, "v", 3);
            TestFrameWork.Verify(mirror, "w", 3);
            TestFrameWork.Verify(mirror, "x", 3);
            TestFrameWork.Verify(mirror, "y", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS0198_method_resolution_1467273_3()
        {
            string code =
                @"
                   class test{
                        def foo(x : bool[]..[])
                        {
                        return = 3;
                        }
                        def foo(x:double[][])
                        {
                        return = 2;
                        }
                        def foo(x:int[])
                        {
                        return = 1;
                        }
                        def foo(x:var)
                        {
                        return = 0;
                        }
                        }
                        a = test.test();
                        u = a.foo({ 1, 2 });
                        v = a.foo({ { 1 }, 2 });
                        w = a.foo({ 1, { 2 } });
                        x = a.foo({ { 1 }, { 2 } });
                        y = a.foo({ { { 1 }, { 2 } } });
                    ";
            string error = "1467419 - Sprint 29 - Rev 4502 - the .operator is doing rank reduction where it is expected to replicate ";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "u", 1);
            TestFrameWork.Verify(mirror, "v", 3);
            TestFrameWork.Verify(mirror, "w", 3);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 1 });
            TestFrameWork.Verify(mirror, "y", 3);
        }

        [Test]
        [Category("Failure")]
        public void replication_1467451()
        {
            string code =
                @"
c = 40;
d = { 1.0+ { { c + 5 }, { c + 5.5 }, { c + 6 } } };// received {46.0,47.00,47.00}
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1690
            string error = "MAGN-1690 Sprint 29 - Rev 4596 , error thrown where not expected";
            var mirror = thisTest.RunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "d", new object[] { 46.0, 47.0, 47.0 });
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void indexintoarray_left_1467462_1()
        {
            string code =
                @"
                x : var[] = { 1, 2, 3, 4 };
                x[2..3] = { 1, 2 };
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_2()
        {
            string code =
                @"
                x : int[] = { 1, 2, 3, 4 };
                x[2..3] = { 1, 2 };
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_3()
        {
            string code =
                @"
                x : int[] = { 1, 2, 3, 4 };
                x[2..3] = { 1, 2 };
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_4()
        {
            string code =
                @"
                x : bool[] = { true, false, true, false };
                x[2..3] = { true, 2 };
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { true, false, true, true });
        }

        [Test]
        public void indexintoarray_left_1467462_5()
        {
            string code =
                @"
                x : var[] = { 1, false,""q"", 1.3 };//jagged
                x[2..3] = { true, 2 };
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, false, true, 2 });
        }

        [Test]
        [Category("Failure")]
        public void indexintoarray_left_1467462_6()
        {
            string code =
                @"
                class A{a=1;}
                class B{a=2;}
                x : var[] = { A.A(),A.A(),A.A(),A.A()};//var 
                x[2..3] = { B.B(),B.B()};
                 y = x.a;
                ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            var mirror = thisTest.VerifyRunScriptSource(code, str);
            TestFrameWork.Verify(mirror, "y", new object[] { 1, 1, 2, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_7()
        {
            string code =
                @"
               class A{ a = 1; }
                class B{ b = 2; }
                x : A[] = { A.A(),A.A(),A.A(),A.A()};//class
                x[2..3] = { B.B(), B.B() };
                y = x.a;
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "y", new object[] { 1, 1, null, null });
        }

        [Test]
        public void TS200_memberproperty_1467486_1()
        {
            string code =
                @"
              class test
                {
                    x3 : double[]..[];
                    def foo()
                    {
                        x3 = {  1.5 , 2.5 };
                        return = x3;
                    }
                }
                z = test.test();
                y=z.foo();
                ";
            string error = "1467486 if a member property is defined as variable array then while assigning value itthrows error could not decide which function to execute";
            var mirror = thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "y", new object[] { 1.5, 2.5 });
            thisTest.VerifyRuntimeWarningCount(0);
        }

        [Test]
        public void TS201_memberproperty_1467486_2()
        {
            string code =
                @"
              class test
                {
                    x3 : int[]..[];
                    def foo()
                    {
                        x3 = {  1 , 2};
                        return = x3;
                    }
                }
                z = test.test();
                y=z.foo();
                ";

            string error = "1467486 if a member property is defined as variable array then while assigning value itthrows error could not decide which function to execute";
            var mirror = thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "y", new object[] { 1, 2 });
            //thisTest.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kAmbiguousMethodDispatch); 
            thisTest.VerifyRuntimeWarningCount(0);
        }

        [Test]
        public void TS202_memberproperty_1467486_3()
        {
            string code =
                @"
             
              class A{ a = 1; }
                class test
                {
                    x3 : A[]..[];
                    def foo()
                    {
                        x3 = {  A.A() , A.A() };
                        return = x3;
                    }
                }
                z = test.test();
                y = z.foo();
                b = y.a;
                ";
            string error = "1467486 if a member property is defined as variable array then while assigning value itthrows error could not decide which function to execute";
            var mirror = thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.Verify(mirror, "b", new object[] { 1, 1 });
            thisTest.VerifyRuntimeWarningCount(0);
        }
    }
}

