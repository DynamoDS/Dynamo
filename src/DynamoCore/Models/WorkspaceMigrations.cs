﻿using System.Xml;
using Dynamo.Models;

namespace Dynamo
{
    public class WorkspaceMigrations
    {
        #region Migrations

        [WorkspaceMigration("0.5.3.0", "0.6.1.0")]
        public static void Migrate_0_5_3_to_0_6_0(XmlDocument doc)
        {
            DynamoLogger.Instance.LogWarning("Applying model migration from 0.5.3.x to 0.6.1.x", WarningLevel.Mild);
        }

        #endregion
    }
}
