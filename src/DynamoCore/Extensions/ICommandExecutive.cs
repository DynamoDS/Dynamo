using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandExecutive
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        void ExecuteCommand(DynamoModel.RecordableCommand command);
    }
}
