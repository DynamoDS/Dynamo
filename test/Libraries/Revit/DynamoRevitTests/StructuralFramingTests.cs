using NUnit.Framework;

using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class StructuralFramingTests : DynamoRevitUnitTestBase
    {
        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Beam()
        {
            OpenAndRun(@".\StructuralFraming\Beam.dyn");

            // Change type of elements

            // Change number of elements
        }

        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Brace()
        {
            OpenAndRun(@".\StructuralFraming\Brace.dyn");
        }

        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Column()
        {
            OpenAndRun(@".\StructuralFraming\Column.dyn");
        }
    }
}
