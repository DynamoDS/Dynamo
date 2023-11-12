using System;
using System.Text;
using Dynamo.Logging;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class CommandHandler
    {
        internal static string HandleFromPackage(FromPackageOptions opts)
        {
            try
            {
                FromPackageFolderCommand.HandlePackageDocumentation(opts);
            }
            catch (Exception e)
            {
                LogExceptionToConsole(e);
            }

            return "";
        }

        internal static string HandleFromDirectory(FromDirectoryOptions opts)
        {
            try
            {
                FromDirectoryCommand.HandleDocumentationFromDirectory(opts);
            }
            catch (Exception e)
            {
                LogExceptionToConsole(e);
            }

            return "";
        }

        internal static string HandleRename(RenameOptions opts)
        {
            try
            {
                RenameCommand.HandleRename(opts);
            }
            catch (Exception e)
            {
                LogExceptionToConsole(e);
            }

            return "";
        }

        internal static void LogExceptionToConsole(Exception e)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"{e.GetType()} :");
            strBuilder.AppendLine(e.Message);
            strBuilder.AppendLine(e.StackTrace);
            Console.WriteLine(strBuilder.ToString());
        }
    }

    /// <summary>
    /// A logger implementation to be used as a parameter to construct Dynamo core model types
    /// </summary>
    internal class DummyConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string tag, string message)
        {
            throw new NotImplementedException();
        }

        public void Log(Exception e)
        {
            CommandHandler.LogExceptionToConsole(e);
        }

        public void LogError(string error)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string warning, WarningLevel level)
        {
            throw new NotImplementedException();
        }
    }
}
