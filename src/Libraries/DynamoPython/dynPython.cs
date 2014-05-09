﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using DynamoPython;
using IronPython.Modules;

namespace Dynamo.Nodes
{
    [NodeName("LEGACY Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs an embedded IronPython script")]
    public class Python : NodeModel
    {
        //private bool _dirty = true;
        //private Value _lastEvalValue;

        ///// <summary>
        ///// Allows a scripter to have a persistent reference to previous runs.
        ///// </summary>
        //private readonly Dictionary<string, dynamic> _stateDict = new Dictionary<string, dynamic>();

        ///// <summary>
        ///// The script used by this node for execution.
        ///// </summary>
        //private string _script;

        //public Python()
        //{
        //    InPortData.Add(new PortData("IN", "Input"));
        //    OutPortData.Add(new PortData("OUT", "Result of the python script"));

        //    RegisterAllPorts();
        //    InitializeDefaultScript();

        //    ArgumentLacing = LacingStrategy.Disabled;
        //}

        //private void InitializeDefaultScript()
        //{
        //    _script = "# Default imports\n";

        //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        //    if (assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")))
        //    {
        //        _script = _script
        //            + "import clr\n"
        //            + "clr.AddReference('RevitAPI')\n"
        //            + "clr.AddReference('RevitAPIUI')\n"
        //            + "from Autodesk.Revit.DB import *\n"
        //            + "import Autodesk\n";
        //    }

        //    string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\dll";

        //    if (!assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
        //    {
        //        //LibG could not be found, possibly because we haven't used a node
        //        //that requires it yet. Let's load it...
        //        string libGPath = Path.Combine(dllDir, "LibG.Managed.dll");
        //        Assembly.LoadFrom(libGPath);

        //        //refresh the collection of loaded assemblies
        //        assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //    }

        //    if (assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
        //    {
        //        _script = _script + "import sys\n"
        //            + "import clr\n"
        //            + "path = r'C:\\Autodesk\\Dynamo\\Core'\n"
        //            + "exec_path = r'" + dllDir + "'\n"
        //            + "sys.path.append(path)\n"
        //            + "sys.path.append(exec_path)\n"
        //            + "clr.AddReference('LibG.Managed')\n"
        //            + "from Autodesk.LibG import *\n";
        //    }

        //    _script = _script + "\n"
        //        + "#The input to this node will be stored in the IN variable.\n"
        //        + "dataEnteringNode = IN\n\n"
        //        + "#Assign your output to the OUT variable\n"
        //        + "OUT = 0";
        //}

        //public void SetupCustomUIElements(dynNodeView nodeUI)
        //{
        //    //topControl.Height = 200;
        //    //topControl.Width = 300;

        //    //add an edit window option to the 
        //    //main context window
        //    var editWindowItem = new System.Windows.Controls.MenuItem
        //    {
        //        Header = "Edit...",
        //        IsCheckable = false
        //    };
        //    nodeUI.MainContextMenu.Items.Add(editWindowItem);
        //    editWindowItem.Click += delegate { EditScriptContent(); };
        //    nodeUI.UpdateLayout();

        //    nodeUI.MouseDown += nodeUI_MouseDown;
        //}

        //void nodeUI_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount >= 2)
        //    {
        //        EditScriptContent();
        //        e.Handled = true;
        //    }
        //}

        //// Property added for test case verification purposes
        //public string Script { get { return this._script; } }

        //protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        //{
        //    XmlElement script = xmlDoc.CreateElement("Script");
        //    //script.InnerText = this.tb.Text;
        //    script.InnerText = _script;
        //    nodeElement.AppendChild(script);
        //}

        //protected override void LoadNode(XmlNode nodeElement)
        //{
        //    foreach (XmlNode subNode in nodeElement.ChildNodes)
        //    {
        //        if (subNode.Name == "Script")
        //            //this.tb.Text = subNode.InnerText;
        //            _script = subNode.InnerText;
        //    }
        //}

