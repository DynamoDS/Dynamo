using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Extensions;

namespace Dynamo.Wpf.Extensions
{
    public class ViewStartupParams
    {
        /// <summary>
        /// A handle to the extensions that are already constructed in the Model layer
        /// </summary>
        public IExtensionManager extensionManager;

        // TBD MAGN-7366
        //
        // Implementation notes:
        // 
        // This should be designed primarily to support the separation of the Package Manager from Core
        // and minimize exposing unnecessary innards.
        //
        // It is expected that this class will be extended in the future, so it should stay as minimal as possible.
    }
}
