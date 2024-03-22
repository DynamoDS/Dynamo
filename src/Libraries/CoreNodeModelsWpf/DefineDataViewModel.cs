using CoreNodeModels;
using Dynamo.Core;

namespace CoreNodeModelsWpf
{
    public class DefineDataViewModel : NotificationObject
    {
        public DefineData Model { get; }

        public DefineDataViewModel()
        {

        }
        public DefineDataViewModel(DefineData model)
        {
            Model = model;
        }


    }
}
