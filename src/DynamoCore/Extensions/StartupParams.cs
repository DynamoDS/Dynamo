using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when the 
    /// Dynamo is starting up and is ready for interaction.  
    /// 
    /// Specifically, this method is invoked 
    /// </summary>
    public class StartupParams
    {
        // TBD MAGN-7366
        //
        // Implementation notes:
        // 
        // This should be designed primarily to support the separation of the Package Manager from Core
        // and minimize exposing unnecessary innards.
        //
        // It is expected that this class will be extended in the future, so it should stay as minimal as possible.
        //
    }
}
