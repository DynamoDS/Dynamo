using System;
using System.IO;
using System.Linq;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    internal static class RenameCommand
    {
        internal static void HandleRename(RenameOptions opts)
        {
            if (opts.InputMdFile is null && opts.InputMdDirectory != null)
            {
                RenameDirectory(opts.InputMdDirectory, opts.MaxLength);
            }
            else if (opts.InputMdFile != null && opts.InputMdDirectory is null)
            {
                RenameFile(opts.InputMdFile);
            }
            else
            {
                Console.WriteLine("Invalid options: You can rename a single file using the file option\nor rename multiple files in a directory (if they are longer than max length)\nusing the directory option");
            }
        }

        private static void RenameFile(string file)
        {
            var extension = Path.GetExtension(file);
            if (!extension.Equals(".md", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"Can only rename MD files: {file}");
                return;
            }

            if (!File.Exists(file))
            {
                Console.WriteLine($"File not found: {file}");
                return;
            }

            var baseName = Path.GetFileNameWithoutExtension(file);
            var shortName = Dynamo.Utilities.Hash.GetHashFilenameFromString(baseName);

            RenameFile(file, baseName, shortName);
        }

        private static void RenameFile(string file, string baseName, string shortName)
        {
            var content = File.ReadAllText(file);
            content = content.Replace(baseName, shortName);
            var path = Path.GetDirectoryName(file);
            var newFile = Path.Combine(path, shortName + ".md");
            File.WriteAllText(newFile, $"<!--- {baseName} --->\n<!--- {shortName} --->\n" + content);
            File.Delete(file);

            var allSupportFiles = Directory.GetFiles(path, baseName + ".*", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x)).ToList();
            allSupportFiles.AddRange(Directory.GetFiles(path, baseName + "_img.*", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x)).ToList());

            foreach (var supportFile in allSupportFiles)
            {
                var newName = Path.Combine(supportFile.DirectoryName,
                    supportFile.Name.Replace(baseName, shortName));
                supportFile.MoveTo(newName);
            }
        }

        private static void RenameDirectory(string directory, int maxLength)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Directory not found: {directory}");
                return;
            }

            var allMdFiles = Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly).Select(x => new FileInfo(x)).ToList();

            foreach (var mdFile in allMdFiles)
            {
                if (mdFile.Name.Length > maxLength)
                {
                    var baseName = Path.GetFileNameWithoutExtension(mdFile.Name);
                    var shortName = Dynamo.Utilities.Hash.GetHashFilenameFromString(baseName);
                    RenameFile(mdFile.FullName, baseName, shortName);
                }
            }
        }
    }
}