        //private IEnumerable<KeyValuePair<string, Value>> MakeBindings(IEnumerable<Value> args)
        //{
        //    //Zip up our inputs
        //    var bindings = InPortData
        //       .Select(x => x.NickName)
        //       .Zip(args, (s, v) => new KeyValuePair<string, Value>(s, v))
        //       .ToList();

        //    //bindings.Add(new KeyValuePair<string, dynamic>("__persistent__", _stateDict));

        //    return bindings;
        //}

        ////public override Value Evaluate(FSharpList<Value> args)
        ////{
        ////    var bindings = new List<KeyValuePair<string, dynamic>>
        ////    {
        ////        new KeyValuePair<string, dynamic>("__persistent__", _stateDict)
        ////    };
        ////    Value result = PythonEngine.Evaluator(_dirty, _script, bindings, MakeBindings(args));
        ////    _lastEvalValue = result;

        ////    Draw();

        ////    return result;
        ////}

        //protected override bool UpdateValueCore(string name, string value)
        //{
        //    if (name == "ScriptContent")
        //    {
        //        this._script = value;
        //        this._dirty = true;
        //        return true;
        //    }

        //    return base.UpdateValueCore(name, value);
        //}

        //private void EditScriptContent()
        //{
        //    ScriptEditWindow editWindow = new ScriptEditWindow();
        //    editWindow.Initialize(this.GUID, "ScriptContent", this._script);
        //    editWindow.ShowDialog();
        //}

        //private void Draw()
        //{
        //    if(_lastEvalValue != null)
        //        PythonEngine.Drawing(_lastEvalValue, GUID.ToString());
        //}

        //#region SerializeCore/DeserializeCore

        //protected override void SerializeCore(XmlElement element, SaveContext context)
        //{
        //    base.SerializeCore(element, context);
        //    var helper = new XmlElementHelper(element);
        //    helper.SetAttribute("Script", this.Script);
        //}

        //protected override void DeserializeCore(XmlElement element, SaveContext context)
        //{
        //    base.DeserializeCore(element, context);
        //    var helper = new XmlElementHelper(element);
        //    var script = helper.ReadString("Script", string.Empty);
        //    this._script = script;
        //}

        //#endregion

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", "1");
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN\b", "IN[0]");
                element.AppendChild(node);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName("LEGACY Python Script With Variable Number of Inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs an embedded IronPython script")]
    public class PythonVarIn : VariableInput
    {
        //private bool _dirty = true;
        //private Value _lastEvalValue;

        ///// <summary>
        ///// Allows a scripter to have a persistent reference to previous runs.
        ///// </summary>
        //private readonly Dictionary<string, dynamic> _stateDict = new Dictionary<string, dynamic>();

        ///// <summary>
        ///// The script used by this node for execution.
        ///// </summary>
        //private string _script;

        //public PythonVarIn()
        //{
        //    InPortData.Add(new PortData("IN0", "Input0"));
        //    OutPortData.Add(new PortData("OUT", "Result of the python script"));

        //    RegisterAllPorts();
        //    InitializeDefaultScript();

        //    ArgumentLacing = LacingStrategy.Disabled;
        //}

        ///// <summary>
        ///// Set the number of inputs.  
        ///// </summary>
        ///// <param name="numInputs"></param>
        //public void SetNumInputs(int numInputs)
        //{
        //    if (numInputs <= 0)
        //    {
        //        return;
        //    }

        //    InPortData.Clear();

        //    for (var i = 0; i < numInputs; i++)
        //    {
        //        InPortData.Add(new PortData(GetInputRootName() + GetInputNameIndex(), ""));
        //    }

        //    RegisterAllPorts();
        //}

        // implement methods from variableinput
        protected override string GetInputRootName()
        {
            return "IN";
        }

        protected override string GetTooltipRootName()
        {
            return "Input";
        }

