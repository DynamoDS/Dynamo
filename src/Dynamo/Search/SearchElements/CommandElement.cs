//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing a command to execute</summary>
    public class CommandElement : SearchElementBase
    {

        #region Properties

        /// <summary>
        /// Command property </summary>
        /// <value>
        /// The command to be executed by this search element </value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        private string _type;
        public override string Type { get { return _type; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name { get { return _name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description { get { return _description; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override double Weight { get; set; }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override string Keywords { get; set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="name">The name of the command as shown in search</param>
        /// <param name="description">A description of what the command does, to be shown in search.</param>
        /// <param name="tags">Some descriptive terms to be shown in search.</param>
        /// <param name="command">The command to be execute in the Execute() method - with no parameters</param>
        public CommandElement(string name, string description, IEnumerable<string> tags, ICommand command)
        {
            this.Command = command;
            this._name = name;
            this.Weight = 1.2;
            this.Keywords = String.Join(" ", tags);
            this._type = "Command";
            this._description = description;
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(this.Command, null));
            dynSettings.Controller.ProcessCommandQueue();
        }

    }

}
