using System;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Interfaces;
using Dynamo.Library;
using Dynamo.Linting;
using Dynamo.Models;
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
        /// Returns Sessions Linter Manager
        /// </summary>
        public LinterManager LinterManager => linterManager;
        private readonly LinterManager linterManager;

        /// <summary>
        /// Returns true if ASM/LibG are loaded. May only be valid in sandbox sessions.
        /// </summary>
        internal bool IsGeometryLibraryLoaded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupParams"/> class.
        /// </summary>
        internal StartupParams(DynamoModel dynamoModel)
        {
            this.authProvider = dynamoModel.AuthenticationManager?.AuthProvider;
            this.pathManager = dynamoModel.PathManager;
            this.libraryLoader = new ExtensionLibraryLoader(dynamoModel);
            this.customNodeManager = dynamoModel.CustomNodeManager;
            this.dynamoVersion = new Version(dynamoModel.Version);
            this.preferences = dynamoModel.PreferenceSettings;
            this.linterManager = dynamoModel.LinterManager;
            this.IsGeometryLibraryLoaded = dynamoModel.IsASMLoaded;
        }

    }
}