        //protected override void RemoveInput()
        //{
        //    if (InPortData.Count > 1)
        //    {
        //        base.RemoveInput();
        //    }   
        //}

        //private void InitializeDefaultScript()
        //{
        //    _script = "# Default imports\n";

        //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        //    if (assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")))
        //    {
        //        _script = _script
        //            + "import clr\n"
        //            + "clr.AddReference('RevitAPI')\n"
        //            + "clr.AddReference('RevitAPIUI')\n"
        //            + "from Autodesk.Revit.DB import *\n"
        //            + "import Autodesk\n";
        //    }

        //    string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\dll";

        //    if (!assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
        //    {
        //        //LibG could not be found, possibly because we haven't used a node
        //        //that requires it yet. Let's load it...
        //        string libGPath = Path.Combine(dllDir, "LibG.Managed.dll");
        //        Assembly.LoadFrom(libGPath);

        //        //refresh the collection of loaded assemblies
        //        assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //    }

        //    if (assemblies.Any(x => x.FullName.Contains("LibG.Managed")))
        //    {
        //        _script = _script + "import sys\n"
        //            + "import clr\n"
        //            + "path = r'C:\\Autodesk\\Dynamo\\Core'\n"
        //            + "exec_path = r'" + dllDir + "'\n"
        //            + "sys.path.append(path)\n"
        //            + "sys.path.append(exec_path)\n"
        //            + "clr.AddReference('LibG.Managed')\n"
        //            + "from Autodesk.LibG import *\n";
        //    }

        //    _script = _script + "\n"
        //        + "#The input to this node will be stored in the IN0...INX variable(s).\n"
        //        + "dataEnteringNode = IN0\n\n"
        //        + "#Assign your output to the OUT variable\n"
        //        + "OUT = 0";
        //}

        //public void SetupCustomUIElements(dynNodeView nodeUI)
        //{
        //    //topControl.Height = 200;
        //    //topControl.Width = 300;

        //    //add an edit window option to the 
        //    //main context window
        //    var editWindowItem = new System.Windows.Controls.MenuItem
        //    {
        //        Header = "Edit...",
        //        IsCheckable = false
        //    };
        //    nodeUI.MainContextMenu.Items.Add(editWindowItem);
        //    editWindowItem.Click += delegate { EditScriptContent(); };
        //    nodeUI.UpdateLayout();

        //    nodeUI.MouseDown += new MouseButtonEventHandler(nodeUI_MouseDown);
        //}

        //void nodeUI_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount >= 2)
        //    {
        //        EditScriptContent();
        //        e.Handled = true;
        //    }
            
        //}

        //// Property added for test case verification purposes
        //public string Script { get { return _script; } }

        //protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        //{
        //    XmlElement script = xmlDoc.CreateElement("Script");
        //    //script.InnerText = this.tb.Text;
        //    script.InnerText = this._script;
        //    nodeElement.AppendChild(script);

        //    // save the number of inputs
        //    nodeElement.SetAttribute("inputs", (InPortData.Count).ToString());
        //}

        //protected override void LoadNode(XmlNode nodeElement)
        //{
        //    var inputAttr = nodeElement.Attributes["inputs"];
        //    int inputs = inputAttr == null ? 1 : Convert.ToInt32(inputAttr.Value);
        //    this.SetNumInputs(inputs);

        //    var scriptNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "Script");
        //    if (scriptNode != null)
        //    {
        //        _script = scriptNode.InnerText;
        //    }
        //}

        //private IEnumerable<KeyValuePair<string, Value>> MakeBindings(IEnumerable<Value> args)
        //{
        //    //Zip up our inputs
        //    var bindings = InPortData
        //       .Select(x => x.NickName)
        //       .Zip(args, (s, v) => new KeyValuePair<string, Value>(s, v))
        //       .ToList();

