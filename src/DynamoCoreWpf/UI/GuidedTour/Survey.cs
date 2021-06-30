using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public class Survey : Step
    {
        public Survey(HostInfo host, double width, double height)
            :base(host, width, height)
        {

        }

        protected override void CreatePopup()
        {
            //In this place will go the code for creating a SurveyWindow
        }
    }
}
