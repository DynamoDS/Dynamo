using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Autodesk.Revit.DB;

namespace Revit.Elements
{
    /// <summary>
    /// An abstract Revit View - All view types inherit from this type
    /// </summary>
    //[SupressImportIntoVM]
    public abstract class AbstractView : AbstractElement
    {
        /// <summary>
        /// Obtain the reference Element as a View
        /// </summary>
        internal Autodesk.Revit.DB.View InternalView
        {
            get
            {
                return (Autodesk.Revit.DB.View) InternalElement;
            }
        }

        /// <summary>
        /// Export the view as an image to the given path as a PNG
        /// </summary>
        /// <param name="path">A valid path for the image</param>
        /// <returns>The image</returns>
        public System.Drawing.Image ExportAsImage(string path)
        {
            var options = new ImageExportOptions
            {
                ExportRange = ExportRange.SetOfViews,
                FilePath = path,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_72,
                ZoomType = ZoomFitType.Zoom,
                ShadowViewsFileType = ImageFileType.PNG
            };

            options.SetViewsAndSheets(new List<ElementId> { InternalView.Id });

            Document.ExportImage(options);

            return Image.FromFile(path + ".png");
        }
    }
}
