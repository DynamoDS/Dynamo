using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using ProtoCore.Utils;
namespace GraphToDSCompiler
{

    #region EXPRESSIONS
    /// <summary>
    /// Represents the various expressions in the language
    /// </summary>
    public abstract class Expr : Node
    {
        private uint guid;
        public Expr(string name, uint guid)
            : base(name, guid)
        {
            this.guid = guid;
        }
    }

    /// <summary>
    /// Represents binary expressions of the form - left operator right : eg. a+b,a-b and likewise
    /// </summary>
    public class BinExprNode : Expr
    {
        Node op;

        // To deprecate Node op and replace with the enum
        public ProtoCore.DSASM.Operator Optr { get; set; }

        public Node left, right;
        /// <summary>
        /// Represents compound binary expressions which involve other expressions on the left and right side
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="o"></param>
        /// <param name="e2"></param>
        internal BinExprNode(Expr e1, Operator o, Expr e2)
            : base(o.Name, o.Guid)
        {
            op = o;
            left = e1;
            right = e2;
        }


        public BinExprNode(ProtoCore.DSASM.Operator optr, Node leftNode, Node rightNode)
            : base(leftNode.Name, leftNode.Guid)
        {
            this.Optr = optr;
            this.left = leftNode;
            this.right = rightNode;
        }

        /// <summary>
        /// Represents simple binary expressions which involve identifiers on both sides
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="o"></param>
        /// <param name="i2"></param>
        internal BinExprNode(IdentNode i1, Operator o, IdentNode i2)
            : base(i1.Name + o.Name + i2.Name, o.Guid)
        {
            op = o;
            left = i1;
            right = i2;
        }
        /// <summary>
        /// Represents simple binary expressions which involve identifier on left and literal on the right
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="o"></param>
        /// <param name="i2"></param>
        internal BinExprNode(IdentNode i1, Operator o, LiteralNode i2)
            : base(i1.Name + o.Name + i2.Name, o.Guid)
        {
            op = o;
            left = i1;
            right = i2;
        }
        /// <summary>
        /// Represents simple binary expressions which involve literal on left and identifier on the right
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="o"></param>
        /// <param name="i2"></param>
        internal BinExprNode(LiteralNode i1, Operator o, IdentNode i2)
            : base(i1.Name + o.Name + i2.Name, o.Guid)
        {
            op = o;
            left = i1;
            right = i2;
        }
        /// <summary>
        /// Represents simple binary expressions which involve literal on left and literal on the right
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="o"></param>
        /// <param name="i2"></param>
        internal BinExprNode(LiteralNode i1, Operator o, LiteralNode i2)
            : base(i1.Name + o.Name + i2.Name, o.Guid)
        {
            op = o;
            left = i1;
            right = i2;
        }
        /// <summary>
        /// Represents binary expressions which involve identifier on the left and compound expression on the right
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="op"></param>
        /// <param name="e2"></param>
        public BinExprNode(IdentNode e1, Operator op, Expr e2)
            : base(op.Name, op.Guid)
        {
            left = e1;
            this.op = op;
            right = e2;
        }

        public override string ToCode()
        {
            return this.ToScript();
        }

