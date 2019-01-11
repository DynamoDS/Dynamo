using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Dynamo.Applications;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using DynamoCLI;
using Newtonsoft.Json;

namespace DynamoWPFCLI
{
    /// <summary>
    /// This class invokes a dynamo model's run methods in a headless mode from the CLI using a set of flags.
    /// This class also has a very limited method for exporting the graph evaluation to an xml file, so that 
    /// the results from invoking dynamo from the command line are useable.
    /// </summary>
    public class CommandLineRunnerWPF : CommandLineRunner
    {
        private readonly DynamoViewModel viewModel;

        public CommandLineRunnerWPF(DynamoViewModel viewModel) : base(viewModel.Model)
        {
            this.viewModel = viewModel;
        }

        private static XmlDocument RunCommandLineArgs(DynamoViewModel viewModel, StartupUtils.CommandLineArguments cmdLineArgs)
        {
            var evalComplete = false;
            if (string.IsNullOrEmpty(cmdLineArgs.OpenFilePath))
            {
                return null;
            }
            if (!(string.IsNullOrEmpty(cmdLineArgs.CommandFilePath)))
            {
                Console.WriteLine("commandFilePath option is only available when running DynamoSandbox, not DynamoWPFCLI");
            }

            cmdLineArgs.ImportedPaths.ToList().ForEach(path =>
            {
                ImportAssembly(viewModel.Model, path);
            });

            viewModel.OpenCommand.Execute(new Tuple<string, bool>(cmdLineArgs.OpenFilePath, true));
            Console.WriteLine("loaded file");
            viewModel.Model.EvaluationCompleted += (o, args) => { evalComplete = true; };

            // Build a list of states, by default there is only a single state `default`
            // If the desire is to have additional states you can add logic here to build
            // up a list and iterate through each state in the list using the loop below.
            // This must be done after potentially loading states from an external file.
            var stateNames = new List<String>();
            stateNames.Add("default");

            XmlDocument doc = null;
            foreach (var stateName in stateNames)
            {
                // Graph execution
                viewModel.Model.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));

                while (evalComplete == false)
                {
                    Thread.Sleep(250);
                }

                //if verbose was true, then print all nodes to the console
                if (!String.IsNullOrEmpty(cmdLineArgs.Verbose))
                {
                    doc = CreateXMLDoc(viewModel.Model);
                }

                if (!String.IsNullOrEmpty(cmdLineArgs.GeometryFilePath))
                {
                    GenerateGeometryJsonFile(viewModel.Model, cmdLineArgs.GeometryFilePath);
                }

            }

            return doc;
        }

        private static void GenerateGeometryJsonFile(DynamoModel model, string geometryFilePath)
        {
            var renderPackageFactory = new DefaultRenderPackageFactory();
            var nodeGeometries = new List<GeometryHolder>();
            foreach (var node in model.CurrentWorkspace.Nodes)
            {
                nodeGeometries.Add(new GeometryHolder(model, renderPackageFactory, node));
            }

            var jsonFilename = geometryFilePath;
            using (StreamWriter jsonFile = new StreamWriter(jsonFilename))
            {
                var geometry = new List<object>();
                foreach (var holder in nodeGeometries)
                {
                    if (holder.HasGeometry)
                    {
                        geometry.Add(holder.Geometry);
                    }
                }
                string json = JsonConvert.SerializeObject(geometry);
                jsonFile.Write(json);
            }
        }

        private static void OpenWorkspaceAndConvert(DynamoViewModel viewModel, string dynPath)
        {
            viewModel.OpenCommand.Execute(dynPath);

            var ws = viewModel.Model.CurrentWorkspace;
            var json = ws.ToJson(viewModel.Model.EngineController);

            var newFilePath = Path.Combine(Path.GetDirectoryName(dynPath), Path.GetFileNameWithoutExtension(dynPath) + ".json");
            File.WriteAllText(newFilePath, json);
        }

        /// <summary>
        /// Run the CLI with the command line arguments in "args" <see cref="StartupUtils.CommandLineArguments"/>
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public new void Run(StartupUtils.CommandLineArguments args)
        {
            if (args.ConvertFile)
            {
                OpenWorkspaceAndConvert(viewModel, args.OpenFilePath);
                return;
            }

            var doc = RunCommandLineArgs(this.viewModel, args);
            if (doc != null && Directory.Exists(new FileInfo(args.Verbose).Directory.FullName))
            {
                //if it exists and the path is valid, save the output file
                doc.Save(args.Verbose);
            }
        }
    }
}
