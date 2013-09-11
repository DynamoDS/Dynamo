using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using ProtoCore.DSASM;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// A helper class to get some information from DesignScript core.
    /// </summary>
    public class DSUtil
    {
        public static DSUtil Instance = new DSUtil();

        public List<DSFunctionDescritpion> DSBuiltInMethods
        {
            get
            {
                if (null == builtInMethods)
                {
                    builtInMethods = new List<DSFunctionDescritpion>();

                    List<string> args = new List<string> { "var1", "var2" };
                    builtInMethods.Add(new DSFunctionDescritpion(Op.GetOpFunction(Operator.add), Op.GetOpSymbol(Operator.add), args));
                    builtInMethods.Add(new DSFunctionDescritpion(Op.GetOpFunction(Operator.sub), Op.GetOpSymbol(Operator.sub), args));
                    builtInMethods.Add(new DSFunctionDescritpion(Op.GetOpFunction(Operator.mul), Op.GetOpSymbol(Operator.mul), args));
                    builtInMethods.Add(new DSFunctionDescritpion(Op.GetOpFunction(Operator.div), Op.GetOpSymbol(Operator.div), args));

                    List<ProcedureNode> builtins = GraphToDSCompiler.GraphUtilities.BuiltInMethods;
                    foreach (var method in builtins)
                    {
                        List<string> argumentNames = method.argInfoList.Select(x => x.Name).ToList();
                        builtInMethods.Add(new DSFunctionDescritpion(method.name, method.name, argumentNames));
                    }
                }
                return builtInMethods;
            }
        }

        private List<DSFunctionDescritpion> builtInMethods = null;

        private DSUtil()
        {
            GraphToDSCompiler.GraphUtilities.PreloadAssembly(new List<string> { "Math.dll" });
        }

    }
}
