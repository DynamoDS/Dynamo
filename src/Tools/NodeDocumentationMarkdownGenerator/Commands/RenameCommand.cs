using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    internal static class RenameCommand
    {
        /// <summary>
        /// simple output of rename operations that were executed during the command.
        /// </summary>
        internal static List<string> Log { get; set; } = new List<string>();
        internal static bool Verbose { get; set; }
        internal static void HandleRename(RenameOptions opts)
        {
            
            Verbose=opts.Verbose;
            if (Verbose)
            {
                Log.Add($"Rename Command{DateTime.Now}");
            }
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

            if (Verbose)
            {
                Log.Add($"renamed {baseName}.md : {shortName}.md");
            }

            var allSupportFiles = Directory.GetFiles(path, baseName + ".*", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x)).ToList();
            allSupportFiles.AddRange(Directory.GetFiles(path, baseName + "_img.*", SearchOption.TopDirectoryOnly)
                .Select(x => new FileInfo(x)).ToList());

            foreach (var supportFile in allSupportFiles)
            {
                var newName = Path.Combine(supportFile.DirectoryName,
                    supportFile.Name.Replace(baseName, shortName));

                if (Verbose)
                {
                    Log.Add($"renamed {supportFile.Name} : {Path.GetFileName(newName)}");
                }

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
            var allFiles = Directory.GetFiles(directory,"*",SearchOption.TopDirectoryOnly).Select(x => new FileInfo(x)).ToList();
            var allNonMDFiles = allFiles.Except(allMdFiles);
            if (Verbose)
            {
                //do a scan for support files which do not share the base name.
                foreach (var nonMDFile in allNonMDFiles)
                {
                    foreach (var mdfile in allMdFiles)
                    {
                        //md file contains a reference to the support file, but the names are not the same.
                        if (HttpUtility.UrlDecode(File.ReadAllText(mdfile.FullName)).Contains(nonMDFile.Name) &&
                            !Path.GetFileNameWithoutExtension(nonMDFile.Name)
                                .Contains(Path.GetFileNameWithoutExtension(mdfile.Name)))
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine(
                                $"{mdfile.Name} references {nonMDFile.Name}, but {nonMDFile} will not be renamed as it does not contain the same basename as the md file." +
                                " Manually rename this file and the reference. ");
                            Console.ResetColor();
                        }
                    }
                }
            }

            foreach (var mdFile in allMdFiles)
            {
                if (mdFile.Name.Length > maxLength)
                {
                    var baseName = Path.GetFileNameWithoutExtension(mdFile.Name);
                    var shortName = Dynamo.Utilities.Hash.GetHashFilenameFromString(baseName);
                    RenameFile(mdFile.FullName, baseName, shortName);
                }
            }

            if (Verbose)
            {
                var fileRenamed = false;
                //log to console any support files in the directory that were not renamed.
                foreach (var file in allFiles)
                {
                    fileRenamed = false;
                    foreach (var entry in Log)
                    {
                        if(entry.Contains(file.Name))
                        {
                            fileRenamed = true;
                            break;
                        }
                    }

                    if (!fileRenamed)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{file.Name} was not found in the rename log");
                    }
                }

                var outputpath = Path.Combine(directory, "rename_log.txt");
                var logString = String.Join(Environment.NewLine, Log);
                if (File.Exists(outputpath))
                {
                    File.AppendAllText(outputpath,logString);
                }
                File.WriteAllText(outputpath,logString);
            }
        }
    }
}
