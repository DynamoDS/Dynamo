using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    // Bag of utilities designed for use during tests
    internal static class TestUtilities
    {
        // Simple string that we can store in DynamoWebView2 instances so that we can track them down more easily
        internal static string WebView2Tag;

        internal static string UserDataFolderDuringTests(string appName)
        {
            var directory = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test", $"webview2_{Environment.ProcessId}_{appName}_appdata");
        }
    }
}
