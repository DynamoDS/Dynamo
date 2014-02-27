using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class DisposeTracer : IDisposable
    {
        static public int DisposeCount { get; set; }


        public void Dispose()
        {
            DisposeCount++;
        }
    }

    public class DerivedDisposeTracer : DisposeTracer
    {
    }


    public class DerivedOverriddedDisposeTracer : DisposeTracer
    {
        new void Dispose()
        {
            DisposeCount = 42;
        }
    }
}
