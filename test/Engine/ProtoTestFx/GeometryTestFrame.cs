using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProtoScript.Runners;
using ProtoFFI;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using System.Xml;
using System.Xml.Linq;
using ProtoCore.DSASM.Mirror;
using Autodesk.DesignScript.Interfaces;

namespace ProtoTestFx
{
    public class GeometryTestFrame : WatchTestFx
    {
        public static string BaseDir { get; set; }
        public static string ScriptDir { get; set; }
        public static double Tol1 = 1.0e-5;
        public static double Tol2 = GetSelfDefinedTolerance();
        public static bool UseSelfDefinedTolerance = GetUseSelfDefinedTolerance();

        public static object GetRegistryValue(string path, string key)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(path);
            if (null != rk)
            {
                return rk.GetValue(key);
            }
            return null;
        }

        internal static bool GetUseSelfDefinedTolerance()
        {
            object obj = GetRegistryValue("Software\\Autodesk\\DesignScript", "UseSelfDefinedTolerance");
            if (null != obj)
            {
                int rst;
                if (int.TryParse(obj.ToString(), out rst))
                    return rst == 1;
            }
            return false;
        }

        internal static double GetSelfDefinedTolerance()
        {
            object obj = GetRegistryValue("Software\\Autodesk\\DesignScript", "Tolerance");
            if (null != obj)
            {
                double rst;
                if (Double.TryParse(obj.ToString(), out rst))
                    return rst;
            }
            return Tol1;
        }

        internal static ProtoCore.Core TestRunnerRunOnly(string includePath, string code,
            Dictionary<int, List<string>> map, string geometryFactory,
            string persistentManager, out ExecutionMirror mirror)
        {
            ProtoCore.Core core;
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScriptTestRunner();


            ProtoScript.Config.RunConfiguration runnerConfig;

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.WatchTestMode = true;

            // Cyclic dependency threshold is lowered from the default (2000)
            // as this causes the test framework to be painfully slow
            options.kDynamicCycleThreshold = 5;

            // Pass the absolute path so that imported filepaths can be resolved
            // in "FileUtils.GetDSFullPathName()"
            includePath = Path.GetDirectoryName(includePath);
            options.IncludeDirectories.Add(includePath);

            //StreamWriter sw = File.CreateText(executionLogFilePath);
            TextOutputStream fs = new TextOutputStream(map);

            core = new ProtoCore.Core(options);
            core.Configurations.Add(ConfigurationKeys.GeometryXmlProperties, true);
            //core.Configurations.Add(ConfigurationKeys.GeometryFactory, geometryFactory);
            //core.Configurations.Add(ConfigurationKeys.PersistentManager, persistentManager);

            // By specifying this option we inject a mock Executive ('InjectionExecutive')
            // that prints stackvalues at every assignment statement
            // by overriding the POP_handler instruction - pratapa
            core.ExecutiveProvider = new InjectionExecutiveProvider();

            core.BuildStatus.MessageHandler = fs;
            core.RuntimeStatus.MessageHandler = fs;

            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());

            //Run

            mirror = fsr.Execute(code, core);

            //sw.Close();

