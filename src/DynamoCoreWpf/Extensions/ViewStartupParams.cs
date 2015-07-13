using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Extensions;
using Dynamo.Core;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    public class ViewStartupParams
    {
        private DynamoViewModel dynamoViewModel;

        /// <summary>
        /// A handle to the extensions that are already constructed in the Model layer
        /// </summary>
        public IExtensionManager extensionManager;

        public ViewStartupParams(DynamoViewModel dynamoVM)
        {
            dynamoViewModel = dynamoVM;
        }

        public AuthenticationManager DynamoAuthenticationManager
        {
            get
            {
                return dynamoViewModel.Model.AuthenticationManager;
            }
        }
    }
}
