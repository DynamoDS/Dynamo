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
	[NodeName("HermiteFace_MixedDerivs")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITEFACE)]
	[NodeDescription("Mixed derivatives of the surface.")]
	public class HermiteFace_MixedDerivs : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteFace_MixedDerivs()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Mixed derivatives of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteFace));
			var result = ((Autodesk.Revit.DB.HermiteFace)(args[0] as Value.Container).Item).MixedDerivs;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("HermiteFace_Points")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITEFACE)]
	[NodeDescription("Interpolation points of the surface.")]
	public class HermiteFace_Points : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteFace_Points()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Interpolation points of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteFace));
			var result = ((Autodesk.Revit.DB.HermiteFace)(args[0] as Value.Container).Item).Points;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Instance_GetTotalTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_INSTANCE)]
	[NodeDescription("The calculated total transform.")]
	public class Instance_GetTotalTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Instance_GetTotalTransform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance",typeof(Autodesk.Revit.DB.Instance)));
			OutPortData.Add(new PortData("out","The calculated total transform.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Instance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Instance));
			var result = ((Autodesk.Revit.DB.Instance)(args[0] as Value.Container).Item).GetTotalTransform();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Instance_GetTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_INSTANCE)]
	[NodeDescription("The inherent transform.")]
	public class Instance_GetTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Instance_GetTransform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance",typeof(Autodesk.Revit.DB.Instance)));
			OutPortData.Add(new PortData("out","The inherent transform.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Instance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Instance));
			var result = ((Autodesk.Revit.DB.Instance)(args[0] as Value.Container).Item).GetTransform();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Mesh_MaterialElementId")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("Element ID of the material from which this mesh is composed.")]
	public class Mesh_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Mesh_MaterialElementId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this mesh is composed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = ((Autodesk.Revit.DB.Mesh)(args[0] as Value.Container).Item).MaterialElementId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Mesh_Vertices")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("Retrieves all vertices used to define this mesh. Intended for indexed access.")]
	public class Mesh_Vertices : dynRevitTransactionNodeWithOneOutput
	{
		public Mesh_Vertices()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Retrieves all vertices used to define this mesh. Intended for indexed access.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = ((Autodesk.Revit.DB.Mesh)(args[0] as Value.Container).Item).Vertices;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Mesh_NumTriangles")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("The number of triangles that the mesh contains.")]
	public class Mesh_NumTriangles : dynRevitTransactionNodeWithOneOutput
	{
		public Mesh_NumTriangles()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","The number of triangles that the mesh contains.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = ((Autodesk.Revit.DB.Mesh)(args[0] as Value.Container).Item).NumTriangles;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_GetInstanceGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the transformed instance.")]
	public class GeometryInstance_GetInstanceGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_GetInstanceGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the transformed instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var arg1=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).GetInstanceGeometry(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_GetInstanceGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the instance.")]
	public class GeometryInstance_GetInstanceGeometry_1 : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_GetInstanceGeometry_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).GetInstanceGeometry();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_GetSymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the transformed symbol.")]
	public class GeometryInstance_GetSymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_GetSymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the transformed symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var arg1=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).GetSymbolGeometry(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_GetSymbolGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the symbol.")]
	public class GeometryInstance_GetSymbolGeometry_1 : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_GetSymbolGeometry_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","An element which contains the computed geometry for the symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).GetSymbolGeometry();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_SymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The geometric representation of the symbol which generates this instance.")]
	public class GeometryInstance_SymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_SymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The geometric representation of the symbol which generates this instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).SymbolGeometry;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_Symbol")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The symbol element that this object is referring to.")]
	public class GeometryInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The symbol element that this object is referring to.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).Symbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryInstance_Transform")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.")]
	public class GeometryInstance_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryInstance_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = ((Autodesk.Revit.DB.GeometryInstance)(args[0] as Value.Container).Item).Transform;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewDividedSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("The newly created DividedSurface element.")]
	public class FamilyItemFactory_NewDividedSurface : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewDividedSurface()
		{
			InPortData.Add(new PortData("ref", "Reference to a surface on an existing element. The elementmust be one of the following:",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","The newly created DividedSurface element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDividedSurface(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewCurveByPoints")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("The newly created curve.")]
	public class FamilyItemFactory_NewCurveByPoints : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewCurveByPoints()
		{
			InPortData.Add(new PortData("val", "Two or more PointElements. The curve will interpolatethese points.",typeof(Autodesk.Revit.DB.ReferencePointArray)));
			OutPortData.Add(new PortData("out","The newly created curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePointArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferencePointArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSymbolicCurve")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("The newly created symbolic curve.")]
	public class FamilyItemFactory_NewSymbolicCurve : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSymbolicCurve()
		{
			InPortData.Add(new PortData("crv", "The geometry curve of the newly created symbolic curve.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane for the symbolic curve.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","The newly created symbolic curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSymbolicCurve(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewControl")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If successful, the newly created control is returned, otherwise anexception with error information will be thrown.")]
	public class FamilyItemFactory_NewControl : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewControl()
		{
			InPortData.Add(new PortData("val", "The shape of the control.",typeof(Autodesk.Revit.DB.ControlShape)));
			InPortData.Add(new PortData("v", "The view in which the control is to be visible. Itmust be a FloorPlan view or a CeilingPlan view.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the control.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If successful, the newly created control is returned, otherwise anexception with error information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ControlShape)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ControlShape));
			var arg1=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.View));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewControl(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewModelText")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If successful, the newly created model text is returned, otherwise anexception with error information will be thrown.")]
	public class FamilyItemFactory_NewModelText : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewModelText()
		{
			InPortData.Add(new PortData("s", "The text to be displayed.",typeof(System.String)));
			InPortData.Add(new PortData("mtt", "The type of model text. If this parameter is",typeof(Autodesk.Revit.DB.ModelTextType)));
			InPortData.Add(new PortData("sp", "The sketch plane of the model text. The direction of model text is determined by the normal of the sketch plane.To extrude in the other direction set the depth value to negative.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("xyz", "The position of the model text. The position must lie in the sketch plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("ha", "The horizontal alignment.",typeof(Autodesk.Revit.DB.HorizontalAlign)));
			InPortData.Add(new PortData("n", "The depth of the model text.",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelText(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewOpening")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If successful, the newly created opening is returned, otherwise anexception with error information will be thrown.")]
	public class FamilyItemFactory_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewOpening()
		{
			InPortData.Add(new PortData("el", "Host elements that new opening would lie in. The host can only be a wall or a ceiling.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created opening. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. The profiles will be projected into the host plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","If successful, the newly created opening is returned, otherwise anexception with error information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewOpening(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewElectricalConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Electrical Connector is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewElectricalConnector : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewElectricalConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ett", "Indicates the system type of this new Electrical connector.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","If creation was successful the new Electrical Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewElectricalConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewPipeConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new pipe connector is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewPipeConnector : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewPipeConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("pst", "Indicates the system type of this new Pipe connector.",typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)));
			OutPortData.Add(new PortData("out","If creation was successful the new pipe connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Plumbing.PipeSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewPipeConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewDuctConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Duct Connector is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewDuctConnector : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewDuctConnector()
		{
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dst", "Indicates the system type of this new duct connector.",typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)));
			OutPortData.Add(new PortData("out","If creation was successful the new Duct Connector is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Mechanical.DuctSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDuctConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewRadialDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewRadialDimension : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewRadialDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewDiameterDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new diameter dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewDiameterDimension : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewDiameterDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the diameter dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If creation was successful the new diameter dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDiameterDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewRadialDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewRadialDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewRadialDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewArcLengthDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewArcLengthDimension : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewArcLengthDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewArcLengthDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewArcLengthDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewArcLengthDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewAngularDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewAngularDimension : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewAngularDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewAngularDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewAngularDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewAngularDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Arc));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			var arg3=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewLinearDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewLinearDimension : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewLinearDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewLinearDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewLinearDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewLinearDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewFormByThickenSingleSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("This function will modify the input singleSurfaceForm and return the same element.")]
	public class FamilyItemFactory_NewFormByThickenSingleSurface : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewFormByThickenSingleSurface()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("frm", "The single-surface form element. It can have one top/bottom face or one side face.",typeof(Autodesk.Revit.DB.Form)));
			InPortData.Add(new PortData("xyz", "The offset of capped solid.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","This function will modify the input singleSurfaceForm and return the same element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Form));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByThickenSingleSurface(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewFormByCap")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class FamilyItemFactory_NewFormByCap : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewFormByCap()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of the newly created cap. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByCap(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewRevolveForms")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful new forms are returned.")]
	public class FamilyItemFactory_NewRevolveForms : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewRevolveForms()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of the newly created revolution. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("ref", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolveForms(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSweptBlendForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class FamilyItemFactory_NewSweptBlendForm : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSweptBlendForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The path of the swept blend. The path should be 2D, where all input curves lie in one plane. If there’s more than one profile, the path should be a single curve. It’s required to reference existing geometry.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created swept blend. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArrayArray)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.ReferenceArrayArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArrayArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlendForm(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewExtrusionForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful new form is returned.")]
	public class FamilyItemFactory_NewExtrusionForm : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewExtrusionForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of extrusion. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("xyz", "The direction of extrusion, with its length the length of the extrusion. The direction must be perpendicular to the plane determined by profile. The length of vector must be non-zero.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If creation was successful new form is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusionForm(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewLoftForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful form is are returned.")]
	public class FamilyItemFactory_NewLoftForm : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewLoftForm()
		{
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created loft. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArrayArray)));
			OutPortData.Add(new PortData("out","If creation was successful form is are returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.ReferenceArrayArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArrayArray));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLoftForm(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSweptBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewSweptBlend : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSweptBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("ref", "The path of the swept blend. The path might be a reference of single curve or edge obtained from existing geometry.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			OutPortData.Add(new PortData("out","If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SweepProfile));
			var arg3=(Autodesk.Revit.DB.SweepProfile)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SweepProfile));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSweptBlend_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewSweptBlend_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSweptBlend_1()
		{
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crv", "The path of the swept blend. The path should be a single curve.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSweep")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewSweep : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSweep()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The path of the sweep. The path should be reference of curve or edge obtained from existing geometry.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(System.Int32)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(Autodesk.Revit.DB.ProfilePlaneLocation)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewSweep_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewSweep_1 : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewSweep_1()
		{
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The path of the sweep. The path should be 2D, where all input curves lie in one plane, and the curves are not required to reference existing geometry.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(System.Int32)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(Autodesk.Revit.DB.ProfilePlaneLocation)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewRevolution")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new revolution is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewRevolution : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewRevolution()
		{
			InPortData.Add(new PortData("b", "Indicates if the Revolution is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created revolution. This may containmore than one curve loop. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the revolution.  The direction of revolutionis determined by the normal for the sketch plane.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crv", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolution(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new blend is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewBlend : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewBlend()
		{
			InPortData.Add(new PortData("b", "Indicates if the Blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The top blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("crvs", "The base blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the base profile. Use this to associate the base of the blend to geometry from another element. Optional, it can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","If creation was successful the new blend is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.CurveArray));
			var arg3=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.SketchPlane));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewBlend(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyItemFactory_NewExtrusion")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Extrusion is returned, otherwise an exception with failure information will be thrown.")]
	public class FamilyItemFactory_NewExtrusion : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyItemFactory_NewExtrusion()
		{
			InPortData.Add(new PortData("b", "Indicates if the Extrusion is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created Extrusion. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. All input curves must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the extrusion.  The direction of extrusionis determined by the normal for the sketch plane.  To extrude in the other direction set the end value to negative.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("n", "The length of the extrusion.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","If creation was successful the new Extrusion is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Boolean)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Boolean));
			var arg1=(Autodesk.Revit.DB.CurveArrArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArrArray));
			var arg2=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusion(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ItemFactoryBase_NewAlignment")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful the new locked alignment dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class ItemFactoryBase_NewAlignment : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewAlignment()
		{
			InPortData.Add(new PortData("v", "The view that determines the orientation of the alignment.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The first reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If creation was successful the new locked alignment dimension is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Reference));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAlignment(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewAlignment(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_PlaceGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new group is returned, otherwise")]
	public class ItemFactoryBase_PlaceGroup : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_PlaceGroup()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the group is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "A GroupType object that represents the type of group that is to be placed.",typeof(Autodesk.Revit.DB.GroupType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new group is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.GroupType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.GroupType));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.PlaceGroup(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.PlaceGroup(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewViewSection")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("The newly created section view.")]
	public class ItemFactoryBase_NewViewSection : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewViewSection()
		{
			InPortData.Add(new PortData("val", "The view volume of the section will correspond geometrically to the specified bounding box.",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","The newly created section view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewViewSection(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewViewSection(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewView3D")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("The newly created 3D view.")]
	public class ItemFactoryBase_NewView3D : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewView3D()
		{
			InPortData.Add(new PortData("xyz", "The view direction - the vector pointing from the eye towards the model.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The newly created 3D view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewView3D(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewView3D(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewTextNotes")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If the creation is successful an ElementSet which contains the TextNotes should be returned, otherwise Autodesk::Revit::Exceptions::InvalidOperationException will be thrown.")]
	public class ItemFactoryBase_NewTextNotes : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewTextNotes()
		{
			InPortData.Add(new PortData("val", "A list of TextNoteCreationData which wraps the creation arguments of the TextNotes to be created.",typeof(List<Autodesk.Revit.Creation.TextNoteCreationData>)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the TextNotes should be returned, otherwise Autodesk::Revit::Exceptions::InvalidOperationException will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.TextNoteCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.TextNoteCreationData>));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNotes(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNotes(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewTextNote")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful, a TextNote object is returned.")]
	public class ItemFactoryBase_NewTextNote : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewTextNote()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(System.Double)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(Autodesk.Revit.DB.TextAlignFlags)));
			InPortData.Add(new PortData("tnlts", "The type and alignment of the leader for the note.",typeof(Autodesk.Revit.DB.TextNoteLeaderTypes)));
			InPortData.Add(new PortData("tnls", "The style of the leader for the note.",typeof(Autodesk.Revit.DB.TextNoteLeaderStyles)));
			InPortData.Add(new PortData("xyz", "The end point for the leader.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The elbow point for the leader.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(System.String)));
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
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewTextNote_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful, a TextNote object is returned.")]
	public class ItemFactoryBase_NewTextNote_1 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewTextNote_1()
		{
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(System.Double)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(Autodesk.Revit.DB.TextAlignFlags)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(System.String)));
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
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewTextNote(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewSketchPlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.")]
	public class ItemFactoryBase_NewSketchPlane : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewSketchPlane()
		{
			InPortData.Add(new PortData("ref", "The planar face reference to locate sketch plane.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewSketchPlane_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.")]
	public class ItemFactoryBase_NewSketchPlane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewSketchPlane_1()
		{
			InPortData.Add(new PortData("val", "The geometry planar face to locate sketch plane.",typeof(Autodesk.Revit.DB.PlanarFace)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PlanarFace)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.PlanarFace));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewSketchPlane_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise")]
	public class ItemFactoryBase_NewSketchPlane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewSketchPlane_2()
		{
			InPortData.Add(new PortData("p", "The geometry plane to locate sketch plane.",typeof(Autodesk.Revit.DB.Plane)));
			OutPortData.Add(new PortData("out","If successful a new sketch plane will be returned. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Plane)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Plane));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewSketchPlane(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewReferencePlane2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("The newly created reference plane.")]
	public class ItemFactoryBase_NewReferencePlane2 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewReferencePlane2()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A third point needed to define the reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.View));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePlane2(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewReferencePlane2(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewReferencePlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("The newly created reference plane.")]
	public class ItemFactoryBase_NewReferencePlane : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewReferencePlane()
		{
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The cut vector apply to reference plane, should perpendicular to the vector  (bubbleEnd-freeEnd).",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","The newly created reference plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.View));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePlane(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewReferencePlane(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewViewPlan")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("if successful, a new plan view object within the project, otherwise")]
	public class ItemFactoryBase_NewViewPlan : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewViewPlan()
		{
			InPortData.Add(new PortData("s", "The name for the new plan view, must be unique or",typeof(System.String)));
			InPortData.Add(new PortData("l", "The level on which the plan view is to be associated.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The type of plan view to be created.",typeof(Autodesk.Revit.DB.ViewPlanType)));
			OutPortData.Add(new PortData("out","if successful, a new plan view object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.ViewPlanType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ViewPlanType));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewViewPlan(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewViewPlan(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewLevel")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("The newly created level.")]
	public class ItemFactoryBase_NewLevel : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewLevel()
		{
			InPortData.Add(new PortData("n", "The elevation to apply to the new level.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The newly created level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Double)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Double));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLevel(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewLevel(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewModelCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new model line element. Otherwise")]
	public class ItemFactoryBase_NewModelCurve : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewModelCurve()
		{
			InPortData.Add(new PortData("crv", "The internal geometry curve for model line.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane this new model line resides in.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","If successful a new model line element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewModelCurve(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("A new instance of a group containing the elements specified.")]
	public class ItemFactoryBase_NewGroup : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewGroup()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(List<Autodesk.Revit.DB.ElementId>)));
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.ElementId>));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewGroup(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewGroup(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewGroup_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("A new instance of a group containing the elements specified.")]
	public class ItemFactoryBase_NewGroup_1 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewGroup_1()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(Autodesk.Revit.DB.ElementSet)));
			OutPortData.Add(new PortData("out","A new instance of a group containing the elements specified.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ElementSet));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewGroup(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewGroup(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstances2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If the creation is successful, a set of ElementIds which contains the Family instances should be returned, otherwise the exception will be thrown.")]
	public class ItemFactoryBase_NewFamilyInstances2 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstances2()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)));
			OutPortData.Add(new PortData("out","If the creation is successful, a set of ElementIds which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstances2(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstances2(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstances")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If the creation is successful an ElementSet which contains the Family instances should be returned, otherwise the exception will be thrown.")]
	public class ItemFactoryBase_NewFamilyInstances : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstances()
		{
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the Family instances should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.FamilyInstanceCreationData>));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstances(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstances(arg0);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned.")]
	public class ItemFactoryBase_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance()
		{
			InPortData.Add(new PortData("xyz", "The origin of family instance. If created on a",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("v", "The 2D view in which to place the family instance.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_1 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_1()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted. Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.FamilySymbol));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_2 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_2()
		{
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.FamilySymbol));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_3")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_3 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_3()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.FamilySymbol));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_4")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_4 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_4()
		{
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","An instance of the new object if creation was successful, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.FamilySymbol));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_5")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_5 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_5()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Structure.StructuralType));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_6")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_6 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_6()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "The object into which the FamilyInstance is to be inserted, often known as the host.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Element));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewFamilyInstance_7")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class ItemFactoryBase_NewFamilyInstance_7 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewFamilyInstance_7()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
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
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewDimension")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new dimension object, otherwise")]
	public class ItemFactoryBase_NewDimension : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewDimension()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg3=(Autodesk.Revit.DB.DimensionType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.DimensionType));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDimension(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDimension(arg0,arg1,arg2,arg3);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewDimension_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new dimension object, otherwise")]
	public class ItemFactoryBase_NewDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewDimension_1()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If successful a new dimension object, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Line));
			var arg2=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ReferenceArray));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDimension(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDimension(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewDetailCurveArray")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful an array of new detail curve elements. Otherwise")]
	public class ItemFactoryBase_NewDetailCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewDetailCurveArray()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curves are to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crvs", "An array containing the internal geometry curves for detail lines. The curve in array should be bound curve.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","If successful an array of new detail curve elements. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDetailCurveArray(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDetailCurveArray(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewDetailCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new detail curve element. Otherwise")]
	public class ItemFactoryBase_NewDetailCurve : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewDetailCurve()
		{
			InPortData.Add(new PortData("v", "The view in which the detail curve is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The internal geometry curve for detail curve. It should be a bound curve.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","If successful a new detail curve element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.View));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDetailCurve(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewDetailCurve(arg0,arg1);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("ItemFactoryBase_NewAnnotationSymbol")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class ItemFactoryBase_NewAnnotationSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public ItemFactoryBase_NewAnnotationSymbol()
		{
			InPortData.Add(new PortData("xyz", "The origin of the annotation symbol. If created on",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "An annotation symbol type that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.AnnotationSymbolType)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the annotation symbol.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.AnnotationSymbolType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.AnnotationSymbolType));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			if (dynRevitSettings.Doc.Document.IsFamilyDocument)
			{
				var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAnnotationSymbol(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
			else
			{
				var result = dynRevitSettings.Doc.Document.Create.NewAnnotationSymbol(arg0,arg1,arg2);
				return DynamoTypeConverter.ConvertToValue(result);
			}
		}
	}

	[NodeName("PolyLine_NumberOfCoordinates")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POLYLINE)]
	[NodeDescription("Gets the number of the coordinate points.")]
	public class PolyLine_NumberOfCoordinates : dynRevitTransactionNodeWithOneOutput
	{
		public PolyLine_NumberOfCoordinates()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine",typeof(Autodesk.Revit.DB.PolyLine)));
			OutPortData.Add(new PortData("out","Gets the number of the coordinate points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PolyLine)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PolyLine));
			var result = ((Autodesk.Revit.DB.PolyLine)(args[0] as Value.Container).Item).NumberOfCoordinates;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_AngleOnPlaneTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number between 0 and 2*PI equal to the projected angle between the two vectors.")]
	public class XYZ_AngleOnPlaneTo : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_AngleOnPlaneTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The normal vector that defines the plane.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number between 0 and 2*PI equal to the projected angle between the two vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).AngleOnPlaneTo(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_AngleTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number between 0 and PI equal to the angle between the two vectors in radians..")]
	public class XYZ_AngleTo : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_AngleTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number between 0 and PI equal to the angle between the two vectors in radians..",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).AngleTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_DistanceTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the distance between the two points.")]
	public class XYZ_DistanceTo : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_DistanceTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number equal to the distance between the two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).DistanceTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_IsAlmostEqualTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class XYZ_IsAlmostEqualTo : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_IsAlmostEqualTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","True if the vectors are the same; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).IsAlmostEqualTo(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_IsAlmostEqualTo_1")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class XYZ_IsAlmostEqualTo_1 : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_IsAlmostEqualTo_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","True if the vectors are the same; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).IsAlmostEqualTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Divide")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The divided vector.")]
	public class XYZ_Divide : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Divide()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The divided vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Divide(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Multiply")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The multiplied vector.")]
	public class XYZ_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Multiply()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The multiplied vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Multiply(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Negate")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector opposite to this vector.")]
	public class XYZ_Negate : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Negate()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The vector opposite to this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Negate();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Subtract")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the difference between the two vectors.")]
	public class XYZ_Subtract : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Subtract()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to subtract from this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The vector equal to the difference between the two vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Subtract(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Add")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the sum of the two vectors.")]
	public class XYZ_Add : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Add()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to add to this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The vector equal to the sum of the two vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Add(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_TripleProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the triple product.")]
	public class XYZ_TripleProduct : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_TripleProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The third vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number equal to the triple product.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).TripleProduct(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_CrossProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the cross product.")]
	public class XYZ_CrossProduct : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_CrossProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The vector equal to the cross product.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).CrossProduct(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_DotProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the dot product.")]
	public class XYZ_DotProduct : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_DotProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number equal to the dot product.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).DotProduct(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Normalize")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The normalized XYZ or zero if the vector is almost Zero.")]
	public class XYZ_Normalize : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Normalize()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The normalized XYZ or zero if the vector is almost Zero.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Normalize();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Z")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the third coordinate.")]
	public class XYZ_Z : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Z()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the third coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Z;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_Y")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the second coordinate.")]
	public class XYZ_Y : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_Y()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).Y;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("XYZ_X")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the first coordinate.")]
	public class XYZ_X : dynRevitTransactionNodeWithOneOutput
	{
		public XYZ_X()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.XYZ)(args[0] as Value.Container).Item).X;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetPointConstraintType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Constraint type of the Adaptive Shape Handle Point.")]
	public class AdaptiveComponentFamilyUtils_GetPointConstraintType : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetPointConstraintType()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Constraint type of the Adaptive Shape Handle Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointConstraintType(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetPointOrientationType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Orientation type of Adaptive Placement Point.")]
	public class AdaptiveComponentFamilyUtils_GetPointOrientationType : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetPointOrientationType()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Orientation type of Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointOrientationType(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetPlacementNumber")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Placement number of the Adaptive Placement Point.")]
	public class AdaptiveComponentFamilyUtils_GetPlacementNumber : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetPlacementNumber()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Placement number of the Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPlacementNumber(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Shape Handle Point Element References in the Adaptive Component Family.")]
	public class AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Number of Adaptive Shape Handle Point Element References in the Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyBase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyBase));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfShapeHandlePoints(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Placement Point Element References in Adaptive Component Family.")]
	public class AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Number of Adaptive Placement Point Element References in Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyBase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyBase));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfPlacementPoints(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Point Element References in Adaptive Component Family.")]
	public class AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Number of Adaptive Point Element References in Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyBase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyBase));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfAdaptivePoints(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Shape Handle Point.")]
	public class AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","True if the Point is an Adaptive Shape Handle Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveShapeHandlePoint(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Placement Point.")]
	public class AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","True if the Point is an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePlacementPoint(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_IsAdaptivePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Point (Placement Point or Shape Handle Point).")]
	public class AdaptiveComponentFamilyUtils_IsAdaptivePoint : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_IsAdaptivePoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","True if the Point is an Adaptive Point (Placement Point or Shape Handle Point).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePoint(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Family is an Adaptive Component Family.")]
	public class AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","True if the Family is an Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyBase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyBase));
			var result = Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("CylindricalFace_Axis")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CYLINDRICALFACE)]
	[NodeDescription("Axis of the surface.")]
	public class CylindricalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public CylindricalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CylindricalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.CylindricalFace));
			var result = ((Autodesk.Revit.DB.CylindricalFace)(args[0] as Value.Container).Item).Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("CylindricalFace_Origin")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CYLINDRICALFACE)]
	[NodeDescription("Origin of the surface.")]
	public class CylindricalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public CylindricalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CylindricalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.CylindricalFace));
			var result = ((Autodesk.Revit.DB.CylindricalFace)(args[0] as Value.Container).Item).Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ConicalFace_HalfAngle")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Half angle of the surface.")]
	public class ConicalFace_HalfAngle : dynRevitTransactionNodeWithOneOutput
	{
		public ConicalFace_HalfAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Half angle of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = ((Autodesk.Revit.DB.ConicalFace)(args[0] as Value.Container).Item).HalfAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ConicalFace_Axis")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Axis of the surface.")]
	public class ConicalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public ConicalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = ((Autodesk.Revit.DB.ConicalFace)(args[0] as Value.Container).Item).Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ConicalFace_Origin")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Origin of the surface.")]
	public class ConicalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public ConicalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = ((Autodesk.Revit.DB.ConicalFace)(args[0] as Value.Container).Item).Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewTopographySurface")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The TopographySurface element.")]
	public class Document_NewTopographySurface : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewTopographySurface()
		{
			InPortData.Add(new PortData("lst", "An array of initial points for the surface.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			OutPortData.Add(new PortData("out","The TopographySurface element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var result = dynRevitSettings.Doc.Document.Create.NewTopographySurface(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewTakeoffFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Document_NewTakeoffFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewTakeoffFitting()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the takeoff.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("mepcrv", "The duct or pipe which is the trunk for the takeoff.",typeof(Autodesk.Revit.DB.MEPCurve)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.MEPCurve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.MEPCurve));
			var result = dynRevitSettings.Doc.Document.Create.NewTakeoffFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewUnionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Document_NewUnionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewUnionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the union.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the union.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var result = dynRevitSettings.Doc.Document.Create.NewUnionFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewCrossFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.")]
	public class Document_NewCrossFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewCrossFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The fourth connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Connector));
			var arg3=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Connector));
			var result = dynRevitSettings.Doc.Document.Create.NewCrossFitting(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewTransitionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Document_NewTransitionFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewTransitionFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the transition.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the transition.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var result = dynRevitSettings.Doc.Document.Create.NewTransitionFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewTeeFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.")]
	public class Document_NewTeeFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewTeeFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the tee.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the tee.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the tee. This should be connected to the branch of the tee.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,and the transition fitting will be added at the connectors’ end if necessary, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Connector));
			var result = dynRevitSettings.Doc.Document.Create.NewTeeFitting(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewElbowFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Document_NewElbowFitting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewElbowFitting()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the elbow.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the elbow.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","If creation was successful then an family instance to the new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var result = dynRevitSettings.Doc.Document.Create.NewElbowFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexPipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexPipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexPipe_1()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the flexible pipe, including the end points.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexPipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexPipe_2()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe, including the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.Plumbing.FlexPipeType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewPipe : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPipe()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewPipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPipe_1()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("con", "The connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewPipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPipe_2()
		{
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeType));
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexDuct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexDuct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexDuct_1()
		{
			InPortData.Add(new PortData("con", "The connector to be connected to the duct, including the end points.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg2=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFlexDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewFlexDuct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFlexDuct_2()
		{
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct, including the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.Mechanical.FlexDuctType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewDuct : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewDuct()
		{
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.")]
	public class Document_NewDuct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewDuct_1()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("con", "The connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Connector));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Document_NewDuct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewDuct_2()
		{
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctType));
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFamilyInstance")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Document_NewFamilyInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFamilyInstance()
		{
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFamilyInstance_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Document_NewFamilyInstance_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFamilyInstance_1()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance to the new object is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(Autodesk.Revit.DB.Structure.StructuralType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.StructuralType));
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFamilyInstance_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Document_NewFamilyInstance_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFamilyInstance_2()
		{
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed on the specified level.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFascia")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new fascia object within the project, otherwise")]
	public class Document_NewFascia : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFascia()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(Autodesk.Revit.DB.Architecture.FasciaType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the fascia.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.FasciaType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFascia_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new fascia object within the project, otherwise")]
	public class Document_NewFascia_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFascia_1()
		{
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(Autodesk.Revit.DB.Architecture.FasciaType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the fascia.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If successful a new fascia object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.FasciaType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.FasciaType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewGutter")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new gutter object within the project, otherwise")]
	public class Document_NewGutter : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewGutter()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(Autodesk.Revit.DB.Architecture.GutterType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the gutter.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.GutterType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewGutter_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new gutter object within the project, otherwise")]
	public class Document_NewGutter_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewGutter_1()
		{
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(Autodesk.Revit.DB.Architecture.GutterType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the gutter.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If successful a new gutter object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.GutterType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.GutterType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSlabEdge")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new slab edge object within the project, otherwise")]
	public class Document_NewSlabEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSlabEdge()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(Autodesk.Revit.DB.SlabEdgeType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the slab edge.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SlabEdgeType));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSlabEdge_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new slab edge object within the project, otherwise")]
	public class Document_NewSlabEdge_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSlabEdge_1()
		{
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(Autodesk.Revit.DB.SlabEdgeType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the slab edge.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","If successful a new slab edge object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SlabEdgeType)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.SlabEdgeType));
			var arg1=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ReferenceArray));
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewCurtainSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The CurtainSystem created will be returned when the operation succeeds.")]
	public class Document_NewCurtainSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewCurtainSystem()
		{
			InPortData.Add(new PortData("val", "The faces new CurtainSystem will be created on.",typeof(Autodesk.Revit.DB.FaceArray)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(Autodesk.Revit.DB.CurtainSystemType)));
			OutPortData.Add(new PortData("out","The CurtainSystem created will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FaceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FaceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewCurtainSystem2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("A set of ElementIds of CurtainSystems will be returned when the operation succeeds.")]
	public class Document_NewCurtainSystem2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewCurtainSystem2()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(Autodesk.Revit.DB.CurtainSystemType)));
			OutPortData.Add(new PortData("out","A set of ElementIds of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewCurtainSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("A set of CurtainSystems will be returned when the operation succeeds.")]
	public class Document_NewCurtainSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewCurtainSystem_1()
		{
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(Autodesk.Revit.DB.CurtainSystemType)));
			OutPortData.Add(new PortData("out","A set of CurtainSystems will be returned when the operation succeeds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferenceArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ReferenceArray));
			var arg1=(Autodesk.Revit.DB.CurtainSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurtainSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWire")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wire element within the project, otherwise")]
	public class Document_NewWire : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWire()
		{
			InPortData.Add(new PortData("crv", "The base line of the wire.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("v", "The view in which the wire is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("con", "The connector which connects with the start point connector of wire, if it is",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The connector which connects with the end point connector of wire, if it is",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Specify wire type of new created wire.",typeof(Autodesk.Revit.DB.Electrical.WireType)));
			InPortData.Add(new PortData("val", "Specify wiring type(Arc or chamfer) of new created wire.",typeof(Autodesk.Revit.DB.Electrical.WiringType)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewWire(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewZone")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new Zone element within the project, otherwise")]
	public class Document_NewZone : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewZone()
		{
			InPortData.Add(new PortData("l", "The level on which the Zone is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The associative phase on which the Zone is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","If successful a new Zone element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var result = dynRevitSettings.Doc.Document.Create.NewZone(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpaceTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a SpaceTag object will be returned, otherwise")]
	public class Document_NewSpaceTag : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpaceTag()
		{
			InPortData.Add(new PortData("val", "The Space which the tag refers.",typeof(Autodesk.Revit.DB.Mechanical.Space)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the space.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If successful a SpaceTag object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mechanical.Space)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Mechanical.Space));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			var result = dynRevitSettings.Doc.Document.Create.NewSpaceTag(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpaces2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewSpaces2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpaces2_1()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces2(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpaces_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewSpaces_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpaces_1()
		{
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpace")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new Space element within the project, otherwise")]
	public class Document_NewSpace : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpace()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","If successful a new Space element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var arg2=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.UV));
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpace_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new space element is returned, otherwise")]
	public class Document_NewSpace_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpace_1()
		{
			InPortData.Add(new PortData("l", "The level on which the space is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","If successful the new space element is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpace_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new space should be returned, otherwise")]
	public class Document_NewSpace_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpace_2()
		{
			InPortData.Add(new PortData("val", "The phase in which the space is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","If successful the new space should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPipingSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance of piping system is returned, otherwise an exception with information will be thrown.")]
	public class Document_NewPipingSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPipingSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(Autodesk.Revit.DB.ConnectorSet)));
			InPortData.Add(new PortData("pst", "The System type.",typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance of piping system is returned, otherwise an exception with information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.ConnectorSet)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ConnectorSet));
			var arg2=(Autodesk.Revit.DB.Plumbing.PipeSystemType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewPipingSystem(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewMechanicalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance of mechanical system is returned, otherwise an exception with information will be thrown.")]
	public class Document_NewMechanicalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewMechanicalSystem()
		{
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(Autodesk.Revit.DB.ConnectorSet)));
			InPortData.Add(new PortData("dst", "The system type.",typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)));
			OutPortData.Add(new PortData("out","If creation was successful then an instance of mechanical system is returned, otherwise an exception with information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.ConnectorSet)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ConnectorSet));
			var arg2=(Autodesk.Revit.DB.Mechanical.DuctSystemType)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewMechanicalSystem(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewElectricalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Document_NewElectricalSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewElectricalSystem()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(List<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.ElementId>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.ElementId>));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewElectricalSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Document_NewElectricalSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewElectricalSystem_1()
		{
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(Autodesk.Revit.DB.ElementSet)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ElementSet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ElementSet));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewElectricalSystem_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Document_NewElectricalSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewElectricalSystem_2()
		{
			InPortData.Add(new PortData("con", "The Connector to create this Electrical System.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","If successful a new MEP Electrical System element within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Connector)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Connector));
			var arg1=(Autodesk.Revit.DB.Electrical.ElectricalSystemType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType));
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreas")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element Set which contains the areas should be returned, otherwise the exception will be thrown.")]
	public class Document_NewAreas : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreas()
		{
			InPortData.Add(new PortData("val", "A list of AreaCreationData which wraps the creation arguments of the areas to be created.",typeof(List<Autodesk.Revit.Creation.AreaCreationData>)));
			OutPortData.Add(new PortData("out","If successful an Element Set which contains the areas should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.AreaCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.AreaCreationData>));
			var result = dynRevitSettings.Doc.Document.Create.NewAreas(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new floor object within the project, otherwise")]
	public class Document_NewSlab : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSlab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the slab.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the slab is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("crv", "A line use to control the sloped angle of the slab. It should be in the same face with profile.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The slope.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewSlab(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an IndependentTag object is returned.")]
	public class Document_NewTag : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewTag()
		{
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("el", "The host object of tag.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("b", "Whether there will be a leader.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The mode of the tag. Add by Category, add by Multi-Category, or add by material.",typeof(Autodesk.Revit.DB.TagMode)));
			InPortData.Add(new PortData("val", "The orientation of the tag.",typeof(Autodesk.Revit.DB.TagOrientation)));
			InPortData.Add(new PortData("xyz", "The position of the tag.",typeof(Autodesk.Revit.DB.XYZ)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewTag(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewOpening")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Document_NewOpening : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewOpening()
		{
			InPortData.Add(new PortData("el", "Host element of the opening. Can be a roof, floor, or ceiling.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "Profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "True if the profile is cut perpendicular to the intersecting face of the host. False if the profile is cut vertically.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewOpening_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Document_NewOpening_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewOpening_1()
		{
			InPortData.Add(new PortData("val", "Host element of the opening.",typeof(Autodesk.Revit.DB.Wall)));
			InPortData.Add(new PortData("xyz", "One corner of the rectangle.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The opposite corner of the rectangle.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Wall));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewOpening_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Document_NewOpening_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewOpening_2()
		{
			InPortData.Add(new PortData("l", "bottom level",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("l", "top level",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.CurveArray));
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewOpening_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Document_NewOpening_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewOpening_3()
		{
			InPortData.Add(new PortData("el", "host element of the opening, can be a beam, brace and column.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "face on which opening is based on.",typeof(Autodesk.Revit.Creation.eRefFace)));
			OutPortData.Add(new PortData("out","If successful, an Opening object is returned.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(Autodesk.Revit.Creation.eRefFace)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.Creation.eRefFace));
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".")]
	public class Document_NewAreaBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "A Wall, Slab or Slab Foundation to host the boundary conditions.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".")]
	public class Document_NewLineBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineBoundaryConditions()
		{
			InPortData.Add(new PortData("el", "A Beam.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".")]
	public class Document_NewAreaBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaBoundaryConditions_1()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference obtained from a Wall, Slab or Slab Foundation.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".")]
	public class Document_NewLineBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineBoundaryConditions_1()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to a Beam's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical line.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPointBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewPointBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 0 - \"Point\".")]
	public class Document_NewPointBoundaryConditions : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPointBoundaryConditions()
		{
			InPortData.Add(new PortData("ref", "A Geometry reference to a Beam's, Brace's or Column's analytical line end.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the Y axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for Y axis. Ignored if Y_Rotation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the Z axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for Z axis. Ignored if Y_Rotation is not \"Spring\".",typeof(System.Double)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10,arg11,arg12);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewBeamSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Document_NewBeamSystem : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewBeamSystem()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem. This argument is optional – may be null.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Whether the BeamSystem is 3D or not",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewBeamSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Document_NewBeamSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewBeamSystem_1()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewBeamSystem_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Document_NewBeamSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewBeamSystem_2()
		{
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the sketch plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "If the BeamSystem is 3D, the sketchPlane must be a level, oran exception will be thrown.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewBeamSystem_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Document_NewBeamSystem_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewBeamSystem_3()
		{
			InPortData.Add(new PortData("crvs", "The profile is the profile of the BeamSystem.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","If successful a new BeamSystem object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.SketchPlane)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.SketchPlane));
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRoomTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a RoomTag object will be returned, otherwise")]
	public class Document_NewRoomTag : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRoomTag()
		{
			InPortData.Add(new PortData("val", "The Room which the tag refers.",typeof(Autodesk.Revit.DB.Architecture.Room)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the room.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","If successful a RoomTag object will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.Room));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.View)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.View));
			var result = dynRevitSettings.Doc.Document.Create.NewRoomTag(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewRooms2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms2()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewRooms2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms2_1()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms2_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms created should be returned, otherwise")]
	public class Document_NewRooms2_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms2_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","If successful, a set of ElementIds which contains the rooms created should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contain the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewRooms : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms()
		{
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewRooms_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms_1()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Phase));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contain the rooms created should be returned, otherwise")]
	public class Document_NewRooms_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","If successful an Element set which contain the rooms created should be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRooms_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Document_NewRooms_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRooms_3()
		{
			InPortData.Add(new PortData("val", "A list of RoomCreationData which wraps the creation arguments of the rooms to be created.",typeof(List<Autodesk.Revit.Creation.RoomCreationData>)));
			OutPortData.Add(new PortData("out","If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RoomCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.RoomCreationData>));
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRoom")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the room is returned, otherwise")]
	public class Document_NewRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRoom()
		{
			InPortData.Add(new PortData("val", "The room which you want to locate in the circuit.  Pass",typeof(Autodesk.Revit.DB.Architecture.Room)));
			InPortData.Add(new PortData("val", "The circuit in which you want to locate a room.",typeof(Autodesk.Revit.DB.PlanCircuit)));
			OutPortData.Add(new PortData("out","If successful the room is returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Architecture.Room)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Architecture.Room));
			var arg1=(Autodesk.Revit.DB.PlanCircuit)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.PlanCircuit));
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRoom_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new room , otherwise")]
	public class Document_NewRoom_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRoom_1()
		{
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","If successful the new room , otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Phase)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Phase));
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewRoom_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new room will be returned, otherwise")]
	public class Document_NewRoom_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewRoom_2()
		{
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location of the room on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","If successful the new room will be returned, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Level));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewGrids")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("An Element set that contains the Grids.")]
	public class Document_NewGrids : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewGrids()
		{
			InPortData.Add(new PortData("crvs", "The curves which represent the new grid lines.  These curves must be lines or bounded arcs.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","An Element set that contains the Grids.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var result = dynRevitSettings.Doc.Document.Create.NewGrids(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewGrid")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The newly created grid line.")]
	public class Document_NewGrid : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewGrid()
		{
			InPortData.Add(new PortData("arc", "An arc object that represents the location of the new grid line.",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Arc));
			var result = dynRevitSettings.Doc.Document.Create.NewGrid(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewGrid_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The newly created grid line.")]
	public class Document_NewGrid_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewGrid_1()
		{
			InPortData.Add(new PortData("crv", "A line object which represents the location of the grid line.",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","The newly created grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Line));
			var result = dynRevitSettings.Doc.Document.Create.NewGrid(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewViewSheet")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The newly created sheet view.")]
	public class Document_NewViewSheet : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewViewSheet()
		{
			InPortData.Add(new PortData("fs", "The titleblock family symbol to apply to this sheet.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","The newly created sheet view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilySymbol));
			var result = dynRevitSettings.Doc.Document.Create.NewViewSheet(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewViewDrafting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The newly created drafting view.")]
	public class Document_NewViewDrafting : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewViewDrafting()
		{
			OutPortData.Add(new PortData("out","The newly created drafting view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFoundationSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("if successful, a new foundation slab object within the project, otherwise")]
	public class Document_NewFoundationSlab : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFoundationSlab()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(Autodesk.Revit.DB.XYZ)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewFoundationSlab(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFloor")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("if successful, a new floor object within the project, otherwise")]
	public class Document_NewFloor : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFloor()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(Autodesk.Revit.DB.XYZ)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFloor_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("if successful, a new floor object within the project, otherwise")]
	public class Document_NewFloor_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFloor_1()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","if successful, a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.FloorType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FloorType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewFloor_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new floor object within the project, otherwise")]
	public class Document_NewFloor_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewFloor_2()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new floor object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWalls")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.")]
	public class Document_NewWalls : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWalls()
		{
			InPortData.Add(new PortData("val", "A list of ProfiledWallCreationData which wraps the creation arguments of the walls to be created.",typeof(List<Autodesk.Revit.Creation.ProfiledWallCreationData>)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.ProfiledWallCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.ProfiledWallCreationData>));
			var result = dynRevitSettings.Doc.Document.Create.NewWalls(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWalls_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.")]
	public class Document_NewWalls_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWalls_1()
		{
			InPortData.Add(new PortData("val", "A list of RectangularWallCreationData which wraps the creation arguments of the walls to be created.",typeof(List<Autodesk.Revit.Creation.RectangularWallCreationData>)));
			OutPortData.Add(new PortData("out","If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.Creation.RectangularWallCreationData>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.Creation.RectangularWallCreationData>));
			var result = dynRevitSettings.Doc.Document.Create.NewWalls(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWall")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Document_NewWall : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWall()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is consideredto be inside and outside.",typeof(Autodesk.Revit.DB.XYZ)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWall_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Document_NewWall_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWall_1()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(Autodesk.Revit.DB.WallType)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.WallType));
			var arg2=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Level));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWall_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Document_NewWall_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWall_2()
		{
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var arg1=(System.Boolean)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWall_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Document_NewWall_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWall_3()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewWall_4")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Document_NewWall_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewWall_4()
		{
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project, otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Level));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpotElevation")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new spot dimension object, otherwise")]
	public class Document_NewSpotElevation : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpotElevation()
		{
			InPortData.Add(new PortData("v", "The view in which the spot elevation is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The reference to which the spot elevation is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point which the spot elevation evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot elevation.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point for the spot elevation.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot elevation evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(System.Boolean)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpotElevation(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewSpotCoordinate")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new spot dimension object, otherwise")]
	public class Document_NewSpotCoordinate : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewSpotCoordinate()
		{
			InPortData.Add(new PortData("v", "The view in which the spot coordinate is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The reference to which the spot coordinate is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point which the spot coordinate evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot coordinate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point for the spot coordinate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot coordinate evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(System.Boolean)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpotCoordinate(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLoadCombination")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLoadCombination and there isn't the Load Combination Element     with the same name returns an object for the newly created LoadCombination.     If such element exist and match desired one (has the same formula and the same    usages set), returns existing element. Otherwise")]
	public class Document_NewLoadCombination : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLoadCombination()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Combination Element to create.",typeof(System.String)));
			InPortData.Add(new PortData("i", "LoadCombination Type Index: 0-Combination, 1-Envelope.",typeof(System.Int32)));
			InPortData.Add(new PortData("i", "LoadCombination State Index: 0-Servicebility, 1-Ultimate.",typeof(System.Int32)));
			InPortData.Add(new PortData("val", "Factors array for Load Combination formula.",typeof(System.Double[])));
			InPortData.Add(new PortData("val", "Load Cases array for Load Combination formula.",typeof(Autodesk.Revit.DB.Structure.LoadCaseArray)));
			InPortData.Add(new PortData("val", "Load Combinations array for Load Combination formula.",typeof(Autodesk.Revit.DB.Structure.LoadCombinationArray)));
			InPortData.Add(new PortData("val", "Load Usages array.",typeof(Autodesk.Revit.DB.Structure.LoadUsageArray)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCombination(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLoadCase")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLoadCase and there isn't the Load Case Element     with the same name returns an object for the newly created LoadCase.     If such element exist and match desired one (has the same nature and number),     returns existing element. Otherwise")]
	public class Document_NewLoadCase : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLoadCase()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Case Element to create.",typeof(System.String)));
			InPortData.Add(new PortData("val", "The Load Case nature.",typeof(Autodesk.Revit.DB.Structure.LoadNature)));
			InPortData.Add(new PortData("val", "The Load Case category.",typeof(Autodesk.Revit.DB.Category)));
			OutPortData.Add(new PortData("out","If successful, NewLoadCase and there isn't the Load Case Element     with the same name returns an object for the newly created LoadCase.     If such element exist and match desired one (has the same nature and number),     returns existing element. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var arg1=(Autodesk.Revit.DB.Structure.LoadNature)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Structure.LoadNature));
			var arg2=(Autodesk.Revit.DB.Category)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Category));
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCase(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLoadUsage")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful and there isn't the Load Usage Element with the    same name NewLoadUsage returns an object for the newly created LoadUsage.     If such element exist it returns existing element.")]
	public class Document_NewLoadUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLoadUsage()
		{
			InPortData.Add(new PortData("s", "The not empty name for the Load Usage Element to create.",typeof(System.String)));
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Usage Element with the    same name NewLoadUsage returns an object for the newly created LoadUsage.     If such element exist it returns existing element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var result = dynRevitSettings.Doc.Document.Create.NewLoadUsage(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLoadNature")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful and there isn't the Load Nature Element with the    same name NewLoadNature returns an object for the newly created LoadNature.     If such element exist it returns existing element.")]
	public class Document_NewLoadNature : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLoadNature()
		{
			InPortData.Add(new PortData("s", "The name for the Load Nature Element to create.",typeof(System.String)));
			OutPortData.Add(new PortData("out","If successful and there isn't the Load Nature Element with the    same name NewLoadNature returns an object for the newly created LoadNature.     If such element exist it returns existing element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.String)DynamoTypeConverter.ConvertInput(args[0],typeof(System.String));
			var result = dynRevitSettings.Doc.Document.Create.NewLoadNature(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Document_NewAreaLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaLoad()
		{
			InPortData.Add(new PortData("el", "The host element (Floor or Wall) of the AreaLoad application.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Document_NewAreaLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaLoad_1()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(System.Int32[])));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(System.Int32[])));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the second reference point. Ignored if only one or two reference points are supplied.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the third reference point.  Ignored if only one or two reference points are supplied.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Document_NewAreaLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaLoad_2()
		{
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load curves.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(System.Int32[])));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(System.Int32[])));
			InPortData.Add(new PortData("lst", "The 3d area forces applied to each of the reference points in the refPntIdxs array.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewAreaLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Document_NewAreaLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewAreaLoad_3()
		{
			InPortData.Add(new PortData("lst", "Vertexes of AreaLoad shape polygon.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("xyz", "The applied 3d area force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","If successful, NewAreaLoad returns an object for the newly created AreaLoad.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(List<Autodesk.Revit.DB.XYZ>)DynamoTypeConverter.ConvertInput(args[0],typeof(List<Autodesk.Revit.DB.XYZ>));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var arg3=(Autodesk.Revit.DB.Structure.AreaLoadType)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.Structure.AreaLoadType));
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Document_NewLineLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical lines.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Document_NewLineLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineLoad_1()
		{
			InPortData.Add(new PortData("el", "The host element (Beam, Brace or Column) of the LineLoad application.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Document_NewLineLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineLoad_2()
		{
			InPortData.Add(new PortData("lst", "The end points of the LineLoad application.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(List<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewLineLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns the newly created LineLoad.")]
	public class Document_NewLineLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewLineLoad_3()
		{
			InPortData.Add(new PortData("xyz", "The first point of the LineLoad application.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear force in the first point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear moment in the first point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second point of the LineLoad application.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear force in the second point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The applied 3d linear moment in the second point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPointLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewPointLoad returns an object for the newly created PointLoad.")]
	public class Document_NewPointLoad : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPointLoad()
		{
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, analytical line's end.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(Autodesk.Revit.DB.Structure.PointLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPointLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewPointLoad returns an object for the newly created PointLoad.")]
	public class Document_NewPointLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPointLoad_1()
		{
			InPortData.Add(new PortData("xyz", "The point of the PointLoad application.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(Autodesk.Revit.DB.Structure.PointLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Document_NewPathReinforcement")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewPathReinforcement returns an object for the newly created Rebar.")]
	public class Document_NewPathReinforcement : dynRevitTransactionNodeWithOneOutput
	{
		public Document_NewPathReinforcement()
		{
			InPortData.Add(new PortData("el", "The element to which the Path Reinforcement belongs. The element must be a structural floor or wall.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "An array of curves forming a chain.  Bars will be placed orthogonal to the chain with their hook ends near the chain, offset by the side cover setting.  The curves must belong to the top face of the floor or the exterior face of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "A flag controlling the bars relative to the curves. If the curves are given in order and with consistent orientation, the bars will lie to the right of the chain if flip=false, to the left if flip=true, when viewed from above the floor or outside the wall.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful, NewPathReinforcement returns an object for the newly created Rebar.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Element)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Element));
			var arg1=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.CurveArray));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = dynRevitSettings.Doc.Document.Create.NewPathReinforcement(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryObject_IsElementGeometry")]
	[NodeSearchTags("geometry","object")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("Indicates whether this geometry is obtained directly from an Element.")]
	public class GeometryObject_IsElementGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryObject_IsElementGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(Autodesk.Revit.DB.GeometryObject)));
			OutPortData.Add(new PortData("out","Indicates whether this geometry is obtained directly from an Element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryObject));
			var result = ((Autodesk.Revit.DB.GeometryObject)(args[0] as Value.Container).Item).IsElementGeometry;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryObject_GraphicsStyleId")]
	[NodeSearchTags("geometry","object")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("The ElementId of the GeometryObject's GraphicsStyle")]
	public class GeometryObject_GraphicsStyleId : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryObject_GraphicsStyleId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(Autodesk.Revit.DB.GeometryObject)));
			OutPortData.Add(new PortData("out","The ElementId of the GeometryObject's GraphicsStyle",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryObject));
			var result = ((Autodesk.Revit.DB.GeometryObject)(args[0] as Value.Container).Item).GraphicsStyleId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GeometryObject_Visibility")]
	[NodeSearchTags("geometry","object")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("The visibility.")]
	public class GeometryObject_Visibility : dynRevitTransactionNodeWithOneOutput
	{
		public GeometryObject_Visibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(Autodesk.Revit.DB.GeometryObject)));
			OutPortData.Add(new PortData("out","The visibility.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryObject)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryObject));
			var result = ((Autodesk.Revit.DB.GeometryObject)(args[0] as Value.Container).Item).Visibility;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("HermiteSpline_Parameters")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns the params of the Hermite spline.")]
	public class HermiteSpline_Parameters : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteSpline_Parameters()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the params of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = ((Autodesk.Revit.DB.HermiteSpline)(args[0] as Value.Container).Item).Parameters;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("HermiteSpline_Tangents")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns the tangents of the Hermite spline.")]
	public class HermiteSpline_Tangents : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteSpline_Tangents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the tangents of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = ((Autodesk.Revit.DB.HermiteSpline)(args[0] as Value.Container).Item).Tangents;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("HermiteSpline_ControlPoints")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("The control points of the Hermite spline.")]
	public class HermiteSpline_ControlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteSpline_ControlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","The control points of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = ((Autodesk.Revit.DB.HermiteSpline)(args[0] as Value.Container).Item).ControlPoints;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("HermiteSpline_IsPeriodic")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns whether the Hermite spline is periodic or not.")]
	public class HermiteSpline_IsPeriodic : dynRevitTransactionNodeWithOneOutput
	{
		public HermiteSpline_IsPeriodic()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns whether the Hermite spline is periodic or not.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = ((Autodesk.Revit.DB.HermiteSpline)(args[0] as Value.Container).Item).IsPeriodic;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Profile_Curves")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_PROFILE)]
	[NodeDescription("Retrieve the curves that make up the boundary of the profile.")]
	public class Profile_Curves : dynRevitTransactionNodeWithOneOutput
	{
		public Profile_Curves()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Retrieve the curves that make up the boundary of the profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Profile)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Profile));
			var result = ((Autodesk.Revit.DB.Profile)(args[0] as Value.Container).Item).Curves;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Profile_Filled")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_PROFILE)]
	[NodeDescription("Get or set whether the profile is filled.")]
	public class Profile_Filled : dynRevitTransactionNodeWithOneOutput
	{
		public Profile_Filled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Get or set whether the profile is filled.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Profile)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Profile));
			var result = ((Autodesk.Revit.DB.Profile)(args[0] as Value.Container).Item).Filled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_MaxSegmentAngle")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The maximum segment angle of the sweep in radians.")]
	public class Sweep_MaxSegmentAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_MaxSegmentAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The maximum segment angle of the sweep in radians.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).MaxSegmentAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_IsTrajectorySegmentationEnabled")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The trajectory segmentation option for the sweep.")]
	public class Sweep_IsTrajectorySegmentationEnabled : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_IsTrajectorySegmentationEnabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The trajectory segmentation option for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).IsTrajectorySegmentationEnabled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_Path3d")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The selected curves used for the sweep path.")]
	public class Sweep_Path3d : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_Path3d()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The selected curves used for the sweep path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).Path3d;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_PathSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The sketched path for the sweep.")]
	public class Sweep_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The sketched path for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).PathSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_ProfileSymbol")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The family symbol profile details for the sweep.")]
	public class Sweep_ProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_ProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The family symbol profile details for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).ProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Sweep_ProfileSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The profile sketch of the sweep.")]
	public class Sweep_ProfileSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Sweep_ProfileSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The profile sketch of the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = ((Autodesk.Revit.DB.Sweep)(args[0] as Value.Container).Item).ProfileSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_IsInstanceFlipped")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the instance is flipped.")]
	public class AdaptiveComponentInstanceUtils_IsInstanceFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_IsInstanceFlipped()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","True if the instance is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsInstanceFlipped(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Placement Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The Placement Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Family Instance")]
	public class AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","The Family Instance",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.FamilySymbol));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilyInstance has an Adaptive Component Instances.")]
	public class AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","True if the FamilyInstance has an Adaptive Component Instances.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilyInstance has an Adaptive Family Symbol.")]
	public class AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","True if the FamilyInstance has an Adaptive Family Symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilySymbol is a valid Adaptive Family Symbol.")]
	public class AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol : dynRevitTransactionNodeWithOneOutput
	{
		public AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol()
		{
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","True if the FamilySymbol is a valid Adaptive Family Symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilySymbol));
			var result = Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Project")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Geometric information if projection is successful.")]
	public class Curve_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Project()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Project(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Intersect")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("")]
	public class Curve_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Intersect()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(Autodesk.Revit.DB.IntersectionResultArray)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.IntersectionResultArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.IntersectionResultArray));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Intersect(arg1,out arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Intersect_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("")]
	public class Curve_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Intersect_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Intersect(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_IsInside")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("True if the parameter is within the curve's bounds, otherwise false.")]
	public class Curve_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_IsInside()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(System.Double)));
			InPortData.Add(new PortData("val", "The end index is equal to 0 for the start point, 1 for the end point, or -1 if the parameter is not at the end.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","True if the parameter is within the curve's bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Int32)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Int32));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).IsInside(arg1,out arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_IsInside_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("True if the parameter is within the bounds, otherwise false.")]
	public class Curve_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_IsInside_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","True if the parameter is within the bounds, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).IsInside(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_ComputeDerivatives")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Curve_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_ComputeDerivatives()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Indicates that the specified parameter is normalized.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).ComputeDerivatives(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Distance")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the shortest distance.")]
	public class Curve_Distance : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Distance()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The real number equal to the shortest distance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Distance(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_ComputeRawParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the raw curve parameter.")]
	public class Curve_ComputeRawParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_ComputeRawParameter()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("n", "The normalized parameter.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The real number equal to the raw curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).ComputeRawParameter(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_ComputeNormalizedParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the normalized curve parameter.")]
	public class Curve_ComputeNormalizedParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_ComputeNormalizedParameter()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("n", "The raw parameter.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The real number equal to the normalized curve parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).ComputeNormalizedParameter(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Period")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The period of this curve.")]
	public class Curve_Period : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Period()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The period of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Period;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_IsCyclic")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The boolean value that indicates whether this curve is cyclic.")]
	public class Curve_IsCyclic : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_IsCyclic()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this curve is cyclic.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).IsCyclic;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Length")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The exact length of the curve.")]
	public class Curve_Length : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Length()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The exact length of the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Length;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_ApproximateLength")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The approximate length of the curve.")]
	public class Curve_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_ApproximateLength()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The approximate length of the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).ApproximateLength;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_Reference")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Returns a stable reference to the curve.")]
	public class Curve_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_Reference()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Curve_IsBound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Describes whether the parameter of the curve is restricted to a particular interval.")]
	public class Curve_IsBound : dynRevitTransactionNodeWithOneOutput
	{
		public Curve_IsBound()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Describes whether the parameter of the curve is restricted to a particular interval.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Curve)(args[0] as Value.Container).Item).IsBound;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_TopProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class SweptBlend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).TopProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_BottomProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class SweptBlend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).BottomProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_SelectedPath")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The selected curve used for the swept blend path.")]
	public class SweptBlend_SelectedPath : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_SelectedPath()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The selected curve used for the swept blend path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).SelectedPath;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_PathSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The sketched path for the swept blend.")]
	public class SweptBlend_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The sketched path for the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).PathSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_BottomProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The bottom family symbol profile of the swept blend.")]
	public class SweptBlend_BottomProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_BottomProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom family symbol profile of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).BottomProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_BottomSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The bottom profile sketch of the swept blend.")]
	public class SweptBlend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom profile sketch of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).BottomSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_TopProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The top family symbol profile of the swept blend.")]
	public class SweptBlend_TopProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_TopProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top family symbol profile of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).TopProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("SweptBlend_TopSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The top profile sketch of the swept blend.")]
	public class SweptBlend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public SweptBlend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top profile sketch of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = ((Autodesk.Revit.DB.SweptBlend)(args[0] as Value.Container).Item).TopSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_AddProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Index of newly created profile.")]
	public class Form_AddProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Form_AddProfile()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			InPortData.Add(new PortData("ref", "The geometry reference of edge.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The param on edge to specify the location.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Index of newly created profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).AddProfile(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_GetCurvesAndEdgesReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Reference array containing all edges and curves that the point is lying on.")]
	public class Form_GetCurvesAndEdgesReference : dynRevitTransactionNodeWithOneOutput
	{
		public Form_GetCurvesAndEdgesReference()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			InPortData.Add(new PortData("ref", "The reference of a point.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Reference array containing all edges and curves that the point is lying on.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).GetCurvesAndEdgesReference(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_GetControlPoints")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Reference array containing all control points lying on it.")]
	public class Form_GetControlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Form_GetControlPoints()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			InPortData.Add(new PortData("ref", "The reference of an edge or curve or face.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Reference array containing all control points lying on it.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).GetControlPoints(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_BaseOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Retrieve/set the base offset of the form object. It is only valid for locked form.")]
	public class Form_BaseOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Form_BaseOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the base offset of the form object. It is only valid for locked form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).BaseOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_TopOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Retrieve/set the top offset of the form object. It is only valid for locked form.")]
	public class Form_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Form_TopOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the top offset of the form object. It is only valid for locked form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).TopOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_HasOpenGeometry")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Tell if the form has an open geometry.")]
	public class Form_HasOpenGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Form_HasOpenGeometry()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has an open geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).HasOpenGeometry;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_AreProfilesConstrained")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Get/set if the form's profiles are constrained.")]
	public class Form_AreProfilesConstrained : dynRevitTransactionNodeWithOneOutput
	{
		public Form_AreProfilesConstrained()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form's profiles are constrained.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).AreProfilesConstrained;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_IsInXRayMode")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Get/set if the form is in X-Ray mode.")]
	public class Form_IsInXRayMode : dynRevitTransactionNodeWithOneOutput
	{
		public Form_IsInXRayMode()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form is in X-Ray mode.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).IsInXRayMode;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_HasOneOrMoreReferenceProfiles")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Tell if the form has any reference profile.")]
	public class Form_HasOneOrMoreReferenceProfiles : dynRevitTransactionNodeWithOneOutput
	{
		public Form_HasOneOrMoreReferenceProfiles()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has any reference profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).HasOneOrMoreReferenceProfiles;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_PathCurveCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("The number of curves in the form path.")]
	public class Form_PathCurveCount : dynRevitTransactionNodeWithOneOutput
	{
		public Form_PathCurveCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of curves in the form path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).PathCurveCount;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Form_ProfileCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("The number of profiles in the form.")]
	public class Form_ProfileCount : dynRevitTransactionNodeWithOneOutput
	{
		public Form_ProfileCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of profiles in the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = ((Autodesk.Revit.DB.Form)(args[0] as Value.Container).Item).ProfileCount;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxUV_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXUV)]
	[NodeDescription("Maximum coordinates (upper-right corner of the box).")]
	public class BoundingBoxUV_Max : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxUV_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxUV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxUV));
			var result = ((Autodesk.Revit.DB.BoundingBoxUV)(args[0] as Value.Container).Item).Max;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxUV_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXUV)]
	[NodeDescription("Minimum coordinates (lower-left corner of the box).")]
	public class BoundingBoxUV_Min : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxUV_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxUV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxUV));
			var result = ((Autodesk.Revit.DB.BoundingBoxUV)(args[0] as Value.Container).Item).Min;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_AlmostEqual")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("True if the two transformations are equal; otherwise, false.")]
	public class Transform_AlmostEqual : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_AlmostEqual()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("val", "The transformation to compare with this transformation.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","True if the two transformations are equal; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).AlmostEqual(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_ScaleBasisAndOrigin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Transform_ScaleBasisAndOrigin : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_ScaleBasisAndOrigin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("n", "The scale value.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The transformation equal to the composition of the two transformations.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).ScaleBasisAndOrigin(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_ScaleBasis")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Transform_ScaleBasis : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_ScaleBasis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("n", "The scale value.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The transformation equal to the composition of the two transformations.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).ScaleBasis(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_Multiply")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Transform_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_Multiply()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("val", "The specified transformation.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The transformation equal to the composition of the two transformations.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).Multiply(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_OfVector")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The new vector after transform")]
	public class Transform_OfVector : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_OfVector()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("xyz", "The vector to be transformed",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The new vector after transform",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).OfVector(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_OfPoint")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformed point.")]
	public class Transform_OfPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_OfPoint()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("xyz", "The point to transform.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The transformed point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).OfPoint(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_Inverse")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The inverse transformation of this transformation.")]
	public class Transform_Inverse : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_Inverse()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The inverse transformation of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).Inverse;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_Determinant")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The determinant of this transformation.")]
	public class Transform_Determinant : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_Determinant()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The determinant of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).Determinant;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_IsConformal")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is conformal.")]
	public class Transform_IsConformal : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_IsConformal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is conformal.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).IsConformal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_HasReflection")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation produces reflection.")]
	public class Transform_HasReflection : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_HasReflection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation produces reflection.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).HasReflection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_Scale")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The real number that represents the scale of the transformation.")]
	public class Transform_Scale : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_Scale()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The real number that represents the scale of the transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).Scale;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_IsTranslation")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is a translation.")]
	public class Transform_IsTranslation : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_IsTranslation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is a translation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).IsTranslation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_IsIdentity")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is an identity.")]
	public class Transform_IsIdentity : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_IsIdentity()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is an identity.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).IsIdentity;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_Origin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("Defines the origin of the old coordinate system in the new coordinate system.")]
	public class Transform_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Defines the origin of the old coordinate system in the new coordinate system.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_BasisZ")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the Z axis of this transformation.")]
	public class Transform_BasisZ : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_BasisZ()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Z axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).BasisZ;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_BasisY")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the Y axis of this transformation.")]
	public class Transform_BasisY : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_BasisY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Y axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).BasisY;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Transform_BasisX")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the X axis of this transformation.")]
	public class Transform_BasisX : dynRevitTransactionNodeWithOneOutput
	{
		public Transform_BasisX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the X axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = ((Autodesk.Revit.DB.Transform)(args[0] as Value.Container).Item).BasisX;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_Project")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns")]
	public class Face_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Face_Project()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).Project(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_Intersect")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("")]
	public class Face_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Face_Intersect()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(Autodesk.Revit.DB.IntersectionResultArray)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.IntersectionResultArray)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.IntersectionResultArray));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).Intersect(arg1,out arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_Intersect_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("")]
	public class Face_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Face_Intersect_1()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).Intersect(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_IsInside")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("True if within this face, otherwise False.")]
	public class Face_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Face_IsInside()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("val", "Provides more information when the point is on the edge; otherwise,",typeof(Autodesk.Revit.DB.IntersectionResult)));
			OutPortData.Add(new PortData("out","True if within this face, otherwise False.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.IntersectionResult)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.IntersectionResult));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).IsInside(arg1,out arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_IsInside_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("True if point is within this face, otherwise false.")]
	public class Face_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Face_IsInside_1()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","True if point is within this face, otherwise false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).IsInside(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_ComputeNormal")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The normal vector. This vector will be normalized.")]
	public class Face_ComputeNormal : dynRevitTransactionNodeWithOneOutput
	{
		public Face_ComputeNormal()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The normal vector. This vector will be normalized.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).ComputeNormal(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_ComputeDerivatives")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The transformation containing tangent vectors and a normal vector.")]
	public class Face_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Face_ComputeDerivatives()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The transformation containing tangent vectors and a normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).ComputeDerivatives(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_GetBoundingBox")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("A BoundingBoxUV with the extents of the parameterization of the face.")]
	public class Face_GetBoundingBox : dynRevitTransactionNodeWithOneOutput
	{
		public Face_GetBoundingBox()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","A BoundingBoxUV with the extents of the parameterization of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).GetBoundingBox();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_GetRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.")]
	public class Face_GetRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Face_GetRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).GetRegions();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_Area")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The area of this face.")]
	public class Face_Area : dynRevitTransactionNodeWithOneOutput
	{
		public Face_Area()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","The area of this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).Area;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_Reference")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Returns a stable reference to the face.")]
	public class Face_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Face_Reference()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_IsTwoSided")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Determines if a face is two-sided (degenerate)")]
	public class Face_IsTwoSided : dynRevitTransactionNodeWithOneOutput
	{
		public Face_IsTwoSided()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Determines if a face is two-sided (degenerate)",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).IsTwoSided;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_MaterialElementId")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Element ID of the material from which this face is composed.")]
	public class Face_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Face_MaterialElementId()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this face is composed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).MaterialElementId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_EdgeLoops")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Each edge loop is a closed boundary of the face.")]
	public class Face_EdgeLoops : dynRevitTransactionNodeWithOneOutput
	{
		public Face_EdgeLoops()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Each edge loop is a closed boundary of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).EdgeLoops;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Face_HasRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Reports if the face contains regions created with the Split Face command.")]
	public class Face_HasRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Face_HasRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Reports if the face contains regions created with the Split Face command.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Face)(args[0] as Value.Container).Item).HasRegions;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxXYZ_Enabled")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Defines whether bounding box is turned on.")]
	public class BoundingBoxXYZ_Enabled : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxXYZ_Enabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Defines whether bounding box is turned on.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = ((Autodesk.Revit.DB.BoundingBoxXYZ)(args[0] as Value.Container).Item).Enabled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxXYZ_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Maximum coordinates (upper-right-front corner of the box).")]
	public class BoundingBoxXYZ_Max : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxXYZ_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right-front corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = ((Autodesk.Revit.DB.BoundingBoxXYZ)(args[0] as Value.Container).Item).Max;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxXYZ_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Minimum coordinates (lower-left-rear corner of the box).")]
	public class BoundingBoxXYZ_Min : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxXYZ_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left-rear corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = ((Autodesk.Revit.DB.BoundingBoxXYZ)(args[0] as Value.Container).Item).Min;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("BoundingBoxXYZ_Transform")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("The transform FROM the coordinate space of the box TO the model space.")]
	public class BoundingBoxXYZ_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public BoundingBoxXYZ_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","The transform FROM the coordinate space of the box TO the model space.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = ((Autodesk.Revit.DB.BoundingBoxXYZ)(args[0] as Value.Container).Item).Transform;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_GetCopingIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The coping ElementIds")]
	public class FamilyInstance_GetCopingIds : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_GetCopingIds()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The coping ElementIds",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).GetCopingIds();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_GetCopings")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The coping elements")]
	public class FamilyInstance_GetCopings : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_GetCopings()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The coping elements",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).GetCopings();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_GetSubComponentIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The subcomponent ElementIDs")]
	public class FamilyInstance_GetSubComponentIds : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_GetSubComponentIds()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The subcomponent ElementIDs",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).GetSubComponentIds();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_IsWorkPlaneFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies if the instance's work plane is flipped.")]
	public class FamilyInstance_IsWorkPlaneFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_IsWorkPlaneFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance's work plane is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).IsWorkPlaneFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_CanFlipWorkPlane")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies if the instance can flip its work plane.")]
	public class FamilyInstance_CanFlipWorkPlane : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_CanFlipWorkPlane()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance can flip its work plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).CanFlipWorkPlane;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_IsSlantedColumn")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Indicates if the family instance is a slanted column.")]
	public class FamilyInstance_IsSlantedColumn : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_IsSlantedColumn()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Indicates if the family instance is a slanted column.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).IsSlantedColumn;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_ExtensionUtility")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to check whether the instance can be extended and return the interface for extension operation.")]
	public class FamilyInstance_ExtensionUtility : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_ExtensionUtility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to check whether the instance can be extended and return the interface for extension operation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).ExtensionUtility;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_SuperComponent")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the super component of current family instance.")]
	public class FamilyInstance_SuperComponent : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_SuperComponent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the super component of current family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).SuperComponent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_SubComponents")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the sub components of current family instance.")]
	public class FamilyInstance_SubComponents : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_SubComponents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the sub components of current family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).SubComponents;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_ToRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The \"To Room\" set for the door or window in the last phase of the project.")]
	public class FamilyInstance_ToRoom : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_ToRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"To Room\" set for the door or window in the last phase of the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).ToRoom;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_FromRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The \"From Room\" set for the door or window in the last phase of the project.")]
	public class FamilyInstance_FromRoom : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_FromRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"From Room\" set for the door or window in the last phase of the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).FromRoom;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_CanRotate")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance can be rotated by 180 degrees.")]
	public class FamilyInstance_CanRotate : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_CanRotate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance can be rotated by 180 degrees.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).CanRotate;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_CanFlipFacing")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance facing can be flipped.")]
	public class FamilyInstance_CanFlipFacing : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_CanFlipFacing()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing can be flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).CanFlipFacing;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_CanFlipHand")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance hand can be flipped.")]
	public class FamilyInstance_CanFlipHand : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_CanFlipHand()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand can be flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).CanFlipHand;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Mirrored")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance is mirrored. (only one axis is flipped)")]
	public class FamilyInstance_Mirrored : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Mirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is mirrored. (only one axis is flipped)",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Mirrored;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Invisible")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance is invisible.")]
	public class FamilyInstance_Invisible : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Invisible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is invisible.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Invisible;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_FacingFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance facing is flipped.")]
	public class FamilyInstance_FacingFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_FacingFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).FacingFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_HandFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance hand is flipped.")]
	public class FamilyInstance_HandFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_HandFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).HandFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_FacingOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the orientation of family instance facing.")]
	public class FamilyInstance_FacingOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_FacingOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance facing.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).FacingOrientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_HandOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the orientation of family instance hand.")]
	public class FamilyInstance_HandOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_HandOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance hand.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).HandOrientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_HostFace")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the reference to the host face of family instance.")]
	public class FamilyInstance_HostFace : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_HostFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the reference to the host face of family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).HostFace;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Host")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.")]
	public class FamilyInstance_Host : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Host;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Location")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("This property is used to find the physical location of an instance within project.")]
	public class FamilyInstance_Location : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Location()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property is used to find the physical location of an instance within project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Location;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Space")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The space in which the instance is located (during the last phase of the project).")]
	public class FamilyInstance_Space : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Space()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The space in which the instance is located (during the last phase of the project).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Space;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Room")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The room in which the instance is located (during the last phase of the project).")]
	public class FamilyInstance_Room : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Room()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The room in which the instance is located (during the last phase of the project).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Room;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_StructuralType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Provides the primary structural type of the instance, such as beam or column etc.")]
	public class FamilyInstance_StructuralType : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_StructuralType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural type of the instance, such as beam or column etc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).StructuralType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_StructuralUsage")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Provides the primary structural usage of the instance, such as brace, girder etc.")]
	public class FamilyInstance_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural usage of the instance, such as brace, girder etc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).StructuralUsage;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_StructuralMaterialId")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies the material that defines the instance's structural analysis properties.")]
	public class FamilyInstance_StructuralMaterialId : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_StructuralMaterialId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies the material that defines the instance's structural analysis properties.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).StructuralMaterialId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_StructuralMaterialType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("This property returns the physical material from which the instance is made.")]
	public class FamilyInstance_StructuralMaterialType : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_StructuralMaterialType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property returns the physical material from which the instance is made.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).StructuralMaterialType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_MEPModel")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Retrieves the MEP model for the family instance.")]
	public class FamilyInstance_MEPModel : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_MEPModel()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Retrieves the MEP model for the family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).MEPModel;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("FamilyInstance_Symbol")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Returns or changes the FamilySymbol object that represents the type of the instance.")]
	public class FamilyInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public FamilyInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Returns or changes the FamilySymbol object that represents the type of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = ((Autodesk.Revit.DB.FamilyInstance)(args[0] as Value.Container).Item).Symbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Color_IsValid")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Identifies if the color represents a valid color, or an uninitialized/invalid value.")]
	public class Color_IsValid : dynRevitTransactionNodeWithOneOutput
	{
		public Color_IsValid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Identifies if the color represents a valid color, or an uninitialized/invalid value.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = ((Autodesk.Revit.DB.Color)(args[0] as Value.Container).Item).IsValid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Color_Blue")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the blue channel of the color.")]
	public class Color_Blue : dynRevitTransactionNodeWithOneOutput
	{
		public Color_Blue()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the blue channel of the color.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = ((Autodesk.Revit.DB.Color)(args[0] as Value.Container).Item).Blue;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Color_Green")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the green channel of the color.")]
	public class Color_Green : dynRevitTransactionNodeWithOneOutput
	{
		public Color_Green()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the green channel of the color.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = ((Autodesk.Revit.DB.Color)(args[0] as Value.Container).Item).Green;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Color_Red")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the red channel of the color.")]
	public class Color_Red : dynRevitTransactionNodeWithOneOutput
	{
		public Color_Red()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the red channel of the color.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = ((Autodesk.Revit.DB.Color)(args[0] as Value.Container).Item).Red;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GenericForm_GetVisibility")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("A copy of visibility settings for the generic form.")]
	public class GenericForm_GetVisibility : dynRevitTransactionNodeWithOneOutput
	{
		public GenericForm_GetVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","A copy of visibility settings for the generic form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = ((Autodesk.Revit.DB.GenericForm)(args[0] as Value.Container).Item).GetVisibility();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GenericForm_Subcategory")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("The subcategory.")]
	public class GenericForm_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public GenericForm_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = ((Autodesk.Revit.DB.GenericForm)(args[0] as Value.Container).Item).Subcategory;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GenericForm_Name")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("Get and Set the Name property")]
	public class GenericForm_Name : dynRevitTransactionNodeWithOneOutput
	{
		public GenericForm_Name()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Get and Set the Name property",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = ((Autodesk.Revit.DB.GenericForm)(args[0] as Value.Container).Item).Name;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GenericForm_IsSolid")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("Identifies if the GenericForm is a solid or a void element.")]
	public class GenericForm_IsSolid : dynRevitTransactionNodeWithOneOutput
	{
		public GenericForm_IsSolid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Identifies if the GenericForm is a solid or a void element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = ((Autodesk.Revit.DB.GenericForm)(args[0] as Value.Container).Item).IsSolid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("GenericForm_Visible")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("The visibility of the GenericForm.")]
	public class GenericForm_Visible : dynRevitTransactionNodeWithOneOutput
	{
		public GenericForm_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The visibility of the GenericForm.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = ((Autodesk.Revit.DB.GenericForm)(args[0] as Value.Container).Item).Visible;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Point_Reference")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINT)]
	[NodeDescription("Returns a stable reference to the point.")]
	public class Point_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Point_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Point)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Point));
			var result = ((Autodesk.Revit.DB.Point)(args[0] as Value.Container).Item).Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Point_Coord")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINT)]
	[NodeDescription("Returns the coordinates of the point.")]
	public class Point_Coord : dynRevitTransactionNodeWithOneOutput
	{
		public Point_Coord()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns the coordinates of the point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Point)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Point));
			var result = ((Autodesk.Revit.DB.Point)(args[0] as Value.Container).Item).Coord;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_CanBeIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("True if the element can be an intersection reference., false otherwise.")]
	public class DividedSurface_CanBeIntersectionElement : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_CanBeIntersectionElement()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			InPortData.Add(new PortData("val", "The element to be checked.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","True if the element can be an intersection reference., false otherwise.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).CanBeIntersectionElement(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_GetAllIntersectionElements")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The intersection elements.")]
	public class DividedSurface_GetAllIntersectionElements : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_GetAllIntersectionElements()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The intersection elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).GetAllIntersectionElements();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_NumberOfVGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Get the number of V-gridlines used on thesurface.")]
	public class DividedSurface_NumberOfVGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_NumberOfVGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of V-gridlines used on thesurface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).NumberOfVGridlines;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_NumberOfUGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Get the number of U-gridlines used on thesurface.")]
	public class DividedSurface_NumberOfUGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_NumberOfUGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of U-gridlines used on thesurface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).NumberOfUGridlines;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_IsComponentFlipped")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Whether the pattern is flipped.")]
	public class DividedSurface_IsComponentFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_IsComponentFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).IsComponentFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_IsComponentMirrored")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Whether the pattern is mirror-imaged.")]
	public class DividedSurface_IsComponentMirrored : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_IsComponentMirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is mirror-imaged.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).IsComponentMirrored;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_ComponentRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The rotation of the pattern by a multipleof 90 degrees.")]
	public class DividedSurface_ComponentRotation : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_ComponentRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The rotation of the pattern by a multipleof 90 degrees.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).ComponentRotation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_VPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The offset applied to the pattern by an integral number of grid nodes in the V-direction.")]
	public class DividedSurface_VPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_VPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by an integral number of grid nodes in the V-direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).VPatternIndent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_UPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The offset applied to the pattern by anintegral number of grid nodes in the U-direction.")]
	public class DividedSurface_UPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_UPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by anintegral number of grid nodes in the U-direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).UPatternIndent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_BorderTile")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Determines the handling of tiles that overlap the surface'sboundary.")]
	public class DividedSurface_BorderTile : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_BorderTile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Determines the handling of tiles that overlap the surface'sboundary.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).BorderTile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_AllGridRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Angle of rotation applied to the U- and V- directions together.")]
	public class DividedSurface_AllGridRotation : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_AllGridRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Angle of rotation applied to the U- and V- directions together.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).AllGridRotation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_VSpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Access to the rule for laying out the second series of equidistantparallel lines on the surface.")]
	public class DividedSurface_VSpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_VSpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the second series of equidistantparallel lines on the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).VSpacingRule;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_USpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Access to the rule for laying out the first series of equidistantparallel lines on the surface.")]
	public class DividedSurface_USpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_USpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the first series of equidistantparallel lines on the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).USpacingRule;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_HostReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("A reference to the divided face on the host.")]
	public class DividedSurface_HostReference : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_HostReference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","A reference to the divided face on the host.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).HostReference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("DividedSurface_Host")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The element whose surface has been divided.")]
	public class DividedSurface_Host : dynRevitTransactionNodeWithOneOutput
	{
		public DividedSurface_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The element whose surface has been divided.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = ((Autodesk.Revit.DB.DividedSurface)(args[0] as Value.Container).Item).Host;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("PointCloudInstance_GetPoints")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("A collection object containing points that pass the filter, but no more than the maximum number requested.")]
	public class PointCloudInstance_GetPoints : dynRevitTransactionNodeWithOneOutput
	{
		public PointCloudInstance_GetPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			InPortData.Add(new PortData("val", "The filter to control which points are extracted. The filter should be passed in the coordinates   of the Revit model.",typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter)));
			InPortData.Add(new PortData("i", "The maximum number of points requested.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","A collection object containing points that pass the filter, but no more than the maximum number requested.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointCloudInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PointCloudInstance));
			var arg1=(Autodesk.Revit.DB.PointClouds.PointCloudFilter)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter));
			var arg2=(System.Int32)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Int32));
			var result = ((Autodesk.Revit.DB.PointCloudInstance)(args[0] as Value.Container).Item).GetPoints(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("PointCloudInstance_Create")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("The newly created point cloud instance.")]
	public class PointCloudInstance_Create : dynRevitTransactionNodeWithOneOutput
	{
		public PointCloudInstance_Create()
		{
			InPortData.Add(new PortData("val", "The document in which the new instance is created",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The element id of the PointCloudType.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "The transform that defines the placement of the instance in the Revit document coordinate system.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The newly created point cloud instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.ElementId));
			var arg2=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.Transform));
			var result = Autodesk.Revit.DB.PointCloudInstance.Create(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("PointCloudInstance_GetSelectionFilter")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("Currently active selection filter or")]
	public class PointCloudInstance_GetSelectionFilter : dynRevitTransactionNodeWithOneOutput
	{
		public PointCloudInstance_GetSelectionFilter()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			OutPortData.Add(new PortData("out","Currently active selection filter or",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointCloudInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PointCloudInstance));
			var result = ((Autodesk.Revit.DB.PointCloudInstance)(args[0] as Value.Container).Item).GetSelectionFilter();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("PointCloudInstance_FilterAction")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("The action taken based on the results of the selection filter applied to this point cloud.")]
	public class PointCloudInstance_FilterAction : dynRevitTransactionNodeWithOneOutput
	{
		public PointCloudInstance_FilterAction()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			OutPortData.Add(new PortData("out","The action taken based on the results of the selection filter applied to this point cloud.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointCloudInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PointCloudInstance));
			var result = ((Autodesk.Revit.DB.PointCloudInstance)(args[0] as Value.Container).Item).FilterAction;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_RadiusY")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the Y vector radius of the ellipse.")]
	public class Ellipse_RadiusY : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_RadiusY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the Y vector radius of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).RadiusY;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_RadiusX")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the X vector radius of the ellipse.")]
	public class Ellipse_RadiusX : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_RadiusX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the X vector radius of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).RadiusX;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_YDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("The Y direction.")]
	public class Ellipse_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_YDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The Y direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).YDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_XDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("The X direction.")]
	public class Ellipse_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_XDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The X direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).XDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_Normal")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the normal to the plane in which the ellipse is defined.")]
	public class Ellipse_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_Normal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the ellipse is defined.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).Normal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Ellipse_Center")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the center of the ellipse.")]
	public class Ellipse_Center : dynRevitTransactionNodeWithOneOutput
	{
		public Ellipse_Center()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the center of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = ((Autodesk.Revit.DB.Ellipse)(args[0] as Value.Container).Item).Center;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Extrusion_EndOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("The offset of the end of the extrusion relative to the sketch plane.")]
	public class Extrusion_EndOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Extrusion_EndOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the end of the extrusion relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = ((Autodesk.Revit.DB.Extrusion)(args[0] as Value.Container).Item).EndOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Extrusion_StartOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("The offset of the start of the extrusion relative to the sketch plane.")]
	public class Extrusion_StartOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Extrusion_StartOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the start of the extrusion relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = ((Autodesk.Revit.DB.Extrusion)(args[0] as Value.Container).Item).StartOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Extrusion_Sketch")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("Returns the Sketch of the Extrusion.")]
	public class Extrusion_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public Extrusion_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Extrusion.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = ((Autodesk.Revit.DB.Extrusion)(args[0] as Value.Container).Item).Sketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewReferencePointArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("An empty array that can hold ReferencePoint objects.")]
	public class Application_NewReferencePointArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewReferencePointArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can hold ReferencePoint objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewReferencePointArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPointRelativeToPoint")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("If creation is successful then a new PointRelativeToPoint object is returned,otherwise an exception with failure information will be thrown.")]
	public class Application_NewPointRelativeToPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPointRelativeToPoint()
		{
			InPortData.Add(new PortData("ref", "The reference of the host point.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","If creation is successful then a new PointRelativeToPoint object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Revit.Application.Create.NewPointRelativeToPoint(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPointOnEdgeEdgeIntersection")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new PointOnEdgeEdgeIntersection object.")]
	public class Application_NewPointOnEdgeEdgeIntersection : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPointOnEdgeEdgeIntersection()
		{
			InPortData.Add(new PortData("ref", "The first edge reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second edge reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","A new PointOnEdgeEdgeIntersection object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Reference));
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdgeEdgeIntersection(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPointOnFace")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new PointOnFace object.")]
	public class Application_NewPointOnFace : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPointOnFace()
		{
			InPortData.Add(new PortData("ref", "The reference whose face the object will be created on.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("uv", "A 2-dimensional position.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","A new PointOnFace object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnFace(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPointOnPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new PointOnPlane object with 2-dimensional Position, XVec, and Offsetproperties set to match the given 3-dimensional arguments.")]
	public class Application_NewPointOnPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPointOnPlane()
		{
			InPortData.Add(new PortData("ref", "A reference to some planein the document. (Note: the reference must satisfyIsValidPlaneReference(), but this is not checked until this PointOnPlane objectis assigned to a ReferencePoint.)",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("uv", "Coordinates of the point's projection onto the plane;see the Position property.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The direction of the point'sX-coordinate vector in the plane's coordinates; see the XVec property. Optional;default value is (1, 0).",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "Signed offset from the plane; see the Offset property.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","A new PointOnPlane object with 2-dimensional Position, XVec, and Offsetproperties set to match the given 3-dimensional arguments.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.UV));
			var arg3=(System.Double)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Double));
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnPlane(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPointOnEdge")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("If creation was successful then a new object is returned,otherwise an exception with failure information will be thrown.")]
	public class Application_NewPointOnEdge : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPointOnEdge()
		{
			InPortData.Add(new PortData("ref", "The reference whose edge the object will be created on.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("loc", "The location on the edge.",typeof(Autodesk.Revit.DB.PointLocationOnCurve)));
			OutPortData.Add(new PortData("out","If creation was successful then a new object is returned,otherwise an exception with failure information will be thrown.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Reference)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Reference));
			var arg1=(Autodesk.Revit.DB.PointLocationOnCurve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.PointLocationOnCurve));
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewFamilySymbolProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new FamilySymbolProfile object.")]
	public class Application_NewFamilySymbolProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewFamilySymbolProfile()
		{
			InPortData.Add(new PortData("fs", "The family symbol of the Profile.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","The new FamilySymbolProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilySymbol)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.FamilySymbol));
			var result = dynRevitSettings.Revit.Application.Create.NewFamilySymbolProfile(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewCurveLoopsProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new CurveLoopsProfile object.")]
	public class Application_NewCurveLoopsProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewCurveLoopsProfile()
		{
			InPortData.Add(new PortData("crvs", "The curve loops of the Profile.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			OutPortData.Add(new PortData("out","The new CurveLoopsProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArrArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArrArray));
			var result = dynRevitSettings.Revit.Application.Create.NewCurveLoopsProfile(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewElementId")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new Autodesk::Revit::DB::ElementId^ object.")]
	public class Application_NewElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewElementId()
		{
			OutPortData.Add(new PortData("out","The new Autodesk::Revit::DB::ElementId^ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementId();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewAreaCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The object containing the data needed for area creation.")]
	public class Application_NewAreaCreationData : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewAreaCreationData()
		{
			InPortData.Add(new PortData("v", "The view of area element.",typeof(Autodesk.Revit.DB.ViewPlan)));
			InPortData.Add(new PortData("uv", "A point which lies in an enclosed region of area boundary where the new area will reside.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The object containing the data needed for area creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ViewPlan)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.ViewPlan));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = dynRevitSettings.Revit.Application.Create.NewAreaCreationData(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("If successful a new geometric plane will be returned. Otherwise")]
	public class Application_NewPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPlane()
		{
			InPortData.Add(new PortData("crvs", "The closed loop of planar curves to locate plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","If successful a new geometric plane will be returned. Otherwise",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CurveArray)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CurveArray));
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPlane_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new plane object.")]
	public class Application_NewPlane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPlane_1()
		{
			InPortData.Add(new PortData("xyz", "Z vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewPlane_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new plane object.")]
	public class Application_NewPlane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewPlane_2()
		{
			InPortData.Add(new PortData("xyz", "X vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Y vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","A new plane object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewColor")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new color object.")]
	public class Application_NewColor : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewColor()
		{
			OutPortData.Add(new PortData("out","The new color object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewColor();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewCombinableElementArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("An empty array that can contain any CombinableElement derived objects.")]
	public class Application_NewCombinableElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewCombinableElementArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can contain any CombinableElement derived objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCombinableElementArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewVertexIndexPairArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new VertexIndexPairArray objects.")]
	public class Application_NewVertexIndexPairArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewVertexIndexPairArray()
		{
			OutPortData.Add(new PortData("out","The new VertexIndexPairArray objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPairArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewVertexIndexPair")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The new VertexIndexPair object.")]
	public class Application_NewVertexIndexPair : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewVertexIndexPair()
		{
			InPortData.Add(new PortData("i", "The index of the vertex pair from the top profile of a blend.",typeof(System.Int32)));
			InPortData.Add(new PortData("i", "The index of the vertex pair from the bottom profile of a blend.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","The new VertexIndexPair object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(System.Int32)DynamoTypeConverter.ConvertInput(args[0],typeof(System.Int32));
			var arg1=(System.Int32)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Int32));
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPair(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewElementArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("An empty array that can contain any Autodesk Revit element derived objects.")]
	public class Application_NewElementArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewElementArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can contain any Autodesk Revit element derived objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewCurveArrArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The empty array of curve loops.")]
	public class Application_NewCurveArrArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewCurveArrArray()
		{
			OutPortData.Add(new PortData("out","The empty array of curve loops.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArrArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewCurveArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("An empty array that can hold geometric curves.")]
	public class Application_NewCurveArray : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewCurveArray()
		{
			OutPortData.Add(new PortData("out","An empty array that can hold geometric curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewStringStringMap")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A map that maps one string to another.")]
	public class Application_NewStringStringMap : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewStringStringMap()
		{
			OutPortData.Add(new PortData("out","A map that maps one string to another.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewStringStringMap();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewLineUnbound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new unbounded line object.")]
	public class Application_NewLineUnbound : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewLineUnbound()
		{
			InPortData.Add(new PortData("xyz", "A point through which the line will pass.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector for the direction of the line.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","A new unbounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Revit.Application.Create.NewLineUnbound(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewLineBound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new bounded line object.")]
	public class Application_NewLineBound : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewLineBound()
		{
			InPortData.Add(new PortData("xyz", "A start point for the line.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "An end point for the line.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","A new bounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var result = dynRevitSettings.Revit.Application.Create.NewLineBound(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewLine")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new bounded or unbounded line object.")]
	public class Application_NewLine : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewLine()
		{
			InPortData.Add(new PortData("xyz", "A start point or a point through which the line will pass.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "An end point of a vector for the direction of the line.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Set to True if you wish the line to be bound or False is the line is to be infinite.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","A new bounded or unbounded line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.XYZ));
			var arg1=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.XYZ));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = dynRevitSettings.Revit.Application.Create.NewLine(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewMaterialSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("The newly created MaterialSet instance.")]
	public class Application_NewMaterialSet : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewMaterialSet()
		{
			OutPortData.Add(new PortData("out","The newly created MaterialSet instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewMaterialSet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewElementSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new Element Set.")]
	public class Application_NewElementSet : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewElementSet()
		{
			OutPortData.Add(new PortData("out","A new Element Set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewElementSet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewTypeBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new type binding object.")]
	public class Application_NewTypeBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewTypeBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(Autodesk.Revit.DB.CategorySet)));
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CategorySet));
			var result = dynRevitSettings.Revit.Application.Create.NewTypeBinding(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewTypeBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new type binding object.")]
	public class Application_NewTypeBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewTypeBinding_1()
		{
			OutPortData.Add(new PortData("out","A new type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewTypeBinding();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewInstanceBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new instance binding object.")]
	public class Application_NewInstanceBinding : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewInstanceBinding()
		{
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(Autodesk.Revit.DB.CategorySet)));
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CategorySet)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.CategorySet));
			var result = dynRevitSettings.Revit.Application.Create.NewInstanceBinding(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewInstanceBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new instance binding object.")]
	public class Application_NewInstanceBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewInstanceBinding_1()
		{
			OutPortData.Add(new PortData("out","A new instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewInstanceBinding();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Application_NewCategorySet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new instance of a Category Set.")]
	public class Application_NewCategorySet : dynRevitTransactionNodeWithOneOutput
	{
		public Application_NewCategorySet()
		{
			OutPortData.Add(new PortData("out","A new instance of a Category Set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var result = dynRevitSettings.Revit.Application.Create.NewCategorySet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revolution_Axis")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("Returns the Axis of the Revolution.")]
	public class Revolution_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public Revolution_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Axis of the Revolution.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = ((Autodesk.Revit.DB.Revolution)(args[0] as Value.Container).Item).Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revolution_EndAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("The end angle of the revolution relative to the sketch plane.")]
	public class Revolution_EndAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revolution_EndAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The end angle of the revolution relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = ((Autodesk.Revit.DB.Revolution)(args[0] as Value.Container).Item).EndAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revolution_StartAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("The start angle of the revolution relative to the sketch plane.")]
	public class Revolution_StartAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revolution_StartAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The start angle of the revolution relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = ((Autodesk.Revit.DB.Revolution)(args[0] as Value.Container).Item).StartAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revolution_Sketch")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("Returns the Sketch of the Revolution.")]
	public class Revolution_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revolution_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Revolution.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = ((Autodesk.Revit.DB.Revolution)(args[0] as Value.Container).Item).Sketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("RevolvedFace_Curve")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Profile curve of the surface.")]
	public class RevolvedFace_Curve : dynRevitTransactionNodeWithOneOutput
	{
		public RevolvedFace_Curve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Profile curve of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = ((Autodesk.Revit.DB.RevolvedFace)(args[0] as Value.Container).Item).Curve;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("RevolvedFace_Axis")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Axis of the surface.")]
	public class RevolvedFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public RevolvedFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = ((Autodesk.Revit.DB.RevolvedFace)(args[0] as Value.Container).Item).Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("RevolvedFace_Origin")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Origin of the surface.")]
	public class RevolvedFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public RevolvedFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = ((Autodesk.Revit.DB.RevolvedFace)(args[0] as Value.Container).Item).Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Edge_ComputeDerivatives")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Edge_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Edge_ComputeDerivatives()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.Edge)(args[0] as Value.Container).Item).ComputeDerivatives(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Edge_AsCurveFollowingFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Edge_AsCurveFollowingFace : dynRevitTransactionNodeWithOneOutput
	{
		public Edge_AsCurveFollowingFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			InPortData.Add(new PortData("f", "Specifies the face, on which the curve will follow the topological direction of the edge.",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","It can be an Arc, Line, or HermiteSpline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var arg1=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Face));
			var result = ((Autodesk.Revit.DB.Edge)(args[0] as Value.Container).Item).AsCurveFollowingFace(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Edge_AsCurve")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Edge_AsCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Edge_AsCurve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","It can be an Arc, Line, or HermiteSpline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var result = ((Autodesk.Revit.DB.Edge)(args[0] as Value.Container).Item).AsCurve();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Edge_ApproximateLength")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("Returns the approximate length of the edge.")]
	public class Edge_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public Edge_ApproximateLength()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns the approximate length of the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var result = ((Autodesk.Revit.DB.Edge)(args[0] as Value.Container).Item).ApproximateLength;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Edge_Reference")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("Returns a stable reference to the edge.")]
	public class Edge_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Edge_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var result = ((Autodesk.Revit.DB.Edge)(args[0] as Value.Container).Item).Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_AngleTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number between 0 and 2*PI equal to the angle between the two vectors in radians.")]
	public class UV_AngleTo : dynRevitTransactionNodeWithOneOutput
	{
		public UV_AngleTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The specified vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The real number between 0 and 2*PI equal to the angle between the two vectors in radians.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).AngleTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_DistanceTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the distance between the two points.")]
	public class UV_DistanceTo : dynRevitTransactionNodeWithOneOutput
	{
		public UV_DistanceTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The specified point.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The real number equal to the distance between the two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).DistanceTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_IsAlmostEqualTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class UV_IsAlmostEqualTo : dynRevitTransactionNodeWithOneOutput
	{
		public UV_IsAlmostEqualTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","True if the vectors are the same; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var arg2=(System.Double)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).IsAlmostEqualTo(arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_IsAlmostEqualTo_1")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class UV_IsAlmostEqualTo_1 : dynRevitTransactionNodeWithOneOutput
	{
		public UV_IsAlmostEqualTo_1()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","True if the vectors are the same; otherwise, false.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).IsAlmostEqualTo(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Divide")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The divided 2-D vector.")]
	public class UV_Divide : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Divide()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The divided 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Divide(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Multiply")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The multiplied 2-D vector.")]
	public class UV_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Multiply()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","The multiplied 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(System.Double)DynamoTypeConverter.ConvertInput(args[1],typeof(System.Double));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Multiply(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Negate")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector opposite to this vector.")]
	public class UV_Negate : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Negate()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The 2-D vector opposite to this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Negate();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Subtract")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector equal to the difference between the two vectors.")]
	public class UV_Subtract : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Subtract()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to subtract from this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The 2-D vector equal to the difference between the two vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Subtract(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Add")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector equal to the sum of the two vectors.")]
	public class UV_Add : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Add()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to add to this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The 2-D vector equal to the sum of the two vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Add(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_CrossProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the cross product.")]
	public class UV_CrossProduct : dynRevitTransactionNodeWithOneOutput
	{
		public UV_CrossProduct()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The real number equal to the cross product.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).CrossProduct(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_DotProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the dot product.")]
	public class UV_DotProduct : dynRevitTransactionNodeWithOneOutput
	{
		public UV_DotProduct()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The real number equal to the dot product.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var arg1=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).DotProduct(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_Normalize")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The normalized UV or zero if the vector is almost Zero.")]
	public class UV_Normalize : dynRevitTransactionNodeWithOneOutput
	{
		public UV_Normalize()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The normalized UV or zero if the vector is almost Zero.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).Normalize();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_V")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("Gets the second coordinate.")]
	public class UV_V : dynRevitTransactionNodeWithOneOutput
	{
		public UV_V()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).V;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("UV_U")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("Gets the first coordinate.")]
	public class UV_U : dynRevitTransactionNodeWithOneOutput
	{
		public UV_U()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = ((Autodesk.Revit.DB.UV)(args[0] as Value.Container).Item).U;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Line_Direction")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LINE)]
	[NodeDescription("Returns the direction of the line.")]
	public class Line_Direction : dynRevitTransactionNodeWithOneOutput
	{
		public Line_Direction()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the direction of the line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Line));
			var result = ((Autodesk.Revit.DB.Line)(args[0] as Value.Container).Item).Direction;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Line_Origin")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LINE)]
	[NodeDescription("Returns the origin of the line.")]
	public class Line_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Line_Origin()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the origin of the line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Line));
			var result = ((Autodesk.Revit.DB.Line)(args[0] as Value.Container).Item).Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_GetMaterialAspectPropertySet")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Identifier of the shared property set, or invalidElementId if independent (i.e. owned by the material).")]
	public class Material_GetMaterialAspectPropertySet : dynRevitTransactionNodeWithOneOutput
	{
		public Material_GetMaterialAspectPropertySet()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			InPortData.Add(new PortData("val", "The material aspect.",typeof(Autodesk.Revit.DB.MaterialAspect)));
			OutPortData.Add(new PortData("out","Identifier of the shared property set, or invalidElementId if independent (i.e. owned by the material).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var arg1=(Autodesk.Revit.DB.MaterialAspect)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.MaterialAspect));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).GetMaterialAspectPropertySet(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_GetCutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The color.")]
	public class Material_GetCutPatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Material_GetCutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).GetCutPatternColor();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_GetCutPatternId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern id.")]
	public class Material_GetCutPatternId : dynRevitTransactionNodeWithOneOutput
	{
		public Material_GetCutPatternId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The cut pattern id.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).GetCutPatternId();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_GetSmoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The smoothness value.")]
	public class Material_GetSmoothness : dynRevitTransactionNodeWithOneOutput
	{
		public Material_GetSmoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The smoothness value.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).GetSmoothness();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Create")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Identifier of the new material.")]
	public class Material_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Create()
		{
			InPortData.Add(new PortData("val", "The document in which to create the material.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("s", "The name of the new material.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Identifier of the new material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(System.String)DynamoTypeConverter.ConvertInput(args[1],typeof(System.String));
			var result = Autodesk.Revit.DB.Material.Create(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Duplicate")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The new material.")]
	public class Material_Duplicate : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Duplicate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			InPortData.Add(new PortData("s", "Name of the new material.",typeof(System.String)));
			OutPortData.Add(new PortData("out","The new material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var arg1=(System.String)DynamoTypeConverter.ConvertInput(args[1],typeof(System.String));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Duplicate(arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_MaterialClass")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The name of the general material type, e.g. 'Wood.'")]
	public class Material_MaterialClass : dynRevitTransactionNodeWithOneOutput
	{
		public Material_MaterialClass()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The name of the general material type, e.g. 'Wood.'",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).MaterialClass;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_ThermalAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The ElementId of the thermal PropertySetElement.")]
	public class Material_ThermalAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public Material_ThermalAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the thermal PropertySetElement.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).ThermalAssetId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_StructuralAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The ElementId of the structural PropertySetElement.")]
	public class Material_StructuralAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public Material_StructuralAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the structural PropertySetElement.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).StructuralAssetId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Shininess")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The shininess of the material.")]
	public class Material_Shininess : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Shininess()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The shininess of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Shininess;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Glow")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Whether the material can glow.")]
	public class Material_Glow : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Glow()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Whether the material can glow.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Glow;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_RenderAppearance")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The rendering appearance property of the material.")]
	public class Material_RenderAppearance : dynRevitTransactionNodeWithOneOutput
	{
		public Material_RenderAppearance()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The rendering appearance property of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).RenderAppearance;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_SurfacePatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The surface pattern color of the material.")]
	public class Material_SurfacePatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Material_SurfacePatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The surface pattern color of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).SurfacePatternColor;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_SurfacePattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The surface pattern of the material.")]
	public class Material_SurfacePattern : dynRevitTransactionNodeWithOneOutput
	{
		public Material_SurfacePattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The surface pattern of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).SurfacePattern;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_CutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern color of the material.")]
	public class Material_CutPatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Material_CutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The cut pattern color of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).CutPatternColor;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_CutPattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern of the material.")]
	public class Material_CutPattern : dynRevitTransactionNodeWithOneOutput
	{
		public Material_CutPattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The cut pattern of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).CutPattern;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Smoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The smoothness of the material.")]
	public class Material_Smoothness : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Smoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The smoothness of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Smoothness;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Transparency")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The transparency of the material.")]
	public class Material_Transparency : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Transparency()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The transparency of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Transparency;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Material_Color")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The color of the material.")]
	public class Material_Color : dynRevitTransactionNodeWithOneOutput
	{
		public Material_Color()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = ((Autodesk.Revit.DB.Material)(args[0] as Value.Container).Item).Color;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Solid_ComputeCentroid")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The XYZ point of the Centroid of this solid.")]
	public class Solid_ComputeCentroid : dynRevitTransactionNodeWithOneOutput
	{
		public Solid_ComputeCentroid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The XYZ point of the Centroid of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = ((Autodesk.Revit.DB.Solid)(args[0] as Value.Container).Item).ComputeCentroid();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Solid_Volume")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("Returns the signed volume of this solid.")]
	public class Solid_Volume : dynRevitTransactionNodeWithOneOutput
	{
		public Solid_Volume()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the signed volume of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = ((Autodesk.Revit.DB.Solid)(args[0] as Value.Container).Item).Volume;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Solid_SurfaceArea")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("Returns the total surface area of this solid.")]
	public class Solid_SurfaceArea : dynRevitTransactionNodeWithOneOutput
	{
		public Solid_SurfaceArea()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the total surface area of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = ((Autodesk.Revit.DB.Solid)(args[0] as Value.Container).Item).SurfaceArea;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Solid_Faces")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The faces that belong to the solid.")]
	public class Solid_Faces : dynRevitTransactionNodeWithOneOutput
	{
		public Solid_Faces()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The faces that belong to the solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = ((Autodesk.Revit.DB.Solid)(args[0] as Value.Container).Item).Faces;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Solid_Edges")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The edges that belong to the solid.")]
	public class Solid_Edges : dynRevitTransactionNodeWithOneOutput
	{
		public Solid_Edges()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The edges that belong to the solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = ((Autodesk.Revit.DB.Solid)(args[0] as Value.Container).Item).Edges;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Arc_Radius")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the radius of the arc.")]
	public class Arc_Radius : dynRevitTransactionNodeWithOneOutput
	{
		public Arc_Radius()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the radius of the arc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = ((Autodesk.Revit.DB.Arc)(args[0] as Value.Container).Item).Radius;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Arc_YDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the Y direction.")]
	public class Arc_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Arc_YDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the Y direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = ((Autodesk.Revit.DB.Arc)(args[0] as Value.Container).Item).YDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Arc_XDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the X direction.")]
	public class Arc_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Arc_XDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the X direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = ((Autodesk.Revit.DB.Arc)(args[0] as Value.Container).Item).XDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Arc_Normal")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the normal to the plane in which the arc is defined.")]
	public class Arc_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public Arc_Normal()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the arc is defined.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = ((Autodesk.Revit.DB.Arc)(args[0] as Value.Container).Item).Normal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Arc_Center")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the center of the arc.")]
	public class Arc_Center : dynRevitTransactionNodeWithOneOutput
	{
		public Arc_Center()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the center of the arc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = ((Autodesk.Revit.DB.Arc)(args[0] as Value.Container).Item).Center;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_TopProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class Blend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).TopProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_BottomProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class Blend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).BottomProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_TopOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The offset of the top end of the blend relative to the sketch plane.")]
	public class Blend_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_TopOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the top end of the blend relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).TopOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_BottomOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The offset of the bottom end of the blend relative to the sketch plane.")]
	public class Blend_BottomOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_BottomOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the bottom end of the blend relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).BottomOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_BottomSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("Returns the Bottom Sketch of the Blend.")]
	public class Blend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Bottom Sketch of the Blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).BottomSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Blend_TopSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("Returns the Top Sketch of the Blend.")]
	public class Blend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Blend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Top Sketch of the Blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = ((Autodesk.Revit.DB.Blend)(args[0] as Value.Container).Item).TopSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ModelCurve_GetVisibility")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("A copy of visibility settings for the model curve in a family document.")]
	public class ModelCurve_GetVisibility : dynRevitTransactionNodeWithOneOutput
	{
		public ModelCurve_GetVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","A copy of visibility settings for the model curve in a family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = ((Autodesk.Revit.DB.ModelCurve)(args[0] as Value.Container).Item).GetVisibility();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ModelCurve_IsReferenceLine")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("Indicates if this curve is a reference curve.")]
	public class ModelCurve_IsReferenceLine : dynRevitTransactionNodeWithOneOutput
	{
		public ModelCurve_IsReferenceLine()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","Indicates if this curve is a reference curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = ((Autodesk.Revit.DB.ModelCurve)(args[0] as Value.Container).Item).IsReferenceLine;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ModelCurve_TrussCurveType")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("The truss curve type of this model curve.")]
	public class ModelCurve_TrussCurveType : dynRevitTransactionNodeWithOneOutput
	{
		public ModelCurve_TrussCurveType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The truss curve type of this model curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = ((Autodesk.Revit.DB.ModelCurve)(args[0] as Value.Container).Item).TrussCurveType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("ModelCurve_Subcategory")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("The subcategory.")]
	public class ModelCurve_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public ModelCurve_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = ((Autodesk.Revit.DB.ModelCurve)(args[0] as Value.Container).Item).Subcategory;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Level_PlaneReference")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Returns a reference to this element as a plane.")]
	public class Level_PlaneReference : dynRevitTransactionNodeWithOneOutput
	{
		public Level_PlaneReference()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Returns a reference to this element as a plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = ((Autodesk.Revit.DB.Level)(args[0] as Value.Container).Item).PlaneReference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Level_LevelType")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("The level style of this level.")]
	public class Level_LevelType : dynRevitTransactionNodeWithOneOutput
	{
		public Level_LevelType()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","The level style of this level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = ((Autodesk.Revit.DB.Level)(args[0] as Value.Container).Item).LevelType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Level_ProjectElevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.")]
	public class Level_ProjectElevation : dynRevitTransactionNodeWithOneOutput
	{
		public Level_ProjectElevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = ((Autodesk.Revit.DB.Level)(args[0] as Value.Container).Item).ProjectElevation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Level_Elevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Retrieves or changes the elevation above or below the ground level.")]
	public class Level_Elevation : dynRevitTransactionNodeWithOneOutput
	{
		public Level_Elevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieves or changes the elevation above or below the ground level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = ((Autodesk.Revit.DB.Level)(args[0] as Value.Container).Item).Elevation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_Knots")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Return/set the knots of the nurb spline.")]
	public class NurbSpline_Knots : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_Knots()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the knots of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).Knots;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_Weights")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the weights of the nurb spline.")]
	public class NurbSpline_Weights : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_Weights()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the weights of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).Weights;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_CtrlPoints")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the control points of the nurb spline.")]
	public class NurbSpline_CtrlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_CtrlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the control points of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).CtrlPoints;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_Degree")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the degree of the nurb spline.")]
	public class NurbSpline_Degree : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_Degree()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the degree of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).Degree;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_isRational")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns whether the nurb spline is rational or not.")]
	public class NurbSpline_isRational : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_isRational()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns whether the nurb spline is rational or not.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).isRational;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("NurbSpline_isClosed")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Return/set the nurb spline's isClosed property.")]
	public class NurbSpline_isClosed : dynRevitTransactionNodeWithOneOutput
	{
		public NurbSpline_isClosed()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the nurb spline's isClosed property.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = ((Autodesk.Revit.DB.NurbSpline)(args[0] as Value.Container).Item).isClosed;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Create")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Wall_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Create()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(List<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered to be inside and outside.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(List<Autodesk.Revit.DB.Curve>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.Curve>));
			var arg2=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ElementId));
			var arg3=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.ElementId));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var arg5=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[5],typeof(Autodesk.Revit.DB.XYZ));
			var result = Autodesk.Revit.DB.Wall.Create(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Create_1")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Wall_Create_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Create_1()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(List<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(List<Autodesk.Revit.DB.Curve>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.Curve>));
			var arg2=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ElementId));
			var arg3=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.ElementId));
			var arg4=(System.Boolean)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Boolean));
			var result = Autodesk.Revit.DB.Wall.Create(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Create_2")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Wall_Create_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Create_2()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(List<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(List<Autodesk.Revit.DB.Curve>)DynamoTypeConverter.ConvertInput(args[1],typeof(List<Autodesk.Revit.DB.Curve>));
			var arg2=(System.Boolean)DynamoTypeConverter.ConvertInput(args[2],typeof(System.Boolean));
			var result = Autodesk.Revit.DB.Wall.Create(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Create_3")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Wall_Create_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Create_3()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ElementId));
			var arg3=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[3],typeof(Autodesk.Revit.DB.ElementId));
			var arg4=(System.Double)DynamoTypeConverter.ConvertInput(args[4],typeof(System.Double));
			var arg5=(System.Double)DynamoTypeConverter.ConvertInput(args[5],typeof(System.Double));
			var arg6=(System.Boolean)DynamoTypeConverter.ConvertInput(args[6],typeof(System.Boolean));
			var arg7=(System.Boolean)DynamoTypeConverter.ConvertInput(args[7],typeof(System.Boolean));
			var result = Autodesk.Revit.DB.Wall.Create(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Create_4")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Wall_Create_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Create_4()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","If successful a new wall object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Document)DynamoTypeConverter.ConvertInput(args[0],typeof(Autodesk.Revit.DB.Document));
			var arg1=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[1],typeof(Autodesk.Revit.DB.Curve));
			var arg2=(Autodesk.Revit.DB.ElementId)DynamoTypeConverter.ConvertInput(args[2],typeof(Autodesk.Revit.DB.ElementId));
			var arg3=(System.Boolean)DynamoTypeConverter.ConvertInput(args[3],typeof(System.Boolean));
			var result = Autodesk.Revit.DB.Wall.Create(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Orientation")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("The normal vector projected from the exterior side of the wall.")]
	public class Wall_Orientation : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Orientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","The normal vector projected from the exterior side of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).Orientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Flipped")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Property to test whether the wall orientation is flipped.")]
	public class Wall_Flipped : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Flipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Property to test whether the wall orientation is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).Flipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_StructuralUsage")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Retrieves or changes  the wall's designated structural usage.")]
	public class Wall_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes  the wall's designated structural usage.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).StructuralUsage;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_Width")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Get the overall thickness of the wall.")]
	public class Wall_Width : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_Width()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the overall thickness of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).Width;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_CurtainGrid")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Get the grid object of a curtain wall")]
	public class Wall_CurtainGrid : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_CurtainGrid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the grid object of a curtain wall",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).CurtainGrid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Wall_WallType")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Retrieves or changes the type of the wall.")]
	public class Wall_WallType : dynRevitTransactionNodeWithOneOutput
	{
		public Wall_WallType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes the type of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = ((Autodesk.Revit.DB.Wall)(args[0] as Value.Container).Item).WallType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

}
