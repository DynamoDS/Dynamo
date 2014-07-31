using System;
using System.Collections.Generic;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo
{
    public class WorkspaceMigrationsRevit
    {
        [WorkspaceMigration("0.6.3.0", "0.6.4.0")]
        public static void Migrate_0_6_3_to_0_6_4_0(XmlDocument doc)
        {
            //replace all the instances of Dynamo.Nodes.dynXYZZero with a Dynamo.Nodes.XYZ
            XmlNodeList elNodes = doc.GetElementsByTagName("Elements");

            if (elNodes.Count == 0)
                elNodes = doc.GetElementsByTagName("dynElements");

            var elementsRoot = elNodes[0];

            var corrections = new List<Tuple<XmlNode, XmlNode>>();

            foreach (XmlElement elNode in elementsRoot.ChildNodes)
            {
                #region add isVisible

                if (elNode.Attributes["isVisible"] == null)
                {
                    elNode.SetAttribute("isVisible", "True");
                }

                if (elNode.Attributes["isUpstreamVisible"] == null)
                {
                    elNode.SetAttribute("isUpstreamVisible", "True");
                }

                #endregion

                #region replace XyzZero with XYZ
                if (elNode.Name == "Dynamo.Nodes.dynXYZZero" ||
                    elNode.Name == "Dynamo.Nodes.XYZZero" ||
                    elNode.Name == "Dynamo.Nodes.XyzZero")
                {
                    //create a new node to replace the old one
                    var newNode = doc.CreateElement("Dynamo.Nodes.Xyz");
                    newNode.SetAttribute("type", "Dynamo.Nodes.Xyz");
                    newNode.SetAttribute("guid", elNode.Attributes["guid"].Value);
                    newNode.SetAttribute("nickname", "XYZ");
                    newNode.SetAttribute("x", elNode.Attributes["x"].Value);
                    newNode.SetAttribute("y", elNode.Attributes["y"].Value);
                    newNode.SetAttribute("isVisible", elNode.Attributes["isVisible"].Value);
                    newNode.SetAttribute("isUpstreamVisible", elNode.Attributes["isUpstreamVisible"].Value);
                    newNode.SetAttribute("lacing", elNode.Attributes["lacing"].Value);

                    //add some info about the default ports
                    var port1Node = doc.CreateElement("PortInfo");
                    port1Node.SetAttribute("index", "0");
                    port1Node.SetAttribute("default", "True");
                    var port2Node = doc.CreateElement("PortInfo");
                    port2Node.SetAttribute("index", "1");
                    port2Node.SetAttribute("default", "True");
                    var port3Node = doc.CreateElement("PortInfo");
                    port3Node.SetAttribute("index", "2");
                    port3Node.SetAttribute("default", "True");

                    newNode.AppendChild(port1Node);
                    newNode.AppendChild(port2Node);
                    newNode.AppendChild(port3Node);

                    corrections.Add(new Tuple<XmlNode, XmlNode>(newNode, elNode));

                    //elementsRoot.InsertBefore(newNode, elNode);
                    //elementsRoot.RemoveChild(elNode);
                }
                #endregion
            }

            foreach (var correction in corrections)
            {
                elementsRoot.InsertBefore(correction.Item1, correction.Item2);
                elementsRoot.RemoveChild(correction.Item2);
            }
        }
    }
}
