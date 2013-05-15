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
using Dynamo.Nodes;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing a local node </summary>
    public class TopSearchElement : SearchElementBase
    {

        #region Properties

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public override string Type { get { return this.Element.Type; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        public override string Name { get { return this.Element.Name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        public override string Description { get { return this.Element.Description; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override double Weight { get { return this.Element.Weight; } set { this.Element.Weight = value; } }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override string Keywords { get { return this.Element.Keywords; } set { this.Element.Keywords = value; } }

        public SearchElementBase Element;

        #endregion

        /// <summary>
        /// The class constructor for a built-in type that is already loaded. </summary>
        /// <param name="node">The local node</param>
        public TopSearchElement(SearchElementBase ele)
        {
            this.Element = ele;
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            this.Element.Execute();
        }
    }

}
