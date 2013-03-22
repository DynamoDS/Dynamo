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
			OutPortData.Add(new PortData("out","The TopographySurface element.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","The CurtainSystem created will be returned when the operation succeeds.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A set of ElementIds of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A set of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wire element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new Zone element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.View)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","If successful a SpaceTag object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds of new unplaced spaces are be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set if new unplaced spaces are be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new Space element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful the new space element is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful the new space should be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance of piping system is returned, otherwise an exception with information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance of mechanical system is returned, otherwise an exception with information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[6];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.RoofType)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.ModelCurveArray)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Structure.TrussType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Curve)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","If successful an Element Set which contains the areas should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Area)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.UV)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)((Value.String)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.AreaElemType)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Curve)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.ViewPlan)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ContFootingType)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Wall)((Value.Container)args[1]).Item;
			var result = args[2];
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
			OutPortData.Add(new PortData("out","If successful a new floor object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, an IndependentTag object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewPointBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 0 - \"Point\".",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a RoomTag object will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms created should be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms created should be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful the room is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful the new room , otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful the new room will be returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","An Element set that contains the Grids.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created sheet view.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created drafting view.",typeof(object)));
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
			OutPortData.Add(new PortData("out","if successful, a new foundation slab object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","if successful, a new floor object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","if successful, a new floor object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new floor object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new spot dimension object, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new spot dimension object, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLoadCombination and there isn't the Load Combination Element     with the same name returns an object for the newly created LoadCombination.     If such element exist and match desired one (has the same formula and the same    usages set), returns existing element. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLoadCase and there isn't the Load Case Element     with the same name returns an object for the newly created LoadCase.     If such element exist and match desired one (has the same nature and number),     returns existing element. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Usage Element with the    same name NewLoadUsage returns an object for the newly created LoadUsage.     If such element exist it returns existing element.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Nature Element with the    same name NewLoadNature returns an object for the newly created LoadNature.     If such element exist it returns existing element.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns the newly created LineLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewPointLoad returns an object for the newly created PointLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewPointLoad returns an object for the newly created PointLoad.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewPathReinforcement returns an object for the newly created Rebar.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, NewAreaReinforcement returns an object for the newly created Rebar.",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Project")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this face.")]
	public class Revit_Face_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Project()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).Project(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face and returns the intersection results.")]
	public class Revit_Face_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.IntersectionResultArray)((Value.Container)args[1]).Item;
			var arg2=((Face)(args[2] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).Intersect(arg0,out arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Intersect_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face.")]
	public class Revit_Face_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect_1()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).Intersect(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face and outputs additional results.")]
	public class Revit_Face_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","True if within this face, otherwise False.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.IntersectionResult)((Value.Container)args[1]).Item;
			var arg2=((Face)(args[2] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).IsInside(arg0,out arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_IsInside_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face.")]
	public class Revit_Face_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside_1()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","True if point is within this face, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).IsInside(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_ComputeNormal")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal vector for the face at the given point.")]
	public class Revit_Face_ComputeNormal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeNormal()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","The normal vector. This vector will be normalized.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).ComputeNormal(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the face at the specified point.")]
	public class Revit_Face_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeDerivatives()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformation containing tangent vectors and a normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).ComputeDerivatives(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_get_Period")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The period of this face in the specified parametric direction.")]
	public class Revit_Face_get_Period : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_get_Period()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the period of this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).get_Period(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_get_IsCyclic")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this face is periodic in the specified parametric direction.")]
	public class Revit_Face_get_IsCyclic : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_get_IsCyclic()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","True if this face is cyclic; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).get_IsCyclic(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Area")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The area of this face.")]
	public class Revit_Face_Area : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Area()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Reference")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the face.")]
	public class Revit_Face_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Reference()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_GetBoundingBox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the UV bounding box of the face.")]
	public class Revit_Face_GetBoundingBox : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetBoundingBox()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","A BoundingBoxUV with the extents of the parameterization of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).GetBoundingBox();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates parameters on the face.")]
	public class Revit_Face_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Evaluate()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = args[1];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Triangulate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class Revit_Face_Triangulate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Triangulate()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=((Face)(args[1] as Value.Container).Item);
			var result = args[1];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_Triangulate_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class Revit_Face_Triangulate_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Triangulate_1()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_IsTwoSided")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines if a face is two-sided (degenerate)")]
	public class Revit_Face_IsTwoSided : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsTwoSided()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_MaterialElementId")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Element ID of the material from which this face is composed.")]
	public class Revit_Face_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_MaterialElementId()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_EdgeLoops")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Each edge loop is a closed boundary of the face.")]
	public class Revit_Face_EdgeLoops : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_EdgeLoops()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_GetRegions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Face regions (created with the Split Face command) of the face.")]
	public class Revit_Face_GetRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetRegions()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = ((Face)(args[0] as Value.Container).Item).GetRegions();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Face_HasRegions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Reports if the face contains regions created with the Split Face command.")]
	public class Revit_Face_HasRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_HasRegions()
		{
			InPortData.Add(new PortData("f", "The face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Face)(args[0] as Value.Container).Item);
			var result = args[0];
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
			OutPortData.Add(new PortData("out","The newly created DividedSurface element.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created curve.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created ReferencePoint.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created ReferencePoint.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created ReferencePoint.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created symbolic curve.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, the newly created control is returned, otherwise anexception with error information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, the newly created model text is returned, otherwise anexception with error information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, the newly created opening is returned, otherwise anexception with error information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Electrical Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new pipe connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Duct Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new diameter dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","This function will modify the input singleSurfaceForm and return the same element.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful new forms are returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful form is are returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new revolution is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new Extrusion is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful the new locked alignment dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created section view.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created 3D view.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the TextNotes should be returned, otherwise Autodesk::Revit::Exceptions::InvalidOperationException will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a TextNote object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful, a TextNote object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
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
			OutPortData.Add(new PortData("out","if successful, a new plan view object within the project, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created level.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an array of new model line elements. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new model line element. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If the creation is successful, a set of ElementIds which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned.",typeof(object)));
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
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful an array of new detail curve elements. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If successful a new detail curve element. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","An empty array that can hold ReferencePoint objects.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation is successful then a new PointRelativeToPoint object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new PointOnEdgeFaceIntersection object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new PointOnEdgeEdgeIntersection object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new PointOnFace object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new PointOnPlane object with 2-dimensional Position, XVec, and Offsetproperties set to match the given 3-dimensional arguments.",typeof(object)));
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
			OutPortData.Add(new PortData("out","If creation was successful then a new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new FamilySymbolProfile object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new CurveLoopsProfile object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new Autodesk::Revit::DB::ElementId^ object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The object containing the data needed for area creation.",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[11];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[7];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var arg4=(Autodesk.Revit.DB.XYZ)((Value.Container)args[4]).Item;
			var result = args[5];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=(Autodesk.Revit.DB.WallType)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=Convert.ToBoolean(((Value.Number)args[3]).Item);
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=dynRevitUtils.ConvertFSharpListListToCurveArray(((Value.List)args[0]).Item);
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Level)((Value.Container)args[1]).Item;
			var arg2=Convert.ToBoolean(((Value.Number)args[2]).Item);
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[7];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.UV)((Value.Container)args[1]).Item;
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.Line)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Element)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			var result = args[5];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Level)((Value.Container)args[3]).Item;
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[4]).Item;
			var result = args[5];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Element)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.FamilySymbol)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)((Value.Container)args[0]).Item;
			var result = args[1];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var result = args[1];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var arg3=(Autodesk.Revit.DB.XYZ)((Value.Container)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)((Value.Container)args[0]).Item;
			var arg1=(List<double>)((Value.Container)args[1]).Item;
			var result = args[2];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[6];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[7];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.XYZ)((Value.Container)args[1]).Item;
			var arg2=(Autodesk.Revit.DB.XYZ)((Value.Container)args[2]).Item;
			var result = args[3];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Plane)((Value.Container)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=(System.Double)((Value.Number)args[2]).Item;
			var arg3=(System.Double)((Value.Number)args[3]).Item;
			var result = args[4];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
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
			var result = args[6];
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var result = args[1];
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
			OutPortData.Add(new PortData("out","If successful a new geometric plane will be returned. Otherwise",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new color object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","An empty array that can contain any CombinableElement derived objects.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new VertexIndexPairArray objects.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The new VertexIndexPair object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","An empty array that can contain any Autodesk Revit element derived objects.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The empty array of curve loops.",typeof(object)));
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
			OutPortData.Add(new PortData("out","An empty array that can hold geometric curves.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A map that maps one string to another.",typeof(object)));
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = args[0];
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
			OutPortData.Add(new PortData("out","A new unbounded line object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new bounded line object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new bounded or unbounded line object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","The newly created MaterialSet instance.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new Element Set.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
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
			OutPortData.Add(new PortData("out","A new instance of a Category Set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCategorySet();
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Clone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this curve.")]
	public class Revit_Curve_Clone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Clone()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Project")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this curve.")]
	public class Revit_Curve_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Project()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).Project(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve and returns the intersection results.")]
	public class Revit_Curve_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=(Autodesk.Revit.DB.IntersectionResultArray)((Value.Container)args[1]).Item;
			var arg2=((Curve)(args[2] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).Intersect(arg0,out arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Intersect_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve.")]
	public class Revit_Curve_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect_1()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)((Value.Container)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).Intersect(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds and outputs the end index.")]
	public class Revit_Curve_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the parameter is within the curve's bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Int32)((Value.Container)args[1]).Item;
			var arg2=((Curve)(args[2] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).IsInside(arg0,out arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_IsInside_1")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds.")]
	public class Revit_Curve_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside_1()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the parameter is within the bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).IsInside(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the curve at the specified parameter.")]
	public class Revit_Curve_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeDerivatives()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var arg2=((Curve)(args[2] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).ComputeDerivatives(arg0,arg1);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_get_Transformed")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Applies the specified transformation to this curve and returns the result.")]
	public class Revit_Curve_get_Transformed : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_get_Transformed()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformed curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)((Value.Container)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).get_Transformed(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Period")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The period of this curve.")]
	public class Revit_Curve_Period : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Period()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_IsCyclic")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this curve is cyclic.")]
	public class Revit_Curve_IsCyclic : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsCyclic()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","True if this curve is cyclic; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).IsCyclic;
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Distance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the shortest distance from the specified point to this curve.")]
	public class Revit_Curve_Distance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Distance()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the shortest distance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)((Value.Container)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).Distance(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Length")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The exact length of the curve.")]
	public class Revit_Curve_Length : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Length()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_ApproximateLength")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The approximate length of the curve.")]
	public class Revit_Curve_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ApproximateLength()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_ComputeRawParameter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the raw parameter from the normalized parameter.")]
	public class Revit_Curve_ComputeRawParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeRawParameter()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the raw curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).ComputeRawParameter(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_ComputeNormalizedParameter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the normalized curve parameter from the raw parameter.")]
	public class Revit_Curve_ComputeNormalizedParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeNormalizedParameter()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the normalized curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).ComputeNormalizedParameter(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_MakeUnbound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Makes this curve unbound.")]
	public class Revit_Curve_MakeUnbound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_MakeUnbound()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_MakeBound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Changes the bounds of this curve to the specified values.")]
	public class Revit_Curve_MakeBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_MakeBound()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=(System.Double)((Value.Number)args[1]).Item;
			var arg2=((Curve)(args[2] as Value.Container).Item);
			var result = args[2];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_get_EndParameter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The parameter of the start or the end point of the curve.")]
	public class Revit_Curve_get_EndParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_get_EndParameter()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = args[1];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_get_EndPointReference")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the start point or the end point of the curve.")]
	public class Revit_Curve_get_EndPointReference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_get_EndPointReference()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","Reference to the point or",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = ((Curve)(args[0] as Value.Container).Item).get_EndPointReference(arg0);
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_get_EndPoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The start or the end point of this curve.")]
	public class Revit_Curve_get_EndPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_get_EndPoint()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)((Value.Number)args[0]).Item;
			var arg1=((Curve)(args[1] as Value.Container).Item);
			var result = args[1];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Reference")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the curve.")]
	public class Revit_Curve_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Reference()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the curve.")]
	public class Revit_Curve_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Evaluate()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)((Value.Number)args[0]).Item;
			var arg1=Convert.ToBoolean(((Value.Number)args[1]).Item);
			var arg2=((Curve)(args[2] as Value.Container).Item);
			var result = args[2];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_Tessellate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Valid only if the curve is bound. Returns a polyline approximation to the curve.")]
	public class Revit_Curve_Tessellate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Tessellate()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	[NodeName("Revit Curve_IsBound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Describes whether the parameter of the curve is restricted to a particular interval.")]
	public class Revit_Curve_IsBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsBound()
		{
			InPortData.Add(new PortData("crv", "The curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=((Curve)(args[0] as Value.Container).Item);
			var result = args[0];
			return Value.NewContainer(result);
		}
	}

	}
