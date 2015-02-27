using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;

namespace ProtoCore.Utils
{
    /// <summary>
    /// These are string manipulation utility functions that focus on lexing and parsing heuristics
    /// </summary>
    public static class ParserUtils
    {
        /// <summary>
        /// Retrieves the lhs identifer of a string
        /// </summary>
        /// <returns></returns>
        public static string GetLHSString(string code)
        {
            if (code != null && code != string.Empty && code.Length > 0)
            {
                for (int n = 0; n < code.Length; ++n)
                {
                    if (code[n] == ' ' || code[n] == '=')
                    {
                        string lhs = code.Substring(0, n);

                        // We must also handle literal "\n" appearing in the identifier
                        lhs = lhs.Replace("\n", "");
                        return lhs;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///  Splits the lines of code given deimiters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetStatementsString(string input)
        {
            List<string> expr = new List<string>();
            int index = 0;
            int oldIndex = 0;
            do
            {
                index = input.IndexOf(";", index);
                if (index != -1)
                {
                    string sub;
                    if (index < input.Length - 2) 
                    {
                        if (input[index + 1].Equals('\r') && input[index + 2].Equals('\n'))
                        {
                            index += 2;
                        }
                    }
                    sub = input.Substring(oldIndex, index - oldIndex + 1);
                    expr.Add(sub);
                    index++;
                    oldIndex = index;
                }
            } while (index != -1);
            return expr;
        }

    }
}
