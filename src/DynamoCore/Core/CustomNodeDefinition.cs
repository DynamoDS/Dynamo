using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore;
using ProtoCore.DSASM;

namespace Dynamo
{
    /// <summary>
    ///     Compiler definition of a Custom Node.
    /// </summary>
    public class CustomNodeDefinition : IFunctionDescriptor
    {
        public CustomNodeDefinition(
            Guid functionId,
            string displayName="",
            IList<NodeModel> nodeModels=null)
        {
            if (functionId == Guid.Empty)
                throw new ArgumentException(@"FunctionId invalid.", "functionId");

            nodeModels = nodeModels ?? new List<NodeModel>();

            #region Find outputs

            // Find output elements for the node

            var outputs = nodeModels.OfType<Output>().ToList();

            var topMost = new List<Tuple<int, NodeModel>>();

            List<string> outNames;

            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => Tuple.Create(0, x as NodeModel)));
                outNames = outputs.Select(x => x.Symbol).ToList();
            }
            else
            {
                outNames = new List<string>();

                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as the output
                IEnumerable<NodeModel> topMostNodes = nodeModels.Where(node => node.IsTopMostNode);

                var rtnPorts =
                    //Grab multiple returns from each node
                    topMostNodes.SelectMany(
                        topNode =>
                            //If the node is a recursive instance...
                            topNode is Function && (topNode as Function).Definition.FunctionId == functionId
                                // infinity output
                                ? new[] {new {portIndex = 0, node = topNode, name = "∞"}}
                                // otherwise, grab all ports with connected outputs and package necessary info
                                : topNode.OutPortData
                                    .Select(
                                        (port, i) =>
                                            new {portIndex = i, node = topNode, name = port.NickName})
                                    .Where(x => !topNode.HasOutput(x.portIndex)));

                foreach (var rtnAndIndex in rtnPorts.Select((rtn, i) => new {rtn, idx = i}))
                {
                    topMost.Add(Tuple.Create(rtnAndIndex.rtn.portIndex, rtnAndIndex.rtn.node));
                    outNames.Add(rtnAndIndex.rtn.name ?? rtnAndIndex.idx.ToString());
                }
            }

            var nameDict = new Dictionary<string, int>();
            foreach (var name in outNames)
            {
                if (nameDict.ContainsKey(name))
                    nameDict[name]++;
                else
                    nameDict[name] = 0;
            }

            nameDict = nameDict.Where(x => x.Value != 0).ToDictionary(x => x.Key, x => x.Value);

            outNames.Reverse();

            var returnKeys = new List<string>();
            foreach (var name in outNames)
            {
                int amt;
                if (nameDict.TryGetValue(name, out amt))
                {
                    nameDict[name] = amt - 1;
                    returnKeys.Add(name == "" ? amt + ">" : name + amt);
                }
                else
                    returnKeys.Add(name);
            }

            returnKeys.Reverse();

            #endregion

            #region Find inputs

            //Find function entry point, and then compile
            var inputNodes = nodeModels.OfType<Symbol>().ToList();
            var parameters = inputNodes.Select(x => new TypedParameter(
                                                   x.GetAstIdentifierForOutputIndex(0).Value, 
                                                   x.Parameter.Type, 
                                                   x.Parameter.DefaultValue));
            var displayParameters = inputNodes.Select(x => x.Parameter.Name);

            #endregion

            FunctionBody = nodeModels.Where(node => !(node is Symbol));
            DisplayName = displayName;
            FunctionId = functionId;
            Parameters = parameters;
            ReturnKeys = returnKeys;
            DisplayParameters = displayParameters;
            OutputNodes = topMost.Select(x => x.Item2.GetAstIdentifierForOutputIndex(x.Item1));
            DirectDependencies = nodeModels
                .OfType<Function>()
                .Select(node => node.Definition)
                .Where(def => def.FunctionId != functionId)
                .Distinct();
        }

        public static CustomNodeDefinition MakeProxy(Guid functionId, string displayName)
        {
            var def = new CustomNodeDefinition(functionId, displayName);
            def.IsProxy = true;
            return def;
        }

        /// <summary>
        ///     Is this CustomNodeDefinition properly loaded?
        /// </summary>
        public bool IsProxy { get; private set; }

        /// <summary>
        ///     Function name.
        /// </summary>
        public string FunctionName
        {
            get { return AstBuilder.StringConstants.FunctionPrefix + 
                         FunctionId.ToString().Replace("-", string.Empty); }
        }

        /// <summary>
        ///     Function unique ID.
        /// </summary>
        public Guid FunctionId { get; private set; }

        /// <summary>
        ///     User-friendly parameters
        /// </summary>
        public IEnumerable<string> DisplayParameters { get; private set; }

        /// <summary>
        ///     Function parameters.
        /// </summary>
        public IEnumerable<TypedParameter> Parameters { get; private set; } 

        /// <summary>
        ///     If the function returns a dictionary, this specifies all keys in
        ///     that dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; private set; }

        /// <summary>
        ///     NodeModels making up the body of the custom node.
        /// </summary>
        public IEnumerable<NodeModel> FunctionBody { get; private set; }

        /// <summary>
        ///     Identifiers associated with the outputs of the custom node.
        /// </summary>
        public IEnumerable<AssociativeNode> OutputNodes { get; private set; }
        
        /// <summary>
        ///     User friendly name on UI.
        /// </summary>
        public string DisplayName { get; private set; }
        
        #region Dependencies

        public IEnumerable<CustomNodeDefinition> Dependencies
        {
            get { return FindAllDependencies(new HashSet<CustomNodeDefinition>()); }
        }

        public IEnumerable<CustomNodeDefinition> DirectDependencies { get; private set; }
        
        private IEnumerable<CustomNodeDefinition> FindAllDependencies(ISet<CustomNodeDefinition> dependencySet)
        {
            var query = DirectDependencies.Where(def => !dependencySet.Contains(def));

            foreach (var definition in query)
            {
                yield return definition;
                dependencySet.Add(definition);
                foreach (var def in definition.FindAllDependencies(dependencySet))
                    yield return def;
            }
        }

        #endregion

        #region IFunctionDescriptor Members

        /// <summary>
        /// Name to create custom node
        /// </summary>
        public string MangledName
        {
            get { return FunctionId.ToString(); }
        }

        #endregion
    }
    
    /// <summary>
    ///     Basic information about a custom node.
    /// </summary>
    public class CustomNodeInfo
    {
        public CustomNodeInfo(Guid functionId, string name, string category, string description, string path, bool isPackageMember = false)
        {
            if (functionId == Guid.Empty)
                throw new ArgumentException(@"FunctionId invalid.", "functionId");
            
            FunctionId = functionId;
            Name = name;
            Category = category;
            Description = description;
            Path = path;
            IsPackageMember = isPackageMember;
        }

        public Guid FunctionId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public bool IsPackageMember { get; set; }
    }
}
