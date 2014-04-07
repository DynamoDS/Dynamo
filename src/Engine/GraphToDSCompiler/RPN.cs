using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

/*doesn't take care of many situations except the very basic ones. just a proto...has to be refined.*/
/*
namespace GraphToDSCompiler
{
    class RPN
    {
        internal void EvalRPN(List<Node> result)
        {
            Stack<Node> stack = new Stack<Node>();
            IEnumerable iter = result;
            //Console.WriteLine("in evalRpn");
            foreach (Node obj in iter)
            {
                if (obj != null)
                {
                    if (obj.Name.Equals("*"))
                    {
                        Node var2 = stack.Pop();
                        Node var1 = stack.Pop();
                        IdentNode ita1 = new IdentNode(var1);
                        IdentNode ita2 = new IdentNode(var2);
                        BinExprNode bine = new BinExprNode(ita1, (Operator)obj, ita2);
                        stack.Push(new Node(bine.EmitCode() + "", Guid.NewGuid().ToString()));
                        //Console.Write("Mult");
                    }
                    else if (obj.Name.Equals("/"))
                    {
                        Node var2 = stack.Pop();
                        Node var1 = stack.Pop();
                        IdentNode ita1 = new IdentNode(var1);
                        IdentNode ita2 = new IdentNode(var2);
                        BinExprNode bine = new BinExprNode(ita1, (Operator)obj, ita2);
                        stack.Push(new Node(bine.EmitCode() + "", Guid.NewGuid().ToString()));
                        //Console.Write("Div");
                    }
                    else if (obj.Name.Equals("-"))
                    {
                        Node var2 = stack.Pop();
                        Node var1 = stack.Pop();
                        IdentNode ita1 = new IdentNode(var1);
                        IdentNode ita2 = new IdentNode(var2);
                        BinExprNode bine = new BinExprNode(ita1, (Operator)obj, ita2);
                        stack.Push(new Node(bine.EmitCode() + "", Guid.NewGuid().ToString()));
                        // Console.Write("Min");
                    }
                    else if (obj.Name.Equals("+"))
                    {
                        Node var2 = stack.Pop();
                        Node var1 = stack.Pop();
                        IdentNode ita1 = new IdentNode(var1);
                        IdentNode ita2 = new IdentNode(var2);
                        BinExprNode bine = new BinExprNode(ita1, (Operator)obj, ita2);
                        stack.Push(new Node(bine.EmitCode() + "", Guid.NewGuid().ToString()));
                        //Console.Write("Add");
                    }
                    else if (obj.Name.Equals("="))
                    {
                        Node var2 = stack.Pop();
                        Node var1 = stack.Pop();
                        IdentNode ita1 = new IdentNode(var1);
                        IdentNode ita2 = new IdentNode(var2);
                        BinExprNode bine = new BinExprNode(ita1, (Operator)obj, ita2);
                        // Console.Write("write");
                        //ass.dump();
                        // Example #4: Append new text to an existing file 
                        using (StreamWriter file = new StreamWriter(@"C:\Research\branches\Experimental\ASTStorage\GeneratedScripts\Test.ds", true))
                        {
                            file.WriteLine(bine.EmitCode());
                        }
                    }
                    else
                    {
                        stack.Push(new Node(obj.Name + "", Guid.NewGuid().ToString()));
                        //Console.Write("pushing");
                    }
                }
            }
        }
    }
}

*/