using System;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using System.Collections;
namespace ProtoFFITests
{
    public class TestData
    {
        public static double MultiplyDoubles(double x, double y)
        {
            return x * y;
        }
        public static double MultiplyFloats(float x, float y)
        {
            return x * y;
        }
        public static float GetFloat()
        {
            return 2.5F;
        }
        public static decimal MultiplyDecimals(decimal x, decimal y)
        {
            return Decimal.Multiply(x, y);
        }
        public static byte IncrementByte(byte value)
        {
            return ++value;
        }
        public static sbyte IncrementSByte(sbyte value)
        {
            return ++value;
        }
        public static char GetAlphabet(int index)
        {
            int c = 'a';
            return (char)(c + index);
        }
        public static char ToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public static char ToChar(object o)
        {
            return (char)(int)o;
        }
        public static int ToAscii(char c)
        {
            return c;
        }
        public static int Combine(byte x, byte y)
        {
            return x << 8 | y;
        }
        public static long MultiplyShorts(short x, short y)
        {
            return x * y;
        }
        public static long MultiplyUShorts(ushort x, ushort y)
        {
            return x * y;
        }
        public static long MultiplyUInts(uint x, uint y)
        {
            return x * y;
        }
        public static ulong MultiplyULongs(ulong x, ulong y)
        {
            return x * y;
        }
        public static bool Equals(float x, float y)
        {
            return Math.Abs(x - y) < 0.0001;
        }
        public static bool Equals(Decimal x, Decimal y)
        {
            return Decimal.Equals(x, y);
        }
        public static IEnumerable<int> GetSomePrimes()
        {
            return new List<int> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        }
        public static IEnumerable<int> GetNumbersByDouble(int x)
        {
            for (int i = 0; i < x; ++i)
            {
                yield return i * 2;
            }
        }
        public static IEnumerable<int> DoubleThem(IEnumerable<int> nums)
        {
            foreach (var item in nums)
            {
                yield return item * 2;
            }
        }

        public static int AddWithDefaultArgument(int x, int y = 100)
        {
            return x + y;
        }

