using Dynamo.Core;
using Dynamo.Interfaces;
using Greg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Library;

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

        public IPreferences Preferences { get { return preferences; } }
        private readonly IPreferences preferences;
        
        public IPathManager PathManager { get { return pathManager; } }
        private readonly IPathManager pathManager;

        public ILibraryLoader LibraryLoader { get { return libraryLoader; } }
        private readonly ILibraryLoader libraryLoader;

        public ICustomNodeManager CustomNodeManager { get { return customNodeManager; } }
        private readonly ICustomNodeManager customNodeManager;

        public Version DynamoVersion { get { return dynamoVersion; } }
        private readonly Version dynamoVersion;

        public StartupParams(IAuthProvider provider, IPathManager pathManager,
            ILibraryLoader libraryLoader, ICustomNodeManager customNodeManager,
            Version dynamoVersion, IPreferences preferences)
        {
            this.authProvider = provider;
            this.pathManager = pathManager;
            this.libraryLoader = libraryLoader;
            this.customNodeManager = customNodeManager;
            this.dynamoVersion = dynamoVersion;
            this.preferences = preferences;
        }
    }
}
