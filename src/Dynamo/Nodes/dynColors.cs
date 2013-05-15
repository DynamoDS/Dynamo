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

using System.Drawing;
using Dynamo.Connectors;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Color Brightness")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Calculates a color's brightness.")]
    class dynColorBrightness : dynNodeWithOneOutput
    {
        public dynColorBrightness()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("mag", "The magnitude of the color's vector", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Color c = (Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetBrightness());
        }

    }
}
