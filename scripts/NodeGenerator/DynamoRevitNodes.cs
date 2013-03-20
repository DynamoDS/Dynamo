using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
namespace Dynamo.Nodes
{
	[NodeName("Revit TopographySurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TopographySurface element in the document, and initializes it with a set of points.")]
	public class Revit_TopographySurface : dynNodeWithOneOutput
	{
		public Revit_TopographySurface()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new TopographySurface element in the document, and initializes it with a set of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTopographySurface(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TakeoffFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an takeoff fitting into the Autodesk Revit document,using one connector and one MEP curve.")]
	public class Revit_TakeoffFitting : dynNodeWithOneOutput
	{
		public Revit_TakeoffFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("mepcrv", "Autodesk.Revit.DB.MEPCurve",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of an takeoff fitting into the Autodesk Revit document,using one connector and one MEP curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.MEPCurve)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTakeoffFitting(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit UnionFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an union fitting into the Autodesk Revit document,using two connectors.")]
	public class Revit_UnionFitting : dynNodeWithOneOutput
	{
		public Revit_UnionFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of an union fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewUnionFitting(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CrossFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a cross fitting into the Autodesk Revit document,using four connectors.")]
	public class Revit_CrossFitting : dynNodeWithOneOutput
	{
		public Revit_CrossFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of a cross fitting into the Autodesk Revit document,using four connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Connector)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Connector)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCrossFitting(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TransitionFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an transition fitting into the Autodesk Revit document,using two connectors.")]
	public class Revit_TransitionFitting : dynNodeWithOneOutput
	{
		public Revit_TransitionFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of an transition fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTransitionFitting(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TeeFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a tee fitting into the Autodesk Revit document,using three connectors.")]
	public class Revit_TeeFitting : dynNodeWithOneOutput
	{
		public Revit_TeeFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of a tee fitting into the Autodesk Revit document,using three connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Connector)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTeeFitting(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElbowFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an elbow fitting into the Autodesk Revit document,using two connectors.")]
	public class Revit_ElbowFitting : dynNodeWithOneOutput
	{
		public Revit_ElbowFitting()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance of an elbow fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewElbowFitting(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using two connector, and flexible pipe type.")]
	public class Revit_FlexPipe : dynNodeWithOneOutput
	{
		public Revit_FlexPipe()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("fpt", "Autodesk.Revit.DB.Plumbing.FlexPipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using two connector, and flexible pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexPipe_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a connector, point array and pipe type.")]
	public class Revit_FlexPipe_1 : dynNodeWithOneOutput
	{
		public Revit_FlexPipe_1()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("fpt", "Autodesk.Revit.DB.Plumbing.FlexPipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using a connector, point array and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexPipe_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a point array and pipe type.")]
	public class Revit_FlexPipe_2 : dynNodeWithOneOutput
	{
		public Revit_FlexPipe_2()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("fpt", "Autodesk.Revit.DB.Plumbing.FlexPipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using a point array and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Plumbing.FlexPipeType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Pipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document,  using two connectors and duct type.")]
	public class Revit_Pipe : dynNodeWithOneOutput
	{
		public Revit_Pipe()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("pt", "Autodesk.Revit.DB.Plumbing.PipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document,  using two connectors and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Pipe_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using a point, connector and pipe type.")]
	public class Revit_Pipe_1 : dynNodeWithOneOutput
	{
		public Revit_Pipe_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("pt", "Autodesk.Revit.DB.Plumbing.PipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document, using a point, connector and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Pipe_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using two points and pipe type.")]
	public class Revit_Pipe_2 : dynNodeWithOneOutput
	{
		public Revit_Pipe_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("pt", "Autodesk.Revit.DB.Plumbing.PipeType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document, using two points and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using two connector, and duct type.")]
	public class Revit_FlexDuct : dynNodeWithOneOutput
	{
		public Revit_FlexDuct()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.FlexDuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using two connector, and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexDuct_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a connector, point array and duct type.")]
	public class Revit_FlexDuct_1 : dynNodeWithOneOutput
	{
		public Revit_FlexDuct_1()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.FlexDuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using a connector, point array and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FlexDuct_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a point array and duct type.")]
	public class Revit_FlexDuct_2 : dynNodeWithOneOutput
	{
		public Revit_FlexDuct_2()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.FlexDuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using a point array and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Mechanical.FlexDuctType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Duct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two connectors and duct type.")]
	public class Revit_Duct : dynNodeWithOneOutput
	{
		public Revit_Duct()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.DuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using two connectors and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Duct_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using a point, connector and duct type.")]
	public class Revit_Duct_1 : dynNodeWithOneOutput
	{
		public Revit_Duct_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.DuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using a point, connector and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Connector)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Duct_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two points and duct type.")]
	public class Revit_Duct_2 : dynNodeWithOneOutput
	{
		public Revit_Duct_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.DuctType",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using two points and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a curve, type/symbol and reference level.")]
	public class Revit_FamilyInstance : dynNodeWithOneOutput
	{
		public Revit_FamilyInstance()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a curve, type/symbol and reference level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstance_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location,type/symbol and a base level.")]
	public class Revit_FamilyInstance_1 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstance_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a location,type/symbol and a base level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstance_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a base level.")]
	public class Revit_FamilyInstance_2 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstance_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a base level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Level)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Fascia")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference.")]
	public class Revit_Fascia : dynNodeWithOneOutput
	{
		public Revit_Fascia()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.FasciaType",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a fascia along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Fascia_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference array.")]
	public class Revit_Fascia_1 : dynNodeWithOneOutput
	{
		public Revit_Fascia_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.FasciaType",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a fascia along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Gutter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference.")]
	public class Revit_Gutter : dynNodeWithOneOutput
	{
		public Revit_Gutter()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.GutterType",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a gutter along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Gutter_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference array.")]
	public class Revit_Gutter_1 : dynNodeWithOneOutput
	{
		public Revit_Gutter_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.GutterType",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a gutter along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SlabEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference.")]
	public class Revit_SlabEdge : dynNodeWithOneOutput
	{
		public Revit_SlabEdge()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SlabEdgeType",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a slab edge along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SlabEdge_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference array.")]
	public class Revit_SlabEdge_1 : dynNodeWithOneOutput
	{
		public Revit_SlabEdge_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SlabEdgeType",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a slab edge along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurtainSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of faces.")]
	public class Revit_CurtainSystem : dynNodeWithOneOutput
	{
		public Revit_CurtainSystem()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FaceArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CurtainSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of faces.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FaceArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurtainSystem2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class Revit_CurtainSystem2 : dynNodeWithOneOutput
	{
		public Revit_CurtainSystem2()
		{
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CurtainSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of face references.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem2(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurtainSystem_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class Revit_CurtainSystem_1 : dynNodeWithOneOutput
	{
		public Revit_CurtainSystem_1()
		{
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CurtainSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of face references.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wire")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new wire element.")]
	public class Revit_Wire : dynNodeWithOneOutput
	{
		public Revit_Wire()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Electrical.WireType",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Electrical.WiringType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new wire element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.View)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Connector)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Connector)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Electrical.WireType)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Electrical.WiringType)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewWire(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Zone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Zone element.")]
	public class Revit_Zone : dynNodeWithOneOutput
	{
		public Revit_Zone()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new Zone element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewZone(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RoomBoundaryLines")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Room border.")]
	public class Revit_RoomBoundaryLines : dynNodeWithOneOutput
	{
		public Revit_RoomBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Room border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoomBoundaryLines(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpaceBoundaryLines")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Space border.")]
	public class Revit_SpaceBoundaryLines : dynNodeWithOneOutput
	{
		public Revit_SpaceBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Space border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaceBoundaryLines(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpaceTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new SpaceTag. ")]
	public class Revit_SpaceTag : dynNodeWithOneOutput
	{
		public Revit_SpaceTag()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mechanical.Space",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new SpaceTag. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mechanical.Space)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaceTag(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Spaces2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a set of new unplaced spaces on a given phase. ")]
	public class Revit_Spaces2 : dynNodeWithOneOutput
	{
		public Revit_Spaces2()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a set of new unplaced spaces on a given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces2(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Spaces")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a set of new unplaced spaces on a given phase. ")]
	public class Revit_Spaces : dynNodeWithOneOutput
	{
		public Revit_Spaces()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a set of new unplaced spaces on a given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Spaces2_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new spaces on the available plan circuits of a the given level. ")]
	public class Revit_Spaces2_1 : dynNodeWithOneOutput
	{
		public Revit_Spaces2_1()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new spaces on the available plan circuits of a the given level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces2(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Spaces_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new spaces on the available plan circuits of a the given level. ")]
	public class Revit_Spaces_2 : dynNodeWithOneOutput
	{
		public Revit_Spaces_2()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new spaces on the available plan circuits of a the given level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Space")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level, at the given location, and assigned to the given phase.")]
	public class Revit_Space : dynNodeWithOneOutput
	{
		public Revit_Space()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new space element on the given level, at the given location, and assigned to the given phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.UV)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Space_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level at the given location.")]
	public class Revit_Space_1 : dynNodeWithOneOutput
	{
		public Revit_Space_1()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new space element on the given level at the given location.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Space_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new unplaced space on a given phase. ")]
	public class Revit_Space_2 : dynNodeWithOneOutput
	{
		public Revit_Space_2()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new unplaced space on a given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PipingSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP piping system element.")]
	public class Revit_PipingSystem : dynNodeWithOneOutput
	{
		public Revit_PipingSystem()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConnectorSet",typeof(object)));
			InPortData.Add(new PortData("pst", "Autodesk.Revit.DB.Plumbing.PipeSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new MEP piping system element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ConnectorSet)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeSystemType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPipingSystem(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit MechanicalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP mechanical system element.")]
	public class Revit_MechanicalSystem : dynNodeWithOneOutput
	{
		public Revit_MechanicalSystem()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConnectorSet",typeof(object)));
			InPortData.Add(new PortData("dst", "Autodesk.Revit.DB.Mechanical.DuctSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new MEP mechanical system element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ConnectorSet)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctSystemType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewMechanicalSystem(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElectricalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from a set of electrical components.")]
	public class Revit_ElectricalSystem : dynNodeWithOneOutput
	{
		public Revit_ElectricalSystem()
		{
			InPortData.Add(new PortData("val", "List<Autodesk.Revit.DB.ElementId>",typeof(object)));
			InPortData.Add(new PortData("ett", "Autodesk.Revit.DB.Electrical.ElectricalSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from a set of electrical components.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElectricalSystem_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from a set of electrical components.")]
	public class Revit_ElectricalSystem_1 : dynNodeWithOneOutput
	{
		public Revit_ElectricalSystem_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementSet",typeof(object)));
			InPortData.Add(new PortData("ett", "Autodesk.Revit.DB.Electrical.ElectricalSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from a set of electrical components.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElectricalSystem_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from an unused Connector.")]
	public class Revit_ElectricalSystem_2 : dynNodeWithOneOutput
	{
		public Revit_ElectricalSystem_2()
		{
			InPortData.Add(new PortData("con", "Autodesk.Revit.DB.Connector",typeof(object)));
			InPortData.Add(new PortData("ett", "Autodesk.Revit.DB.Electrical.ElectricalSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from an unused Connector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ExtrusionRoof")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Extrusion Roof.")]
	public class Revit_ExtrusionRoof : dynNodeWithOneOutput
	{
		public Revit_ExtrusionRoof()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePlane",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RoofType",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new Extrusion Roof.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ReferencePlane)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.RoofType)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(System.Double)((Value.Number)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewExtrusionRoof(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FootPrintRoof")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new FootPrintRoof element.")]
	public class Revit_FootPrintRoof : dynNodeWithOneOutput
	{
		public Revit_FootPrintRoof()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RoofType",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurveArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new FootPrintRoof element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.RoofType)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.ModelCurveArray)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFootPrintRoof(arg0,arg1,arg2,out arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Truss")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a New Truss.")]
	public class Revit_Truss : dynNodeWithOneOutput
	{
		public Revit_Truss()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TrussType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a New Truss.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Structure.TrussType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Curve)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTruss(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Areas")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new areas")]
	public class Revit_Areas : dynNodeWithOneOutput
	{
		public Revit_Areas()
		{
			InPortData.Add(new PortData("val", "List<Autodesk.Revit.Creation.AreaCreationData>",typeof(object)));
			OutPortData.Add(new PortData("out","Creates new areas",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.AreaCreationData>)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreas(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Area")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new area")]
	public class Revit_Area : dynNodeWithOneOutput
	{
		public Revit_Area()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.ViewPlan",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new area",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewArea(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new area tag.")]
	public class Revit_AreaTag : dynNodeWithOneOutput
	{
		public Revit_AreaTag()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.ViewPlan",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Area",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new area tag.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Area)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.UV)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaTag(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaViewPlan")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new view for the new area.")]
	public class Revit_AreaViewPlan : dynNodeWithOneOutput
	{
		public Revit_AreaViewPlan()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AreaElemType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new view for the new area.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.AreaElemType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaViewPlan(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaBoundaryLine")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Area border.")]
	public class Revit_AreaBoundaryLine : dynNodeWithOneOutput
	{
		public Revit_AreaBoundaryLine()
		{
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.ViewPlan",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Area border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Curve)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryLine(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FoundationWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new continuous footing object.")]
	public class Revit_FoundationWall : dynNodeWithOneOutput
	{
		public Revit_FoundationWall()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ContFootingType",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new continuous footing object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ContFootingType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Wall)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFoundationWall(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Slab")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a slab within the project with the given horizontal profile using the default floor style.")]
	public class Revit_Slab : dynNodeWithOneOutput
	{
		public Revit_Slab()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a slab within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Line)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewSlab(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Tag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new IndependentTag Element. ")]
	public class Revit_Tag : dynNodeWithOneOutput
	{
		public Revit_Tag()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TagMode",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TagOrientation",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new IndependentTag Element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Element)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.TagMode)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.TagOrientation)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewTag(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new opening in a roof, floor and ceiling. ")]
	public class Revit_Opening : dynNodeWithOneOutput
	{
		public Revit_Opening()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new opening in a roof, floor and ceiling. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a rectangular opening on a wall. ")]
	public class Revit_Opening_1 : dynNodeWithOneOutput
	{
		public Revit_Opening_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a rectangular opening on a wall. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new shaft opening between a set of levels. ")]
	public class Revit_Opening_2 : dynNodeWithOneOutput
	{
		public Revit_Opening_2()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new shaft opening between a set of levels. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new opening in a beam, brace and column. ")]
	public class Revit_Opening_3 : dynNodeWithOneOutput
	{
		public Revit_Opening_3()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.eRefFace",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new opening in a beam, brace and column. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.Creation.eRefFace)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Area BoundaryConditions element on a host element. ")]
	public class Revit_AreaBoundaryConditions : dynNodeWithOneOutput
	{
		public Revit_AreaBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new Area BoundaryConditions element on a host element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Line BoundaryConditions element on a host element. ")]
	public class Revit_LineBoundaryConditions : dynNodeWithOneOutput
	{
		public Revit_LineBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new Line BoundaryConditions element on a host element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[7]).Item;
			var arg8=(System.Double)((Value.Number)args[8]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaBoundaryConditions_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Area BoundaryConditions element on a reference. ")]
	public class Revit_AreaBoundaryConditions_1 : dynNodeWithOneOutput
	{
		public Revit_AreaBoundaryConditions_1()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new Area BoundaryConditions element on a reference. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineBoundaryConditions_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Line BoundaryConditions element on a reference. ")]
	public class Revit_LineBoundaryConditions_2 : dynNodeWithOneOutput
	{
		public Revit_LineBoundaryConditions_2()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new Line BoundaryConditions element on a reference. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[7]).Item;
			var arg8=(System.Double)((Value.Number)args[8]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Point BoundaryConditions Element. ")]
	public class Revit_PointBoundaryConditions : dynNodeWithOneOutput
	{
		public Revit_PointBoundaryConditions()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.TranslationRotationValue",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new Point BoundaryConditions Element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[7]).Item;
			var arg8=(System.Double)((Value.Number)args[8]).Item;
			var arg9=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[9]).Item;
			var arg10=(System.Double)((Value.Number)args[10]).Item;
			var arg11=(Autodesk.Revit.DB.Structure.TranslationRotationValue)((Value.Container)args[11]).Item;
			var arg12=(System.Double)((Value.Number)args[12]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPointBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10,arg11,arg12);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BeamSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new BeamSystem with specified profile curves. ")]
	public class Revit_BeamSystem : dynNodeWithOneOutput
	{
		public Revit_BeamSystem()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BeamSystem_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new 2D BeamSystem with specified profile curves. ")]
	public class Revit_BeamSystem_1 : dynNodeWithOneOutput
	{
		public Revit_BeamSystem_1()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new 2D BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BeamSystem_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new BeamSystem with specified profile curves.")]
	public class Revit_BeamSystem_2 : dynNodeWithOneOutput
	{
		public Revit_BeamSystem_2()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BeamSystem_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new BeamSystem with specified profile curves. ")]
	public class Revit_BeamSystem_3 : dynNodeWithOneOutput
	{
		public Revit_BeamSystem_3()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RoomTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new RoomTag. ")]
	public class Revit_RoomTag : dynNodeWithOneOutput
	{
		public Revit_RoomTag()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.Room",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new RoomTag. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoomTag(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new unplaced rooms in the given phase. ")]
	public class Revit_Rooms2 : dynNodeWithOneOutput
	{
		public Revit_Rooms2()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new unplaced rooms in the given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms2_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new rooms in each plan circuit found in the given level in the given phase. ")]
	public class Revit_Rooms2_1 : dynNodeWithOneOutput
	{
		public Revit_Rooms2_1()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new rooms in each plan circuit found in the given level in the given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms2_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new rooms in each plan circuit found in the given level in the last phase. ")]
	public class Revit_Rooms2_2 : dynNodeWithOneOutput
	{
		public Revit_Rooms2_2()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new rooms in each plan circuit found in the given level in the last phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new unplaced rooms in the given phase. ")]
	public class Revit_Rooms : dynNodeWithOneOutput
	{
		public Revit_Rooms()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new unplaced rooms in the given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new rooms in each plan circuit found in the given level in the given phase. ")]
	public class Revit_Rooms_1 : dynNodeWithOneOutput
	{
		public Revit_Rooms_1()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new rooms in each plan circuit found in the given level in the given phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Phase)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new rooms in each plan circuit found in the given level in the last phase. ")]
	public class Revit_Rooms_2 : dynNodeWithOneOutput
	{
		public Revit_Rooms_2()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new rooms in each plan circuit found in the given level in the last phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Rooms_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new rooms using the specified placement data. ")]
	public class Revit_Rooms_3 : dynNodeWithOneOutput
	{
		public Revit_Rooms_3()
		{
			InPortData.Add(new PortData("val", "List<Autodesk.Revit.Creation.RoomCreationData>",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates new rooms using the specified placement data. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RoomCreationData>)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Room")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new room within the confines of a plan circuit, or places an unplaced room within the confines of the plan circuit. ")]
	public class Revit_Room : dynNodeWithOneOutput
	{
		public Revit_Room()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Architecture.Room",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PlanCircuit",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new room within the confines of a plan circuit, or places an unplaced room within the confines of the plan circuit. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.PlanCircuit)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Room_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new unplaced room and with an assigned phase. ")]
	public class Revit_Room_1 : dynNodeWithOneOutput
	{
		public Revit_Room_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new unplaced room and with an assigned phase. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Room_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new room on a level at a specified point. ")]
	public class Revit_Room_2 : dynNodeWithOneOutput
	{
		public Revit_Room_2()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new room on a level at a specified point. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Grids")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new grid lines.")]
	public class Revit_Grids : dynNodeWithOneOutput
	{
		public Revit_Grids()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates new grid lines.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewGrids(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Grid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new radial grid line. ")]
	public class Revit_Grid : dynNodeWithOneOutput
	{
		public Revit_Grid()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new radial grid line. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewGrid(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Grid_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new linear grid line. ")]
	public class Revit_Grid_1 : dynNodeWithOneOutput
	{
		public Revit_Grid_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new linear grid line. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewGrid(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ViewSheet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sheet view.")]
	public class Revit_ViewSheet : dynNodeWithOneOutput
	{
		public Revit_ViewSheet()
		{
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new sheet view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewViewSheet(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FoundationSlab")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level. ")]
	public class Revit_FoundationSlab : dynNodeWithOneOutput
	{
		public Revit_FoundationSlab()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FloorType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FloorType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFoundationSlab(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Floor")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector. ")]
	public class Revit_Floor : dynNodeWithOneOutput
	{
		public Revit_Floor()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FloorType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FloorType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Floor_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a floor within the project with the given horizontal profile and floor style on the specified level. ")]
	public class Revit_Floor_1 : dynNodeWithOneOutput
	{
		public Revit_Floor_1()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FloorType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile and floor style on the specified level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FloorType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Floor_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a floor within the project with the given horizontal profile using the default floor style.")]
	public class Revit_Floor_2 : dynNodeWithOneOutput
	{
		public Revit_Floor_2()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Walls")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates profile walls within the project.")]
	public class Revit_Walls : dynNodeWithOneOutput
	{
		public Revit_Walls()
		{
			InPortData.Add(new PortData("val", "List<Autodesk.Revit.Creation.ProfiledWallCreationData>",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates profile walls within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.ProfiledWallCreationData>)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewWalls(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Walls_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates rectangular walls within the project.")]
	public class Revit_Walls_1 : dynNodeWithOneOutput
	{
		public Revit_Walls_1()
		{
			InPortData.Add(new PortData("val", "List<Autodesk.Revit.Creation.RectangularWallCreationData>",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates rectangular walls within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RectangularWallCreationData>)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewWalls(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a non rectangular profile wall within the project using the specified wall type and normal vector.")]
	public class Revit_Wall : dynNodeWithOneOutput
	{
		public Revit_Wall()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the specified wall type and normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a non rectangular profile wall within the project using the specified wall type.")]
	public class Revit_Wall_1 : dynNodeWithOneOutput
	{
		public Revit_Wall_1()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the specified wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a non rectangular profile wall within the project using the default wall type.")]
	public class Revit_Wall_2 : dynNodeWithOneOutput
	{
		public Revit_Wall_2()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the default wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.")]
	public class Revit_Wall_3 : dynNodeWithOneOutput
	{
		public Revit_Wall_3()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall_4")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new rectangular profile wall within the project using the default wall style.")]
	public class Revit_Wall_4 : dynNodeWithOneOutput
	{
		public Revit_Wall_4()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new rectangular profile wall within the project using the default wall style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpotElevation")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Generate a new spot elevation object within the project.")]
	public class Revit_SpotElevation : dynNodeWithOneOutput
	{
		public Revit_SpotElevation()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Generate a new spot elevation object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewSpotElevation(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpotCoordinate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Generate a new spot coordinate object within the project.")]
	public class Revit_SpotCoordinate : dynNodeWithOneOutput
	{
		public Revit_SpotCoordinate()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out"," Generate a new spot coordinate object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewSpotCoordinate(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadCombination")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination element     within the project.")]
	public class Revit_LoadCombination : dynNodeWithOneOutput
	{
		public Revit_LoadCombination()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("val", "System.Double[]",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LoadCaseArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LoadCombinationArray",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LoadUsageArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCombination element     within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var arg2=(System.Int32)((Value.Number)args[2]).Item;
			var arg3=(System.Double[])((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.LoadCaseArray)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.Structure.LoadCombinationArray)((Value.Container)args[5]).Item;
			var arg6=(Autodesk.Revit.DB.Structure.LoadUsageArray)((Value.Container)args[6]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCombination(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadCase")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase element within the project.")]
	public class Revit_LoadCase : dynNodeWithOneOutput
	{
		public Revit_LoadCase()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LoadNature",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Category",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCase element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Structure.LoadNature)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Category)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCase(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadUsage")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage element within the project.")]
	public class Revit_LoadUsage : dynNodeWithOneOutput
	{
		public Revit_LoadUsage()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadUsage element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLoadUsage(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadNature")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadNature element within the project.")]
	public class Revit_LoadNature : dynNodeWithOneOutput
	{
		public Revit_LoadNature()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadNature element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLoadNature(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform hosted area load with polygonal shape within the project.")]
	public class Revit_AreaLoad : dynNodeWithOneOutput
	{
		public Revit_AreaLoad()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.AreaLoadType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new uniform hosted area load with polygonal shape within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaLoad_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class Revit_AreaLoad_1 : dynNodeWithOneOutput
	{
		public Revit_AreaLoad_1()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "System.Int32[]",typeof(object)));
			InPortData.Add(new PortData("val", "System.Int32[]",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.AreaLoadType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(System.Int32[])((Value.Container)args[1]).Item;
			var arg2=(System.Int32[])((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var arg7=(Autodesk.Revit.DB.Structure.AreaLoadType)((Value.Container)args[7]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaLoad_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class Revit_AreaLoad_2 : dynNodeWithOneOutput
	{
		public Revit_AreaLoad_2()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("val", "System.Int32[]",typeof(object)));
			InPortData.Add(new PortData("val", "System.Int32[]",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.AreaLoadType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(System.Int32[])((Value.Container)args[1]).Item;
			var arg2=(System.Int32[])((Value.Container)args[2]).Item;
			var arg3=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[3]).Item;
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var arg5=(Autodesk.Revit.DB.Structure.AreaLoadType)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaLoad_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform unhosted area load with polygonal shape within the project.")]
	public class Revit_AreaLoad_3 : dynNodeWithOneOutput
	{
		public Revit_AreaLoad_3()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.AreaLoadType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new uniform unhosted area load with polygonal shape within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at an array of points.")]
	public class Revit_LineLoad : dynNodeWithOneOutput
	{
		public Revit_LineLoad()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LineLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new hosted line load within the project using data at an array of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[1]).Item;
			var arg2=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)((Value.Container)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[7]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineLoad_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at two points.")]
	public class Revit_LineLoad_1 : dynNodeWithOneOutput
	{
		public Revit_LineLoad_1()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LineLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new hosted line load within the project using data at two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[1]).Item;
			var arg2=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)((Value.Container)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[7]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineLoad_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at an array of points.")]
	public class Revit_LineLoad_2 : dynNodeWithOneOutput
	{
		public Revit_LineLoad_2()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LineLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted line load within the project using data at an array of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[1]).Item;
			var arg2=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)((Value.Container)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[7]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineLoad_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at two points.")]
	public class Revit_LineLoad_3 : dynNodeWithOneOutput
	{
		public Revit_LineLoad_3()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.LineLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted line load within the project using data at two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var arg7=Convert.ToBoolean(((Value.Number)args[7]).Item);
			var arg8=Convert.ToBoolean(((Value.Number)args[8]).Item);
			var arg9=(Autodesk.Revit.DB.Structure.LineLoadType)((Value.Container)args[9]).Item;
			var arg10=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[10]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted point load within the project.")]
	public class Revit_PointLoad : dynNodeWithOneOutput
	{
		public Revit_PointLoad()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.PointLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new hosted point load within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.Structure.PointLoadType)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointLoad_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted point load within the project.")]
	public class Revit_PointLoad_1 : dynNodeWithOneOutput
	{
		public Revit_PointLoad_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Structure.PointLoadType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted point load within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.Structure.PointLoadType)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PathReinforcement")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a Path Reinforcement element within the project")]
	public class Revit_PathReinforcement : dynNodeWithOneOutput
	{
		public Revit_PathReinforcement()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a Path Reinforcement element within the project",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewPathReinforcement(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaReinforcement")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an Area Reinforcement element within the project")]
	public class Revit_AreaReinforcement : dynNodeWithOneOutput
	{
		public Revit_AreaReinforcement()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of an Area Reinforcement element within the project",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaReinforcement(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DividedSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a DividedSurface element on one surface of another element.")]
	public class Revit_DividedSurface : dynNodeWithOneOutput
	{
		public Revit_DividedSurface()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Create a DividedSurface element on one surface of another element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDividedSurface(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurveByPoints")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a 3d curve through two or more points in an AutodeskRevit family document.")]
	public class Revit_CurveByPoints : dynNodeWithOneOutput
	{
		public Revit_CurveByPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePointArray",typeof(object)));
			OutPortData.Add(new PortData("out","Create a 3d curve through two or more points in an AutodeskRevit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePointArray)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ReferencePoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point on an existing reference in an AutodeskRevit family document.")]
	public class Revit_ReferencePoint : dynNodeWithOneOutput
	{
		public Revit_ReferencePoint()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointElementReference",typeof(object)));
			OutPortData.Add(new PortData("out","Create a reference point on an existing reference in an AutodeskRevit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointElementReference)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ReferencePoint_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point at a given location and with a givencoordinate system in an Autodesk Revit family document.")]
	public class Revit_ReferencePoint_1 : dynNodeWithOneOutput
	{
		public Revit_ReferencePoint_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			OutPortData.Add(new PortData("out","Create a reference point at a given location and with a givencoordinate system in an Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ReferencePoint_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point at a given location in an AutodeskRevit family document.")]
	public class Revit_ReferencePoint_2 : dynNodeWithOneOutput
	{
		public Revit_ReferencePoint_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Create a reference point at a given location in an AutodeskRevit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SymbolicCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a symbolic curve in an Autodesk Revit family document.")]
	public class Revit_SymbolicCurve : dynNodeWithOneOutput
	{
		public Revit_SymbolicCurve()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Create a symbolic curve in an Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSymbolicCurve(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Control")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new control into the Autodesk Revit family document.")]
	public class Revit_Control : dynNodeWithOneOutput
	{
		public Revit_Control()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ControlShape",typeof(object)));
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new control into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ControlShape)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.View)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewControl(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ModelText")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a model text in the Autodesk Revit family document.")]
	public class Revit_ModelText : dynNodeWithOneOutput
	{
		public Revit_ModelText()
		{
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			InPortData.Add(new PortData("mtt", "Autodesk.Revit.DB.ModelTextType",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("ha", "Autodesk.Revit.DB.HorizontalAlign",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Create a model text in the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.ModelTextType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.HorizontalAlign)((Value.Container)args[4]).Item;
			var arg5=(System.Double)((Value.Number)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelText(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create an opening to cut the wall or ceiling.")]
	public class Revit_Opening_1 : dynNodeWithOneOutput
	{
		public Revit_Opening_1()
		{
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			OutPortData.Add(new PortData("out","Create an opening to cut the wall or ceiling.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewOpening(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElectricalConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Electrical connector into the Autodesk Revit family document.")]
	public class Revit_ElectricalConnector : dynNodeWithOneOutput
	{
		public Revit_ElectricalConnector()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ett", "Autodesk.Revit.DB.Electrical.ElectricalSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Electrical connector into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewElectricalConnector(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PipeConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new pipe connector into the Autodesk Revit family document.")]
	public class Revit_PipeConnector : dynNodeWithOneOutput
	{
		public Revit_PipeConnector()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("pst", "Autodesk.Revit.DB.Plumbing.PipeSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new pipe connector into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Plumbing.PipeSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewPipeConnector(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DuctConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new duct connector into the Autodesk Revit family document.")]
	public class Revit_DuctConnector : dynNodeWithOneOutput
	{
		public Revit_DuctConnector()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("dst", "Autodesk.Revit.DB.Mechanical.DuctSystemType",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new duct connector into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Mechanical.DuctSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDuctConnector(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RadialDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Generate a new radial dimension object using a specified dimension type.")]
	public class Revit_RadialDimension : dynNodeWithOneOutput
	{
		public Revit_RadialDimension()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("dimt", "Autodesk.Revit.DB.DimensionType",typeof(object)));
			OutPortData.Add(new PortData("out"," Generate a new radial dimension object using a specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DiameterDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new diameter dimension object using the default dimension type.")]
	public class Revit_DiameterDimension : dynNodeWithOneOutput
	{
		public Revit_DiameterDimension()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new diameter dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDiameterDimension(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RadialDimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new radial dimension object using the default dimension type.")]
	public class Revit_RadialDimension_1 : dynNodeWithOneOutput
	{
		public Revit_RadialDimension_1()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new radial dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ArcLengthDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new arc length dimension object using the specified dimension type.")]
	public class Revit_ArcLengthDimension : dynNodeWithOneOutput
	{
		public Revit_ArcLengthDimension()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("dimt", "Autodesk.Revit.DB.DimensionType",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new arc length dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Arc)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Reference)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Reference)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ArcLengthDimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new arc length dimension object using the default dimension type.")]
	public class Revit_ArcLengthDimension_1 : dynNodeWithOneOutput
	{
		public Revit_ArcLengthDimension_1()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new arc length dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Arc)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Reference)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Reference)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AngularDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new angular dimension object using the specified dimension type.")]
	public class Revit_AngularDimension : dynNodeWithOneOutput
	{
		public Revit_AngularDimension()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("dimt", "Autodesk.Revit.DB.DimensionType",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new angular dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Arc)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Reference)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AngularDimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new angular dimension object using the default dimension type.")]
	public class Revit_AngularDimension_1 : dynNodeWithOneOutput
	{
		public Revit_AngularDimension_1()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new angular dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Arc)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Reference)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LinearDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new linear dimension object using the specified dimension type.")]
	public class Revit_LinearDimension : dynNodeWithOneOutput
	{
		public Revit_LinearDimension()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("dimt", "Autodesk.Revit.DB.DimensionType",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new linear dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LinearDimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Generate a new linear dimension object using the default dimension type.")]
	public class Revit_LinearDimension_1 : dynNodeWithOneOutput
	{
		public Revit_LinearDimension_1()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			OutPortData.Add(new PortData("out"," Generate a new linear dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FormByThickenSingleSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.")]
	public class Revit_FormByThickenSingleSurface : dynNodeWithOneOutput
	{
		public Revit_FormByThickenSingleSurface()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.Form)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByThickenSingleSurface(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FormByCap")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.")]
	public class Revit_FormByCap : dynNodeWithOneOutput
	{
		public Revit_FormByCap()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByCap(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RevolveForms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.")]
	public class Revit_RevolveForms : dynNodeWithOneOutput
	{
		public Revit_RevolveForms()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolveForms(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SweptBlendForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.")]
	public class Revit_SweptBlendForm : dynNodeWithOneOutput
	{
		public Revit_SweptBlendForm()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("arar", "Autodesk.Revit.DB.ReferenceArrayArray",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ReferenceArrayArray)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlendForm(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ExtrusionForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.")]
	public class Revit_ExtrusionForm : dynNodeWithOneOutput
	{
		public Revit_ExtrusionForm()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusionForm(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoftForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Loft operation, and add it into the Autodesk Revit family document.")]
	public class Revit_LoftForm : dynNodeWithOneOutput
	{
		public Revit_LoftForm()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("arar", "Autodesk.Revit.DB.ReferenceArrayArray",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by Loft operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArrayArray)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLoftForm(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SweptBlend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new swept blend into the family document, using a selected reference as the path.")]
	public class Revit_SweptBlend : dynNodeWithOneOutput
	{
		public Revit_SweptBlend()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new swept blend into the family document, using a selected reference as the path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SweptBlend_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new swept blend into the family document, using a curve as the path.")]
	public class Revit_SweptBlend_1 : dynNodeWithOneOutput
	{
		public Revit_SweptBlend_1()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new swept blend into the family document, using a curve as the path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.Curve)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form into the family document, using an array of selected references as a 3D path.")]
	public class Revit_Sweep : dynNodeWithOneOutput
	{
		public Revit_Sweep()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("refa", "Autodesk.Revit.DB.ReferenceArray",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("ppl", "Autodesk.Revit.DB.ProfilePlaneLocation",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new sweep form into the family document, using an array of selected references as a 3D path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.ReferenceArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[2]).Item;
			var arg3=(System.Int32)((Value.Number)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.ProfilePlaneLocation)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Sweep_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form to the family document, using a path of curve elements.")]
	public class Revit_Sweep_1 : dynNodeWithOneOutput
	{
		public Revit_Sweep_1()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("swpp", "Autodesk.Revit.DB.SweepProfile",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("ppl", "Autodesk.Revit.DB.ProfilePlaneLocation",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new sweep form to the family document, using a path of curve elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.SweepProfile)((Value.Container)args[3]).Item;
			var arg4=(System.Int32)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.ProfilePlaneLocation)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Revolution instance into the Autodesk Revit family document.")]
	public class Revit_Revolution : dynNodeWithOneOutput
	{
		public Revit_Revolution()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArrArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Revolution instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurveArrArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Line)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(System.Double)((Value.Number)args[5]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolution(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Blend instance into the Autodesk Revit family document.")]
	public class Revit_Blend : dynNodeWithOneOutput
	{
		public Revit_Blend()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Blend instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewBlend(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Extrusion instance into the Autodesk Revit family document.")]
	public class Revit_Extrusion : dynNodeWithOneOutput
	{
		public Revit_Extrusion()
		{
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArrArray",typeof(object)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Extrusion instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurveArrArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusion(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointRelativeToPoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.")]
	public class Revit_PointRelativeToPoint : dynNodeWithOneOutput
	{
		public Revit_PointRelativeToPoint()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPointRelativeToPoint(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointOnEdgeFaceIntersection")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnEdgeFaceIntersection object which is used to define the placement of a ReferencePoint given a references to edge and a reference to face.")]
	public class Revit_PointOnEdgeFaceIntersection : dynNodeWithOneOutput
	{
		public Revit_PointOnEdgeFaceIntersection()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Construct a PointOnEdgeFaceIntersection object which is used to define the placement of a ReferencePoint given a references to edge and a reference to face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdgeFaceIntersection(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointOnEdgeEdgeIntersection")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnEdgeEdgeIntersection object which is used to define the placement of a ReferencePoint given two references to edge.")]
	public class Revit_PointOnEdgeEdgeIntersection : dynNodeWithOneOutput
	{
		public Revit_PointOnEdgeEdgeIntersection()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			OutPortData.Add(new PortData("out","Construct a PointOnEdgeEdgeIntersection object which is used to define the placement of a ReferencePoint given two references to edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdgeEdgeIntersection(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointOnFace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnFace object which is used to define the placement of a ReferencePoint given a reference and a location on the face.")]
	public class Revit_PointOnFace : dynNodeWithOneOutput
	{
		public Revit_PointOnFace()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Construct a PointOnFace object which is used to define the placement of a ReferencePoint given a reference and a location on the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnFace(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointOnPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnPlane object which is used to define the placement of a ReferencePoint from its property values.")]
	public class Revit_PointOnPlane : dynNodeWithOneOutput
	{
		public Revit_PointOnPlane()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Construct a PointOnPlane object which is used to define the placement of a ReferencePoint from its property values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.UV)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnPlane(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointOnEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointOnEdge object which is used to define the placement of a ReferencePoint.")]
	public class Revit_PointOnEdge : dynNodeWithOneOutput
	{
		public Revit_PointOnEdge()
		{
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(object)));
			InPortData.Add(new PortData("loc", "Autodesk.Revit.DB.PointLocationOnCurve",typeof(object)));
			OutPortData.Add(new PortData("out","Create a PointOnEdge object which is used to define the placement of a ReferencePoint.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.PointLocationOnCurve)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdge(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilySymbolProfile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new FamilySymbolProfile object.")]
	public class Revit_FamilySymbolProfile : dynNodeWithOneOutput
	{
		public Revit_FamilySymbolProfile()
		{
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new FamilySymbolProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilySymbolProfile(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurveLoopsProfile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurveLoopsProfile object.")]
	public class Revit_CurveLoopsProfile : dynNodeWithOneOutput
	{
		public Revit_CurveLoopsProfile()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArrArray",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurveLoopsProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArrArray)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewCurveLoopsProfile(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of Area for batch creation.")]
	public class Revit_AreaCreationData : dynNodeWithOneOutput
	{
		public Revit_AreaCreationData()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.ViewPlan",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of Area for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewAreaCreationData(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TextNoteCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewTextNote() for batch creation. ")]
	public class Revit_TextNoteCreationData : dynNodeWithOneOutput
	{
		public Revit_TextNoteCreationData()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("tafs", "Autodesk.Revit.DB.TextAlignFlags",typeof(object)));
			InPortData.Add(new PortData("tnlts", "Autodesk.Revit.DB.TextNoteLeaderTypes",typeof(object)));
			InPortData.Add(new PortData("tnls", "Autodesk.Revit.DB.TextNoteLeaderStyles",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewTextNote() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)((Value.Container)args[5]).Item;
			var arg6=(Autodesk.Revit.DB.TextNoteLeaderTypes)((Value.Container)args[6]).Item;
			var arg7=(Autodesk.Revit.DB.TextNoteLeaderStyles)((Value.Container)args[7]).Item;
			var arg8=(Autodesk.Revit.DB.XYZ)((Value.Container)args[8]).Item;
			var arg9=(Autodesk.Revit.DB.XYZ)((Value.Container)args[9]).Item;
			var arg10=(System.String)((Value.String)args[10]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewTextNoteCreationData(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TextNoteCreationData_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewTextNote()  for batch creation. ")]
	public class Revit_TextNoteCreationData_1 : dynNodeWithOneOutput
	{
		public Revit_TextNoteCreationData_1()
		{
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.View",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("tafs", "Autodesk.Revit.DB.TextAlignFlags",typeof(object)));
			InPortData.Add(new PortData("s", "System.String",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewTextNote()  for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)((Value.Container)args[5]).Item;
			var arg6=(System.String)((Value.String)args[6]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewTextNoteCreationData(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ProfiledWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_ProfiledWallCreationData : dynNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewProfiledWallCreationData(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ProfiledWallCreationData_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_ProfiledWallCreationData_1 : dynNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData_1()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewProfiledWallCreationData(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ProfiledWallCreationData_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_ProfiledWallCreationData_2 : dynNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData_2()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewProfiledWallCreationData(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RectangularWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_RectangularWallCreationData : dynNodeWithOneOutput
	{
		public Revit_RectangularWallCreationData()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewRectangularWallCreationData(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RectangularWallCreationData_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_RectangularWallCreationData_1 : dynNodeWithOneOutput
	{
		public Revit_RectangularWallCreationData_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("wt", "Autodesk.Revit.DB.WallType",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var arg4=(System.Double)((Value.Number)args[4]).Item;
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var arg6=Convert.ToBoolean(((Value.Number)args[6]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewRectangularWallCreationData(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RoomCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewRoom() for batch creation. ")]
	public class Revit_RoomCreationData : dynNodeWithOneOutput
	{
		public Revit_RoomCreationData()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewRoom() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewRoomCreationData(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_1 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_1()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_2 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Element)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_3 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_3()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Level)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3,arg4);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_4")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_4 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_4()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_5")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_5 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_5()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_6")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_6 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_6()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FamilyInstanceCreationData_7")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ")]
	public class Revit_FamilyInstanceCreationData_7 : dynNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_7()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("fs", "Autodesk.Revit.DB.FamilySymbol",typeof(object)));
			InPortData.Add(new PortData("st", "Autodesk.Revit.DB.Structure.StructuralType",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewFamilyInstanceCreationData(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit UV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object by copying the supplied UV object.")]
	public class Revit_UV : dynNodeWithOneOutput
	{
		public Revit_UV()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a UV object by copying the supplied UV object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewUV(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit UV_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object representing coordinates in 2-space with supplied values.")]
	public class Revit_UV_1 : dynNodeWithOneOutput
	{
		public Revit_UV_1()
		{
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a UV object representing coordinates in 2-space with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewUV(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit XYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object by copying the supplied XYZ object.")]
	public class Revit_XYZ : dynNodeWithOneOutput
	{
		public Revit_XYZ()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a XYZ object by copying the supplied XYZ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewXYZ(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit XYZ_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object representing coordinates in 3-space with supplied values.")]
	public class Revit_XYZ_1 : dynNodeWithOneOutput
	{
		public Revit_XYZ_1()
		{
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a XYZ object representing coordinates in 3-space with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewXYZ(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BoundingBoxUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a two-dimensional rectangle with supplied values.")]
	public class Revit_BoundingBoxUV : dynNodeWithOneOutput
	{
		public Revit_BoundingBoxUV()
		{
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a two-dimensional rectangle with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewBoundingBoxUV(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit HermiteSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with specified tangency at its endpoints.")]
	public class Revit_HermiteSpline : dynNodeWithOneOutput
	{
		public Revit_HermiteSpline()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with specified tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewHermiteSpline(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit HermiteSpline_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with default tangency at its endpoints.")]
	public class Revit_HermiteSpline_1 : dynNodeWithOneOutput
	{
		public Revit_HermiteSpline_1()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with default tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewHermiteSpline(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit NurbSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.")]
	public class Revit_NurbSpline : dynNodeWithOneOutput
	{
		public Revit_NurbSpline()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("lst", "List<double>",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(List<double>)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewNurbSpline(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit NurbSpline_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric nurbSpline object.")]
	public class Revit_NurbSpline_1 : dynNodeWithOneOutput
	{
		public Revit_NurbSpline_1()
		{
			InPortData.Add(new PortData("lst", "List<Autodesk.Revit.DB.XYZ>",typeof(object)));
			InPortData.Add(new PortData("arr", "Autodesk.Revit.DB.DoubleArray",typeof(object)));
			InPortData.Add(new PortData("arr", "Autodesk.Revit.DB.DoubleArray",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric nurbSpline object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.DoubleArray)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.DoubleArray)((Value.Container)args[2]).Item;
			var arg3=(System.Int32)((Value.Number)args[3]).Item;
			var arg4=Convert.ToBoolean(((Value.Number)args[4]).Item);
			var arg5=Convert.ToBoolean(((Value.Number)args[5]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewNurbSpline(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric ellipse object.")]
	public class Revit_Ellipse : dynNodeWithOneOutput
	{
		public Revit_Ellipse()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric ellipse object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(System.Double)((Value.Number)args[5]).Item;
			var arg6=(System.Double)((Value.Number)args[6]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewEllipse(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ProjectPosition")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new project position object.")]
	public class Revit_ProjectPosition : dynNodeWithOneOutput
	{
		public Revit_ProjectPosition()
		{
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new project position object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewProjectPosition(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on three points.")]
	public class Revit_Arc : dynNodeWithOneOutput
	{
		public Revit_Arc()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on three points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewArc(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Arc_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on plane, radius, and angles.")]
	public class Revit_Arc_1 : dynNodeWithOneOutput
	{
		public Revit_Arc_1()
		{
			InPortData.Add(new PortData("p", "Autodesk.Revit.DB.Plane",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on plane, radius, and angles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Plane)((Value.Container)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewArc(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Arc_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on center, radius, unit vectors, and angles.")]
	public class Revit_Arc_2 : dynNodeWithOneOutput
	{
		public Revit_Arc_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("n", "System.Double",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on center, radius, unit vectors, and angles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var arg5=(Autodesk.Revit.DB.XYZ)((Value.Container)args[5]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewArc(arg0,arg1,arg2,arg3,arg4,arg5);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Point")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric point object.")]
	public class Revit_Point : dynNodeWithOneOutput
	{
		public Revit_Point()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric point object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPoint(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Plane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new geometric plane from a loop of planar curves. ")]
	public class Revit_Plane : dynNodeWithOneOutput
	{
		public Revit_Plane()
		{
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new geometric plane from a loop of planar curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Plane_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on a normal vector and an origin.")]
	public class Revit_Plane_1 : dynNodeWithOneOutput
	{
		public Revit_Plane_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric plane object based on a normal vector and an origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Plane_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on two coordinate vectors and an origin.")]
	public class Revit_Plane_2 : dynNodeWithOneOutput
	{
		public Revit_Plane_2()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new geometric plane object based on two coordinate vectors and an origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit VertexIndexPair")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new VertexIndexPair object.")]
	public class Revit_VertexIndexPair : dynNodeWithOneOutput
	{
		public Revit_VertexIndexPair()
		{
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new VertexIndexPair object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=(System.Int32)((Value.Number)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPair(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineUnbound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unbounded geometric line object.")]
	public class Revit_LineUnbound : dynNodeWithOneOutput
	{
		public Revit_LineUnbound()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unbounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewLineUnbound(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineBound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bounded geometric line object.")]
	public class Revit_LineBound : dynNodeWithOneOutput
	{
		public Revit_LineBound()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new bounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewLineBound(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bound or unbounded geometric line object.")]
	public class Revit_Line : dynNodeWithOneOutput
	{
		public Revit_Line()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new bound or unbounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewLine(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TypeBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type binding object containing the categories passed as a parameter.")]
	public class Revit_TypeBinding : dynNodeWithOneOutput
	{
		public Revit_TypeBinding()
		{
			InPortData.Add(new PortData("cats", "Autodesk.Revit.DB.CategorySet",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new type binding object containing the categories passed as a parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewTypeBinding(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit InstanceBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance binding object containing the categories passed as a parameter.")]
	public class Revit_InstanceBinding : dynNodeWithOneOutput
	{
		public Revit_InstanceBinding()
		{
			InPortData.Add(new PortData("cats", "Autodesk.Revit.DB.CategorySet",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance binding object containing the categories passed as a parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)((Value.Container)args[0]).Item;
			var result = dynRevitSettings.Revit.Application.Create.NewInstanceBinding(arg0);
			return Value.NewContainer(result);
		}
	}

	}
