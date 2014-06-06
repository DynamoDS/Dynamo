using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    class CodeBlockNodeMutator : AbstractMutator
    {
        public CodeBlockNodeMutator(Random rand)
            : base(rand)
        {
            
        }

        public override int Mutate()
        {

            List<NodeModel> nodes = DynamoModel.Nodes.Where(x => x.GetType() == typeof (CodeBlockNodeModel)).ToList();

            //If there aren't any CBNs, we can't mutate anything
            if (nodes.Count == 0)
                return 0;

            NodeModel node = nodes[Rand.Next(nodes.Count)];

            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                {
                    string code = ((CodeBlockNodeModel) node).Code;

                    if (code.Length == 0)
                        code = "";


                    string replacement;

                    if (Rand.NextDouble() <= 0.5)
                    {
                        //Strategy 1: Replacement with simplest minimal replacement

                        replacement = "1;";

                    }
                    else
                    {
                        //Strategy 2: Noise injection

                        replacement = code;
                        
                        while (Rand.NextDouble() > 0.5)
                        {
                            int locat = Rand.Next(code.Length);
                            const string junk = "<>:L/;'\\/[";

                            replacement = code.Substring(0, locat) + junk[Rand.Next(junk.Length)] +
                                          code.Substring(locat);
                        }

                    }

                DynamoViewModel.UpdateModelValueCommand cmd =
                    new DynamoViewModel.UpdateModelValueCommand(node.GUID, "Code", replacement);
                

                DynamoViewModel.ExecuteCommand(cmd);

            }));

            //We've performed a single edit from the perspective of undo
            return 1;


        }
    }
}
