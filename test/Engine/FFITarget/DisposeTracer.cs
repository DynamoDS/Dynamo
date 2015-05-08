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

        public HiddenDisposer(HiddenDisposeTracer tracer)
        {
            this.tracer = tracer;
        }

        public void Dispose()
        {
            tracer.DisposeCount += 1;
        }
    }

    public class HiddenDisposeTracer
    {
        private int disposeCount = 0;
        public int DisposeCount
        {
            get
            {
                return disposeCount;
            }
            set
            {
                disposeCount = value;
            }
        }

        public HiddenDisposeTracer()
        {
            DisposeCount = 0;
        }

        public HiddenDisposer GetHiddenDisposer()
        {
            return new HiddenDisposer(this);
        }
    }
}
