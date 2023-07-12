using Dynamo.Applications;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PythonServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoApplications
{
    public sealed class PathManagerPreference
    {
        internal static readonly Lazy<PathManagerPreference>
            lazy =
            new Lazy<PathManagerPreference>
            (() => new PathManagerPreference());

        public static PathManagerPreference Instance { get { return lazy.Value; } }

        private PathManagerPreference()
        {
            IPathResolver pathResolver = CreateIPathResolver(false, "", "", "");
            PathManager = new PathManager(new PathManagerParams
            {
                CorePath = string.Empty,
                HostPath = string.Empty,
                PathResolver = pathResolver
            });

            Preferences = (PreferenceSettings)DynamoModel.CreateOrLoadPreferences(null, false, PathManager.PreferenceFilePath);
        }

        public IPathResolver CreateIPathResolver(bool CLImode, string preloaderLocation, string userDataFolder, string commonDataFolder)
        {
            IPathResolver pathResolver = CLImode ? new CLIPathResolver(preloaderLocation, userDataFolder, commonDataFolder) as IPathResolver : new SandboxPathResolver(preloaderLocation) as IPathResolver;
            return pathResolver;
        }

        public PathManager PathManager { get; set; }
        public PreferenceSettings Preferences { get; set; }
    }    
}
