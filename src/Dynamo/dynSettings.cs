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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using System.Windows.Input;

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
                return dynSettings.Controller.CustomNodeLoader.NodeNames;
            }
        }

        public static Dynamo.Controls.DragCanvas Workbench { get; internal set; }

        public static DynamoView Bench { get; internal set; }

        public static DynamoController Controller { get; internal set; }

        public static PackageManagerClient PackageManagerClient { get; internal set; }

        public static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" }
                );
        }

        public static void ReturnFocusToSearch() {
            
            if ( Dynamo.Commands.ShowSearchCommand.search != null)
            {
                Keyboard.Focus(Dynamo.Commands.ShowSearchCommand.search.SearchTextBox );
            }

        }

        public static string RemoveChars(string s, IEnumerable<string> chars)
        {
            foreach (string c in chars)
                s = s.Replace(c, "");
            return s;
        }

    }
 
}