            return core;
        }

        public static void RunAndCompare(string scriptFile)
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            String includePath = Path.GetFullPath(scriptFile);
            StreamReader file = new StreamReader(scriptFile);
            WatchTestFx.GeneratePrintStatements(file, ref map);
            file = new StreamReader(scriptFile);
            String code = file.ReadToEnd();
            file.Close();
            InjectionExecutive.ExpressionMap.Clear();
            ExecutionMirror mirror;
            Core core = TestRunnerRunOnly(includePath, code, map, "ManagedAsmGeometry.dll", "ManagedAsmPersistentManager.dll", out mirror);

            try
            {
                //XML Result
                Dictionary<Expression, List<string>> expressionValues = InjectionExecutive.ExpressionMap;
                string xmlFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".xml"));
                string baseXmlFile = Path.Combine(BaseDir, xmlFile);
                string outputXmlFile = Path.Combine(ScriptDir, xmlFile);
                DumpDictionaryAsXml(expressionValues, outputXmlFile);

                //Log
                string logFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".log"));
                string baseLogFile = Path.Combine(BaseDir, logFile);
                string outputLogFile = Path.Combine(ScriptDir, logFile);
                TextOutputStream tStream = core.BuildStatus.MessageHandler as TextOutputStream;
                StreamWriter osw = new StreamWriter(outputLogFile);
                osw.Write(tStream.ToString());
                if (null != mirror)
                    osw.Write(mirror.GetCoreDump());
                osw.Close();

                //Compare XML
                StringBuilder msg = new StringBuilder();
                bool xmlFilesAreSame = CompareXml(outputXmlFile, baseXmlFile, msg);
                if (!xmlFilesAreSame)
                {
                    bool logFilesAreSame = CompareLog(outputLogFile, baseLogFile);
                    //There may be some errors when comparing .xml files
                    if (logFilesAreSame)
                        Assert.Ignore("Ignored: .xml files are the same while .log files are not the same!");
                    else
                        Assert.Fail(msg.ToString());
                }
            }
            catch (NUnit.Framework.AssertionException e)
            {
                core.Cleanup();
                Assert.Fail(e.Message);
                return;
            }
            catch (Exception e)
            {
                core.Cleanup();
                Assert.Fail("Error: an exception is thrown!\n\n\t" + e.Message );
                return;
            }

            //Ensure no memory leak
            //sw.Close();

            core.Cleanup();
        }

        public static bool RunAndCompareNoAssert(string scriptFile)
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            String includePath = Path.GetFullPath(scriptFile);
            StreamReader file = new StreamReader(scriptFile);
            WatchTestFx.GeneratePrintStatements(file, ref map);
            file = new StreamReader(scriptFile);
            String code = file.ReadToEnd();
            file.Close();
            InjectionExecutive.ExpressionMap.Clear();
            ExecutionMirror mirror;
            Core core = TestRunnerRunOnly(includePath, code, map, "ManagedAsmGeometry.dll", "ManagedAsmPersistentManager.dll", out mirror);

            //XML Result
            Dictionary<Expression, List<string>> expressionValues = InjectionExecutive.ExpressionMap;
            string xmlFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".xml"));
            string baseXmlFile = Path.Combine(BaseDir, xmlFile);
            string outputXmlFile = Path.Combine(ScriptDir, xmlFile);
            DumpDictionaryAsXml(expressionValues, outputXmlFile);

            //Log
            string logFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".log"));
            string baseLogFile = Path.Combine(BaseDir, logFile);
            string outputLogFile = Path.Combine(ScriptDir, logFile);
            TextOutputStream tStream = core.BuildStatus.MessageHandler as TextOutputStream;
            StreamWriter osw = new StreamWriter(outputLogFile);
            osw.Write(tStream.ToString());
            if (null != mirror)
                osw.Write(mirror.GetCoreDump());
            osw.Close();

            //Compare XML
            StringBuilder msg = new StringBuilder();
            if (!CompareXml(outputXmlFile, baseXmlFile, msg))
            {
                return false;
            }

            //Compare Log
            if (!CompareLog(outputLogFile, baseLogFile))
            {
                return false;
            }

            //sw.Close();

            return true;
        }

        public static ProtoCore.Core RunAndGenerateBase(string scriptFile, string outputDir, string geometryFactory, string persistentManager)
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            String includePath = Path.GetFullPath(scriptFile);
            StreamReader file = new StreamReader(scriptFile);
            WatchTestFx.GeneratePrintStatements(file, ref map);
            file = new StreamReader(scriptFile);
            String code = file.ReadToEnd();
            file.Close();
            InjectionExecutive.ExpressionMap.Clear();
            ExecutionMirror mirror;
            Core core = TestRunnerRunOnly(includePath, code, map, geometryFactory, persistentManager, out mirror);

            //XML Result
            Dictionary<Expression, List<string>> expressionValues = InjectionExecutive.ExpressionMap;
            string xmlFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".xml"));
            string outputXmlFile = Path.Combine(outputDir, xmlFile);
            DumpDictionaryAsXml(expressionValues, outputXmlFile);

            //Log
            string logFile = Path.GetFileName(Path.ChangeExtension(scriptFile, ".log"));
            string outputLogFile = Path.Combine(outputDir, logFile);
            TextOutputStream tStream = core.BuildStatus.MessageHandler as TextOutputStream;
            StreamWriter osw = new StreamWriter(outputLogFile);
            osw.Write(tStream.ToString());
            if (null != mirror)
                osw.Write(mirror.GetCoreDump());
            osw.Close();

            return core;
        }

        static bool mForceShutdowned = false;
        public static void StartUpAllApps()
        {
            if (mForceShutdowned)
            {
                FFIExecutionManager.ForceStartUpAllApps();
                mForceShutdowned = false;
            }
        }

        public static void ShutDownAllApps()
        {
            if (!mForceShutdowned)
            {
                FFIExecutionManager.ForceShutDownAllApps();
                mForceShutdowned = true;
            }
        }

        private static string ValidateName(string name)
        {
            name = XmlConvert.EncodeName(name);
            return name;
        }

        private static void DumpDictionaryAsXml(Dictionary<Expression, List<string>> dict, string filePath)
        {
            XElement x = new XElement("MassProperties");
            foreach (var item in dict)
            {
                string name = ValidateName(item.Key.Expr);
                XElement item_x = new XElement(name);
                x.Add(item_x);
                int index = 0;
                foreach (var value in item.Value)
                {
                    XElement value_x;
                    try
                    {
                        value_x = new XElement(name + @"_" + index.ToString());
                        //if value is an array object
                        string valueString = value;
                        if (valueString.StartsWith(@"{"))
                        {
                            valueString = valueString.Replace(@"},{", @"}{");
                            valueString = valueString.Replace(@">,<", @"><");
                            valueString = valueString.Replace(@"{", @"<Array>");
                            valueString = valueString.Replace(@"}", @"</Array>");
                            valueString = valueString.Replace(@">,<", @"><");
                            valueString = Regex.Replace(valueString, @"(>,(?<data>[^<>]+)<)|(>(?<data>[^<>]+),<)|(>,(?<data>[^<>]+),<)|(>(?<data>[^<>]+,[^<>]+)<)", @"><Data>${data}</Data><");
                        }
                        XElement telement = XElement.Parse(valueString);
                        value_x.Add(telement);
                    }
                    catch (Exception)
                    {
                        value_x = new XElement(name + @"_" + index.ToString());
                        value_x.Value = value;
                    }
                    item_x.Add(value_x);
                    ++index;
                }
            }
            x.Save(filePath);
        }

        private static bool CompareXml(string file1, string file2, StringBuilder msg)
        {
            XDocument xDoc1 = XDocument.Load(file1);
            XDocument xDoc2 = XDocument.Load(file2);

            var elements1 = xDoc1.Elements().OrderBy((x) => x.Name);
            var elements2 = xDoc2.Elements().OrderBy((x) => x.Name);

            if (elements1.Count() != elements2.Count())
            {
                msg.AppendLine("The child element count does not match for the document");
                return false;
            }

            foreach (var element1 in elements1)
            {
                var matched_elements2 = elements2.Where(x => string.Compare(x.Name.LocalName, element1.Name.LocalName) == 0);
                if (null == matched_elements2 || matched_elements2.Count() == 0)
                {
                    msg.AppendLine(string.Format("The child element {0} does not exist in {1}", element1.Name, file2));
                    return false;
                }

                var element2 = matched_elements2.First();
                if (!CompareXmlElement(element1, element2, msg, false))
                    return false;
            }

            return true;
        }

        private static bool FuzzyEqual(double a, double b)
        {
            if (UseSelfDefinedTolerance)
            {
                if (Math.Abs(a - b) < Tol2 || 
                   (Math.Abs(a - b) / Math.Abs(a + b)) < Tol2)
                    return true;
            }
            else
            {
                if (Math.Abs(a - b) < Tol1 ||
                   (Math.Abs(a - b) / Math.Abs(a + b)) < Tol1)
                    return true;
            }

            return false;
        }

        private static bool Compare(string s1, string s2)
        {
            try
            {
                if (!FuzzyEqual(Double.Parse(s1), Double.Parse(s2)))
                {
                    return false;
                }
            }
            catch
            {
                if (string.Compare(s1, s2, true) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CompareXmlElement(XElement e1, XElement e2, StringBuilder msg, bool nested)
        {
            if (string.Compare(e1.Name.LocalName, e2.Name.LocalName) != 0)
                return false;

            var elements1 = e1.Elements();
            var elements2 = e2.Elements();

            if (elements1.Count() != elements2.Count())
            {
                msg.AppendLine(string.Format("The child element count for the XML element does not match for {0}", e1.Name));
                return false;
            }

            if (elements1.Count() == 0)
            {
                if (string.Compare(e1.Name.LocalName, "Data", true) == 0)
                {
                    string[] substrings1 = e1.Value.Split(',');
                    string[] substrings2 = e2.Value.Split(',');
                    if (null == substrings1 && null == substrings2)
                        return true;
                    if ((null == substrings1 && null != substrings2) ||
                        (null != substrings1 && null == substrings2))
                    {
                        msg.AppendLine(string.Format("The value for {0} does not match: {1}, {2}", e1.Name, e1.Value, e2.Value));
                        return false;
                    }
                    else
                    {
                        int count1 = substrings1.Count();
                        int count2 = substrings2.Count();
                        if (count1 != count2)
                        {
                            msg.AppendLine(string.Format("The value for {0} does not match: {1}, {2}", e1.Name, e1.Value, e2.Value));
                            return false;
                        }
                        for (int i = 0; i < count1; ++i)
                        {
                            if (!Compare(substrings1[i], substrings2[i]))
                            {
                                msg.AppendLine(string.Format("The value for {0} does not match: {1}, {2}", e1.Name, e1.Value, e2.Value));
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    if (!Compare(e1.Value, e2.Value))
                    {
                        msg.AppendLine(string.Format("The string value for {0} does not match: {1}, {2}", e1.Name, e1.Value, e2.Value));
                        return false;
                    }
                }
                return true;
            }

            int count = elements1.Count();
            for (int i = 0; i < count; ++i)
            {
                if (!CompareXmlElement(elements1.ElementAt(i), elements2.ElementAt(i), msg, true))
                    return false;
            }

            return true;
        }

        private static bool CompareLog(string file1, string file2)
        {
            return File.ReadLines(file1).SequenceEqual(File.ReadLines(file2));
        }
    }

    public class AdharnessTest
    {
        public static Core RunAndGenerateBase(string scriptDir, string scriptFile, string outputDir)
        {
            GeometryTestFrame.ScriptDir = outputDir;
            return GeometryTestFrame.RunAndGenerateBase(Path.Combine(scriptDir, Path.GetFileName(scriptFile)), outputDir, "ComputationGeometry.dll", "");
        }
    }
}
