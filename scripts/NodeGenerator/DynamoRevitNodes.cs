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
	[NodeName("Revit_Instance_GetTotalTransform")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The calculated total transform.")]
	public class Revit_Instance_GetTotalTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Instance_GetTotalTransform()
		{
			OutPortData.Add(new PortData("out","The calculated total transform.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Instance_GetTransform")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The inherent transform.")]
	public class Revit_Instance_GetTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Instance_GetTransform()
		{
			OutPortData.Add(new PortData("out","The inherent transform.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryInstance_GetInstanceGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An element which contains the computed geometry for the transformed instance.")]
	public class Revit_GeometryInstance_GetInstanceGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetInstanceGeometry()
		{
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(object)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the transformed instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Transform));
		}
	}
	[NodeName("Revit_GeometryInstance_GetInstanceGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An element which contains the computed geometry for the instance.")]
	public class Revit_GeometryInstance_GetInstanceGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetInstanceGeometry()
		{
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryInstance_GetSymbolGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An element which contains the computed geometry for the transformed symbol.")]
	public class Revit_GeometryInstance_GetSymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetSymbolGeometry()
		{
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(object)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the transformed symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Transform));
		}
	}
	[NodeName("Revit_GeometryInstance_GetSymbolGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An element which contains the computed geometry for the symbol.")]
	public class Revit_GeometryInstance_GetSymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetSymbolGeometry()
		{
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewDividedSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created DividedSurface element.")]
	public class Revit_FamilyItemFactory_NewDividedSurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewDividedSurface()
		{
			InPortData.Add(new PortData("ref", "Reference to a surface on an existing element. The elementmust be one of the following:",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created DividedSurface element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewCurveByPoints")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created curve.")]
	public class Revit_FamilyItemFactory_NewCurveByPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewCurveByPoints()
		{
			InPortData.Add(new PortData("val", "Two or more PointElements. The curve will interpolatethese points.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePointArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferencePointArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSymbolicCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created symbolic curve.")]
	public class Revit_FamilyItemFactory_NewSymbolicCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSymbolicCurve()
		{
			InPortData.Add(new PortData("crv", "The geometry curve of the newly created symbolic curve.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the symbolic curve.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created symbolic curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewControl")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, the newly created control is returned, otherwise anexception with error information will be thrown.")]
	public class Revit_FamilyItemFactory_NewControl : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewControl()
		{
			InPortData.Add(new PortData("val", "The shape of the control.",typeof(object)));
			InPortData.Add(new PortData("v", "The view in which the control is to be visible. Itmust be a FloorPlan view or a CeilingPlan view.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the control.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, the newly created control is returned, otherwise anexception with error information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ControlShape)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ControlShape));
			var arg1=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.View));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewModelText")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, the newly created model text is returned, otherwise anexception with error information will be thrown.")]
	public class Revit_FamilyItemFactory_NewModelText : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewModelText()
		{
			InPortData.Add(new PortData("s", "The text to be displayed.",typeof(object)));
			InPortData.Add(new PortData("mtt", "The type of model text. If this parameter is",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane of the model text. The direction of model text is determined by the normal of the sketch plane.To extrude in the other direction set the depth value to negative.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The position of the model text. The position must lie in the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("ha", "The horizontal alignment.",typeof(object)));
			InPortData.Add(new PortData("n", "The depth of the model text.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, the newly created model text is returned, otherwise anexception with error information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(Autodesk.Revit.DB.ModelTextType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ModelTextType));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.HorizontalAlign)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.HorizontalAlign));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, the newly created opening is returned, otherwise anexception with error information will be thrown.")]
	public class Revit_FamilyItemFactory_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewOpening()
		{
			InPortData.Add(new PortData("el", "Host elements that new opening would lie in. The host can only be a wall or a ceiling.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created opening. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. The profiles will be projected into the host plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, the newly created opening is returned, otherwise anexception with error information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewElectricalConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Electrical Connector is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewElectricalConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewElectricalConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("ett", "Indicates the system type of this new Electrical connector.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Electrical Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewPipeConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new pipe connector is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewPipeConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewPipeConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("pst", "Indicates the system type of this new Pipe connector.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new pipe connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Plumbing.PipeSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewDuctConnector")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Duct Connector is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewDuctConnector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewDuctConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(object)));
			InPortData.Add(new PortData("dst", "Indicates the system type of this new duct connector.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Duct Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Mechanical.DuctSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewRadialDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewRadialDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewRadialDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewDiameterDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new diameter dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewDiameterDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewDiameterDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the diameter dimension will lie.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new diameter dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewRadialDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewRadialDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewRadialDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewArcLengthDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewArcLengthDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewArcLengthDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Arc));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Reference));
			var arg4=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Reference));
			var arg5=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.DimensionType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewArcLengthDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewArcLengthDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewArcLengthDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Arc));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Reference));
			var arg4=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewAngularDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewAngularDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewAngularDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Arc));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Reference));
			var arg4=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.DimensionType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewAngularDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewAngularDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewAngularDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Arc));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewLinearDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewLinearDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewLinearDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewLinearDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewLinearDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewLinearDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewFormByThickenSingleSurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This function will modify the input singleSurfaceForm and return the same element.")]
	public class Revit_FamilyItemFactory_NewFormByThickenSingleSurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewFormByThickenSingleSurface()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("frm", "The single-surface form element. It can have one top/bottom face or one side face.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The offset of capped solid.",typeof(object)));
			OutPortData.Add(new PortData("out","This function will modify the input singleSurfaceForm and return the same element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Form));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewFormByCap")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class Revit_FamilyItemFactory_NewFormByCap : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewFormByCap()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of the newly created cap. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewRevolveForms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful new forms are returned.")]
	public class Revit_FamilyItemFactory_NewRevolveForms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewRevolveForms()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of the newly created revolution. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("ref", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful new forms are returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSweptBlendForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class Revit_FamilyItemFactory_NewSweptBlendForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweptBlendForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The path of the swept blend. The path should be 2D, where all input curves lie in one plane. If there’s more than one profile, the path should be a single curve. It’s required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created swept blend. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.ReferenceArrayArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArrayArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewExtrusionForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class Revit_FamilyItemFactory_NewExtrusionForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewExtrusionForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The profile of extrusion. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction of extrusion, with its length the length of the extrusion. The direction must be perpendicular to the plane determined by profile. The length of vector must be non-zero.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewLoftForm")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful form is are returned.")]
	public class Revit_FamilyItemFactory_NewLoftForm : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewLoftForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created loft. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful form is are returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArrayArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArrayArray));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSweptBlend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweptBlend : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweptBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("ref", "The path of the swept blend. The path might be a reference of single curve or edge obtained from existing geometry.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SweepProfile));
			var arg3=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SweepProfile));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSweptBlend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweptBlend : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweptBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crv", "The path of the swept blend. The path should be a single curve.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(object)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SweepProfile));
			var arg4=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.SweepProfile));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweep : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweep()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("refa", "The path of the sweep. The path should be reference of curve or edge obtained from existing geometry.",typeof(object)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(object)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SweepProfile));
			var arg3=(System.Int32)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Int32));
			var arg4=(Autodesk.Revit.DB.ProfilePlaneLocation)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.ProfilePlaneLocation));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewSweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweep : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweep()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The path of the sweep. The path should be 2D, where all input curves lie in one plane, and the curves are not required to reference existing geometry.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(object)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(object)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SweepProfile));
			var arg4=(System.Int32)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Int32));
			var arg5=(Autodesk.Revit.DB.ProfilePlaneLocation)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.ProfilePlaneLocation));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewRevolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new revolution is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewRevolution : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewRevolution()
		{
			InPortData.Add(new PortData("b", "Indicates if the Revolution is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created revolution. This may containmore than one curve loop. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the revolution.  The direction of revolutionis determined by the normal for the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("crv", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new revolution is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArrArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArrArray));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Line));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewBlend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new blend is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewBlend : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the Blend is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The top blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The base blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the base profile. Use this to associate the base of the blend to geometry from another element. Optional, it can be",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.CurveArray));
			var arg3=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_FamilyItemFactory_NewExtrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new Extrusion is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewExtrusion : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewExtrusion()
		{
			InPortData.Add(new PortData("b", "Indicates if the Extrusion is Solid or Void.",typeof(object)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created Extrusion. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. All input curves must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane for the extrusion.  The direction of extrusionis determined by the normal for the sketch plane.  To extrude in the other direction set the end value to negative.",typeof(object)));
			InPortData.Add(new PortData("n", "The length of the extrusion.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new Extrusion is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArrArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArrArray));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
		}
	}
	[NodeName("Revit_CylindricalHelix_getGCylindricalHelix")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_CylindricalHelix_getGCylindricalHelix : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CylindricalHelix_getGCylindricalHelix()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_CylindricalHelix_Create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_CylindricalHelix_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CylindricalHelix_Create()
		{
			InPortData.Add(new PortData("xyz", "Base point of the axis. It can be any point in 3d.",typeof(object)));
			InPortData.Add(new PortData("n", "Radius. It should be a positive number.",typeof(object)));
			InPortData.Add(new PortData("xyz", "X vector. Should be Non-zero vector.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Z vector = axis direction. Should be non-zero and orthogonal to X Vector.",typeof(object)));
			InPortData.Add(new PortData("n", "Pitch. It should be non-zero number, can be positive or negative.                          Positive means right handed and negative means left handed.",typeof(object)));
			InPortData.Add(new PortData("n", "Start angle. It specifies the start point of the Helix.",typeof(object)));
			InPortData.Add(new PortData("n", "End angle. It specifies the end point of the Helix.                           End angle should not be equal to start angle.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewAlignment")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful the new locked alignment dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_ItemFactoryBase_NewAlignment : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewAlignment()
		{
			InPortData.Add(new PortData("v", "The view that determines the orientation of the alignment.",typeof(object)));
			InPortData.Add(new PortData("ref", "The first reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second reference.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful the new locked alignment dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_ItemFactoryBase_PlaceGroup")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new group is returned, otherwise")]
	public class Revit_ItemFactoryBase_PlaceGroup : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_PlaceGroup()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the group is to be placed.",typeof(object)));
			InPortData.Add(new PortData("val", "A GroupType object that represents the type of group that is to be placed.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new group is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.GroupType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.GroupType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewViewSection")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created section view.")]
	public class Revit_ItemFactoryBase_NewViewSection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewViewSection()
		{
			InPortData.Add(new PortData("val", "The view volume of the section will correspond geometrically to the specified bounding box.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created section view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewView3D")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created 3D view.")]
	public class Revit_ItemFactoryBase_NewView3D : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewView3D()
		{
			InPortData.Add(new PortData("xyz", "The view direction - the vector pointing from the eye towards the model.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created 3D view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewTextNotes")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the creation is successful an ElementSet which contains the TextNotes should be returned, otherwise Autodesk::Revit::Exceptions::InvalidOperationException will be thrown.")]
	public class Revit_ItemFactoryBase_NewTextNotes : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewTextNotes()
		{
			InPortData.Add(new PortData("val", "A list of TextNoteCreationData which wraps the creation arguments of the TextNotes to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the TextNotes should be returned, otherwise Autodesk::Revit::Exceptions::InvalidOperationException will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.TextNoteCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.TextNoteCreationData>));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewTextNote")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a TextNote object is returned.")]
	public class Revit_ItemFactoryBase_NewTextNote : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewTextNote()
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
			OutPortData.Add(new PortData("out","If successful, a TextNote object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.TextAlignFlags));
			var arg6=(Autodesk.Revit.DB.TextNoteLeaderTypes)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.TextNoteLeaderTypes));
			var arg7=(Autodesk.Revit.DB.TextNoteLeaderStyles)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.TextNoteLeaderStyles));
			var arg8=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[8],typeof(Autodesk.Revit.DB.XYZ));
			var arg9=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[9],typeof(Autodesk.Revit.DB.XYZ));
			var arg10=(System.String)DynamoTypeConverter.ConvertInput(args[10],typeof(System.String));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewTextNote")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a TextNote object is returned.")]
	public class Revit_ItemFactoryBase_NewTextNote : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewTextNote()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, a TextNote object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.TextAlignFlags));
			var arg6=(System.String)DynamoTypeConverter.ConvertInput(args[6],typeof(System.String));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewSketchPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.")]
	public class Revit_ItemFactoryBase_NewSketchPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewSketchPlane()
		{
			InPortData.Add(new PortData("ref", "The planar face reference to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewSketchPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.")]
	public class Revit_ItemFactoryBase_NewSketchPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewSketchPlane()
		{
			InPortData.Add(new PortData("val", "The geometry planar face to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PlanarFace)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.PlanarFace));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewSketchPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise")]
	public class Revit_ItemFactoryBase_NewSketchPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewSketchPlane()
		{
			InPortData.Add(new PortData("p", "The geometry plane to locate sketch plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Plane)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Plane));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewReferencePlane2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created reference plane.")]
	public class Revit_ItemFactoryBase_NewReferencePlane2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewReferencePlane2()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A third point needed to define the reference plane.",typeof(object)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewReferencePlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created reference plane.")]
	public class Revit_ItemFactoryBase_NewReferencePlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewReferencePlane()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The cut vector apply to reference plane, should perpendicular to the vector  (bubbleEnd-freeEnd).",typeof(object)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewViewPlan")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("if successful, a new plan view object within the project, otherwise")]
	public class Revit_ItemFactoryBase_NewViewPlan : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewViewPlan()
		{
			InPortData.Add(new PortData("s", "The name for the new plan view, must be unique or",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the plan view is to be associated.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of plan view to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","if successful, a new plan view object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.ViewPlanType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ViewPlanType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewLevel")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created level.")]
	public class Revit_ItemFactoryBase_NewLevel : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewLevel()
		{
			InPortData.Add(new PortData("n", "The elevation to apply to the new level.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewModelCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new model line element. Otherwise")]
	public class Revit_ItemFactoryBase_NewModelCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewModelCurve()
		{
			InPortData.Add(new PortData("crv", "The internal geometry curve for model line.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane this new model line resides in.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new model line element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewGroup")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new instance of a group containing the elements specified.")]
	public class Revit_ItemFactoryBase_NewGroup : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewGroup()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(object)));
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.ElementId>));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewGroup")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new instance of a group containing the elements specified.")]
	public class Revit_ItemFactoryBase_NewGroup : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewGroup()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(object)));
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ElementSet));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstances2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the creation is successful, a set of ElementIds which contains the Family instances should be returned, otherwise the exception will be thrown.")]
	public class Revit_ItemFactoryBase_NewFamilyInstances2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstances2()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If the creation is successful, a set of ElementIds which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstances")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the creation is successful an ElementSet which contains the Family instances should be returned, otherwise the exception will be thrown.")]
	public class Revit_ItemFactoryBase_NewFamilyInstances : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstances()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("crv", "The line location of family instance. The line must in the plane of the view.",typeof(object)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the family instance.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Line));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned.")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The origin of family instance. If created on a",typeof(object)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "The 2D view in which to place the family instance.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted. Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(object)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the FamilyInstance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Element));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(object)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Element));
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new dimension object, otherwise")]
	public class Revit_ItemFactoryBase_NewDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(object)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewDimension")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new dimension object, otherwise")]
	public class Revit_ItemFactoryBase_NewDimension : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewDetailCurveArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an array of new detail curve elements. Otherwise")]
	public class Revit_ItemFactoryBase_NewDetailCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewDetailCurveArray()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curves are to be visible.",typeof(object)));
			InPortData.Add(new PortData("crvs", "An array containing the internal geometry curves for detail lines. The curve in array should be bound curve.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an array of new detail curve elements. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewDetailCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new detail curve element. Otherwise")]
	public class Revit_ItemFactoryBase_NewDetailCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewDetailCurve()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curve is to be visible.",typeof(object)));
			InPortData.Add(new PortData("crv", "The internal geometry curve for detail curve. It should be a bound curve.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new detail curve element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
		}
	}
	[NodeName("Revit_ItemFactoryBase_NewAnnotationSymbol")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewAnnotationSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewAnnotationSymbol()
		{
			InPortData.Add(new PortData("xyz", "The origin of the annotation symbol. If created on",typeof(object)));
			InPortData.Add(new PortData("val", "An annotation symbol type that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the annotation symbol.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.AnnotationSymbolType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.AnnotationSymbolType));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_PolyLine_Clone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PolyLine_Clone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_Clone()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_PolyLine_GetOutline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PolyLine_GetOutline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_GetOutline()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_PolyLine_GetCoordinates")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PolyLine_GetCoordinates : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_GetCoordinates()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_PolyLine_GetCoordinate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PolyLine_GetCoordinate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_GetCoordinate()
		{
			InPortData.Add(new PortData("i", "The index of the coordinates.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Int32));
		}
	}
	[NodeName("Revit_PolyLine_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PolyLine_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_Evaluate()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated. It is expected to be in [0,1] interval mapped to the bounds of the whole polyline.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewTopographySurface")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The TopographySurface element.")]
	public class Revit_Document_NewTopographySurface : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTopographySurface()
		{
			InPortData.Add(new PortData("lst", "An array of initial points for the surface.",typeof(object)));
			OutPortData.Add(new PortData("out","The TopographySurface element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
		}
	}
	[NodeName("Revit_Document_NewTakeoffFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewTakeoffFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTakeoffFitting()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the takeoff.",typeof(object)));
			InPortData.Add(new PortData("mepcrv", "The duct or pipe which is the trunk for the takeoff.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.MEPCurve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.MEPCurve));
		}
	}
	[NodeName("Revit_Document_NewUnionFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewUnionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewUnionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the union.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the union.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
		}
	}
	[NodeName("Revit_Document_NewCrossFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewCrossFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewCrossFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the cross.",typeof(object)));
			InPortData.Add(new PortData("con", "The fourth connector to be connected to the cross.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Connector));
			var arg3=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Connector));
		}
	}
	[NodeName("Revit_Document_NewTransitionFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewTransitionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTransitionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the transition.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the transition.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
		}
	}
	[NodeName("Revit_Document_NewTeeFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewTeeFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTeeFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the tee.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the tee.",typeof(object)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the tee. This should be connected to the branch of the tee.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Connector));
		}
	}
	[NodeName("Revit_Document_NewElbowFitting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewElbowFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElbowFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the elbow.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the elbow.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
		}
	}
	[NodeName("Revit_Document_NewFlexPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexPipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
		}
	}
	[NodeName("Revit_Document_NewFlexPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexPipe()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the flexible pipe, including the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
		}
	}
	[NodeName("Revit_Document_NewFlexPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexPipe()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe, including the end points.",typeof(object)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
		}
	}
	[NodeName("Revit_Document_NewPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
		}
	}
	[NodeName("Revit_Document_NewPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipe()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector to be connected to the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
		}
	}
	[NodeName("Revit_Document_NewPipe")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipe()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second point of the pipe.",typeof(object)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
		}
	}
	[NodeName("Revit_Document_NewFlexDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexDuct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
		}
	}
	[NodeName("Revit_Document_NewFlexDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexDuct()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the duct, including the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
		}
	}
	[NodeName("Revit_Document_NewFlexDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexDuct()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct, including the end points.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
		}
	}
	[NodeName("Revit_Document_NewDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewDuct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
		}
	}
	[NodeName("Revit_Document_NewDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewDuct()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector to be connected to the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
		}
	}
	[NodeName("Revit_Document_NewDuct")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewDuct()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second point of the duct.",typeof(object)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
		}
	}
	[NodeName("Revit_Document_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_Document_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFamilyInstance()
		{
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Document_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_Document_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Document_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_Document_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed on the specified level.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Element));
			var arg3=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Level));
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Document_NewFascia")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new fascia object within the project, otherwise")]
	public class Revit_Document_NewFascia : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFascia()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the fascia.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.FasciaType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_Document_NewFascia")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new fascia object within the project, otherwise")]
	public class Revit_Document_NewFascia : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFascia()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the fascia.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.FasciaType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_Document_NewGutter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new gutter object within the project, otherwise")]
	public class Revit_Document_NewGutter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGutter()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the gutter.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.GutterType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_Document_NewGutter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new gutter object within the project, otherwise")]
	public class Revit_Document_NewGutter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGutter()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the gutter.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.GutterType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_Document_NewSlabEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new slab edge object within the project, otherwise")]
	public class Revit_Document_NewSlabEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSlabEdge()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(object)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the slab edge.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SlabEdgeType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_Document_NewSlabEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new slab edge object within the project, otherwise")]
	public class Revit_Document_NewSlabEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSlabEdge()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(object)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the slab edge.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SlabEdgeType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
		}
	}
	[NodeName("Revit_Document_NewCurtainSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The CurtainSystem created will be returned when the operation succeeds.")]
	public class Revit_Document_NewCurtainSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewCurtainSystem()
		{
			InPortData.Add(new PortData("val", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","The CurtainSystem created will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FaceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FaceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
		}
	}
	[NodeName("Revit_Document_NewCurtainSystem2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A set of ElementIds of CurtainSystems will be returned when the operation succeeds.")]
	public class Revit_Document_NewCurtainSystem2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewCurtainSystem2()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","A set of ElementIds of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
		}
	}
	[NodeName("Revit_Document_NewCurtainSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A set of CurtainSystems will be returned when the operation succeeds.")]
	public class Revit_Document_NewCurtainSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewCurtainSystem()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(object)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","A set of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
		}
	}
	[NodeName("Revit_Document_NewWire")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wire element within the project, otherwise")]
	public class Revit_Document_NewWire : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWire()
		{
			InPortData.Add(new PortData("crv", "The base line of the wire.",typeof(object)));
			InPortData.Add(new PortData("v", "The view in which the wire is to be visible.",typeof(object)));
			InPortData.Add(new PortData("con", "The connector which connects with the start point connector of wire, if it is",typeof(object)));
			InPortData.Add(new PortData("con", "The connector which connects with the end point connector of wire, if it is",typeof(object)));
			InPortData.Add(new PortData("val", "Specify wire type of new created wire.",typeof(object)));
			InPortData.Add(new PortData("val", "Specify wiring type(Arc or chamfer) of new created wire.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wire element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.View));
			var arg2=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Connector));
			var arg3=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Connector));
			var arg4=(Autodesk.Revit.DB.Electrical.WireType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Electrical.WireType));
			var arg5=(Autodesk.Revit.DB.Electrical.WiringType)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Electrical.WiringType));
		}
	}
	[NodeName("Revit_Document_NewZone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new Zone element within the project, otherwise")]
	public class Revit_Document_NewZone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewZone()
		{
			InPortData.Add(new PortData("l", "The level on which the Zone is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The associative phase on which the Zone is to exist.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new Zone element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
		}
	}
	[NodeName("Revit_Document_NewRoomBoundaryLines")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewRoomBoundaryLines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoomBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(object)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Room",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewSpaceBoundaryLines")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewSpaceBoundaryLines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaceBoundaryLines()
		{
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(object)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Space",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewSpaceTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a SpaceTag object will be returned, otherwise")]
	public class Revit_Document_NewSpaceTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaceTag()
		{
			InPortData.Add(new PortData("val", "The Space which the tag refers.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the space.",typeof(object)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a SpaceTag object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mechanical.Space)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Mechanical.Space));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewSpaces2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewSpaces2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaces2()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewSpaces")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewSpaces : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaces()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(object)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewSpace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new Space element within the project, otherwise")]
	public class Revit_Document_NewSpace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpace()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new Space element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Document_NewSpace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful the new space element is returned, otherwise")]
	public class Revit_Document_NewSpace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpace()
		{
			InPortData.Add(new PortData("l", "The level on which the space is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful the new space element is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Document_NewSpace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful the new space should be returned, otherwise")]
	public class Revit_Document_NewSpace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpace()
		{
			InPortData.Add(new PortData("val", "The phase in which the space is to exist.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful the new space should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
		}
	}
	[NodeName("Revit_Document_NewPipingSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance of piping system is returned, otherwise an exception with information will be thrown.")]
	public class Revit_Document_NewPipingSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipingSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(object)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(object)));
			InPortData.Add(new PortData("pst", "The System type.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance of piping system is returned, otherwise an exception with information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.ConnectorSet)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ConnectorSet));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeSystemType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType));
		}
	}
	[NodeName("Revit_Document_NewMechanicalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then an instance of mechanical system is returned, otherwise an exception with information will be thrown.")]
	public class Revit_Document_NewMechanicalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewMechanicalSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(object)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(object)));
			InPortData.Add(new PortData("dst", "The system type.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance of mechanical system is returned, otherwise an exception with information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.ConnectorSet)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ConnectorSet));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctSystemType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType));
		}
	}
	[NodeName("Revit_Document_NewElectricalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Revit_Document_NewElectricalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElectricalSystem()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.ElementId>));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
		}
	}
	[NodeName("Revit_Document_NewElectricalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Revit_Document_NewElectricalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElectricalSystem()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ElementSet));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
		}
	}
	[NodeName("Revit_Document_NewElectricalSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Revit_Document_NewElectricalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElectricalSystem()
		{
			InPortData.Add(new PortData("con", "The Connector to create this Electrical System.",typeof(object)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
		}
	}
	[NodeName("Revit_Document_NewExtrusionRoof")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewExtrusionRoof : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewExtrusionRoof()
		{
			InPortData.Add(new PortData("crvs", "The profile of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("val", "The work plane for the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("l", "The level of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("val", "Type of the extrusion roof.",typeof(object)));
			InPortData.Add(new PortData("n", "Start the extrusion.",typeof(object)));
			InPortData.Add(new PortData("n", "End the extrusion.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.ReferencePlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferencePlane));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.RoofType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.RoofType));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewFootPrintRoof")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewFootPrintRoof : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFootPrintRoof()
		{
			InPortData.Add(new PortData("crvs", "The footprint of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("l", "The level of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("val", "Type of the FootPrintRoof.",typeof(object)));
			InPortData.Add(new PortData("val", "An array of Model Curves corresponding to the set of Curves input in the footPrint. By knowing which Model Curve was created by each footPrint curve, you can set properties like SlopeAngle for each curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.RoofType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.RoofType));
			var arg3=(Autodesk.Revit.DB.ModelCurveArray)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.ModelCurveArray));
		}
	}
	[NodeName("Revit_Document_NewTruss")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewTruss : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTruss()
		{
			InPortData.Add(new PortData("val", "The type for truss.",typeof(object)));
			InPortData.Add(new PortData("sp", "The sketch plane where the truss is going to reside. It could be",typeof(object)));
			InPortData.Add(new PortData("crv", "The curve that represents truss's base curve.It must be a line, must not be a vertical line, and must be within the sketch plane if sketchPlane is valid.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Structure.TrussType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Structure.TrussType));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg2=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Curve));
		}
	}
	[NodeName("Revit_Document_NewAreas")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an Element Set which contains the areas should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewAreas : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreas()
		{
			InPortData.Add(new PortData("val", "A list of AreaCreationData which wraps the creation arguments of the areas to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an Element Set which contains the areas should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.AreaCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.AreaCreationData>));
		}
	}
	[NodeName("Revit_Document_NewArea")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewArea : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewArea()
		{
			InPortData.Add(new PortData("v", "The view of area element.",typeof(object)));
			InPortData.Add(new PortData("uv", "The point which lies in the enclosed region of AreaBoundaryLines to put the new created Area",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ViewPlan));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Document_NewAreaBoundaryLine")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewAreaBoundaryLine : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaBoundaryLine()
		{
			InPortData.Add(new PortData("sp", "The sketch plane.",typeof(object)));
			InPortData.Add(new PortData("crv", "The geometry curve on which the boundary line are",typeof(object)));
			InPortData.Add(new PortData("v", "The View for the new Area",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.ViewPlan)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ViewPlan));
		}
	}
	[NodeName("Revit_Document_NewFoundationWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewFoundationWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFoundationWall()
		{
			InPortData.Add(new PortData("val", "The ContFooting type.",typeof(object)));
			InPortData.Add(new PortData("val", "The Wall to append a ContFooting.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ContFootingType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ContFootingType));
			var arg1=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Wall));
		}
	}
	[NodeName("Revit_Document_NewSlab")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new floor object within the project, otherwise")]
	public class Revit_Document_NewSlab : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSlab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the slab.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the slab is to be placed.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line use to control the sloped angle of the slab. It should be in the same face with profile.",typeof(object)));
			InPortData.Add(new PortData("n", "The slope.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Line));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, an IndependentTag object is returned.")]
	public class Revit_Document_NewTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewTag()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(object)));
			InPortData.Add(new PortData("el", "The host object of tag.",typeof(object)));
			InPortData.Add(new PortData("b", "Whether there will be a leader.",typeof(object)));
			InPortData.Add(new PortData("val", "The mode of the tag. Add by Category, add by Multi-Category, or add by material.",typeof(object)));
			InPortData.Add(new PortData("val", "The orientation of the tag.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The position of the tag.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, an IndependentTag object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Element));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var arg3=(Autodesk.Revit.DB.TagMode)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.TagMode));
			var arg4=(Autodesk.Revit.DB.TagOrientation)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.TagOrientation));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Document_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening()
		{
			InPortData.Add(new PortData("el", "Host element of the opening. Can be a roof, floor, or ceiling.",typeof(object)));
			InPortData.Add(new PortData("crvs", "Profile of the opening.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the profile is cut perpendicular to the intersecting face of the host. False if the profile is cut vertically.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening()
		{
			InPortData.Add(new PortData("val", "Host element of the opening.",typeof(object)));
			InPortData.Add(new PortData("xyz", "One corner of the rectangle.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The opposite corner of the rectangle.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Wall));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Document_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening()
		{
			InPortData.Add(new PortData("l", "bottom level",typeof(object)));
			InPortData.Add(new PortData("l", "top level",typeof(object)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.CurveArray));
		}
	}
	[NodeName("Revit_Document_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening()
		{
			InPortData.Add(new PortData("el", "host element of the opening, can be a beam, brace and column.",typeof(object)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(object)));
			InPortData.Add(new PortData("val", "face on which opening is based on.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.Creation.eRefFace)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.Creation.eRefFace));
		}
	}
	[NodeName("Revit_Document_NewAreaBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".")]
	public class Revit_Document_NewAreaBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "A Wall, Slab or Slab Foundation to host the boundary conditions.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewLineBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".")]
	public class Revit_Document_NewLineBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineBoundaryConditions()
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
			OutPortData.Add(new PortData("out","If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg8=(System.Double)DynamoTypeConverter.ConvertInput(args[8],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewAreaBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".")]
	public class Revit_Document_NewAreaBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaBoundaryConditions()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference obtained from a Wall, Slab or Slab Foundation.",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(object)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(object)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewLineBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".")]
	public class Revit_Document_NewLineBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineBoundaryConditions()
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
			OutPortData.Add(new PortData("out","If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg8=(System.Double)DynamoTypeConverter.ConvertInput(args[8],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewPointBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewPointBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 0 - \"Point\".")]
	public class Revit_Document_NewPointBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPointBoundaryConditions()
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
			OutPortData.Add(new PortData("out","If successful, NewPointBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 0 - \"Point\".",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
			var arg7=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg8=(System.Double)DynamoTypeConverter.ConvertInput(args[8],typeof(System.Double));
			var arg9=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[9],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg10=(System.Double)DynamoTypeConverter.ConvertInput(args[10],typeof(System.Double));
			var arg11=(Autodesk.Revit.DB.Structure.TranslationRotationValue)DynamoTypeConverter.ConvertInput(args[11],typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue));
			var arg12=(System.Double)DynamoTypeConverter.ConvertInput(args[12],typeof(System.Double));
		}
	}
	[NodeName("Revit_Document_NewBeamSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem. This argument is optional – may be null.",typeof(object)));
			InPortData.Add(new PortData("b", "Whether the BeamSystem is 3D or not",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewBeamSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
		}
	}
	[NodeName("Revit_Document_NewBeamSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the sketch plane.",typeof(object)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("b", "If the BeamSystem is 3D, the sketchPlane must be a level, oran exception will be thrown.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewBeamSystem")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile is the profile of the BeamSystem.",typeof(object)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewRoomTag")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a RoomTag object will be returned, otherwise")]
	public class Revit_Document_NewRoomTag : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoomTag()
		{
			InPortData.Add(new PortData("val", "The Room which the tag refers.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the room.",typeof(object)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a RoomTag object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.Room));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
		}
	}
	[NodeName("Revit_Document_NewRooms2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms2()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(object)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
		}
	}
	[NodeName("Revit_Document_NewRooms2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
		}
	}
	[NodeName("Revit_Document_NewRooms2")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms created should be returned, otherwise")]
	public class Revit_Document_NewRooms2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms created should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
		}
	}
	[NodeName("Revit_Document_NewRooms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an Element set which contain the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(object)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
		}
	}
	[NodeName("Revit_Document_NewRooms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
		}
	}
	[NodeName("Revit_Document_NewRooms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an Element set which contain the rooms created should be returned, otherwise")]
	public class Revit_Document_NewRooms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms created should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
		}
	}
	[NodeName("Revit_Document_NewRooms")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms()
		{
			InPortData.Add(new PortData("val", "A list of RoomCreationData which wraps the creation arguments of the rooms to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RoomCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.RoomCreationData>));
		}
	}
	[NodeName("Revit_Document_NewRoom")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful the room is returned, otherwise")]
	public class Revit_Document_NewRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoom()
		{
			InPortData.Add(new PortData("val", "The room which you want to locate in the circuit.  Pass",typeof(object)));
			InPortData.Add(new PortData("val", "The circuit in which you want to locate a room.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful the room is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.Room));
			var arg1=(Autodesk.Revit.DB.PlanCircuit)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.PlanCircuit));
		}
	}
	[NodeName("Revit_Document_NewRoom")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful the new room , otherwise")]
	public class Revit_Document_NewRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoom()
		{
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful the new room , otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
		}
	}
	[NodeName("Revit_Document_NewRoom")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful the new room will be returned, otherwise")]
	public class Revit_Document_NewRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoom()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location of the room on that specified level.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful the new room will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Document_NewGrids")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An Element set that contains the Grids.")]
	public class Revit_Document_NewGrids : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGrids()
		{
			InPortData.Add(new PortData("crvs", "The curves which represent the new grid lines.  These curves must be lines or bounded arcs.",typeof(object)));
			OutPortData.Add(new PortData("out","An Element set that contains the Grids.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
		}
	}
	[NodeName("Revit_Document_NewGrid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created grid line.")]
	public class Revit_Document_NewGrid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGrid()
		{
			InPortData.Add(new PortData("arc", "An arc object that represents the location of the new grid line.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Arc));
		}
	}
	[NodeName("Revit_Document_NewGrid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created grid line.")]
	public class Revit_Document_NewGrid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGrid()
		{
			InPortData.Add(new PortData("crv", "A line object which represents the location of the grid line.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Line));
		}
	}
	[NodeName("Revit_Document_NewViewSheet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created sheet view.")]
	public class Revit_Document_NewViewSheet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewViewSheet()
		{
			InPortData.Add(new PortData("fs", "The titleblock family symbol to apply to this sheet.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created sheet view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_Document_NewViewDrafting")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created drafting view.")]
	public class Revit_Document_NewViewDrafting : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewViewDrafting()
		{
			OutPortData.Add(new PortData("out","The newly created drafting view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Document_NewFoundationSlab")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("if successful, a new foundation slab object within the project, otherwise")]
	public class Revit_Document_NewFoundationSlab : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFoundationSlab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(object)));
			OutPortData.Add(new PortData("out","if successful, a new foundation slab object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.FloorType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FloorType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Document_NewFloor")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("if successful, a new floor object within the project, otherwise")]
	public class Revit_Document_NewFloor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFloor()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(object)));
			OutPortData.Add(new PortData("out","if successful, a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.FloorType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FloorType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Document_NewFloor")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("if successful, a new floor object within the project, otherwise")]
	public class Revit_Document_NewFloor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFloor()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","if successful, a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.FloorType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FloorType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewFloor")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new floor object within the project, otherwise")]
	public class Revit_Document_NewFloor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFloor()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewWalls")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewWalls : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWalls()
		{
			InPortData.Add(new PortData("val", "A list of ProfiledWallCreationData which wraps the creation arguments of the walls to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.ProfiledWallCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.ProfiledWallCreationData>));
		}
	}
	[NodeName("Revit_Document_NewWalls")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewWalls : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWalls()
		{
			InPortData.Add(new PortData("val", "A list of RectangularWallCreationData which wraps the creation arguments of the walls to be created.",typeof(object)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RectangularWallCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.RectangularWallCreationData>));
		}
	}
	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is consideredto be inside and outside.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(object)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(object)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewSpotElevation")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new spot dimension object, otherwise")]
	public class Revit_Document_NewSpotElevation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpotElevation()
		{
			InPortData.Add(new PortData("v", "The view in which the spot elevation is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference to which the spot elevation is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point which the spot elevation evaluate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot elevation.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the spot elevation.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot elevation evaluate.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new spot dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewSpotCoordinate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new spot dimension object, otherwise")]
	public class Revit_Document_NewSpotCoordinate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpotCoordinate()
		{
			InPortData.Add(new PortData("v", "The view in which the spot coordinate is to be visible.",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference to which the spot coordinate is to be bound.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point which the spot coordinate evaluate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot coordinate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point for the spot coordinate.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot coordinate evaluate.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new spot dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewLoadCombination")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLoadCombination and there isn't the Load Combination Element     with the same name returns an object for the newly created LoadCombination.     If such element exist and match desired one (has the same formula and the same    usages set), returns existing element. Otherwise")]
	public class Revit_Document_NewLoadCombination : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLoadCombination()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Combination Element to create.",typeof(object)));
			InPortData.Add(new PortData("i", "LoadCombination Type Index: 0-Combination, 1-Envelope.",typeof(object)));
			InPortData.Add(new PortData("i", "LoadCombination State Index: 0-Servicebility, 1-Ultimate.",typeof(object)));
			InPortData.Add(new PortData("val", "Factors array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Cases array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Combinations array for Load Combination formula.",typeof(object)));
			InPortData.Add(new PortData("val", "Load Usages array.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewLoadCombination and there isn't the Load Combination Element     with the same name returns an object for the newly created LoadCombination.     If such element exist and match desired one (has the same formula and the same    usages set), returns existing element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
			var arg2=(System.Int32)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Int32));
			var arg3=(System.Double[])DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double[]));
			var arg4=(Autodesk.Revit.DB.Structure.LoadCaseArray)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.LoadCaseArray));
			var arg5=(Autodesk.Revit.DB.Structure.LoadCombinationArray)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.LoadCombinationArray));
			var arg6=(Autodesk.Revit.DB.Structure.LoadUsageArray)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.Structure.LoadUsageArray));
		}
	}
	[NodeName("Revit_Document_NewLoadCase")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLoadCase and there isn't the Load Case Element     with the same name returns an object for the newly created LoadCase.     If such element exist and match desired one (has the same nature and number),     returns existing element. Otherwise")]
	public class Revit_Document_NewLoadCase : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLoadCase()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Case Element to create.",typeof(object)));
			InPortData.Add(new PortData("val", "The Load Case nature.",typeof(object)));
			InPortData.Add(new PortData("val", "The Load Case category.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewLoadCase and there isn't the Load Case Element     with the same name returns an object for the newly created LoadCase.     If such element exist and match desired one (has the same nature and number),     returns existing element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(Autodesk.Revit.DB.Structure.LoadNature)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.LoadNature));
			var arg2=(Autodesk.Revit.DB.Category)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Category));
		}
	}
	[NodeName("Revit_Document_NewLoadUsage")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful and there isn't the Load Usage Element with the    same name NewLoadUsage returns an object for the newly created LoadUsage.     If such element exist it returns existing element.")]
	public class Revit_Document_NewLoadUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLoadUsage()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Usage Element to create.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Usage Element with the    same name NewLoadUsage returns an object for the newly created LoadUsage.     If such element exist it returns existing element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
		}
	}
	[NodeName("Revit_Document_NewLoadNature")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful and there isn't the Load Nature Element with the    same name NewLoadNature returns an object for the newly created LoadNature.     If such element exist it returns existing element.")]
	public class Revit_Document_NewLoadNature : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLoadNature()
		{
			InPortData.Add(new PortData("s", "The name for the Load Nature Element to create.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Nature Element with the    same name NewLoadNature returns an object for the newly created LoadNature.     If such element exist it returns existing element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
		}
	}
	[NodeName("Revit_Document_NewAreaLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad()
		{
			InPortData.Add(new PortData("el", "The host element (Floor or Wall) of the AreaLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
		}
	}
	[NodeName("Revit_Document_NewAreaLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load.",typeof(object)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(object)));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the second reference point. Ignored if only one or two reference points are supplied.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the third reference point.  Ignored if only one or two reference points are supplied.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Int32[])DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32[]));
			var arg2=(System.Int32[])DynamoTypeConverter.ConvertInput(args[2],typeof(System.Int32[]));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
			var arg7=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
		}
	}
	[NodeName("Revit_Document_NewAreaLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load curves.",typeof(object)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(object)));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(object)));
			InPortData.Add(new PortData("lst", "The 3d area forces applied to each of the reference points in the refPntIdxs array.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Int32[])DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32[]));
			var arg2=(System.Int32[])DynamoTypeConverter.ConvertInput(args[2],typeof(System.Int32[]));
			var arg3=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[3],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
		}
	}
	[NodeName("Revit_Document_NewAreaLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad()
		{
			InPortData.Add(new PortData("lst", "Vertexes of AreaLoad shape polygon.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The applied 3d area force.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
		}
	}
	[NodeName("Revit_Document_NewLineLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical lines.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[2],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.Structure.LineLoadType));
			var arg7=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewLineLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad()
		{
			InPortData.Add(new PortData("el", "The host element (Beam, Brace or Column) of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[2],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.Structure.LineLoadType));
			var arg7=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewLineLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad()
		{
			InPortData.Add(new PortData("lst", "The end points of the LineLoad application.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(object)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns an object for the newly created LineLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[2],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
			var arg6=(Autodesk.Revit.DB.Structure.LineLoadType)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.Structure.LineLoadType));
			var arg7=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewLineLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewLineLoad returns the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad()
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
			OutPortData.Add(new PortData("out","If successful, NewLineLoad returns the newly created LineLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
			var arg7=(System.Boolean)DynamoTypeConverter.ConvertInput(args[7],typeof(System.Boolean));
			var arg8=(System.Boolean)DynamoTypeConverter.ConvertInput(args[8],typeof(System.Boolean));
			var arg9=(Autodesk.Revit.DB.Structure.LineLoadType)DynamoTypeConverter.ConvertInput(args[9],typeof(Autodesk.Revit.DB.Structure.LineLoadType));
			var arg10=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[10],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewPointLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewPointLoad returns an object for the newly created PointLoad.")]
	public class Revit_Document_NewPointLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPointLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, analytical line's end.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewPointLoad returns an object for the newly created PointLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.Structure.PointLoadType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.PointLoadType));
			var arg5=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewPointLoad")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewPointLoad returns an object for the newly created PointLoad.")]
	public class Revit_Document_NewPointLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPointLoad()
		{
			InPortData.Add(new PortData("xyz", "The point of the PointLoad application.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(object)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(object)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(object)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewPointLoad returns an object for the newly created PointLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.Structure.PointLoadType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.PointLoadType));
			var arg5=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.SketchPlane));
		}
	}
	[NodeName("Revit_Document_NewPathReinforcement")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful, NewPathReinforcement returns an object for the newly created Rebar.")]
	public class Revit_Document_NewPathReinforcement : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPathReinforcement()
		{
			InPortData.Add(new PortData("el", "The element to which the Path Reinforcement belongs. The element must be a structural floor or wall.",typeof(object)));
			InPortData.Add(new PortData("crvs", "An array of curves forming a chain.  Bars will be placed orthogonal to the chain with their hook ends near the chain, offset by the side cover setting.  The curves must belong to the top face of the floor or the exterior face of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "A flag controlling the bars relative to the curves. If the curves are given in order and with consistent orientation, the bars will lie to the right of the chain if flip=false, to the left if flip=true, when viewed from above the floor or outside the wall.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful, NewPathReinforcement returns an object for the newly created Rebar.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Document_NewRebarBarType")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Document_NewRebarBarType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRebarBarType()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryObject_getReferenceForAPIUser")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_GeometryObject_getReferenceForAPIUser : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_getReferenceForAPIUser()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryObject_GetHashCode")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_GeometryObject_GetHashCode : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_GetHashCode()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryObject_Equals")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_GeometryObject_Equals : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_Equals()
		{
			InPortData.Add(new PortData("val", "Another object.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Object)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Object));
		}
	}
	[NodeName("Revit_GeometryObject_op_Inequality")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if the GeometryObjects are different; otherwise, false.")]
	public class Revit_GeometryObject_op_Inequality : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_op_Inequality()
		{
			InPortData.Add(new PortData("val", "The first GeometryObject.",typeof(object)));
			InPortData.Add(new PortData("val", "The second GeometryObject.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the GeometryObjects are different; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.GeometryObject));
			var arg1=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.GeometryObject));
		}
	}
	[NodeName("Revit_GeometryObject_op_Equality")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if the GeometryObjects are the same; otherwise, false.")]
	public class Revit_GeometryObject_op_Equality : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_op_Equality()
		{
			InPortData.Add(new PortData("val", "The first GeometryObject.",typeof(object)));
			InPortData.Add(new PortData("val", "The second GeometryObject.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the GeometryObjects are the same; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.GeometryObject));
			var arg1=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.GeometryObject));
		}
	}
	[NodeName("Revit_Profile_Clone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Profile_Clone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Profile_Clone()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Curve_Clone")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_Clone : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Clone()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Curve_Project")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Geometric information if projection is successful.")]
	public class Revit_Curve_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Project()
		{
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(object)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Curve_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect()
		{
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.IntersectionResultArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.IntersectionResultArray));
		}
	}
	[NodeName("Revit_Curve_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect()
		{
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
		}
	}
	[NodeName("Revit_Curve_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if the parameter is within the curve's bounds, otherwise false.")]
	public class Revit_Curve_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside()
		{
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(object)));
			InPortData.Add(new PortData("val", "The end index is equal to 0 for the start point, 1 for the end point, or -1 if the parameter is not at the end.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the parameter is within the curve's bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
		}
	}
	[NodeName("Revit_Curve_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if the parameter is within the bounds, otherwise false.")]
	public class Revit_Curve_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside()
		{
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(object)));
			OutPortData.Add(new PortData("out","True if the parameter is within the bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Curve_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Revit_Curve_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeDerivatives()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicates that the specified parameter is normalized.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Curve_Distance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The real number equal to the shortest distance.")]
	public class Revit_Curve_Distance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Distance()
		{
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the shortest distance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Curve_ComputeRawParameter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The real number equal to the raw curve parameter.")]
	public class Revit_Curve_ComputeRawParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeRawParameter()
		{
			InPortData.Add(new PortData("n", "The normalized parameter.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the raw curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Curve_ComputeNormalizedParameter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The real number equal to the normalized curve parameter.")]
	public class Revit_Curve_ComputeNormalizedParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeNormalizedParameter()
		{
			InPortData.Add(new PortData("n", "The raw parameter.",typeof(object)));
			OutPortData.Add(new PortData("out","The real number equal to the normalized curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Curve_MakeUnbound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_MakeUnbound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_MakeUnbound()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Curve_MakeBound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_MakeBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_MakeBound()
		{
			InPortData.Add(new PortData("n", "The new parameter of the start point.",typeof(object)));
			InPortData.Add(new PortData("n", "The new parameter of the end point.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
		}
	}
	[NodeName("Revit_Curve_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Evaluate()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(object)));
			InPortData.Add(new PortData("b", "If false, param is interpreted as natural parameterization of the curve. If true, param is expected to be in [0,1] interval mapped to the bounds of the curve. Setting to true is valid only if the curve is bound.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Curve_Tessellate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Curve_Tessellate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Tessellate()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Face_Project")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns")]
	public class Revit_Face_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Project()
		{
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(object)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Face_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Face_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect()
		{
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.IntersectionResultArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.IntersectionResultArray));
		}
	}
	[NodeName("Revit_Face_Intersect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Face_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect()
		{
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
		}
	}
	[NodeName("Revit_Face_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if within this face, otherwise False.")]
	public class Revit_Face_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside()
		{
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information when the point is on the edge; otherwise,",typeof(object)));
			OutPortData.Add(new PortData("out","True if within this face, otherwise False.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.IntersectionResult)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.IntersectionResult));
		}
	}
	[NodeName("Revit_Face_IsInside")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("True if point is within this face, otherwise false.")]
	public class Revit_Face_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside()
		{
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			OutPortData.Add(new PortData("out","True if point is within this face, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Face_ComputeNormal")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The normal vector. This vector will be normalized.")]
	public class Revit_Face_ComputeNormal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeNormal()
		{
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			OutPortData.Add(new PortData("out","The normal vector. This vector will be normalized.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Face_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transformation containing tangent vectors and a normal vector.")]
	public class Revit_Face_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeDerivatives()
		{
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformation containing tangent vectors and a normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Face_GetBoundingBox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A BoundingBoxUV with the extents of the parameterization of the face.")]
	public class Revit_Face_GetBoundingBox : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetBoundingBox()
		{
			OutPortData.Add(new PortData("out","A BoundingBoxUV with the extents of the parameterization of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Face_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Face_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Evaluate()
		{
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Face_Triangulate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Face_Triangulate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Triangulate()
		{
			InPortData.Add(new PortData("n", "The level of detail. Its range is from 0 to 1. 0 is the lowest level of detail and 1 is the highest.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Face_Triangulate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Face_Triangulate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Triangulate()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Face_GetRegions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.")]
	public class Revit_Face_GetRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetRegions()
		{
			OutPortData.Add(new PortData("out","A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_GetOriginalGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_GetOriginalGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetOriginalGeometry()
		{
			InPortData.Add(new PortData("val", "The options used to obtain the geometry.  Note that ComputeReferences may notbe set to true.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Options)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Options));
		}
	}
	[NodeName("Revit_FamilyInstance_GetFamilyPointPlacementReferences")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_GetFamilyPointPlacementReferences : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetFamilyPointPlacementReferences()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_RemoveCoping")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_RemoveCoping : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_RemoveCoping()
		{
			InPortData.Add(new PortData("val", "A steel beam or column for which this beam currently has a coping cut. May not be",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
		}
	}
	[NodeName("Revit_FamilyInstance_AddCoping")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_AddCoping : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_AddCoping()
		{
			InPortData.Add(new PortData("val", "A steel beam or column. May not be",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
		}
	}
	[NodeName("Revit_FamilyInstance_SetCopingIds")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_SetCopingIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_SetCopingIds()
		{
			InPortData.Add(new PortData("val", "A set of coping cutters (steel beams and steel columns).",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.ElementId>));
		}
	}
	[NodeName("Revit_FamilyInstance_GetCopingIds")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The coping ElementIds")]
	public class Revit_FamilyInstance_GetCopingIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetCopingIds()
		{
			OutPortData.Add(new PortData("out","The coping ElementIds",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_SetCopings")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_SetCopings : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_SetCopings()
		{
			InPortData.Add(new PortData("val", "A set of coping cutters (steel beams and steel columns).",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ElementSet));
		}
	}
	[NodeName("Revit_FamilyInstance_GetCopings")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The coping elements")]
	public class Revit_FamilyInstance_GetCopings : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetCopings()
		{
			OutPortData.Add(new PortData("out","The coping elements",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_GetSubComponentIds")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The subcomponent ElementIDs")]
	public class Revit_FamilyInstance_GetSubComponentIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetSubComponentIds()
		{
			OutPortData.Add(new PortData("out","The subcomponent ElementIDs",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_FlipFromToRoom")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_FlipFromToRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_FlipFromToRoom()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_rotate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_rotate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_rotate()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_flipFacing")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_flipFacing : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_flipFacing()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_FamilyInstance_flipHand")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_FamilyInstance_flipHand : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_flipHand()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_PointCloudInstance_GetPoints")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A collection object containing points that pass the filter, but no more than the maximum number requested.")]
	public class Revit_PointCloudInstance_GetPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_GetPoints()
		{
			InPortData.Add(new PortData("val", "The filter to control which points are extracted. The filter should be passed in the coordinates   of the Revit model.",typeof(object)));
			InPortData.Add(new PortData("i", "The maximum number of points requested.",typeof(object)));
			OutPortData.Add(new PortData("out","A collection object containing points that pass the filter, but no more than the maximum number requested.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointClouds.PointCloudFilter)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
		}
	}
	[NodeName("Revit_PointCloudInstance_Create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created point cloud instance.")]
	public class Revit_PointCloudInstance_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_Create()
		{
			InPortData.Add(new PortData("val", "The document in which the new instance is created",typeof(object)));
			InPortData.Add(new PortData("val", "The element id of the PointCloudType.",typeof(object)));
			InPortData.Add(new PortData("val", "The transform that defines the placement of the instance in the Revit document coordinate system.",typeof(object)));
			OutPortData.Add(new PortData("out","The newly created point cloud instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var arg2=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Transform));
		}
	}
	[NodeName("Revit_PointCloudInstance_SetSelectionFilter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_PointCloudInstance_SetSelectionFilter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_SetSelectionFilter()
		{
			InPortData.Add(new PortData("val", "The filter object to be made active.  If",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointClouds.PointCloudFilter)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter));
		}
	}
	[NodeName("Revit_PointCloudInstance_GetSelectionFilter")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Currently active selection filter or")]
	public class Revit_PointCloudInstance_GetSelectionFilter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_GetSelectionFilter()
		{
			OutPortData.Add(new PortData("out","Currently active selection filter or",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewReferencePointArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An empty array that can hold ReferencePoint objects.")]
	public class Revit_Application_NewReferencePointArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewReferencePointArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can hold ReferencePoint objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewPointRelativeToPoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation is successful then a new PointRelativeToPoint object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Application_NewPointRelativeToPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPointRelativeToPoint()
		{
			InPortData.Add(new PortData("ref", "The reference of the host point.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation is successful then a new PointRelativeToPoint object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_Application_NewPointOnEdgeEdgeIntersection")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new PointOnEdgeEdgeIntersection object.")]
	public class Revit_Application_NewPointOnEdgeEdgeIntersection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPointOnEdgeEdgeIntersection()
		{
			InPortData.Add(new PortData("ref", "The first edge reference.",typeof(object)));
			InPortData.Add(new PortData("ref", "The second edge reference.",typeof(object)));
			OutPortData.Add(new PortData("out","A new PointOnEdgeEdgeIntersection object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
		}
	}
	[NodeName("Revit_Application_NewPointOnFace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new PointOnFace object.")]
	public class Revit_Application_NewPointOnFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPointOnFace()
		{
			InPortData.Add(new PortData("ref", "The reference whose face the object will be created on.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2-dimensional position.",typeof(object)));
			OutPortData.Add(new PortData("out","A new PointOnFace object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Application_NewPointOnPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new PointOnPlane object with 2-dimensional Position, XVec, and Offsetproperties set to match the given 3-dimensional arguments.")]
	public class Revit_Application_NewPointOnPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPointOnPlane()
		{
			InPortData.Add(new PortData("ref", "A reference to some planein the document. (Note: the reference must satisfyIsValidPlaneReference(), but this is not checked until this PointOnPlane objectis assigned to a ReferencePoint.)",typeof(object)));
			InPortData.Add(new PortData("uv", "Coordinates of the point's projection onto the plane;see the Position property.",typeof(object)));
			InPortData.Add(new PortData("uv", "The direction of the point'sX-coordinate vector in the plane's coordinates; see the XVec property. Optional;default value is (1, 0).",typeof(object)));
			InPortData.Add(new PortData("n", "Signed offset from the plane; see the Offset property.",typeof(object)));
			OutPortData.Add(new PortData("out","A new PointOnPlane object with 2-dimensional Position, XVec, and Offsetproperties set to match the given 3-dimensional arguments.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.UV));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewPointOnEdge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If creation was successful then a new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Revit_Application_NewPointOnEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPointOnEdge()
		{
			InPortData.Add(new PortData("ref", "The reference whose edge the object will be created on.",typeof(object)));
			InPortData.Add(new PortData("loc", "The location on the edge.",typeof(object)));
			OutPortData.Add(new PortData("out","If creation was successful then a new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.PointLocationOnCurve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.PointLocationOnCurve));
		}
	}
	[NodeName("Revit_Application_NewFamilySymbolProfile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new FamilySymbolProfile object.")]
	public class Revit_Application_NewFamilySymbolProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilySymbolProfile()
		{
			InPortData.Add(new PortData("fs", "The family symbol of the Profile.",typeof(object)));
			OutPortData.Add(new PortData("out","The new FamilySymbolProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_Application_NewCurveLoopsProfile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new CurveLoopsProfile object.")]
	public class Revit_Application_NewCurveLoopsProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewCurveLoopsProfile()
		{
			InPortData.Add(new PortData("crvs", "The curve loops of the Profile.",typeof(object)));
			OutPortData.Add(new PortData("out","The new CurveLoopsProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArrArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArrArray));
		}
	}
	[NodeName("Revit_Application_NewElementId")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new Autodesk::Revit::DB::ElementId^ object.")]
	public class Revit_Application_NewElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewElementId()
		{
			OutPortData.Add(new PortData("out","The new Autodesk::Revit::DB::ElementId^ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewAreaCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The object containing the data needed for area creation.")]
	public class Revit_Application_NewAreaCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewAreaCreationData()
		{
			InPortData.Add(new PortData("v", "The view of area element.",typeof(object)));
			InPortData.Add(new PortData("uv", "A point which lies in an enclosed region of area boundary where the new area will reside.",typeof(object)));
			OutPortData.Add(new PortData("out","The object containing the data needed for area creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ViewPlan));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Application_NewTextNoteCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewTextNoteCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewTextNoteCreationData()
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
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.TextAlignFlags));
			var arg6=(Autodesk.Revit.DB.TextNoteLeaderTypes)DynamoTypeConverter.ConvertInput(args[6],typeof(Autodesk.Revit.DB.TextNoteLeaderTypes));
			var arg7=(Autodesk.Revit.DB.TextNoteLeaderStyles)DynamoTypeConverter.ConvertInput(args[7],typeof(Autodesk.Revit.DB.TextNoteLeaderStyles));
			var arg8=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[8],typeof(Autodesk.Revit.DB.XYZ));
			var arg9=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[9],typeof(Autodesk.Revit.DB.XYZ));
			var arg10=(System.String)DynamoTypeConverter.ConvertInput(args[10],typeof(System.String));
		}
	}
	[NodeName("Revit_Application_NewTextNoteCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewTextNoteCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewTextNoteCreationData()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(object)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(object)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(object)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(Autodesk.Revit.DB.TextAlignFlags)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.TextAlignFlags));
			var arg6=(System.String)DynamoTypeConverter.ConvertInput(args[6],typeof(System.String));
		}
	}
	[NodeName("Revit_Application_NewProfiledWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewProfiledWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewProfiledWallCreationData()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewProfiledWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewProfiledWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewProfiledWallCreationData()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewProfiledWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewProfiledWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewProfiledWallCreationData()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewRectangularWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewRectangularWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewRectangularWallCreationData()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewRectangularWallCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewRectangularWallCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewRectangularWallCreationData()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("n", "The height of the wall.",typeof(object)));
			InPortData.Add(new PortData("n", "An offset distance, in feet from the specified baseline. The wall will be placed that distancefrom the baseline.",typeof(object)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewRoomCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewRoomCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewRoomCreationData()
		{
			InPortData.Add(new PortData("l", "- The level on which the room is to exist.",typeof(object)));
			InPortData.Add(new PortData("uv", "A 2D point the dictates the location on that specified level.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.FamilySymbol));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Element));
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Element));
			var arg3=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Level));
			var arg4=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Element));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(object)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewFamilyInstanceCreationData")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFamilyInstanceCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFamilyInstanceCreationData()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(object)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(object)));
			InPortData.Add(new PortData("st", "Specify if the family instance is structural.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Structure.StructuralType));
		}
	}
	[NodeName("Revit_Application_NewSpaceSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewSpaceSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewSpaceSet()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewLoadCombinationArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewLoadCombinationArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLoadCombinationArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewLoadUsageArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewLoadUsageArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLoadUsageArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewLoadCaseArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewLoadCaseArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLoadCaseArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewViewSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewViewSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewViewSet()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewIntersectionResultArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewIntersectionResultArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewIntersectionResultArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewFaceArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFaceArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFaceArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewReferenceArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewReferenceArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewReferenceArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewDoubleArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewDoubleArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewDoubleArray()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewVolumeCalculationOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewVolumeCalculationOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewVolumeCalculationOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewGBXMLImportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewGBXMLImportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewGBXMLImportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewImageImportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewImageImportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewImageImportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewBuildingSiteExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewBuildingSiteExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewBuildingSiteExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewFBXExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewFBXExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewFBXExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewGBXMLExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewGBXMLExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewGBXMLExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewDWFXExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewDWFXExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewDWFXExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewDWFExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewDWFExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewDWFExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewSATExportOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewSATExportOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewSATExportOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewUV()
		{
			InPortData.Add(new PortData("uv", "The supplied UV object",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.UV));
		}
	}
	[NodeName("Revit_Application_NewUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewUV()
		{
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewUV()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewXYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewXYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewXYZ()
		{
			InPortData.Add(new PortData("xyz", "The supplied XYZ object",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewXYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewXYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewXYZ()
		{
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(object)));
			InPortData.Add(new PortData("n", "The third coordinate.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewXYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewXYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewXYZ()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewBoundingBoxUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewBoundingBoxUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewBoundingBoxUV()
		{
			InPortData.Add(new PortData("n", "The first coordinate of min.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate of min.",typeof(object)));
			InPortData.Add(new PortData("n", "The first coordinate of max.",typeof(object)));
			InPortData.Add(new PortData("n", "The second coordinate of max.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewBoundingBoxUV")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewBoundingBoxUV : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewBoundingBoxUV()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewBoundingBoxXYZ")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewBoundingBoxXYZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewBoundingBoxXYZ()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewHermiteSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewHermiteSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewHermiteSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the start of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the end of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewHermiteSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewHermiteSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewHermiteSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(object)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewNurbSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewNurbSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewNurbSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("lst", "The weights of the nurbSpline.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(List<double>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<double>));
		}
	}
	[NodeName("Revit_Application_NewNurbSpline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewNurbSpline : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewNurbSpline()
		{
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("arr", "The weights of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("arr", "The knots of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("i", "The degree of the nurbSpline.",typeof(object)));
			InPortData.Add(new PortData("b", "The nurbSpline is closed or not.",typeof(object)));
			InPortData.Add(new PortData("b", "The nurbSpline is rational or not rational.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.DoubleArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.DoubleArray));
			var arg2=(Autodesk.Revit.DB.DoubleArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.DoubleArray));
			var arg3=(System.Int32)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Int32));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(System.Boolean)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewEllipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewEllipse : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewEllipse()
		{
			InPortData.Add(new PortData("xyz", "The center of the ellipse.",typeof(object)));
			InPortData.Add(new PortData("n", "The x vector radius of the ellipse. Should be > 0.",typeof(object)));
			InPortData.Add(new PortData("n", "The y vector radius of the ellipse. Should be > 0.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The x axis to define the ellipse plane.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The y axis to define the ellipse plane. xVec must be orthogonal with yVec.",typeof(object)));
			InPortData.Add(new PortData("n", "The raw parameter value at the start of the ellipse. Should be greater than or equal to -2PI and less than Param1.",typeof(object)));
			InPortData.Add(new PortData("n", "The raw parameter value at the end of the ellipse. Should be greater than Param0 and less than or equal to 2*PI.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.XYZ));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
			var arg6=(System.Double)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewProjectPosition")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewProjectPosition : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewProjectPosition()
		{
			InPortData.Add(new PortData("n", "East to West offset in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "North to South offset in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "Elevation above sea level in feet.",typeof(object)));
			InPortData.Add(new PortData("n", "Rotation angle away from true north in the range of -PI to +PI.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
		}
	}
	[NodeName("Revit_Application_NewArc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewArc : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewArc()
		{
			InPortData.Add(new PortData("xyz", "The start point of the arc.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The end point of the arc.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A point on the arc.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewArc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewArc : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewArc()
		{
			InPortData.Add(new PortData("xyz", "The center of the arc.",typeof(object)));
			InPortData.Add(new PortData("n", "The radius of the arc.",typeof(object)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(object)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(object)));
			InPortData.Add(new PortData("xyz", "The x axis to define the arc plane. Must be normalized.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The y axis to define the arc plane. Must be normalized.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var arg4=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[4],typeof(Autodesk.Revit.DB.XYZ));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewPoint")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPoint()
		{
			InPortData.Add(new PortData("xyz", "The coordinates of the point.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If successful a new geometric plane will be returned. Otherwise")]
	public class Revit_Application_NewPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPlane()
		{
			InPortData.Add(new PortData("crvs", "The closed loop of planar curves to locate plane.",typeof(object)));
			OutPortData.Add(new PortData("out","If successful a new geometric plane will be returned. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
		}
	}
	[NodeName("Revit_Application_NewPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new plane object.")]
	public class Revit_Application_NewPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPlane()
		{
			InPortData.Add(new PortData("xyz", "Z vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(object)));
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewPlane")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new plane object.")]
	public class Revit_Application_NewPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPlane()
		{
			InPortData.Add(new PortData("xyz", "X vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Y vector of the plane coordinate system.",typeof(object)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(object)));
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewColor")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new color object.")]
	public class Revit_Application_NewColor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewColor()
		{
			OutPortData.Add(new PortData("out","The new color object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewCombinableElementArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An empty array that can contain any CombinableElement derived objects.")]
	public class Revit_Application_NewCombinableElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewCombinableElementArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can contain any CombinableElement derived objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewVertexIndexPairArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new VertexIndexPairArray objects.")]
	public class Revit_Application_NewVertexIndexPairArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewVertexIndexPairArray()
		{
			OutPortData.Add(new PortData("out","The new VertexIndexPairArray objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewVertexIndexPair")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The new VertexIndexPair object.")]
	public class Revit_Application_NewVertexIndexPair : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewVertexIndexPair()
		{
			InPortData.Add(new PortData("i", "The index of the vertex pair from the top profile of a blend.",typeof(object)));
			InPortData.Add(new PortData("i", "The index of the vertex pair from the bottom profile of a blend.",typeof(object)));
			OutPortData.Add(new PortData("out","The new VertexIndexPair object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Int32));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
		}
	}
	[NodeName("Revit_Application_NewElementArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An empty array that can contain any Autodesk Revit element derived objects.")]
	public class Revit_Application_NewElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewElementArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can contain any Autodesk Revit element derived objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewCurveArrArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The empty array of curve loops.")]
	public class Revit_Application_NewCurveArrArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewCurveArrArray()
		{
			OutPortData.Add(new PortData("out","The empty array of curve loops.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewCurveArray")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An empty array that can hold geometric curves.")]
	public class Revit_Application_NewCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewCurveArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can hold geometric curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewStringStringMap")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A map that maps one string to another.")]
	public class Revit_Application_NewStringStringMap : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewStringStringMap()
		{
			OutPortData.Add(new PortData("out","A map that maps one string to another.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewGeometryOptions")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Application_NewGeometryOptions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewGeometryOptions()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewLineUnbound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new unbounded line object.")]
	public class Revit_Application_NewLineUnbound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLineUnbound()
		{
			InPortData.Add(new PortData("xyz", "A point through which the line will pass.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector for the direction of the line.",typeof(object)));
			OutPortData.Add(new PortData("out","A new unbounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewLineBound")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new bounded line object.")]
	public class Revit_Application_NewLineBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLineBound()
		{
			InPortData.Add(new PortData("xyz", "A start point for the line.",typeof(object)));
			InPortData.Add(new PortData("xyz", "An end point for the line.",typeof(object)));
			OutPortData.Add(new PortData("out","A new bounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
		}
	}
	[NodeName("Revit_Application_NewLine")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new bounded or unbounded line object.")]
	public class Revit_Application_NewLine : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewLine()
		{
			InPortData.Add(new PortData("xyz", "A start point or a point through which the line will pass.",typeof(object)));
			InPortData.Add(new PortData("xyz", "An end point of a vector for the direction of the line.",typeof(object)));
			InPortData.Add(new PortData("b", "Set to True if you wish the line to be bound or False is the line is to be infinite.",typeof(object)));
			OutPortData.Add(new PortData("out","A new bounded or unbounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
		}
	}
	[NodeName("Revit_Application_NewMaterialSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The newly created MaterialSet instance.")]
	public class Revit_Application_NewMaterialSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewMaterialSet()
		{
			OutPortData.Add(new PortData("out","The newly created MaterialSet instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewElementSet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new Element Set.")]
	public class Revit_Application_NewElementSet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewElementSet()
		{
			OutPortData.Add(new PortData("out","A new Element Set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewTypeBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new type binding object.")]
	public class Revit_Application_NewTypeBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewTypeBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(object)));
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CategorySet));
		}
	}
	[NodeName("Revit_Application_NewTypeBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new type binding object.")]
	public class Revit_Application_NewTypeBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewTypeBinding()
		{
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewInstanceBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new instance binding object.")]
	public class Revit_Application_NewInstanceBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewInstanceBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(object)));
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CategorySet));
		}
	}
	[NodeName("Revit_Application_NewInstanceBinding")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new instance binding object.")]
	public class Revit_Application_NewInstanceBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewInstanceBinding()
		{
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Application_NewCategorySet")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A new instance of a Category Set.")]
	public class Revit_Application_NewCategorySet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewCategorySet()
		{
			OutPortData.Add(new PortData("out","A new instance of a Category Set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Edge_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Revit_Edge_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_ComputeDerivatives()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(object)));
			OutPortData.Add(new PortData("out","The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Edge_AsCurveFollowingFace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Revit_Edge_AsCurveFollowingFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_AsCurveFollowingFace()
		{
			InPortData.Add(new PortData("f", "Specifies the face, on which the curve will follow the topological direction of the edge.",typeof(object)));
			OutPortData.Add(new PortData("out","It can be an Arc, Line, or HermiteSpline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
		}
	}
	[NodeName("Revit_Edge_AsCurve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Revit_Edge_AsCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_AsCurve()
		{
			OutPortData.Add(new PortData("out","It can be an Arc, Line, or HermiteSpline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Edge_EvaluateOnFace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Edge_EvaluateOnFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_EvaluateOnFace()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated, in [0,1].",typeof(object)));
			InPortData.Add(new PortData("f", "The face on which to perform the evaluation. Must belong to the edge.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			var arg1=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Face));
		}
	}
	[NodeName("Revit_Edge_Evaluate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Edge_Evaluate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_Evaluate()
		{
			InPortData.Add(new PortData("n", "The parameter to be evaluated, in [0,1].",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
		}
	}
	[NodeName("Revit_Edge_TessellateOnFace")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Edge_TessellateOnFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_TessellateOnFace()
		{
			InPortData.Add(new PortData("f", "The face on which to perform the tessellation. Must belong to the edge.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
		}
	}
	[NodeName("Revit_Edge_Tessellate")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Edge_Tessellate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_Tessellate()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Solid_getGeometry")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_Solid_getGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_getGeometry()
		{
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_Solid_ComputeCentroid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The XYZ point of the Centroid of this solid.")]
	public class Revit_Solid_ComputeCentroid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_ComputeCentroid()
		{
			OutPortData.Add(new PortData("out","The XYZ point of the Centroid of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryElement_GetTransformed")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("")]
	public class Revit_GeometryElement_GetTransformed : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryElement_GetTransformed()
		{
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(object)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Transform));
		}
	}
	[NodeName("Revit_GeometryElement_GetEnumeratorNG")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An IEnumerator(GeometryObject) object that can be used to iterate through the collection.")]
	public class Revit_GeometryElement_GetEnumeratorNG : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryElement_GetEnumeratorNG()
		{
			OutPortData.Add(new PortData("out","An IEnumerator(GeometryObject) object that can be used to iterate through the collection.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryElement_GetBoundingBox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The bounding box.")]
	public class Revit_GeometryElement_GetBoundingBox : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryElement_GetBoundingBox()
		{
			OutPortData.Add(new PortData("out","The bounding box.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	[NodeName("Revit_GeometryElement_GetEnumerator")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("An IEnumerator(GeometryObject) object that can be used to iterate through the collection.")]
	public class Revit_GeometryElement_GetEnumerator : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryElement_GetEnumerator()
		{
			OutPortData.Add(new PortData("out","An IEnumerator(GeometryObject) object that can be used to iterate through the collection.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
		}
	}
	}
