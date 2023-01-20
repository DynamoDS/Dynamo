using System;
using System.Collections.Generic;
using Dynamo.Core;
using Dynamo.ViewModels;

namespace ViewModels.Core
{
    /// <summary>
    /// This class will be contain information about the current Geometry Scale selected in the Dynamo Workspace
    /// </summary>
    public class GeometryScalingViewModel : NotificationObject
    {
        private DynamoViewModel dynViewModel;
        private GeometryScaleSize scaleSize;
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

        internal Tuple<string, string, string> ScaleRange
        {
            get
            {
                return scaleRanges[ScaleSize];
            }
        }


        internal static Dictionary<GeometryScaleSize, Tuple<string, string, string>> scaleRanges = new Dictionary<GeometryScaleSize, Tuple<string, string, string>>
        {
            {GeometryScaleSize.Medium, new Tuple<string, string, string>("medium", "0.0001", "10,000")},
            {GeometryScaleSize.Small, new Tuple<string, string, string>("small", "0.000,001", "100")},
            {GeometryScaleSize.Large, new Tuple<string, string, string>("large", "0.01", "1,000,000")},
            {GeometryScaleSize.ExtraLarge, new Tuple<string, string, string>("extra large", "1", "100,000,000")}
        };

        /// <summary>
        /// Current Geometry Scale selected in dynamo workspace.
        /// </summary>
        public GeometryScaleSize ScaleSize
        {
            get
            {
                return scaleSize;
            }
            set
            {
                if(scaleSize != value)
                {
                    scaleSize = value;
                    RaisePropertyChanged(nameof(ScaleSize));
                }               
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
