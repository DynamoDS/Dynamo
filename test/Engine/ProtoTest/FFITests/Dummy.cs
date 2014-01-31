using System;
using System.Collections.Generic;
namespace ProtoFFITests
{
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
}
