using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    public class HasUnsavedChangesResponse : Response
    {
        /// <summary>
        /// Guid of the specified workspace.
        /// Home workspace should have empty string as guid
        /// </summary>
        public string Guid
        {
            get { return guid; }
            private set
            {
                System.Guid guidValue;
                if (System.Guid.TryParse(value, out guidValue))
                    guid = value;
                else
                    guid = string.Empty;
            }
        }

        string guid;

        /// <summary>
        /// Path where the workspace is saved
        /// </summary>
        public bool HasUnsavedChanges { get; private set; }

        public HasUnsavedChangesResponse(string guid, bool hasUnsavedChanges)
        {
            Guid = guid;
            HasUnsavedChanges = hasUnsavedChanges;
        }
    }
}
