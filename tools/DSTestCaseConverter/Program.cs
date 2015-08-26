using DSTestCaseUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DSTestCaseConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            int numOfTestCases = GetNumberOfTestCases();
            ReplaceBasic();
        }

        private static void ReplaceBasic()
        {
            Debug.Assert(false); 
            TestCaseUtils.InitProcess();
            TestCaseUtils.ReplaceBasic();
        }

        private static int GetNumberOfTestCases()
        {
            int count = 0;
            if (File.Exists(TestCaseUtils.AssemblyFilePath))
            {
                Assembly assemblyInfo = Assembly.LoadFrom(TestCaseUtils.AssemblyFilePath);
                Type[] types = assemblyInfo.GetTypes();
                foreach (var testClass in types)
                {
                    foreach (var member in testClass.GetMethods())
                    {
                        if (TestCaseUtils.IsTestCase(member))
                        {
                            count++;
                        }
                    }
                }

                Console.WriteLine("Number of test cases : " + count);
            }
            return count;
        }

    }
}
