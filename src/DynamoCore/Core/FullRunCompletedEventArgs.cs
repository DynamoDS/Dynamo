using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Core
{
    public class FullRunCompletedEventArgs : EventArgs
    {
        public bool WasRun { get; private set; }

        public FullRunCompletedEventArgs(bool wasRun)
        {
            WasRun = wasRun;
        }
    }
}
