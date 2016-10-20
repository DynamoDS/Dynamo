using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Threading;
using System.IO;
using Dynamo.Models;
using DynamoUtilities;
using Dynamo.Applications;
using Dynamo.Graph;
using Autodesk.Workspaces;

namespace DynamoCLI
{

    /// <summary>
    /// This class invokes a dynamo model's run methods in a headless mode from the CLI using a set of flags
    /// that set the graph to different preset states. This class also has a very limited method for exporting 
    /// the graph evaluation to an xml file, so that the results from invoking dynamo from the command line 
    /// are useable.
    /// </summary>
    public class CommandLineRunner
    {
        private readonly DynamoModel model;

        public CommandLineRunner(DynamoModel model)
        {
            this.model = model;
        }

        private static XmlDocument RunCommandLineArgs(DynamoModel model, StartupUtils.CommandLineArguments cmdLineArgs)
        {
            var evalComplete = false;
            if (string.IsNullOrEmpty(cmdLineArgs.OpenFilePath))
            {
                return null;
            }
            if (!(string.IsNullOrEmpty(cmdLineArgs.CommandFilePath)))
            {
                Console.WriteLine("commandFilePath option is only available when running DynamoSandbox, not DynamoCLI");
            }

            model.OpenFileFromPath(cmdLineArgs.OpenFilePath, true);
            Console.WriteLine("loaded file");
            model.EvaluationCompleted += (o, args) => { evalComplete = true; };

            if (!string.IsNullOrEmpty(cmdLineArgs.PresetFilePath))
            {
                //first load the openfile nodegraph
                var originalGraphdoc = XmlHelper.CreateDocument("tempworkspace");
                originalGraphdoc.Load(cmdLineArgs.OpenFilePath);
                var graph = NodeGraph.LoadGraphFromXml(originalGraphdoc, model.NodeFactory);

                //then load the presetsfile nodegraph (this should only contain presets),
                var presetsDoc = XmlHelper.CreateDocument("presetstempworkspace");
                presetsDoc.Load(cmdLineArgs.PresetFilePath);
                //when we load the presets we need to pass in the nodeModels from the original graph
                var presets = NodeGraph.LoadPresetsFromXml(presetsDoc, graph.Nodes);

                //load the presets contained in the presetsfile into the workspace,
                model.CurrentWorkspace.ImportPresets(presets);
            }

            //build a list of states, for now, none, a single state, or all of them
            //this must be done after potentially loading states from external file
            var stateNames = new List<String>();
            if (!string.IsNullOrEmpty(cmdLineArgs.PresetStateID))
            {
                if (cmdLineArgs.PresetStateID == "all")
                {
                    foreach (var state in model.CurrentWorkspace.Presets)
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
                Guid stateGuid = Guid.Empty;
                var state = model.CurrentWorkspace.Presets.Where(x => x.Name == stateName).FirstOrDefault();
                if (state != null)
                {
                    stateGuid = state.GUID;
                }
                
                model.ExecuteCommand(new DynamoModel.ApplyPresetCommand(model.CurrentWorkspace.Guid, stateGuid));
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
                            if (value.IsCollection)
                            {
                                portvalues.Add(GetStringRepOfCollection(value));
                            }
                            else
                            {
                                portvalues.Add(value.StringData);
                            }
                            
                        }
                        resultsdict.Add(node.GUID, portvalues);
                    }
                    outputresults.Add(resultsdict);
                    populateXmlDocWithResults(doc, outputresults);
                }
                evalComplete = false;

            }


            return doc;
        }

        private static string GetStringRepOfCollection(ProtoCore.Mirror.MirrorData collection)
        {

           var items = string.Join(",", collection.GetElements().Select(x => x.IsCollection ? GetStringRepOfCollection(x) : x.StringData));
            return "{"+items +"}";

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
                foreach (KeyValuePair<Guid, List<object>> entry in resultsDict[index])
                {
                    var nodeval = doc.CreateElement("Node");
                    nodeval.SetAttribute("guid", entry.Key.ToString());
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

        private static void OpenWorkspaceAndConvert(DynamoModel model, string dynPath)
        {
            model.OpenFileFromPath(dynPath);

            var ws = model.CurrentWorkspace;
            var json = Utilities.SaveWorkspaceToJson(ws, model.LibraryServices, model.EngineController,
                model.Scheduler, model.NodeFactory, false, false, model.CustomNodeManager);

            var newFilePath = Path.Combine(Path.GetDirectoryName(dynPath), Path.GetFileNameWithoutExtension(dynPath) + ".json");
            File.WriteAllText(newFilePath, json);
        }

        public void Run(StartupUtils.CommandLineArguments args)
        {
            if (args.ConvertFile)
            {
                OpenWorkspaceAndConvert(model, args.OpenFilePath);
                return;
            }

            var doc = RunCommandLineArgs(this.model, args);
            if (doc != null && Directory.Exists(new FileInfo(args.Verbose).Directory.FullName))
            {
                //if it exists and the path is valid, save the output file
                doc.Save(args.Verbose);
            }
        }
    }
}
