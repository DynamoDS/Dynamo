using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;

namespace TestServices
{
    public class TestPathResolverParams
    {
        public string CommonDataRootFolder { get; set; }
        public string UserDataRootFolder { get; set; }

        public TestPathResolverParams()
        {
            CommonDataRootFolder = string.Empty;
            UserDataRootFolder = string.Empty;
        }
    }

    public class TestPathResolver : IPathResolver
    {
        private readonly HashSet<string> additionalResolutionPaths;
        private readonly HashSet<string> additionalNodeDirectories;
        private readonly HashSet<string> preloadedLibraryPaths;
        private readonly string userDataRootFolder = string.Empty;
        private readonly string commonDataRootFolder = string.Empty;

        public TestPathResolver()
        {
            additionalResolutionPaths = new HashSet<string>();
            additionalNodeDirectories = new HashSet<string>();
            preloadedLibraryPaths = new HashSet<string>();
        }

        public TestPathResolver(TestPathResolverParams resolverParams)
        {
            additionalResolutionPaths = new HashSet<string>();
            additionalNodeDirectories = new HashSet<string>();
            preloadedLibraryPaths = new HashSet<string>();

            userDataRootFolder = resolverParams.UserDataRootFolder;
            commonDataRootFolder = resolverParams.CommonDataRootFolder;
        }

        public void AddNodeDirectory(string nodeDirectory)
        {
            if (!additionalNodeDirectories.Contains(nodeDirectory))
                additionalNodeDirectories.Add(nodeDirectory);
        }

        public void AddResolutionPath(string resolutionPath)
        {
            if (!additionalResolutionPaths.Contains(resolutionPath))
                additionalResolutionPaths.Add(resolutionPath);
        }

        public void AddPreloadLibraryPath(string preloadLibraryPath)
        {
            preloadedLibraryPaths.Add(preloadLibraryPath);
        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return additionalResolutionPaths; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return additionalNodeDirectories; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return preloadedLibraryPaths; }
        }

        public string UserDataRootFolder
        {
            get { return userDataRootFolder; }
        }

        public string CommonDataRootFolder
        {
            get { return commonDataRootFolder; }
        }
    }
}
