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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using System.Xml.Serialization;
using Dynamo.Nodes.Prompts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;

using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Utilities;
using IWin32Window = System.Windows.Interop.IWin32Window;
using MessageBox = System.Windows.Forms.MessageBox;
using Rectangle = System.Drawing.Rectangle;
using Dynamo.FSchemeInterop;


#if DEBUG
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Framework;
using NUnit.Util;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

#endif

//MDJ needed for spatialfeildmanager
//TAF added to get strings from resource files

namespace Dynamo.Applications
{
    //MDJ - Added by Matt Jezyk - 10.27.2011
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string m_AssemblyName = Assembly.GetExecutingAssembly().Location;
        public static DynamoUpdater updater;
        private static ResourceManager res;
        public static ExecutionEnvironment env;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));

                //Create a push button in the ribbon panel 
                var pushButton = ribbonPanel.AddItem(new PushButtonData("Dynamo",
                                                                        res.GetString("App_Name"), m_AssemblyName,
                                                                        "Dynamo.Applications.DynamoRevit")) as
                                 PushButton;

                Bitmap dynamoIcon = Resources.Nodes_32_32;


                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    dynamoIcon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

#if DEBUG
                var pushButton1 = ribbonPanel.AddItem(new PushButtonData("Test",
                                                                        res.GetString("App_Name"), m_AssemblyName,
                                                                        "Dynamo.Applications.DynamoRevitTester")) as PushButton;
                pushButton1.LargeImage = bitmapSource;
                pushButton1.Image = bitmapSource;
#endif

                // MDJ = element level events and dyanmic model update
                // MDJ 6-8-12  trying to get new dynamo to watch for user created ref points and re-run definition when they are moved

                IdlePromise.RegisterIdle(application);

                updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
                if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
                    UpdaterRegistry.RegisterUpdater(updater);

                var SpatialFieldFilter = new ElementClassFilter(typeof (SpatialFieldManager));
                var familyFilter = new ElementClassFilter(typeof (FamilyInstance));
                var refPointFilter = new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints);
                var modelCurveFilter = new ElementClassFilter(typeof (CurveElement));
                var sunFilter = new ElementClassFilter(typeof (SunAndShadowSettings));
                IList<ElementFilter> filterList = new List<ElementFilter>();

                filterList.Add(SpatialFieldFilter);
                filterList.Add(familyFilter);
                filterList.Add(modelCurveFilter);
                filterList.Add(refPointFilter);
                filterList.Add(sunFilter);

                ElementFilter filter = new LogicalOrFilter(filterList);

                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());

                env = new ExecutionEnvironment();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevit : IExternalCommand
    {
        private static DynamoView dynamoView;
        private UIDocument m_doc;
        private UIApplication m_revit;
        private DynamoController dynamoController;
        private static bool isRunning = false;

        public static double? dynamoViewX = null;
        public static double? dynamoViewY = null;
        public static double? dynamoViewWidth = null;
        public static double? dynamoViewHeight = null;
        private bool handledCrash = false;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            //When a user double-clicks the Dynamo icon, we need to make
            //sure that we don't create another instance of Dynamo.
            if (isRunning)
            {
                Debug.WriteLine("Dynamo is already running.");
                if (dynamoView != null)
                {
                    dynamoView.Focus();
                }
                return Result.Succeeded;
            }

            isRunning = true;

            DynamoLogger.Instance.StartLogging();

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof (Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = m_revit;
                dynRevitSettings.Doc = m_doc;
                dynRevitSettings.DefaultLevel = defaultLevel;

                IdlePromise.ExecuteOnIdle(delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        //show the window

                        Regex r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                        string context = r.Replace(m_revit.Application.VersionName, "");

                        //they changed the application version name conventions for vasari
                        //it no longer has a version year so we can't compare it to other versions
                        //TODO:come up with a more stable way to test for Vasari beta 3
                        if (context == "Vasari")
                            context = "Vasari 2014";

                        dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.updater, typeof(DynamoRevitViewModel), context);

                        dynSettings.Bench = new DynamoView {DataContext = dynamoController.DynamoViewModel};
                        dynamoController.UIDispatcher = dynSettings.Bench.Dispatcher;
                        dynamoView = dynSettings.Bench;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoView).Owner = mwHandle;

                        handledCrash = false;

                        dynamoView.WindowStartupLocation = WindowStartupLocation.Manual;

                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        dynamoView.Left = dynamoViewX ?? bounds.X;
                        dynamoView.Top = dynamoViewY ?? bounds.Y;
                        dynamoView.Width = dynamoViewWidth ?? 1000.0;
                        dynamoView.Height = dynamoViewHeight ?? 800.0;

                        dynamoView.Show();
                        dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException; 
                        dynamoView.Closing += dynamoView_Closing;
                        dynamoView.Closed += dynamoView_Closed;
                    });
            }
            catch (Exception ex)
            {
                isRunning = false;
                MessageBox.Show(ex.ToString());

                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
                DynamoLogger.Instance.Log("Dynamo log ended " + DateTime.Now.ToString());

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        /// Dynamo is still valid at this time, but further work may cause corruption.  Here, 
        /// we run the ExitCommand, allowing the user to save all of their work.  Then, we send them
        /// to the issues page on Github. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {

            // only handle a single crash per Dynamo sesh, this should be reset in the initial command
            if (handledCrash)
            {
                args.Handled = true;
                return;
            }

            handledCrash = true;

            var exceptionMessage = args.Exception.Message;
            var stackTrace = args.Exception.StackTrace;

            var prompt = new CrashPrompt(exceptionMessage + "\n\n" + stackTrace);
            prompt.ShowDialog();

            try
            {
                DynamoLogger.Instance.Log("Dynamo Unhandled Exception");
                DynamoLogger.Instance.Log(exceptionMessage);
            }
            catch
            {

            }

            try
            {
                dynamoController.DynamoViewModel.ExitCommand.Execute(false); // don't allow cancellation
                dynamoController.DynamoViewModel.ReportABugCommand.Execute();
            }
            catch
            {

            }
            finally
            {
                
                args.Handled = true;
            }
            
        }

        /// <summary>
        /// Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closing(object sender, EventArgs e)
        {
            // cache the size of the window for later reloading
            dynamoViewX = dynamoView.Left;
            dynamoViewY = dynamoView.Top;
            dynamoViewWidth = dynamoView.ActualWidth;
            dynamoViewHeight = dynamoView.ActualHeight;
            IdlePromise.ClearPromises();
        }

        /// <summary>
        /// Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closed(object sender, EventArgs e)
        {
            dynamoView = null;
            isRunning = false;
        }
    }

