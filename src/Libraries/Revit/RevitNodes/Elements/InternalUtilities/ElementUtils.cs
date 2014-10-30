using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace Revit.Elements.InternalUtilities
{
    [IsVisibleInDynamoLibrary(false)]
    public class ElementUtils
    {
        /// <summary>
        /// This function checks if the name ends with "(num)". Here num is a integer.
        /// If yes, it will replace "(num)" with "(num+1)". Here num+1 is the form of the
        /// evaluated integer. Otherwise, it will append "(1)" at the end of the name.
        /// For example:
        /// This function will change the name from "abc(2)" to "abc(3)",
        /// from "abc" to "abc(1)".
        /// </summary>
        /// <param name="name"></param>
        public static void UpdateLevelName(ref string name)
        {
            if (name.EndsWith(")"))
            {
                int index = name.LastIndexOf("(");
                if (index < 0)
                {
                    name += "(1)";
                }
                else
                {
                    int length = name.Length - index - 2;
                    string strNumber = name.Substring(index + 1, length);
                    int number = 0;
                    if (int.TryParse(strNumber, out number))
                    {
                        number = number + 1;
                        name = name.Substring(0, index) + "(" + number.ToString() + ")";
                    }
                    else
                    {
                        name += "(1)";
                    }
                }
            }
            else
            {
                name += "(1)";
            }
        }
    }
}
