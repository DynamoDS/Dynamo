using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using DSNodeServices;
using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Services;
using Dynamo.TestInfrastructure;
using Dynamo.UI;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUnits;
using DynamoUtilities;
using Microsoft.Practices.Prism.ViewModel;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Dynamo.UI.Prompts;

namespace Dynamo
{
    // KILLDYNSETTINGS - Move everything to DynamoViewModel
    public class DynamoController : NotificationObject
    {
        #region properties

        private bool uiLocked = true;
        public bool IsUILocked
        {
            get { return uiLocked; }
            set
            {
                uiLocked = value;
                RaisePropertyChanged("IsUILocked");
            }
        }

        public DynamoModel DynamoModel { get; set; }

        #endregion

        #region Constructor and Initialization

        public static DynamoViewModel MakeSandbox(string commandFilePath = null)
        {
            var corePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DynamoPathManager.Instance.InitializeCore(corePath);

            var updateManager = new UpdateManager.UpdateManager(logger);

            var dynamoModel = new DynamoModel("None", preferences, updateManager);

            var dynamoViewModel = new DynamoViewModel(dynamoModel, commandFilePath);

            this.WatchHandler = watchHandler;





            

            DynamoPathManager.Instance.InitializeCore(corePath);

            DynamoController controller;
            var logger = new DynamoLogger(DynamoPathManager.Instance.Logs);

            

            // If a command file path is not specified or if it is invalid, then fallback.
            if (string.IsNullOrEmpty(commandFilePath) || (File.Exists(commandFilePath) == false))
            {
                controller = new DynamoModel("None", Dynamo.PreferenceSettings.Load(), updateManager, corePath);

                new DefaultWatchHandler(), 
                controller.DynamoViewModel = new DynamoViewModel(controller, null);
            }
            else
            {
                controller = new DynamoController("None", updateManager,
                 new DefaultWatchHandler(), Dynamo.PreferenceSettings.Load(), corePath);

                controller.DynamoViewModel = new DynamoViewModel(controller, commandFilePath);
            }

            controller.VisualizationManager = new VisualizationManager();
            return controller;
        }

        /// <summary>
        ///     Class constructor
        /// </summary>
        public DynamoController(string context, IUpdateManager updateManager,
            IWatchHandler watchHandler, IPreferences preferences, string corePath)
        {

            

        }

        #endregion

        
    }
    
}
