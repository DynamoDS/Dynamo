using CommandLine;
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

    class Options
    {
        [Option('i', "InputAssembly", Required = true, HelpText = "The assembly to perorm operations on.")]
        public string InputAssembly { get; set; }
        [Option('o', "OutputPath", Required = true, HelpText = "Output path where generated assembly will be written to.")]
        public string OutputPath { get; set; }

        [Option('t', "TextToReplace", Required = true, HelpText = "A | sepereated list of string values to search of which will be replaced by strings set by -ReplacementText.")]
        public string TextToReplace {get;set;}
        [Option('r', "ReplacementText", Required = true, HelpText = "A | sepereated list of string values used as replacement text - length after parsing must match the length of -TextToReplace.")]
        public string ReplacementText { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                     .WithParsed<Options>(o =>
                     {
                         var executingPath = Assembly.GetExecutingAssembly().Location;
                         var ad = AssemblyDefinition.ReadAssembly(o.InputAssembly);
                         var replacementData = ParseReplaceText(o.TextToReplace, o.ReplacementText);
                         Console.WriteLine("starting name replacement");
                         foreach(var data in replacementData)
                         {
                             ReplaceNamesInAssembly(ad, data.Item1,data.Item2);
                         }
                        

                         //remove strong name to avoid this modified assembly somehow having the same id of the original.
                         ad.Name.HasPublicKey = false;
                         ad.Name.PublicKey = new byte[0];
                         ad.MainModule.Attributes &= ~ModuleAttributes.StrongNameSigned;

                         ad.Write(Path.Combine(o.OutputPath, ad.Name.Name + ".dll"));
                     });

        }

        private static void ReplaceNamesInAssembly(AssemblyDefinition ad, string textToReplace, string replacementText)
        {
            ad.Name.Name = ad.Name.Name.Replace(textToReplace, replacementText);

            foreach(var type in ad.MainModule.Types)
            {
                var originalname = type.Namespace;
                type.Namespace = type.Namespace.Replace(textToReplace, replacementText);
                if(originalname != type.Namespace)
                {
                    Console.WriteLine($"replaced {originalname} with {type.Namespace} ");
                }
            }

            foreach (var resource in ad.MainModule.Resources)
            {
               if(resource.ResourceType == ResourceType.Embedded)
                {
                    var originalname = resource.Name;
                    resource.Name = resource.Name.Replace(textToReplace, replacementText);
                    if(originalname != resource.Name)
                    {
                        Console.WriteLine($"replaced {originalname} with {resource.Name} ");
                    }
                }
            }
        }

        private static List<Tuple<string,string>> ParseReplaceText(string textToReplace,string replacementText)
        {
            var textCollection = textToReplace.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var replacementCollection = replacementText.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if(textCollection.Length != replacementCollection.Length)
            {
                throw new ArgumentOutOfRangeException("replacement text and text to replace must contain same number of strings");
            }
            return textCollection.Select((x, i) => { return Tuple.Create<string, string>(x, replacementCollection[i]); }).ToList();
        }

        
    }
}
