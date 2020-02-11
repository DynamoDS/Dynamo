using Dynamo.Logging;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Interface to Dynamo's recordable command framework
    /// </summary>
    public interface ICommandExecutive : ILogSource
    {
        /// <summary>
        /// Endpoint method to execute any commands deriving from RecordableCommand
        /// </summary>
        /// <param name="command">Dynamo command</param>
        /// <param name="uniqueId">extension unique id</param>
        /// <param name="extensionName">extension name</param>
        void ExecuteCommand(DynamoModel.RecordableCommand command, string uniqueId, string extensionName);
    }
}
