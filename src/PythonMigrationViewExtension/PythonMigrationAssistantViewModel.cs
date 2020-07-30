using Dynamo.PythonMigration.MigrationAssistant;
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationAssistantViewModel
    {
        public string OldCode { get; set; }
        public string NewCode { get; set; }
        private PythonNode PythonNode { get; set; }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode)
        {
            PythonNode = pythonNode;
            OldCode = pythonNode.Script;
            MigrateCode();
        }

        private void MigrateCode()
        {
            NewCode = ScriptMigrator.MigrateCode(OldCode);
        }

        public void ChangeCode()
        {
            PythonNode.MigrateCode(NewCode);
        }
    }
}
