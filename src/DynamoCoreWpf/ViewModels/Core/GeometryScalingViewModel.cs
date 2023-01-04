using System;
using System.Collections.Generic;
using System.Text;
using Dynamo.Core;
using Dynamo.ViewModels;

namespace ViewModels.Core
{
    public class GeometryScalingViewModel : NotificationObject
    {
        DynamoViewModel dynViewModel;

        internal GeometryScalingViewModel(DynamoViewModel dynViewModel)
        {
            this.dynViewModel = dynViewModel;
        }

        public GeometryScaleSize ScaleSize
        {
            get
            {
                return dynViewModel.PreferencesViewModel.ScaleSize;
            }
            set
            {
                dynViewModel.PreferencesViewModel.ScaleSize = value;
                RaisePropertyChanged(nameof(ScaleSize));
            }
        }

        internal void UpdateGeometryScaling(string newScaleSize)
        {
            if (string.IsNullOrEmpty(newScaleSize)) return;
            ScaleSize = (GeometryScaleSize)Enum.Parse(typeof(GeometryScaleSize), newScaleSize);
        }
    }
}
