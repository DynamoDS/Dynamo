using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo;
using ProtoCore;
using Dynamo.Models;
using System.Xml;
using System.Threading;
using System.IO;

namespace Dynamo.Core
{
    public struct CommandLineArguments
    {
        internal static CommandLineArguments FromArguments(string[] args)
        {
            // Running Dynamo sandbox with a command file:
            // DynamoSandbox.exe /c "C:\file path\file.xml"
            // 
            var commandFilePath = string.Empty;

            // Running Dynamo under a different locale setting:
            // DynamoSandbox.exe /l "ja-JP"
            //
            var locale = string.Empty;

            // Open Dynamo headless and open file at path
            // DynamoSandbox.exe /o "C:\file path\graph.dyn"
            //
            var openfilepath = string.Empty;

            // import a set of presets from another dyn or presetfile 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /p "C:\states.dyn"
            //
            var presetFile = string.Empty;

            // set current opened graph to state by name 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /s "state1"
            //
            var presetStateid = string.Empty;

            // print the resulting values of all nodes to the console 
            // DynamoSandbox.exe /o "C:\file path\graph.dyn" /v "C:\someoutputfilepath.txt"
            //
            var verbose = string.Empty;

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg.Length != 2 || (arg[0] != '/'))
                {
                    continue; // Not a "/x" type of command switch.
                }

                switch (arg[1])
                {
                    case 'c':
                    case 'C':
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[++i];
                        break;

                    case 'l':
                    case 'L':
                        if (i < args.Length - 1)
                            locale = args[++i];
                        break;

                    case 'o':
                    case 'O':
                        if (i < args.Length - 1)
                            openfilepath = args[++i];
                        break;

                    case 's':
                    case 'S':
                        if (i < args.Length - 1)
                            presetStateid = args[++i];
                        break;

                    case 'p':
                    case 'P':
                        if (i < args.Length - 1)
                            presetFile = args[++i];
                        break;

                    case 'v':
                    case 'V':
                        if (i < args.Length - 1)
                            verbose = args[++i];
                        break;
                }
            }

            return new CommandLineArguments
            {
                Locale = locale,
                CommandFilePath = commandFilePath,
                OpenFilePath = openfilepath,
                PresetStateID = presetStateid,
                PresetFilePath = presetFile,
                Verbose = verbose,
            };
        }

        internal string Locale { get; set; }
        internal string CommandFilePath { get; set; }
        internal string OpenFilePath { get; set; }
        internal string PresetStateID { get; set; }
        internal string PresetFilePath { get; set; }
        internal string Verbose { get; set; }
    }

    public class CommandLineRunner
    {
        private readonly DynamoModel model;

        public CommandLineRunner(DynamoModel model)
        {
            this.model = model;
        }

        private static XmlDocument RunCommandLineArgs(DynamoModel model, CommandLineArguments cmdLineArgs)
        {
            var evalComplete = false;
            model.OpenFileFromPath(cmdLineArgs.OpenFilePath);
            Console.WriteLine("loaded file");
            model.EvaluationCompleted += (o, args) => { evalComplete = true; };
            if (!string.IsNullOrEmpty(cmdLineArgs.PresetFilePath))
            {
                //load the states contained in this file, it should be structured so
                //that there is a PresetsModel element containing multiple PresetStates elements
                //load it pointing to the file we opened so that states will point to the correct nodes
                model.CurrentWorkspace.PresetsCollection.ImportStates(
                    PresetsModel.LoadFromXmlPaths(cmdLineArgs.PresetFilePath, cmdLineArgs.OpenFilePath, model.NodeFactory));
            }

            //build a list of states, for now, none, a single state, or all of them
            //this must be done after potentially loading states from external file
            var stateNames = new List<String>();
            if (!string.IsNullOrEmpty(cmdLineArgs.PresetStateID))
            {
                if (cmdLineArgs.PresetStateID == "all")
                {
                    foreach (var state in model.CurrentWorkspace.PresetsCollection.DesignStates)
                    {
                        stateNames.Add(state.Name);
                    }
                }
                else
                {
                    stateNames.Add(cmdLineArgs.PresetStateID);
                }
            }
            else
            {
                stateNames.Add("default");
            }

            var outputresults = new List<Dictionary<Guid, List<object>>>();
            XmlDocument doc = null;
            foreach (var stateName in stateNames)
            {
                model.CurrentWorkspace.SetWorkspaceToState(stateName);
                model.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));

                while (evalComplete == false)
                {
                    Thread.Sleep(250);
                }

                //if verbose was true, then print all nodes to the console
                if (!String.IsNullOrEmpty(cmdLineArgs.Verbose))
                {
                    doc = new XmlDocument();
                    var resultsdict = new Dictionary<Guid, List<object>>();
                    foreach (var node in model.CurrentWorkspace.Nodes)
                    {
                        var portvalues = new List<object>();
                        foreach (var port in node.OutPorts)
                        {
                            var value = node.GetValue(port.Index, model.EngineController);
                            portvalues.Add(value.StringData);
                        }
                        resultsdict.Add(node.GUID, portvalues);
                    }
                    outputresults.Add(resultsdict);
                }
                evalComplete = false;

            }
            
            populateXmlDocWithResults(doc, outputresults);
            return doc;
        }

        private static void populateXmlDocWithResults(XmlDocument doc, List<Dictionary<Guid, List<object>>> resultsDict)
          {
              var evalele = doc.CreateElement("evaluations");
              doc.AppendChild(evalele);
              foreach (var evaluation in resultsDict)
              {
                  var index = resultsDict.IndexOf(evaluation);

                   var currenteval = doc.CreateElement("evaluation" + index.ToString());
                        evalele.AppendChild(currenteval);
                  //foreach node:results pair in this eval
                    foreach(KeyValuePair<Guid, List<object>> entry in resultsDict[index])
                    {
                        var nodeval = doc.CreateElement(entry.Key.ToString());
                        currenteval.AppendChild(nodeval);

                        foreach (var value in entry.Value)
                        {
                            var portindex = entry.Value.IndexOf(value);
                            var portelement = doc.CreateElement("output" + portindex.ToString());
                            portelement.SetAttribute("value", value.ToString());
                            nodeval.AppendChild(portelement);
                        }
                    }

              }
          }


        public void Run(CommandLineArguments args)
        {
            var doc = RunCommandLineArgs(this.model, args);
            if (doc != null && Directory.Exists(new FileInfo(args.Verbose).Directory.FullName))
            {
                doc.Save(args.Verbose);
            }
        }
    }
}
