using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CoreNodeModelsWpf.Charts.Controls;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Win32;

namespace CoreNodeModelsWpf.Charts.Utilities
{
    public static class Export
    {
        /// <summary>
        /// Saves the chart as a image to the user defined location.
        /// </summary>
        /// <param name="control">For Cartesian or Pie Charts use this argument </param>
        /// <param name="gcontrol">GeoMapChart Control, if the incoming chart is of type GeoMap, use this argument, defaults to null</param>
        public static void ToPng(Chart control, GeoMap gcontrol = null)
        {
            if (control == null && gcontrol == null) return;

            int img_width = control == null ? (int)gcontrol.ActualWidth : (int)control.ActualWidth;
            int img_height = control == null ? (int)gcontrol.ActualHeight : (int)control.ActualHeight;
            InMemorySkiaSharpChart skChart;
            switch (control)
            {
                case CartesianChart cartesianChart:
                    skChart = new SKCartesianChart(cartesianChart) { Width = img_width, Height = img_height, };
                    break;
                case PieChart pieChart:
                    skChart = new SKPieChart(pieChart) { Width = img_width, Height = img_height, };
                    break;
                default:
                    skChart = new SKGeoMap(gcontrol) { Width = img_width, Height = img_height, };
                    break;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "NodeModelChart"; // Default file name
            dialog.DefaultExt = ".png"; // Default file extension
            dialog.Filter = "Image files (*.png) | *.png"; // Filter files by extension

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dialog.FileName;
                skChart.SaveImage(filename, SkiaSharp.SKEncodedImageFormat.Png);
            }
        }
    }
}
