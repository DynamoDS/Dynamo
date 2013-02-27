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
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Linq;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Nodes
{
    [NodeName("Color Brightness")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Calculates a color's brightness.")]
    class dynColorBrightness : dynNode
    {
        public dynColorBrightness()
        {
            InPortData.Add(new PortData("c", "The color", typeof(object)));
            OutPortData = new PortData("mag", "The magnitude of the color's vector", typeof(double));

            NodeUI.RegisterInputsAndOutput();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Color c = (Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetBrightness());
        }

    }
}
