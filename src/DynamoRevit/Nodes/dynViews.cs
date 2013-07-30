//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Drafting View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a drafting view.")]
    public class dynDraftingView: dynNodeWithOneOutput
    {
        public dynDraftingView()
        {
            InPortData.Add(new PortData("name", "Name", typeof(Value.String)));
            OutPortData.Add(new PortData("v", "Drafting View", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            ViewDrafting vd = null;
            string viewName = ((Value.String)args[0]).Item;

            if (!string.IsNullOrEmpty(viewName))
            {
                //if we've already found the view
                //and it's the same one, get out
                if (vd != null && vd.Name == viewName)
                {
                    return Value.NewContainer(vd);
                }

                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fec.OfClass(typeof(ViewDrafting));

                IList<Element> els = fec.ToElements();

                var vds = from v in els
                            where ((ViewDrafting)v).Name == viewName
                            select v;

                if (vds.Count() == 0)
                {
                    try
                    {
                        //create the view
                        vd = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
                        if (vd != null)
                        {
                            vd.Name = viewName;
                        }
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log(string.Format("Could not create view: {0}", viewName));
                    }
                }
                else
                {
                    vd = vds.First() as ViewDrafting;
                }
            }

            return Value.NewContainer(vd);
        }
    }

    public delegate View3D View3DCreationDelegate(ViewOrientation3D orient, string name, bool isPerspective);

    public abstract class dynViewBase:dynRevitTransactionNodeWithOneOutput
    {
        protected bool _isPerspective = false;

        protected dynViewBase()
        {
            InPortData.Add(new PortData("eye", "The eye position point.", typeof(Value.Container)));
            InPortData.Add(new PortData("up", "The up direction of the view.", typeof(Value.Container)));
            InPortData.Add(new PortData("forward", "The view direction - the vector pointing from the eye towards the model.", typeof(Value.Container)));
            InPortData.Add(new PortData("name", "The name of the view.", typeof(Value.String)));

            OutPortData.Add(new PortData("v", "The newly created 3D view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            View3D view = null;
            var eye = (XYZ)((Value.Container)args[0]).Item;
            var userUp = (XYZ)((Value.Container)args[1]).Item;
            var direction = (XYZ)((Value.Container)args[2]).Item;
            var name = ((Value.String)args[3]).Item;

            XYZ side;
            if (direction.IsAlmostEqualTo(userUp) || direction.IsAlmostEqualTo(userUp.Negate()))
                side = XYZ.BasisZ.CrossProduct(direction);
            else
                side = userUp.CrossProduct(direction);
            XYZ up = side.CrossProduct(direction);

            //need to reverse the up direction to get the 
            //proper orientation - there might be a better way to handle this
            var orient = new ViewOrientation3D(eye, -up, direction);

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(View3D), out e))
                {
                    view = (View3D)e;
                    if (!view.ViewDirection.IsAlmostEqualTo(direction))
                    {
                        view.Unlock();
                        view.SetOrientation(orient);
                        view.SaveOrientationAndLock();
                    }
                    if (view.Name != null && view.Name != name)
                        view.Name = CreateUniqueViewName(name);
                }
                else
                {
                    //create a new view
                    view = dynViewBase.Create3DView(orient, name, false);
                    Elements[0] = view.Id;
                }
            }
            else
            {
                view = Create3DView(orient, name, false);
                Elements.Add(view.Id);
            }

            return Value.NewContainer(view);
        }

        public static View3D Create3DView(ViewOrientation3D orient, string name, bool isPerspective)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.ThreeDimensional
                                                          select type;

            //create a new view
            View3D view = isPerspective ?
                              View3D.CreateIsometric(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id) :
                              View3D.CreatePerspective(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id);

            view.SetOrientation(orient);
            view.SaveOrientationAndLock();
            view.Name = CreateUniqueViewName(name);

            return view;
        }
    
        /// <summary>
        /// Determines whether a view with the provided name already exists. Increment
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateUniqueViewName(string name)
        {
            string viewName = name;
            bool found = false;

            var collector = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            collector.OfClass(typeof(View3D));

            int count = 0;
            while (!found)
            {
                string[] nameChunks = viewName.Split('_');

                viewName = string.Format("{0}_{1}", nameChunks[0], count.ToString(CultureInfo.InvariantCulture));

                if (collector.ToElements().ToList().Any(x => x.Name == viewName))
                    count++;
                else
                    found = true;
            }

            return viewName;
        }
    }

    [NodeName("Axonometric View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates an axonometric view.")]
    public class dynIsometricView : dynViewBase
    {
        public dynIsometricView ()
        {
            _isPerspective = false;
        }
    }

    [NodeName("Perspective View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a perspective view.")]
    public class dynPerspectiveView : dynViewBase
    {
        public dynPerspectiveView()
        {
            _isPerspective = true;
        }
    }

    [NodeName("Bounding Box XYZ")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Create a bounding box.")]
    public class dynBoundingBoxXYZ : dynNodeWithOneOutput
    {
        public dynBoundingBoxXYZ()
        {
            InPortData.Add(new PortData("trans", "The coordinate system of the box.", typeof(Value.Container)));
            InPortData.Add(new PortData("x size", "The size of the bounding box in the x direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("y size", "The size of the bounding box in the y direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("z size", "The size of the bounding box in the z direction of the local coordinate system.", typeof(Value.Number)));
            OutPortData.Add(new PortData("bbox", "The bounding box.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            
            Transform t = (Transform)((Value.Container)args[0]).Item;
            double x = (double)((Value.Number)args[1]).Item;
            double y = (double)((Value.Number)args[2]).Item;
            double z = (double)((Value.Number)args[3]).Item;

            bbox.Transform = t;
            bbox.Min = new XYZ(0, 0, 0);
            bbox.Max = new XYZ(x, y, z);
            return Value.NewContainer(bbox);
        }

    }

    [NodeName("Section View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a section view.")]
    public class dynSectionView : dynRevitTransactionNodeWithOneOutput
    {
        public dynSectionView()
        {
            InPortData.Add(new PortData("bbox", "The bounding box of the view.", typeof(Value.Container)));
            OutPortData.Add(new PortData("v", "The newly created section view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ViewSection view = null;
            BoundingBoxXYZ bbox = (BoundingBoxXYZ)((Value.Container)args[0]).Item;

            //recreate the view. it does not seem possible to update a section view's orientation
            if (this.Elements.Any())
            {
                //create a new view
                view = CreateSectionView(bbox);
                Elements[0] = view.Id;
            }
            else
            {
                view = CreateSectionView(bbox);
                Elements.Add(view.Id);
            }

            return Value.NewContainer(view);
        }

        private static ViewSection CreateSectionView(BoundingBoxXYZ bbox)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.Section
                                                          select type;

            //create a new view
            ViewSection view = ViewSection.CreateSection(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id, bbox);
            return view;
        }
    }


    [NodeName("Get Active View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Gets the active Revit view.")]
    public class dynActiveRevitView : dynRevitTransactionNodeWithOneOutput
    {
        public dynActiveRevitView()
        {
            OutPortData.Add(new PortData("v", "The active revit view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(dynRevitSettings.Doc.Document.ActiveView);
        }

    }

    [NodeName("Save Image from View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Saves an image from a Revit view.")]
    public class dynSaveImageFromRevitView : dynRevitTransactionNodeWithOneOutput
    {
        public dynSaveImageFromRevitView()
        {
            InPortData.Add(new PortData("view", "The view to export.", typeof(Value.Container)));
            InPortData.Add(new PortData("path", "The path to export to.", typeof(Value.String)));
            OutPortData.Add(new PortData("image", "An image from the revit view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            View view = (View)((Value.Container)args[0]).Item;
            string path = ((Value.String)args[1]).Item;

            string name = view.ViewName;
            string pathName = path + "\\" + name;
            System.Drawing.Image image;


            ImageExportOptions options = new ImageExportOptions();
            options.ExportRange = ExportRange.VisibleRegionOfCurrentView;
            options.FilePath = pathName;
            options.HLRandWFViewsFileType = ImageFileType.PNG; //hack - make sure to change the read image below if other file types are supported
            options.ImageResolution = ImageResolution.DPI_72; 
            options.ZoomType = ZoomFitType.Zoom;
            options.ShadowViewsFileType = ImageFileType.PNG;
 

            try
            {
                dynRevitSettings.Doc.Document.ExportImage(options);//revit only has a method to save image to disk.
                //hack - make sure to change the read image below if other file types are supported
                image = Image.FromFile(pathName + ".png");//read the saved image so we can pass it downstream


            }
            catch (Exception e)
            {
                dynSettings.Controller.DynamoViewModel.Log(e);
                return Value.NewContainer(0);
            }

            return Value.NewContainer(image);
        }

    }

    
    [NodeName("Watch Image")]
    [NodeDescription("Previews an image")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    public class dynWatchImage : dynNodeWithOneOutput
    {

        ResultImageUI resultImageUI = new ResultImageUI();

        System.Windows.Controls.Image image1 = null;
        public dynWatchImage()
        {
            InPortData.Add(new PortData("image", "image", typeof(object)));
            OutPortData.Add(new PortData("", "Success?", typeof(bool)));

            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            image1 = new System.Windows.Controls.Image();
            image1.Width = 320;
            image1.Height = 240;
            //image1.Margin = new Thickness(5);
            image1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image1.Name = "image1";
            image1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //image1.DataContext = resultImageUI;

            var bindingVal = new System.Windows.Data.Binding("ResultImage")
            {
                Mode = BindingMode.OneWay,
                Converter = new ImageConverter(),
                NotifyOnValidationError = false,
                Source = resultImageUI,
                //Path = ResultImageUI,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            image1.SetBinding(System.Windows.Controls.Image.SourceProperty, bindingVal);

            NodeUI.inputGrid.Children.Add(image1);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            resultImageUI.ResultImage = (Image)((Value.Container)args[0]).Item;

            //DispatchOnUIThread(delegate
            //{

            //   image1.Source = resultImage;
            //});

            return Value.NewNumber(1);
        }

        /// <summary>
        /// One-way converter from System.Drawing.Image to System.Windows.Media.ImageSource
        /// from http://www.stevecooper.org/index.php/2010/08/06/databinding-a-system-drawing-image-into-a-wpf-system-windows-image/
        /// </summary>
        [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
        public class ImageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType,
                object parameter, CultureInfo culture)
            {
                // empty images are empty...
                if (value == null) { return null; }

                try
                {
                    var image = (System.Drawing.Image)value;
                    System.Drawing.Image readImage = null;
                    // Winforms Image we want to get the WPF Image from...
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    MemoryStream memoryStream = new MemoryStream();
                    // Save to a memory stream...
                    //image.Save("C:\\falconOut\\Falcon.bmp");

                    image.Save(memoryStream, ImageFormat.Bmp);
                    // Rewind the stream...
                    //memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch (Exception ex)
                {
                    dynSettings.Controller.DynamoViewModel.Log(ex.Message);
                    dynSettings.Controller.DynamoViewModel.Log(ex.StackTrace);
                    return null;
                }
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, CultureInfo culture)
            {
                return null;
            }
        }

        public class ResultImageUI : INotifyPropertyChanged
        {
            private Image resultImage;

            public Image ResultImage
            {
                get
                {
                    return resultImage;
                }

                set
                {
                    resultImage = value;
                    Notify("ResultImage");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, e);
            }

            protected void OnPropertyChanged(string propertyName)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }

            protected void Notify(string propertyName)
            {

                if (this.PropertyChanged != null)
                {
                    try
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                    catch (Exception ex)
                    {
                        dynSettings.Controller.DynamoViewModel.Log(ex.Message);
                        dynSettings.Controller.DynamoViewModel.Log(ex.StackTrace);
                    }
                }
            }

        }
      
    }
     
}
