using System;
using Dynamo.Configuration;
using Dynamo.Logging;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class CommandHandler
    {
        internal static string HandleFromPackage(FromPackageOptions opts)
        {
            var logger = CreateLogger(opts.InputFolderPath);
            try
            {
                var command = new FromPackageFolderCommand(logger);
                command.HandlePackageDocumentation(opts);
            }
            catch (Exception e)
            {
                logger.Log(e);
            }

            Console.WriteLine(logger.LogText);
            return "";
        }

        internal static string HandleFromDirectory(FromDirectoryOptions opts)
        {
            var logger = CreateLogger(opts.LoggerPath);
            try
            {
                var command = new FromDirectoryCommand(logger);
                command.HandleDocumentationFromDirectory(opts);
            }
            catch (Exception e)
            {
                logger.Log(e);
            }

            Console.WriteLine(logger.LogText);
            return "";
        }

        private static DynamoLogger CreateLogger(string directoryPath)
        {
            var debugSettings = new DebugSettings();
            var logger = new DynamoLogger(debugSettings, directoryPath, false);
            return logger;
        }
    }
}