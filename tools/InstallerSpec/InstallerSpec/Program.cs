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
        public bool CopyForInstaller { get; set; }
        public string Author { get; set; }
    }

    public class DynamoInstallSpec
    {
        [XmlArray]
        public List<ModuleSpec> Modules { get; set; }

        public static DynamoInstallSpec CreateFromPath(string binPath)
        {
            return new DynamoInstallSpec() { Modules = GetModules(new DirectoryInfo(binPath)).ToList() };
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

        public static IEnumerable<ModuleSpec> GetModules(DirectoryInfo dir)
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
                var info = FileVersionInfo.GetVersionInfo(item.FullName);
                yield return new ModuleSpec() { Name = item.Name, FilePath = item.FullName.Replace(DynamoCoreVersion.BaseDirectory, @".\"), CopyForInstaller = true, DigitalSignature = Sign(info), Author = info != null ? info.CompanyName : "" };
            }
        }

        private static bool Sign(FileVersionInfo info)
        {
            if(info != null && info.FileVersion != null)
            {
                return info.FileVersion.StartsWith(DynamoCoreVersion.BaseVersion);
            }

            return false;
        }
    }

    class DynamoCoreVersion
    {
        public static string BaseVersion { get; private set; }
        public static string BaseDirectory { get; private set; }
        
        public static void InitVersion(string binpath)
        {
            BaseDirectory = binpath;
            if (!BaseDirectory.EndsWith(@"\"))
                BaseDirectory += @"\";
            string path = Path.Combine(binpath, "DynamoCore.dll");
            if (!File.Exists(path))
                return; //File is not available

            var info = FileVersionInfo.GetVersionInfo(path);
            BaseVersion = info.FileVersion.Substring(0, info.FileVersion.Length - 2); //drop last two characters
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
                Console.WriteLine("Generates installer spec xml file to be used to create installer and digital signature based on a bin folder.");
                Console.WriteLine("InstallerSpec.exe [bin folder] [xml file]");
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

            DynamoCoreVersion.InitVersion(binpath);
            var installspec = DynamoInstallSpec.CreateFromPath(binpath);
            installspec.Save(filePath);
        }

    }
}
