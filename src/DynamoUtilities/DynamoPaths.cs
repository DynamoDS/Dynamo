using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DynamoUtilities
{
    /// <summary>
    /// DynamoPaths stores paths to dynamo libraries and assets.
    /// </summary>
    public static class DynamoPaths
    {
        public static string Core { get; set; }
        public static string Definitions { get; set; }
        public static string Packages { get; set; }
        public static string Ui { get; set; }
        public static string Asm { get; set; }

        // All 'nodes' folders.
        public static HashSet<string> Nodes { get; set; } 

        public static void SetupDynamoPaths(string corePath)
        {
            if (Directory.Exists(corePath))
            {
                Core = corePath;
            }
            else
            {
                throw new Exception(string.Format("The specified core path: {0}, does not exist.", corePath));
            }

            Definitions = Path.Combine(Core, "definitions");
            Packages = Path.Combine(Core , "dynamo_packages");
            Asm = Path.Combine(Core, "dll");
            Ui = Path.Combine(Core , "UI");

            if (Nodes == null)
            {
                Nodes = new HashSet<string>();
            }

            // Only register the core nodes directory
            Nodes.Add(Path.Combine(Core, "nodes"));

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Core: {0}", Core));
            sb.AppendLine(string.Format("Definitions: {0}", Definitions));
            sb.AppendLine(string.Format("Packages: {0}", Packages));
            sb.AppendLine(string.Format("Ui: {0}", Asm));
            sb.AppendLine(string.Format("Asm: {0}", Ui));
            Nodes.ToList().ForEach(n=>sb.AppendLine(string.Format("Nodes: {0}", n)));
            
            Debug.WriteLine(sb);
        }
    }
}
