﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;

using NUnit.Framework;

namespace Dynamo.Tests
{
    public class RecentFileTests : DynamoViewModelUnitTest
    {
        [Test]
        public void SavingFilesUpdateRecentFileList()
        {
            // Open a file
            var examplePath = Path.Combine(TestDirectory, @"core\math", "Add.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            List<string> paths = new List<string>();

            // Save the file as different files for (maxNum + 1) times
            int maxNum = ViewModel.Model.PreferenceSettings.MaxNumRecentFiles;
            for (int i = 0; i < maxNum + 1; i++)
            {
                var newPath = GetNewFileNameOnTempPath("dyn");
                var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerRuntimeCore);
                Assert.IsTrue(res);
                paths.Add(newPath);
            }

            // Ensure the number of recent files reaches the maximum number
            Assert.AreEqual(maxNum, ViewModel.Model.PreferenceSettings.RecentFiles.Count);

            // Ensure the recent files are recent
            for (int i = 0; i < maxNum; i++)
            {
                Assert.AreEqual(0, string.CompareOrdinal(paths.ElementAt(maxNum - i), ViewModel.Model.PreferenceSettings.RecentFiles[i]));
            }

            // Clear and delete the temporary files
            ViewModel.RecentFiles.Clear();
            foreach (var filePath in paths)
            {
                File.Delete(filePath);
            }
        }
        
        [Test]
        public void SetMaxNumRecentFiles()
        {
            ViewModel.Model.PreferenceSettings.MaxNumRecentFiles = 0;

            Assert.AreEqual(PreferenceSettings.DefaultMaxNumRecentFiles, ViewModel.Model.PreferenceSettings.MaxNumRecentFiles);
        }
    }
}