        public object[] GetMixedObjects()
        {
            object[] objs = { new DerivedDummy(), new Derived1(), new TestDispose(), new DummyDispose() };
            return objs;
        }
        public override bool Equals(Object obj)
        {
            return true;
        }
        public object FuncReturningVariousObjectTypes(int x)
        {
            switch (x)
            {
                case 0:
                    {
                        ulong u = 1;
                        return u;
                    }
                case 1:
                    {
                        Byte b = 1;
                        return b;
                    }
                case 2:
                    {
                        sbyte s = 1;
                        return s;
                    }
                case 3:
                    {
                        short s = 1;
                        return s;
                    }
                case 4:
                    {
                        UInt16 u = 1;
                        return u;
                    }
                case 5:
                    {
                        return new DummyDispose();
                    }
                case 6:
                    {
                        UInt64 u = 1;
                        return u;
                    }
                case 7:
                    {
                        char c = '1';
                        return c;
                    }
                case 8:
                    {
                        float f = 1;
                        return f;
                    }
                case 9:
                    {
                        Decimal d = 1;
                        return d;
                    }
                case 10:
                    {
                        ushort u = 1;
                        return u;
                    }
                case 11:
                    {
                        return new DerivedDummy();
                    }
                case 12:
                    {
                        return new TestDisposeDerived();
                    }
                case 13:
                    {
                        return new Derived1();
                    }
                case 14:
                    {
                        return new TestDispose();
                    }
                case 15:
                    {
                        string s = "test";
                        return s;
                    }
                case 16:
                    {
                        int i = 1;
                        return i;
                    }
                case 17:
                    {
                        double d = 1;
                        return d;
                    }
                case 18:
                    {
                        Boolean b = true;
                        return b;
                    }
                default:
                    return 0;
            }
        }
        public int TestUlong(ulong x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUlong2(object x)
        {
            ulong y = Convert.ToUInt64(x);
            if (y == 1)
                return 1;
            else return 0;
        }
        public int TestByte(Byte x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestSbyte(sbyte x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestShort(short x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUint16(UInt16 x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestDummyDispose(DummyDispose x)
        {
            return x.Value;
        }
        public int TestUint64(UInt64 x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestChar(Char x)
        {
            if (x == '1')
                return 1;
            else return 0;
        }
        public int TestFloat(float x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestDecimal(Decimal x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUshort(ushort x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public double TestDerivedDummy(DerivedDummy x)
        {
            return x.random123();
        }
        public int TestDerivedDummyClass(DerivedDummy x)
        {
            return x.random123();
        }
        public int TestDerivedDisposeClass(TestDisposeDerived x)
        {
            return x.get_MyValue();
        }
        public double TestDerived1(Derived1 x)
        {
            return x.GetNumber();
        }
        public int TestDisposeClass(TestDispose x)
        {
            return x.get_MyValue();
        }
        public int TestString(String x)
        {
            return x.Length;
        }
        public int TestInt(int x)
        {
            return x;
        }
        public int TestInt2(object x)
        {
            int y = Convert.ToInt32(x);
            return y;
        }
        public int TestDouble(Double x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestBoolean(Boolean x)
        {
            if (x == true)
                return 1;
            else return 0;
        }
        public int TestIEnumerable(IEnumerable<int> x)
        {
            IEnumerator<int> y2 = x.GetEnumerator();
            y2.Reset();
            y2.MoveNext();
            return y2.Current;
        }
        public int TestIEnumerable2(object x)
        {
            IEnumerable<int> y = (IEnumerable<int>)x;
            IEnumerator<int> y2 = y.GetEnumerator();
            y2.Reset();
            y2.MoveNext();
            return y2.Current;
        }
        public object GetIEnumerable()
        {
            return new List<int> { 2, 2, 2, 2 };
        }
        public object GetInt()
        {
            int x = 1;
            return x;
        }
        public object GetUlong()
        {
            ulong x = 1;
            return x;
        }
        public Object FuncReturningByteAsObject()
        {
            Byte b = 1;
            return b;
        }
        public double FuncVerifyingVariousObjectTypes(object y, int x)
        {
            switch (x)
            {
                case 0: return this.TestUlong(Convert.ToUInt64(y));
                case 1: return this.TestByte(Convert.ToByte(y));
                case 2: return this.TestSbyte(Convert.ToSByte(y));
                case 3: return this.TestShort(Convert.ToInt16(y));
                case 4: return this.TestUint16(Convert.ToUInt16(y));
                case 5: return this.TestDummyDispose((DummyDispose)y);
                case 6: return this.TestUint64(Convert.ToUInt64(y));
                case 7: return this.TestChar(Convert.ToChar(y));
                case 8: return this.TestFloat(Convert.ToSingle(y));
                case 9: return this.TestDecimal(Convert.ToDecimal(y));
                case 10: return this.TestUshort(Convert.ToUInt16(y));
                case 11: return this.TestDerivedDummyClass((DerivedDummy)y);
                case 12: return this.TestDerivedDisposeClass((TestDisposeDerived)y);
                case 13: return this.TestDerived1((Derived1)y);
                case 14: return this.TestDisposeClass((TestDispose)y);
                case 15: return this.TestString(Convert.ToString(y));
                case 16: return this.TestInt(Convert.ToInt32(y));
                case 17: return this.TestDouble(Convert.ToDouble(y));
                case 18: return this.TestBoolean(Convert.ToBoolean(y));
                default:
                    return -1;
            }
        }
        public object CreateInternalClass(int y)
        {
            return InternalClass.CreateObject(5);
        }
        public int TestInternalClass(object y)
        {
            InternalClass x = (InternalClass)y;
            return x.GetValue();
        }

        [MultiReturnAttribute("color", "string")]
        [MultiReturnAttribute("weight", "int")]
        [MultiReturnAttribute("ok", "boolean")]
        [RuntimeRequirement(RequireTracing = true)]
        public Dictionary<string, object> GetDictionary()
        {
            return new Dictionary<string, object>() 
            {
                {"color", "green"},
                {"weight", 42},
                {"ok", false},
                {"nums", new int[] {101, 202}}
            };
        }

        public static int GetDepth([ArbitraryDimensionArrayImport] IList arr)
        {
            int maxSubListDepth = 0;
            foreach (var item in arr)
            {
                var subList = item as System.Collections.IList;
                if (subList != null)
                {
                    var subListDepth = GetDepth(subList);
                    maxSubListDepth = Math.Max(subListDepth, maxSubListDepth);
                }
            }

            return maxSubListDepth + 1;
        }

        public static int SumList([ArbitraryDimensionArrayImport] IList arr)
        {
            int sum = 0;
            foreach (var item in arr)
            {
                if (item is System.Collections.IList)
                {
                    sum += SumList(item as System.Collections.IList);
                }
                else if (item is Int32)
                {
                    sum += (Int32)item;
                }
            }

            return sum;
        }
   }

    internal class InternalClass
    {
        private int x = 5;
        public static InternalClass CreateObject(int y)
        {
            return new InternalClass { x = y };
        }
        public int GetValue()
        {
            return x;
        }
    }
    public class MethodOverloadingClass
    {
        float f = 1.5F;
        public float GetValue()
        {
            return f;
        }
        public int foo(int x)
        {
            return 1;
        }
        public int foo(float x)
        {
            return 0;
        }
    }

    class CSFFIDataMarshalingTest : FFITestSetup
    {

        [Test]
        public void TestDoubles()
        {
            String code =
            @"               //import(""ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"");               value = TestData.MultiplyDoubles(11111, 11111);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 123454321.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloats()
        {
            String code =
            @"               value = TestData.MultiplyFloats(111.11, 1111.1);               success = TestData.Equals(value, 123454.321);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatOut()
        {
            String code =
            @"               value = TestData.GetFloat();               success = TestData.Equals(value, 2.5);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatsOutOfRangeWarning()
        {
            String code =
            @"               value = TestData.MultiplyFloats(3.40282e+039, 0.001);            ";
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestDecimals()
        {
            String code =
            @"               import(TestData from ""ProtoTest.dll"");               import(""System.Decimal"");               x = Decimal.Decimal(1.1111e+10);               y = Decimal.Decimal(1.1111e+5);               value = TestData.MultiplyDecimals(x, y);               result = Decimal.Decimal(1.23454321e+15);               success = Decimal.Equals(value, result);            ";
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            int nErrors = -1;
            ExecuteAndVerify(code, data, out nErrors);
            Assert.IsTrue(nErrors == 0);
        }

        [Test]
        public void TestChar()
        {
            String code =
            @"               f = TestData.GetAlphabet(5); //5th alphabet               c = TestData.ToUpper(f);               F = TestData.ToAscii(c);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
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
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestByte()
        {
            String code =
            @"               f = TestData.IncrementByte(101);                c = TestData.ToUpper(TestData.ToChar(f));               F = TestData.ToAscii(c);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
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
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestSByte()
        {
            String code =
            @"               f = TestData.IncrementSByte(101);                c = TestData.ToUpper(TestData.ToChar(f));               F = TestData.ToAscii(c);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
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
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestCombineByte()
        {
            String code =
            @"               value = TestData.Combine(100, 100);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 25700, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestShort()
        {
            String code =
            @"               value = TestData.MultiplyShorts(100, 100);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUShort()
        {
            String code =
            @"               value = TestData.MultiplyUShorts(100, 100);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUInt()
        {
            String code =
            @"               value = TestData.MultiplyUInts(100, 100);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestULong()
        {
            String code =
            @"               value = TestData.MultiplyULongs(100, 100);            ";
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullForPrimitiveType() //Defect 1462014 
        {
            String code =
            @"               bytevalue = TestData.IncrementSByte(null);               dvalue = TestData.MultiplyDoubles(bytevalue, 45.0);               fvalue = TestData.MultiplyFloats(dvalue, 2324.0);               ulvalue = TestData.MultiplyULongs(dvalue, fvalue);            ";
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "bytevalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "dvalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "fvalue", ExpectedValue = null, BlockIndex = 0 },                                      new ValidationData { ValueName = "ulvalue", ExpectedValue = null, BlockIndex = 0 }                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable()
        {
            String code =
            @"               primes = TestData.GetSomePrimes();               prime = primes[5];            ";
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "prime", ExpectedValue = 13, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable2()
        {
            String code =
            @"               nums = TestData.GetNumbersByDouble(10);               num = nums[5];            ";
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable3()
        {
            String code =
            @"               nums = TestData.DoubleThem({1,2,3,4,5});               num = nums[4];            ";
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },                                    };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetIEnumerable(); // creates an IEnumerable class and returns as an 'object'                   t2 = t.TestIEnumerable(t1);  // implicitly casts the 'object' to IEnumerable based on the argument 'type', and tests its value                   ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetIEnumerable(); // creates an IEnumerable class and returns as an 'object'                   t2 = t.TestIEnumerable2(t1);  // explicitly casts the 'object' to IEnumerable inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetInt();     // creates an int and returns as 'object'                   t2 = t.TestInt(t1);  // implicitly casts the 'object' to int based on the argument 'type', and tests its value                  ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetInt();     // creates an int and returns as 'object'                   t2 = t.TestInt2(t1);  // explicitly casts the 'object' to int inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetUlong();     // creates a 'ulong' and returns as 'object'                   t2 = t.TestUlong(t1);  // implicitly casts the 'object' to ulong based on the argument 'type', and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.GetUlong();     // creates a 'ulong' and returns as 'object'                   t2 = t.TestUlong2(t1);  // explicitly casts the the 'object' to ulong inside the function, and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Over_Internal_Classes()
        {
            string code =
                @" t = TestData.TestData();                   t1 = t.CreateInternalClass(5); // creates an internal class returned as an 'object'                   t2 = t.TestInternalClass(t1);  // internally converts the 'object' to the class and tests its value                 ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 5, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
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
            Type dummy = Type.GetType("ProtoFFITests.TestData");
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
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_MethodOverloading_In_Csharp_Classes()
        {
            string code =
                @" t = MethodOverloadingClass.MethodOverloadingClass();                   t1 = t.GetValue();                   t2 = t.foo(t1);                                     ";
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 0, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.MethodOverloadingClass");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_Dictionary()
        {
            string code =
                @" t = TestData.TestData();                   d = t.GetDictionary();                     r1 = d[""weight""];";
            ValidationData[] data = { new ValidationData { ValueName = "r1", ExpectedValue = 42, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
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
            Type dummy = Type.GetType("ProtoFFITests.TestData");
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
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }
    }
}
