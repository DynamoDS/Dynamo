using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoTestFx.TD;

namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class AstFactoryTest : ProtoTestBase
    {
        [Test]
        public void Test_AddReplicationGuide()
        {
            var variable = AstFactory.BuildIdentifier("foo");

            var guideList = new List<int> {1, 2, 3};
            var newVariable = AstFactory.AddReplicationGuide(variable, guideList, true);

            // Verify replication guide is properyly added to the node
            var nodeWithRP = newVariable as ArrayNameNode;
            Assert.IsNotNull(nodeWithRP);

            var guides = nodeWithRP.ReplicationGuides.Select(r => (r as ReplicationGuideNode).RepGuide as IdentifierNode)
                                                     .Select(i => Int32.Parse(i.Value));
            Assert.IsTrue(guides.SequenceEqual(guideList));

            var lacings = nodeWithRP.ReplicationGuides.Select(r => (r as ReplicationGuideNode).IsLongest);
            Assert.IsTrue(lacings.All(x => x));

            // Verify old node is not modified.
            var oldRP = (variable as ArrayNameNode).ReplicationGuides;
            Assert.AreEqual(0, oldRP.Count);
        }
    }
}
