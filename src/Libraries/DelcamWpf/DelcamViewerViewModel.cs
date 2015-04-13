using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Delcam.FileSystem;
using Delcam.Geometry;
using Delcam.ProductInterface;
using Delcam.ProductInterface.PowerMILL;
using Delcam.WPFControls.WPF3DViewer;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

using DynamoDelcam;

namespace Dynamo.Wpf
{
    public class DelcamViewerViewModel : NotificationObject
    {
        private readonly DelcamViewer delcamViewerModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        private PMProject pmProject = null;

        public PMAutomation PowerMillAutomation { get; set; }

        public void InitializeView()
        {
            PowerMillAutomation.Execute("STATUS LOWER");
            PowerMillAutomation.Execute("EXPLORER LOWER");
        }

        public DelcamViewerViewModel(DelcamViewer model, NodeView nodeView)
        {
            delcamViewerModel = model;           
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged +=model_PropertyChanged;

            PowerMillAutomation = new PMAutomation(InstanceReuse.CreateSingleInstance);

            PowerMillAutomation.LoadProject(new Directory(@"C:\temp\curve_and_pattern"));

            pmProject = PowerMillAutomation.ActiveProject;

            var splinePoints = new List<Delcam.Geometry.Point>();

            splinePoints.Add(new Delcam.Geometry.Point(0, 0, 0));
            splinePoints.Add(new Delcam.Geometry.Point(5, 0, 0));
            splinePoints.Add(new Delcam.Geometry.Point(5, 5, 0));
            splinePoints.Add(new Delcam.Geometry.Point(10, 5, 0));

            Delcam.Geometry.Spline spline = new Spline(splinePoints);

            var tempFileName = System.IO.Path.GetTempFileName();

            var delcamFile = new Delcam.FileSystem.File(tempFileName);

            spline.WriteToDUCTPictureFile(delcamFile);

            if (pmProject.Patterns.Count > 0)
                pmProject.Patterns[0].InsertFile(delcamFile);
            else
                Console.WriteLine("Foobar");

            InitializeView();
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            }
        }

        public void Dispose()
        {
            PowerMillAutomation.CloseProject();
            //PowerMillAutomation.Quit();
            //PowerMillAutomation.Dispose();
        }

    }
}
