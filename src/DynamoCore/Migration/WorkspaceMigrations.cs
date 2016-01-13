using System.Xml;
using Dynamo.Logging;
using Dynamo.Models;

namespace Dynamo.Migration
{
    public class WorkspaceMigrations
    {
        public WorkspaceMigrations(DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;
        }

        private DynamoModel dynamoModel;

        #region Migrations

        [WorkspaceMigration("0.5.3.0", "0.6.1.0")]
        public void Migrate_0_5_3_to_0_6_0(XmlDocument doc)
        {
            dynamoModel.Logger.LogWarning("Applying model migration from 0.5.3.x to 0.6.1.x", WarningLevel.Mild);
        }

        #endregion
    }
}
