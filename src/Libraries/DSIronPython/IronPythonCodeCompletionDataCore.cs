using Autodesk.DesignScript.Interfaces;
using Dynamo.PythonServices;

namespace DSIronPython
{
    /// <summary>
    /// Concrete type that gets returned and converted to an Avalonedit type implementing
    /// ICompletionData when used from WPF ScriptEditorContorl.
    /// </summary>
    internal class IronPythonCodeCompletionDataCore : PythonCodeCompletionDataCore
    {
        public IronPythonCodeCompletionDataCore(string text, string stub, bool isInstance,
                ExternalCodeCompletionType completionType, IExternalCodeCompletionProviderCore providerCore) : 
            base(text, stub, isInstance, completionType, providerCore)
        {
        }
    }
}
