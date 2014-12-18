using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

using Dynamo.Utilities;

namespace Dynamo.Models
{
    public class WorkspaceHeader
    {
        private WorkspaceHeader()
        {

        }

        public static WorkspaceHeader FromPath(DynamoModel dynamoModel, string path)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                string funName = null;
                double cx = 0;
                double cy = 0;
                double zoom = 1.0;
                string id = "";

                var topNode = xmlDoc.GetElementsByTagName(/*NXLT*/"Workspace");

                // legacy support
                if (topNode.Count == 0)
                {
                    topNode = xmlDoc.GetElementsByTagName(/*NXLT*/"dynWorkspace");
                }

                // load the header
                foreach (XmlNode node in topNode)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals(/*NXLT*/"X"))
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals(/*NXLT*/"Y"))
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals(/*NXLT*/"zoom"))
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals(/*NXLT*/"Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }
                    }
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName) && funName != /*NXLT*/"Home")
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                return new WorkspaceHeader() { ID = id, Name = funName, X = cx, Y = cy, Zoom = zoom, FileName = path };

            }
            catch (Exception ex)
            {
                dynamoModel.Logger.Log(/*NXLT*/"There was an error opening the workbench.");
                dynamoModel.Logger.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);

                if (DynamoModel.IsTestMode)
                    throw ex; // Rethrow for NUnit.

                return null;
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Zoom { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string FileName { get; set; }

        public bool IsCustomNodeWorkspace()
        {
            return !String.IsNullOrEmpty(ID);
        }
    }
}
