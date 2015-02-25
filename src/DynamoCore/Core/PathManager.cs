using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoCore.AST;

namespace Dynamo.Core
{
    public interface IPathManager
    {
        IEnumerable<string> NodeDirectories { get; }
    }

    class PathManager : IPathManager
    {
        private readonly HashSet<string> nodeDirectories;
        private readonly string dynamoCoreDir; // DynamoCore.dll directory.

        internal PathManager()
        {
            // This method is invoked in DynamoCore.dll, dynamoCorePath 
            // represents the directory that contains DynamoCore.dll.
            var dynamoCorePath = Assembly.GetExecutingAssembly().Location;
            dynamoCoreDir = Path.GetDirectoryName(dynamoCorePath);

            if (!File.Exists(Path.Combine(dynamoCoreDir, "DynamoCore.dll")))
            {
                throw new InvalidOperationException(
                    "PathManager starts off within an invalid assembly.");
            }

            nodeDirectories = new HashSet<string>();
            nodeDirectories.Add(Path.Combine(dynamoCoreDir, "nodes"));
        }

        public IEnumerable<string> NodeDirectories
        {
            get { return nodeDirectories; }
        }
    }
}
