using Dynamo.Extensions;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    public class ViewStartupParams : StartupParams
    {
        private readonly DynamoViewModel dynamoViewModel;

        /// <summary>
        /// A handle to the extensions that are already constructed in the Model layer
        /// </summary>
        public IExtensionManager ExtensionManager
        {
            get
            {
                return dynamoViewModel.Model.ExtensionManager;
            }
        }

        internal ViewStartupParams(DynamoViewModel dynamoVM) :
            base(dynamoVM.Model.AuthenticationManager.AuthProvider,
                dynamoVM.Model.PathManager,
                new ExtensionLibraryLoader(dynamoVM.Model), 
				dynamoVM.Model.CustomNodeManager,
                dynamoVM.Model.GetType().Assembly.GetName().Version,
                dynamoVM.Model.PreferenceSettings)
        {
            dynamoViewModel = dynamoVM;
        }
    }
}
