using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
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
