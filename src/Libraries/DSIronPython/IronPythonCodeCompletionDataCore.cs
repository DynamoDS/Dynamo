using Autodesk.DesignScript.Interfaces;

namespace DSIronPython
{
    /// <summary>
    /// Concrete type that gets returned and converted to an Avalonedit type implementing
    /// ICompletionData when used from WPF ScriptEditorContorl.
    /// </summary>
    internal class IronPythonCodeCompletionDataCore : IExternalCodeCompletionData
    {
        private IExternalCodeCompletionProviderCore provider;
        private string description;

        public IronPythonCodeCompletionDataCore(string text, string stub, bool isInstance,
            ExternalCodeCompletionType completionType, IExternalCodeCompletionProviderCore providerCore)
        {
            Text = text;
            Stub = stub;
            IsInstance = isInstance;
            provider = providerCore;
            CompletionType = completionType;
        }

        public string Text { get; private set; }

        public string Stub { get; private set; }

        public bool IsInstance { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text; }
        }

        public string Description
        {
            get
            {
                // lazily get the description
                if (description == null)
                {
                    description = provider.GetDescription(this.Stub, this.Text, this.IsInstance).TrimEnd('\r', '\n');
                }

                return description;
            }
        }
        public double Priority { get { return 0; } }

        public ExternalCodeCompletionType CompletionType { get; private set; }

    }

}
