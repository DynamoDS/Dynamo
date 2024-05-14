using Dynamo.NodeAutoComplete;
using NUnit.Framework;

namespace DynamoCoreWpfTests.ViewExtensions
{
    class NodeAutoCompleteViewExtensionTests : DynamoTestUIBase
    {
        private NodeAutoCompleteViewExtension viewExtension = new NodeAutoCompleteViewExtension();

        [TestOnSeparateThread]
        public void TestPropertiesWithCodeInit()
        {
            Assert.AreEqual("Node Auto Complete", viewExtension.Name);

            Assert.AreEqual("64F28473-0DCB-4E41-BB5B-A409FF6C90AD", viewExtension.UniqueId);
        }
    }
}
