using Dynamo.Models;

namespace Dynamo.Nodes
{
    public abstract class GeometryBase : NodeWithOneOutput
    {
        protected GeometryBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }
} 
