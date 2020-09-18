using System;

namespace FFITarget
{
    public abstract class AbstractDisposeTracert : IDisposable
    {
        static public int DisposeCount { get; set; }

        public virtual void Dispose()
        {
            DisposeCount++;
        }

        public int I { get; set; }
    }



    public class AbstractDerivedDisposeTracer2 : AbstractDisposeTracert
    {

        public AbstractDerivedDisposeTracer2()
        {

        }

        public AbstractDerivedDisposeTracer2(int i)
        {
            I = i;
        }

        public void StupidPlaceHolderMethod()
        {

        }


    }

}
