using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Autodesk.Revit.DB;
using Revit.Elements;

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
        public System.Drawing.Image ExportAsImage(string fullPath)
        {
            string pathName = fullPath;
            string extension = null;

            var fileType = ImageFileType.PNG;
            if (Path.HasExtension(fullPath))
            {
                extension = Path.GetExtension(fullPath).ToLower();
                switch (extension)
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
                pathName = Path.Combine(
                    Path.GetDirectoryName(fullPath),
                    Path.GetFileNameWithoutExtension(fullPath));
            }

            extension = (extension ?? ".png");

            var options = new ImageExportOptions
            {
                ExportRange = ExportRange.SetOfViews,
                FilePath = pathName,
                HLRandWFViewsFileType = fileType,
                ImageResolution = ImageResolution.DPI_72,
                ZoomType = ZoomFitType.Zoom,
                ShadowViewsFileType = fileType
            };

            options.SetViewsAndSheets(new List<ElementId> { InternalView.Id });

            Document.ExportImage(options);

            // Revit outputs file with a bunch of crap in the file name, let's construct that
            var actualFn = string.Format("{0} - {1} - {2}{3}", pathName, ViewTypeString(InternalView.ViewType),
                InternalView.ViewName, extension);

            // and the intended destination
            var destFn = pathName + extension;

            // rename the file
            if (File.Exists(destFn)) File.Delete(destFn);
            File.Move(actualFn, destFn);

            return Image.FromFile(destFn);
        }

        private string ViewTypeString(ViewType vt)
        {
            switch (vt)
            {
                case ViewType.ThreeD:
                    return "3D View";
                case ViewType.AreaPlan:
                    return "Area Plan";
                case ViewType.CeilingPlan:
                    return "Ceiling Plan";
                case ViewType.EngineeringPlan:
                    return "Engineering Plan";
                case ViewType.FloorPlan:
                    return "Floor Plan";
                case ViewType.Elevation:
                    return "Elevation";
                default:
                    return "Section View";
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + "(Name = " + this.InternalView.ViewName + " )";
        }
    }
}
