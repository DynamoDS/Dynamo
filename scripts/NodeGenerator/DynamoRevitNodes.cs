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
	[NodeName("Revit_HermiteFace_MixedDerivs")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITEFACE)]
	[NodeDescription("Mixed derivatives of the surface.")]
	public class Revit_HermiteFace_MixedDerivs : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteFace_MixedDerivs()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(object)));
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

	[NodeName("Revit_HermiteFace_Points")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITEFACE)]
	[NodeDescription("Interpolation points of the surface.")]
	public class Revit_HermiteFace_Points : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteFace_Points()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(object)));
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

	[NodeName("Revit_Instance_GetTotalTransform")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_INSTANCE)]
	[NodeDescription("The calculated total transform.")]
	public class Revit_Instance_GetTotalTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Instance_GetTotalTransform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance",typeof(object)));
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

	[NodeName("Revit_Instance_GetTransform")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_INSTANCE)]
	[NodeDescription("The inherent transform.")]
	public class Revit_Instance_GetTransform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Instance_GetTransform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance",typeof(object)));
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

	[NodeName("Revit_Mesh_MaterialElementId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("Element ID of the material from which this mesh is composed.")]
	public class Revit_Mesh_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Mesh_MaterialElementId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(object)));
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

	[NodeName("Revit_Mesh_Vertices")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("Retrieves all vertices used to define this mesh. Intended for indexed access.")]
	public class Revit_Mesh_Vertices : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Mesh_Vertices()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(object)));
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

	[NodeName("Revit_Mesh_NumTriangles")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MESH)]
	[NodeDescription("The number of triangles that the mesh contains.")]
	public class Revit_Mesh_NumTriangles : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Mesh_NumTriangles()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_GetInstanceGeometry")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the transformed instance.")]
	public class Revit_GeometryInstance_GetInstanceGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetInstanceGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_GetInstanceGeometry_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the instance.")]
	public class Revit_GeometryInstance_GetInstanceGeometry_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetInstanceGeometry_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_GetSymbolGeometry")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the transformed symbol.")]
	public class Revit_GeometryInstance_GetSymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetSymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_GetSymbolGeometry_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("An element which contains the computed geometry for the symbol.")]
	public class Revit_GeometryInstance_GetSymbolGeometry_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_GetSymbolGeometry_1()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_SymbolGeometry")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The geometric representation of the symbol which generates this instance.")]
	public class Revit_GeometryInstance_SymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_SymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_Symbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The symbol element that this object is referring to.")]
	public class Revit_GeometryInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
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

	[NodeName("Revit_GeometryInstance_Transform")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYINSTANCE)]
	[NodeDescription("The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.")]
	public class Revit_GeometryInstance_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryInstance_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(object)));
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

	[NodeName("Revit_FamilyItemFactory_NewDividedSurface")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDividedSurface(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewCurveByPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSymbolicCurve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSymbolicCurve(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewControl")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewControl(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewModelText")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewModelText(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewOpening(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewElectricalConnector")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewElectricalConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewPipeConnector")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewPipeConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewDuctConnector")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDuctConnector(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewRadialDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewDiameterDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewDiameterDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewRadialDimension_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewRadialDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewRadialDimension_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRadialDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewArcLengthDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewArcLengthDimension_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new arc length dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewArcLengthDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewArcLengthDimension_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewArcLengthDimension(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewAngularDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewAngularDimension_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new angular dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewAngularDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewAngularDimension_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewAngularDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewLinearDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewLinearDimension_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new linear dimension is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewLinearDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewLinearDimension_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLinearDimension(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewFormByThickenSingleSurface")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByThickenSingleSurface(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewFormByCap")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewFormByCap(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewRevolveForms")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolveForms(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSweptBlendForm")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlendForm(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewExtrusionForm")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusionForm(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewLoftForm")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewLoftForm(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSweptBlend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSweptBlend_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Swept blend is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweptBlend_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweptBlend_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweptBlend(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSweep")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewSweep_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
	[NodeDescription("If creation was successful the new Sweep is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_FamilyItemFactory_NewSweep_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyItemFactory_NewSweep_1()
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewSweep(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewRevolution")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewRevolution(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewBlend")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewBlend(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_FamilyItemFactory_NewExtrusion")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_FAMILYITEMFACTORY)]
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
			var result = dynRevitSettings.Doc.Document.FamilyCreate.NewExtrusion(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_ItemFactoryBase_NewAlignment")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_PlaceGroup")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewViewSection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewView3D")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewTextNotes")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewTextNote")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewTextNote_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful, a TextNote object is returned.")]
	public class Revit_ItemFactoryBase_NewTextNote_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewTextNote_1()
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

	[NodeName("Revit_ItemFactoryBase_NewSketchPlane")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewSketchPlane_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise an exception withfailure information will be thrown.")]
	public class Revit_ItemFactoryBase_NewSketchPlane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewSketchPlane_1()
		{
			InPortData.Add(new PortData("val", "The geometry planar face to locate sketch plane.",typeof(object)));
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

	[NodeName("Revit_ItemFactoryBase_NewSketchPlane_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new sketch plane will be returned. Otherwise")]
	public class Revit_ItemFactoryBase_NewSketchPlane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewSketchPlane_2()
		{
			InPortData.Add(new PortData("p", "The geometry plane to locate sketch plane.",typeof(object)));
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

	[NodeName("Revit_ItemFactoryBase_NewReferencePlane2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewReferencePlane")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewViewPlan")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewLevel")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewModelCurve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewGroup")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewGroup_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("A new instance of a group containing the elements specified.")]
	public class Revit_ItemFactoryBase_NewGroup_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewGroup_1()
		{
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(object)));
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstances2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstances")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_1()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_2()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_3()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_4")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("An instance of the new object if creation was successful, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_4()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_5")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_5 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_5()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_6")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_6 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_6()
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

	[NodeName("Revit_ItemFactoryBase_NewFamilyInstance_7")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_ItemFactoryBase_NewFamilyInstance_7 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewFamilyInstance_7()
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

	[NodeName("Revit_ItemFactoryBase_NewDimension")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewDimension_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
	[NodeDescription("If successful a new dimension object, otherwise")]
	public class Revit_ItemFactoryBase_NewDimension_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ItemFactoryBase_NewDimension_1()
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

	[NodeName("Revit_ItemFactoryBase_NewDetailCurveArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewDetailCurve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_ItemFactoryBase_NewAnnotationSymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_ITEMFACTORYBASE)]
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

	[NodeName("Revit_PolyLine_NumberOfCoordinates")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POLYLINE)]
	[NodeDescription("Gets the number of the coordinate points.")]
	public class Revit_PolyLine_NumberOfCoordinates : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PolyLine_NumberOfCoordinates()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine",typeof(object)));
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

	[NodeName("Revit_XYZ_AngleOnPlaneTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number between 0 and 2*PI equal to the projected angle between the two vectors.")]
	public class Revit_XYZ_AngleOnPlaneTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_AngleOnPlaneTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The normal vector that defines the plane.",typeof(object)));
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

	[NodeName("Revit_XYZ_AngleTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number between 0 and PI equal to the angle between the two vectors in radians..")]
	public class Revit_XYZ_AngleTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_AngleTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_DistanceTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the distance between the two points.")]
	public class Revit_XYZ_DistanceTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_DistanceTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(object)));
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

	[NodeName("Revit_XYZ_IsAlmostEqualTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class Revit_XYZ_IsAlmostEqualTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_IsAlmostEqualTo()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(object)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(object)));
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

	[NodeName("Revit_XYZ_IsAlmostEqualTo_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class Revit_XYZ_IsAlmostEqualTo_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_IsAlmostEqualTo_1()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_Divide")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The divided vector.")]
	public class Revit_XYZ_Divide : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Divide()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(object)));
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

	[NodeName("Revit_XYZ_Multiply")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The multiplied vector.")]
	public class Revit_XYZ_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Multiply()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_Negate")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector opposite to this vector.")]
	public class Revit_XYZ_Negate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Negate()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
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

	[NodeName("Revit_XYZ_Subtract")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the difference between the two vectors.")]
	public class Revit_XYZ_Subtract : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Subtract()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to subtract from this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_Add")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the sum of the two vectors.")]
	public class Revit_XYZ_Add : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Add()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to add to this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_TripleProduct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the triple product.")]
	public class Revit_XYZ_TripleProduct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_TripleProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The second vector.",typeof(object)));
			InPortData.Add(new PortData("xyz", "The third vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_CrossProduct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The vector equal to the cross product.")]
	public class Revit_XYZ_CrossProduct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_CrossProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_DotProduct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The real number equal to the dot product.")]
	public class Revit_XYZ_DotProduct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_DotProduct()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_XYZ_Normalize")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("The normalized XYZ or zero if the vector is almost Zero.")]
	public class Revit_XYZ_Normalize : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Normalize()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
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

	[NodeName("Revit_XYZ_Z")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the third coordinate.")]
	public class Revit_XYZ_Z : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Z()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
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

	[NodeName("Revit_XYZ_Y")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the second coordinate.")]
	public class Revit_XYZ_Y : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_Y()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
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

	[NodeName("Revit_XYZ_X")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_XYZ)]
	[NodeDescription("Gets the first coordinate.")]
	public class Revit_XYZ_X : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_XYZ_X()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetPointConstraintType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Constraint type of the Adaptive Shape Handle Point.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetPointConstraintType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetPointConstraintType()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetPointOrientationType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Orientation type of Adaptive Placement Point.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetPointOrientationType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetPointOrientationType()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetPlacementNumber")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Placement number of the Adaptive Placement Point.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetPlacementNumber : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetPlacementNumber()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Shape Handle Point Element References in the Adaptive Component Family.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Placement Point Element References in Adaptive Component Family.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("Number of Adaptive Point Element References in Adaptive Component Family.")]
	public class Revit_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Shape Handle Point.")]
	public class Revit_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Placement Point.")]
	public class Revit_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_IsAdaptivePoint")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Point is an Adaptive Point (Placement Point or Shape Handle Point).")]
	public class Revit_AdaptiveComponentFamilyUtils_IsAdaptivePoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_IsAdaptivePoint()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTFAMILYUTILS)]
	[NodeDescription("True if the Family is an Adaptive Component Family.")]
	public class Revit_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily()
		{
			InPortData.Add(new PortData("val", "The Family",typeof(object)));
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

	[NodeName("Revit_CylindricalFace_Axis")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CYLINDRICALFACE)]
	[NodeDescription("Axis of the surface.")]
	public class Revit_CylindricalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CylindricalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(object)));
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

	[NodeName("Revit_CylindricalFace_Origin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CYLINDRICALFACE)]
	[NodeDescription("Origin of the surface.")]
	public class Revit_CylindricalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_CylindricalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(object)));
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

	[NodeName("Revit_ConicalFace_HalfAngle")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Half angle of the surface.")]
	public class Revit_ConicalFace_HalfAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ConicalFace_HalfAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(object)));
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

	[NodeName("Revit_ConicalFace_Axis")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Axis of the surface.")]
	public class Revit_ConicalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ConicalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(object)));
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

	[NodeName("Revit_ConicalFace_Origin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CONICALFACE)]
	[NodeDescription("Origin of the surface.")]
	public class Revit_ConicalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ConicalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(object)));
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

	[NodeName("Revit_Document_NewTopographySurface")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewTopographySurface(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewTakeoffFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewTakeoffFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewUnionFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewUnionFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewCrossFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewCrossFitting(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewTransitionFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewTransitionFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewTeeFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewTeeFitting(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewElbowFitting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewElbowFitting(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexPipe")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexPipe_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned,  otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexPipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexPipe_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexPipe_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexPipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexPipe_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexPipe(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPipe")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPipe_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewPipe_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipe_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPipe_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new pipe is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewPipe_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPipe_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewPipe(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexDuct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexDuct_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexDuct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexDuct_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFlexDuct_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new flexible duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewFlexDuct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFlexDuct_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFlexDuct(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewDuct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewDuct_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new duct is returned,  otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewDuct_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewDuct_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewDuct_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then a new duct is returned, otherwise an exception with failure information will be thrown.")]
	public class Revit_Document_NewDuct_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewDuct_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewDuct(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFamilyInstance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFamilyInstance_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_Document_NewFamilyInstance_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFamilyInstance_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFamilyInstance_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If creation was successful then an instance to the new object is returned, otherwise")]
	public class Revit_Document_NewFamilyInstance_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFamilyInstance_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFascia")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFascia_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new fascia object within the project, otherwise")]
	public class Revit_Document_NewFascia_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFascia_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFascia(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewGutter")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewGutter_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new gutter object within the project, otherwise")]
	public class Revit_Document_NewGutter_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGutter_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewGutter(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSlabEdge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSlabEdge_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new slab edge object within the project, otherwise")]
	public class Revit_Document_NewSlabEdge_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSlabEdge_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewSlabEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewCurtainSystem")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewCurtainSystem2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewCurtainSystem_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("A set of CurtainSystems will be returned when the operation succeeds.")]
	public class Revit_Document_NewCurtainSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewCurtainSystem_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewCurtainSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWire")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewWire(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewZone")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewZone(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpaceTag")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpaceTag(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpaces2_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewSpaces2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaces2_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces2(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpaces_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewSpaces_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpaces_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpaces(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpace")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpace_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new space element is returned, otherwise")]
	public class Revit_Document_NewSpace_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpace_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpace(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpace_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new space should be returned, otherwise")]
	public class Revit_Document_NewSpace_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewSpace_2()
		{
			InPortData.Add(new PortData("val", "The phase in which the space is to exist.",typeof(object)));
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

	[NodeName("Revit_Document_NewPipingSystem")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewPipingSystem(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewMechanicalSystem")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewMechanicalSystem(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewElectricalSystem")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewElectricalSystem_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Revit_Document_NewElectricalSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElectricalSystem_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewElectricalSystem_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new MEP Electrical System element within the project, otherwise")]
	public class Revit_Document_NewElectricalSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewElectricalSystem_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewElectricalSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreas")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreas(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSlab")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSlab(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewTag")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewTag(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewOpening")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewOpening_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewOpening_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewOpening_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, an Opening object is returned.")]
	public class Revit_Document_NewOpening_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewOpening_3()
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
			var result = dynRevitSettings.Doc.Document.Create.NewOpening(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaBoundaryConditions_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 2 - \"Area\".")]
	public class Revit_Document_NewAreaBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaBoundaryConditions_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineBoundaryConditions_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineBoundaryConditions returns an object for the newly created BoundaryConditionswith the BoundaryType = 1 - \"Line\".")]
	public class Revit_Document_NewLineBoundaryConditions_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineBoundaryConditions_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPointBoundaryConditions")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointBoundaryConditions(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10,arg11,arg12);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewBeamSystem")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewBeamSystem_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewBeamSystem_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewBeamSystem_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new BeamSystem object will be returned, otherwise")]
	public class Revit_Document_NewBeamSystem_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewBeamSystem_3()
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
			var result = dynRevitSettings.Doc.Document.Create.NewBeamSystem(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRoomTag")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewRoomTag(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRooms2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRooms2_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms2_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms2_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewRooms2(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRooms2_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, a set of ElementIds which contains the rooms created should be returned, otherwise")]
	public class Revit_Document_NewRooms2_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms2_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
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

	[NodeName("Revit_Document_NewRooms")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRooms_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewRooms(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRooms_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an Element set which contain the rooms created should be returned, otherwise")]
	public class Revit_Document_NewRooms_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms_2()
		{
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(object)));
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

	[NodeName("Revit_Document_NewRooms_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful an ElementSet contains the rooms should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewRooms_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRooms_3()
		{
			InPortData.Add(new PortData("val", "A list of RoomCreationData which wraps the creation arguments of the rooms to be created.",typeof(object)));
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

	[NodeName("Revit_Document_NewRoom")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewRoom_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new room , otherwise")]
	public class Revit_Document_NewRoom_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoom_1()
		{
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(object)));
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

	[NodeName("Revit_Document_NewRoom_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful the new room will be returned, otherwise")]
	public class Revit_Document_NewRoom_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewRoom_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewRoom(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewGrids")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewGrids(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewGrid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewGrid(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewGrid_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("The newly created grid line.")]
	public class Revit_Document_NewGrid_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewGrid_1()
		{
			InPortData.Add(new PortData("crv", "A line object which represents the location of the grid line.",typeof(object)));
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

	[NodeName("Revit_Document_NewViewSheet")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewViewSheet(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewViewDrafting")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFoundationSlab")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFoundationSlab(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFloor")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFloor_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("if successful, a new floor object within the project, otherwise")]
	public class Revit_Document_NewFloor_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFloor_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewFloor_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new floor object within the project, otherwise")]
	public class Revit_Document_NewFloor_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewFloor_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewFloor(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWalls")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewWalls(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWalls_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If the creation is successful an ElementSet which contains the walls should be returned, otherwise the exception will be thrown.")]
	public class Revit_Document_NewWalls_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWalls_1()
		{
			InPortData.Add(new PortData("val", "A list of RectangularWallCreationData which wraps the creation arguments of the walls to be created.",typeof(object)));
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

	[NodeName("Revit_Document_NewWall")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWall_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWall_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWall_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall_3()
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewWall_4")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful a new wall object within the project, otherwise")]
	public class Revit_Document_NewWall_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewWall_4()
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
			var result = dynRevitSettings.Doc.Document.Create.NewWall(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpotElevation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpotElevation(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewSpotCoordinate")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewSpotCoordinate(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLoadCombination")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCombination(arg0,arg1,arg2,arg3,arg4,arg5,arg6);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLoadCase")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLoadCase(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLoadUsage")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLoadUsage(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLoadNature")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLoadNature(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaLoad")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaLoad_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaLoad_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewAreaLoad_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewAreaLoad returns an object for the newly created AreaLoad.")]
	public class Revit_Document_NewAreaLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewAreaLoad_3()
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
			var result = dynRevitSettings.Doc.Document.Create.NewAreaLoad(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineLoad")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineLoad_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineLoad_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns an object for the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad_2()
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewLineLoad_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewLineLoad returns the newly created LineLoad.")]
	public class Revit_Document_NewLineLoad_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewLineLoad_3()
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
			var result = dynRevitSettings.Doc.Document.Create.NewLineLoad(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9,arg10);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPointLoad")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPointLoad_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
	[NodeDescription("If successful, NewPointLoad returns an object for the newly created PointLoad.")]
	public class Revit_Document_NewPointLoad_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Document_NewPointLoad_1()
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
			var result = dynRevitSettings.Doc.Document.Create.NewPointLoad(arg0,arg1,arg2,arg3,arg4,arg5);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Document_NewPathReinforcement")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_DOCUMENT)]
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
			var result = dynRevitSettings.Doc.Document.Create.NewPathReinforcement(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_GeometryObject_IsElementGeometry")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("Indicates whether this geometry is obtained directly from an Element.")]
	public class Revit_GeometryObject_IsElementGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_IsElementGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(object)));
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

	[NodeName("Revit_GeometryObject_GraphicsStyleId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("The ElementId of the GeometryObject's GraphicsStyle")]
	public class Revit_GeometryObject_GraphicsStyleId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_GraphicsStyleId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(object)));
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

	[NodeName("Revit_GeometryObject_Visibility")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GEOMETRYOBJECT)]
	[NodeDescription("The visibility.")]
	public class Revit_GeometryObject_Visibility : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GeometryObject_Visibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryObject",typeof(object)));
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

	[NodeName("Revit_HermiteSpline_Parameters")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns the params of the Hermite spline.")]
	public class Revit_HermiteSpline_Parameters : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline_Parameters()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(object)));
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

	[NodeName("Revit_HermiteSpline_Tangents")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns the tangents of the Hermite spline.")]
	public class Revit_HermiteSpline_Tangents : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline_Tangents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(object)));
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

	[NodeName("Revit_HermiteSpline_ControlPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("The control points of the Hermite spline.")]
	public class Revit_HermiteSpline_ControlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline_ControlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(object)));
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

	[NodeName("Revit_HermiteSpline_IsPeriodic")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_HERMITESPLINE)]
	[NodeDescription("Returns whether the Hermite spline is periodic or not.")]
	public class Revit_HermiteSpline_IsPeriodic : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_HermiteSpline_IsPeriodic()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(object)));
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

	[NodeName("Revit_Profile_Curves")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_PROFILE)]
	[NodeDescription("Retrieve the curves that make up the boundary of the profile.")]
	public class Revit_Profile_Curves : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Profile_Curves()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(object)));
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

	[NodeName("Revit_Profile_Filled")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_PROFILE)]
	[NodeDescription("Get or set whether the profile is filled.")]
	public class Revit_Profile_Filled : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Profile_Filled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(object)));
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

	[NodeName("Revit_Sweep_MaxSegmentAngle")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The maximum segment angle of the sweep in radians.")]
	public class Revit_Sweep_MaxSegmentAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_MaxSegmentAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_Sweep_IsTrajectorySegmentationEnabled")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The trajectory segmentation option for the sweep.")]
	public class Revit_Sweep_IsTrajectorySegmentationEnabled : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_IsTrajectorySegmentationEnabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_Sweep_Path3d")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The selected curves used for the sweep path.")]
	public class Revit_Sweep_Path3d : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_Path3d()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_Sweep_PathSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The sketched path for the sweep.")]
	public class Revit_Sweep_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_Sweep_ProfileSymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The family symbol profile details for the sweep.")]
	public class Revit_Sweep_ProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_ProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_Sweep_ProfileSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEEP)]
	[NodeDescription("The profile sketch of the sweep.")]
	public class Revit_Sweep_ProfileSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Sweep_ProfileSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_IsInstanceFlipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the instance is flipped.")]
	public class Revit_AdaptiveComponentInstanceUtils_IsInstanceFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_IsInstanceFlipped()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class Revit_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Placement Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class Revit_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class Revit_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("The Family Instance")]
	public class Revit_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance()
		{
			InPortData.Add(new PortData("val", "The Document",typeof(object)));
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilyInstance has an Adaptive Component Instances.")]
	public class Revit_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilyInstance has an Adaptive Family Symbol.")]
	public class Revit_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol()
		{
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(object)));
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

	[NodeName("Revit_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ADAPTIVECOMPONENTINSTANCEUTILS)]
	[NodeDescription("True if the FamilySymbol is a valid Adaptive Family Symbol.")]
	public class Revit_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol()
		{
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(object)));
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

	[NodeName("Revit_Curve_Project")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Geometric information if projection is successful.")]
	public class Revit_Curve_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Project()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(object)));
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

	[NodeName("Revit_Curve_Intersect")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("")]
	public class Revit_Curve_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(object)));
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

	[NodeName("Revit_Curve_Intersect_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("")]
	public class Revit_Curve_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Intersect_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(object)));
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

	[NodeName("Revit_Curve_IsInside")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("True if the parameter is within the curve's bounds, otherwise false.")]
	public class Revit_Curve_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(object)));
			InPortData.Add(new PortData("val", "The end index is equal to 0 for the start point, 1 for the end point, or -1 if the parameter is not at the end.",typeof(object)));
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

	[NodeName("Revit_Curve_IsInside_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("True if the parameter is within the bounds, otherwise false.")]
	public class Revit_Curve_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsInside_1()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(object)));
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

	[NodeName("Revit_Curve_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Revit_Curve_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeDerivatives()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(object)));
			InPortData.Add(new PortData("b", "Indicates that the specified parameter is normalized.",typeof(object)));
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

	[NodeName("Revit_Curve_Distance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the shortest distance.")]
	public class Revit_Curve_Distance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Distance()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(object)));
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

	[NodeName("Revit_Curve_ComputeRawParameter")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the raw curve parameter.")]
	public class Revit_Curve_ComputeRawParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeRawParameter()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("n", "The normalized parameter.",typeof(object)));
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

	[NodeName("Revit_Curve_ComputeNormalizedParameter")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The real number equal to the normalized curve parameter.")]
	public class Revit_Curve_ComputeNormalizedParameter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ComputeNormalizedParameter()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
			InPortData.Add(new PortData("n", "The raw parameter.",typeof(object)));
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

	[NodeName("Revit_Curve_Period")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The period of this curve.")]
	public class Revit_Curve_Period : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Period()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_Curve_IsCyclic")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The boolean value that indicates whether this curve is cyclic.")]
	public class Revit_Curve_IsCyclic : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsCyclic()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_Curve_Length")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The exact length of the curve.")]
	public class Revit_Curve_Length : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Length()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_Curve_ApproximateLength")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("The approximate length of the curve.")]
	public class Revit_Curve_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_ApproximateLength()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_Curve_Reference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Returns a stable reference to the curve.")]
	public class Revit_Curve_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_Reference()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_Curve_IsBound")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_CURVE)]
	[NodeDescription("Describes whether the parameter of the curve is restricted to a particular interval.")]
	public class Revit_Curve_IsBound : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Curve_IsBound()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(object)));
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

	[NodeName("Revit_SweptBlend_TopProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class Revit_SweptBlend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_BottomProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class Revit_SweptBlend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_SelectedPath")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The selected curve used for the swept blend path.")]
	public class Revit_SweptBlend_SelectedPath : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_SelectedPath()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_PathSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The sketched path for the swept blend.")]
	public class Revit_SweptBlend_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_BottomProfileSymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The bottom family symbol profile of the swept blend.")]
	public class Revit_SweptBlend_BottomProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_BottomProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_BottomSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The bottom profile sketch of the swept blend.")]
	public class Revit_SweptBlend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_TopProfileSymbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The top family symbol profile of the swept blend.")]
	public class Revit_SweptBlend_TopProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_TopProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_SweptBlend_TopSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SWEPTBLEND)]
	[NodeDescription("The top profile sketch of the swept blend.")]
	public class Revit_SweptBlend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_SweptBlend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(object)));
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

	[NodeName("Revit_Form_AddProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Index of newly created profile.")]
	public class Revit_Form_AddProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_AddProfile()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
			InPortData.Add(new PortData("ref", "The geometry reference of edge.",typeof(object)));
			InPortData.Add(new PortData("n", "The param on edge to specify the location.",typeof(object)));
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

	[NodeName("Revit_Form_GetCurvesAndEdgesReference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Reference array containing all edges and curves that the point is lying on.")]
	public class Revit_Form_GetCurvesAndEdgesReference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_GetCurvesAndEdgesReference()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference of a point.",typeof(object)));
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

	[NodeName("Revit_Form_GetControlPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Reference array containing all control points lying on it.")]
	public class Revit_Form_GetControlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_GetControlPoints()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
			InPortData.Add(new PortData("ref", "The reference of an edge or curve or face.",typeof(object)));
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

	[NodeName("Revit_Form_BaseOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Retrieve/set the base offset of the form object. It is only valid for locked form.")]
	public class Revit_Form_BaseOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_BaseOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_TopOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Retrieve/set the top offset of the form object. It is only valid for locked form.")]
	public class Revit_Form_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_TopOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_HasOpenGeometry")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Tell if the form has an open geometry.")]
	public class Revit_Form_HasOpenGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_HasOpenGeometry()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_AreProfilesConstrained")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Get/set if the form's profiles are constrained.")]
	public class Revit_Form_AreProfilesConstrained : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_AreProfilesConstrained()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_IsInXRayMode")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Get/set if the form is in X-Ray mode.")]
	public class Revit_Form_IsInXRayMode : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_IsInXRayMode()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_HasOneOrMoreReferenceProfiles")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("Tell if the form has any reference profile.")]
	public class Revit_Form_HasOneOrMoreReferenceProfiles : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_HasOneOrMoreReferenceProfiles()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_PathCurveCount")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("The number of curves in the form path.")]
	public class Revit_Form_PathCurveCount : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_PathCurveCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_Form_ProfileCount")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FORM)]
	[NodeDescription("The number of profiles in the form.")]
	public class Revit_Form_ProfileCount : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Form_ProfileCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(object)));
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

	[NodeName("Revit_BoundingBoxUV_Max")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXUV)]
	[NodeDescription("Maximum coordinates (upper-right corner of the box).")]
	public class Revit_BoundingBoxUV_Max : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxUV_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(object)));
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

	[NodeName("Revit_BoundingBoxUV_Min")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXUV)]
	[NodeDescription("Minimum coordinates (lower-left corner of the box).")]
	public class Revit_BoundingBoxUV_Min : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxUV_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(object)));
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

	[NodeName("Revit_Transform_AlmostEqual")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("True if the two transformations are equal; otherwise, false.")]
	public class Revit_Transform_AlmostEqual : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_AlmostEqual()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("val", "The transformation to compare with this transformation.",typeof(object)));
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

	[NodeName("Revit_Transform_ScaleBasisAndOrigin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Revit_Transform_ScaleBasisAndOrigin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_ScaleBasisAndOrigin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("n", "The scale value.",typeof(object)));
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

	[NodeName("Revit_Transform_ScaleBasis")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Revit_Transform_ScaleBasis : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_ScaleBasis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("n", "The scale value.",typeof(object)));
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

	[NodeName("Revit_Transform_Multiply")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformation equal to the composition of the two transformations.")]
	public class Revit_Transform_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_Multiply()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("val", "The specified transformation.",typeof(object)));
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

	[NodeName("Revit_Transform_OfVector")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The new vector after transform")]
	public class Revit_Transform_OfVector : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_OfVector()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("xyz", "The vector to be transformed",typeof(object)));
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

	[NodeName("Revit_Transform_OfPoint")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The transformed point.")]
	public class Revit_Transform_OfPoint : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_OfPoint()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point to transform.",typeof(object)));
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

	[NodeName("Revit_Transform_Inverse")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The inverse transformation of this transformation.")]
	public class Revit_Transform_Inverse : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_Inverse()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_Determinant")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The determinant of this transformation.")]
	public class Revit_Transform_Determinant : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_Determinant()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_IsConformal")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is conformal.")]
	public class Revit_Transform_IsConformal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_IsConformal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_HasReflection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation produces reflection.")]
	public class Revit_Transform_HasReflection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_HasReflection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_Scale")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The real number that represents the scale of the transformation.")]
	public class Revit_Transform_Scale : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_Scale()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_IsTranslation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is a translation.")]
	public class Revit_Transform_IsTranslation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_IsTranslation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_IsIdentity")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The boolean value that indicates whether this transformation is an identity.")]
	public class Revit_Transform_IsIdentity : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_IsIdentity()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_Origin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("Defines the origin of the old coordinate system in the new coordinate system.")]
	public class Revit_Transform_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_BasisZ")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the Z axis of this transformation.")]
	public class Revit_Transform_BasisZ : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_BasisZ()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_BasisY")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the Y axis of this transformation.")]
	public class Revit_Transform_BasisY : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_BasisY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Transform_BasisX")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_TRANSFORM)]
	[NodeDescription("The basis of the X axis of this transformation.")]
	public class Revit_Transform_BasisX : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Transform_BasisX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(object)));
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

	[NodeName("Revit_Face_Project")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Geometric information if projection is successful;if projection fails or the nearest point is outside of this face, returns")]
	public class Revit_Face_Project : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Project()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(object)));
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

	[NodeName("Revit_Face_Intersect")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("")]
	public class Revit_Face_Intersect : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(object)));
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

	[NodeName("Revit_Face_Intersect_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("")]
	public class Revit_Face_Intersect_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Intersect_1()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(object)));
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

	[NodeName("Revit_Face_IsInside")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("True if within this face, otherwise False.")]
	public class Revit_Face_IsInside : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
			InPortData.Add(new PortData("val", "Provides more information when the point is on the edge; otherwise,",typeof(object)));
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

	[NodeName("Revit_Face_IsInside_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("True if point is within this face, otherwise false.")]
	public class Revit_Face_IsInside_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsInside_1()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
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

	[NodeName("Revit_Face_ComputeNormal")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The normal vector. This vector will be normalized.")]
	public class Revit_Face_ComputeNormal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeNormal()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
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

	[NodeName("Revit_Face_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The transformation containing tangent vectors and a normal vector.")]
	public class Revit_Face_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_ComputeDerivatives()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(object)));
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

	[NodeName("Revit_Face_GetBoundingBox")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("A BoundingBoxUV with the extents of the parameterization of the face.")]
	public class Revit_Face_GetBoundingBox : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetBoundingBox()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_GetRegions")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("A list of faces, one for the main face of the object hosting the Split Face (such as wall of floor) and one face for each Split Face regions.")]
	public class Revit_Face_GetRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_GetRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_Area")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("The area of this face.")]
	public class Revit_Face_Area : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Area()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_Reference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Returns a stable reference to the face.")]
	public class Revit_Face_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_Reference()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_IsTwoSided")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Determines if a face is two-sided (degenerate)")]
	public class Revit_Face_IsTwoSided : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_IsTwoSided()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_MaterialElementId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Element ID of the material from which this face is composed.")]
	public class Revit_Face_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_MaterialElementId()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_EdgeLoops")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Each edge loop is a closed boundary of the face.")]
	public class Revit_Face_EdgeLoops : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_EdgeLoops()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_Face_HasRegions")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FACE)]
	[NodeDescription("Reports if the face contains regions created with the Split Face command.")]
	public class Revit_Face_HasRegions : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Face_HasRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(object)));
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

	[NodeName("Revit_BoundingBoxXYZ_Enabled")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Defines whether bounding box is turned on.")]
	public class Revit_BoundingBoxXYZ_Enabled : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxXYZ_Enabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(object)));
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

	[NodeName("Revit_BoundingBoxXYZ_Max")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Maximum coordinates (upper-right-front corner of the box).")]
	public class Revit_BoundingBoxXYZ_Max : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxXYZ_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(object)));
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

	[NodeName("Revit_BoundingBoxXYZ_Min")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("Minimum coordinates (lower-left-rear corner of the box).")]
	public class Revit_BoundingBoxXYZ_Min : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxXYZ_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(object)));
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

	[NodeName("Revit_BoundingBoxXYZ_Transform")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BOUNDINGBOXXYZ)]
	[NodeDescription("The transform FROM the coordinate space of the box TO the model space.")]
	public class Revit_BoundingBoxXYZ_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_BoundingBoxXYZ_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_GetCopingIds")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The coping ElementIds")]
	public class Revit_FamilyInstance_GetCopingIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetCopingIds()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_GetCopings")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The coping elements")]
	public class Revit_FamilyInstance_GetCopings : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetCopings()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_GetSubComponentIds")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The subcomponent ElementIDs")]
	public class Revit_FamilyInstance_GetSubComponentIds : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_GetSubComponentIds()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_IsWorkPlaneFlipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies if the instance's work plane is flipped.")]
	public class Revit_FamilyInstance_IsWorkPlaneFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_IsWorkPlaneFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_CanFlipWorkPlane")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies if the instance can flip its work plane.")]
	public class Revit_FamilyInstance_CanFlipWorkPlane : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_CanFlipWorkPlane()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_IsSlantedColumn")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Indicates if the family instance is a slanted column.")]
	public class Revit_FamilyInstance_IsSlantedColumn : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_IsSlantedColumn()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_ExtensionUtility")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to check whether the instance can be extended and return the interface for extension operation.")]
	public class Revit_FamilyInstance_ExtensionUtility : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_ExtensionUtility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_SuperComponent")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the super component of current family instance.")]
	public class Revit_FamilyInstance_SuperComponent : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_SuperComponent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_SubComponents")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the sub components of current family instance.")]
	public class Revit_FamilyInstance_SubComponents : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_SubComponents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_ToRoom")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The \"To Room\" set for the door or window in the last phase of the project.")]
	public class Revit_FamilyInstance_ToRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_ToRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_FromRoom")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The \"From Room\" set for the door or window in the last phase of the project.")]
	public class Revit_FamilyInstance_FromRoom : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_FromRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_CanRotate")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance can be rotated by 180 degrees.")]
	public class Revit_FamilyInstance_CanRotate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_CanRotate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_CanFlipFacing")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance facing can be flipped.")]
	public class Revit_FamilyInstance_CanFlipFacing : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_CanFlipFacing()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_CanFlipHand")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance hand can be flipped.")]
	public class Revit_FamilyInstance_CanFlipHand : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_CanFlipHand()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Mirrored")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance is mirrored. (only one axis is flipped)")]
	public class Revit_FamilyInstance_Mirrored : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Mirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Invisible")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the family instance is invisible.")]
	public class Revit_FamilyInstance_Invisible : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Invisible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_FacingFlipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance facing is flipped.")]
	public class Revit_FamilyInstance_FacingFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_FacingFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_HandFlipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to test whether the orientation of family instance hand is flipped.")]
	public class Revit_FamilyInstance_HandFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_HandFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_FacingOrientation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the orientation of family instance facing.")]
	public class Revit_FamilyInstance_FacingOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_FacingOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_HandOrientation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the orientation of family instance hand.")]
	public class Revit_FamilyInstance_HandOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_HandOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_HostFace")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Property to get the reference to the host face of family instance.")]
	public class Revit_FamilyInstance_HostFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_HostFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Host")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.")]
	public class Revit_FamilyInstance_Host : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Location")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("This property is used to find the physical location of an instance within project.")]
	public class Revit_FamilyInstance_Location : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Location()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Space")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The space in which the instance is located (during the last phase of the project).")]
	public class Revit_FamilyInstance_Space : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Space()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Room")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("The room in which the instance is located (during the last phase of the project).")]
	public class Revit_FamilyInstance_Room : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Room()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_StructuralType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Provides the primary structural type of the instance, such as beam or column etc.")]
	public class Revit_FamilyInstance_StructuralType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_StructuralType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_StructuralUsage")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Provides the primary structural usage of the instance, such as brace, girder etc.")]
	public class Revit_FamilyInstance_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_StructuralMaterialId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Identifies the material that defines the instance's structural analysis properties.")]
	public class Revit_FamilyInstance_StructuralMaterialId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_StructuralMaterialId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_StructuralMaterialType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("This property returns the physical material from which the instance is made.")]
	public class Revit_FamilyInstance_StructuralMaterialType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_StructuralMaterialType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_MEPModel")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Retrieves the MEP model for the family instance.")]
	public class Revit_FamilyInstance_MEPModel : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_MEPModel()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_FamilyInstance_Symbol")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_FAMILYINSTANCE)]
	[NodeDescription("Returns or changes the FamilySymbol object that represents the type of the instance.")]
	public class Revit_FamilyInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_FamilyInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(object)));
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

	[NodeName("Revit_Color_IsValid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Identifies if the color represents a valid color, or an uninitialized/invalid value.")]
	public class Revit_Color_IsValid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Color_IsValid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(object)));
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

	[NodeName("Revit_Color_Blue")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the blue channel of the color.")]
	public class Revit_Color_Blue : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Color_Blue()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(object)));
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

	[NodeName("Revit_Color_Green")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the green channel of the color.")]
	public class Revit_Color_Green : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Color_Green()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(object)));
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

	[NodeName("Revit_Color_Red")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_COLOR)]
	[NodeDescription("Get or set the red channel of the color.")]
	public class Revit_Color_Red : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Color_Red()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(object)));
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

	[NodeName("Revit_GenericForm_GetVisibility")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("A copy of visibility settings for the generic form.")]
	public class Revit_GenericForm_GetVisibility : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GenericForm_GetVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(object)));
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

	[NodeName("Revit_GenericForm_Subcategory")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("The subcategory.")]
	public class Revit_GenericForm_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GenericForm_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(object)));
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

	[NodeName("Revit_GenericForm_Name")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("Get and Set the Name property")]
	public class Revit_GenericForm_Name : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GenericForm_Name()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(object)));
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

	[NodeName("Revit_GenericForm_IsSolid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("Identifies if the GenericForm is a solid or a void element.")]
	public class Revit_GenericForm_IsSolid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GenericForm_IsSolid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(object)));
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

	[NodeName("Revit_GenericForm_Visible")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_GENERICFORM)]
	[NodeDescription("The visibility of the GenericForm.")]
	public class Revit_GenericForm_Visible : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_GenericForm_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(object)));
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

	[NodeName("Revit_Point_Reference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINT)]
	[NodeDescription("Returns a stable reference to the point.")]
	public class Revit_Point_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Point_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(object)));
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

	[NodeName("Revit_Point_Coord")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINT)]
	[NodeDescription("Returns the coordinates of the point.")]
	public class Revit_Point_Coord : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Point_Coord()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(object)));
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

	[NodeName("Revit_DividedSurface_CanBeIntersectionElement")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("True if the element can be an intersection reference., false otherwise.")]
	public class Revit_DividedSurface_CanBeIntersectionElement : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_CanBeIntersectionElement()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
			InPortData.Add(new PortData("val", "The element to be checked.",typeof(object)));
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

	[NodeName("Revit_DividedSurface_GetAllIntersectionElements")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The intersection elements.")]
	public class Revit_DividedSurface_GetAllIntersectionElements : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_GetAllIntersectionElements()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_NumberOfVGridlines")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Get the number of V-gridlines used on thesurface.")]
	public class Revit_DividedSurface_NumberOfVGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_NumberOfVGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_NumberOfUGridlines")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Get the number of U-gridlines used on thesurface.")]
	public class Revit_DividedSurface_NumberOfUGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_NumberOfUGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_IsComponentFlipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Whether the pattern is flipped.")]
	public class Revit_DividedSurface_IsComponentFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_IsComponentFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_IsComponentMirrored")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Whether the pattern is mirror-imaged.")]
	public class Revit_DividedSurface_IsComponentMirrored : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_IsComponentMirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_ComponentRotation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The rotation of the pattern by a multipleof 90 degrees.")]
	public class Revit_DividedSurface_ComponentRotation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_ComponentRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_VPatternIndent")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The offset applied to the pattern by an integral number of grid nodes in the V-direction.")]
	public class Revit_DividedSurface_VPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_VPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_UPatternIndent")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The offset applied to the pattern by anintegral number of grid nodes in the U-direction.")]
	public class Revit_DividedSurface_UPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_UPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_BorderTile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Determines the handling of tiles that overlap the surface'sboundary.")]
	public class Revit_DividedSurface_BorderTile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_BorderTile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_AllGridRotation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Angle of rotation applied to the U- and V- directions together.")]
	public class Revit_DividedSurface_AllGridRotation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_AllGridRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_VSpacingRule")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Access to the rule for laying out the second series of equidistantparallel lines on the surface.")]
	public class Revit_DividedSurface_VSpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_VSpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_USpacingRule")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("Access to the rule for laying out the first series of equidistantparallel lines on the surface.")]
	public class Revit_DividedSurface_USpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_USpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_HostReference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("A reference to the divided face on the host.")]
	public class Revit_DividedSurface_HostReference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_HostReference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_DividedSurface_Host")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_DIVIDEDSURFACE)]
	[NodeDescription("The element whose surface has been divided.")]
	public class Revit_DividedSurface_Host : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_DividedSurface_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(object)));
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

	[NodeName("Revit_PointCloudInstance_GetPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("A collection object containing points that pass the filter, but no more than the maximum number requested.")]
	public class Revit_PointCloudInstance_GetPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_GetPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(object)));
			InPortData.Add(new PortData("val", "The filter to control which points are extracted. The filter should be passed in the coordinates   of the Revit model.",typeof(object)));
			InPortData.Add(new PortData("i", "The maximum number of points requested.",typeof(object)));
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

	[NodeName("Revit_PointCloudInstance_Create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
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
			var result = Autodesk.Revit.DB.PointCloudInstance.Create(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_PointCloudInstance_GetSelectionFilter")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("Currently active selection filter or")]
	public class Revit_PointCloudInstance_GetSelectionFilter : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_GetSelectionFilter()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(object)));
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

	[NodeName("Revit_PointCloudInstance_FilterAction")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_POINTCLOUDINSTANCE)]
	[NodeDescription("The action taken based on the results of the selection filter applied to this point cloud.")]
	public class Revit_PointCloudInstance_FilterAction : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_PointCloudInstance_FilterAction()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(object)));
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

	[NodeName("Revit_Ellipse_RadiusY")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the Y vector radius of the ellipse.")]
	public class Revit_Ellipse_RadiusY : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_RadiusY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Ellipse_RadiusX")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the X vector radius of the ellipse.")]
	public class Revit_Ellipse_RadiusX : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_RadiusX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Ellipse_YDirection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("The Y direction.")]
	public class Revit_Ellipse_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_YDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Ellipse_XDirection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("The X direction.")]
	public class Revit_Ellipse_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_XDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Ellipse_Normal")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the normal to the plane in which the ellipse is defined.")]
	public class Revit_Ellipse_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_Normal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Ellipse_Center")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ELLIPSE)]
	[NodeDescription("Returns the center of the ellipse.")]
	public class Revit_Ellipse_Center : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Ellipse_Center()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(object)));
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

	[NodeName("Revit_Extrusion_EndOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("The offset of the end of the extrusion relative to the sketch plane.")]
	public class Revit_Extrusion_EndOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Extrusion_EndOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(object)));
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

	[NodeName("Revit_Extrusion_StartOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("The offset of the start of the extrusion relative to the sketch plane.")]
	public class Revit_Extrusion_StartOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Extrusion_StartOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(object)));
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

	[NodeName("Revit_Extrusion_Sketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EXTRUSION)]
	[NodeDescription("Returns the Sketch of the Extrusion.")]
	public class Revit_Extrusion_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Extrusion_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(object)));
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

	[NodeName("Revit_Application_NewReferencePointArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewReferencePointArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPointRelativeToPoint")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPointRelativeToPoint(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPointOnEdgeEdgeIntersection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdgeEdgeIntersection(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPointOnFace")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnFace(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPointOnPlane")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnPlane(arg0,arg1,arg2,arg3);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPointOnEdge")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPointOnEdge(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewFamilySymbolProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewFamilySymbolProfile(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewCurveLoopsProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewCurveLoopsProfile(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewElementId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewElementId();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewAreaCreationData")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewAreaCreationData(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPlane")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPlane_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new plane object.")]
	public class Revit_Application_NewPlane_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPlane_1()
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
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewPlane_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new plane object.")]
	public class Revit_Application_NewPlane_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewPlane_2()
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
			var result = dynRevitSettings.Revit.Application.Create.NewPlane(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewColor")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewColor();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewCombinableElementArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewCombinableElementArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewVertexIndexPairArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPairArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewVertexIndexPair")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewVertexIndexPair(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewElementArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewElementArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewCurveArrArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArrArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewCurveArray")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewCurveArray();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewStringStringMap")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewStringStringMap();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewLineUnbound")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewLineUnbound(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewLineBound")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewLineBound(arg0,arg1);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewLine")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewLine(arg0,arg1,arg2);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewMaterialSet")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewMaterialSet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewElementSet")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewElementSet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewTypeBinding")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewTypeBinding(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewTypeBinding_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new type binding object.")]
	public class Revit_Application_NewTypeBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewTypeBinding_1()
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

	[NodeName("Revit_Application_NewInstanceBinding")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewInstanceBinding(arg0);
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Application_NewInstanceBinding_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
	[NodeDescription("A new instance binding object.")]
	public class Revit_Application_NewInstanceBinding_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Application_NewInstanceBinding_1()
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

	[NodeName("Revit_Application_NewCategorySet")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_CREATION_APPLICATION)]
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
			var result = dynRevitSettings.Revit.Application.Create.NewCategorySet();
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("Revit_Revolution_Axis")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("Returns the Axis of the Revolution.")]
	public class Revit_Revolution_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Revolution_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(object)));
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

	[NodeName("Revit_Revolution_EndAngle")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("The end angle of the revolution relative to the sketch plane.")]
	public class Revit_Revolution_EndAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Revolution_EndAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(object)));
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

	[NodeName("Revit_Revolution_StartAngle")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("The start angle of the revolution relative to the sketch plane.")]
	public class Revit_Revolution_StartAngle : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Revolution_StartAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(object)));
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

	[NodeName("Revit_Revolution_Sketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLUTION)]
	[NodeDescription("Returns the Sketch of the Revolution.")]
	public class Revit_Revolution_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Revolution_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(object)));
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

	[NodeName("Revit_RevolvedFace_Curve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Profile curve of the surface.")]
	public class Revit_RevolvedFace_Curve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RevolvedFace_Curve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(object)));
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

	[NodeName("Revit_RevolvedFace_Axis")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Axis of the surface.")]
	public class Revit_RevolvedFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RevolvedFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(object)));
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

	[NodeName("Revit_RevolvedFace_Origin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_REVOLVEDFACE)]
	[NodeDescription("Origin of the surface.")]
	public class Revit_RevolvedFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_RevolvedFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(object)));
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

	[NodeName("Revit_Edge_ComputeDerivatives")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("The transformation containing a tangent vector, derivative of tangent vector, and bi-normal vector.")]
	public class Revit_Edge_ComputeDerivatives : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_ComputeDerivatives()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(object)));
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(object)));
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

	[NodeName("Revit_Edge_AsCurveFollowingFace")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Revit_Edge_AsCurveFollowingFace : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_AsCurveFollowingFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(object)));
			InPortData.Add(new PortData("f", "Specifies the face, on which the curve will follow the topological direction of the edge.",typeof(object)));
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

	[NodeName("Revit_Edge_AsCurve")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("It can be an Arc, Line, or HermiteSpline.")]
	public class Revit_Edge_AsCurve : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_AsCurve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(object)));
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

	[NodeName("Revit_Edge_ApproximateLength")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("Returns the approximate length of the edge.")]
	public class Revit_Edge_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_ApproximateLength()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(object)));
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

	[NodeName("Revit_Edge_Reference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_EDGE)]
	[NodeDescription("Returns a stable reference to the edge.")]
	public class Revit_Edge_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Edge_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(object)));
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

	[NodeName("Revit_UV_AngleTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number between 0 and 2*PI equal to the angle between the two vectors in radians.")]
	public class Revit_UV_AngleTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_AngleTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The specified vector.",typeof(object)));
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

	[NodeName("Revit_UV_DistanceTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the distance between the two points.")]
	public class Revit_UV_DistanceTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_DistanceTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The specified point.",typeof(object)));
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

	[NodeName("Revit_UV_IsAlmostEqualTo")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class Revit_UV_IsAlmostEqualTo : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_IsAlmostEqualTo()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(object)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(object)));
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

	[NodeName("Revit_UV_IsAlmostEqualTo_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("True if the vectors are the same; otherwise, false.")]
	public class Revit_UV_IsAlmostEqualTo_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_IsAlmostEqualTo_1()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(object)));
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

	[NodeName("Revit_UV_Divide")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The divided 2-D vector.")]
	public class Revit_UV_Divide : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Divide()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(object)));
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

	[NodeName("Revit_UV_Multiply")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The multiplied 2-D vector.")]
	public class Revit_UV_Multiply : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Multiply()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_UV_Negate")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector opposite to this vector.")]
	public class Revit_UV_Negate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Negate()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
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

	[NodeName("Revit_UV_Subtract")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector equal to the difference between the two vectors.")]
	public class Revit_UV_Subtract : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Subtract()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to subtract from this vector.",typeof(object)));
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

	[NodeName("Revit_UV_Add")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The 2-D vector equal to the sum of the two vectors.")]
	public class Revit_UV_Add : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Add()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to add to this vector.",typeof(object)));
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

	[NodeName("Revit_UV_CrossProduct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the cross product.")]
	public class Revit_UV_CrossProduct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_CrossProduct()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_UV_DotProduct")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The real number equal to the dot product.")]
	public class Revit_UV_DotProduct : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_DotProduct()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(object)));
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

	[NodeName("Revit_UV_Normalize")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("The normalized UV or zero if the vector is almost Zero.")]
	public class Revit_UV_Normalize : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_Normalize()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
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

	[NodeName("Revit_UV_V")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("Gets the second coordinate.")]
	public class Revit_UV_V : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_V()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
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

	[NodeName("Revit_UV_U")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_UV)]
	[NodeDescription("Gets the first coordinate.")]
	public class Revit_UV_U : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_UV_U()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(object)));
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

	[NodeName("Revit_Line_Direction")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LINE)]
	[NodeDescription("Returns the direction of the line.")]
	public class Revit_Line_Direction : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Line_Direction()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
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

	[NodeName("Revit_Line_Origin")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LINE)]
	[NodeDescription("Returns the origin of the line.")]
	public class Revit_Line_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Line_Origin()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(object)));
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

	[NodeName("Revit_Material_GetMaterialAspectPropertySet")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Identifier of the shared property set, or invalidElementId if independent (i.e. owned by the material).")]
	public class Revit_Material_GetMaterialAspectPropertySet : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_GetMaterialAspectPropertySet()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
			InPortData.Add(new PortData("val", "The material aspect.",typeof(object)));
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

	[NodeName("Revit_Material_GetCutPatternColor")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The color.")]
	public class Revit_Material_GetCutPatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_GetCutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_GetCutPatternId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern id.")]
	public class Revit_Material_GetCutPatternId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_GetCutPatternId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_GetSmoothness")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The smoothness value.")]
	public class Revit_Material_GetSmoothness : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_GetSmoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Identifier of the new material.")]
	public class Revit_Material_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Create()
		{
			InPortData.Add(new PortData("val", "The document in which to create the material.",typeof(object)));
			InPortData.Add(new PortData("s", "The name of the new material.",typeof(object)));
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

	[NodeName("Revit_Material_Duplicate")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The new material.")]
	public class Revit_Material_Duplicate : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Duplicate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
			InPortData.Add(new PortData("s", "Name of the new material.",typeof(object)));
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

	[NodeName("Revit_Material_MaterialClass")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The name of the general material type, e.g. 'Wood.'")]
	public class Revit_Material_MaterialClass : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_MaterialClass()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_ThermalAssetId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The ElementId of the thermal PropertySetElement.")]
	public class Revit_Material_ThermalAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_ThermalAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_StructuralAssetId")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The ElementId of the structural PropertySetElement.")]
	public class Revit_Material_StructuralAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_StructuralAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Shininess")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The shininess of the material.")]
	public class Revit_Material_Shininess : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Shininess()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Glow")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("Whether the material can glow.")]
	public class Revit_Material_Glow : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Glow()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_RenderAppearance")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The rendering appearance property of the material.")]
	public class Revit_Material_RenderAppearance : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_RenderAppearance()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_SurfacePatternColor")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The surface pattern color of the material.")]
	public class Revit_Material_SurfacePatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_SurfacePatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_SurfacePattern")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The surface pattern of the material.")]
	public class Revit_Material_SurfacePattern : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_SurfacePattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_CutPatternColor")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern color of the material.")]
	public class Revit_Material_CutPatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_CutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_CutPattern")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The cut pattern of the material.")]
	public class Revit_Material_CutPattern : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_CutPattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Smoothness")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The smoothness of the material.")]
	public class Revit_Material_Smoothness : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Smoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Transparency")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The transparency of the material.")]
	public class Revit_Material_Transparency : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Transparency()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Material_Color")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MATERIAL)]
	[NodeDescription("The color of the material.")]
	public class Revit_Material_Color : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Material_Color()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(object)));
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

	[NodeName("Revit_Solid_ComputeCentroid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The XYZ point of the Centroid of this solid.")]
	public class Revit_Solid_ComputeCentroid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_ComputeCentroid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(object)));
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

	[NodeName("Revit_Solid_Volume")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("Returns the signed volume of this solid.")]
	public class Revit_Solid_Volume : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_Volume()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(object)));
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

	[NodeName("Revit_Solid_SurfaceArea")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("Returns the total surface area of this solid.")]
	public class Revit_Solid_SurfaceArea : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_SurfaceArea()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(object)));
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

	[NodeName("Revit_Solid_Faces")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The faces that belong to the solid.")]
	public class Revit_Solid_Faces : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_Faces()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(object)));
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

	[NodeName("Revit_Solid_Edges")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_SOLID)]
	[NodeDescription("The edges that belong to the solid.")]
	public class Revit_Solid_Edges : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Solid_Edges()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(object)));
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

	[NodeName("Revit_Arc_Radius")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the radius of the arc.")]
	public class Revit_Arc_Radius : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_Radius()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
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

	[NodeName("Revit_Arc_YDirection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the Y direction.")]
	public class Revit_Arc_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_YDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
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

	[NodeName("Revit_Arc_XDirection")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the X direction.")]
	public class Revit_Arc_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_XDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
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

	[NodeName("Revit_Arc_Normal")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the normal to the plane in which the arc is defined.")]
	public class Revit_Arc_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_Normal()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
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

	[NodeName("Revit_Arc_Center")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_ARC)]
	[NodeDescription("Returns the center of the arc.")]
	public class Revit_Arc_Center : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Arc_Center()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(object)));
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

	[NodeName("Revit_Blend_TopProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class Revit_Blend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_Blend_BottomProfile")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class Revit_Blend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_Blend_TopOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The offset of the top end of the blend relative to the sketch plane.")]
	public class Revit_Blend_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_TopOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_Blend_BottomOffset")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("The offset of the bottom end of the blend relative to the sketch plane.")]
	public class Revit_Blend_BottomOffset : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_BottomOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_Blend_BottomSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("Returns the Bottom Sketch of the Blend.")]
	public class Revit_Blend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_Blend_TopSketch")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_BLEND)]
	[NodeDescription("Returns the Top Sketch of the Blend.")]
	public class Revit_Blend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Blend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(object)));
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

	[NodeName("Revit_ModelCurve_GetVisibility")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("A copy of visibility settings for the model curve in a family document.")]
	public class Revit_ModelCurve_GetVisibility : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurve_GetVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(object)));
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

	[NodeName("Revit_ModelCurve_IsReferenceLine")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("Indicates if this curve is a reference curve.")]
	public class Revit_ModelCurve_IsReferenceLine : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurve_IsReferenceLine()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(object)));
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

	[NodeName("Revit_ModelCurve_TrussCurveType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("The truss curve type of this model curve.")]
	public class Revit_ModelCurve_TrussCurveType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurve_TrussCurveType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(object)));
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

	[NodeName("Revit_ModelCurve_Subcategory")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_MODELCURVE)]
	[NodeDescription("The subcategory.")]
	public class Revit_ModelCurve_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_ModelCurve_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(object)));
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

	[NodeName("Revit_Level_PlaneReference")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Returns a reference to this element as a plane.")]
	public class Revit_Level_PlaneReference : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Level_PlaneReference()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
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

	[NodeName("Revit_Level_LevelType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("The level style of this level.")]
	public class Revit_Level_LevelType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Level_LevelType()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
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

	[NodeName("Revit_Level_ProjectElevation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.")]
	public class Revit_Level_ProjectElevation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Level_ProjectElevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
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

	[NodeName("Revit_Level_Elevation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_LEVEL)]
	[NodeDescription("Retrieves or changes the elevation above or below the ground level.")]
	public class Revit_Level_Elevation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Level_Elevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(object)));
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

	[NodeName("Revit_NurbSpline_Knots")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Return/set the knots of the nurb spline.")]
	public class Revit_NurbSpline_Knots : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_Knots()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_NurbSpline_Weights")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the weights of the nurb spline.")]
	public class Revit_NurbSpline_Weights : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_Weights()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_NurbSpline_CtrlPoints")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the control points of the nurb spline.")]
	public class Revit_NurbSpline_CtrlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_CtrlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_NurbSpline_Degree")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns the degree of the nurb spline.")]
	public class Revit_NurbSpline_Degree : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_Degree()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_NurbSpline_isRational")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Returns whether the nurb spline is rational or not.")]
	public class Revit_NurbSpline_isRational : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_isRational()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_NurbSpline_isClosed")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_NURBSPLINE)]
	[NodeDescription("Return/set the nurb spline's isClosed property.")]
	public class Revit_NurbSpline_isClosed : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_NurbSpline_isClosed()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(object)));
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

	[NodeName("Revit_Wall_Create")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Revit_Wall_Create : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Create()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(object)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered to be inside and outside.",typeof(object)));
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

	[NodeName("Revit_Wall_Create_1")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Revit_Wall_Create_1 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Create_1()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(object)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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

	[NodeName("Revit_Wall_Create_2")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Revit_Wall_Create_2 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Create_2()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(object)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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

	[NodeName("Revit_Wall_Create_3")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Revit_Wall_Create_3 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Create_3()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(object)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(object)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(object)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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

	[NodeName("Revit_Wall_Create_4")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("If successful a new wall object within the project.")]
	public class Revit_Wall_Create_4 : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Create_4()
		{
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(object)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(object)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(object)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(object)));
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

	[NodeName("Revit_Wall_Orientation")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("The normal vector projected from the exterior side of the wall.")]
	public class Revit_Wall_Orientation : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Orientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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

	[NodeName("Revit_Wall_Flipped")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Property to test whether the wall orientation is flipped.")]
	public class Revit_Wall_Flipped : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Flipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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

	[NodeName("Revit_Wall_StructuralUsage")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Retrieves or changes  the wall's designated structural usage.")]
	public class Revit_Wall_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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

	[NodeName("Revit_Wall_Width")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Get the overall thickness of the wall.")]
	public class Revit_Wall_Width : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_Width()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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

	[NodeName("Revit_Wall_CurtainGrid")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Get the grid object of a curtain wall")]
	public class Revit_Wall_CurtainGrid : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_CurtainGrid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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

	[NodeName("Revit_Wall_WallType")]
	[NodeCategory(BuiltinNodeCategories.AUTODESK_REVIT_DB_WALL)]
	[NodeDescription("Retrieves or changes the type of the wall.")]
	public class Revit_Wall_WallType : dynRevitTransactionNodeWithOneOutput
	{
		public Revit_Wall_WallType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(object)));
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
