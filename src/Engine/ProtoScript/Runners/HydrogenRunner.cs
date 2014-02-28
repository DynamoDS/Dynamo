using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoScript.Runners
{
    [Obsolete]
    public class HydrogenRunner
    {
        static public void MinTest(ProtoCore.Core core)
        {
            ProtoAssociative.DependencyPass.GraphOptimiser optimiser = new ProtoAssociative.DependencyPass.GraphOptimiser();
            ProtoAssociative.DependencyPass.AST ast = new ProtoAssociative.DependencyPass.AST();
            ProtoAssociative.DependencyPass.DependencyTracker optimisedTracker = ast.GetDemoTracker2(core);
            optimiser.Execute(optimisedTracker);
        }

        static public void LoadAndExecute(string pathFilename, ProtoCore.Core core)
        {
            // Simulate Load and parse DS script
            ProtoCore.DSASM.SymbolTable symbols = null;
            ProtoAssociative.DependencyPass.AST ast = new ProtoAssociative.DependencyPass.AST();
            ProtoAssociative.DependencyPass.DependencyTracker optimisedTracker = ast.GetDemoTracker3(out symbols, pathFilename, core);

            // Optimize it
            //Hydrogen.DependencyPass.GraphOptimiser optimiser = new Hydrogen.DependencyPass.GraphOptimiser();
            //optimiser.Execute(optimisedTracker);

            // Generate code after graph optimization pass
            ProtoAssociative.CodeGen codegen = new ProtoAssociative.CodeGen(core);
            
            codegen.Emit(optimisedTracker);

            //core.executable = codegen.executable;
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
            interpreter.Run();
        }
    }
}
