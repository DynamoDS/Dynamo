using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;

namespace ProtoCore
{
    /// <summary>
    /// The code generator takes Abstract Syntax Tree and generates the DesignScript code
    /// </summary>
    public class CodeGenDS
    {
        public IEnumerable<AssociativeNode> astNodeList { get; private set; }
        string code = string.Empty;

        /// <summary>
        /// This is used during ProtoAST generation to connect BinaryExpressionNode's 
        /// generated from Block nodes to its child AST tree - pratapa
        /// </summary>
        //protected ProtoCore.AST.AssociativeAST.BinaryExpressionNode ChildTree { get; set; }

        public CodeGenDS(IEnumerable<AssociativeNode> astList)
        {
            this.astNodeList = astList;
        }
        
        /// <summary>
        /// This function prints the DS code into the destination stream
        /// </summary>
        /// <param name="code"></param>
        protected virtual void EmitCode(string code)
        {
            this.code += code;
        }

        public string GenerateCode()
        {
            Validity.Assert(null != astNodeList);

            foreach (var astNode in astNodeList)
            {
                EmitCode(astNode.ToString());
            }
            return code;
        }
    }
}
