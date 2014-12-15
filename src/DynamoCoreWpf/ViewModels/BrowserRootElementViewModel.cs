using Dynamo.Search;

namespace Dynamo.Wpf.ViewModels
{
    public class BrowserRootElementViewModel : BrowserItemViewModel
    {
        private ClassInformationViewModel classDetails;
        public ClassInformationViewModel ClassDetails
        {
            get
            {
                if (classDetails == null && CastedModel.IsPlaceholder)
                {
                    classDetails = new ClassInformationViewModel();
                    classDetails.IsRootCategoryDetails = true;
                    classDetails.PopulateMemberCollections(this.Model);
                }

                return classDetails;
            }
        }

        public BrowserRootElement CastedModel { get; private set; }

        public BrowserRootElementViewModel(BrowserRootElement model)
            : base(model)
        {
            CastedModel = model;
        }
    }
}
