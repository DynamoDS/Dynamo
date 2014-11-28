using System;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using System.Collections;
namespace ProtoFFITests
{
    public class CSFFIDataMarshalingTest : FFITestSetup
    {
        [Test]
        public void TestDoubles()
        {
            String code =
            @"               //import(""ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"");               value = TestData.MultiplyDoubles(11111, 11111);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 123454321.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloats()
        {
            String code =
            @"               value = TestData.MultiplyFloats(111.11, 1111.1);               success = TestData.Equals(value, 123454.321);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatOut()
        {
            String code =
            @"               value = TestData.GetFloat();               success = TestData.Equals(value, 2.5);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatsOutOfRangeWarning()
        {
            String code =
            @"               value = TestData.MultiplyFloats(3.40282e+039, 0.001);            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        [Category("Failure")]
        public void TestDecimals()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4392
            String code =
            @"               import(TestData from ""FFITarget.dll"");               import(""System.Decimal"");               x = Decimal.Decimal(1.11e+10);               y = Decimal.Decimal(1.11e+5);               value = TestData.MultiplyDecimals(x, y);               result = Decimal.Decimal(1.2321e+15);               success = Decimal.Equals(value, result);            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            int nErrors = -1;
            ExecuteAndVerify(code, data, out nErrors);
            string err = "MAGN-4392: Test fails due to FFI function calls for Decimal not being resolved";
            Assert.IsTrue(nErrors == 0, err);
        }

        [Test]
        public void TestChar()
        {
            String code =
            @"               f = TestData.GetAlphabet(5); //5th alphabet               c = TestData.ToUpper(f);               F = TestData.ToAscii(c);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestCharOutOfRangeWarning()
        {
            String code =
            @"               XYZ = TestData.ToUpper(70000); //out of range char value.            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestByte()
        {
            String code =
            @"               f = TestData.IncrementByte(101);                c = TestData.ToUpper(TestData.ToChar(f));               F = TestData.ToAscii(c);            ";
            
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestByteOutOfRangeWarning()
        {
            String code =
            @"               XYZ = TestData.IncrementByte(257);  //out of range byte value.            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestSByte()
        {
            String code =
            @"               f = TestData.IncrementSByte(101);                c = TestData.ToUpper(TestData.ToChar(f));               F = TestData.ToAscii(c);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestSByteOutOfRangeWarning()
        {
            String code =
            @"               XYZ = TestData.IncrementSByte(257);  //out of range sbyte value.            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestCombineByte()
        {
            String code =
            @"               value = TestData.Combine(100, 100);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 25700, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestShort()
        {
            String code =
            @"               value = TestData.MultiplyShorts(100, 100);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUShort()
        {
            String code =
            @"               value = TestData.MultiplyUShorts(100, 100);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUInt()
        {
            String code =
            @"               value = TestData.MultiplyUInts(100, 100);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestULong()
        {
            String code =
            @"               value = TestData.MultiplyULongs(100, 100);            ";
            Type t = typeof (FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullForPrimitiveType() //Defect 1462014 
        {
            String code =
            @"               bytevalue = TestData.IncrementSByte(null);               dvalue = TestData.MultiplyDoubles(bytevalue, 45.0);               fvalue = TestData.MultiplyFloats(dvalue, 2324.0);               ulvalue = TestData.MultiplyULongs(dvalue, fvalue);            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "bytevalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "dvalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "fvalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "ulvalue", ExpectedValue = null, BlockIndex = 0 }                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable()
        {
            String code =
            @"               primes = TestData.GetSomePrimes();               prime = primes[5];            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "prime", ExpectedValue = 13, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable2()
        {
            String code =
            @"               nums = TestData.GetNumbersByDouble(10);               num = nums[5];            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable3()
        {
            String code =
            @"               nums = TestData.DoubleThem({1,2,3,4,5});               num = nums[4];            ";
            Type t = typeof (FFITarget.TestData); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerableNestedCollection()
        {
            String code =
            @"               data = TestData.GetNestedCollection();               list = Flatten(data);               size = Count(list);            ";
            Type t = typeof(FFITarget.TestData); 
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "data", ExpectedValue = new List<object> { 2, 3, "DesignScript", new List<string> { "Dynamo", "Revit" }, new List<object> { true, new List<object> { 5.5, 10 } } }, BlockIndex = 0 },                                      new ValidationData { ValueName = "size", ExpectedValue = 8, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerableWithArbitraryRankArray()
        {
            String code =
            @"
               data = TestData.GetNestedCollection();
               list = TestData.RemoveItemsAtIndices(data, {3,1});
               size = Count(Flatten(list));
            ";
            Type t = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "list", ExpectedValue = new List<object> { 2, "DesignScript", new List<object> { true, new List<object> { 5.5, 10 } } }, BlockIndex = 0 },
                                      new ValidationData { ValueName = "size", ExpectedValue = 5, BlockIndex = 0 },
                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestMarshalingIEnumerableOfDummyPoint()
        {
            String code =
            @"
               points = DummyPoint.ByCoordinates(1..5, 0, 0);
               centroid = DummyPoint.Centroid(points);
               x = centroid.X;
            ";
            Type t = typeof(FFITarget.DummyPoint);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "x", ExpectedValue = 3.0, BlockIndex = 0 },
                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestArrayPromotion()
        {
            String code =
            @"
               data = TestData.GetNestedCollection();
               list = TestData.RemoveItemsAtIndices(data, 2);
               size = Count(Flatten(list));
            ";
            Type t = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "list", ExpectedValue = new List<object> { 2, 3, new List<string> { "Dynamo", "Revit" }, new List<object> { true, new List<object> { 5.5, 10 } } }, BlockIndex = 0 },
                                      new ValidationData { ValueName = "size", ExpectedValue = 7, BlockIndex = 0 },
                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestSingletonMarshaledAsIEnumerable()
        {
            String code =
            @"
               list = TestData.RemoveItemsAtIndices(3, 0);
               size = Count(list);
            ";
            Type t = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "list", ExpectedValue = new List<object>(), BlockIndex = 0 },
                                      new ValidationData { ValueName = "size", ExpectedValue = 0, BlockIndex = 0 },
                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetIEnumerable(); // creates an IEnumerable class and returns as an 'object'                   t2 = t.TestIEnumerable(t1);  // implicitly casts the 'object' to IEnumerable based on the argument 'type', and tests its value                   ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetIEnumerable(); // creates an IEnumerable class and returns as an 'object'                   t2 = t.TestIEnumerable2(t1);  // explicitly casts the 'object' to IEnumerable inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetInt();     // creates an int and returns as 'object'                   t2 = t.TestInt(t1);  // implicitly casts the 'object' to int based on the argument 'type', and tests its value                  ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetInt();     // creates an int and returns as 'object'                   t2 = t.TestInt2(t1);  // explicitly casts the 'object' to int inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetUlong();     // creates a 'ulong' and returns as 'object'                   t2 = t.TestUlong(t1);  // implicitly casts the 'object' to ulong based on the argument 'type', and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetUlong();     // creates a 'ulong' and returns as 'object'                   t2 = t.TestUlong2(t1);  // explicitly casts the the 'object' to ulong inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Over_Internal_Classes()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.CreateInternalClass(5); // creates an internal class returned as an 'object'                   t2 = t.TestInternalClass(t1);  // internally converts the 'object' to the class and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 5, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Using_Implicit_Type_Cast_In_Method_Arguments()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.FuncReturningVariousObjectTypes(0..18); // this function uses replication to create 19 different 'types' of variables, returned as 'objects'                   // Now each of those objects are passed to respective functions where the values are verified                    t2 = t.TestUlong(t1[0]);//1                   t3 = t.TestByte(t1[1]);//1                   t4 = t.TestSbyte(t1[2]);//1                   t5 = t.TestShort(t1[3]);//1                   t6 = t.TestUint16(t1[4]);//1                   t7 = t.TestDummyDispose(t1[5]);//20                   t8 = t.TestUint64(t1[6]);//1                   t9 = t.TestChar(t1[7]); //1                   t10 = t.TestFloat(t1[8]);//1                   t11 = t.TestDecimal(t1[9]);//1                   t12 = t.TestUshort(t1[10]);//1                   t13 = t.TestDerivedDummyClass(t1[11]);//123                   t14 = t.TestDerivedDisposeClass(t1[12]);//5                   t15 = t.TestDerived1(t1[13]);//20                   t16 = t.TestDisposeClass(t1[14]);//5                   t17 = t.TestString(t1[15]);  //4                    t18 = t.TestInt(t1[16]); //1                   t19 = t.TestDouble(t1[17]); //1                   t20 = t.TestBoolean(t1[18]); //1                   t21 = { t2, t3, t4, t5, t6, t7, t8,  t9, t10, t11, t12, t13, t14, t15, t16, t17 , t18, t19, t20};                                               ";
            object[] b = new object[] { 1, 1, 1, 1, 1, 20, 1, 1, 1, 1, 1, 123, 5, 20.0, 5, 4, 1, 1, 1 };
            ValidationData[] data = { new ValidationData { ValueName = "t21", ExpectedValue = b, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Using_Explicit_Type_Cast_In_Methods()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.FuncReturningVariousObjectTypes(0..18); // Using replication : t1 is an array of 19 different 'types' , returned as 'object's                   t2 = t.FuncVerifyingVariousObjectTypes(t1, 0..18); // Again using replication, the objects are passed to relevant functions and the vlaues verified                  ";
            object[] b = new object[] { 1.0, 1.0, 1.0, 1.0, 1.0, 20.0, 1.0, 1.0, 1.0, 1.0, 1.0, 123.0, 5.0, 20.0, 5.0, 4.0, 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = b, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_MethodOverloading_In_Csharp_Classes()
        {
            string code =
                @" t = MethodOverloadingClass.MethodOverloadingClass();                   t1 = t.GetValue();                   t2 = t.foo(t1);                                     ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 0, BlockIndex = 0 } };
            Type dummy = typeof (FFITarget.MethodOverloadingClass);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_Dictionary()
        {
            string code =
                @" t = TestData.TestData();                   d = t.GetDictionary();                     r1 = d[""weight""];";
            ValidationData[] data = { new ValidationData { ValueName = "r1", ExpectedValue = 42, BlockIndex = 0 } };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DefaultArgument()
        {
            string code =
                @" d = TestData.AddWithDefaultArgument(42);  
";
            ValidationData[] data = { new ValidationData { ValueName = "d", ExpectedValue = 142} };
            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_ArbitaryDimensionParameter()
        {
            string code =
@" 
d1 = TestData.GetDepth({1, 2, {3, 4}, {5, {6, {7}}}});  
d2 = TestData.SumList({1, 2, {3, 4}, {5, {6, {7}}}});  
";
            ValidationData[] data = 
            { 
                new ValidationData { ValueName = "d1", ExpectedValue = 4} ,
                new ValidationData { ValueName = "d2", ExpectedValue = 28} , 
            };




            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_ObjectAsArbiteraryDimensionArray()
        {
            string code = @"
                     a = 1..5;
                     l1 = TestData.AddItemToFront(10, a);
                     l2 = TestData.AddItemToFront(a, a);";
            ValidationData[] data = 
            { 
                new ValidationData { ValueName = "l1", ExpectedValue = new int[] {10, 1, 2, 3, 4, 5}} ,
                new ValidationData { ValueName = "l2", ExpectedValue = new Object[] {new int[]{1, 2, 3, 4, 5}, 1, 2, 3, 4, 5}} , 
            };

            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_MarshalingNullInCollection()
        {
            string code = @"
                     list = {1, null, ""test"", true, 3.5};
                     l1 = TestData.AddItemToFront(null, list);
                     l2 = TestData.AddItemToFront(list, list);";
            ValidationData[] data = 
            { 
                new ValidationData { ValueName = "l1", ExpectedValue = new object[] {null, 1, null, "test", true, 3.5}} ,
                new ValidationData { ValueName = "l2", ExpectedValue = new object[] {new object[]{1, null, "test", true, 3.5}, 1, null, "test", true, 3.5}} , 
            };

            Type dummy = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_MarshlingFunctionPointer()
        {
            String code =
            @"               def foo() { return = 42;}               fptr = foo;               sameFptr = TestData.ReturnObject(fptr);               value = sameFptr();            ";
            Type t = typeof(FFITarget.TestData);
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 42, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
    }
}
