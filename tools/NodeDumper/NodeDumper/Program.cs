using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NodeDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            //args should be 1
            if (args.Count() != 1)
            {
                Console.WriteLine("You must specify a directory to query for Dynamo nodes.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location);
            //find if the directory specified exists
            var nodesDir = Path.Combine(assDir.Directory.FullName, args[0]);
            Console.WriteLine(nodesDir);
            if (!Directory.Exists(nodesDir))
            {
                Console.WriteLine("The specified directory does not exist.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            var outPath = Path.Combine(nodesDir, "NodeDump.csv");

            try
            {
                using (var tw = new StreamWriter(outPath))
                {
                    tw.WriteLine("Type Name, Name, Description, Category");

                    var nodeDirInfo = new DirectoryInfo(nodesDir);
                    var nodeLibs = nodeDirInfo.GetFiles("*.dll");

                    foreach (var nodeLib in nodeLibs)
                    {
                        var module = ModuleDefinition.ReadModule(nodeLib.FullName);
                        var cecilNodes = module.Types.Where(x => x.Namespace == "Dynamo.Nodes");
                        var typeDefinitions = cecilNodes as TypeDefinition[] ?? cecilNodes.ToArray();
                        if (!typeDefinitions.Any())
                            continue;

                        foreach (var nn in typeDefinitions)
                        {
                            if(nn.IsAbstract)
                                continue;

                            if (!nn.HasCustomAttributes)
                                continue;

                            var name = "";
                            var descrip = "";
                            var cat = "";

                            bool isCompilerGenerated = false;

                            foreach (var ca in nn.CustomAttributes)
                            {
                                if (ca.AttributeType.Name == "NodeNameAttribute")
                                {
                                    name = ca.ConstructorArguments[0].Value.ToString();
                                }
                                else if (ca.AttributeType.Name == "NodeDescriptionAttribute")
                                {
                                    descrip = ca.ConstructorArguments[0].Value.ToString();
                                }
                                else if (ca.AttributeType.Name == "NodeCategoryAttribute")
                                {
                                    cat = ca.ConstructorArguments[0].Value.ToString();
                                }
                                else if (ca.AttributeType.Name == "CompilerGeneratedAttribute")
                                {
                                    isCompilerGenerated = true;
                                }
                            }

                            if (isCompilerGenerated)
                                continue;

                            string line = string.Format("{0},{1},{2},{3}", nn.ToString(), name, descrip.Replace(",", " "), cat);

                            //find the port data
                            var constructors = nn.Methods.Where(x => x.IsConstructor);
                            foreach (var constructor in constructors)
                            {
                                if (!constructor.HasBody)
                                    continue;

                                int instructionCount = 0;
                                foreach (var instruction in constructor.Body.Instructions)
                                {
                                    if (instruction.OpCode == OpCodes.Newobj)
                                    {
                                        var methodRef = (MethodReference)instruction.Operand;
                                        if (methodRef.DeclaringType.Name == "PortData")
                                        {
                                            var nickname = "";
                                            var description = "";
                                            if (constructor.Body.Instructions[instructionCount - 4].Operand != null)
                                                description = constructor.Body.Instructions[instructionCount - 4].Operand.ToString();
                                            if (constructor.Body.Instructions[instructionCount - 5].Operand != null)
                                                nickname = constructor.Body.Instructions[instructionCount - 5].Operand.ToString();
                                            line += string.Format(",{0}:{1}", nickname, description);
                                        }
                                    }
                                    instructionCount++;
                                }
                            }

                            tw.WriteLine(line);
                        }

                        #region using reflection
                        //var nodeAss = Assembly.ReflectionOnlyLoadFrom(nodeLib.FullName);
                        //var nodes = nodeAss.GetTypes().Where(x => x.Namespace == "Dynamo.Nodes");
                        
                        //foreach (var n in nodes)
                        //{
                        //    if (n.IsAbstract)
                        //        continue;

                        //    var attribDatas = n.GetCustomAttributesData();
                        //    if (attribDatas.Count == 0)
                        //        continue;

                        //    string name = "";
                        //    string descrip = "";
                        //    string cat = "";

                        //    bool isCompilerGenerated = false;

                        //    foreach (CustomAttributeData attribData in attribDatas)
                        //    {
                        //        if (attribData.Constructor.ReflectedType.Name == "NodeNameAttribute")
                        //        {
                        //            name = attribData.ConstructorArguments[0].Value.ToString();
                        //        }
                        //        else if (attribData.Constructor.ReflectedType.Name == "NodeDescriptionAttribute")
                        //        {
                        //            descrip = attribData.ConstructorArguments[0].Value.ToString();
                        //        }
                        //        else if (attribData.Constructor.ReflectedType.Name == "NodeCategoryAttribute")
                        //        {
                        //            cat = attribData.ConstructorArguments[0].Value.ToString();
                        //        }
                        //        else if (attribData.Constructor.ReflectedType.Name == "CompilerGeneratedAttribute")
                        //        {
                        //            isCompilerGenerated = true;
                        //        }
                        //    }

                        //    if (isCompilerGenerated)
                        //        continue;

                        //    string line = string.Format("{0},{1},{2},{3}", n.ToString(), name, descrip.Replace(",", " "), cat);

                        //    tw.WriteLine(line);
                        //} 
                        #endregion
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        var exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                var errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
                return;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //http://msdn.microsoft.com/en-us/library/system.appdomain.reflectiononlyassemblyresolve.aspx

            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }

    
}
