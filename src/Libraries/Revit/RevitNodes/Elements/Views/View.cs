using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Autodesk.Revit.DB;

namespace Revit.Elements.Views
{
    /// <summary>
    /// An abstract Revit View - All view types inherit from this type
    /// </summary>
    //[SupressImportIntoVM]
    public abstract class View : Element
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
        /// Export the view as an image to the given path - defaults to png, but you can override 
        /// the file type but supplying a path with the appropriate extension
        /// </summary>
        /// <param name="fullPath">A valid path for the image</param>
        /// <returns>The image</returns>
        public Bitmap ExportAsImage(string fullPath)
        {
            var fileType = ImageFileType.PNG;
            if (Path.HasExtension(fullPath))
            {
                string extension = Path.GetExtension(fullPath) ?? ".png";
                switch (extension.ToLower())
                {
                    case ".jpg":
                        fileType = ImageFileType.JPEGLossless;
                        break;
                    case ".png":
                        fileType = ImageFileType.PNG;
                        break;
                    case ".bmp":
                        fileType = ImageFileType.BMP;
                        break;
                    case ".tga":
                        fileType = ImageFileType.TARGA;
                        break;
                    case ".tif":
                        fileType = ImageFileType.TIFF;
                        break;
                }
            }
            
            var options = new ImageExportOptions
            {
                ExportRange = ExportRange.VisibleRegionOfCurrentView,
                FilePath = fullPath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = fileType,
                ImageResolution = ImageResolution.DPI_150,
                ShadowViewsFileType = fileType,
                ShouldCreateWebSite = false,
                ViewName = Guid.NewGuid().ToString(),
                Zoom = 100,
                ZoomType = ZoomFitType.Zoom
            };

            Document.ExportImage(options);

            using (var fs = new FileStream(fullPath, FileMode.Open))
                return new Bitmap(Image.FromStream(fs));
        }

        public override string ToString()
        {
            return GetType().Name + "(Name = " + InternalView.ViewName + " )";
        }
    }
}
