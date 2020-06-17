using System;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PythonNodeModels;

namespace Dynamo.Python
{
    internal class SharedCompletionProvider : LogSourceBase
    {

        #region Properties and Fields
        private readonly IExternalCodeCompletionProviderCore providerImplementation;

        #endregion

        #region constructors
        internal SharedCompletionProvider(PythonEngineVersion version ,string dynamoCoreDir)
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
                    .SelectMany(s => s.GetTypes())
                    .Where(p => completionType.IsAssignableFrom(p) && !p.IsInterface);
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
        internal ICompletionData[] GetCompletionData(string code, bool expand = false)
        {
            return this.providerImplementation?.GetCompletionData(code, expand).
                Select(x => new IronPythonCompletionData(x)).ToArray();
        }
        #endregion
    }
}