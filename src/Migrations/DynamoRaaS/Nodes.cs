using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{

    public class CloudRender : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Analyze.Render.CloudRender", "Do Cloud Render");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class UploadRenderData : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Analyze.Render.UploadRenderData", "Upload Cloud Render Data");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class ExportDocumentRenderData : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "Analyze.Render.ExportDocumentRenderData", "Export Cloud Render Data");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class RenderType : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "Analyze.Render.RenderTypeDropDown", "RenderType"));

            return migrationData;
        }
    }

    public class RenderQuality : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "Analyze.Render.RenderQualityDropDown", "RenderQuality"));

            return migrationData;
        }
    }

    public class SkymodelType : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "Analyze.Render.SkyModelTypeDropDown", "SkyModelType"));

            return migrationData;
        }
    }

    public class CloudDaylightingJob : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "CloudDaylightingJob.ByViewNameDivisionsGridBoundary",
                "CloudDaylightingJob.ByViewNameDivisionsGridBoundary@string,int,int,Point,Point,Point,RenderingEnvironment");
        }
    }

    public class CloudRenderingJob : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "CloudRenderingJob.ByViewNameWidthHeight",
                "CloudRenderingJob.ByViewNameWidthHeight@string,int,int,RenderType,RenderQuality");
        }
    }

    public class MakeDaylightingSkyModel : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "SkyModel.ByTypeConstants",
                "SkyModel.ByTypeConstants@SkyModelType,double,double,double,double,double,double");
        }
    }

    public class MakeDateTime :MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DateTime.ByDateInformation",
                "DateTime.ByDateInformation@int,int,int,int,int,int,int");
        }
    }

    public class MakeDaylightingEnvironmentData : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "RenderingEnvironment.BySkyModelLocationDate",
                "RenderingEnvironment.BySkyModelLocationDate@SkyModel,double,double,DateTime");
        }
    }

    public class IlluminanceToSRGB : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "Illuminance.ToSRGB",
                "Illuminance.ToSRGB");
        }
    }

    public class LuxToFootCandles : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "Illuminance.ToFootCandles",
                "Illuminance.ToFootCandles@double");
        }
    }

    public class ParseSDF : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.Parse",
                "DaylightingResults.Parse@string");
        }
    }

    public class GetSDFResolution : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.Resolution",
                "DaylightingResults.Resolution");
        }
    }

    public class GetDaylightingIlluminanceValues : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.IlluminanceValues",
                "DaylightingResults.IlluminanceValues");
        }
    }

    public class GetDaylightingColors : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.Colors",
                "DaylightingResults.Colors@double");
        }
    }

    public class GetDaylightingNormals : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.Normals",
                "DaylightingResults.Normals");
        }
    }

    public class GetDaylightingPositions : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.Points",
                "DaylightingResults.Points");
        }
    }

    public class GetDaylightingForegroundImage : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimpleRaaS.dll", "DaylightingResults.ToImage",
                "DaylightingResults.ToImage@double");
        }
    }

}
