using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Application level parameters pass to an extension when the DynamoView is loaded
    /// </summary>
    public class ViewLoadedParams
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
