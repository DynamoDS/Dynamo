using System;
using Autodesk.DesignScript.Runtime;

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
        private int? num = null; 

        public HiddenDisposer(HiddenDisposeTracer tracer)
        {
            this.tracer = tracer;
            num = 123;
        }

        public void Dispose()
        {
            num = null;
            tracer.DisposeCount += 1;
        }

        public override int GetHashCode()
        {
            if (num == null)
                throw new Exception("num");
            return num.GetHashCode();
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
