using Dynamo.Models;

namespace Dynamo.Interfaces
{
    public interface ICustomNodeSource
    {
        NodeModel NewInstance();
    }
}