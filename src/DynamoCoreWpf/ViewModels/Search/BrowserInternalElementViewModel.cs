using Dynamo.Search;

namespace Dynamo.Wpf.ViewModels
{
    public class BrowserInternalElementViewModel : BrowserItemViewModel
    {
        public BrowserInternalElement CastedModel { get; private set; }

        public BrowserInternalElementViewModel(BrowserInternalElement model)
            : base(model)
        {
            CastedModel = model;
        }
    }
}
