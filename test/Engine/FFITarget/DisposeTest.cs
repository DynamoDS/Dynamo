using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DisposeTestClassA : IDisposable
    {
        static public int count { get; set; }

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
        static public int s_dispose { get; set; }

        public DisposeTestClassD()
        {
        }

        public DisposeTestClassD(int n)
        {
        }

        public void Dispose()
        {
            s_dispose = s_dispose + 1;
        }
    }
}
