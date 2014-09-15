using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace NUnitCI
{
    class Program
    {
        static void Main(string[] args)
        {

            var body = new StringBuilder();
            //curPath and prePath are the current and previous paths of either directory or files
            try
            {
                string curPath = @args[0];
                string prePath = @args[1];
                string outPath = @args[2];

                Console.WriteLine(curPath);
                Console.WriteLine(prePath);
                AppendNunitSummary(curPath, prePath, ref body);
                AppendNuintDiff(curPath, prePath, ref body);
                using (StreamWriter outfile = new StreamWriter(outPath))
                {
                    outfile.Write(body.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Less than 3 arguments passed in");
            }
        
        }

        public static void GetNunitRegressionInfo(string curXml, string preXml, out int PositiveRegression, out int NegativeRegression, out int newTestCase)
        {
            PositiveRegression = 0;
            NegativeRegression = 0;
            newTestCase = 0;
            Console.WriteLine("nunit regression info started");
            if (preXml == null || curXml == null || !File.Exists(preXml) || !File.Exists(curXml))
                return;
            try
            {
                NuintTestResult preTestResult = new NuintTestResult(0, preXml);
                NuintTestResult curTestResult = new NuintTestResult(0, curXml);

                int ix1 = 0;
                int ix2 = 0;
                while (ix1 < preTestResult.results.Count && ix2 < curTestResult.results.Count)
                {
                    int flag = string.Compare(preTestResult.results[ix1].Name, curTestResult.results[ix2].Name);
                    if (flag == 0)
                    {
                        if (preTestResult.results[ix1].Result == NuintTestCaseResult.ResultType.SUCCESS &&
                            (curTestResult.results[ix2].Result == NuintTestCaseResult.ResultType.ERROR ||
                            curTestResult.results[ix2].Result == NuintTestCaseResult.ResultType.FAILURE))
                            ++PositiveRegression;
                        else if ((preTestResult.results[ix1].Result == NuintTestCaseResult.ResultType.FAILURE ||
                            curTestResult.results[ix2].Result == NuintTestCaseResult.ResultType.ERROR) &&
                            curTestResult.results[ix2].Result == NuintTestCaseResult.ResultType.SUCCESS)
                            --NegativeRegression;

                        ++ix1;
                        ++ix2;
                    }
                    if (flag > 0)
                    {
                        ++ix2;
                    }
                    else
                    {
                        ++ix1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                PositiveRegression = 0;
                NegativeRegression = 0;
            }
            Console.WriteLine("nunit regression info done");
        }
        /// <summary>
        /// this function is to build the summary dictionary of the test case results
        /// </summary>
        /// <param name="path">path is the path of folder or files</param>
        /// <returns></returns>
        static Dictionary<string, int> GetResultSummaryForFile(string path)
        {
            Dictionary<string, int> summary = new Dictionary<string, int>();
            string[] keyValues = { "total", "errors", "not-run", "inconclusive", "ignored", "skipped", "invalid", "failures" };
            if (Directory.Exists(path))
            {
                string[] filePaths = Directory.GetFiles(path);
                foreach (string filePath in filePaths)
                {
                    Console.WriteLine(File.Exists(filePath));
                    Console.WriteLine(filePath);
                    if (filePath == null || !File.Exists(filePath))
                    {
                        return null;
                    }

                    buildSummary(summary, keyValues, filePath);
                }

                return summary;
            }
            else
            {
                Console.WriteLine(File.Exists(path));
                Console.WriteLine(path);
                if (path == null || !File.Exists(path))
                {
                    return null;
                }
                buildSummary(summary, keyValues, path);
                return summary;
            }
        }

        private static void buildSummary(Dictionary<string, int> summary, string[] keyValues, string filePath)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                XmlNode rootNode = null;

                foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
                {
                    if (xmlNode.Name == "test-results")
                    {
                        rootNode = xmlNode;
                        break;
                    }
                }
                if (summary.ContainsKey(keyValues[0]))
                {
                    summary["total"] -= summary["not-run"];
                    for (int i = 0; i < 8; i++)
                    {
                        summary[keyValues[i]] = summary[keyValues[i]] + Convert.ToInt32(rootNode.Attributes[keyValues[i]].Value);
                    }
                    summary["total"] += summary["not-run"];
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        summary.Add(keyValues[i], Convert.ToInt32(rootNode.Attributes[keyValues[i]].Value));
                    }
                    summary["total"] += summary["not-run"];
                }
            }

            catch (Exception ex)
            {
                summary.Clear();
            }
        }
        /// <summary>
        /// this fuction is to append the summary to the body of the comparison report
        /// </summary>
        /// <param name="curPath">curPath and prePath are the paths of folder or xml files</param>
        /// <param name="prePath"></param>
        /// <param name="body"></param>
        public static void AppendNunitSummary(string curPath, string prePath, ref StringBuilder body)
        {          
            if (body == null)
                body = new StringBuilder();

            Dictionary<string, int> currentSummary = null;
            Dictionary<string, int> previousSummary = null;

            currentSummary = GetResultSummaryForFile(curPath);
            Console.WriteLine(curPath);
            previousSummary = GetResultSummaryForFile(prePath);
            Console.WriteLine(prePath);

            body.AppendLine("<table border=\"1\">");
            body.Append("<tr><td bgcolor =\"grey\">Item</td><td bgcolor=\"grey\">Current Run</td>");
            body.Append("<td bgcolor= \"grey\">Previous Run</td>");
            body.Append("<td bgcolor=\"grey\">Diff</td>");
            body.AppendLine("</tr>");
            Console.WriteLine("nunit summary started");
            Console.WriteLine(currentSummary.Count());
            for (int ix = 0; ix < currentSummary.Count(); ++ix)
            {
                body.Append("<tr>");
                body.Append("<th>" + currentSummary.ElementAt(ix).Key + "</th><td>" + currentSummary.ElementAt(ix).Value + "</td>");
                if (previousSummary != null && previousSummary.ContainsKey(currentSummary.ElementAt(ix).Key))
                {
                    int preData = previousSummary[currentSummary.ElementAt(ix).Key];
                    int curData = currentSummary.ElementAt(ix).Value;
                    int difference = curData - preData;

                    if (ix != 0 && difference != 0)
                    {
                        body.Append("<td>" + preData + "</td><td>" +
                            ((difference > 0) ? ("<font color=\"red\">+" + difference.ToString() + "</font>") : ("<font color=\"green\">" + difference.ToString() + "</font>")) +
                            "</td>");
                    }
                    else
                    {
                        body.Append("<td>" + preData + "</td><td>" + ((difference > 0) ? "+" : "") + difference.ToString() + "</td>");
                    }
                }
                else
                    body.Append("<td>N.A.</td><td>N.A.</td>");

                body.AppendLine("</tr>");
            }
            Console.WriteLine("nunit summary done");
            body.AppendLine("</table>");
            //}
        }
        public struct NunitDiffResult
        {
            public NunitDiffResult(NuintTestCaseResult _result1, NuintTestCaseResult _result2)
            {
                result1 = _result1;
                result2 = _result2;
            }

            public NuintTestCaseResult result1;
            public NuintTestCaseResult result2;
        }

        /// <summary>
        /// this function is to append the difference between xml files to the report
        /// </summary>
        /// <param name="curPath"></param>
        /// <param name="prePath"></param>
        /// <param name="body"></param>
        public static void AppendNuintDiff(string curPath, string prePath, ref StringBuilder body)
        {
            int successToFailure = 0, failureToSuccess = 0;
            if (body == null)
                body = new StringBuilder();
            if (prePath == null || curPath == null)
                return;

            NuintTestResult pre = new NuintTestResult(0, prePath);
            NuintTestResult cur = new NuintTestResult(0, curPath);

            if (pre.results == null || cur.results == null)
                return;
            else if (pre.results.Count == 0 || cur.results.Count == 0)
                return;

            List<NunitDiffResult> diff = NuintDiff(pre, cur);
            Console.WriteLine(diff.Count);
            if (diff.Count == 0)
            {
                Console.WriteLine("No Change in tests");
                body.AppendLine("<h4>" + "No Change in tests" + "<h4>");

                return;
            }
            Console.WriteLine("starting creation of report");
            foreach (NunitDiffResult result in diff)
            {
                if (result.result1 != null && result.result2 != null)
                {
                    if (result.result1.Result == NuintTestCaseResult.ResultType.SUCCESS && (result.result2.Result == NuintTestCaseResult.ResultType.FAILURE || result.result2.Result == NuintTestCaseResult.ResultType.ERROR))
                        successToFailure++;
                    if ((result.result1.Result == NuintTestCaseResult.ResultType.FAILURE || result.result1.Result == NuintTestCaseResult.ResultType.ERROR) && result.result2.Result == NuintTestCaseResult.ResultType.SUCCESS)
                        failureToSuccess++;
                }
            }
            body.AppendLine("<h2>" +"<b><u>Regression Summary</u></b>"+ ": Success and Error to Failure: " + "<font color=\"red\"> <b>" + successToFailure + "</b> <font color=\"black\">" + " Failure and Error to Success: " + "<font color=\"green\"> <b>" + failureToSuccess + "</b> </h2>");
            body.AppendLine("<table border=\"1\">");
            body.AppendLine("<tr><td  bgcolor=\"grey\">TestCase Name</td><td bgcolor=\"grey\">Directory</td><td bgcolor=\"grey\">Previous</td><td bgcolor=\"grey\">Current</td></tr>");

            foreach (NunitDiffResult result in diff)
            {
                body.Append("<tr><td align=\"left\">");
                string testCaseName = ((result.result1 == null) ? result.result2.Name : result.result1.Name);


                body.Append("<font color=\"blue\">" + testCaseName);
                if (result.result1 != null)
                    body.Append("<td align=\"left\"> " + result.result1.FileName + "</td>");
                else
                    body.Append("<td align=\"left\"> " + result.result2.FileName + "</td>");
                body.AppendLine("</td><td>");

                if (result.result1 == null)
                {
                    body.Append("<font color=\"green\">Newly Added</font></td><td>");
                }
                else
                {
                    if (result.result1.Executed)
                    {
                        body.Append("<font color=\"green\">-Executed</font>");
                    }
                    else
                    {
                        body.Append("<font color=\"red\">-Not Executed</font>");
                    }
                    if (result.result1.Result == NuintTestCaseResult.ResultType.SUCCESS)
                    {
                        body.Append("<font color=\"green\">-" + result.result1.Result.ToString() + "</font></td><td>");
                    }
                    else
                    {
                        body.Append("<font color=\"red\">-" + result.result1.Result.ToString() + "</font></td><td>");
                    }
                }

                if (result.result2 == null)
                {
                    body.Append("<font color=\"red\">Deleted</font></td>");
                }
                else
                {
                    if (result.result2.Executed)
                    {
                        body.Append("<font color=\"green\">-Executed</font>");
                    }
                    else
                    {
                        body.Append("<font color=\"red\">-Not Executed</font>");
                    }
                    if (result.result2.Result == NuintTestCaseResult.ResultType.SUCCESS)
                    {
                        body.Append("<font color=\"green\">-" + result.result2.Result.ToString() + "</font></td>");
                    }
                    else
                    {
                        body.Append("<font color=\"red\">-" + result.result2.Result.ToString() + "</font></td>");
                    }
                }
            }
            body.AppendLine("</table>");
            Console.WriteLine("report creation over ");
        }
        public static string GetFullPathByName(string name, string root)
        {
            if (!Directory.Exists(root))
                return "";

            if (File.Exists(root + "\\" + name))
                return root + "\\" + name;
            else
            {
                string path = "";
                string[] dirs = new string[] { };

                try
                {
                    dirs = Directory.GetDirectories(root);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                foreach (string dir in dirs)
                {
                    path = GetFullPathByName(name, dir);
                    if (path != "")
                        break;
                }
                return path;
            }

        }
        /// <summary>
        /// this function is to do the comparison
        /// </summary>
        /// <param name="result1">result1 is from previous xml </param>
        /// <param name="result2">result2 is from current xml </param>
        /// <returns></returns>
        public static List<NunitDiffResult> NuintDiff(NuintTestResult result1, NuintTestResult result2)
        {
            if (result1 == null || result2 == null)
                throw new ArgumentNullException("some arguments are null");

            List<NunitDiffResult> diff = new List<NunitDiffResult>();
            int ix1 = 0;
            int ix2 = 0;

            while (ix1 < result1.results.Count() && ix2 < result2.results.Count())
            {
                if (result1.results[ix1].Name == result2.results[ix2].Name)
                {
                    if (result1.results[ix1].Executed != result2.results[ix2].Executed || result1.results[ix1].Result != result2.results[ix2].Result)
                        diff.Add(new NunitDiffResult(result1.results[ix1], result2.results[ix2]));
                    ++ix1;
                    ++ix2;
                }
                else
                {
                    if (string.Compare(result1.results[ix1].Name, result2.results[ix2].Name) > 0)
                    {
                        diff.Add(new NunitDiffResult(null, result2.results[ix2]));
                        ++ix2;
                    }
                    else
                    {
                        diff.Add(new NunitDiffResult(result1.results[ix1], null));
                        ++ix1;
                    }
                }
            }

            if (ix1 != result1.results.Count())
            {
                for (; ix1 < result1.results.Count(); ++ix1)
                    diff.Add(new NunitDiffResult(result1.results[ix1], null));
            }
            else
            {
                for (; ix2 < result2.results.Count(); ++ix2)
                    diff.Add(new NunitDiffResult(null, result2.results[ix2]));
            }

            return diff;
        }

    }

    public class NuintTestResult
    {
        public NuintTestResult(int _revision, string path)
        {
            Revision = _revision;
            if (Directory.Exists(path))
            {
                string[] filePaths = Directory.GetFiles(path);
                results = new List<NuintTestCaseResult>();
                foreach (var filePath in filePaths)
                {
                    string fileName;
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (filePath == null || !File.Exists(filePath))
                        return;

                    IEnumerable<XElement> testCases = null;
                    try
                    {
                        XDocument document = XDocument.Load(filePath);
                        testCases = document.Descendants("test-case");
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    checkTestCaseAttribute(filePath, fileName, testCases);
                }
            }
            else
            {
                results = new List<NuintTestCaseResult>();
                if (path == null || !File.Exists(path))
                    return;
                string fileName = Path.GetFileName(path);
                IEnumerable<XElement> testCases = null;
                try
                {
                    XDocument document = XDocument.Load(path);
                    testCases = document.Descendants("test-case");
                }
                catch (Exception ex)
                {
                    return;
                }

                checkTestCaseAttribute(path, fileName, testCases);
            }

            results = results.OrderBy(x => x.FileName).ToList();
        }

        private void checkTestCaseAttribute(string filePath, string fileName, IEnumerable<XElement> testCases)
        {
            foreach (XElement testCase in testCases)
            {
                try
                {
                    string name = testCase.Attribute("name").Value;
                    bool executed = false;
                    NuintTestCaseResult.ResultType result;
                    if (testCase.Attribute("executed").Value == "True")
                        executed = true;

                    switch (testCase.Attribute("result").Value)
                    {
                        case "Success":
                            result = NuintTestCaseResult.ResultType.SUCCESS;
                            break;
                        case "Failure":
                            result = NuintTestCaseResult.ResultType.FAILURE;
                            break;
                        case "Error":
                            result = NuintTestCaseResult.ResultType.ERROR;
                            break;
                        case "Skipped":
                            result = NuintTestCaseResult.ResultType.SKIPPED;
                            break;
                        case "Ignored":
                            result = NuintTestCaseResult.ResultType.IGNORED;
                            break;
                        case "Inconclusive":
                            result = NuintTestCaseResult.ResultType.INCONCLUSIVE;
                            break;
                        case "Invalid":
                            result = NuintTestCaseResult.ResultType.INVALID;
                            break;
                        case "NotRunnable":
                            result = NuintTestCaseResult.ResultType.NOTRUNNABLE;
                            break;
                        default:
                            throw new InvalidDataException("Unknow result =" + testCase.Attribute("result").Value + "; in " + filePath);
                    }
                    results.Add(new NuintTestCaseResult(name, executed, result, fileName));
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public int Revision { get; private set; }
        public List<NuintTestCaseResult> results;
    }
    public class NuintTestCaseResult
    {
        public NuintTestCaseResult(string _name, bool _executed, ResultType _result, string _fileName)
        {
            Name = _name;
            Executed = _executed;
            Result = _result;
            FileName = _fileName;
        }

        public enum ResultType { SUCCESS, ERROR, FAILURE, IGNORED, SKIPPED, INVALID, INCONCLUSIVE, NOTRUNNABLE }

        public string FileName { get; private set; }
        public string Name { get; private set; }
        public bool Executed { get; private set; }
        public ResultType Result { get; private set; }
    }

}

