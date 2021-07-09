using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Wpf.UI.GuidedTour;

namespace Dynamo.Wpf.ViewModels.GuidedTour
{
    public class SurveyPopupViewModel
    {
        #region Private Properties
        private Survey step;
        #endregion

        /// <summary>
        /// This property hold a reference to the Step that was created (can be Welcome, Survey, Tooltip, ExitTour). 
        /// </summary>
        public Survey Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
            }
        }

        /// <summary>
        /// For creating a SurveyPopupViewModel we need to pass the step to be used 
        /// </summary>
        /// <param name="popupType">Step is a abstract class then the parameter can be any of the child clases like Welcome, Tooltip, Survey....</param>
        public SurveyPopupViewModel(Survey popupType)
        {
            Step = popupType;
        }
    }
}
