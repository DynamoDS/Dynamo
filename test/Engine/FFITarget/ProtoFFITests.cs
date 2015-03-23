using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
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
        public static IEnumerable GetNestedCollection()
        {
            return new List<object> { 2, 3, "DesignScript", new List<string> { "Dynamo", "Revit" }, new List<object> { true, new List<object> { 5.5, 10 } } };
        }
        public static IList RemoveItemsAtIndices(IEnumerable list, int[] indices)
        {
            return list.Cast<object>().Where((_, i) => !indices.Contains(i)).ToList();
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

        public override int GetHashCode()
        {
            return 10061;
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
        public int TestIEnumerable2([ArbitraryDimensionArrayImport] object x)
        {
            IEnumerable y = (IEnumerable)x;
            IEnumerator y2 = y.GetEnumerator();
            y2.Reset();
            y2.MoveNext();
            return (int)y2.Current;
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

        [MultiReturnAttribute(new string[]{"color", "weight", "ok"})]
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

        public static object ReturnObject(object x)
        {
            return x;
        }

        public static int GetDepth(IList arr)
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

        public static int SumList(IList arr)
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

        public static IList AddItemToFront(
            [ArbitraryDimensionArrayImport] object item,
            IList list)
        {
            var newList = new ArrayList { item };
            newList.AddRange(list);
            return newList;
        }

        public static double GetCircleArea([DefaultArgumentAttribute("TestData.GetFloat()")]double radius)
        {
            return radius * radius * Math.PI;
        }

        public static int MultiplyBy2WithWrongDefaultArgument(
            [DefaultArgumentAttribute("TestData.NonExistFunction()")] int x)
        {
            return x * 2;
        }

        public static int MultiplyBy3NonParsableDefaultArgument(
           [DefaultArgumentAttribute("%!48asfasd4")] int x)
        {
            return x * 3;
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

    public class Dummy : IDisposable
    {
        public Dummy()
        { }
        public void Dispose() // DesignScript VM when this object goes out of scope.
        {
            DummyProperty = 90;

        }
        protected Dummy(int a)
        {
            DummyProperty = a;
        }
        private int dummyProperty;
        public int DummyProperty
        {
            get
            {
                return dummyProperty;
            }
            set
            {
                dummyProperty = value;
            }
        }
        public static Dummy Create(int a)
        {
            return new Dummy(a);
        }
        public virtual bool CallMethod()
        {
            return true;
        }
        public static Dummy ReturnNullDummy()
        {
            return null;
        }
        public static int Return100()
        {
            return 100;
        }
        public double SumAll(double value)
        {
            return value;
        }
        public double SumAll(double[] arr)
        {
            var sum = 0.0;
            if (arr == null)
            {
                return sum;
            }

            var numElems = arr.Length;
            for (var idx = 0; idx < numElems; ++idx)
            {
                sum += arr[idx];
            }
            return sum;
        }
        public double SumAll(double[][] arr)
        {
            var sum = 0.0;
            if (arr == null)
                return sum;
            foreach (var item in arr)
                sum += SumAll(item);
            return sum;
        }
        //public double SumAll(double[,] arr)
        //{
        //    var sum = 0.0;
        //    if(arr == null)
        //        return sum;
        //    foreach (var item in arr)
        //        sum += SumAll(item);
        //}
        public double[] Twice(double[] arr)
        {
            if (arr == null)
            {
                return null;
            }
            var numElems = arr.Length;
            double[] doubledArr = new double[numElems];
            for (var idx = 0; idx < numElems; ++idx)
            {
                doubledArr[idx] = 2 * arr[idx];
            }
            return doubledArr;
        }
        public double AddAll(List<double> list)
        {
            double sum = 0;
            foreach (var item in list)
            {
                sum += item;
            }
            return sum;
        }
        public Stack<DummyBase> DummyStack()
        {
            Stack<DummyBase> stack = new Stack<DummyBase>();
            stack.Push(new DummyDispose());
            stack.Push(DummyBase.Create());
            stack.Push(TestDispose.Create());
            return stack;
        }
        public int StackSize(Stack<DummyBase> stack)
        {
            return stack.Count;
        }
        public Dictionary<string, int> CreateDictionary()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            return dict;
        }
        public Dictionary<string, int> AddData(Dictionary<string, int> dictionary, string name, int age)
        {
            dictionary.Add(name, age);
            return dictionary;
        }
        public int SumAges(Dictionary<string, int> dictionary)
        {
            int sum = 0;
            foreach (var item in dictionary)
            {
                sum += item.Value;
            }
            return sum;
        }
        public List<double> Range(double from, double to, double step)
        {
            List<double> list = new List<double>();
            for (double d = from; d <= to; d += step)
                list.Add(d);
            return list;
        }
        public object[] GetMixedObjects()
        {
            object[] objs = { new DerivedDummy(), new Derived1(), new TestDispose(), new DummyDispose() };
            return objs;
        }
    }


    public class DerivedDummy : Dummy
    {
        public override bool CallMethod()
        {
            return false;
        }
        public static Dummy CreateDummy(bool derived)
        {
            if (derived)
                return new DerivedDummy();
            return new Dummy();
        }
        public int random123()
        {
            return 123;
        }
    }
    public class Base
    {
        public static Base Create()
        {
            return new Base();
        }
        public static Base CreateDerived()
        {
            return new Derived1();
        }
        public virtual double GetNumber()
        {
            return 10;
        }
        public override bool Equals(Object obj)
        {
            return true;
        }
        public override int GetHashCode()
        {
            return 10093;
        }
    }
    public class Derived1 : Base
    {
        public static new Derived1 Create()
        {
            return new Derived1();
        }
        public override double GetNumber()
        {
            return 20;
        }
    }
    public class DummyBase
    {
        public static DummyBase Create()
        {
            return new DummyBase();
        }
        public DummyBase(int val = 5)
        {
            mValue = val;
        }
        private int mValue = 5;
        public int Value
        {
            get { return mValue; }
            set { mValue = value; }
        }
        public void SetValue(int val)
        {
            Value = val;
        }
        public int get_MyValue()
        {
            return Value;
        }
        public void set_MyValue(int val)
        {
            Value = val;
        }
        public int TestNullable(double? value, Nullable<int> intval)
        {
            if (value.HasValue)
                return 123;
            if (intval.HasValue)
                return 321;
            return 111;
        }
        public override string ToString()
        {
            string typename = GetType().Name;
            return string.Format("{0}(Value = {1})", typename, mValue);
        }
        public override bool Equals(Object obj)
        {
            DummyBase db = obj as DummyBase;
            if (null == db)
                return false;
            return mValue == db.mValue;
        }

        public override int GetHashCode()
        {
            return mValue;
        }
    }
    public class DummyDispose : DummyBase
    {
        public DummyDispose()
            : base(20)
        {
        }
        public void Dispose()
        {
            Value = -2;
        }
    }
    public class TestDispose : DummyBase, IDisposable
    {
        public static new TestDispose Create()
        {
            return new TestDispose { Value = 15 };
        }
        public void Dispose()
        {
            Value = -3;
        }
    }
    public class TestDisposeDerived : TestDispose
    {
        public static TestDisposeDerived CreateDerived()
        {
            return new TestDisposeDerived { Value = 10 };
        }
        public new void Dispose()
        {
            Value = -4;
        }
    }

    // the following classes are used to test Dispose method call on FFI
    public class DisposeVerify
    {
        public static int val = 10;
        public static DisposeVerify CreateObject()
        {
            return new DisposeVerify();
        }
        public static int GetValue()
        {
            return val;
        }
        public static int SetValue(int _val)
        {
            val = _val;
            return val;
        }
    }
    public class AClass : IDisposable
    {
        private int val;
        public static AClass CreateObject(int _val)
        {
            return new AClass { val = _val };
        }
        public int Value
        {
            get { return val; }
        }
        public void Dispose()
        {
            DisposeVerify.SetValue(val);
        }
    }
    public class BClass : IDisposable
    {
        private int val;
        public static BClass CreateObject(int _val)
        {
            return new BClass { val = _val };
        }
        public void Dispose()
        {
            DisposeVerify.val += val;
        }
    }
    public class NestedClass
    {
        public static Type GetType(int value)
        {
            return new Type(value);
        }
        public static bool CheckType(Type t, int value)
        {
            return t._x == value;
        }
        public class Type
        {
            public int _x;
            public Type(int x)
            {
                _x = x;
            }
            public bool Equals(Type obj)
            {
                return obj._x == this._x;
            }
        }
    }

    namespace DesignScript
    {
        public class Point
        {
            public static Point XYZ(double x, double y, double z)
            {
                var p = new Point { dX = x, dY = y, dZ = z };
                return p;
            }

            public double dX { get; set; }
            public double dY { get; set; }
            public double dZ { get; set; }
        }
    }

    namespace Dynamo
    {
        public class Point
        {
            public static Point XYZ(double x, double y, double z)
            {
                var p = new Point { X = x, Y = y, Z = z };
                return p;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }
    }
}
