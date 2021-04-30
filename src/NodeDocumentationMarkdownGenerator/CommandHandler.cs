using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    internal class CommandHandler
    {
        internal CommandHandler()
        {

        }

        internal string HandleFromPackage(FromPackageOptions opts)
        {
            var command = new FromPackageFolderCommand();
            return command.HandlePackageDocumentation(opts);
        }

        internal string HandleFromDirectory(FromDirectoryOptions opts)
        {
            var command = new FromDirectoryCommand();
            command.HandleDocumentationFromDirectory(opts);
            return "";
        }
    }
}