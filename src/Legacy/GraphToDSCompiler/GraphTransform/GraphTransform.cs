using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using ProtoCore.Utils;
using ProtoCore.AST.AssociativeAST;


namespace GraphToDSCompiler
{
    public class GraphTransform
    {
        private List<SnapshotNode> astGraph = null;
        private List<IPatternRewrite> patternRewriteList = null;

        public GraphTransform()
        {
            astGraph = null;
        }

        public GraphTransform(List<SnapshotNode> graph)
        {
            astGraph = graph;
        }

        /// <summary>
        /// Initialize all the pattern classes that need to be applied to the graph
        /// </summary>
        private void InitializePatterns()
        {
            Validity.Assert(patternRewriteList == null);
            patternRewriteList = new List<IPatternRewrite>();
            patternRewriteList.Add(new IntermediateFormRewrite());
            patternRewriteList.Add(new DegreeToRadianRewrite());
            patternRewriteList.Add(new ThreePointCircleRewrite());
            patternRewriteList.Add(new ThreePointToPlaneRewrite());
        }

        /// <summary>
        /// Runs all the pattern rewrites to the graph and outputs the final graph
        /// the final graph is the one that is transformed to DS source code
        /// </summary>
        /// <returns></returns>
        public List<SnapshotNode> ToFinalGraph()
        {
            InitializePatterns();

            Validity.Assert(astGraph != null);
            Validity.Assert(patternRewriteList != null);

            foreach (IPatternRewrite patternRW in patternRewriteList)
            {
                astGraph = patternRW.Rewrite(astGraph);
            }
            return astGraph; 
        }

        /// <summary>
        /// Transforms Node to the Final ProtoAST form
        /// </summary>
        /// <returns></returns>
        public List<AssociativeNode> ToGraphIR(AST graph, GraphCompiler graphCompiler)
        {
            GraphNodeToASTGenerator astGen = new GraphNodeToASTGenerator(graph, graphCompiler);
            astGen.AddNodesToAST();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> irGraph = astGen.SplitAST();


            return irGraph;
        }
    }
}
