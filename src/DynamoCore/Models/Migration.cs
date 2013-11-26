using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Utilities;
using System.Collections.Generic;

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
    }

    public class MigrationManager
    {
        private static MigrationManager _instance;

        /// <summary>
        /// The singleton instance property.
        /// </summary>
        public static MigrationManager Instance
        {
            get { return _instance ?? (_instance = new MigrationManager()); }
        }

        /// <summary>
        /// A collection of types which contain migration methods.
        /// </summary>
        public List<Type> MigrationTargets { get; set; }

        /// <summary>
        /// The private constructor.
        /// </summary>
        private MigrationManager()
        {
            MigrationTargets = new List<Type>();
        }

        /// <summary>
        /// Runs all migration methods found on the listed migration target types.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="version"></param>
        public void ProcessWorkspaceMigrations(XmlDocument xmlDoc, string version)
        {
            var methods = MigrationTargets.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static));

            var migrations =
                (from method in methods
                    let attribute =
                        method.GetCustomAttributes(false)
                            .OfType<WorkspaceMigrationAttribute>()
                            .FirstOrDefault()
                    where attribute != null
                    let result = new { method, attribute.From, attribute.To }
                    orderby result.From
                    select result).ToList();

            var currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;
            var workspaceVersion = String.IsNullOrEmpty(version) ? new Version() : new Version(version);

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
