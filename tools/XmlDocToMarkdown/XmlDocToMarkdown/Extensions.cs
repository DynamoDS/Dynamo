using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XmlDocToMarkdown
{
    public static class MarkDownExtensions
    {
        private static Dictionary<string, int> nodeDictionary = new Dictionary<string, int>()
        {
            {"typeparam", 1},
            {"returns", 2},
            {"summary", 3},
            {"param", 4},
            {"remarks", 5},
            {"example", 6},
            {"exception",7},
            {"c", 8},
            {"search", 9},
            {"SeePage", 10},
            {"SeeAnchor", 11},
            {"api_stability",12},
            {"filterpriority",13},
            {"notranslation",14}
        };

        public static IEnumerable<XNode> ReOrderXMLElements(this IEnumerable<XNode> listOfNodes)
        {
            var newListOfNodes = new List<XNode>();
            var elements = new Dictionary<XElement, int>();
            foreach (var node in listOfNodes)
            {
                var element = (XElement)node;
                if (element != null)
                {
                    if (nodeDictionary.ContainsKey(element.Name.LocalName))
                    {
                        elements.Add(element, nodeDictionary[element.Name.LocalName]);
                    }
                }
            }

            var ordered = elements.OrderBy(x => x.Value);
            foreach (var kk in ordered)
            {
                newListOfNodes.Add(kk.Key);
            }

            return newListOfNodes;
        }

        public static string MarkDownFormat(this string name, string format)
        {
            var returnString = String.Empty;
            switch (format)
            {
                case "Bold" :
                    //string can be Test or Test | stability.
                    var splits = name.Split('|');
                    if (splits.Any())
                    {
                        returnString = "**" + splits[0].Trim() + "**";
                        returnString = returnString + " | " + splits[1];
                    }
                    else
                    {
                        returnString = "**" + name + "**";
                    }
                    break;

                case "Italic":
                    if (name.Length > 0)
                    {
                        returnString = "*" + name.Trim() + "*";
                    }
                    break;

            }

            return returnString;
        }

    }
}
