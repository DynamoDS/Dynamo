using Dynamo.Models;
using Dynamo.Nodes;

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

    [InputParameters("par1", "par2")]
    [OutputParameters("item1", "item2", "item3")]
    public class DummyZeroTouchClass1
    {

    }
}
