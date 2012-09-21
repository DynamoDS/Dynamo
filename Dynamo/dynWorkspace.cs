//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Connectors;
using System.Windows;
using Dynamo.Utilities;

namespace Dynamo.Elements
{
   public interface dynWorkspace
   {
      List<dynNode> Elements { get; }
      List<dynConnector> Connectors { get; }
      List<dynNote> Notes { get; }

      double PositionX { get; set; }
      double PositionY { get; set; }
      string FilePath { get; set; }

      String Name { get; }

      void Modified();

      event Action OnModified;
   }

   public class FuncWorkspace : dynWorkspace
   {
      public List<dynNode> Elements { get; private set; }
      public List<dynConnector> Connectors { get; private set; }
      public List<dynNote> Notes {get; private set;}
      public double PositionX { get; set; }
      public double PositionY { get; set; }
      public string FilePath { get; set; }
      public String Name { get; set; }
      public String Category { get; set; }

      #region Contructors

      public FuncWorkspace()
         : this("", "", new List<dynNode>(), new List<dynConnector>(), new List<dynNote>(), 0, 0)
      { }

      public FuncWorkspace(String name, String category)
         : this(name, category, new List<dynNode>(), new List<dynConnector>(), new List<dynNote>(), 0, 0)
      { }

      public FuncWorkspace(String name, String category, double x, double y)
         : this(name, category, new List<dynNode>(), new List<dynConnector>(), new List<dynNote>(), x, y)
      { }

      public FuncWorkspace(String name, String category, List<dynNode> e, List<dynConnector> c, List<dynNote> n, double x, double y)
      {
         this.Name = name;
         this.Category = category;
         this.Elements = e;
         this.Connectors = c;
         this.PositionX = x;
         this.PositionY = y;
         this.Notes = n;
      }

      #endregion

      public event Action OnModified;

      public void Modified()
      {
         if (OnModified != null)
            OnModified();
         dynElementSettings.SharedInstance.Bench.SaveFunction(this);
      }
   }

   public class HomeWorkspace : dynWorkspace
   {

      public List<dynNode> Elements { get; private set; }
      public List<dynConnector> Connectors { get; private set; }
      public List<dynNote> Notes {get; private set;}
      public double PositionX { get; set; }
      public double PositionY { get; set; }
      public string FilePath { get; set; }

      public String Name 
      {
         get { return "Home"; } 
      }

      #region Contructors

      public HomeWorkspace()
         : this(new List<dynNode>(), new List<dynConnector>(), new List<dynNote>(), 0, 0)
      { }

      public HomeWorkspace(double x, double y)
         : this(new List<dynNode>(), new List<dynConnector>(), new List<dynNote>(), x, y)
      { }

      public HomeWorkspace(List<dynNode> e, List<dynConnector> c, List<dynNote> n, double x, double y)
      {
         this.Elements = e;
         this.Connectors = c;
         this.PositionX = x;
         this.PositionY = y;
         this.Notes = n;
      }

      #endregion

      public event Action OnModified;

      public void Modified()
      {
         if (OnModified != null)
            OnModified();
         var bench = dynElementSettings.SharedInstance.Bench;
         if (bench.DynamicRunEnabled)
         {
            if (!bench.Running)
               bench.RunExpression(false, false);
            else
               bench.QueueRun();
         }
      }
   }
}
