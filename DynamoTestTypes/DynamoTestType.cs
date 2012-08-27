//Copyright 2012 Ian Keough

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
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Controls;
using Dynamo.Connectors;

//PUT YOUR ELEMENT IN THE Dynamo.Elements NAMESPACE!!
namespace Dynamo.Nodes
{
    //Every dynamo type has three attributes
    //1. The name of the node which will appear above the node on the workbench
    //2. A description for the node which will be used for searching for node types.
    //3. A flag to specifiy whether this node requires a transaction. If you're making Revit stuff,
    //chances are you'll need a transaction. Otherwise, you can speed things up a bit by not making
    //the node request a transaction.
    [ElementName("User Node")]
    [ElementDescription("This is a template for a user node.")]
    [RequiresTransaction(true)]
    public class TestNode : dynElement, IDynamic
    {

        /// <summary>
        /// The constructor for your type is where you define inputs and outputs.
        /// </summary>
        /// <param name="nickName"></param>
        public TestNode()
        {
            //System.Uri resourceLocater = new System.Uri("/dynElement.xaml", UriKind.Relative);
            //System.Windows.Application.LoadComponent(this, resourceLocater);

            //Define the data for the input ports
            //For each port you'll define an object value - this is usually null to start,
            //a short name which will appear on the node, a tooltip which appears when
            //you hover over the node, and a type which will keep nodes from connecting to
            //others of the wrong type
            InPortData.Add(new PortData("A", "The first port", typeof(dynElement)));
            InPortData.Add(new PortData("B", "The second port", typeof(dynElement)));
            InPortData.Add(new PortData("C", "The third port", typeof(dynElement)));

            //The ouput data looks very similar
            OutPortData = new PortData("me", "The result of this node.", typeof(dynElement));
            
            //make the first output port's object the tree
            //from this object. this isn't required yet by the interface, but it's
            //nice if you want the results of this node to be
            //a tree.
            //Don't worry about creating the tree, that's done in the base class.
            //OutPortData[0].Object = this.Tree;

            //tell the base type to setup for
            //the number of inputs and outputs
            base.RegisterInputsAndOutputs();

        }

        /// <summary>
        /// The Draw method is used to create geometry or process this nodes inputs.
        /// It is required by the IDynamic interface.
        /// </summary>
        public override void Draw()
        {
            //Check the inputs to make
            //sure that none of them
            //are null
            if (CheckInputs())
            {
                //THIS IS WHERE YOU PUT ALL OF YOUR GREAT STUFF!!!

                //Here you can create Revit elements an put them into this
                //objects Elements list

                //-OR-

                //you can also create elements while processing 
                //this object's tree
                Process(this.Tree.Trunk);
            }
            base.Draw();
        }

        /// <summary>
        /// Process is an optional recursive method for dealing with this objects' DataTree
        /// In future versions, it will likely be required by the interface.
        /// </summary>
        /// <param name="branch"></param>
        public void Process(DataTreeBranch branch)
        {
            foreach (object o in branch.Leaves)
            {
                //do something
            }

            foreach (DataTreeBranch subBranch in branch.Branches)
            {
                //recurse through all sub branches
                //of the tree
                Process(subBranch);
            }
        }

        /// <summary>
        /// The Destroy method is used to cleanup after yourself. Objects that you've
        /// created and placed in the Elements list will be destroyed by the base class.
        /// It is required by the IDynamic interface.
        /// </summary>
        public override void Destroy()
        {
            //if you need to clean up local variables
            //do it here.
            //if you've put any Revit elements in the 'Elements' list
            //they'll be destroyed 
            base.Destroy();
        }

        /// <summary>
        /// The Update method triggers the processing of down stream nodes.
        /// You shouldn't have to mess with this.
        /// </summary>
        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }
}
