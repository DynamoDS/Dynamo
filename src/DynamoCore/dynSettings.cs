//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using Dynamo.PackageManager;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        public static HashSet<FunctionDefinition> FunctionWasEvaluated =
            new HashSet<FunctionDefinition>();

        static dynSettings()
        {
        }

        public static ObservableDictionary<string, Guid> CustomNodes {
            get
            {
                return dynSettings.Controller.CustomNodeManager.NodeNames;
            }
        }

        public static PackageLoader PackageLoader { get; internal set; }

        public static CustomNodeManager CustomNodeManager { get { return Controller.CustomNodeManager; } }

        public static DynamoController Controller { get; internal set; }

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