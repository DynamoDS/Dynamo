using Dynamo.Models;

namespace Dynamo.Nodes
{
    public abstract class MeasurementBase:NodeWithOneOutput
    {
        protected MeasurementBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }
}


