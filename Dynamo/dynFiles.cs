using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using System.Windows.Forms;

namespace Dynamo.Elements
{
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

   #region FileWatcher

   //SJE
   //TODO: Update (or make different versions)
   [ElementName("Watch File")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Create an element for reading and watching data in a file on disk.")]
   [RequiresTransaction(false)]
   public class dynFileWatcher : dynElement
   {
      public dynFileWatcher()
      {
         this.InPortData.Add(new PortData("path", "Path to the file to create a watcher for.", typeof(FileWatcher)));
         this.OutPortData = new PortData("fw", "Instance of a FileWatcher.", typeof(FileWatcher));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         //FileStream fs;

         //int tick = 0;
         //while (!fileChanged || isFileInUse(@FilePath, out fs))
         //{
         //   Thread.Sleep(10);
         //   tick += 10;

         //   if (tick >= 5000)
         //   {
         //      throw new Exception("File watcher timeout!");
         //   }
         //}

         //StreamReader reader = new StreamReader(fs);

         //string result = reader.ReadToEnd();

         //reader.Close();

         //fileChanged = false;

         //return Expression.NewString(result);

         string fileName = ((Expression.String)args[0]).Item;

         return Expression.NewContainer(new FileWatcher(fileName));
      }

      //private bool isFileInUse(string path, out FileStream stream)
      //{
      //   try
      //   {
      //      stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      //   }
      //   catch (IOException)
      //   {
      //      //the file is unavailable because it is:
      //      //still being written to
      //      //or being processed by another thread
      //      //or does not exist (has already been processed)
      //      stream = null;
      //      return true;
      //   }

      //   //file is not locked
      //   return false;
      //}
   }

   [ElementName("Watched File Changed?")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Checks if the file watched by the given FileWatcher has changed.")]
   [RequiresTransaction(false)]
   public class dynFileWatcherChanged : dynElement
   {
      public dynFileWatcherChanged()
      {
         this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
         this.OutPortData = new PortData("changed?", "Whether or not the file has been changed.", typeof(bool));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;

         return Expression.NewNumber(watcher.Changed ? 1 : 0);
      }
   }

   //TODO: Add UI for specifying whether should error or continue (checkbox?)
   [ElementName("Wait for Change")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Waits for the specified watched file to change.")]
   [RequiresTransaction(false)]
   public class dynFileWatcherWait : dynElement
   {
      public dynFileWatcherWait()
      {
         this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
         this.InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing.", typeof(double)));
         this.OutPortData = new PortData("changed?", "True: File was changed. False: Timed out.", typeof(bool));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;
         double timeout = ((Expression.Number)args[1]).Item;

         int tick = 0;
         while (!watcher.Changed)
         {
            Thread.Sleep(10);
            tick += 10;

            if (tick >= timeout)
            {
               throw new Exception("File watcher timeout!");
            }
         }

         return Expression.NewNumber(1);
      }
   }

   [ElementName("Reset File Watcher")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Resets state of FileWatcher so that it watches again.")]
   [RequiresTransaction(false)]
   public class dynFileWatcherReset : dynElement
   {
      public dynFileWatcherReset()
      {
         this.InPortData.Add(new PortData("fw", "File Watcher to check for a change.", typeof(FileWatcher)));
         this.OutPortData = new PortData("fw", "Updated watcher.", typeof(FileWatcher));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FileWatcher watcher = (FileWatcher)((Expression.Container)args[0]).Item;

         watcher.Reset();

         return Expression.NewContainer(watcher);
      }
   }

   class FileWatcher
   {
      public bool Changed = false;

      private FileSystemWatcher watcher;

      public FileWatcher(string filePath)
      {
         this.watcher = new FileSystemWatcher(
            Path.GetDirectoryName(filePath),
            Path.GetFileName(filePath)
         );

         this.watcher.Changed += new FileSystemEventHandler(watcher_Changed);
         this.watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
         this.watcher.EnableRaisingEvents = true;
      }

      void watcher_Changed(object sender, FileSystemEventArgs e)
      {
         this.Changed = true;
      }

      public void Reset()
      {
         this.Changed = false;
      }
   }

   #endregion
}
