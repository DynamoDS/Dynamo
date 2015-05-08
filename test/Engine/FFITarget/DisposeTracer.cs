using Autodesk.DesignScript.Runtime;
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

    [SupressImportIntoVM]
    public class HiddenDisposer : IDisposable
    {
        private HiddenDisposeTracer tracer = null;

        public HiddenDisposer(HiddenDisposeTracer t)
        {
            tracer = t;
        }

        public void Dispose()
        {
            tracer.DisposeCount *= 2;
        }
    }

    public class HiddenDisposeTracer
    {
        public int DisposeCount
        {
            get;
            set;
        }

        public HiddenDisposeTracer()
        {
            DisposeCount = 1;
        }

        public HiddenDisposer GetHiddenDisposer()
        {
            return new HiddenDisposer(this);
        }
    }
}
