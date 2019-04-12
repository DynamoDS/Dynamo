using System.Linq;
using NUnit.Framework;
using ProtoCore.Mirror;

namespace ProtoTest.CoreUnitTests
{
    class StaticMirrorTest : ProtoTestBase
    {
        [Test]
        public void TestGetAllTypesContainPrimitiveType()
        {
            var types = StaticMirror.GetAllTypes(core);
            Assert.IsTrue(types.Any(cm => cm.ClassName.Equals("int")));
            Assert.IsTrue(types.Any(cm => cm.ClassName.Equals("double")));
        }
    }
}
