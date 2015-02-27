using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoScript.Runners
{
    [Obsolete]
    public class DeuteriumRunner
    {
        static public void LoadAndExecute(string pathFilename, ProtoCore.Core core)
        {
            ProtoImperative.AST.GenAST genast = new ProtoImperative.AST.GenAST();
            ProtoImperative.AST.CodeBlockNode codeblock = genast.GenerateAST(pathFilename, core);

            ProtoImperative.CodeGen codegen = new ProtoImperative.CodeGen(core);
            codegen.Emit(codeblock);

            //core.executable = codegen.executable;
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
            interpreter.Run();
        }
    }
}
