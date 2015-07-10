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
    /// Application-level handles provided to an extension when
    /// Dynamo is starting up and is not yet ready for interaction.  
    /// </summary>
    public class StartupParams
    {
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
