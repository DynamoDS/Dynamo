using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class ElementResolverTests : RecordedUnitTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        [Test, RequiresSTA]
        public void RegressMagn7917()
        {
            RunCommandsFromFile("RegressMagn7917.xml", (commandTag) =>
            {
                if (commandTag == "UpdateCBN")
                {
                    foreach (var node in GetModel().CurrentWorkspace.Nodes.ToList())
                    {
                        Assert.IsTrue(node.State != ElementState.Warning); 
                    }
                }
                else if (commandTag == "UndoUpdate")
                {
                    foreach (var node in GetModel().CurrentWorkspace.Nodes.ToList())
                    {
                        Assert.IsTrue(node.State != ElementState.Warning); 
                    }
                }
                else if (commandTag == "UndoCopyUpdate")
                {
                    foreach (var node in GetModel().CurrentWorkspace.Nodes.ToList())
                    {
                        Assert.IsTrue(node.State != ElementState.Warning); 
                    }
                }
            });
        }
    }
}
