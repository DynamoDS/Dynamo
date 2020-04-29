using System;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class created with the purpose of testing several methods/properties inside the next classes:
    /// ZoomEventArgs
    /// TaskDialogEventArgs
    /// EvaluationCompletedEventArgs
    /// DynamoModelUpdateArgs
    /// FunctionNamePromptEventArgs
    /// PresetsNamePromptEventArgs
    /// ViewOperationEventArgs
    /// PointEventArgs
    /// WorkspaceEventArgs
    /// ModelEventArgs
    /// </summary>
    [TestFixture]
    class DynamoModelEventArgsTest : DynamoModelTestBase
    {
        //This method is for creating a Code Block node in the current model
        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        /// <summary>
        /// This test method will execute the  next ZoomEventArgs methods/properties:
        ///  internal ZoomEventArgs(Point2D point)
        ///  internal ZoomEventArgs(double zoom, Point2D point)
        ///  internal Point2D Point { get; set; }
        ///   internal bool hasPoint()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestZoomEventArgs()
        {
            //Arrange/Act
            //This will execute two constructor of the ZoomEventArgs class
            var zoom = new ZoomEventArgs(new Point2D(100, 50));
            var zoom2 = new ZoomEventArgs(100, new Point2D(200, 100));

            //Assert
            //Checking that the values for Point were stored correctly and also this will execute the Point.Get method
            Assert.AreEqual(zoom.Point.X, 100);
            Assert.AreEqual(zoom.Point.Y, 50);

            Assert.AreEqual(zoom2.Point.X, 200);
            Assert.AreEqual(zoom2.Point.Y, 100);

            //This will execute the bool hasPoint() method, it returns true because we passed the Point as parameter
            Assert.IsTrue(zoom.hasPoint());
        }

        /// <summary>
        /// This test method will execute the next methods in the TaskDialogEventArgs class
        /// void AddLeftAlignedButton(int id, string content)
        /// Exception Exception { get; set; }
        /// IEnumerable<Tuple<int, string, bool>> Buttons
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestTaskDialogEventArgs()
        {
            //Act
            const string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
            //Using the constructor of TaskDialogEventArgs, the properties ImageUri, DialogTitle, Summary and Description will be populated
            var args = new TaskDialogEventArgs(new Uri(imageUri, UriKind.Relative),
               "SymbolWarningTitle", "Summary", "Description");

            //In this section is adding two buttons in the IEnumerable Buttons
            args.AddLeftAlignedButton(1000, "OK Button");
            args.AddLeftAlignedButton(1001, "Cancel Button");

            //This will execute the Set method of the Exception property
            args.Exception = new Exception("Testing Exception");

            //Assert
            //This will validate that the values of the properties ImageUri, DialogTitle, Summary and Description were stored correctly
            Assert.AreEqual(args.ImageUri.ToString(), imageUri);
            Assert.AreEqual(args.DialogTitle, "SymbolWarningTitle");
            Assert.AreEqual(args.Summary, "Summary");
            Assert.AreEqual(args.Description, "Description");

            Assert.IsNotNull(args.Exception);//This will execute the Get method of the Exception property
            Assert.AreEqual(args.Buttons.ToObservableCollection().Count,2);//This will execute the Get of the Buttons property
        }

        /// <summary>
        /// This test method will execute the next method/properties:
        /// public DynamoModelUpdateArgs(object item)
        /// public object Item { get; set; }
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestDynamoModelUpdateArgs()
        {
            //Arrange/Act
            var updateArgs = new DynamoModelUpdateArgs("test");

            //Assert
            Assert.AreEqual(updateArgs.Item,"test");
        }

        /// <summary>
        /// This test method will execute the next properties/methods in the PresetsNamePromptEventArgs class
        /// public PresetsNamePromptEventArgs()
        /// public string Name { get; set; }
        /// public string Description { get; set; }
        /// public bool Success { get; set; }
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestPresetsNamePromptEventArgs()
        {
            //Arrange/Act
            var args = new PresetsNamePromptEventArgs();
            //Subscribe the local function to the RequestPresetsNamePrompt event
            CurrentDynamoModel.RequestPresetsNamePrompt += CurrentDynamoModel_RequestPresetsNamePrompt;
            CurrentDynamoModel.OnRequestPresetNamePrompt(args);

            //After the event execution we need to unsubscribe the local method from the event
            CurrentDynamoModel.RequestPresetsNamePrompt -= CurrentDynamoModel_RequestPresetsNamePrompt;

            //Assert
            Assert.IsNotNull(args.Name);
            Assert.AreEqual(args.Description, "Test Description");
            Assert.AreEqual(args.Success, false);
        }

        /// <summary>
        /// This test method will execute the contructor for ViewOperationEventArgs 
        /// also will execute the Get/Set methods of the ViewOperation property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestViewOperationEventArgs()
        {
            //Arrange/Act
            //It will execute the constructor and internally the Set method for the ViewOperation property
            var args = new ViewOperationEventArgs(ViewOperationEventArgs.Operation.ZoomIn);

            //Assert
            //It will validate the stored operation and also will execute the Get method of the ViewOperation property
            Assert.AreEqual(args.ViewOperation, ViewOperationEventArgs.Operation.ZoomIn);
        }

        /// <summary>
        /// This test method will execute the next methods/properties in the WorkspaceEventArgs class
        ///  public WorkspaceEventArgs(WorkspaceModel workspace)
        ///  public WorkspaceModel Workspace { get; set; }
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWorkspaceEventArgs()
        {
            //Arrange/Act
            //It will execute the constructor and internally the Set for Workspace property
            var args = new WorkspaceEventArgs(CurrentDynamoModel.CurrentWorkspace);

            //Assert
            //This will execute the Get method for Workspace property
            Assert.IsNotNull(args.Workspace);
        }

        /// <summary>
        /// This test method will execute the next methods/properties in the ModelEventArgs class
        ///  public double X { get; private set; }
        ///  public double Y { get; private set; }
        ///  public ModelEventArgs(ModelBase model, double x, double y, bool transformCoordinates)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestModelEventArgs()
        {
            //Arrange
            var codeBlock = CreateCodeBlockNode();

            //Act
            //It will execute the constructor and internally the Set method X and Y properties
            var args = new ModelEventArgs(codeBlock, 100,50, true);

            //Assert
            //This will execute the Get method for X and Y properties
            Assert.AreEqual(args.X, 100);
            Assert.AreEqual(args.Y, 50);
        }


        //This method will be used in the TestPresetsNamePromptEventArgs() method when subscribing to the RequestPresetsNamePrompt event
        private void CurrentDynamoModel_RequestPresetsNamePrompt(PresetsNamePromptEventArgs obj)
        {
            obj.Description = "Test Description";
        }
    }
} 
