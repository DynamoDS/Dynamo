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
            OutPortData.Add(new PortData("mag", "The magnitude of the color vector", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetBrightness());
        } 
    }

    [NodeName("Color Saturation")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Calculates a color's saturation.")]
    class dynColorSaturation : dynNodeWithOneOutput
    {
        public dynColorSaturation()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("sat", "The saturation of the color as a number between 0 and 1", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetSaturation());
        }
    }

    [NodeName("Color")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Make a color from its alpha, red, green, and blue components.")]
    [NodeSearchTags("argb")]
    class dynColor : dynNodeWithOneOutput
    {
        public dynColor()
        {
            InPortData.Add(new PortData("A", "The alpha part of the color between 0 and 255", typeof(Value.Number)));
            InPortData.Add(new PortData("R", "The red part of the color between 0 and 255", typeof(Value.Number)));
            InPortData.Add(new PortData("G", "The green part of the color between 0 and 255", typeof(Value.Number)));
            InPortData.Add(new PortData("B", "The blue part of the color between 0 and 255", typeof(Value.Number)));
            OutPortData.Add(new PortData("c", "The color", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = (int) Math.Round(((Value.Number) args[0]).Item);
            var r = (int) Math.Round(((Value.Number)args[1]).Item);
            var g = (int) Math.Round(((Value.Number)args[2]).Item);
            var b = (int) Math.Round(((Value.Number)args[3]).Item);

            return Value.NewContainer(Color.FromArgb(a, r, g, b));
        }
    }

    [NodeName("Color Components")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Separate a color into its alpha, red, green, and blue components.")]
    [NodeSearchTags("argb")]
    class dynColorComponents : dynNodeWithMultipleOutputs
    {
        public dynColorComponents()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("A", "The alpha part of the color between 0 and 255", typeof(Value.Number)));
            OutPortData.Add(new PortData("R", "The red part of the color between 0 and 255", typeof(Value.Number)));
            OutPortData.Add(new PortData("G", "The green part of the color between 0 and 255", typeof(Value.Number)));
            OutPortData.Add(new PortData("B", "The blue part of the color between 0 and 255", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Color)((Value.Container)args[0]).Item;

            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewNumber(c.B), results);
            results = FSharpList<Value>.Cons(Value.NewNumber(c.G), results);
            results = FSharpList<Value>.Cons(Value.NewNumber(c.R), results);
            results = FSharpList<Value>.Cons(Value.NewNumber(c.A), results);

            return Value.NewList(results);

        }
    }

    [NodeName("Color Hue")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Calculates a color's hue.")]
    class dynColorHue : dynNodeWithOneOutput
    {
        public dynColorHue()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("hue", "The hue of the color as a number between 0 and 1", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetHue());
        }
    }
}
