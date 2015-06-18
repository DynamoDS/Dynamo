using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Models;
using DynamoShapeManager;
using Microsoft.Win32;
using NUnit.Framework;
using TestServices;

namespace Dynamo.Tests
{
    [TestFixture]
    public class BackupFileCoreTests : DynamoModelTestBase
    {
        public static readonly string backupFileDir = @"core\backupFile";
        public static readonly string testDynFile = @"addTwoNumbers.dyn";
        public static readonly string testDyfFile = @"dyfFileToBeBackedup.dyf";

        public static void StartBackupFilesTimer(DynamoModel model)
        {
            var isTest = DynamoModel.IsTestMode;
            DynamoModel.IsTestMode = false;

            model.PreferenceSettings.BackupInterval = 5000;
            var type = typeof(DynamoModel);
            MethodInfo m = type.GetMethod("StartBackupFilesTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(model, null);

            DynamoModel.IsTestMode = isTest;
        }

        [Test]
        public void AllWorkspaceFilesAreBackedup()
        {
            CurrentDynamoModel.PreferenceSettings.BackupFiles = new List<string>();
            var path1 = Path.Combine(CurrentDynamoModel.PathManager.BackupDirectory, testDynFile);
            if (File.Exists(path1))
                File.Delete(path1);
            var path2 = Path.Combine(CurrentDynamoModel.PathManager.BackupDirectory, testDyfFile);
            if (File.Exists(path2))
                File.Delete(path2);

            var ws1 = Open<HomeWorkspaceModel>(TestDirectory, backupFileDir, testDynFile);
            var ws2 = Open<CustomNodeWorkspaceModel>(TestDirectory, backupFileDir, testDyfFile);
            StartBackupFilesTimer(CurrentDynamoModel);

            System.Threading.Thread.Sleep(10000);

            Assert.IsTrue(File.Exists(path1));
            Assert.IsTrue(File.Exists(path2));
            File.Delete(path1);
            File.Delete(path2);
        }
    }
}
