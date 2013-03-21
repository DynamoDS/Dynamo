using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Revit;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
namespace Dynamo.Nodes
{
	[NodeName("Revit TopographySurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TopographySurface element in the document, and initializes it with a set of points.")]
	public class Revit_TopographySurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TopographySurface()
		{
			InPortData.Add(new PortData("lst", "An array of initial points for the surface.",typeof(object)));
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
	public class Revit_TakeoffFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TakeoffFitting()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the takeoff.",typeof(object)));
			InPortData.Add(new PortData("mepcrv", "The duct or pipe which is the trunk for the takeoff.",typeof(object)));
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
	public class Revit_UnionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UnionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the union.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the union.",typeof(object)));
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
	public class Revit_CrossFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CrossFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The fourth connector to be connected to the cross.",typeof(object)));
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
	public class Revit_TransitionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TransitionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the transition.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the transition.",typeof(object)));
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
	public class Revit_TeeFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TeeFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the tee.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the tee.",typeof(object)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the tee. This should be connected to the branch of the tee.",typeof(object)));
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
	public class Revit_ElbowFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElbowFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the elbow.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the elbow.",typeof(object)));
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
	public class Revit_FlexPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexPipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
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
	public class Revit_FlexPipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexPipe_1()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the flexible pipe, including the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
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
	public class Revit_FlexPipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexPipe_2()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe, including the end points.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
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
	public class Revit_Pipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Pipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
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
	public class Revit_Pipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Pipe_1()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
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
	public class Revit_Pipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Pipe_2()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
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
	public class Revit_FlexDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexDuct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
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
	public class Revit_FlexDuct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexDuct_1()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the duct, including the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
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
	public class Revit_FlexDuct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FlexDuct_2()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct, including the end points.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
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
	public class Revit_Duct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Duct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
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
	public class Revit_Duct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Duct_1()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
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
	public class Revit_Duct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Duct_2()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second point of the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
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
	public class Revit_FamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance()
		{
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstance_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_1()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstance_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_2()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed on the specified level.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_Fascia : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Fascia()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the fascia.",typeof(object)));
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
	public class Revit_Fascia_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Fascia_1()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the fascia.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a fascia along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Gutter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference.")]
	public class Revit_Gutter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Gutter()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the gutter.",typeof(object)));
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
	public class Revit_Gutter_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Gutter_1()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the gutter.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a gutter along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SlabEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference.")]
	public class Revit_SlabEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SlabEdge()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the slab edge.",typeof(object)));
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
	public class Revit_SlabEdge_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SlabEdge_1()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the slab edge.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a slab edge along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurtainSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of faces.")]
	public class Revit_CurtainSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurtainSystem()
		{
			InPortData.Add(new PortData("val", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
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
	public class Revit_CurtainSystem2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurtainSystem2()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of face references.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem2(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurtainSystem_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class Revit_CurtainSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurtainSystem_1()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of face references.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wire")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new wire element.")]
	public class Revit_Wire : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wire()
		{
			InPortData.Add(new PortData("crv", "The base line of the wire.",typeof(object)));
			InPortData.Add(new PortData("v", "The view in which the wire is to be visible.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector which connects with the start point connector of wire, if it is",typeof(object)));
			InPortData.Add(new PortData("con", "The connector which connects with the end point connector of wire, if it is",typeof(object)));
			InPortData.Add(new PortData("val", "Specify wire type of new created wire.",typeof(object)));
			InPortData.Add(new PortData("val", "Specify wiring type(Arc or chamfer) of new created wire.",typeof(object)));
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
	public class Revit_Zone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Zone()
		{
			InPortData.Add(new PortData("l", "The level on which the Zone is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The associative phase on which the Zone is to exist.",typeof(object)));
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
	public class Revit_RoomBoundaryLines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RoomBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(object)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Room",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Room border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewRoomBoundaryLines(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpaceBoundaryLines")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Space border.")]
	public class Revit_SpaceBoundaryLines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SpaceBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(object)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Space",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Space border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewSpaceBoundaryLines(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SpaceTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new SpaceTag. ")]
	public class Revit_SpaceTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SpaceTag()
		{
			InPortData.Add(new PortData("val", "The Space which the tag refers.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the space.",typeof(object)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(object)));
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
	public class Revit_Spaces2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Spaces2()
		{
			InPortData.Add(new PortData("val", "The phase in which the spaces are to exist.",typeof(object)));
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
	public class Revit_Spaces : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Spaces()
		{
			InPortData.Add(new PortData("val", "The phase in which the spaces are to exist.",typeof(object)));
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
	public class Revit_Spaces2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Spaces2_1()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(object)));
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

	[NodeName("Revit Spaces_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates new spaces on the available plan circuits of a the given level. ")]
	public class Revit_Spaces_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Spaces_1()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(object)));
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
	public class Revit_Space : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Space()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(object)));
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
	public class Revit_Space_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Space_1()
		{
			InPortData.Add(new PortData("l", "The level on which the space is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(object)));
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
	public class Revit_Space_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Space_2()
		{
			InPortData.Add(new PortData("val", "The phase in which the space is to exist.",typeof(object)));
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
	public class Revit_PipingSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PipingSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(object)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(object)));
			InPortData.Add(new PortData("pst", "The System type.",typeof(object)));
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
	public class Revit_MechanicalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_MechanicalSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(object)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(object)));
			InPortData.Add(new PortData("dst", "The system type.",typeof(object)));
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
	public class Revit_ElectricalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElectricalSystem()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
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
	public class Revit_ElectricalSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElectricalSystem_1()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
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
	public class Revit_ElectricalSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElectricalSystem_2()
		{
			InPortData.Add(new PortData("con", "The Connector to create this Electrical System.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
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
	public class Revit_ExtrusionRoof : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ExtrusionRoof()
		{
			InPortData.Add(new PortData("crvs", "The profile of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("val", "The work plane for the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("l", "The level of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("val", "Type of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("n", "Start the extrusion.",typeof(object)));
			InPortData.Add(new PortData("n", "End the extrusion.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new Extrusion Roof.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_FootPrintRoof : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FootPrintRoof()
		{
			InPortData.Add(new PortData("crvs", "The footprint of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("l", "The level of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("val", "Type of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("val", "An array of Model Curves corresponding to the set of Curves input in the footPrint. By knowing which Model Curve was created by each footPrint curve, you can set properties like SlopeAngle for each curve.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new FootPrintRoof element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Truss : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Truss()
		{
			InPortData.Add(new PortData("val", "The type for truss.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane where the truss is going to reside. It could be",typeof(object)));
			InPortData.Add(new PortData("crv", "The curve that represents truss's base curve.It must be a line, must not be a vertical line, and must be within the sketch plane if sketchPlane is valid.",typeof(object)));
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
	public class Revit_Areas : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Areas()
		{
			InPortData.Add(new PortData("val", "A list of AreaCreationData which wraps the creation arguments of the areas to be created.",typeof(object)));
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
	public class Revit_Area : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Area()
		{
			InPortData.Add(new PortData("v", "The view of area element.",typeof(object)));
			InPortData.Add(new PortData("uv", "The point which lies in the enclosed region of AreaBoundaryLines to put the new created Area",typeof(object)));
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
	public class Revit_AreaTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaTag()
		{
			InPortData.Add(new PortData("v", "The area view",typeof(object)));
			InPortData.Add(new PortData("val", "The position of the area tag",typeof(object)));
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
	public class Revit_AreaViewPlan : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaViewPlan()
		{
			InPortData.Add(new PortData("s", "The name of new created view",typeof(object)));
			InPortData.Add(new PortData("l", "The type of area element",typeof(object)));
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
	public class Revit_AreaBoundaryLine : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaBoundaryLine()
		{
			InPortData.Add(new PortData("sp", "The sketch plane.",typeof(object)));
			InPortData.Add(new PortData("crv", "The geometry curve on which the boundary line are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Area",typeof(object)));
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
	public class Revit_FoundationWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FoundationWall()
		{
			InPortData.Add(new PortData("val", "The ContFooting type.",typeof(object)));
			InPortData.Add(new PortData("val", "The Wall to append a ContFooting.",typeof(object)));
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
	public class Revit_Slab : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Slab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the slab.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the slab is to be placed.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line use to control the sloped angle of the slab. It should be in the same face with profile.",typeof(object)));
			InPortData.Add(new PortData("n", "The slope.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a slab within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Tag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Tag()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("el", "The host object of tag.",typeof(object)));
			InPortData.Add(new PortData("b", "Whether there will be a leader.",typeof(object)));
			InPortData.Add(new PortData("val", "The mode of the tag. Add by Category, add by Multi-Category, or add by material.",typeof(object)));
			InPortData.Add(new PortData("val", "The orientation of the tag.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The position of the tag.",typeof(object)));
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
	public class Revit_Opening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Opening()
		{
			InPortData.Add(new PortData("el", "Host element of the opening. Can be a roof, floor, or ceiling.",typeof(object)));
			InPortData.Add(new PortData("crvs", "Profile of the opening.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the profile is cut perpendicular to the intersecting face of the host. False if the profile is cut vertically.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new opening in a roof, floor and ceiling. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a rectangular opening on a wall. ")]
	public class Revit_Opening_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Opening_1()
		{
			InPortData.Add(new PortData("val", "Host element of the opening.",typeof(object)));
			InPortData.Add(new PortData("xyz", "One corner of the rectangle.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The opposite corner of the rectangle.",typeof(object)));
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
	public class Revit_Opening_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Opening_2()
		{
			InPortData.Add(new PortData("l", "bottom level",typeof(object)));
			InPortData.Add(new PortData("l", "top level",typeof(object)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new shaft opening between a set of levels. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Opening_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new opening in a beam, brace and column. ")]
	public class Revit_Opening_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Opening_3()
		{
			InPortData.Add(new PortData("el", "host element of the opening, can be a beam, brace and column.",typeof(object)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(object)));
			InPortData.Add(new PortData("val", "face on which opening is based on.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new opening in a beam, brace and column. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.Creation.eRefFace)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Area BoundaryConditions element on a host element. ")]
	public class Revit_AreaBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "A Wall, Slab or Slab Foundation to host the boundary conditions.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
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
	public class Revit_LineBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "A Beam.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(object)));
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
	public class Revit_AreaBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaBoundaryConditions_1()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference obtained from a Wall, Slab or Slab Foundation.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
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

	[NodeName("Revit LineBoundaryConditions_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new Line BoundaryConditions element on a reference. ")]
	public class Revit_LineBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineBoundaryConditions_1()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to a Beam's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical line.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(object)));
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
	public class Revit_PointBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointBoundaryConditions()
		{
			InPortData.Add(new PortData("ref", "A Geometry reference to a Beam's, Brace's or Column's analytical line end.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the Y axis.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for Y axis. Ignored if Y_Rotation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the Z axis.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for Z axis. Ignored if Y_Rotation is not \"Spring\".",typeof(object)));
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
	public class Revit_BeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem. This argument is optional – may be null.",typeof(object)));
			InPortData.Add(new PortData("b", "Whether the BeamSystem is 3D or not",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_BeamSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BeamSystem_1()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new 2D BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BeamSystem_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new BeamSystem with specified profile curves.")]
	public class Revit_BeamSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BeamSystem_2()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("b", "If the BeamSystem is 3D, the sketchPlane must be a level, oran exception will be thrown.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_BeamSystem_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BeamSystem_3()
		{
			InPortData.Add(new PortData("crvs", "The profile is the profile of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new BeamSystem with specified profile curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RoomTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new RoomTag. ")]
	public class Revit_RoomTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RoomTag()
		{
			InPortData.Add(new PortData("val", "The Room which the tag refers.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the room.",typeof(object)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(object)));
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
	public class Revit_Rooms2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms2()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(object)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(object)));
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
	public class Revit_Rooms2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms2_1()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(object)));
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
	public class Revit_Rooms2_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms2_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
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
	public class Revit_Rooms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(object)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(object)));
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
	public class Revit_Rooms_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms_1()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(object)));
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
	public class Revit_Rooms_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
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
	public class Revit_Rooms_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Rooms_3()
		{
			InPortData.Add(new PortData("val", "A list of RoomCreationData which wraps the creation arguments of the rooms to be created.",typeof(object)));
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
	public class Revit_Room : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Room()
		{
			InPortData.Add(new PortData("val", "The room which you want to locate in the circuit.  Pass",typeof(object)));
			InPortData.Add(new PortData("val", "The circuit in which you want to locate a room.",typeof(object)));
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
	public class Revit_Room_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Room_1()
		{
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(object)));
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
	public class Revit_Room_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Room_2()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location of the room on that specified level.",typeof(object)));
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
	public class Revit_Grids : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Grids()
		{
			InPortData.Add(new PortData("crvs", "The curves which represent the new grid lines.  These curves must be lines or bounded arcs.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates new grid lines.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewGrids(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Grid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new radial grid line. ")]
	public class Revit_Grid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Grid()
		{
			InPortData.Add(new PortData("arc", "An arc object that represents the location of the new grid line.",typeof(object)));
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
	public class Revit_Grid_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Grid_1()
		{
			InPortData.Add(new PortData("crv", "A line object which represents the location of the grid line.",typeof(object)));
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
	public class Revit_ViewSheet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ViewSheet()
		{
			InPortData.Add(new PortData("fs", "The titleblock family symbol to apply to this sheet.",typeof(object)));
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

	[NodeName("Revit ViewDrafting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new drafting view.")]
	public class Revit_ViewDrafting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ViewDrafting()
		{
			OutPortData.Add(new PortData("out","Creates a new drafting view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FoundationSlab")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level. ")]
	public class Revit_FoundationSlab : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FoundationSlab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Floor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Floor()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Floor_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Floor_1()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile and floor style on the specified level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Floor_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Floor_2()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a floor within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Walls")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates profile walls within the project.")]
	public class Revit_Walls : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Walls()
		{
			InPortData.Add(new PortData("val", "A list of ProfiledWallCreationData which wraps the creation arguments of the walls to be created.",typeof(object)));
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
	public class Revit_Walls_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Walls_1()
		{
			InPortData.Add(new PortData("val", "A list of RectangularWallCreationData which wraps the creation arguments of the walls to be created.",typeof(object)));
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
	public class Revit_Wall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is consideredto be inside and outside.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the specified wall type and normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Wall_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_1()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the specified wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_Wall_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_2()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a non rectangular profile wall within the project using the default wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Wall_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.")]
	public class Revit_Wall_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_3()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(object)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(object)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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
	public class Revit_Wall_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_4()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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
	public class Revit_SpotElevation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SpotElevation()
		{
			InPortData.Add(new PortData("v", "The view in which the spot elevation is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference to which the spot elevation is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point which the spot elevation evaluate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot elevation.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the spot elevation.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot elevation evaluate.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(object)));
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
	public class Revit_SpotCoordinate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SpotCoordinate()
		{
			InPortData.Add(new PortData("v", "The view in which the spot coordinate is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference to which the spot coordinate is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point which the spot coordinate evaluate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot coordinate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the spot coordinate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot coordinate evaluate.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(object)));
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
	public class Revit_LoadCombination : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadCombination()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Combination Element to create.",typeof(object)));
			InPortData.Add(new PortData("i", "LoadCombination Type Index: 0-Combination, 1-Envelope.",typeof(object)));
			InPortData.Add(new PortData("i", "LoadCombination State Index: 0-Servicebility, 1-Ultimate.",typeof(object)));
			InPortData.Add(new PortData("val", "Factors array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Cases array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Combinations array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Usages array.",typeof(object)));
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
	public class Revit_LoadCase : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadCase()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Case Element to create.",typeof(object)));
			InPortData.Add(new PortData("val", "The Load Case nature.",typeof(object)));
			InPortData.Add(new PortData("val", "The Load Case category.",typeof(object)));
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
	public class Revit_LoadUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadUsage()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Usage Element to create.",typeof(object)));
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
	public class Revit_LoadNature : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadNature()
		{
			InPortData.Add(new PortData("s", "The name for the Load Nature Element to create.",typeof(object)));
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
	public class Revit_AreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaLoad()
		{
			InPortData.Add(new PortData("el", "The host element (Floor or Wall) of the AreaLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
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
	public class Revit_AreaLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaLoad_1()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load.",typeof(object)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(object)));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the second reference point. Ignored if only one or two reference points are supplied.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the third reference point.  Ignored if only one or two reference points are supplied.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_AreaLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaLoad_2()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load curves.",typeof(object)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(object)));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(object)));
			InPortData.Add(new PortData("lst", "The 3d area forces applied to each of the reference points in the refPntIdxs array.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_AreaLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaLoad_3()
		{
			InPortData.Add(new PortData("lst", "Vertexes of AreaLoad shape polygon.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d area force.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
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
	public class Revit_LineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical lines.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
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
	public class Revit_LineLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineLoad_1()
		{
			InPortData.Add(new PortData("el", "The host element (Beam, Brace or Column) of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
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
	public class Revit_LineLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineLoad_2()
		{
			InPortData.Add(new PortData("lst", "The end points of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
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
	public class Revit_LineLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineLoad_3()
		{
			InPortData.Add(new PortData("xyz", "The first point of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear force in the first point.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear moment in the first point.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second point of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear force in the second point.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear moment in the second point.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
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
	public class Revit_PointLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, analytical line's end.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(object)));
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
	public class Revit_PointLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointLoad_1()
		{
			InPortData.Add(new PortData("xyz", "The point of the PointLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(object)));
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
	public class Revit_PathReinforcement : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PathReinforcement()
		{
			InPortData.Add(new PortData("el", "The element to which the Path Reinforcement belongs. The element must be a structural floor or wall.",typeof(object)));
			InPortData.Add(new PortData("crvs", "An array of curves forming a chain.  Bars will be placed orthogonal to the chain with their hook ends near the chain, offset by the side cover setting.  The curves must belong to the top face of the floor or the exterior face of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "A flag controlling the bars relative to the curves. If the curves are given in order and with consistent orientation, the bars will lie to the right of the chain if flip=false, to the left if flip=true, when viewed from above the floor or outside the wall.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of a Path Reinforcement element within the project",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.Create.NewPathReinforcement(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaReinforcement")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an Area Reinforcement element within the project")]
	public class Revit_AreaReinforcement : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaReinforcement()
		{
			InPortData.Add(new PortData("el", "The element to which the Area Reinforcement belongs. The element must be a structural floor or wall.",typeof(object)));
			InPortData.Add(new PortData("crvs", "An array of curves that define the boundary of the area. They must belong to the top face of the floor or the exterior face of the wall.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new instance of an Area Reinforcement element within the project",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.Create.NewAreaReinforcement(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RebarBarType")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of Rebar Bar Type, which defines the bar diameter, bar bend diameter and bar material of the rebar.")]
	public class Revit_RebarBarType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RebarBarType()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of Rebar Bar Type, which defines the bar diameter, bar bend diameter and bar material of the rebar.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Doc.Document.Create.NewRebarBarType();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DividedSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a DividedSurface element on one surface of another element.")]
	public class Revit_DividedSurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface()
		{
			InPortData.Add(new PortData("ref", "Reference to a surface on an existing element. The elementmust be one of the following:",typeof(object)));
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
	public class Revit_CurveByPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurveByPoints()
		{
			InPortData.Add(new PortData("val", "Two or more PointElements. The curve will interpolatethese points.",typeof(object)));
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
	public class Revit_ReferencePoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePoint()
		{
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
	public class Revit_ReferencePoint_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePoint_1()
		{
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
	public class Revit_ReferencePoint_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePoint_2()
		{
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
	public class Revit_SymbolicCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SymbolicCurve()
		{
			InPortData.Add(new PortData("crv", "The geometry curve of the newly created symbolic curve.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the symbolic curve.",typeof(object)));
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
	public class Revit_Control : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Control()
		{
			InPortData.Add(new PortData("val", "The shape of the control.",typeof(object)));
			InPortData.Add(new PortData("v", "The view in which the control is to be visible. Itmust be a FloorPlan view or a CeilingPlan view.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the control.",typeof(object)));
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
	public class Revit_ModelText : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelText()
		{
			InPortData.Add(new PortData("s", "The text to be displayed.",typeof(object)));
			InPortData.Add(new PortData("mtt", "The type of model text. If this parameter is",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane of the model text. The direction of model text is determined by the normal of the sketch plane.To extrude in the other direction set the depth value to negative.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The position of the model text. The position must lie in the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("ha", "The horizontal alignment.",typeof(object)));
			InPortData.Add(new PortData("n", "The depth of the model text.",typeof(object)));
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

	[NodeName("Revit Opening_4")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create an opening to cut the wall or ceiling.")]
	public class Revit_Opening_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Opening_4()
		{
			InPortData.Add(new PortData("el", "Host elements that new opening would lie in. The host can only be a wall or a ceiling.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created opening. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. The profiles will be projected into the host plane.",typeof(object)));
			OutPortData.Add(new PortData("out","Create an opening to cut the wall or ceiling.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewOpening(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElectricalConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Electrical connector into the Autodesk Revit family document.")]
	public class Revit_ElectricalConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElectricalConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("ett", "Indicates the system type of this new Electrical connector.",typeof(object)));
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
	public class Revit_PipeConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PipeConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("pst", "Indicates the system type of this new Pipe connector.",typeof(object)));
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
	public class Revit_DuctConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DuctConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("dst", "Indicates the system type of this new duct connector.",typeof(object)));
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
	public class Revit_RadialDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RadialDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
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
	public class Revit_DiameterDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DiameterDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the diameter dimension will lie.",typeof(object)));
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
	public class Revit_RadialDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RadialDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(object)));
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
	public class Revit_ArcLengthDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ArcLengthDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
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
	public class Revit_ArcLengthDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ArcLengthDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(object)));
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
	public class Revit_AngularDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AngularDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
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
	public class Revit_AngularDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AngularDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
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
	public class Revit_LinearDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LinearDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new linear dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LinearDimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Generate a new linear dimension object using the default dimension type.")]
	public class Revit_LinearDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LinearDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(object)));
			OutPortData.Add(new PortData("out"," Generate a new linear dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FormByThickenSingleSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.")]
	public class Revit_FormByThickenSingleSurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FormByThickenSingleSurface()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("frm", "The single-surface form element. It can have one top/bottom face or one side face.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The offset of capped solid.",typeof(object)));
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
	public class Revit_FormByCap : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FormByCap()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of the newly created cap. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByCap(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RevolveForms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.")]
	public class Revit_RevolveForms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RevolveForms()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of the newly created revolution. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("ref", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
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
	public class Revit_SweptBlendForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlendForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The path of the swept blend. The path should be 2D, where all input curves lie in one plane. If there’s more than one profile, the path should be a single curve. It’s required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created swept blend. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var arg2=dynRevitUtils.ConvertFSharpListListToReferenceArrayArray(((Value.List)args[2]).Item);
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlendForm(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ExtrusionForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.")]
	public class Revit_ExtrusionForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ExtrusionForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of extrusion. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction of extrusion, with its length the length of the extrusion. The direction must be perpendicular to the plane determined by profile. The length of vector must be non-zero.",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusionForm(arg0,arg1,arg2);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoftForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Loft operation, and add it into the Autodesk Revit family document.")]
	public class Revit_LoftForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoftForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created loft. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","Create new Form element by Loft operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArrayArray(((Value.List)args[1]).Item);
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLoftForm(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SweptBlend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new swept blend into the family document, using a selected reference as the path.")]
	public class Revit_SweptBlend : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("ref", "The path of the swept blend. The path might be a reference of single curve or edge obtained from existing geometry.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
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
	public class Revit_SweptBlend_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_1()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crv", "The path of the swept blend. The path should be a single curve.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(object)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
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
	public class Revit_Sweep : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The path of the sweep. The path should be reference of curve or edge obtained from existing geometry.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(object)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new sweep form into the family document, using an array of selected references as a 3D path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[1]).Item);
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
	public class Revit_Sweep_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_1()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The path of the sweep. The path should be 2D, where all input curves lie in one plane, and the curves are not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(object)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(object)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(object)));
			OutPortData.Add(new PortData("out","Adds a new sweep form to the family document, using a path of curve elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
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
	public class Revit_Revolution : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Revolution()
		{
			InPortData.Add(new PortData("b", "Indicates if the Revolution is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created revolution. This may containmore than one curve loop. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the revolution.  The direction of revolutionis determined by the normal for the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("crv", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Revolution instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArrayArray(((Value.List)args[1]).Item);
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
	public class Revit_Blend : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend()
		{
			InPortData.Add(new PortData("b", "Indicates if the Blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The top blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The base blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the base profile. Use this to associate the base of the blend to geometry from another element. Optional, it can be",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Blend instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewBlend(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Extrusion instance into the Autodesk Revit family document.")]
	public class Revit_Extrusion : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Extrusion()
		{
			InPortData.Add(new PortData("b", "Indicates if the Extrusion is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created Extrusion. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. All input curves must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the extrusion.  The direction of extrusionis determined by the normal for the sketch plane.  To extrude in the other direction set the end value to negative.",typeof(object)));
			InPortData.Add(new PortData("n", "The length of the extrusion.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new Extrusion instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=Convert.ToBoolean(((Value.Number)args[0]).Item);
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArrayArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusion(arg0,arg1,arg2,arg3);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Alignment")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new locked alignment into the Autodesk Revit document.")]
	public class Revit_Alignment : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Alignment()
		{
			InPortData.Add(new PortData("v", "The view that determines the orientation of the alignment.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second reference.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new locked alignment into the Autodesk Revit document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Reference)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Reference)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAlignment(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewAlignment(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ViewSection")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new section view.")]
	public class Revit_ViewSection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ViewSection()
		{
			InPortData.Add(new PortData("val", "The view volume of the section will correspond geometrically to the specified bounding box.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new section view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewViewSection(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewViewSection(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit View3D")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new 3D view.")]
	public class Revit_View3D : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_View3D()
		{
			InPortData.Add(new PortData("xyz", "The view direction - the vector pointing from the eye towards the model.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new 3D view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewView3D(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewView3D(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit TextNotes")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates TextNotes with the specified data. ")]
	public class Revit_TextNotes : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TextNotes()
		{
			InPortData.Add(new PortData("val", "A list of TextNoteCreationData which wraps the creation arguments of the TextNotes to be created.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates TextNotes with the specified data. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.TextNoteCreationData>)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNotes(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNotes(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit TextNote")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new text note with a single leader. ")]
	public class Revit_TextNote : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TextNote()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("tnlts", "The type and alignment of the leader for the note.",typeof(object)));
			InPortData.Add(new PortData("tnls", "The style of the leader for the note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the leader.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The elbow point for the leader.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new text note with a single leader. ",typeof(object)));
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
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit TextNote_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new TextNote object without a leader. ")]
	public class Revit_TextNote_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TextNote_1()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new TextNote object without a leader. ",typeof(object)));
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
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit SketchPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new sketch plane on a reference to existing planar geometry. ")]
	public class Revit_SketchPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SketchPlane()
		{
			InPortData.Add(new PortData("ref", "The planar face reference to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new sketch plane on a reference to existing planar geometry. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit SketchPlane_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new sketch plane on a planar face of existing geometry. ")]
	public class Revit_SketchPlane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SketchPlane_1()
		{
			InPortData.Add(new PortData("val", "The geometry planar face to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new sketch plane on a planar face of existing geometry. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PlanarFace)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit SketchPlane_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new sketch plane from an arbitrary geometric plane. ")]
	public class Revit_SketchPlane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SketchPlane_2()
		{
			InPortData.Add(new PortData("p", "The geometry plane to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new sketch plane from an arbitrary geometric plane. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Plane)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ReferencePlane2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new instance of ReferencePlane. ")]
	public class Revit_ReferencePlane2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePlane2()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A third point needed to define the reference plane.",typeof(object)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new instance of ReferencePlane. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.View)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePlane2(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewReferencePlane2(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ReferencePlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new instance of ReferencePlane. ")]
	public class Revit_ReferencePlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePlane()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The cut vector apply to reference plane, should perpendicular to the vector  (bubbleEnd-freeEnd).",typeof(object)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new instance of ReferencePlane. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.View)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePlane(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewReferencePlane(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ViewPlan")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a plan view based on the specified level. ")]
	public class Revit_ViewPlan : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ViewPlan()
		{
			InPortData.Add(new PortData("s", "The name for the new plan view, must be unique or",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the plan view is to be associated.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of plan view to be created.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a plan view based on the specified level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ViewPlanType)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewViewPlan(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewViewPlan(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit Level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new level. ")]
	public class Revit_Level : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Level()
		{
			InPortData.Add(new PortData("n", "The elevation to apply to the new level.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new level. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLevel(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewLevel(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ModelCurveArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates an array of new model line elements. ")]
	public class Revit_ModelCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurveArray()
		{
			InPortData.Add(new PortData("crvs", "An array containing the internal geometry curves for model lines.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates an array of new model line elements. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurveArray(arg0,arg1);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewModelCurveArray(arg0,arg1);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ModelCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new model line element. ")]
	public class Revit_ModelCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurve()
		{
			InPortData.Add(new PortData("crv", "The internal geometry curve for model line.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane this new model line resides in.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new model line element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(arg0,arg1);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewModelCurve(arg0,arg1);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit Group")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type of group.")]
	public class Revit_Group : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Group()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new type of group.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewGroup(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewGroup(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit Group_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type of group.")]
	public class Revit_Group_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Group_1()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new type of group.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewGroup(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewGroup(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstances2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates Family instances within the document.")]
	public class Revit_FamilyInstances2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstances2()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates Family instances within the document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstances2(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstances2(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstances")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates Family instances within the document.")]
	public class Revit_FamilyInstances : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstances()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates Family instances within the document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)((Value.Container)args[0]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstances(arg0);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstances(arg0);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_3")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a line based detail family instance into the Autodesk Revit document, using an line and a view where the instance should be placed.")]
	public class Revit_FamilyInstance_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_3()
		{
			InPortData.Add(new PortData("crv", "The line location of family instance. The line must in the plane of the view.",typeof(object)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the family instance.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a line based detail family instance into the Autodesk Revit document, using an line and a view where the instance should be placed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_4")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance into the Autodesk Revit document, using an origin and a view where the instance should be placed.")]
	public class Revit_FamilyInstance_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_4()
		{
			InPortData.Add(new PortData("xyz", "The origin of family instance. If created on a",typeof(object)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "The 2D view in which to place the family instance.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new family instance into the Autodesk Revit document, using an origin and a view where the instance should be placed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_5")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a line on that face for its position, and a type/symbol.")]
	public class Revit_FamilyInstance_5 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_5()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted. Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face referenced by the input Reference instance, using a line on that face for its position, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_6")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a location, reference direction, and a type/symbol.")]
	public class Revit_FamilyInstance_6 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_6()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face referenced by the input Reference instance, using a location, reference direction, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_7")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a line on that face for its position, and a type/symbol.")]
	public class Revit_FamilyInstance_7 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_7()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face of an existing element, using a line on that face for its position, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_8")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a location, reference direction, and a type/symbol.")]
	public class Revit_FamilyInstance_8 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_8()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face of an existing element, using a location, reference direction, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_9")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location and atype/symbol.")]
	public class Revit_FamilyInstance_9 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_9()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a location and atype/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_10")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, and the host element.")]
	public class Revit_FamilyInstance_10 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_10()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the FamilyInstance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, and the host element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit FamilyInstance_11")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a reference direction.")]
	public class Revit_FamilyInstance_11 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_11()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(object)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a reference direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Element)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit Dimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new linear dimension object using the specified dimension style.")]
	public class Revit_Dimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Dimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new linear dimension object using the specified dimension style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[2]).Item);
			var arg3=(Autodesk.Revit.DB.DimensionType)((Value.Container)args[3]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDimension(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDimension(arg0,arg1,arg2,arg3);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit Dimension_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new linear dimension object using the default dimension style.")]
	public class Revit_Dimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Dimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new linear dimension object using the default dimension style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=dynRevitUtils.ConvertFSharpListListToReferenceArray(((Value.List)args[2]).Item);
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDimension(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDimension(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit DetailCurveArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates an array of new detail curve elements. ")]
	public class Revit_DetailCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DetailCurveArray()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curves are to be visible.",typeof(object)));
			InPortData.Add(new PortData("crvs", "An array containing the internal geometry curves for detail lines. The curve in array should be bound curve.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates an array of new detail curve elements. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDetailCurveArray(arg0,arg1);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDetailCurveArray(arg0,arg1);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit DetailCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Creates a new detail curve element. ")]
	public class Revit_DetailCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DetailCurve()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curve is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The internal geometry curve for detail curve. It should be a bound curve.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new detail curve element. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Curve)((Value.Container)args[1]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDetailCurve(arg0,arg1);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDetailCurve(arg0,arg1);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit AnnotationSymbol")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new instance of an Annotation Symbol into the Autodesk Revit document, using an origin and a view where the instance should be placed.")]
	public class Revit_AnnotationSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AnnotationSymbol()
		{
			InPortData.Add(new PortData("xyz", "The origin of the annotation symbol. If created on",typeof(object)));
			InPortData.Add(new PortData("val", "An annotation symbol type that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the annotation symbol.",typeof(object)));
			OutPortData.Add(new PortData("out","Add a new instance of an Annotation Symbol into the Autodesk Revit document, using an origin and a view where the instance should be placed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.AnnotationSymbolType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAnnotationSymbol(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewAnnotationSymbol(arg0,arg1,arg2);
				return Value.NewContainer(result);
			}
		}
	}

	[NodeName("Revit ReferencePointArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store ReferencePoint objects.")]
	public class Revit_ReferencePointArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferencePointArray()
		{
			OutPortData.Add(new PortData("out","Creates an empty array that can store ReferencePoint objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewReferencePointArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit PointRelativeToPoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.")]
	public class Revit_PointRelativeToPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointRelativeToPoint()
		{
			InPortData.Add(new PortData("ref", "The reference of the host point.",typeof(object)));
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
	public class Revit_PointOnEdgeFaceIntersection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointOnEdgeFaceIntersection()
		{
			InPortData.Add(new PortData("ref", "The edge reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The face reference.",typeof(object)));
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
	public class Revit_PointOnEdgeEdgeIntersection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointOnEdgeEdgeIntersection()
		{
			InPortData.Add(new PortData("ref", "The first edge reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second edge reference.",typeof(object)));
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
	public class Revit_PointOnFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointOnFace()
		{
			InPortData.Add(new PortData("ref", "The reference whose face the object will be created on.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2-dimensional position.",typeof(object)));
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
	public class Revit_PointOnPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointOnPlane()
		{
			InPortData.Add(new PortData("ref", "A reference to some planein the document. (Note: the reference must satisfyIsValidPlaneReference(), but this is not checked until this PointOnPlane objectis assigned to a ReferencePoint.)",typeof(object)));
			InPortData.Add(new PortData("uv", "Coordinates of the point's projection onto the plane;see the Position property.",typeof(object)));
			InPortData.Add(new PortData("uv", "The direction of the point'sX-coordinate vector in the plane's coordinates; see the XVec property. Optional;default value is (1, 0).",typeof(object)));
			InPortData.Add(new PortData("n", "Signed offset from the plane; see the Offset property.",typeof(object)));
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
	public class Revit_PointOnEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointOnEdge()
		{
			InPortData.Add(new PortData("ref", "The reference whose edge the object will be created on.",typeof(object)));
			InPortData.Add(new PortData("loc", "The location on the edge.",typeof(object)));
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
	public class Revit_FamilySymbolProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilySymbolProfile()
		{
			InPortData.Add(new PortData("fs", "The family symbol of the Profile.",typeof(object)));
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
	public class Revit_CurveLoopsProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurveLoopsProfile()
		{
			InPortData.Add(new PortData("crvs", "The curve loops of the Profile.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates a new CurveLoopsProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArrayArray(((Value.List)args[0]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewCurveLoopsProfile(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElementId")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Autodesk::Revit::DB::ElementId^ object.")]
	public class Revit_ElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElementId()
		{
			OutPortData.Add(new PortData("out","Creates a new Autodesk::Revit::DB::ElementId^ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementId();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit AreaCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of Area for batch creation.")]
	public class Revit_AreaCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AreaCreationData()
		{
			InPortData.Add(new PortData("v", "The view of area element.",typeof(object)));
			InPortData.Add(new PortData("uv", "A point which lies in an enclosed region of area boundary where the new area will reside.",typeof(object)));
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
	public class Revit_TextNoteCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TextNoteCreationData()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("tnlts", "The type and alignment of the leader for the note.",typeof(object)));
			InPortData.Add(new PortData("tnls", "The style for the leader.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the leader.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The elbow point for the leader.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
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
	public class Revit_TextNoteCreationData_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TextNoteCreationData_1()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
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
	public class Revit_ProfiledWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_ProfiledWallCreationData_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData_1()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
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
	public class Revit_ProfiledWallCreationData_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ProfiledWallCreationData_2()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewProfiledWallCreationData(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit RectangularWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation ")]
	public class Revit_RectangularWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RectangularWallCreationData()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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
	public class Revit_RectangularWallCreationData_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RectangularWallCreationData_1()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("n", "The height of the wall.",typeof(object)));
			InPortData.Add(new PortData("n", "An offset distance, in feet from the specified baseline. The wall will be placed that distancefrom the baseline.",typeof(object)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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
	public class Revit_RoomCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RoomCreationData()
		{
			InPortData.Add(new PortData("l", "- The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point the dictates the location on that specified level.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_1()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_2()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_3()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_4()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_5 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_5()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_6 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_6()
		{
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
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
	public class Revit_FamilyInstanceCreationData_7 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstanceCreationData_7()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("st", "Specify if the family instance is structural.",typeof(object)));
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

	[NodeName("Revit SpaceSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a space set.")]
	public class Revit_SpaceSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SpaceSet()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a space set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewSpaceSet();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadCombinationArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination array.")]
	public class Revit_LoadCombinationArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadCombinationArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCombination array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewLoadCombinationArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadUsageArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage array.")]
	public class Revit_LoadUsageArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadUsageArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadUsage array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewLoadUsageArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LoadCaseArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase array.")]
	public class Revit_LoadCaseArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LoadCaseArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCase array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewLoadCaseArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ViewSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a View set.")]
	public class Revit_ViewSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ViewSet()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a View set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewViewSet();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit IntersectionResultArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an IntersectionResult array.")]
	public class Revit_IntersectionResultArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_IntersectionResultArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of an IntersectionResult array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewIntersectionResultArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FaceArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a face array.")]
	public class Revit_FaceArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FaceArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a face array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewFaceArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ReferenceArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a reference array.")]
	public class Revit_ReferenceArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ReferenceArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewReferenceArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DoubleArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a double array.")]
	public class Revit_DoubleArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DoubleArray()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a double array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewDoubleArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit VolumeCalculationOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates options related to room volume and area computations.")]
	public class Revit_VolumeCalculationOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_VolumeCalculationOptions()
		{
			OutPortData.Add(new PortData("out","Creates options related to room volume and area computations.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewVolumeCalculationOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit GBXMLImportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Import options.")]
	public class Revit_GBXMLImportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GBXMLImportOptions()
		{
			OutPortData.Add(new PortData("out","Creates Green-Building XML Import options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewGBXMLImportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ImageImportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Image Import options.")]
	public class Revit_ImageImportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ImageImportOptions()
		{
			OutPortData.Add(new PortData("out","Creates Image Import options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewImageImportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BuildingSiteExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Building Site Export options.")]
	public class Revit_BuildingSiteExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BuildingSiteExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates Building Site Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewBuildingSiteExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit FBXExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates 3D-Studio Max (FBX) Export options.")]
	public class Revit_FBXExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FBXExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates 3D-Studio Max (FBX) Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewFBXExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit GBXMLExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Export options.")]
	public class Revit_GBXMLExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GBXMLExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates Green-Building XML Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewGBXMLExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DWFXExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWFX Export options.")]
	public class Revit_DWFXExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DWFXExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates DWFX Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewDWFXExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit DWFExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWF Export options.")]
	public class Revit_DWFExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DWFExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates DWF Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewDWFExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit SATExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates SAT Export options.")]
	public class Revit_SATExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SATExportOptions()
		{
			OutPortData.Add(new PortData("out","Creates SAT Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewSATExportOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit UV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object by copying the supplied UV object.")]
	public class Revit_UV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV()
		{
			InPortData.Add(new PortData("uv", "The supplied UV object",typeof(object)));
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
	public class Revit_UV_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_1()
		{
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(object)));
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

	[NodeName("Revit UV_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object at the origin.")]
	public class Revit_UV_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_2()
		{
			OutPortData.Add(new PortData("out","Creates a UV object at the origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewUV();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit XYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object by copying the supplied XYZ object.")]
	public class Revit_XYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ()
		{
			InPortData.Add(new PortData("xyz", "The supplied XYZ object",typeof(object)));
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
	public class Revit_XYZ_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_1()
		{
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The third coordinate.",typeof(object)));
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

	[NodeName("Revit XYZ_2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object at the origin.")]
	public class Revit_XYZ_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_2()
		{
			OutPortData.Add(new PortData("out","Creates a XYZ object at the origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewXYZ();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BoundingBoxUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a two-dimensional rectangle with supplied values.")]
	public class Revit_BoundingBoxUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxUV()
		{
			InPortData.Add(new PortData("n", "The first coordinate of min.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate of min.",typeof(object)));
			InPortData.Add(new PortData("n", "The first coordinate of max.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate of max.",typeof(object)));
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

	[NodeName("Revit BoundingBoxUV_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty two-dimensional rectangle.")]
	public class Revit_BoundingBoxUV_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxUV_1()
		{
			OutPortData.Add(new PortData("out","Creates an empty two-dimensional rectangle.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewBoundingBoxUV();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit BoundingBoxXYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a three-dimensional rectangular box.")]
	public class Revit_BoundingBoxXYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxXYZ()
		{
			OutPortData.Add(new PortData("out","Creates a three-dimensional rectangular box.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewBoundingBoxXYZ();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit HermiteSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with specified tangency at its endpoints.")]
	public class Revit_HermiteSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the start of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the end of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(object)));
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
	public class Revit_HermiteSpline_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline_1()
		{
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(object)));
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
	public class Revit_NurbSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("lst", "The weights of the nurbSpline.",typeof(object)));
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
	public class Revit_NurbSpline_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_1()
		{
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("arr", "The weights of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("arr", "The knots of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("i", "The degree of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("b", "The nurbSpline is closed or not.",typeof(object)));
			InPortData.Add(new PortData("b", "The nurbSpline is rational or not rational.",typeof(object)));
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
	public class Revit_Ellipse : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse()
		{
			InPortData.Add(new PortData("xyz", "The center of the ellipse.",typeof(object)));
			InPortData.Add(new PortData("n", "The x vector radius of the ellipse. Should be > 0.",typeof(object)));
			InPortData.Add(new PortData("n", "The y vector radius of the ellipse. Should be > 0.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The x axis to define the ellipse plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The y axis to define the ellipse plane. xVec must be orthogonal with yVec.",typeof(object)));
			InPortData.Add(new PortData("n", "The raw parameter value at the start of the ellipse. Should be greater than or equal to -2PI and less than Param1.",typeof(object)));
			InPortData.Add(new PortData("n", "The raw parameter value at the end of the ellipse. Should be greater than Param0 and less than or equal to 2*PI.",typeof(object)));
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
	public class Revit_ProjectPosition : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ProjectPosition()
		{
			InPortData.Add(new PortData("n", "East to West offset in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "North to South offset in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "Elevation above sea level in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation angle away from true north in the range of -PI to +PI.",typeof(object)));
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
	public class Revit_Arc : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc()
		{
			InPortData.Add(new PortData("xyz", "The start point of the arc.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point of the arc.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A point on the arc.",typeof(object)));
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
	public class Revit_Arc_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_1()
		{
			InPortData.Add(new PortData("p", "The plane which the arc resides in. The plane's origin is the center of the arc.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(object)));
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
	public class Revit_Arc_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_2()
		{
			InPortData.Add(new PortData("xyz", "The center of the arc.",typeof(object)));
			InPortData.Add(new PortData("n", "The radius of the arc.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(object)));
			InPortData.Add(new PortData("xyz", "The x axis to define the arc plane. Must be normalized.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The y axis to define the arc plane. Must be normalized.",typeof(object)));
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
	public class Revit_Point : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Point()
		{
			InPortData.Add(new PortData("xyz", "The coordinates of the point.",typeof(object)));
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
	public class Revit_Plane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Plane()
		{
			InPortData.Add(new PortData("crvs", "The closed loop of planar curves to locate plane.",typeof(object)));
			OutPortData.Add(new PortData("out"," Creates a new geometric plane from a loop of planar curves. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Plane_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on a normal vector and an origin.")]
	public class Revit_Plane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Plane_1()
		{
			InPortData.Add(new PortData("xyz", "Z vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(object)));
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
	public class Revit_Plane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Plane_2()
		{
			InPortData.Add(new PortData("xyz", "X vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Y vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(object)));
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

	[NodeName("Revit Color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new color object.")]
	public class Revit_Color : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Color()
		{
			OutPortData.Add(new PortData("out","Returns a new color object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewColor();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CombinableElementArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold combinable element objects.")]
	public class Revit_CombinableElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CombinableElementArray()
		{
			OutPortData.Add(new PortData("out","Returns an array that can hold combinable element objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCombinableElementArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit VertexIndexPairArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold VertexIndexPair objects.")]
	public class Revit_VertexIndexPairArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_VertexIndexPairArray()
		{
			OutPortData.Add(new PortData("out","Returns an array that can hold VertexIndexPair objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPairArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit VertexIndexPair")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new VertexIndexPair object.")]
	public class Revit_VertexIndexPair : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_VertexIndexPair()
		{
			InPortData.Add(new PortData("i", "The index of the vertex pair from the top profile of a blend.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the vertex pair from the bottom profile of a blend.",typeof(object)));
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

	[NodeName("Revit ElementArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold element objects.")]
	public class Revit_ElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElementArray()
		{
			OutPortData.Add(new PortData("out","Returns an array that can hold element objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurveArrArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curve loops.")]
	public class Revit_CurveArrArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurveArrArray()
		{
			OutPortData.Add(new PortData("out","Creates an empty array that can store geometric curve loops.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArrArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CurveArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curves.")]
	public class Revit_CurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CurveArray()
		{
			OutPortData.Add(new PortData("out","Creates an empty array that can store geometric curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArray();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit StringStringMap")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new map that maps one string to another string.")]
	public class Revit_StringStringMap : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_StringStringMap()
		{
			OutPortData.Add(new PortData("out","Creates a new map that maps one string to another string.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewStringStringMap();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit GeometryOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object to specify user preferences in parsing of geometry.")]
	public class Revit_GeometryOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryOptions()
		{
			OutPortData.Add(new PortData("out","Creates an object to specify user preferences in parsing of geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewGeometryOptions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit LineUnbound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unbounded geometric line object.")]
	public class Revit_LineUnbound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineUnbound()
		{
			InPortData.Add(new PortData("xyz", "A point through which the line will pass.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector for the direction of the line.",typeof(object)));
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
	public class Revit_LineBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_LineBound()
		{
			InPortData.Add(new PortData("xyz", "A start point for the line.",typeof(object)));
			InPortData.Add(new PortData("xyz", "An end point for the line.",typeof(object)));
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
	public class Revit_Line : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Line()
		{
			InPortData.Add(new PortData("xyz", "A start point or a point through which the line will pass.",typeof(object)));
			InPortData.Add(new PortData("xyz", "An end point of a vector for the direction of the line.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish the line to be bound or False is the line is to be infinite.",typeof(object)));
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

	[NodeName("Revit MaterialSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription(" Create a new instance of MaterialSet. ")]
	public class Revit_MaterialSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_MaterialSet()
		{
			OutPortData.Add(new PortData("out"," Create a new instance of MaterialSet. ",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewMaterialSet();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit ElementSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding elements.")]
	public class Revit_ElementSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ElementSet()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a set specifically for holding elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementSet();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit TypeBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type binding object containing the categories passed as a parameter.")]
	public class Revit_TypeBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TypeBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(object)));
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

	[NodeName("Revit TypeBinding_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty type binding object.")]
	public class Revit_TypeBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_TypeBinding_1()
		{
			OutPortData.Add(new PortData("out","Creates a new empty type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewTypeBinding();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit InstanceBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance binding object containing the categories passed as a parameter.")]
	public class Revit_InstanceBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_InstanceBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(object)));
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

	[NodeName("Revit InstanceBinding_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty instance binding object.")]
	public class Revit_InstanceBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_InstanceBinding_1()
		{
			OutPortData.Add(new PortData("out","Creates a new empty instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewInstanceBinding();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit CategorySet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding category objects.")]
	public class Revit_CategorySet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CategorySet()
		{
			OutPortData.Add(new PortData("out","Creates a new instance of a set specifically for holding category objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCategorySet();
			return Value.NewContainer(result);
		}
	}

	}
