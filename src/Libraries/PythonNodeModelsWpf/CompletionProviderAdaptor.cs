using System;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PythonNodeModels;

namespace Dynamo.Python
{
    // TODO 3.0: Change this file name and class to CompletionProviderAdaptor. 
    internal class CompletionProviderAdaptor : LogSourceBase
    {

        #region Properties and Fields
        private readonly IExternalCodeCompletionProviderCore providerImplementation;

        internal static string doubleQuoteStringRegex = "(\"[^\"]*\")"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        internal static string singleQuoteStringRegex = "(\'[^\']*\')"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        internal static string arrayRegex = "(\\[.*\\])";
        internal static string spacesOrNone = @"(\s*)";
        internal static string atLeastOneSpaceRegex = @"(\s+)";
        internal static string equals = @"(=)"; // Not CLS compliant - replaced with equalsRegex - Remove in Dynamo 3.0
        internal static string dictRegex = "({.*})";
        internal static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        internal static string intRegex = @"([-+]?\d+)[\s\n]*$";
        internal static string basicImportRegex = @"(import)";
        internal static string fromImportRegex = @"^(from)";

        #endregion

        #region constructors
        internal CompletionProviderAdaptor(PythonEngineVersion version ,string dynamoCoreDir)
        {
            var versionName = Enum.GetName(typeof(PythonEngineVersion), version);
            var matchingCore = FindMatchingCodeCompletionCore(versionName, this.AsLogger()) ;
            if(matchingCore != null)
            {
                this.providerImplementation = matchingCore;
                this.providerImplementation.Initialize(dynamoCoreDir);
                if(providerImplementation is ILogSource)
                {
                    (providerImplementation as ILogSource).MessageLogged += this.Log;
                }
            }
        }

        internal static IExternalCodeCompletionProviderCore FindMatchingCodeCompletionCore
            (string versionName, ILogger logger = null)
        {
            try
            {
                var completionType = typeof(IExternalCodeCompletionProviderCore);
                var loadedCodeCompletionTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => {
                        try {
                            return s.GetTypes();
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            return new Type[0];
                        }
                    }).Where(p => completionType.IsAssignableFrom(p) && !p.IsInterface).ToList();
                //instantiate them - so we can check which is a match using their match method
                foreach (var type in loadedCodeCompletionTypes)
                {
                    var inst = Activator.CreateInstance(type);

                    if ((inst as IExternalCodeCompletionProviderCore).IsSupportedEngine(versionName))
                    {
                        return inst as IExternalCodeCompletionProviderCore;
                    }
                }

                //if no matching completionprovider is found, just use the first one.
                if (loadedCodeCompletionTypes.Any())
                {
                    var inst = Activator.CreateInstance(loadedCodeCompletionTypes.First());
                    logger?.Log($"could not find a matching completion core for {versionName}, using instance of {inst.GetType().ToString()}");
                    return inst as IExternalCodeCompletionProviderCore;
                }
            }
            catch(Exception e)
            {
                logger?.Log(e);
                return null;
            }
            logger?.Log("could not find any IExternalCodeCompletionCore Types Loaded");
            return null;

        }

       
        #endregion

        #region Methods
        internal ICompletionData[] GetCompletionData(string code, PythonEngineVersion engineVersion, bool expand = false)
        {
            if (engineVersion.Equals(PythonEngineVersion.IronPython2) || engineVersion.Equals(PythonEngineVersion.CPython3))
            {
                return this.providerImplementation?.GetCompletionData(code, expand).
                    Select(x => new IronPythonCompletionData(x)).ToArray();
            }

            return null;
        }

        #endregion
    }
}