        public override string ToScript()
        {
            string result = string.Empty;
            result += "(";
            if (((Operator)op).replicationGuide.Equals("") || ((Operator)op).replicationGuide == null)
            {
                // Handle left
                string code = string.Empty;
                if (null != left)
                {
                    if (left is Block)
                    {
                        result += (left as Block).LHS + " ";
                    }
                    else
                    {
                        // TODO Jun: Implement a codeblock ToScript function such that it returns the contents, not the name
                        result += left.ToScript() + " ";
                    }
                }
                else
                {
                    result += ProtoCore.DSDefinitions.Keyword.Null + " ";
                }

                // Handle operator
                if (null != op)
                {
                    result += op.ToScript();
                }

                //Handle right
                if (null != right)
                {
                    if (right is Block)
                    {
                        result += (right as Block).LHS + " ";
                    }
                    else
                    {
                        result += right.ToScript() + " ";
                    }
                }
                else
                {
                    result += " " + ProtoCore.DSDefinitions.Keyword.Null;
                }
            }
            else
            {
                string[] repGuideforParam = ((Operator)op).replicationGuide.Split('¡');
                string[] repGuideForLeftNode = repGuideforParam[0].Split(',');
                string[] repGuideForRightNode = repGuideforParam[1].Split(',');
                if (null != left)
                {
                    if (left is Block)
                    {
                        result += (left as Block).LHS + " ";
                    }
                    else
                    {
                        result += left.ToScript() + " ";
                    }
                    //for (int j = 0; j < repGuideForLeftNode.Length - 1; j++)
                    //    result += "<" + repGuideForLeftNode[j] + ">";

                    result += SnapshotNode.ParseReplicationGuideText(repGuideforParam[0]);

                    result += " ";
                }
                else result += ProtoCore.DSDefinitions.Keyword.Null + " ";
                if (null != op)
                    result += op.ToScript();
                if (null != right)
                {
                    if (right is Block)
                    {
                        result += (right as Block).LHS + " ";
                    }
                    else
                    {
                        result += right.ToScript() + " ";
                    }
                    //for (int j = 0; j < repGuideForRightNode.Length - 1; j++)
                    //    result += "<" + repGuideForRightNode[j] + ">";


                    result += SnapshotNode.ParseReplicationGuideText(repGuideforParam[1]);

                    result += " ";
                }
                else result += " " + ProtoCore.DSDefinitions.Keyword.Null;
            }
            return result + ")";
        }
    }

    /// <summary>
    /// Represents unary operators which take 1 operand. Eg. ++i;i--;
    /// </summary>
    public class UnaryExprNode : Expr
    {
        string op;
        Expr e;
        /// <summary>
        /// Represents unary expressions which take one operand.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal UnaryExprNode(string x, Expr y)
            : base(y.Name, y.Guid)
        {
            op = x;
            e = y;
        }

        public override string ToScript()
        {
            throw new NotImplementedException();
        }

        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    /// <summary>
    /// Represents calls to functions eg. Math.Sin(1.56)
    /// </summary>
    public class FunctionCall : Expr
    {
        Node func;
        /// <summary>
        /// List of parameters, index -> entry
        /// Not a list as during assembly it may not be a complete mapping
        /// </summary>
        public Dictionary<int, Node> parameters = new Dictionary<int, Node>();
        string name;

