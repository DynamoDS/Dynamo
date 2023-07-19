using System;
using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PythonServices;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    /// <summary>
    /// Python tests that require real logging to be enabled (StartInTestMode = false)
    /// </summary>
    public class PythonTestsWithLogging : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCPython.dll");
            libraries.Add("DSCoreNodes.dll");
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = base.CreateStartConfiguration(settings);
            config.StartInTestMode = false;
            return config;
        }

        [Test]
        public void DynamoPrintLogsToConsole()
        {
            var expectedOutput = ".*Greeting CPython node: Hello from Python3!!!" + Environment.NewLine
                + ".*Greeting CPython String node: Hello from Python3!!!" + Environment.NewLine
                + ".*Greeting CPython String node: Hello from Python3!!!" + Environment.NewLine
                + ".*Multiple print parameter node: Hello Dynamo Print !!!" + Environment.NewLine
                + ".*Print separator parameter node: Hello_Dynamo_Print_!!!" + Environment.NewLine
                + ".*`!\"£\\$%\\^&\\*\\(\\)_\\+-\\[\\{\\]\\}#~'@;:\\|\\\\,<\\.>/\\? Special character node: Lot's of special characters!!!" + Environment.NewLine
                + ".*";

            CurrentDynamoModel.OpenFileFromPath(Path.Combine(TestDirectory, "core", "python", "DynamoPrint.dyn"));
            StringAssert.IsMatch(expectedOutput, CurrentDynamoModel.Logger.LogText);
        }

        [Test]
        public void ResetCypythonLogsToConsoleAfterRun()
        {
            (CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunType = RunType.Manual;
            var expectedOutput = @"attempting reload of cpython3 modules
Python Script: considering sys
Python Script: considering builtins
Python Script: considering _frozen_importlib
Python Script: considering _imp
Python Script: considering _thread
Python Script: considering _warnings
Python Script: considering _weakref
Python Script: considering _io
Python Script: considering marshal
Python Script: considering nt
Python Script: considering winreg
Python Script: considering _frozen_importlib_external
Python Script: considering time
Python Script: considering zipimport
Python Script: considering zlib
Python Script: considering _codecs
Python Script: considering codecs
Python Script: considering encodings.aliases
Python Script: considering encodings
Python Script: considering encodings.utf_8
Python Script: considering encodings.cp1252
Python Script: considering encodings.latin_1
Python Script: considering _abc
Python Script: considering abc
Python Script: considering io
Python Script: considering __main__
Python Script: considering warnings
Python Script: considering CLR
Python Script: considering clr
Python Script: considering atexit
Python Script: considering clr._extras
Python Script: considering Autodesk
Python Script: considering Autodesk.DesignScript
Python Script: considering Autodesk.DesignScript.Geometry
Python Script: considering importlib._bootstrap
Python Script: considering importlib._bootstrap_external
Python Script: considering types
Python Script: considering importlib
Python Script: considering importlib.machinery
Python Script: considering _collections_abc
Python Script: considering _heapq
Python Script: considering heapq
Python Script: considering itertools
Python Script: considering keyword
Python Script: considering _operator
Python Script: considering operator
Python Script: considering reprlib
Python Script: considering _collections
Python Script: considering collections
Python Script: considering collections.abc
Python Script: considering _functools
Python Script: considering functools
Python Script: considering contextlib
Python Script: considering enum
Python Script: considering _sre
Python Script: considering sre_constants
Python Script: considering sre_parse
Python Script: considering sre_compile
Python Script: considering _locale
Python Script: considering copyreg
Python Script: considering re
Python Script: considering typing.io
Python Script: considering typing.re
Python Script: considering typing
Python Script: considering importlib.abc
Python Script: considering importlib.util
Python Script: considering _stat
Python Script: considering stat
Python Script: considering genericpath
Python Script: considering ntpath
Python Script: considering os.path
Python Script: considering os";
            var pythonNode = new PythonNode();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(pythonNode);
            pythonNode.EngineName = PythonEngineManager.CPython3EngineName;
          
            RunCurrentModel();
            CurrentDynamoModel.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);
            foreach(var line in expectedOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                StringAssert.Contains(line, CurrentDynamoModel.Logger.LogText);
            }
        }
    }
}
