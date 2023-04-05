using Dynamo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CoreNodeModels.Input;
using Dynamo;
using NUnit.Framework;
using static DSPythonTests.PythonEvalTests;
using Directory = CoreNodeModels.Input.Directory;


namespace DynamoPythonTests
{
    internal class PythonModulesTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
            libraries.Add("DSCoreNodes.dll");
        }

        public IEnumerable<PythonEvaluatorDelegate> Evaluators = new List<PythonEvaluatorDelegate> {
            DSCPython.CPythonEvaluator.EvaluatePythonScript
        };

        #region Python Modules

        [Test]
        public void TestNumpyPythonModuleEncoding()
        {
            string code = @"
import sys
import clr

import numpy as np
a = np.arange(15).reshape(3, 5)
result = []
for list in a:
    sub_result = []
    for b in list:
        # Cast to int, otherwise getting DynamoCPythonHandle 
        sub_result.append(int(b))
    result.append(sub_result)
OUT = result
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { 0, 1, 2, 3, 4 }, new ArrayList { 5, 6, 7, 8, 9 }, new ArrayList { 10, 11, 12, 13, 14 } };

            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }


        [Test]
        public void TestPandasPythonModuleEncoding()
        {
            string code = @"
import sys
import clr

import numpy as np
import pandas as pd
dates = pd.date_range(""20130101"", periods=6)
a = [str(i) for i in dates]
OUT = a
";
            var empty = new ArrayList();
            var expected = new ArrayList
            {
                "2013-01-01 00:00:00",
                "2013-01-02 00:00:00",
                "2013-01-03 00:00:00",
                "2013-01-04 00:00:00",
                "2013-01-05 00:00:00",
                "2013-01-06 00:00:00",
            };

            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }

        [Test]
        public void TestScypiPythonModuleEncoding()
        {
            string code = @"
import sys
import clr

import numpy as np
from scipy import linalg
A = np.array([[1,2],[3,4]])
B = linalg.inv(A)
C = [[round(i) for i in b] for b in B]
OUT = C
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { -2, 1 }, new ArrayList { 1, 0 } };

            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }

        [Test]
        public void TestOpenpyxlPythonModuleEncoding()
        {
            string code = @"
import sys
import clr

from openpyxl import Workbook

wb = Workbook()
ws = wb.active

ws1 = wb.create_sheet(""Mysheet"") # insert at the end (default)
ws.title = ""New Title""
ws.sheet_properties.tabColor = ""1072BA""

result = wb.sheetnames
OUT = result
";
            var empty = new ArrayList();
            var expected = new ArrayList { "New Title", "Mysheet" };

            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }
        

        [Test]
        public void TestPillowPythonModuleEncoding()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\python\python_modules", "test.png");
            Assert.NotNull(examplePath);
            Assert.IsTrue(File.Exists(examplePath));
            examplePath = examplePath.Replace("\\", "/"); // Convert to python file path

            string code = string.Format(@"
import sys
import clr

from PIL import Image

im = Image.open('{0}')
OUT = im.format, im.size, im.mode
", examplePath);


            var empty = new ArrayList();
            var expected = new ArrayList{ "PNG", new ArrayList { 640, 480 }, "RGBA" };

        foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }


        [Test]
        public void TestMatplotlibPythonModuleEncoding()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\python\python_modules", "test_export.png");
            Assert.NotNull(examplePath);
            Assert.IsFalse(File.Exists(examplePath));   // Make sure the file is not there when we start
            examplePath = examplePath.Replace("\\", "/"); // Convert to python file path

            string code = string.Format(@"
import sys
import clr

import matplotlib.pyplot as plt
import numpy as np

x = np.linspace(0, 2 * np.pi, 200)
y = np.sin(x)

fig, ax = plt.subplots()
ax.plot(x, y)
plt.show()
image = plt.savefig('{0}')

OUT = ''
", examplePath);


            var empty = new ArrayList();

            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(File.Exists(examplePath));    // We should have exported an image now
                File.Delete(examplePath);
                Assert.IsFalse(File.Exists(examplePath));   // Make sure the file is not there when we finish
            }
        }

        #endregion
    }
}
