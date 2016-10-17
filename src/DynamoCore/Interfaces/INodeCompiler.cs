using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// Interface describing a class which can be compiled.
    /// </summary>
    public interface INodeCompiler<TCompile>
    {
        /// <summary>
        /// The node type for which this class contains the compile logic.
        /// </summary>
        Type NodeType { get; }

        /// <summary>
        /// Called on each node which participates in graph compilation.
        /// </summary>
        /// <param name="node">The NodeModel to compile.</param>
        /// <param name="inputs">A list of TCompile for inputs indexed by input port index.</param>
        /// <returns>A list of TCompile representing this Node's code output.</returns>
        IEnumerable<TCompile> Compile(NodeModel node, List<TCompile> inputs);
    }

    public static class ExpressionCompilerUtils
    {
        public static ParameterExpression GetExpressionIdentifierForOutputIndex<T>(NodeModel node, int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > node.OutPorts.Count)
                throw new ArgumentOutOfRangeException(nameof(outputIndex), @"Index must correspond to an OutPortData index.");

            if (node.OutPorts.Count <= 1)
                return Expression.Variable(typeof(T), GetExpressionIdentifierForNode(node));

            var id = GetExpressionIdentifierForNode(node) + "_out" + outputIndex;
            return Expression.Variable(typeof(T), id);
        }

        private static string GetExpressionIdentifierForNode(NodeModel node)
        {
            var guid = node.GUID.ToString().Replace("-", string.Empty).ToLower();
            return "expr_" + guid;
        }
    }

    /// <summary>
    /// Class which contains a collection of INodeCompilers.
    /// </summary>
    /// <typeparam name="TCompile"></typeparam>
    public class NodeCompilerRegistry<TCompile>
    {
        /// <summary>
        /// A Dictionary of INodeCompilers, keyed by the NodeModel type for which they
        /// contain the compilation logic.
        /// </summary>
        private IDictionary<Type, INodeCompiler<TCompile>> compilers;

        private NodeCompilerRegistry(IDictionary<Type, INodeCompiler<TCompile>> compilers)
        {
            this.compilers = compilers ?? new Dictionary<Type, INodeCompiler<TCompile>>();
        }

        public static NodeCompilerRegistry<TCompile> FromDirectory(string directory, ILogger logger)
        {
            if (!Directory.Exists(directory))
            {
                logger.LogError("Directory provided for compiler lookup does not exist.");
                return null;
            }

            var compilers = new Dictionary<Type, INodeCompiler<TCompile>>();

            // Load compilers from the specified directory.
            var dirInfo = new DirectoryInfo(directory);
            foreach (var file in dirInfo.GetFiles().Where(f=>f.Extension==".dll"))
            {
                var asm = Assembly.LoadFrom(file.FullName);
                var compilerTypes = asm.GetTypes().Where(t => typeof(INodeCompiler<TCompile>).IsAssignableFrom(t)).ToArray();
                if (!compilerTypes.Any()) continue;

                foreach (var t in compilerTypes)
                {
                    try
                    {
                        var compiler = (INodeCompiler<TCompile>)Activator.CreateInstance(t);
                        if (compilers.ContainsKey(compiler.NodeType))
                        {
                            compilers[compiler.NodeType] = compiler;
                        }
                        compilers.Add(compiler.NodeType, compiler);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error when attempting to load INodeCompiler : " + t);
                        logger.Log(ex);
                    }
                }
            }

            return new NodeCompilerRegistry<TCompile>(compilers); 
        }

        /// <summary>
        /// Try to get a compiler for the specified node type.
        /// </summary>
        /// <param name="nodeType">The type of the node for which to find a compiler.</param>
        /// <param name="compiler">The compiler, if one is found, otherwise null.</param>
        /// <returns>True if a compiler can be found, false if not.</returns>
        public bool TryGetCompilerOfType(Type nodeType, out INodeCompiler<TCompile> compiler)
        {
            if (!compilers.ContainsKey(nodeType))
            {
                compiler = null;
                return false;
            }

            compiler = compilers[nodeType];
            return true;
        }
    }
}
