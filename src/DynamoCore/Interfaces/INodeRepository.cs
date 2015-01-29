
using System;
using Dynamo.Models;

namespace Dynamo.Interfaces
{
    public interface INodeRepository
    {
        NodeModel GetNodeModel(Guid guid);
    }

}
