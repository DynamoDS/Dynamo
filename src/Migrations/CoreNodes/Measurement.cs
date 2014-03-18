using System.Globalization;
using System.Xml;
using Dynamo.Models;
using Dynamo.Units;
using Migrations;

namespace Dynamo.Nodes
{
    public class LengthInput : MigrationNode
    {
        [NodeMigration(from:"0.6.2")]
        public void MigrateLengthFromFeetToMeters(XmlNode node)
        {
            //length values were previously stored as decimal feet
            //convert them internally to SI meters.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "System.Double")
                {
                    if (child.Attributes != null && child.Attributes.Count > 0)
                    {
                        var valueAttrib = child.Attributes["value"];
                        valueAttrib.Value = (double.Parse(valueAttrib.Value)/SIUnit.ToFoot).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }

    public class AreaInput : MigrationNode
    {
    }

    public class VolumeInput : MigrationNode
    {
    }

    public class LengthFromNumber : MigrationNode
    {
    }

    public class AreaFromNumber : MigrationNode
    {
    }

    public class VolumeFromNumber : MigrationNode
    {
    }
}

