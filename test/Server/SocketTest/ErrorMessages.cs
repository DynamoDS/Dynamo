using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests
{
    public static class WebServerErrorMessages
    {
        public const string NodeWasNotCreated = " node hasn't been created";
        public const string WrongCBNOutputPorts = "Code block node's output ports haven't been updated correctly";
        public const string WrongResponse = "Wrong Response";
        public const string NoSaving = "Saving hasn't occured";
        public const string WrongSavedFileContent = "Content of saved file is incorrect";
        public const string WrongPathInResponse = "Wrong file path in WorkspacePathResponse";
        public const string WrongNodesInResponse = "Wrong number of loaded nodes in NodeCreationDataResponse";
        public const string ProxyNodeWasNotUpdated = "Proxy custom node hasn't been updated";
    }
}
