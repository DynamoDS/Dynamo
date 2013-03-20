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
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TextAlignFlags",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TextNoteLeaderTypes",typeof(object)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TextNoteLeaderStyles",typeof(object)));
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
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.TextAlignFlags",typeof(object)));
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
