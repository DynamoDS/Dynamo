using System;
using System.Collections.Generic;
using Dynamo.Interfaces;
using System.Linq;

using Dynamo.PackageManager;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        public static ObservableDictionary<string, Guid> CustomNodes
        {
            get { return Controller.CustomNodeManager.GetAllNodeNames(); }
        }

        public static PackageLoader PackageLoader { get; internal set; }
        public static CustomNodeManager CustomNodeManager { get { return Controller.CustomNodeManager; } }
        public static DynamoController Controller { get; set; }

        private static PackageManagerClient _packageManagerClient;
        public static PackageManagerClient PackageManagerClient
        {
            get { return _packageManagerClient ?? (_packageManagerClient = new PackageManagerClient()); }
        }

        public static ILogger DynamoLogger { get; set; }

        /// <summary>
        /// Setting this flag enables creation of an XML in following format that records 
        /// node mapping information - which old node has been converted to which to new node(s) 
        /// </summary>
        public static bool EnableMigrationLogging { get; set; }

        public static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" });
        }

        public static void ReturnFocusToSearch()
        {
            Controller.SearchViewModel.OnRequestReturnFocusToSearch(null, EventArgs.Empty);
        }

        public static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }
    }
}