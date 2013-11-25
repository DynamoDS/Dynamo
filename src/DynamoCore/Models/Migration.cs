using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    internal class Migration
    {
        /// <summary>
        /// A version after which this migration will be applied.
        /// </summary>
        public Version Version { get; set; }
        
        /// <summary>
        /// The action to perform during the upgrade.
        /// </summary>
        public Action Upgrade { get; set; }

        /// <summary>
        /// A migration which can be applied to a workspace to upgrade the workspace to the current version.
        /// </summary>
        /// <param name="v">A version number specified as x.x.x.x after which a workspace will be upgraded</param>
        /// <param name="upgrade">The action to perform during the upgrade.</param>
        public Migration(Version v, Action upgrade)
        {
            Version = v;
            Upgrade = upgrade;
        }

        public static void ProcessWorkspaceMigrations(XmlDocument xmlDoc, string version)
        {

            var migrations =
                (from method in typeof(WorkspaceMigrations).GetMethods(BindingFlags.Public | BindingFlags.Static)
                 let attribute =
                     method.GetCustomAttributes(false)
                           .OfType<WorkspaceMigrationAttribute>()
                           .FirstOrDefault()
                 where attribute != null
                 let result = new { method, attribute.From, attribute.To }
                 orderby result.From
                 select result).ToList();

                var currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;
                var workspaceVersion = string.IsNullOrEmpty(version) ? new Version() : new Version(version);

                while (workspaceVersion != null && workspaceVersion < currentVersion)
                {
                    var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                    if (nextMigration == null)
                        break;

                    nextMigration.method.Invoke(null, new object[] { xmlDoc });
                    workspaceVersion = nextMigration.To;
                }
        }
    }

    /// <summary>
    /// Marks methods on a NodeModel to be used for version migration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeMigrationAttribute : Attribute
    {
        /// <summary>
        /// Latest Version this migration applies to.
        /// </summary>
        public Version From { get; private set; }

        /// <summary>
        /// Version this migrates to.
        /// </summary>
        public Version To { get; private set; }

        public NodeMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class WorkspaceMigrationAttribute : Attribute
    {
        public Version From { get; private set; }
        public Version To { get; private set; }

        public WorkspaceMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }
}
