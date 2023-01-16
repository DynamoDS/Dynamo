using System;
using System.Collections.Generic;
using System.Text;
using Dynamo.Core;
using Dynamo.ViewModels;

namespace ViewModels.Core
{
    public class GeometryScalingViewModel : NotificationObject
    {
        private DynamoViewModel dynViewModel;
        private double scaleValue = 0;
        internal double ScaleValue
        {
            get
            {
                return scaleValue;
            }
            set
            {
                scaleValue = value;
                dynViewModel.ScaleFactorLog = (int)scaleValue;
                UpdateGeometryScale(scaleValue);
            }
        }

        internal GeometryScalingViewModel(DynamoViewModel dynViewModel)
        {
            this.dynViewModel = dynViewModel;
        }

        public Tuple<string, string, string> ScaleRange
        {
            get
            {
                return scaleRanges[ScaleSize];
            }
        }


        private Dictionary<GeometryScaleSize, Tuple<string, string, string>> scaleRanges = new Dictionary<GeometryScaleSize, Tuple<string, string, string>>
        {
            {GeometryScaleSize.Medium, new Tuple<string, string, string>("medium", "0.0001", "10,000")},
            {GeometryScaleSize.Small, new Tuple<string, string, string>("small", "0.000,001", "100")},
            {GeometryScaleSize.Large, new Tuple<string, string, string>("large", "0.01", "1,000,000")},
            {GeometryScaleSize.ExtraLarge, new Tuple<string, string, string>("extra large", "1", "100,000,000")}
        };

        public GeometryScaleSize ScaleSize
        {
            get
            {
                return dynViewModel.PreferencesViewModel.CurrentGeometryScaling;
            }
            set
            {
                dynViewModel.PreferencesViewModel.CurrentGeometryScaling = value;
                RaisePropertyChanged(nameof(ScaleSize));
            }
        }

        internal void UpdateGeometryScale(double scaleFactor)
        {
            int UIScaleFactor = GeometryScalingOptions.ConvertScaleFactorToUI((int)scaleFactor);

            if (Enum.IsDefined(typeof(GeometryScaleSize), UIScaleFactor))
            {
                ScaleSize = (GeometryScaleSize)UIScaleFactor;
            }
        }
    }
}
