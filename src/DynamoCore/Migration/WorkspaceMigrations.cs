using System.Xml;
using Dynamo.Logging;
using Dynamo.Models;

namespace Dynamo.Migration
{
    /// <summary>
    /// Contains methods to migrate a workspace from one version to another
    /// </summary>
    public class WorkspaceMigrations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceMigrations"/> class 
        /// with current <see cref="DynamoModel"/> object.
        /// </summary>
        /// <param name="dynamoModel">Current <see cref="DynamoModel"/> object.</param>
        public WorkspaceMigrations(DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;
        }

        private DynamoModel dynamoModel;

        #region Migrations

        /// <summary>
        /// Performs migration of a workspace from 0.5.3.0 to 0.6.1.0.
        /// </summary>
        /// <param name="doc">The <see cref="XmlDocument"/> object.</param>
        [WorkspaceMigration("0.5.3.0", "0.6.1.0")]
        public void Migrate_0_5_3_to_0_6_0(XmlDocument doc)
        {
            dynamoModel.Logger.LogWarning("Applying model migration from 0.5.3.x to 0.6.1.x", WarningLevel.Mild);
        }

        #endregion
    }
}
