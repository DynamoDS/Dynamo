using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
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
