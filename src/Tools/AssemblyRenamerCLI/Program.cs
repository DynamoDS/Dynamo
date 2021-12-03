using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyRenamerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var executingPath = Assembly.GetExecutingAssembly().Location;
            var xamlBehaviorsPath = Path.Combine(Path.GetDirectoryName(executingPath), "Microsoft.Xaml.Behaviors.dll");

            var externPath = Path.Combine(new DirectoryInfo(Path.GetDirectoryName(executingPath)).Parent.Parent.Parent.Parent.Parent.FullName,
                "extern", 
                "Microsoft.Xaml.Behaviors");

            var ad = AssemblyDefinition.ReadAssembly(xamlBehaviorsPath);
            WrapNameSpace(ad, "Dynamo.");

            //remove strong name to avoid this modified assembly somehow having the same id of the original.
            ad.Name.HasPublicKey = false;
            ad.Name.PublicKey = new byte[0];
            ad.MainModule.Attributes &= ~ModuleAttributes.StrongNameSigned;

            ad.Write(Path.Combine(externPath, ad.Name.Name+".dll"));
        }

        private static void WrapNameSpace(AssemblyDefinition ad, string namespaceToAdd)
        {
            ad.Name.Name = namespaceToAdd + ad.Name.Name;
            foreach(var type in ad.MainModule.Types)
            {
                type.Namespace = namespaceToAdd + type.Namespace;
            }
        }

        
    }
}
