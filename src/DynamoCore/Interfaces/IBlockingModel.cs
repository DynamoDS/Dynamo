using System;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// This interface prescribes two events, one raised at the 
    /// begining of a blocking operation and one raised when
    /// the blocking operation is complete.
    /// </summary>
    public interface IBlockingModel
    {
        event EventHandler BlockingStarted;
        void OnBlockingStarted(EventArgs e);
        event EventHandler BlockingEnded;
        void OnBlockingEnded(EventArgs e);
    }
}
