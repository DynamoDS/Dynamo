using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    [TestFixture]
    class TypeSystemTestsImperative : ProtoTestBase
    {
        [Test]
        [Category("Type System")]
        public void TS001_IntToDoubleTypeConversion_Imperative()
        {
            string code =
                @"a=i[0];b=i[1];
                i = [Imperative]
                {
                    a = 1;
                    b : double = a;
                    return [a,b];
                }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1.0);
        }

        [Test]
        [Category("Type System")]
        public void TS001a_DoubleToIntTypeConversion_Imperative()
        {
            string code =
                @"a=i[0];b=i[1];c=i[2];d=i[3];e=i[4];
                i=[Imperative]
                {
                    a = 1.0;
                    b : int = a;
                    c : int = 2.1;
                    d : int = 2.5;
                    e : int = 3.0;
                    return [a, b, c, d, e];
                }";
            thisTest.RunScriptSource(code);
            //These should convert and emit warnings
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 3);
            thisTest.Verify("e", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS002_IntToUserDefinedTypeConversion_Imperative()
        {
            string code =
                @"
import(""FFITarget.dll"");
a=i[0];b=i[1];
i=[Imperative]
{                
    a = 1;
    b : ClassFunctionality = a;
    return [a, b];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("Type System")]
        public void TS003IntToChar1467119_Imperative()
        {
            string code =
                @"
                def foo ( x : char )
                {    
                    return = x;
                }
                y = [Imperative]
                {
                    return foo (1);
                }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.MethodResolutionFailure);
        }

        [Test]
        [Category("Type System")]
        public void TS004_IntToChar_1467119_2_Imperative()
        {
            string code =
                @"
                def foo ( x : char )
                {    
                    return = true;
                }
                y = [Imperative]{
                return foo ('1');}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", true);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS005_RetTypeArray_return_Singleton_1467196_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    def length : ClassFunctionality[] (pts : ClassFunctionality[])
                    {
                        return = pts[0];
                    }
                    a = [Imperative]{
                    //return type class and return an array of class-
                   
                        pt1 = ClassFunctionality.ClassFunctionality( );
                        pt2 = ClassFunctionality.ClassFunctionality(  );
                        pts = [pt1, pt2];
                        numpts = length(pts); 
                        a=numpts.IntVal;
                        return a;
                    }";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null); 
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS006_RetTypeuserdefinedArray_return_double_1467196_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                def length : ClassFunctionality (pts : ClassFunctionality[])
                {
                    return = 1.0;
                }
                a = [Imperative]{
                   //return type class and return a double
                   
                    pt1 = ClassFunctionality.ClassFunctionality();
                    pt2 = ClassFunctionality.ClassFunctionality();
                    pts = [pt1, pt2];
                    numpts = length(pts); 
                    return numpts.IntVal;
                }";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            //thisTest.Verify("a",); not sure what is the expected behaviour 
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS007_Return_double_To_int_1467196_Imperative()
        {
            string code =
                @" 
                        import(""FFITarget.dll"");
                        numpts = i[0];
                        a = i[1];
                        def length : int (pts : ClassFunctionality[])
                        {
                              return = 1;
                        }
                        i = [Imperative]
                        {
                            //return type int and return a double
                       
                            pt1 = ClassFunctionality.ClassFunctionality( );
                            pt2 = ClassFunctionality.ClassFunctionality( );
                            pts = [pt1, pt2];
                            numpts = length(pts); 
                            return [numpts, numpts.IntVal];
                        }";
            //Assert.Fail("1467196 - Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 1);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS008_Param_Int_IntArray_1467208_Imperative()
        {
            string code =
                @"  def foo:int[] (x:int[])
                      {
                          return = 1;
                      }
                    r = [Imperative]{
                      return foo(3);            
                }";
            string error = "DNL-1467208 Auto-upcasting of int -> int[] is not happening in some cases";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 1 });
        }

        [Test]
        [Category("Type System")]
        public void TS009_Parameter_Int_ToBoolArray_1467182_Imperative()
        {
            string code =
                @"     def foo(x:bool[])
                       {
                          return 1;
                       }
                        r = [Imperative]
                        {
                            return foo(3);            
                        }";
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 1);
        }

        [Test]
        [Category("Type System")]
        public void TS010_Parameter_Bool_ToIntArray_1467182_Imperative()
        {
            string code =
                @"  def foo(x:int[])
                    {
                            return = 1;
                    }
                      r = [Imperative]
                      {
                       return foo(false);    
                      }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS011_Return_Int_ToIntArray_Imperative()
        {
            string code =
                @"      def foo:int[](x:int)
                        {
                              return = x;
                        }
                        r = [Imperative]{

                        return foo(3); // r = {3};
                }";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS012_Return_Int_ToBoolArray_1467182_Imperative()
        {
            string code =
                @"  def foo:bool[]()
                    {
                        return = x;
                     }
                r = [Imperative]{
                     return foo(3);            // r = {null} ?
                }";
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases  ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS013_Parameter_Bool_ToIntArray_Imperative()
        {
            string code =
                @"      def foo:int[]()
                        {
                           return = false;
                        }
                        r = [Imperative]{
                            return foo(); // r = {null}
                        }";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS014_Return_IntArray_ToInt_Imperative()
        {
            string code =
                @"     def foo:int()
                       {
                            return = [1, 2, 3];
                       }    
                        r = [Imperative]
                        {
                            return foo();                              
                        }";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS015_Parameter_BoolArray_ToInt_Imperative()
        {
            string code =
                @"    def foo(x:int)
                      {
                             return = x + 1;
                      }
                      r = [Imperative]{
                      return foo([true, false]); // method resolution failure, r= null
                              
                }";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("Type System")]
        public void TS016_Return_BoolArray_ToInt_Imperative()
        {
            string code =
                @"
                       def foo:int()
                       {
                              return = [true, false];
                       }   
                        r = [Imperative]{
                        return foo();                             
                }";
            //Assert.Fail("1467200 - Sprint 25 - rev 3242 type checking negative cases failing ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS017_Return_BoolArray_ToInt_1467182_Imperative()
        {
            string code =
                @"
                            import(""FFITarget.dll"");                            c = [Imperative]
                            {
                       
                                a = [ClassFunctionality.ClassFunctionality(),ClassFunctionalityMirror.ClassFunctionalityMirror()];
                                b=a;
                                a[0].IntVal = 100;
                                b[0].IntVal = ""false"";
                                c=a[0].IntVal;
                                d=b[0].IntVal;
                                return c;
                            }";
            string error = "1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("c", 100); // null 
        }

        [Test]
        [Category("Type System")]
        public void TS018_Param_Int_ordouble_ToBool_1467172_Imperative()
        {
            string code =
                @"
                            b=i[0];c=i[1];d=i[2];
                            def foo( a : bool )
                            {
                                c=[a];
                                return c; 
                            }
                            i = [Imperative]
                            {
                                c=[];

                                b = foo( 1 );
                                c = foo( 1.5 );
                                d = 0;
                                if(1.5 == true )
                                {
                                    d = 3;
                                }
                                return [b,c,d];
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
        public void TS018_Return_Int_ordouble_ToBool_1467172_2_Imperative()
        {
            string code =
                @"
                            b=i[0];c=i[1];d=i[2];
                            def foo:bool( a  )
                            {
                                return a; 
                            }
                            i = [Imperative]
                            {
                                b = foo( 1 );
                                c = foo( 1.5 );
                                d = 0;
                                if(1.5 == true )
                                {
                                    d = 3;
                                }
                                return [b,c,d];
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
        public void TS019_conditional_cantevaluate_1467170_Imperative()
        {
            string code =
                @"
                     A = [Imperative]
                    {
                        A = 1;
                        if (0)
                            A = 2;
                        else
                            A= 3;
                        return A;
                    }
                        //expected A=1;
                        //Received A=3;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS020_conditional_cantevaluate_1467170_Imperative()
        {
            string code =
                @"
                    A = [Imperative]
                    {
                        A = 1;
                        B=1;
                        if (B)
                            A = 2;
                        else
                            A= 3;
                        return A;
                    }
                        //expected A=1;
                        //Received A=3;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS021_OverallPrimitiveConversionTestInt_Imperative()
        {
            string code =
                @"
import(""FFITarget.dll"");
zero_var = i[0];
zero_int = i[1];
zero_double = i[2];
zero_bool = i[3];
zero_String = i[4];
zero_char = i[5];
zero_a = i[6];
one_var = i[7];
one_int = i[8];
one_double = i[9];
one_bool = i[10];
one_String = i[11];
one_char = i[12];
one_a = i[13];
foo = i[14];
foo2 = i[15];
foo3 = i[16];
i = [Imperative]
{
    zero_var:var = 0;
    zero_int:int = 0;
    zero_double:double = 0;
    zero_bool:bool = 0;
    zero_String:string = 0;
    zero_char:char = 0;
    zero_a:ClassFunctionality = 0;
    one_var:var = 1;
    one_int:int = 1;
    one_double:double = 1;
    one_bool:bool = 1;
    one_String:string = 1;
    one_char:char = 1;
    one_a:ClassFunctionality = 1;
    foo:int = 32.342;
    foo2:int = 32.542;
    foo3:int = 32.5;
    return [zero_var,
zero_int,
zero_double,
zero_bool,
zero_String,
zero_char,
zero_a,
one_var,
one_int,
one_double,
one_bool,
one_String,
one_char,
one_a,
foo,
foo2,
foo3];
}";
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
        public void TS022_conditional_cantevaluate_1467170_Imperative()
        {
            string code =
                @"  A = [Imperative]
                    {
                        A = 1;
                        B=1;
                        if (null)
                            A = 2;
                        else
                            A= 3;
                        return A;
                    }
                    //expected A=1;
                    //Received A=3;
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("A", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084_Imperative()
        {
            string code =
                @"                    
                        def foo:int(i:int)
                        {
                             return = i;
                        }
                        x = [Imperative]{
                            return foo(2.5);// returning 2.5 it should return 2
                        }";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084_2_Imperative()
        {
            string code =
                @"
                    def foo ( x : int )
                    {
                        return = x + 1;
                    }
                    t = [Imperative]{
                        return foo( 1.5);
                    }";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS023_Double_To_Int_1467084_3_Imperative()
        {
            string code =
                @"
                        def twice : int []( a : double )
                        {
                            return = [[1,1],[1,1]];
                        }
                        d = [Imperative]{
                     
                            d=1..4;
                            d=twice(4);
                            return d;
                        }
";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { null, null });
        }

        [Test]
        [Category("Type System")]
        public void TS024_Double_To_Int_IndexIntoArray_1467214_Imperative()
        {
            string code =
                @"b = [Imperative]
                {
                  a=[1,2,3,4,5];
                    x=2.5;
                    return a[x];
                }";
            //     Assert.Fail("1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS025_KeyWords_Doesnotexist_1467215_Imperative()
        {
            string code =
                @"a = [Imperative]
                {
                    a:z=3;
                    return a;
                }";
            string error = "1467215 - Sprint 26 - rev 3310 language is too easy on key words for typesystem , even when does not exist it passes  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS026_Double_ToInt_1467211_Imperative()
        {
            string code =
                @"                
                def foo:int()
                {
                    return 3.5; 
                }
                a = [Imperative]
                {
                  return foo();
                }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS027_Double_ToInt_1467217_Imperative()
        {
            string code =
                @"
                 def foo:int[]()
                    {
                         return = [3.5]; 
                    }
                    a = [Imperative]
                    {
                        return foo();
                    }";
            //Assert.Fail("1467217 - Sprint 26 - Rev 3337 - Type Conversion does not happen if the function returns an array ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 4 });
        }

        [Test]
        [Category("Type System")]
        public void TS028_Double_ToInt_1467218_Imperative()
        {
            string code =
                @"
                  def foo:int()
                  {
                      return = 3.5; 
                  }
                  a = [Imperative]
                  {
                    return foo();
                  }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS028_Double_ToInt_1467218_1_Imperative()
        {
            string code =
                @"
                  def foo:int[]()
                  {
                      return [3.5];
                  }
                    a = [Imperative]
                    {
                        return foo()[0];
                    }";
            //Assert.Fail("1467218 - Sprint 26 - Rev 3337 - Type Conversion does not happen if the function returns and array and and index into function call ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS029_Double_ToVar_1467222_Imperative()
        {
            string code =
                @"
                        def foo()
                        {
                            x : int = 3.5; // x still is int, and 3.5 converted to 4
                            return = x;
                        }
                    
                        b = [Imperative]
                        {
                            return foo();
                        }";
            //Assert.Fail("1467222 - Sprint 26 - rev 3345 - if return type is var it still does type conversion ");
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS029_Double_ToInt_1463268_Imperative()
        {
            string code =
                @"
                    def foo ( x : int )
                    {
                        return = x + 1;
                    }
                    t = [Imperative]
                    {
                        return foo( 1.5);
                    }";
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected");
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS030_eachtype_To_var_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0]; b=i[1]; c=i[2]; d1=i[3]; e=i[4]; f=i[5];
                        def foo ( x )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]
                        {
                            a = foo( 1.5); 
                            b = foo( 1); 
                            c = foo( ""1.5""); //char to var 
                            //a = foo( '1.5');// char to var 
                             d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var 
                            d1=d.IntVal;
                            e = foo( false);//bool to var 
                            f = foo( null);//null to var 
                            return [a, b, c, d1, e, f];
                        }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.5);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", "1.5");
            thisTest.Verify("d1", 1);
            thisTest.Verify("e", false);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS031_eachType_To_int_Imperative()
        {
            string code =
                @" 
                        import(""FFITarget.dll"");
                        a=i[0]; b=i[1]; c=i[2]; d1=i[3]; e=i[4]; f=i[5];
                        def foo ( x:int )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]
                        {
                            a = foo( 1.5); 
                            b = foo( 1); 
                            c = foo( ""1.5""); // var to int 
                            //a = foo( '1.5');// var to int
                             d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var 
                            d1=d.IntVal;
                            e = foo( false);// var to int 
                            f = foo( null);//null to int
                            return [a, b, c, d1, e, f];
                        }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS031_eachtype_To_double_Imperative()
        {
            string code =
                @" 
                        import(""FFITarget.dll"");
                        a=i[0]; b=i[1]; c=i[2]; d1=i[3]; e=i[4]; f=i[5];
                        def foo ( x:double )
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]
                        {
                            a = foo( 1); 
                            b = foo( 1); 
                            c = foo( ""1.5""); // var to int 
                            //a = foo( '1.5');// var to int
                             d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var 
                            d1=d.IntVal;
                            e = foo( false);// var to int 
                            f = foo( null);//null to int
                            return [a, b, c, d1, e, f];
                        }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1.0);
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS032_eachType_To_bool_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];b=i[1];c=i[2];c1=i[3];d=i[4];e=i[5];e1=i[6];
                        i = [Imperative]
                        {
                    
                            a:bool= 1;//true
                            b:bool= -0.1; //true
                            c:bool=""1.5""; //true
                            c1:bool= """"; //false
                            //d:bool='1.5';
                            d:bool= ClassFunctionality.ClassFunctionality(1); // user def to bool - > if not null true
                       
                            e:bool= true;
                            e1:bool=null;
                            return [a, b, c, c1, d, e, e1];
                        }";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS033_eachType_To_string_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0]; b=i[1]; c=i[2]; d1=i[3]; e=i[4]; f=i[5];
                       def foo ( x:string)
                        {
                            b1= x ;
                            return =b1;
                        }
                    i = [Imperative]
                    {
                          a = foo(0.00); // double to string
                          b = foo( 1); // int to  string
                          c = foo( ""1.5"");//char to string  
                          c1 = foo( '1');// char to string 
                          d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to string
                          d1=d.IntVal;
                          e = foo( false);//bool to string
                          f = foo( null);//null to string
                          return [a, b, c, d1, e, f];
                    }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", "1.5");
            thisTest.Verify("d1", null);
            //   thisTest.Verify("c1", "1");//Assert.Fail("1467227 -Sprint 26 - 3329 char not convertible to string ");
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS034_eachType_To_char_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0]; b=i[1]; c=i[2]; d1=i[3]; c1 = i[4]; e=i[5]; f=i[6];
                       def foo ( x:char)
                        {
                            b1 = x ;
                            return b1;
                        }
                    i = [Imperative]
                    {
                          a = foo(0.00); // double to char
                          b = foo( 1); // int to  char
                          c = foo( ""1.5"");//char to char
                          c1 = foo( '1');// char to char  
                          d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to char
                          d1=d.IntVal;
                          e = foo( false);//bool to char
                          f = foo( null);//null to char
                          return [a, b, c, d1, c1, e, f];
                    }";
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
        public void TS34_CharToString_1467227_Imperative()
        {
            string code =
               @"a = [Imperative]
                {
                    a:string='1';
                    return a;
                }";
            thisTest.RunScriptSource(code, "1467227 -Sprint 26 - 3329 char not convertible to string ");
            thisTest.Verify("a", "1");
        }

        [Test]
        [Category("Type System")]
        public void TS35_nullTobool_1467231_Imperative()
        {
            string code =
                @"a = [Imperative]
                {
                    a:bool=null;
                    return a;                
                }";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467231 - Sprint 26 - Rev 3393 null to bool conversion should not be allowed ");
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS36_stringTobool_1467239_Imperative()
        {
            string code =
                @"c=i[0];c1=i[1];
                i = [Imperative]
                {
                    c:bool=""1.5""; //expected :true, received : null
                    c1:bool= """"; //expected :false,received : null 
                    return [c, c1];
                }";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467239 - Sprint 26 - Rev 3425 type conversion - string to bool conversion failing  ");
            thisTest.Verify("c", true);
            thisTest.Verify("c1", false);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS37_userdefinedTobool_1467240_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    d = [Imperative]
                    {
                
                        d:bool=ClassFunctionality.ClassFunctionality(5); // user def to bool - > if not null true
                        return d;
                    }";
            string error = "1467287 Sprint 26 - 3721 user defined to bool conversion does not happen in imperative ";
            thisTest.RunScriptSource(code, error);
            //Assert.Fail("1467240 - Sprint 26 - Rev 3426 user defined type not convertible to bool");
            thisTest.Verify("d", true);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS038_eachType_To_Userdefined_Imperative()
        {
            string code =
                @"
                            import(""FFITarget.dll"");
                            a=i[0];b=i[1];c=i[2];d=i[3];e1=i[4];f=i[5];g=i[6];
                            i = [Imperative]
                            {
                    
                                a:ClassFunctionality = 1;//
                                b:ClassFunctionality = -0.1; //
                                c:ClassFunctionality = ""1.5""; //false
                                d:ClassFunctionality =true;
                                d:ClassFunctionality = ClassFunctionalityMirror.ClassFunctionalityMirror(); // user def to bool - > if not null true
                                e:ClassFunctionality = ClassFunctionality.ClassFunctionality(1);
                                e1=e.IntVal;
                                f:ClassFunctionality = true;
                                g:ClassFunctionality = null;
                                return [a, b, c, d, e1, f, g];
                          }";
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
        public void TS40_null_toBool_1467231_Imperative()
        {
            string code =
                @"a = [Imperative]{
                    a:bool=null;
                    return a;
                }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Type System")]
        public void TS41_null_toBool_1467231_2_Imperative()
        {
            string code =
                @"a=i[0];c=i[1];
                def test(b:bool) {
                    return = b;
                }
                i = [Imperative]{
                a=null; 
                c=test(a);
                return [a, c];}"; //expected :true, received : null
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("Type System")]
        public void TS42_null_toBool_1467231_3_Imperative()
        {
            string code =
                @"
                def test:bool(b)
                {
                return = b;
                }
                c = [Imperative]{
                a=null; 
                return test(a);}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("Type System")]
        public void TS43_null_toBool_1467231_positive_Imperative()
        {
            string code =
                @"b=[Imperative]
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
                    return b;
                }"; //expected :true, received : null
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS44_any_toNull_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];b=i[1];c=i[2];d=i[3];e=i[4];f=i[5];g=i[6];
                    i = [Imperative]
                    {
                        a:double= null; 
                        b:int =  null; 
                        c:string=null; 
                        d:char = null;
                        e:ClassFunctionality = null;
                        f:bool = null;
                        g = null;
                        return [a, b, c, d, e, f, g];
                    }"; //expected :true, received : null
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
        public void TS45_int_To_Double_1463268_Imperative()
        {
            string code =
                @"
def foo(x:int) 
{
    return = x + 1; 
}
t = [Imperative]{return foo(1.5);}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS46_typedassignment_To_array_1467206_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];b=i[1];c=i[2];d=i[3];e1=i[4];f=i[5];g=i[6];
                    i = [Imperative]{
              
                        a:double[]= [1,2,3];  
                    
                        b:int[] =  [1,2,3];  
                        c:string[]=[""a"",""b"",""c""]; 
                        d:char []= ['c','d','e'];
                        x1= ClassFunctionality.ClassFunctionality();
                        y1= ClassFunctionality.ClassFunctionality();
                        z1= ClassFunctionality.ClassFunctionality();
                        e:ClassFunctionality []= [x1,y1,z1];
                        e1 = [ e[0].IntVal, e[1].IntVal, e[2].IntVal ];
                        f:bool []=[true,false,null];
                        g =[ null,null,null];
                        return [a, b, c, d, e1, f, g];
                }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1.0, 2.0, 3.0 });
            thisTest.Verify("b", new object[] { 1, 2, 3 });
            thisTest.Verify("c", new object[] { "a", "b", "c" });
            thisTest.Verify("d", new object[] { 'c', 'd', 'e' });
            thisTest.Verify("e1", new object[] { 0, 0, 0 });
            thisTest.Verify("f", new object[] { true, false, null });
            thisTest.Verify("g", new object[] { null, null, null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_To_array_1467294_2()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a=i[0];b=i[1];c=i[2];d=i[3];e1=i[4];f=i[5];g=i[6];
                i=[Imperative]
                {
                    a:double[]= 1; 
                    
                    b:int[] =  1.1;                     
                    d:char []= 'c';
                    x1= ClassFunctionality.ClassFunctionality();
                    e:ClassFunctionality []= x1;
                    e1=e.IntVal;
                    f:bool []= true;
                    g []=null;
                    return [a, b, c, d, e1, f, g];
                }";

            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3943
            string error = "MAGN-3943 Array promotion in Imperative block is not happening";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.0 });
            thisTest.Verify("b", new object[] { 1 });
            
            thisTest.Verify("d", new object[] { 'c' });
            thisTest.Verify("e1", new object[] { 1 });
            thisTest.Verify("f", new object[] { true });
            thisTest.Verify("g", new object[] { null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_To_array_1467294_3()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];b=i[1];c=i[2];d=i[3];e1=i[4];f=i[5];g=i[6];
                    i = [Imperative]
                    {
                        a:double[][]= [1]; 
                    
                        b:int[][] =  [1.1]; 
                        c:string[][]=[""a""]; 
                    d:char [][]= ['c'];
                        x1= ClassFunctionality.ClassFunctionality();
                        e:ClassFunctionality [][]=[x1];
                        e1=e.IntVal;
                        f:bool [][]= [true];
                        g [][]=[null];
                        return [a, b, c, d, e1, f, g];
                    }
                    ";

            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3943
            string error = "MAGN-3943 Array promotion in Imperative block is not happening";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1.0 } });
            thisTest.Verify("b", new object[] { new object[] { 1 } });
            thisTest.Verify("c", new object[] { new object[] { "a" } });
            thisTest.Verify("d", new object[] { new object[] { 'c' } });
            thisTest.Verify("e1", new object[] { new object[] { 1 } });
            thisTest.Verify("f", new object[] { new object[] { true } });
            thisTest.Verify("g", new object[] { new object[] { null } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS46_typedassignment_To_Vararray_1467294_4()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];b=i[1];c=i[2];d=i[3];e1=i[4];f=i[5];g=i[6];
                    i = [Imperative]
                    {
                        a:var[][]= 1; 
                    
                        b:var[][] =  1.1; 
                        c:var[][]=""a""; 
                        d:var[][]= 'c';
                        x1= ClassFunctionality.ClassFunctionality(1);
                        e:var[][]= x1;
                        e1=e.IntVal;
                        f:var[][]= true;
                        g :var[][]=null;
                        return [a, b, c, d, e1, f, g];
                    }";

            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3943
            string error = "MAGN-3943 Array promotion in Imperative block is not happening";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { 1.0 } });
            thisTest.Verify("b", new object[] { new object[] { 1 } });
            thisTest.Verify("c", new object[] { new object[] { "a" } });
            thisTest.Verify("d", new object[] { new object[] { 'c' } });
            thisTest.Verify("e1", new object[] { new object[] { 1 } });
            thisTest.Verify("f", new object[] { new object[] { true } });
            thisTest.Verify("g", new object[] { new object[] { null } });
        }

        [Test]
        [Category("Type System")]
        public void TS047_double_To_Int_insidefunction_Imperative()
        {
            string code =
                @"
                def foo(x:int) 
                {
	                x = 3.5; 
                
	                return = x; 
                }
                t = [Imperative]{
                a=1.5;
                return foo(a);}";
            //thisTest.SetErrorMessage("1467250 Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ");
            string error = "1467250 - Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("t", 4);
        }

        [Test]
        [Category("Type System")]
        public void TS047_double_To_Int_insidefunction_2_Imperative()
        {
            string code =
                @"
                def foo(x:int )
                {
                    x:double = 3.5;
                    y = 3;
                    return = x * y;
                }
                t=[Imperative]{
                a = 1.5;
                return foo(a);}";
            //thisTest.SetErrorMessage("1467250 Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ");
            string error = "1467250 - Sprint 26 - 3472 - variable modification inside a function does not follow type conversion rules ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("t", 10.5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS048_Param_eachType_To_varArray_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];b=i[1];c=i[2];d1=i[3];e=i[4];f=i[5];
                        def foo ( x:var[] )
                        {
	                        b1 = x ;
	                        return b1;
                        }
                        i = [Imperative]
                        {
                            a = foo( 1.5); 
                            b = foo( 1); 
                            c = foo( ""1.5""); //char to var 
                            d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
                            d1=[d[0].IntVal];
                            e = foo( false);//bool to var 
                            f = foo( null);//null to var 
                            return [a, b, c, d1, e, f];
                        }";

            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3975
            string error = "MAGN-3975: Type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("d1", new object[] { 1 });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", new object[] { null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS049_Return_eachType_To_varArray_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];b=i[1];c=i[2];d1=i[3];e=i[4];f=i[5];
                        def foo :var[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]{
                            a = foo( 1.5); 
                            b = foo( 1); 
                            d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
                             d1 =  [d[0].IntVal ];
                            e = foo( false); 
                            f = foo( null); 
                            return [a, b, c, d1, e, f];
                        }";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3975
            string error = "MAGN-3975: Type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1 });
            thisTest.Verify("d1", new object[] { 1 });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("f", new object[] { null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS050_Return_eachType_To_intArray_Imperative()
        {
            //  
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];a1=i[1];b=i[2];c=i[3];d1=i[4];e=i[5];f=i[6];
                        def foo :int[]( x)
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]{
                            a = foo( 1.5); 
                            z:var=1.5;
                            a1=foo(z);
                            b = foo( 1); 
                            c = foo( ""1.5""); 
                            d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
                             d1 =  [d[0].IntVal] ;
                            e = foo( false); 
                            f = foo( null); 
                            return [a,a1,b,c,d1,e,f];
                        }";

            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3975
            string error = "MAGN-3975: Type conversion from var to var array promotion is not happening ";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS051_Param_eachType_To_intArray_Imperative()
        {
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];a1=i[1];b=i[2];c=i[3];d1=i[4];e=i[5];f=i[6];
                        def foo ( x:int[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                    i = [Imperative]
                    {
                        a = foo( 1.5); 
                         z:var=1.5;
                        a1=foo(z);
                        b = foo( 1); 
                        c = foo( ""1.5""); 
                        //a = foo( '1.5');
                       d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
                         d1 = d[0].IntVal ;
                        e = foo( false); 
                        f = foo( null);
                        return [a,a1,b,c,d1,e,f];
                    }";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS052_Return_AllTypeTo_doubleArray_Imperative()
        {
            //  
            string code =
                @"
import(""FFITarget.dll"");
a=i[0];a1=i[1];b=i[2];c=i[3];d1=i[4];e=i[5];f=i[6];
def foo :double[]( x)
{
	b1= x ;
	return =b1;
}
i=[Imperative]
{
    a = foo( 1.5); 
    z:var=1.5;
    a1=foo(z);
    b = foo( 1); 
    c = foo( ""1.5""); 
    //a = foo( '1.5');
    d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
    d1 =  d[0].IntVal ;
    e = foo( false); 
    f = foo( null); 
    return [a,a1,b,c,d1,e,f];
}";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("a1", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1.0 });
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS053_Param_AlltypeTo_doubleArray_Imperative()
        {
            //  
            string code =
                @"
                        import(""FFITarget.dll"");
                        a=i[0];b=i[1];c=i[2];d1=i[3];e=i[4];f=i[5];
                        def foo ( x:double[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                        i = [Imperative]
                        {
                            a = foo( 1.5); 
                            z:var=1.5;
                            a1=foo(z);
                            b = foo( 1); 
                            c = foo( ""1.5"");  
                            //a = foo( '1.5');
                            d = foo( ClassFunctionality.ClassFunctionality(1)); // user define to var
                            d1=d.IntVal;
                            e = foo( false);
                            f = foo( null);
                            return [a, b, c, d1, e, f];
                        }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { 1.5 });
            thisTest.Verify("b", new object[] { 1.0 });
            thisTest.Verify("c", null);
            thisTest.Verify("d1", null);
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS055_Param_AlltypeTo_BoolArray_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];a1=i[1];b=i[2];c=i[3];d=i[4];e=i[5];e1=i[6];
                    def foo ( x:bool[])
                    {
	                    b1= x ;
	                    return =b1;
                    }
                    i = [Imperative]
                    {
                        a = foo([ 1.5, 2.5 ]);
                        z:var[]=[ 1.5, 2.5 ];
                        a1=foo(z);
                        b = foo([ 1, 0 ]);
                        c = foo([ ""1.5"" ,""""]);
                        c1 = foo( ['1','0']);
                        d = foo([ ClassFunctionality.ClassFunctionality(1),ClassFunctionality.ClassFunctionality(1) ]);
                        e = foo([ false,true ]);
                        f = foo([ null, null ]);
                        return [a,a1,b,c,d,e,e1];
                    }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, true });
            thisTest.Verify("e", new object[] { false, true });
            thisTest.Verify("e1", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS056_Return_AlltypeTo_BoolArray_Imperative()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    a=i[0];a1=i[1];b=i[2];c=i[3];d=i[4];e=i[5];f=i[6];g=i[7];
                    def foo :bool( x)
                    {
	                    b1= x ;
	                    return =b1;
                    }
                    i = [Imperative]
                    {
                        a = foo([ 1.5, 2.5 ]);
                        z:var[]=[ 1.5,2.5 ];
                        a1=foo(z);
                        b = foo([ 1, 0 ]);
                        c = foo([ ""1.5"" ,""""]);
                        d = foo([ '1','0']);
                        e = d = foo([ ClassFunctionality.ClassFunctionality(1),ClassFunctionality.ClassFunctionality(1) ]);
                        f = foo([ false,true ]);
                        g = foo([ null, null ]);
                        return [a,a1,b,c,d,e,f,g];
                    }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true, true });
            thisTest.Verify("a1", new object[] { true, true });
            thisTest.Verify("b", new object[] { true, false });
            thisTest.Verify("c", new object[] { true, false });
            thisTest.Verify("d", new object[] { true, true });
            thisTest.Verify("e", new object[] { true, true });
            thisTest.Verify("f", new object[] { false, true });
            thisTest.Verify("g", new object[] { null, null});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS056_Return_BoolArray_1467258_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a=i[0];a1=i[1];b=i[2];c=i[3];d=i[4];e=i[5];
                def foo:bool[](x)
                {
		 	        b1= x ;
	                return =b1;
                }
                i=[Imperative]
                {
                    a = foo([ 1.5, 2.5 ]);
                    a1 : var = foo([ 1.5,2.5 ]);
                    b = foo([ 1, 0 ]);
                    c = foo([""1.5"" ,""""]);
                    d = foo(['1', '0' ]);
                    e = d = foo([ ClassFunctionality.ClassFunctionality(1),ClassFunctionality.ClassFunctionality(1) ]);
                    return [a,a1,b,c,d,e];
                }";
            string error = "1467258 - sprint 26 - Rev 3541 if the return type is bool array , type conversion does not happen for some cases  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { new object[] { true }, new object[] { true } });
            thisTest.Verify("a1", null);
            thisTest.Verify("b", new object[] { new object[] { true }, new object[] { false } });
            thisTest.Verify("c", new object[] { new object[] { true }, new object[] { false } });
            thisTest.Verify("d", new object[] { new object[] { true }, new object[] { true } });
            thisTest.Verify("e", new object[] { new object[] { true }, new object[] { true } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void SetWrongTypeInFFIClass()
        {
            string code =
                @"
                    import(""FFITarget.dll"");
                    d = [Imperative]
                    {
                   
                        a = ClassFunctionality.ClassFunctionality();
                        a.IntVal = true;
                        return = a.IntVal;
                    }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 0);
        }

        [Test]
        [Category("Type System")]
        public void TS059Double_To_int_1467203_Imperative()
        {
            string code =
                @"a = [Imperative]
                {
                   a:int=2.5;
                    return a;
                 }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS060Double_To_int_1467203_Imperative()
        {
            string code =
                @"a = [Imperative]
                {
                   a:int=2.5;
                    return a;
                 }";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS061_typeconersion_imperative_1467213_Imperative()
        {
            string code =
                @"a=[Imperative]
                { 
                    a : int = 3.2;
                    return a;
                }
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TS062_basic_upcoerce_assign_Imperative()
        {
            string code =
                @"
                a=[Associative]
                { 
                    a : int[] = 3;
                    return a;
                }
                                                 ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS063_basic_upcoerce_dispatch_Imperative()
        {
            string code =
                @"
                    def foo(i : int[])
                    { return=i; }
                a = [Associative]
                { 
                    return foo(3);
                }
                                                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS063_basic_upcoerce_return_Imperative()
        {
            string code =
                @"
                    def foo:int[]()
                    { return=3; }
                a = [Associative]
                { 
                    return foo();
                }
                                               ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
        }

        [Test]
        [Category("Type System")]
        public void TS064_bool_Conditionals_1467278_Imperative()
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
        public void TS065_doubleToInt_IndexingIntoArray_1467214_Imperative()
        {
            string code =
                @"b=i[0];c=i[1];d=i[2];
                i = [Imperative]
                {
                    a=[1,2,3,4,5];
                    x=2.5;
                    b=a[x];
                    c=a[2.1];       
                    d=a[-2.1]; 
                    return [b,c,d];
                }";
            string error = "1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 4);

        }


        [Test]
        [Category("Type System")]
        public void TS065_doubleToInt_IndexingIntoArray_1467214_2_Imperative()
        {
            string code =
                @"b = [Imperative]
                {
                    a = [ 1, [ 2 ], 3, 4, 5 ];
                    x=-0.1;
                    return a[1][x];
                }";
            string error = "1467214 - Sprint 26- Rev 3313 Type Conversion from Double to Int not happening while indexing into array ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("Type System")]
        public void TS066_Int_To_Char_1467119_Imperative()
        {
            string code =
                @"
                    def foo ( x : char )
                    {
                        return = x;
                    }
                    y = [Imperative]
                    {
                     return foo (1);
                    }";
            string error = "1467119 - Sprint24 : rev 2807 : Type conversion issue with char  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("Type System")]
        public void TS067_string_To_Char_1467119_2_Imperative()
        {
            string code =
                @"
                    def foo ( x : char )
                    {
                        return = x;
                    }
                    y = [Imperative]{
                    return foo (""1"");
                    }";
            string error = "1467119 - Sprint24 : rev 2807 : Type conversion issue with char  ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS068_Param_singleton_AlltypeTo_BoolArray_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a=i[0];a1=i[1];b=i[2];c=i[3];d=i[4];e=i[5];e1=i[6];
                def foo ( x:bool[])
                {
	                b1= x ;
	                return =b1;
                }
                i = [Imperative]
                {
                    a = foo( 1.5);
                    z:var=1.5;
                    a1=foo(z);
                    b = foo( 1);
                    c = foo( ""1.5"" );
                    c1 = foo( '1');
                    d = foo(ClassFunctionality.ClassFunctionality(1) );
                    e = foo( false );
                    f = foo( null );
                    return [a,a1,b,c,d,e,e1];
                }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true });
            thisTest.Verify("a1", new object[] { true });
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("d", new object[] { true });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("e1", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS069_Return_singleton_AlltypeTo_BoolArray_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a=i[0];a1=i[1];b=i[2];c=i[3];c1=i[4];d=i[5];e=i[6];e1=i[7];
                def foo:bool[]( x)
                {
	                b1= x ;
	                return =b1;
                }
                i = [Imperative]
                {
                    a = foo( 1.5);
                    z:var=1.5;
                    a1=foo(z);
                    b = foo( 1);
                    c = foo( ""1.5"" );
                    c1 = foo( '1');
                    d = foo(ClassFunctionality.ClassFunctionality(1) );
                    e = foo( false );
                    f = foo( null );
                    return [a,a1,b,c,c1,d,e,e1];
                }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", new object[] { true });
            thisTest.Verify("a1", new object[] { true });
            thisTest.Verify("b", new object[] { true });
            thisTest.Verify("c", new object[] { true });
            thisTest.Verify("c1", new object[] { true });
            thisTest.Verify("d", new object[] { true });
            thisTest.Verify("e", new object[] { false });
            thisTest.Verify("e1", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS070_Param_singleton_AlltypeTo_StringArray_Imperative()
        {
            string code =
                @" 
                    import(""FFITarget.dll"");
                    a=i[0];a1=i[1];b=i[2];c=i[3];c1=i[4];d=i[5];e=i[6];e1=i[7];
                    def foo ( x:string[])
                    {
	                    b1= x ;
	                    return =b1;
                    }
                    i = [Imperative]{
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( ClassFunctionality.ClassFunctionality(1) );
                        e = foo(false);
                        f = foo( null );
                        return [a,a1,b,c,c1,d,e,e1];
                    }";
            string error = "1467251 - sprint 26 - Rev 3485 type conversion from var to var array promotion is not happening ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", new object[] { "1.5" });
            thisTest.Verify("c1", new object[] { "1" });
            thisTest.Verify("d", null);
            thisTest.Verify("e", null);
            thisTest.Verify("e1", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS071_return_singleton_AlltypeTo_StringArray_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a=i[0];a1=i[1];b=i[2];c=i[3];c1=i[4];d=i[5];e=i[6];f=i[7];
                def foo :string[]( x)
                {
	                b1= x ;
	                return =b1;
                }
                i = [Imperative]{
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( ClassFunctionality.ClassFunctionality(1) );
                        e = foo(false);
                        f = foo( null );
                        return [a,a1,b,c,c1,d,e,f];
                    }";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void TS072_Param_singleton_AlltypeTo_CharArray_Imperative()
        {
            string code =
                @" 
                        import(""FFITarget.dll"");
                        a=i[0];a1=i[1];b=i[2];c=i[3];c1=i[4];d=i[5];e=i[6];f=i[7];
                        def foo ( x:char[])
                        {
	                        b1= x ;
	                        return =b1;
                        }
                   i = [Imperative]
                    {
                        a = foo(1.5);
                        z:var=1.5;
                        a1=foo(z);
                        b = foo(1);
                        c = foo( ""1.5"" );
                        c1 = foo( '1');
                        d = foo( ClassFunctionality.ClassFunctionality(1) );
                        e = foo(false);
                        f = foo( null );
                        return [a,a1,b,c,c1,d,e,f];
                    }";
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
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        [Category("Failure")]
        public void TS078_userdefinedToUserdefinedArray_Imperative()
        {
            string code =
                @"
                import(""FFITarget.dll"");
                a1 = [Imperative]
                {
                        a : ClassFunctionality[] =  ClassFunctionality.ClassFunctionality(1) ;
                       return a.IntVal;
                }";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3943
            thisTest.RunScriptSource(code);
            thisTest.Verify("a1", new object[] { 1 });
        }

        [Test]
        public void indexintoarray_left_1467462_1_imperative()
        {
            string code =
                @"x=[Imperative]
                {
                    x : var[] = [ 1, 2, 3, 4 ];
                    x[2..3] =[ 1, 2 ];
                    return x;
                }
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_2_imperative()
        {
            string code =
                @"x=
                [Imperative]
                {
                    x : int[] = [ 1, 2, 3, 4 ];
                    x[2..3] = [ 1, 2 ];
                    return x;
                }
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_3_imperative()
        {
            string code =
                @"x=[Imperative]
                {
                    x : int[] = [ 1, 2, 3, 4 ];
                    x[2..3] = [ 1, 2 ];
                    return x;
                }
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_4_imperative()
        {
            string code =
                @"x=[Imperative]
                {
                    x : bool[] = [ true, false, true, false ];
                    x[2..3] = [ true, 2 ];
                    return x;
                }
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { true, false, true, 2 });
        }

        [Test]
        public void indexintoarray_left_1467462_5_imperative()
        {
            string code =
                @"x=[Imperative]
                {
                    x : var[] = [ 1, false,""q"", 1.3 ];//jagged
                    x[2..3] = [ true, 2 ];
                    return x;
                }
                ";
            var mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x", new object[] { 1, false, true, 2 });
        }

        [Test]
        [Category("Type System")]
        public void TS0189_TypeConversion_conditionals_1465293_1_imperative()
        {
            string code =
                @"
                a=i[0];
                b=i[1];
                c=i[2];
                d=i[3];
                 i = [Imperative]
                 {
                  a=2==null;  
                  b=2!=null;
                  c=null==null;
                  d=null!=null;
                    return [a,b,c,d];
                 }
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
        public void TS0190_TypeConversion_nested_block_1467568()
        {
            string code =
                @"
myValue = 2;
c = 0;
i = [Imperative]
{
    while( myValue <  8)
    {
        [Associative] 
        {
            a = 1;
            b = a + myValue;            
            c = c + b;
            Print(""b : "" + b);
        }
        
        myValue = myValue + 2.5; 
        Print(""myValue : "" + myValue); 
    }
    return c;
}             
                 ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("i", 16.5);
        }

        [Test]
        [Category("Type System"), Category("Failure")]
        public void TS0191_TypeConversion_nested_block_1467568()
        {
            string code =
                @"
myValue = 9; // (1) myValue is a variable which is initially defined as an int 
myIntRangeExpression = 0..180..#5; 
            
myRangeExpressionResult = myIntRangeExpression * myValue; //(2) myValue is used with an array of ints
            
Print(myRangeExpressionResult); // (3) and the results are more ints.. so far so good!
i = [Imperative]
{
    while( myValue < 10) // (5) but inside an Imperative whle loop
    {
        [Associative] // (6) and then inside an associative block
        {
            myIntRangeExpression = 0..180..#5;
            
            myRangeExpressionResult = myIntRangeExpression * myValue; // (7) myValue is still be treated as an int
            
            Print(myRangeExpressionResult); // (8) because the variable myRangeExpressionResult are all ints!!
        }
        
        myValue = myValue + 0.2; // (3) but by adding  a decimal fraction.. it become a double
        //Print(""P_myValue : "" + myValue); // (4) as the output from this 'Print' shows.. what is going on?
    }
    return myRangeExpressionResult;
}            
                 ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("i", new object[] { 0.0, 449.99999999999983, 899.99999999999966, 1349.9999999999995, 1799.9999999999993 });
        }

        [Test]
        [Category("Type System"), Category("Failure")]
        public void TS0192_TypeConversion_nested_block_1467568()
        {
            string code =
                @"
myValue = 1; 
myIntRangeExpression = 0..2;             
myRangeExpressionResult = [Imperative]
{
    while( myValue < 2) 
    {
        [Associative] 
        {
            myIntRangeExpression = 0..2;            
            myRangeExpressionResult = myIntRangeExpression * myValue;             
        }        
        myValue = myValue + 0.5;        
    }
    return myRangeExpressionResult;
} ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("myRangeExpressionResult", new object[] { 0.0, 1.5, 3.0 });
        }

        [Test]
        [Category("Type System"), Category("Failure")]
        public void TS0193_TypeConversion_nested_block_1467568()
        {
            string code =
                @"
myValue:int = 1; 
myIntRangeExpression = 0..2;             
myRangeExpressionResult = [Imperative]
{
    c = 1;
    while( c <= 2 ) 
    {
        [Associative] 
        {
            myIntRangeExpression = 0..2;            
            myRangeExpressionResult = myIntRangeExpression * myValue;             
        }        
        myValue = myValue - 0.5;
        c = c + 1;        
    }
    return myRangeExpressionResult;
}";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(0);
            thisTest.Verify("myRangeExpressionResult", new object[] { 0, 0.5, 1.0 });
        }

        [Test]
        [Category("Type System")]
        public void TS0194_TypeConversion_nested_block_1467568()
        {
            string code =
                @"
myValue = 1; 
myIntRangeExpression = 0..2;             
myRangeExpressionResult = [Imperative]
{
    myRangeExpressionResult = null;
    while( myValue > 0.25) 
    {
        myRangeExpressionResult = [Associative] 
        {
            myIntRangeExpression = 0..2;            
            return myIntRangeExpression * myValue;             
        }        
        myValue = myValue * 0.5;        
    }
    return myRangeExpressionResult;
} ";
            string error = " ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("myRangeExpressionResult", new object[] { 0.0, 0.5, 1.0 });
        }
    }
}