#if DEBUG

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitTester : IExternalCommand
    {
        private UIDocument m_doc;
        private UIApplication m_revit;
        public DynamoRevitTestRunner Results{get;set;}

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof(Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = m_revit;
                dynRevitSettings.Doc = m_doc;
                dynRevitSettings.DefaultLevel = defaultLevel;

                //create dynamo
                Regex r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                string context = r.Replace(m_revit.Application.VersionName, "");

                var dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.updater, typeof(DynamoRevitViewModel), context);

                //flag to run evalauation synchronously, helps to 
                //avoid threading issues when testing.
                dynamoController.Testing = true;
                
                //execute the tests
                Results = new DynamoRevitTestRunner();
                DynamoRevitTestResultsView resultsView = new DynamoRevitTestResultsView();
                resultsView.DataContext = Results;

                //http://stackoverflow.com/questions/2798561/how-to-run-nunit-from-my-code
                string assLocation = Assembly.GetExecutingAssembly().Location;
                FileInfo fi = new FileInfo(assLocation);
                string testLoc = Path.Combine(fi.DirectoryName, @"DynamoRevitTester.dll");

                //Tests must be executed on the main thread in order to access the Revit API.
                //NUnit's SimpleTestRunner runs the tests on the main thread
                //http://stackoverflow.com/questions/16216011/nunit-c-run-specific-tests-through-coding?rq=1
                CoreExtensions.Host.InitializeService();
                SimpleTestRunner runner = new SimpleTestRunner();
                TestSuiteBuilder builder = new TestSuiteBuilder();
                TestPackage package = new TestPackage("DynamoRevitTests", new List<string>() { testLoc });
                runner.Load(package);
                TestSuite suite = builder.Build(package);
                TestFixture fixture = null;
                FindFixtureByName(out fixture, suite, "DynamoRevitTests");
                if (fixture == null)
                    throw new Exception("Could not find DynamoRevitTests fixture.");

                foreach (var t in fixture.Tests)
                {
                    if (t is ParameterizedMethodSuite)
                    {
                        var paramSuite = t as ParameterizedMethodSuite;
                        foreach (var tInner in paramSuite.Tests)
                        {
                            if (tInner is TestMethod)
                                Results.Results.Add(new DynamoRevitTest(tInner as TestMethod));
                        }
                    }
                    else if (t is TestMethod)
                        Results.Results.Add(new DynamoRevitTest(t as TestMethod));
                }

                resultsView.ShowDialog();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void FindFixtureByName(out TestFixture fixture, TestSuite suite, string name)
        {
            foreach (TestSuite innerSuite in suite.Tests)
            {
                if (innerSuite is TestFixture)
                {
                    if (((TestFixture)innerSuite).TestName.Name == name)
                    {
                        fixture = innerSuite as TestFixture;
                        return;
                    }
                }
                else
                {
                    FindFixtureByName(out fixture, innerSuite, name);
                    if (fixture != null)
                        return;
                }
            }

            fixture = null;
        }
    }

    public delegate void TestStartedHandler(object sender, EventArgs e);

    public delegate void TestCompletedHandler(object sender, EventArgs e);

    /// <summary>
    /// A wrapper to an NUnit test.
    /// </summary>
    public class DynamoRevitTest:NotificationObject
    {
        public event TestStartedHandler TestStarted;
        protected virtual void OnTestStarted(EventArgs e)
        {
            if (TestStarted != null)
                TestStarted(this, e);
        }

        public event TestCompletedHandler TestCompleted;
        protected virtual void OnTestCompleted(EventArgs e)
        {
            if (TestCompleted != null)
                TestCompleted(this, e);
        }

        DynamoRevitTestResultType _resultType;
        private string _message;
        private TestMethod _test;
        private readonly RevitTestEventListener _listener;
        private string _testName="";

        [XmlIgnore]
        public DelegateCommand RunCommand { get; set; }

        public DynamoRevitTestResultType ResultType 
        {
            get { return _resultType; }
            set
            {
                _resultType = value;
                RaisePropertyChanged("ResultType");
            }
        }
        
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        [XmlAttribute]
        public string TestName
        {
            get { return _testName; }
            set { _testName = value; }
        }

        [XmlIgnoreAttribute]
        public TestMethod Test
        {
            get { return _test; } 
            set { _test = value; }
        }

        public DynamoRevitTest()
        {
        }

        public DynamoRevitTest(TestMethod test)
        {
            _test = test;
            _listener = new RevitTestEventListener(this);
            RunCommand = new DelegateCommand(Run, CanRun);
            _resultType = DynamoRevitTestResultType.Unknown;
            _testName = _test.TestName.Name;
        }

        public void Run()
        {
            OnTestStarted(EventArgs.Empty);

            Debug.WriteLine(string.Format("Running test {0}", _test.TestName));
            try
            {
                TestName testName = _test.TestName;
                TestFilter filter = new NameFilter(testName);
                TestResult result = _test.Run(_listener, filter);
                var summ = new ResultSummarizer(result);
                Assert.AreEqual(1, summ.ResultCount);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e.Message);
                DynamoLogger.Instance.Log(string.Format("Failed to run test : {0}", _test.TestName));
            }

            OnTestCompleted(EventArgs.Empty);
        }

        public bool CanRun()
        {
            return true;
        }
    }

    //http://sqa.stackexchange.com/questions/2880/nunit-global-error-method-event-for-handling-exceptions
    /// <summary>
    /// Listens for test events and logs information
    /// </summary>
    public class RevitTestEventListener : EventListener
    {
        readonly DynamoRevitTest _dynamoRevitTest;

        public RevitTestEventListener(DynamoRevitTest test)
        {
            _dynamoRevitTest = test;
        }
        public void RunStarted(string name, int testCount) {}
        public void RunFinished(TestResult result) { }
        public void RunFinished(Exception exception) { }
        public void TestStarted(TestName testName) 
        {
            DynamoLogger.Instance.Log(string.Format("Starting test {0}", testName.Name));
        }
        public void TestFinished(TestResult result) 
        {
            if (result.Executed && result.IsFailure)
            {
                DynamoLogger.Instance.Log(string.Format("Test FAILED : {0}", result.Message));
                _dynamoRevitTest.ResultType = DynamoRevitTestResultType.Fail;
            }
            else if (result.Executed && result.IsSuccess)
            {
                DynamoLogger.Instance.Log("Test PASS");
                _dynamoRevitTest.ResultType = DynamoRevitTestResultType.Pass;
            }
            else if (result.Executed && result.IsError)
            {
                DynamoLogger.Instance.Log("Test ERROR");
                _dynamoRevitTest.ResultType = DynamoRevitTestResultType.Error;
            }
            else if (result.Executed && result.ResultState == ResultState.Inconclusive)
            {
                DynamoLogger.Instance.Log("Test INCONCLUSIVE");
                _dynamoRevitTest.ResultType = DynamoRevitTestResultType.Inconclusive;
            }
            _dynamoRevitTest.Message = result.Message;
        }
        public void SuiteStarted(TestName testName) { }
        public void SuiteFinished(TestResult result) { }
        public void UnhandledException(Exception exception) 
        {
            DynamoLogger.Instance.Log(exception.Message);
            _dynamoRevitTest.Message = exception.Message;
        }
        public void TestOutput(TestOutput testOutput) { }
    }

    public class DynamoRevitTestRunner:NotificationObject
    {
        ObservableCollection<DynamoRevitTest> _tests;

        public ObservableCollection<DynamoRevitTest> Results
        {
            get { return _tests; }
            set
            {
                _tests = value;
            }
        }
        
        public DelegateCommand RunAllCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        public Stopwatch Timer { get; set; }

        public string TestSummary
        {
            get
            {
                var results = (IEnumerable<DynamoRevitTest>)Results;
                var list = new List<DynamoRevitTest>(results);

                int failCount = list.Count(x => x.ResultType == DynamoRevitTestResultType.Fail);
                int passCount = list.Count(x => x.ResultType == DynamoRevitTestResultType.Pass);
                int errorCount = list.Count(x => x.ResultType == DynamoRevitTestResultType.Error);
                int exceptionCount = list.Count(x => x.ResultType == DynamoRevitTestResultType.Exception);
                int runCount = list.Count(x => x.ResultType != DynamoRevitTestResultType.Unknown);

                return (string.Format("{0} tests run. {1} passed. {2} failed. {3} exceptions. {4} total time ellapsed.",
                    new object[] { runCount, passCount, failCount, exceptionCount, Timer.Elapsed.ToString() }));
            }
        }

        public DynamoRevitTestRunner() 
        {
            _tests = new ObservableCollection<DynamoRevitTest>();
            _tests.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_results_CollectionChanged);
            Timer = new Stopwatch();
            RunAllCommand = new DelegateCommand(RunAllTests, CanRunAllTests);
            SaveCommand = new DelegateCommand(Save, CanSave);
        }

        void _results_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (DynamoRevitTest t in e.NewItems)
            {
                t.TestStarted += new TestStartedHandler(t_TestStarted);
                t.TestCompleted += new TestCompletedHandler(t_TestCompleted);
            }
            RaisePropertyChanged("Results");
        }

        void t_TestStarted(object sender, EventArgs e)
        {
            if (!Timer.IsRunning)
            {
                Timer.Start();
            }
        }

        void t_TestCompleted(object sender, EventArgs e)
        {
            Timer.Stop();
            RaisePropertyChanged("TestSummary");
        }

        public void RunAllTests()
        {
            Timer.Reset();

            foreach (DynamoRevitTest t in _tests)
            {
                t.Run();
                RaisePropertyChanged("TestSummary");
            }

            Save();
        }

        public bool CanRunAllTests()
        {
            return true;
        }

        public void Save()
        {
            try
            {
                //serialize the test results to a file
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(Results.GetType());
                string resultsDir = Path.GetDirectoryName(DynamoLogger.Instance.LogPath);
                string resultsPath = Path.Combine(resultsDir,
                                                  string.Format("dynamoRevitTests_{0}.xml", Guid.NewGuid().ToString()));
                using (TextWriter tw = new StreamWriter(resultsPath))
                {
                    x.Serialize(tw, Results);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);   
            }
        }

        public bool CanSave()
        {
            return true;
        }
    }

#endif


    public enum DynamoRevitTestResultType { Pass, Fail, Error, Exception, Unknown, Inconclusive }

    public class ResultTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DynamoRevitTestResultType resultType = (DynamoRevitTestResultType)value;
            switch (resultType)
            {
                case DynamoRevitTestResultType.Pass:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0,255,0));
                case DynamoRevitTestResultType.Fail:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255,0,0));
                case DynamoRevitTestResultType.Error:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 160, 0));
                case DynamoRevitTestResultType.Inconclusive:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 160, 0));
                case DynamoRevitTestResultType.Exception:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                case DynamoRevitTestResultType.Unknown:
                    return new SolidColorBrush(Colors.LightGray);
            }

            return System.Drawing.Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    internal class WindowHandle : IWin32Window
    {
        private readonly IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
                         "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}