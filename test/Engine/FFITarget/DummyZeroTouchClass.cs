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
}
