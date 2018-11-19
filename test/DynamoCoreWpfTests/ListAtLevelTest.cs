using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SystemTestServices;
using System.IO;

namespace DynamoCoreWpfTests
{

    [TestFixture]
    public class ListAtLevelInteractionTests : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void ShouldKeepListStructureChangesOutput()
        {
            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"UI\keepListStructure.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            var rankNode = this.Model.CurrentWorkspace.Nodes.Where(x => x.Name == "List.Rank").FirstOrDefault();
            Assert.NotNull(rankNode);
            Assert.AreEqual(4, this.Model.CurrentWorkspace.Nodes.Count());
            RunCurrentModel();

            var rank = rankNode.CachedValue.Data;
            Assert.AreEqual(2, rank);

            var addNode = this.Model.CurrentWorkspace.Nodes.Where(x => x.Name == "+").FirstOrDefault();
            Assert.NotNull(addNode);
            addNode.InPorts.First().KeepListStructure = true;

            RunCurrentModel();
            rank = rankNode.CachedValue.Data;
            Assert.AreEqual(3, rank);
        }
    }

    [TestFixture]
    public class ListAtLevelTest : RecordedUnitTestBase
    {
        [Test, RequiresSTA]
        public void Test01()
        {
            var listNode = "e4988561-5a7c-4936-8ba4-e07fda0dd733";

            RunCommandsFromFile("listatlevel-01.xml", (commandTag) =>
            {
                if (commandTag == "UseLevel2")
                {
                    AssertPreviewValue(listNode, new string[] { "foo", "qux" });
                }
                else if (commandTag == "UseLevel1")
                {
                    AssertPreviewValue(listNode, new string[] { "foo", "bar", "qux", "quz" });
                }
                else if (commandTag == "UseLevel3")
                {
                    AssertPreviewValue(listNode, new object[] { new string[] { "foo", "bar" } });

                }
                else if (commandTag == "Undo_UseLevel1")
                {
                    AssertPreviewValue(listNode, new string[] { "foo", "bar", "qux", "quz" });
                }
                else if (commandTag == "Undo_UseLevel2")
                {
                    AssertPreviewValue(listNode, new string[] { "foo", "qux" });
                }
            });
        }
    }
}
