using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.PackageManager;
using Dynamo.Utilities;

namespace Dynamo.Core
{
    public static class DynamoSettings
    {
        public static HashSet<CustomNodeDefinition> FunctionWasEvaluated =
            new HashSet<CustomNodeDefinition>();

        static DynamoSettings()
        {
        }

        public static ObservableDictionary<string, Guid> CustomNodes {
            get
            {
                return Controller.CustomNodeManager.GetAllNodeNames();
            }
        }

        public static PackageLoader PackageLoader { get; internal set; }

        public static CustomNodeManager CustomNodeManager { get { return Controller.CustomNodeManager; } }

        public static DynamoController Controller { get; internal set; }

        private static PackageManagerClient _packageManagerClient;

        public static PackageManagerClient PackageManagerClient
        {
            get { return _packageManagerClient ?? (_packageManagerClient = new PackageManagerClient()); }
        }

        public static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" }
                );
        }

        public static void ReturnFocusToSearch() {

            Controller.SearchViewModel.OnRequestReturnFocusToSearch(null, EventArgs.Empty);

        }

        public static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }
    }
 
}