        //    //bindings.Add(new KeyValuePair<string, dynamic>("__persistent__", _stateDict));

        //    return bindings;
        //}

        ////public override Value Evaluate(FSharpList<Value> args)
        ////{
        ////    var bindings = new List<KeyValuePair<string, dynamic>>
        ////    {
        ////        new KeyValuePair<string, dynamic>("__persistent__", _stateDict)
        ////    };
        ////    Value result = PythonEngine.Evaluator(_dirty, _script, bindings, MakeBindings(args));
        ////    _lastEvalValue = result;

        ////    Draw();

        ////    return result;
        ////}

        //protected override bool UpdateValueCore(string name, string value)
        //{
        //    if (name == "ScriptContent")
        //    {
        //        this._script = value;
        //        this._dirty = true;
        //        return true;
        //    }

        //    return base.UpdateValueCore(name, value);
        //}

        //private void EditScriptContent()
        //{
        //    ScriptEditWindow editWindow = new ScriptEditWindow();
        //    editWindow.Initialize(this.GUID, "ScriptContent", this._script);
        //    editWindow.ShowDialog();
        //}

        //private void Draw()
        //{
        //    if(_lastEvalValue != null)
        //        PythonEngine.Drawing(_lastEvalValue, GUID.ToString());
        //}

        //#region SerializeCore/DeserializeCore

        //protected override void SerializeCore(XmlElement element, SaveContext context)
        //{
        //    base.SerializeCore(element, context);
        //    var helper = new XmlElementHelper(element);
        //    helper.SetAttribute("Script", this.Script);
        //}

        //protected override void DeserializeCore(XmlElement element, SaveContext context)
        //{
        //    base.DeserializeCore(element, context);
        //    var helper = new XmlElementHelper(element);
        //    var script = helper.ReadString("Script", string.Empty);
        //    this._script = script;
        //}

        //#endregion

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", xmlNode.GetAttribute("inputs"));
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {

                
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN[0-9]+\b", delegate(Match m)
                {
                    return "IN[" + m.ToString().Substring(2) + "]";
                });
                element.AppendChild(subNode.Clone());
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName("LEGACY Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs a IronPython script from a string")]
    public class PythonString : NodeModel
    {

        ///// <summary>
        ///// Allows a scripter to have a persistent reference to previous runs.
        ///// </summary>
        //private readonly Dictionary<string, dynamic> _stateDict = new Dictionary<string, dynamic>();

        //public PythonString()
        //{
        //    InPortData.Add(new PortData("script", "Script to run"));
        //    InPortData.Add(new PortData("IN", "Input"));
        //    OutPortData.Add(new PortData("OUT", "Result of the python script"));

        //    RegisterAllPorts();

        //    ArgumentLacing = LacingStrategy.Disabled;
        //}

        //private IEnumerable<KeyValuePair<string, Value>> makeBindings(IEnumerable<Value> args)
        //{
        //    //Zip up our inputs
        //    var bindings = 
        //       InPortData
        //       .Select(x => x.NickName)
        //       .Zip(args, (s, v) => new KeyValuePair<string, Value>(s, v))
        //       //.Concat(PythonBindings.Bindings)
        //       .ToList();

        //    //bindings.Add(new KeyValuePair<string, dynamic>("__persistent__", _stateDict));

        //    return bindings;
        //}

        ////public override Value Evaluate(FSharpList<Value> args)
        ////{
        ////    var script = ((Value.String) args[0]).Item;
        ////    var inputs = makeBindings(args);
        ////    var bindings = new List<KeyValuePair<string, dynamic>>
        ////    {
        ////        new KeyValuePair<string, dynamic>("__persistent__", _stateDict)
        ////    };
        ////    var value = PythonEngine.Evaluator(RequiresRecalc, script, bindings, inputs);
        ////    return value;
        ////}

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonStringNode", "Python Script From String");
            element.SetAttribute("inputcount", "2");

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
