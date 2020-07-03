using Dynamo.PythonMigration.MigrationAssistant;
using PythonNodeModels;
using System.Collections.Generic;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationAssistantViewModel
    {
        public string OldCode { get; set; }
        public string NewCode { get; set; }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode)
        {
            OldCode = pythonNode.Script;
            MigrateCode();
        }

        private void MigrateCode()
        {
            NewCode = ScriptMigrator.MigrateCode(OldCode);
        }
    }
}