        internal FunctionCall(string name, uint guid)
            : base(name, guid)
        {
            this.name = name;
        }
        internal FunctionCall(Func f)
            : base(f.Name, f.Guid)
        {
            func = f;
            if (f.numParameters != 0)
            {
                int k = f.numParameters;
                while (k > 0)
                {
                    parameters.Add(k - 1, null); 
                    k--;
                }
            }
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
        public override string ToScript()
        {
            string result = string.Empty;
            #region StaticFunctrionPrinting
            if (((Func)func).isStatic)
            {
                if (null != func)
                {
                    if (((Func)func).isRange)
                    {
                    }
                    else
                    {
                        result += func.ToScript();
                    }
                }
                #region Range
                if (((Func)func).isRange)
                {
                    if (null != parameters)
                    {
                        var sortedDict = (from entry in parameters orderby entry.Key ascending select entry)
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                        IEnumerable iter = sortedDict;
                        int count = 0;
                        if (((Func)func).replicationGuide.Equals("") || ((Func)func).replicationGuide == null)
                        {
                            foreach (Node node in sortedDict.Values)
                            {
                                string arg = string.Empty;
                                count++;
                                if (node != null)
                                {
                                    if (node is Block)
                                    {
                                        arg = (node as Block).LHS;
                                    }
                                    else
                                    {
                                        // TODO Jun: Implement a codeblock ToScript function such that it returns the contents, not the name
                                        arg = node.ToScript();
                                    }
                                    if (count == 3)
                                    {
                                        if (((Func)func).argTypeRange == 0) result += arg + "..";
                                        else if (((Func)func).argTypeRange == 1) result += "~" + arg + "..";
                                        else result += "#" + arg + "..";
                                    }
                                    else
                                        result += arg + "..";
                                }
                                else
                                {
                                    if (count == 3)
                                        ;
                                    else
                                        result += ProtoCore.DSDefinitions.Keyword.Null + "..";
                                }
                            }
                        }
                        else
                        {
                            string[] repGuideforParam = ((Func)func).replicationGuide.Split('¡');
                            int i = 0;
                            foreach (Node node in sortedDict.Values)
                            {
                                string[] repGuideForNode = repGuideforParam[i++].Split(',');
                                string arg = string.Empty;
                                count++;
                                if (node != null)
                                {
                                    if (node is Block)
                                    {
                                        arg = (node as Block).LHS;
                                    }
                                    else
                                    {
                                        // TODO Jun: Implement a codeblock ToScript function such that it returns the contents, not the name
                                        arg = node.ToScript();
                                    }
                                    if (count == 3)
                                    {
                                        if (((Func)func).argTypeRange == 0)
                                        {
                                            result += arg;// +
                                            for (int j = 0; j < repGuideForNode.Length - 1; j++)
                                                result += "<" + repGuideForNode[j] + ">";
                                            result += "..";
                                            if (i == repGuideforParam.Length - 1) break;
                                        }
                                        else if (((Func)func).argTypeRange == 1)
                                        {
                                            result += "~" + arg;// +"..";
                                            for (int j = 0; j < repGuideForNode.Length - 1; j++)
                                                result += "<" + repGuideForNode[j] + ">";
                                            result += "..";
                                            if (i == repGuideforParam.Length - 1) break;
                                        }
                                        else
                                        {
                                            result += "#" + arg;// +"..";
                                            for (int j = 0; j < repGuideForNode.Length - 1; j++)
                                                result += "<" + repGuideForNode[j] + ">";
                                            result += "..";
                                            if (i == repGuideforParam.Length - 1) break;
                                        }
                                    }
                                    else
                                    {
                                        result += arg;// +"..";
                                        for (int j = 0; j < repGuideForNode.Length - 1; j++)
                                            result += "<" + repGuideForNode[j] + ">";
                                        result += "..";
                                    }
                                }
                                else
                                {
                                    if (count == 3)
                                        ;
                                    else
                                        result += ProtoCore.DSDefinitions.Keyword.Null + "..";
                                }
                            }
                        }
                        result = result.Substring(0, result.Length - 2);
                    }
                }
                #endregion
                else
                {
                    result += "(";
                    if (null != parameters)
                    {
                        var sortedDict = (from entry in parameters orderby entry.Key ascending select entry)
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                        IEnumerable iter = sortedDict;
                      
                        string[] repGuideforParam = ((Func)func).replicationGuide.Split(GraphToDSCompiler.Constants.ReplicationGuideDelimiter);
                        int n = 0;
                        foreach (Node node in sortedDict.Values)
                        {
                            string arg = string.Empty;
                            if (node != null)
                            {
                                if (node is Block)
                                {
                                    arg = (node as Block).LHS + " ";
                                }
                                else
                                {
                                    arg = node.ToScript() + " ";
                                }

                                result += " " + arg;

                                if (n < repGuideforParam.Length)
                                {
                                    result += SnapshotNode.ParseReplicationGuideText(repGuideforParam[n]);
                                }

                                result += ",";
                            }
                            else
                            {
                                result += " " + ProtoCore.DSDefinitions.Keyword.Null + ",";
                            }
                            n++;
                        }
                    }
                    if (parameters.Count > 0)
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                    result += " )";
                }
            }
            #endregion

            else
            {
                if (null != func)
                {
                    if (null != parameters)
                    {
                        var sortedDict = (from entry in parameters orderby entry.Key ascending select entry)
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                        IEnumerable iter = sortedDict;
                        int count = 0;
                        if (string.IsNullOrEmpty(((Func)func).replicationGuide))
                        {
                            foreach (Node node in sortedDict.Values)
                            {
                                string arg = string.Empty;
                                count++;
                                if (node != null)
                                {
                                    if (count == 1)
                                    {
                                        result += node.ToScript() + "." + func.ToScript();
                                        if (!((Func)func).isProperty)
                                        {
                                            result += "(";
                                        }
                                    }
                                    else
                                    {
                                        if (node is Block)
                                        {
                                            arg = (node as Block).LHS + " ";
                                        }
                                        else
                                        {
                                            arg = node.ToScript() + " ";
                                        }
                                        result += " " + arg + ",";
                                    }
                                }
                                else
                                {
                                    if (count == 1)
                                    {
                                        result += Constants.kwTempNull + "." + func.ToScript();
                                        if (!((Func)func).isProperty)
                                        {
                                            result += "(";
                                        }
                                    }
                                    else
                                    {
                                        result += " " + ProtoCore.DSDefinitions.Keyword.Null + ",";
                                    }
                                }
                            }
                        }
                        else
                        {

                            string[] repGuideforParam = ((Func)func).replicationGuide.Split(GraphToDSCompiler.Constants.ReplicationGuideDelimiter);
                            int i = 0;
                            int n = 0;
                            foreach (Node node in sortedDict.Values)
                            {
                                string arg = string.Empty;
                                count++;
                                string[] repGuideForNode = repGuideforParam[i++].Split(',');
                                if (node != null)
                                {
                                    if (count == 1)
                                    {
                                        result += node.ToScript();
                                        result += SnapshotNode.ParseReplicationGuideText(repGuideforParam[n]);
                                        result += "." + func.ToScript() + "("; 
                                    }
                                    else
                                    {
                                        if (node is Block)
                                        {
                                            arg = (node as Block).LHS + " ";
                                        }
                                        else
                                        {
                                            arg = node.ToScript() + " ";
                                        }
                                        result += " " + arg;
                                        result += SnapshotNode.ParseReplicationGuideText(repGuideforParam[n]);
                                        result += ",";

                                        if (i == repGuideforParam.Length - 1) break;
                                    }
                                }
                                else
                                {
                                    if (count == 1)
                                    {
                                        result += Constants.kwTempNull + "." + func.ToScript() + "(";
                                    }
                                    else
                                    {
                                        result += " " + ProtoCore.DSDefinitions.Keyword.Null + ",";
                                    }
                                }
                                n++;
                            }
                        }
                        if (count == 0)
                        {
                            result += func.ToScript();
                        }
                    }

                    if (result.EndsWith(","))
                    {
                        result = result.Substring(0, result.Length - 1);
                    }

                    if (!((Func)func).isProperty)
                    {
                        result += " )";
                    }
                }
            }
            return result;
        }
    }

