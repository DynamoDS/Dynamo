using Dynamo.Core;
using Dynamo.Engine;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;
using ProtoCore;
using Compiler = ProtoAssociative.Compiler;

namespace NodeDocumentationMarkdownGenerator
{
    internal class CommandHandler
    {
        private readonly LibraryServices libraryService;

        internal CommandHandler()
        {
            var libraryCore = new ProtoCore.Core(new Options());
            libraryCore.Compilers.Add(Language.Associative, new Compiler(libraryCore));
            libraryCore.Compilers.Add(Language.Imperative, new ProtoImperative.Compiler(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;
            this.libraryService = new LibraryServices(libraryCore, new PathManager(new PathManagerParams()));
        }

        internal string HandleFromPackage(FromPackageOptions opts)
        {
            var command = new FromPackageFolderCommand(libraryService);
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