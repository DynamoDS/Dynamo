using System;

namespace FFITarget
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractDisposeTracert : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        static public int DisposeCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            DisposeCount++;
        }

        /// <summary>
        /// 
        /// </summary>
        public int I { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class AbstractDerivedDisposeTracer2 : AbstractDisposeTracert
    {
        /// <summary>
        /// 
        /// </summary>
        public AbstractDerivedDisposeTracer2()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public AbstractDerivedDisposeTracer2(int i)
        {
            I = i;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StupidPlaceHolderMethod()
        {

        }


    }

}
