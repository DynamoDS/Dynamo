using System;
using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Configuration;
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
            libraries.Add("DSCoreNodes.dll");
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = base.CreateStartConfiguration(new PreferenceSettings());
            config.StartInTestMode = false;
            return config;
        }

        [Test]
        public void DynamoPrintLogsToConsole()
        {
            var expectedOutput1 = "Greeting PythonNet3 node: Hello from Python3!!!" + Environment.NewLine;
            var expectedOutput2 = "Greeting PythonNet3 String node: Hello from Python3!!!" + Environment.NewLine;
            var expectedOutput3 = "Multiple print parameter node: Hello Dynamo Print !!!" + Environment.NewLine;
            var expectedOutput4 = "Print separator parameter node: Hello_Dynamo_Print_!!!" + Environment.NewLine;
            var expectedOutput5 = @"!£$%^&*()_+-[{]}#~'@;:|\,<.>/? Special character node: Lot's of special characters!!!";

            CurrentDynamoModel.OpenFileFromPath(Path.Combine(TestDirectory, "core", "python", "DynamoPrint.dyn"));
            StringAssert.Contains(expectedOutput1, CurrentDynamoModel.Logger.LogText);
            StringAssert.Contains(expectedOutput2, CurrentDynamoModel.Logger.LogText);
            StringAssert.Contains(expectedOutput3, CurrentDynamoModel.Logger.LogText);
            StringAssert.Contains(expectedOutput4, CurrentDynamoModel.Logger.LogText);
            StringAssert.Contains(expectedOutput5, CurrentDynamoModel.Logger.LogText);
        }

        [Test]
        public void ResetCypythonLogsToConsoleAfterRun()
        {
            (CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunType = RunType.Manual;
            var expectedOutput = @"attempting reload of pythonnet3 modules
Python Script: considering sys
Python Script: considering builtins
Python Script: considering _frozen_importlib
Python Script: considering _imp
Python Script: considering _thread
Python Script: considering _warnings
Python Script: considering _weakref
Python Script: considering winreg
Python Script: considering _io
Python Script: considering marshal
Python Script: considering nt
Python Script: considering _frozen_importlib_external
Python Script: considering time
Python Script: considering zipimport
Python Script: reloading zipimport
Python Script: considering zlib
Python Script: considering _codecs
Python Script: considering codecs
Python Script: reloading codecs
Python Script: considering encodings.aliases
Python Script: considering encodings
Python Script: considering encodings.utf_8
Python Script: considering encodings.cp1252
Python Script: considering _abc
Python Script: considering abc
Python Script: reloading abc
Python Script: considering io
Python Script: reloading io
Python Script: considering __main__
Python Script: considering _stat
Python Script: considering stat
Python Script: reloading stat
Python Script: considering _collections_abc
Python Script: reloading _collections_abc
Python Script: considering genericpath
Python Script: reloading genericpath
Python Script: considering _winapi
Python Script: considering ntpath
Python Script: reloading ntpath
Python Script: considering os.path
Python Script: reloading os.path
Python Script: considering os
Python Script: reloading os
Python Script: considering _sitebuiltins
Python Script: reloading _sitebuiltins
Python Script: considering pywin32_system32
Python Script: considering pywin32_bootstrap
Python Script: reloading pywin32_bootstrap
Python Script: considering site
Python Script: reloading site
Python Script: considering CLR
Python Script: considering clr
Python Script: considering importlib._bootstrap
Python Script: considering importlib._bootstrap_external
Python Script: considering warnings
Python Script: considering importlib
Python Script: considering importlib.machinery
Python Script: reloading importlib.machinery
Python Script: considering importlib._abc
Python Script: considering posixpath
Python Script: reloading posixpath
Python Script: considering types
Python Script: considering _operator
Python Script: considering operator
Python Script: considering itertools
Python Script: considering keyword
Python Script: considering reprlib
Python Script: considering _collections
Python Script: considering collections
Python Script: considering _functools
Python Script: considering functools
Python Script: considering enum
Python Script: considering _sre
Python Script: considering re._constants
Python Script: considering re._parser
Python Script: considering re._casefix
Python Script: considering re._compiler
Python Script: considering copyreg
Python Script: considering re
Python Script: considering fnmatch
Python Script: considering errno
Python Script: considering urllib
Python Script: considering urllib.parse
Python Script: considering pathlib
Python Script: considering _compression
Python Script: considering _bz2
Python Script: considering bz2
Python Script: considering _lzma
Python Script: considering lzma
Python Script: considering shutil
Python Script: considering math
Python Script: considering _bisect
Python Script: considering bisect
Python Script: considering _random
Python Script: considering _sha512
Python Script: considering random
Python Script: considering _weakrefset
Python Script: considering weakref
Python Script: considering tempfile
Python Script: considering contextlib
Python Script: considering collections.abc
Python Script: considering _typing
Python Script: considering typing.io
Python Script: considering typing.re
Python Script: considering typing
Python Script: considering importlib.resources.abc
Python Script: considering importlib.resources._adapters
Python Script: considering importlib.resources._common
Python Script: considering importlib.resources._legacy
Python Script: considering importlib.resources
Python Script: considering importlib.abc
Python Script: considering clr._extras
Python Script: considering clr.interop
Python Script: considering clr._extras.collections
Python Script: considering Autodesk
Python Script: considering Autodesk.DesignScript
Python Script: considering Autodesk.DesignScript.Geometry
Python Script: considering importlib.util
Python Script: reloading importlib.util";
            var pythonNode = new PythonNode();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(pythonNode);
            pythonNode.EngineName = PythonEngineManager.PythonNet3EngineName;
          
            RunCurrentModel();
            CurrentDynamoModel.OnRequestPythonReset(PythonEngineManager.PythonNet3EngineName);
            foreach(var line in expectedOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                StringAssert.Contains(line, CurrentDynamoModel.Logger.LogText);
            }
        }
    }
}
