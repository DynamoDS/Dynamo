using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    public class FunctionDefinition
    {
        internal FunctionDefinition() : this(Guid.NewGuid()) { }

        internal FunctionDefinition(Guid id)
        {
            FunctionId = id;
            RequiresRecalc = true;
        }

        public Guid FunctionId { get; private set; }
        public CustomNodeWorkspaceModel WorkspaceModel { get; internal set; }
        public List<Tuple<int, NodeModel>> OutPortMappings { get; internal set; }
        public List<Tuple<int, NodeModel>> InPortMappings { get; internal set; }

        public bool RequiresRecalc { get; internal set; }

        /// <summary>
        /// A list of all dependencies with no duplicates
        /// </summary>
        public IEnumerable<FunctionDefinition> Dependencies
        {
            get
            {
                return findAllDependencies(new HashSet<FunctionDefinition>());
            }
        }

        /// <summary>
        /// A list of all direct dependencies without duplicates
        /// </summary>
        public IEnumerable<FunctionDefinition> DirectDependencies
        {
            get
            {
                return findDirectDependencies();
            }
        }

        private IEnumerable<FunctionDefinition> findAllDependencies(HashSet<FunctionDefinition> dependencySet)
        {
            var query = DirectDependencies.Where(def => !dependencySet.Contains(def));

            foreach (var definition in query)
            {
                yield return definition;
                dependencySet.Add(definition);
                foreach (var def in definition.findAllDependencies(dependencySet))
                    yield return def;
            }
        }

        private IEnumerable<FunctionDefinition> findDirectDependencies()
        {
            return WorkspaceModel.Nodes
                            .OfType<Function>()
                            .Select(node => node.Definition)
                            .Where(def => def != this)
                            .Distinct();
        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        public bool Save(bool writeDefinition = true, bool addToSearch = false, bool compileFunction = true)
        {

            // Get the internal nodes for the function
            var functionWorkspace = this.WorkspaceModel;

            string path = this.WorkspaceModel.FilePath;
            // If asked to, write the definition to file
            if (writeDefinition && !System.String.IsNullOrEmpty(path))
            {
                Models.WorkspaceModel.SaveWorkspace(path, functionWorkspace);
            }

            try
            {
                dynSettings.Controller.CustomNodeManager.AddFunctionDefinition(this.FunctionId, this);

                if (addToSearch)
                {
                    dynSettings.Controller.SearchViewModel.Add(
                        functionWorkspace.Name,
                        functionWorkspace.Category,
                        functionWorkspace.Description,
                        this.FunctionId);
                }

                var info = new CustomNodeInfo(this.FunctionId, functionWorkspace.Name, functionWorkspace.Category,
                                              functionWorkspace.Description, path);
                dynSettings.Controller.CustomNodeManager.SetNodeInfo(info);

                #region Compile Function and update all nodes

                IEnumerable<string> inputNames;
                IEnumerable<string> outputNames;

                var compiledFunction = CustomNodeManager.CompileFunction(this, out inputNames, out outputNames);

                if (compiledFunction == null)
                    return false;

                dynSettings.Controller.FSchemeEnvironment.DefineSymbol(
                    this.FunctionId.ToString(),
                    compiledFunction);

                //Update existing function nodes which point to this function to match its changes
                foreach (
                    Function node in
                        dynSettings.Controller.DynamoModel.AllNodes.OfType<Function>()
                                   .Where(el => el.Definition == this))
                {
                    node.SetInputs(inputNames);
                    node.SetOutputs(outputNames);
                    node.RegisterAllPorts();
                }

                //Call OnSave for all saved elements
                foreach (NodeModel el in functionWorkspace.Nodes)
                    el.onSave();

                #endregion

            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Error saving:" + e.GetType());
                DynamoLogger.Instance.Log(e);
                return false;
            }

            return true;
        }
    }
}
