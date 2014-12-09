using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    public class WorkspacePathResponse : Response
    {
        /// <summary>
        /// Guid of the specified workspace.
        /// Home workspace should have empty string as guid
        /// </summary>
        public string Guid
        {
            get { return guid; }
            private set { guid = value != null ? value : string.Empty; }
        }

        string guid;

        /// <summary>
        /// Path where the workspace is saved
        /// </summary>
        public string Path { get; private set; }

        public WorkspacePathResponse(string guid, string path)
        {
            this.Guid = guid;
            this.Path = path;
        }
    }
}
