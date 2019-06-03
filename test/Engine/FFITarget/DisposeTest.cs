using System;

namespace FFITarget
{
    public class DisposeCounter : IDisposable
    {
        static public int x { get; set; }

        public static void Reset(int num)
        {
            x = num;
        }
        public void Dispose()
        {
        }
    }

    public class DisposeCounterTest : IDisposable
    {
        public int x { get; set; }

        public DisposeCounterTest()
        {
            x = 10;
        }
        public void Dispose()
        {
            DisposeCounter.x = DisposeCounter.x + 1;
        }
    }

    public class DisposeTestClassA : IDisposable
    {
        static public int count { get; set; }

        public static void Reset()
        {
            count = 0;
        }

        public DisposeTestClassA()
        {
            count = count + 1;
        }

        public void Dispose()
        {
            count = count - 1;
        }
    }

    public class DisposeTestClassB : IDisposable
    {
        static public int count { get; set; }

        public static void Reset()
        {
            count = 0;
        }

        public DisposeTestClassB()
        {
            count = count + 1;
        }

        public void Dispose()
        {
            count = count - 1;
        }
    }

    public class DisposeTestClassC : IDisposable
    {
        static public int count { get; set; }

        public static void Reset()
        {
            count = 0;
        }

        public DisposeTestClassC()
        {
            count = count + 1;
        }

        public void Dispose()
        {
            count = count - 1;
        }
    }

    public class DisposeTestClassD : IDisposable
    {
        static public int count { get; set; }

        public static void Reset()
        {
            count = 0;
        }

        public DisposeTestClassD()
        {
        }

        public DisposeTestClassD(int n)
        {
        }

        public void Dispose()
        {
            count = count + 1;
        }
    }
}
