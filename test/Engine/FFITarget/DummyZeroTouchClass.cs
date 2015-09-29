using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Properties;

namespace FFITarget
{
    public class DummyZeroTouchClass
    {
        [NodeDescription("Description")]
        public int FunctionWithDescription(int a)
        {
            return 0;
        }

        public int FunctionWithoutDescription(int a)
        {
            return 0;
        }
    }

    [InPortNames("par1", "par2")]
    [InPortTypes("int", "double")]
    [InPortDescriptions(typeof(Resources), "DescriptionResource1")]

    [OutPortNames("out1", "out2", "out3")]
    [OutPortTypes("1", "2", "3")]
    [OutPortDescriptions("desc")]
    public class DummyZeroTouchClass1
    {

    }
}
