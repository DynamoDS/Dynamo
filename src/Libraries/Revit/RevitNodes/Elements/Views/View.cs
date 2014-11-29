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
        /// <param name="path">A valid path for the image</param>
        /// <returns>The image</returns>
        public Bitmap ExportAsImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Resource1.View_ExportAsImage_Path_Invalid, "path");
            }

            var fileType = ImageFileType.PNG;
            string extension = ".png";
            if (Path.HasExtension(path))
            {
                extension = Path.GetExtension(path);
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
                ExportRange = ExportRange.SetOfViews,
                FilePath = path,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = fileType,
                ImageResolution = ImageResolution.DPI_150,
                ShadowViewsFileType = fileType,
                ShouldCreateWebSite = false,
                ViewName = Guid.NewGuid().ToString(),
                Zoom = 100,
                ZoomType = ZoomFitType.Zoom
            };

            options.SetViewsAndSheets(new List<ElementId>{InternalView.Id});

            Document.ExportImage(options);

            var pathName = Path.Combine(
                            Path.GetDirectoryName(path),
                            Path.GetFileNameWithoutExtension(path));

            // Revit outputs file with a bunch of crap in the file name, let's construct that
            var actualFn = string.Format("{0} - {1} - {2}{3}", pathName, ViewTypeString(InternalView.ViewType),
                InternalView.ViewName, extension);

            // and the intended destination
            var destFn = pathName + extension;

            // rename the file
            if (File.Exists(destFn)) File.Delete(destFn);
            File.Move(actualFn, destFn);
            
            Bitmap bmp;
            try
            {
                using (var fs = new FileStream(destFn, FileMode.Open))
                {
                    bmp = new Bitmap(Image.FromStream(fs));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error exporting the image.", ex);
            }

            return bmp;
        }

        private static string ViewTypeString(ViewType vt)
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
            return GetType().Name + "(Name = " + InternalView.ViewName + " )";
        }
    }
}
