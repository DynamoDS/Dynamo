using System;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Interfaces;
using Dynamo.Library;
using Greg;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when
    /// Dynamo is starting up and is not yet ready for interaction.  
    /// </summary>
    public class StartupParams
    {
        /// <summary>
        /// Returns <see cref="IAuthProvider"/> for DynamoModel
        /// </summary>
        public IAuthProvider AuthProvider { get { return authProvider; } }
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Returns <see cref="IPreferences"/> for DynamoModel
        /// </summary>
        public IPreferences Preferences { get { return preferences; } }
        private readonly IPreferences preferences;

        /// <summary>
        /// Returns <see cref="IPathManager"/> for DynamoModel
        /// </summary>
        public IPathManager PathManager { get { return pathManager; } }
        private readonly IPathManager pathManager;

        /// <summary>
        /// Returns <see cref="ILibraryLoader"/> for DynamoModel
        /// </summary>
        public ILibraryLoader LibraryLoader { get { return libraryLoader; } }
        private readonly ILibraryLoader libraryLoader;

        /// <summary>
        /// Returns <see cref="ICustomNodeManager"/> for DynamoModel
        /// </summary>
        public ICustomNodeManager CustomNodeManager { get { return customNodeManager; } }
        private readonly ICustomNodeManager customNodeManager;

        /// <summary>
        /// Defines version of Dynamo
        /// </summary>
        public Version DynamoVersion { get { return dynamoVersion; } }
        private readonly Version dynamoVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupParams"/> class.
        /// </summary>
        /// <param name="provider"><see cref="IAuthProvider"/> for DynamoModel</param>
        /// <param name="pathManager"><see cref="IPathManager"/> for DynamoModel</param>
        /// <param name="libraryLoader"><see cref="ILibraryLoader"/> for DynamoModel</param>
        /// <param name="customNodeManager"><see cref="ICustomNodeManager"/> for DynamoModel</param>
        /// <param name="dynamoVersion"><see cref="Version"/> for DynamoModel</param>
        /// <param name="preferences"><see cref="IPreferences"/> for DynamoModel</param>
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
