using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;

namespace TestServices
{
    public class TestPathResolver : IPathResolver
    {
        private readonly List<string> additionalResolutionPaths;
        private readonly List<string> additionalNodeDirectories;
        private readonly List<string> preloadedLibraryPaths;

        public TestPathResolver()
        {
            additionalResolutionPaths = new List<string>();
            additionalNodeDirectories = new List<string>();
            preloadedLibraryPaths = new List<string>();
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
            get { return string.Empty; }
        }

        public string CommonDataRootFolder
        {
            get { return string.Empty; }
        }
    }
}
