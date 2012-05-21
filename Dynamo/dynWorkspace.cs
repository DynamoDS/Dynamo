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
      List<dynElement> Elements { get; }
      List<dynConnector> Connectors { get; }

      double PositionX { get; set; }
      double PositionY { get; set; }

      String Name { get; }

      void Modified();

      event Action OnModified;
   }

   public class FuncWorkspace : dynWorkspace
   {
      public List<dynElement> Elements { get; private set; }
      public List<dynConnector> Connectors { get; private set; }

      public double PositionX { get; set; }
      public double PositionY { get; set; }

      public String Name { get; set; }

      #region Contructors

      public FuncWorkspace()
         : this("", new List<dynElement>(), new List<dynConnector>(), 0, 0)
      { }

      public FuncWorkspace(String name)
         : this(name, new List<dynElement>(), new List<dynConnector>(), 0, 0)
      { }

      public FuncWorkspace(String name, double x, double y)
         : this(name, new List<dynElement>(), new List<dynConnector>(), x, y)
      { }

      public FuncWorkspace(String name, List<dynElement> e, List<dynConnector> c, double x, double y)
      {
         this.Name = name;
         this.Elements = e;
         this.Connectors = c;
         this.PositionX = x;
         this.PositionY = y;
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
      public List<dynElement> Elements { get; private set; }
      public List<dynConnector> Connectors { get; private set; }

      public double PositionX { get; set; }
      public double PositionY { get; set; }

      public String Name 
      {
         get { return "Home"; } 
      }

      #region Contructors

      public HomeWorkspace()
         : this(new List<dynElement>(), new List<dynConnector>(), 0, 0)
      { }

      public HomeWorkspace(double x, double y)
         : this(new List<dynElement>(), new List<dynConnector>(), x, y)
      { }

      public HomeWorkspace(List<dynElement> e, List<dynConnector> c, double x, double y)
      {
         this.Elements = e;
         this.Connectors = c;
         this.PositionX = x;
         this.PositionY = y;
      }

      #endregion

      public event Action OnModified;

      public void Modified()
      {
         if (OnModified != null)
            OnModified();
         dynElementSettings.SharedInstance.Bench.RunExpression(false);
      }
   }
}
