using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   //SJE
   [ElementName("Read File")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Create an element for reading and watching data in a file on disk.")]
   [RequiresTransaction(false)]
   public class dynFileReader : dynElement
   {
      public dynFileReader()
      {
         InPortData.Add(new PortData("path", "Path to the file", typeof(string)));
         OutPortData = new PortData("contents", "File contents", typeof(string));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         string arg = ((Expression.String)args[0]).Item;

         StreamReader reader = new StreamReader(new FileStream(arg, FileMode.Open, FileAccess.Read, FileShare.Read));
         string contents = reader.ReadToEnd();
         reader.Close();

         return Expression.NewString(contents);
      }
   }

   //SJE
   //TODO: Update (or make different versions)
   [ElementName("Watch File")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Create an element for reading and watching data in a file on disk.")]
   [RequiresTransaction(false)]
   public class dynFileWatcher : dynElement
   {
      System.Windows.Controls.TextBox tb;

      string dataFromFileString;
      string filePath = "";

      public string DataFromFileString
      {
         get { return dataFromFileString; }
         set
         {
            dataFromFileString = value;
            NotifyPropertyChanged("DataFromFileString");
         }
      }

      public string FilePath
      {
         get { return filePath; }
         set
         {
            filePath = value;
            NotifyPropertyChanged("FilePath");
         }
      }

      private FileSystemWatcher watcher;

      public dynFileWatcher()
      {
         StackPanel myStackPanel;

         //Define a StackPanel
         myStackPanel = new StackPanel();
         myStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
         System.Windows.Controls.Grid.SetRow(myStackPanel, 1);

         this.inputGrid.Children.Add(myStackPanel);

         //add a button to the inputGrid on the dynElement
         System.Windows.Controls.Button readFileButton = new System.Windows.Controls.Button();

         System.Windows.Controls.Grid.SetColumn(readFileButton, 0); // trying to get this button to be on top... grrr.
         System.Windows.Controls.Grid.SetRow(readFileButton, 0);
         readFileButton.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
         readFileButton.Content = "Read File";
         readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         myStackPanel.Children.Add(readFileButton);


         //add a list box
         //label = new System.Windows.Controls.Label();
         tb = new System.Windows.Controls.TextBox();
         //tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
         //tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

         DataFromFileString = "Ready to read file!";

         //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx


         //this.inputGrid.Children.Add(label);
         //this.inputGrid.Children.Add(tb);
         //tb.Visibility = System.Windows.Visibility.Hidden;
         //System.Windows.Controls.Grid.SetColumn(tb, 0);
         //System.Windows.Controls.Grid.SetRow(tb, 1);
         tb.TextWrapping = System.Windows.TextWrapping.Wrap;
         tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
         tb.Height = 100;
         //tb.AcceptsReturn = true;

         System.Windows.Data.Binding b = new System.Windows.Data.Binding("DataFromFileString");
         b.Source = this;
         tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

         myStackPanel.Children.Add(tb);
         myStackPanel.Height = 200;

         OutPortData = new PortData("contents", "downstream data", typeof(object));

         base.RegisterInputsAndOutputs();

         //resize the panel
         this.topControl.Height = 200;
         this.topControl.Width = 300;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
         //this.UpdateLayout();
      }


      void readFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         // string txtPath = "C:\\xfer\\dev\\dynamo_git\\test\\text_files\test.txt";
         System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog();

         if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            FilePath = openDialog.FileName;
            AddFileWatch(FilePath);
         }

         if (string.IsNullOrEmpty(DataFromFileString))
         {
            string fileError = "Data file could not be opened.";
            TaskDialog.Show("Error", fileError);
            dynElementSettings.SharedInstance.Writer.WriteLine(fileError);
            dynElementSettings.SharedInstance.Writer.WriteLine(FilePath);
         }
      }

      public void AddFileWatch(string filePath)
      {
         if (this.watcher != null)
         {
            this.watcher.Changed -= new FileSystemEventHandler(OnChanged);
            this.watcher.Dispose();
         }

         // Create a new FileSystemWatcher and set its properties.
         this.watcher = new FileSystemWatcher(
            Path.GetDirectoryName(filePath),
            Path.GetFileName(filePath)
         );

         try
         {
            //MDJ hard crash - threading / context issue?

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            // | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            //watcher.Filter = "*.csv";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            // watcher.Created += new FileSystemEventHandler(OnChanged);
            // watcher.Deleted += new FileSystemEventHandler(OnChanged);
            // watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            fileChanged = true;
         }

         catch (Exception e)
         {
            TaskDialog.Show("Error", e.ToString());
         }
      }

      private bool fileChanged = false;

      // mdj to do - figure out how to dispose of this FileSystemWatcher
      // Define the event handlers.
      private void OnChanged(object source, FileSystemEventArgs e)
      {
         fileChanged = true;

         // Specify what is done when a file is changed, created, or deleted.
         //this.Dispatcher.BeginInvoke(new Action(
         //   () =>
         //      dynElementSettings.SharedInstance.Bench.Log("File Changed: " + e.FullPath + " " + e.ChangeType)
         //));
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FileStream fs;

         int tick = 0;
         while (!fileChanged || isFileInUse(@FilePath, out fs))
         {
            Thread.Sleep(10);
            tick += 10;

            if (tick >= 5000)
            {
               throw new Exception("File watcher timeout!");
            }
         }

         StreamReader reader = new StreamReader(fs);

         string result = reader.ReadToEnd();

         reader.Close();

         fileChanged = false;

         return Expression.NewString(result);
      }

      private bool isFileInUse(string path, out FileStream stream)
      {
         try
         {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
         }
         catch (IOException)
         {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            stream = null;
            return true;
         }

         //file is not locked
         return false;
      }
   }
}
