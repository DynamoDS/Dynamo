using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Dynamo.Utilities;
using NUnit.Framework;


[SetUpFixture]
public class Setup
{
    private AssemblyHelper assemblyHelper;

    private void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        System.Console.WriteLine($"PID {Process.GetCurrentProcess().Id} Unhandled exception thrown during test {TestContext.CurrentContext.Test.Name} with message : {e.Exception.Message + Environment.NewLine + e.Exception.StackTrace}");
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        System.Console.WriteLine($"PID {Process.GetCurrentProcess().Id}  Unhandled exception thrown during test {TestContext.CurrentContext.Test.Name} with message : {ex.Message + Environment.NewLine + ex.StackTrace}");
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var moduleRootFolder = new DirectoryInfo(assemblyPath).Parent;

        var resolutionPaths = new[]
        {
            // These tests need "CoreNodeModels.dll" under "nodes" folder.
            Path.Combine(moduleRootFolder.FullName, "nodes"),
            Path.Combine(moduleRootFolder.Parent.Parent.Parent.FullName, "test", "test_dependencies")

        };

        assemblyHelper = new AssemblyHelper(moduleRootFolder.FullName, resolutionPaths, true);
        AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
        assemblyHelper = null;

        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        Dispatcher.CurrentDispatcher.UnhandledException -= CurrentDispatcher_UnhandledException;
    }
}
