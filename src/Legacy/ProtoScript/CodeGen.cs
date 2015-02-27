using System;
using ProtoCore;
//using ProtoCore.AST.ImperativeAST;

namespace ProtoScript
{
    public class GenerateScript
    {
        private Script script;
        private Core core;
        public GenerateScript(Core core)
        {
            script = new Script();
            this.core = core;
        }

        public ProtoCore.Script preParseFromFile(string pathFilename)
        {
            ProtoScript.Scanner s = new ProtoScript.Scanner(pathFilename);
            ProtoScript.Parser p = new ProtoScript.Parser(s);
            System.IO.StringWriter errorStream = new System.IO.StringWriter();
            p.errors.errorStream = errorStream;
            p.Parse();
            //core.BuildStatus.errorCount += p.errors.count;

            //if (errorStream.ToString() != String.Empty)
                //core.BuildStatus.Errors.Add(errorStream.ToString());

            //foreach (Object node in p.fusionScript.codeblockList)
            //{
            //    dfsGraphOptimize(node);
            //}
            return p.script;
        }

        public ProtoCore.Script preParseFromStream(System.IO.Stream sourceStream)
        {
            ProtoScript.Scanner s = new ProtoScript.Scanner(sourceStream);
            ProtoScript.Parser p = new ProtoScript.Parser(s);
            System.IO.StringWriter errorStream = new System.IO.StringWriter();
            p.errors.errorStream = errorStream;
            p.Parse();
            //core.BuildStatus.errorCount += p.errors.count;

            //if (errorStream.ToString() != String.Empty)
                //core.BuildStatus.Errors.Add(errorStream.ToString());

            //foreach (Object node in p.fusionScript.codeblockList)
            //{
            //    dfsGraphOptimize(node);
            //}
            return p.script;
        }

        private void dfsGraphOptimize(Object node)
        {
            //Hydrogen.DependencyPass.AST ast = new Hydrogen.DependencyPass.AST();
            //Hydrogen.DependencyPass.DependencyTracker optimisedTracker = ast.genGraphOptimizedAST(pathFilename);

            // Optimize it
            //Hydrogen.DependencyPass.GraphOptimiser optimiser = new Hydrogen.DependencyPass.GraphOptimiser();
            //optimiser.Execute(optimisedTracker);
        }
    }
}

