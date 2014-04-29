using System;
using System.Collections.Generic;
using Dynamo.PackageManager;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        public static HashSet<CustomNodeDefinition> FunctionWasEvaluated =
            new HashSet<CustomNodeDefinition>();

        static dynSettings()
        {
        }

        public static ObservableDictionary<string, Guid> CustomNodes {
            get
            {
                return dynSettings.Controller.CustomNodeManager.GetAllNodeNames();
            }
        }

        public static PackageLoader PackageLoader { get; internal set; }

        public static CustomNodeManager CustomNodeManager { get { return Controller.CustomNodeManager; } }

        public static DynamoController Controller { get; set; }

        private static PackageManagerClient _packageManagerClient;
        public static PackageManagerClient PackageManagerClient { 
                get
                {
                    if (_packageManagerClient == null) _packageManagerClient = new PackageManagerClient();
                    return _packageManagerClient;
                }
        }

        public static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" }
                );
        }

        public static void ReturnFocusToSearch() {

            dynSettings.Controller.SearchViewModel.OnRequestReturnFocusToSearch(null, EventArgs.Empty);

        }

        public static string RemoveChars(string s, IEnumerable<string> chars)
        {
            foreach (string c in chars)
                s = s.Replace(c, "");
            return s;
        }
    }
 
}