    public class ArrayNode : Expr
    {
        string content;
        internal ArrayNode(string content, uint guid)
            : base(content, guid)
        {
            this.content = content;
        }

        public override string ToScript()
        {
            string result = string.Empty;
            result += "{ ";

            result += content;
            return result + " }";
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }
    /*class RangeExprNode : Expr
   {
       Node upLim;
       Node loLim;
       Node interval;
       Node approxInterval;
       Node numEl;
       internal RangeExprNode() { }
       internal override string EmitCode()
       {
           return " ";
       }

       public override string ToScript()
       {
           string result = string.Empty;
           result += "(";

           return result + ")";
       }
   }*/
    #endregion

    #region SpecialNodes
    /// <summary>
    /// Represents an identifier node
    /// </summary>
    public class IdentNode : Node
    {
        string name;

        public IdentNode(string name, uint guid)
            : base(name, guid)
        {
            this.name = name;
        }

        internal IdentNode(Node o)
            : base(o.Name, o.Guid)
        {
        }

        public override string ToScript()
        {
            return this.Name;
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    /// <summary>
    /// Represents an import node
    /// </summary>
    public class ImportNode : Node
    {
        private string importStatement;
        public string ModuleName { get; private set; }

        internal ImportNode(string moduleName, uint guid)
            : base(moduleName, guid)
        {
            ModuleName = moduleName;
        }
        public override string ToScript()
        {
            Validity.Assert(ModuleName != null);
            importStatement = "import (" + '"'.ToString() + ModuleName + '"'.ToString() + ")";
            return this.importStatement;
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    /// <summary>
    /// Represents an Function Node like Sin,Cos etc.
    /// </summary>
    public class Func : Node
    {
        static internal string symb;
        static internal uint guid;
        public string tempName;
        public bool isRange;
        public int argTypeRange;
        public int numParameters;
        public string replicationGuide;
        public bool isStatic;
        public bool isProperty;
        public bool isMemberFunction = false;

        internal Func(string symbol, uint guid)
            : base(symbol, guid)
        {
            symb = symbol;
            guid = guid;
        }
        internal Func(string symbol, uint guid, int args)
            : base(symbol, guid)
        {
            symb = symbol;
            guid = guid;
            numParameters = args; replicationGuide = "";
        }
        internal Func(string symbol, uint guid, int args, string replicationGuide)
            : base(symbol, guid)
        {
            symb = symbol;
            guid = guid;
            numParameters = args;
            this.replicationGuide = replicationGuide;
        }
        public override string ToScript()
        {
            return this.Name;
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    /// <summary>
    /// Represents operator node like +,-,*,/
    /// </summary>
    public class Operator : Node
    {
        public string tempName;
        static internal string symb;
        static internal uint guid;
        public string replicationGuide;
        static internal string EQU() { symb = "="; return "="; }
        static internal string LSS() { symb = "<"; return "<"; }
        static internal string GTR() { symb = "<"; return "<"; }
        static internal string ADD() { symb = "+"; return "+"; }
        static internal string SUB() { symb = "-"; return "-"; }
        static internal string MUL() { symb = "*"; return "*"; }
        static internal string DIV() { symb = "/"; return "/"; }
        static internal string MOD() { symb = "%"; return "%"; }

        internal Operator(string symbol, uint guid)
            : base(symbol, guid)
        {
            symb = symbol;
            guid = guid;
            replicationGuide = "";
        }
        internal Operator(string symbol, uint guid, string replicationGuides)
            : base(symbol, guid)
        {
            symb = symbol;
            guid = guid;
            replicationGuide = replicationGuides;
        }
        public override string ToScript()
        {
            //if (this.GetChildren().Count == 0) return "null" + this.Name + "null";
            return this.Name;
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    #endregion

    #region LITERALS

    /// <summary>
    /// Represents a literal type like bool,string,int,double 
    /// </summary>
    public class LiteralNode : Node
    {

        string value;
        public string tempName;

        public LiteralNode(string val, uint guid)
            : base(val, guid)
        {
            value = val;
        }

        public override string ToScript()
        {
            return value.ToString();
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }
   
    #endregion

    #region STATEMENTS

    /// <summary>
    /// Represents a valid statement node
    /// </summary>
    public class Statement : Node
    {
        internal static int indent = 0;
        private uint guid;

        /*Tron's*/
        public bool wasModified = false;

        public Statement(string name, uint guid)
            : base(name, guid)
        {
            this.guid = guid;
        }

        public override string ToScript()
        {
            throw new NotImplementedException();
        }
        public override string ToCode()
        {
            return this.ToScript();
        }
    }

    /// <summary>
    /// Represents a assignment of the form left = right. 
    /// </summary>
    public class Assignment : Statement
    {
        internal Node left;
        internal Node right;
        public ProtoCore.DSASM.Operator Optr{get; private set;}

        internal Assignment(Node o, Expr e)
            : base("=", e.Guid)
        {
            left = o; right = e;
        }
        internal Assignment(Node o, LiteralNode l)
            : base("=", l.Guid)
        {
            left = o; right = l;
        }

        internal Assignment(IdentNode identNodeL, IdentNode identNodeR)
            : base("=", identNodeL.Guid)
        {
            // TODO: Complete member initialization
            left = identNodeL; right = identNodeR;
        }


       
        //
        // TODO Jun: Deprecate the overloaded constructors. 
        // All assignment nodes must be instantiated with this constructor
        public Assignment(Node left, Node right)
            : base("", left.Guid)
        {
            Optr = ProtoCore.DSASM.Operator.assign;
            base.Name = ProtoCore.Utils.CoreUtils.GetOperatorString(Optr);
            this.left = left;
            this.right = right;
        }


        public override string ToScript()
        {
            return left.ToScript() + "=" + right.ToScript();
        }
        public override string ToCode()
        {
            //if (right is GraphToDSCompiler.BinExprNode)
            //{
            //    if ((right as GraphToDSCompiler.BinExprNode).left != null && (right as GraphToDSCompiler.BinExprNode).right != null)
            //    {
            //        return left.ToScript() + "=" + right.ToScript();
            //    }
            //    else
            //        return null;
            //}
            //else if (right is IdentNode || right is LiteralNode || right is FunctionCall)
            //{
            //    return left.ToScript() + "=" + right.ToScript();
            //}
            //else
            //    return null;
            return this.ToScript();
        }
    }


    class Block : Statement
    {
        internal string content;
        
        public string LHS {get; set; }
        public Dictionary<string, string> outputIndexMap = new Dictionary<string, string>();
        public Dictionary<string, Statement> stats = new Dictionary<string, Statement>();
        public List<AssignmentStatement> assignmentData { get; private set; }
        public uint splitFomUint=0;

        void add(string a, Statement s)
        {
            stats.Add(a, s);
        }

        internal Block(string code, uint guid, List<AssignmentStatement> assignments = null)
            : base(code, guid)
        {
            this.Name = code;

            // Comment Jun: We want to get the LHS given a line of DS code
            // This line of code must contain an assignment operator
            int index = code.IndexOf('=');

            // TODO Jun: Handle '=' characters that is not an assignment statement

            // IndexOf return -1 if the char was not found
            LHS = string.Empty;
            if (index != -1)
            {
                LHS = code.Substring(0, index);
                LHS = LHS.Replace(" ", string.Empty);
                LHS = LHS.Trim('\n');
            }

            // Store all rhs identifiers of this codeblocks' expression
            assignmentData = new List<AssignmentStatement>();
            if (null != assignments)
            {
                this.assignmentData.AddRange(assignments);
            }

        }

        /// <summary>
        /// TrimName is a function that trims the Name of a codeblock in order to retrieve the idetntifier
        /// This function contains the intelligence what tokens it needs to trim from the Name
        /// </summary>
        /// <returns></returns>
        public string TrimName()
        {
            string trimmedstring = Name;
            Validity.Assert(null != trimmedstring);
            trimmedstring = trimmedstring.Replace(" ", string.Empty);
            trimmedstring = trimmedstring.Trim('\n');
            trimmedstring = trimmedstring.Trim(';');
            return trimmedstring;
        }

        public void SetData(string lhs, string sourceContents)
        {
            this.LHS = lhs;
            this.Name = sourceContents;
            this.content = sourceContents;
        }

        internal string EmitCode()
        {
            return this.Name + ";";
        }

        public override string ToScript()
        {
            // TODO Jun: Implement a codeblock ToScript function such that it returns the contents, not the name
            return this.Name;
        }

        public override string ToCode()
        {
            return this.Name;
        }
    }
    #endregion

}
