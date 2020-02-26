using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstallerSpec
{
    public class ModuleSpec
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public bool DigitalSignature { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
    }

    public class DynamoInstallSpec
    {
        [XmlArray]
        public List<ModuleSpec> Modules { get; set; }

        private DynamoCoreVersion DynamoVersion;

        public static DynamoInstallSpec CreateFromPath(string binPath, string corefile, IEnumerable<string> additionalFilePaths)
        {
            var spec = new DynamoInstallSpec()
            {
                DynamoVersion = DynamoCoreVersion.FromPath(binPath, corefile)
            };

            var dir = new DirectoryInfo(spec.DynamoVersion.BaseDirectory);
            spec.Modules = spec.GetModules(dir).ToList();
            spec.Modules.AddRange(spec.GetModules(additionalFilePaths));
            return spec;
        }

        public static DynamoInstallSpec CreateFromSpecFile(string filePath)
        {
            if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(DynamoInstallSpec), new[] { typeof(ModuleSpec) });
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return serializer.Deserialize(fs) as DynamoInstallSpec;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(string.Format("Failed to Create DynamoInstallSpec from file {0}", filePath));
                Console.Write(ex.Message);
            }

            return null;
        }

        public void Save(string filePath)
        {
            var serializer = new XmlSerializer(typeof(DynamoInstallSpec), new[] { typeof(ModuleSpec) });
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, this);
            }
        }

        private IEnumerable<ModuleSpec> GetModules(DirectoryInfo dir)
        {
            foreach (var d in dir.EnumerateDirectories())
            {
                var modules = GetModules(d);
                foreach (var m in modules)
                {
                    yield return m;
                }
            }

            foreach (FileInfo item in dir.EnumerateFiles())
            {
                yield return ModuleSpecFromFileInfo(item, false);
            }
        }

        private IEnumerable<ModuleSpec> GetModules(IEnumerable<string> additionalFilePaths)
        {
            foreach(var filePath in additionalFilePaths)
            {
                if (!File.Exists(filePath))
                {
                    throw new System.IO.FileNotFoundException($"could not find file to sign at: {filePath} while generating binariestosign.txt");
                }
                yield return ModuleSpecFromFileInfo(new FileInfo(filePath), true);
            }
        }

        private ModuleSpec ModuleSpecFromFileInfo(FileInfo fileInfo, bool forceSignature)
        {
            var info = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
            return new ModuleSpec()
            {
                Name = fileInfo.Name,
                FilePath = fileInfo.FullName.Replace(DynamoVersion.BaseDirectory, @".\"),
                Version = info != null ? info.FileVersion : "",
                DigitalSignature = forceSignature ? true : NeedSignature(info),
                Author = info != null ? info.CompanyName : ""
            };
        }

        private bool NeedSignature(FileVersionInfo info)
        {
            if(info != null && info.FileVersion != null)
            {
                return info.FileVersion.StartsWith(DynamoVersion.BaseVersion);
            }

            return false;
        }
    }

    class DynamoCoreVersion
    {
        public string BaseVersion { get; private set; }
        public string BaseDirectory { get; private set; }
        
        public static DynamoCoreVersion FromPath(string binpath, string corefile)
        {
            string path = binpath;
            if (Directory.Exists(binpath))//binpath is a directory
            {
                if (string.IsNullOrEmpty(corefile))
                {
                    corefile = "DynamoCore.dll";
                }

                path = Directory.GetFiles(binpath, corefile, SearchOption.AllDirectories).FirstOrDefault();
            }

            if (!File.Exists(path)) return null; //File is not available

            if (!binpath.EndsWith(@"\"))
                binpath += @"\";
            
            var info = FileVersionInfo.GetVersionInfo(path);
            return new DynamoCoreVersion()
            {
                BaseDirectory = binpath,
                //Drop last two characters from the version string to consider version diff
                //in localized resources and any other binaries coming from different repo.
                BaseVersion = info.FileVersion.Substring(0, info.FileVersion.Length - 2), 
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int nArgs = args.Length;
            if(nArgs == 0)
            {
                Console.WriteLine("============== How to use this tool? ===========");
                Console.Write(@"
Generates installer spec XML file to be used to create installer and digital 
signature based on a given folder.

InstallerSpec.exe binfolder [xmlfilepath] [corefile] [/f:files.txt]

  binfolder     The folder in which all binaries reside. The search will 
                enumerate this folder as well as all sub-folders.

  xmlfilepath   The path to the output XML file path

  corefile      The core file name such as DynamoCore or DynamoRevitDS etc. 
                to detect the version of the installer.

  /f:files.txt  An optional text file containing paths to additional binary 
                files that need to be signed. Each file in this text file 
                should be on one line.
");
                
                return;
            }
            var binpath = args[0];
            if(!Directory.Exists(binpath))
            {
                Console.WriteLine("===================== Error =====================");
                Console.WriteLine("Wrong bin folder path");
                return;
            }
            var filePath = @"InstallSpec.xml";
            if(nArgs > 1)
            {
                filePath = args[1];
            }

            var corefile = string.Empty;
            var fileList = new List<string>();

            var index = 2;
            while (index < nArgs)
            {
                var value = args[index++];
                if (value.StartsWith("/f:"))
                {
                    var textFilePath = value.Substring(3);
                    if (File.Exists(textFilePath))
                    {
                        try
                        {
                            using (var streamReader = File.OpenText(textFilePath))
                            {
                                while (!streamReader.EndOfStream)
                                {

                                    // WARNING: The following codes are written with the assumption that 
                                    // files with listed names are residing under "binpath", but this may 
                                    // not be true in the future. Rewrite this logic and make "textFilePath"
                                    // contains fully qualified paths instead of doing the prefix here.
                                    // 
                                    var fileNameWithoutPath = streamReader.ReadLine().Trim();
                                    //this is a comment, ignore this line
                                    if (fileNameWithoutPath.StartsWith("##") || string.IsNullOrWhiteSpace(fileNameWithoutPath))
                                    {
                                        continue;
                                    }
                                    var fileNameWithPath = Path.Combine(binpath, fileNameWithoutPath);
                                    if (!string.IsNullOrEmpty(fileNameWithPath))
                                    {
                                        Console.WriteLine($"adding {fileNameWithPath} to filesToSign list while parsing {textFilePath}");
                                        fileList.Add(fileNameWithPath);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException($"could not find {textFilePath} but it was passed as an argument to installerSpec.exe");
                    }
                }
                else
                {
                    corefile = value; // Without "/f" flag, it's a file name.
                }
            }

            var installspec = DynamoInstallSpec.CreateFromPath(binpath, corefile, fileList);
            installspec.Save(filePath);

            var binariestosigntxt = Path.Combine(Path.GetDirectoryName(filePath), @"binariestosign.txt");
            using (var writer = new StreamWriter(binariestosigntxt))
            {
                foreach (var item in installspec.Modules)
                {
                    if(item.DigitalSignature)
                    {
                        writer.WriteLine(Path.GetFullPath(Path.Combine(binpath, item.FilePath)));
                    }
                }
                writer.Flush();
            }
        }

    }
}
