using Dynamo.Core;
using Dynamo.Interfaces;
using Greg;
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
        public IAuthProvider AuthProvider { get { return authProvider; } }
        private readonly IAuthProvider authProvider;

        public IPathManager PathManager { get { return pathManager; } }
        private readonly IPathManager pathManager;

        public ICustomNodeManager CustomNodeManager { get { return customNodeManager; } }
        private readonly ICustomNodeManager customNodeManager;

        public StartupParams(IAuthProvider provider, IPathManager pathManager,
            ICustomNodeManager customNodeManager)
        {
            this.authProvider = provider;
            this.pathManager = pathManager;
            this.customNodeManager = customNodeManager;
        }
    }
}
