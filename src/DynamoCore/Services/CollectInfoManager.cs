using Dynamo.Services;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dynamo.Services
{
    public class CollectInfoManager : NotificationObject
    {
        #region Private
        private static CollectInfoManager instance;
        #endregion

        #region Properties
        public static CollectInfoPrompt CollectInfoPrompt { get; set; }

        public static CollectInfoManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new CollectInfoManager();
                return instance;
            }
        }

        public bool CollectInfoOption
        {
            get
            {
                return dynSettings.Controller.PreferenceSettings.CollectInfoOption;
            }
            private set
            {
                dynSettings.Controller.PreferenceSettings.CollectInfoOption = value;
                RaisePropertyChanged("CollectInfoOption");
            }
        }

        public bool NeverAgreeBefore
        {
            get
            {
                return dynSettings.Controller.PreferenceSettings.NeverAgreeBefore;
            }
            private set
            {
                dynSettings.Controller.PreferenceSettings.NeverAgreeBefore = value;
                RaisePropertyChanged("NeverAgreeBefore");
            }
        }

        public bool FirstRun
        {
            get
            {
                return dynSettings.Controller.PreferenceSettings.FirstRun;
            }
            private set
            {
                dynSettings.Controller.PreferenceSettings.FirstRun = value;
                RaisePropertyChanged("FirstRun");
            }
        }
        #endregion

        public void CheckFirstRun()
        {
            // First run of Dynamo
            if (dynSettings.Controller.PreferenceSettings.FirstRun)
            {
                this.FirstRun = false;

                if (!dynSettings.Controller.Testing)
                    SetCollectInfoOption(ShowCollectInfoPrompt());
            }
        }

        public void SetCollectInfoOption(bool collectInfoOption)
        {
            CollectInfoOption = collectInfoOption;
            if ( collectInfoOption )
                NeverAgreeBefore = false;

            //dynSettings.Controller.DynamoViewModel.R
        }

        public void ToggleCollectInfoOption()
        {
            bool resultOption;
            // If never consent to collection of information
            if (NeverAgreeBefore) // Prompt user to agree to the condition
                resultOption = ShowCollectInfoPrompt();
            else // User agree before, just toggle the option instead
                resultOption = !CollectInfoOption;

            SetCollectInfoOption(resultOption);
        }

        public bool ShowCollectInfoPrompt()
        {
            CollectInfoPrompt = new CollectInfoPrompt();
            if (null != Application.Current)
                CollectInfoPrompt.Owner = Application.Current.MainWindow;

            CollectInfoPrompt.ShowDialog();
            return CollectInfoPrompt.CollectDataConsent;
        }
    }
}
