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
using System.Linq;
using System.Text;

using System.Windows.Controls;

using Dynamo;
using Dynamo.Nodes;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;

using Microsoft.FSharp.Collections;

using System.Windows;
using System.Xml;
using Microsoft.FSharp.Core;

using Autodesk.Revit.UI.Selection;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Analysis;//MDJ needed for spatialfeildmanager

using Dynamo.Revit;

using Autodesk.ASM;

// DS
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoFFI;

namespace Dynamo.Applications
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DesignScriptNodeHelper : Autodesk.Revit.UI.IExternalApplication
    {
        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        {
            Autodesk.ASM.State.Start();
            Autodesk.ASM.State.StartViewer();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            Autodesk.ASM.State.ClearPersistedObjects();
            Autodesk.ASM.OUT.Reset();

            Autodesk.ASM.State.StopViewer();
            bool is_plugin = true;
            Autodesk.ASM.State.Stop(is_plugin);

            return Result.Succeeded;
        }
    }
}

namespace Dynamo.Nodes
{
    [NodeName("DesignScript Script")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING)]
    [NodeDescription("Runs an embedded DesignScript script")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class dynDesignScript : dynRevitTransactionNodeWithOneOutput
    {
        bool dirty = false;

        string temp_dir = "C:\\Temp\\";

        string script;

        ProtoCore.Core core;
        bool coreSet = false;

        List<Autodesk.Revit.DB.Element> created_elements = new List<Element>();

        public dynDesignScript()
        {
            //add an edit window option to the 
            //main context window
            System.Windows.Controls.MenuItem editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;
            MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);

            InPortData.Add(new PortData("IN", "A list of objects", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the DesignScript script", typeof(object)));

            RegisterAllPorts();

            UpdateLayout();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement script = xmlDoc.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = this.script;
            dynEl.AppendChild(script);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Script")
                    //this.tb.Text = subNode.InnerText;
                    script = subNode.InnerText;
            }
        }

        private void ClearCreatedElements()
        {
            foreach (Autodesk.Revit.DB.Element elem in created_elements)
            {
                // sometimes the Revit element gets lost or doesn't import
                // correctly, so DeleteElement throws an exception
                try
                {
                    DeleteElement(elem.Id);
                }
                catch (System.Exception)
                {
                }
            }

            created_elements.Clear();
        }

        ~dynDesignScript()
        {
        }

        public override void Cleanup()
        {
            if (coreSet)
                core.Cleanup();

            Autodesk.ASM.State.ClearPersistedObjects();
            Autodesk.ASM.OUT.Reset();
        }

        private object ConvertInput(Value val)
        {
            if (val.IsNumber)
            {
                return (object)((Value.Number)val).Item;
            }
            else if (val.IsContainer)
            {
                return (object)((Value.Container)val).Item;
            }

            return null;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (coreSet)
            {
                Cleanup();
                ClearCreatedElements();
            } 
            else
                coreSet = true;

            Dictionary<string, object> context = new Dictionary<string, object>();

            if (args[0].IsList)
            {
                FSharpList<Value> containers = Utils.SequenceToFSharpList(
                    ((Value.List)args[0]).Item);

                List<object> binding_objects = new List<object>();

                foreach (Value val in containers)
                {
                    binding_objects.Add(ConvertInput(val));
                }

                context.Add("IN", binding_objects.ToArray());
            }
            else
            {
                context.Add("IN", ConvertInput(args[0]));
            }

            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, 
                new ProtoFFI.CSModuleHelper());

            ExecutionMirror mirror = fsr.Execute(script, core, context);

            FSharpList<Value> created_objects = FSharpList<Value>.Empty;

            // These are the objects added in the DesignScript script
            List<object> output_objects = Autodesk.ASM.OUT.Objects();

            foreach (object o in output_objects)
            {
                Autodesk.DesignScript.Geometry.Point p = o as Autodesk.DesignScript.Geometry.Point;

                if (p != null)
                {
                    Autodesk.Revit.DB.XYZ xyz = new Autodesk.Revit.DB.XYZ(p.X, p.Y, p.Z);
                    ReferencePoint elem = Dynamo.Utilities.dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(xyz);
                    created_elements.Add(elem);

                    continue;
                }

                Autodesk.DesignScript.Geometry.Line l = o as Autodesk.DesignScript.Geometry.Line;

                if (l != null)
                {
                    ReferencePoint start_point = Dynamo.Utilities.dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(
                        new Autodesk.Revit.DB.XYZ(l.StartPoint.X, l.StartPoint.Y, 
                            l.StartPoint.Z));
                    ReferencePoint end_point = Dynamo.Utilities.dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(
                        new Autodesk.Revit.DB.XYZ(
                        l.EndPoint.X, l.EndPoint.Y, l.EndPoint.Z));

                    ReferencePointArray point_array = new ReferencePointArray();
                    point_array.Append(start_point);
                    point_array.Append(end_point);

                    CurveByPoints curve_elem = Dynamo.Utilities.dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(
                        point_array);

                    created_elements.Add(curve_elem);

                    continue;
                }

                Autodesk.DesignScript.Geometry.Geometry g = o as Autodesk.DesignScript.Geometry.Geometry;

                if (g == null)
                    continue;

                System.Guid guid = System.Guid.NewGuid();

                string temp_file_name = temp_dir + guid.ToString() + ".sat";

                try
                {
                    g.ExportToSAT(temp_file_name);

                    Autodesk.Revit.DB.SATImportOptions options = new
                        Autodesk.Revit.DB.SATImportOptions();

                    // TODO: get this from the current document. This should be 
                    //       synced with the "default" unit used for ReferencePoints
                    options.Unit = ImportUnit.Foot;

                    Autodesk.Revit.DB.ElementId new_id =
                        Dynamo.Utilities.dynRevitSettings.Doc.Document.Import(
                        temp_file_name, options,
                        Dynamo.Utilities.dynRevitSettings.Doc.ActiveView);

                    created_elements.Add(Dynamo.Utilities.dynRevitSettings.Doc.Document.GetElement(new_id));
                }
                catch (System.Exception)
                {
                    continue;
                }
            }

            foreach (Autodesk.Revit.DB.Element elem in created_elements)
            {
                Value element = Value.NewContainer(elem);
                created_objects = FSharpList<Value>.Cons(element, created_objects);
            }

            return Value.NewList(created_objects);
        }

        void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            dynEditWindow editWindow = new dynEditWindow();

            //set the text of the edit window to begin
            editWindow.editText.Text = script;

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            script = editWindow.editText.Text;

            this.dirty = true;
        }
    }

    //[NodeName("DesignScript Script From String")]
    //[NodeCategory(BuiltinNodeCategories.SCRIPTING)]
    //[NodeDescription("Runs a DesignScript script from a string")]
    //public class dynDesignScriptString : dynNodeWithOneOutput
    //{
    //    public dynDesignScriptString()
    //    {
    //        InPortData.Add(new PortData("script", "Script to run", typeof(string)));
    //        InPortData.Add(new PortData("IN", "Input", typeof(object)));
    //        OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

    //        NodeUI.RegisterAllPorts();
    //    }

    //    public override Value Evaluate(FSharpList<Value> args)
    //    {
    //        return Value.NewContainer(true);
    //    }
    //}
}
