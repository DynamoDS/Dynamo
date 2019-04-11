using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class ArrayMember : IDisposable
    {
        public IList<int> X { get; set; }

        public ArrayMember()
        {
        }

        public static ArrayMember Ctor(IList<int> array)
        {
            return new ArrayMember()
            {
                X = array
            };
        }

        public static ArrayMember Ctor(int a, int b)
        {
            return new ArrayMember()
            {
                X = new List<int>(){a,b}
            };
        }

        public IList<int> foo()
        {
            return new List<int>(){0,1,2,3,4,5,6,7,8,9,10};
        }

        public List<List<int>> foo2()
        {
            List<int> d1 = new List<int>(){1,2};
            List<int> d2 = new List<int>(){3,4};
            return new List<List<int>>() { d1, d2 };
        }

        public int Add()
        {
            int sum = 0;
            foreach (int n in X)
            {
                sum = sum + n;
            }
            return sum;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class BooleanMember : IDisposable
    {
        public bool a { get; set; }

        public BooleanMember()
        {
        }

        public BooleanMember(bool b)
        {
            a = b;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestObjectA : IDisposable
    {
        public int a { get; set; }

        public TestObjectA()
        {
        }

        public TestObjectA(int a1)
        {
            a = a1;
        }

        public void Set(int a1)
        {
            a = a1;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestObjectB : IDisposable
    {
        public int value { get; set; }

        public TestObjectB()
        {
        }

        public TestObjectB(int b1)
        {
            value = b1;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestObjectC : IDisposable
    {
        public int z { get; set; }

        public TestObjectC()
        {
        }

        public TestObjectC(int x1, int x2)
        {
            z = x1 + x2;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestObjectD : IDisposable
    {
        public int t { get; set; }

        public TestObjectD()
        {
        }

        public TestObjectD(int x1, int x2, double x3)
        {
            t = x1 + x2 + (int)x3;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class Integer : IDisposable
    {
        public int value { get; set; }

        public static Integer ValueCtor(int _value)
        {
            Integer ret = new Integer()
            {
                value = _value
            };
            return ret;
        }
        	
        public int Mul(Integer i1, Integer i2)
	    {
		    return i1.value * value * i2.value;
	    }

        public int GetValue()
        {
            return (int)(value * value);
        }

        public int GetIndex()
        {
            return (int)(value * value);
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class Point_1D : IDisposable
    {
        public double x { get; set; }

        public static Point_1D ValueCtor(int _x)
        {
            Point_1D ret = new Point_1D()
            {
                x = _x
            };
            return ret;
        }

        public int GetValue()
        {
            return (int)(x * x);
        }

        public int GetIndex()
        {
            return (int)(x * x);
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class Point_2D : IDisposable
    {
        public int x { get; set; }
        public int y { get; set; }

        public static Point_2D ValueCtor(int _x, int _y)
        {
            Point_2D ret = new Point_2D()
            {
                x = _x,
                y = _y,
            };
            return ret;
        }

        public int GetValue()
        {
            return x * y;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }


    public class Point_3D : IDisposable
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public static Point_3D ValueCtor(int _x, int _y, int _z)
        {
            Point_3D ret = new Point_3D()
            {
                x = _x,
                y = _y,
                z = _z
            };
            return ret;
        }

        public static Point_3D Point_1DCtor(Point_1D px, Point_1D py, Point_1D pz)
        {
            Point_3D ret = new Point_3D()
            {
                x = px.x,
                y = py.x,
                z = pz.x
            };
            return ret;
        }

        public static Point_3D PointOnXCtor(Point_1D p)
        {
            Point_3D ret = new Point_3D()
            {
                x = p.x,
                y = 0,
                z = 0
            };
            return ret;
        }

        public double GetIndexX()
        {
            return x * x;
        }

        public double GetCoor(int type)
        {
            return type == 1 ? x : type == 2 ? y : z;
        }

        public int GetValue()
        {
            return (int)(x + y + z);
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }


    public class TestSamePropertyName : IDisposable
    {
        public TestObjectA a { get; set; }

        public TestSamePropertyName()
        {
            a = null;
        }

        public TestSamePropertyName(TestObjectA obj)
        {
            a = obj;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestDefaultArgument : IDisposable
    {
        public int a { get; set; }
        public int b { get; set; }

        public TestDefaultArgument()
        {
            a = 0;
        }

        public TestDefaultArgument(int m = 1)
        {
            a = m;
        }

        public TestDefaultArgument(int m, int n = 2)
        {
            a = m;
            b = n;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class DummyTuple4 : IDisposable
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double H { get; set; }

        public DummyTuple4()
        {
        }

        public static DummyTuple4 ByCoordinates3(List<double> coordinates)
        {
            return new DummyTuple4()
            {
                X = coordinates[0],
                Y = coordinates[1],
                Z = coordinates[2],
                H = 1.0       
            };
        }

        public static DummyTuple4 XYZH(double xValue, double yValue, double zValue, double hValue)
        {
            return new DummyTuple4()
            {
                X = xValue,
                Y = yValue,
                Z = zValue,
                H = hValue
            };
        }

        public static DummyTuple4 XYZ(double xValue, double yValue, double zValue)
        {
            return new DummyTuple4()
            {
                X = xValue,
                Y = yValue,
                Z = zValue,
                H = 1.0
            };
        }

        public double Multiply(DummyTuple4 other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + H * other.H;
        }

        
        public List<double> Coordinates3()
        {
            return new List<double>(){ X, Y, Z };
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestOverloadConstructor : IDisposable
    {
        public int a { get; set; }
        public int b { get; set; }
        public int c { get; set; }

        public TestOverloadConstructor()
        {
            a = 0;
            b = 0;
            c = 0;
        }

        public TestOverloadConstructor(int x)
        {
            a = x;
            b = 0;
            c = 0;
        }

        public TestOverloadConstructor(int x, int y)
        {
            a = x;
            b = y;
            c = 0;
        }

        public TestOverloadConstructor(int x, int y, int z)
        {
            a = x;
            b = y;
            c = z;
        }

        public void Dispose()
        {
            //Don't do anything
        }
    }

    public class TestRankReduce
    {
        private string RankReduceTestField;

        [AllowRankReduction]
        public List<string> RankReduceProperty
        {
            get { return new List<string> { RankReduceTestField }; }
        }

        public List<string> Property
        {
            get { return new List<string> { RankReduceTestField }; }
        }

        public TestRankReduce(string s)
        {
            RankReduceTestField = s;
        }
        
        [AllowRankReduction]
        public List<string> RankReduceMethod()
        {
            return new List<string> { RankReduceTestField };
        }

        public List<string> Method()
        {
            return new List<string> { RankReduceTestField };
        }
    }
}
