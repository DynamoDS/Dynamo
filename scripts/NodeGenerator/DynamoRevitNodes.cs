using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Core;
using System.Reflection;
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
	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetHubId
	///</summary>
	[NodeName("API_ReferencePoint_GetHubId")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Id of associated Hub.")]
	public class API_ReferencePoint_GetHubId : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetHubId
		///</summary>
		public API_ReferencePoint_GetHubId()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetHubId", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Id of associated Hub.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetVisibility
	///</summary>
	[NodeName("API_ReferencePoint_GetVisibility")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the point.")]
	public class API_ReferencePoint_GetVisibility : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetVisibility
		///</summary>
		public API_ReferencePoint_GetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetVisibility", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the visibility for the point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXZ
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceXZ")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the XZ plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceXZ : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXZ
		///</summary>
		public API_ReferencePoint_GetCoordinatePlaneReferenceXZ()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinatePlaneReferenceXZ", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","A reference for the XZ plane of the coordinatesystem.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceYZ
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceYZ")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the YZ plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceYZ : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceYZ
		///</summary>
		public API_ReferencePoint_GetCoordinatePlaneReferenceYZ()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinatePlaneReferenceYZ", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","A reference for the YZ plane of the coordinatesystem.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXY
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceXY")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the XY plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceXY : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXY
		///</summary>
		public API_ReferencePoint_GetCoordinatePlaneReferenceXY()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinatePlaneReferenceXY", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","A reference for the XY plane of the coordinatesystem.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetInterpolatingCurves
	///</summary>
	[NodeName("API_ReferencePoint_GetInterpolatingCurves")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The set of CurveByPoints elements that interpolatea ReferencePoint.")]
	public class API_ReferencePoint_GetInterpolatingCurves : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetInterpolatingCurves
		///</summary>
		public API_ReferencePoint_GetInterpolatingCurves()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInterpolatingCurves", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The set of CurveByPoints elements that interpolatea ReferencePoint.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinateSystem
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinateSystem")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The position and orientation of the ReferencePoint.")]
	public class API_ReferencePoint_GetCoordinateSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinateSystem
		///</summary>
		public API_ReferencePoint_GetCoordinateSystem()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinateSystem", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The position and orientation of the ReferencePoint.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetPointElementReference
	///</summary>
	[NodeName("API_ReferencePoint_SetPointElementReference")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Change the rule for computing the location of the ReferencePoint relative to other elements inthe document.")]
	public class API_ReferencePoint_SetPointElementReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetPointElementReference
		///</summary>
		public API_ReferencePoint_SetPointElementReference()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetPointElementReference", false, new Type[]{typeof(Autodesk.Revit.DB.PointElementReference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			InPortData.Add(new PortData("val", "An object specifyinga rule for the location and orientation of a ReferencePoint.(Note: The ReferencePoint object does not store thepointElementReference object after this call.)",typeof(Autodesk.Revit.DB.PointElementReference)));
			OutPortData.Add(new PortData("out","Change the rule for computing the location of the ReferencePoint relative to other elements inthe document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetPointElementReference
	///</summary>
	[NodeName("API_ReferencePoint_GetPointElementReference")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve a copy of the rule that computes thelocation of the ReferencePoint relative to other elements inthe document.")]
	public class API_ReferencePoint_GetPointElementReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetPointElementReference
		///</summary>
		public API_ReferencePoint_GetPointElementReference()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPointElementReference", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Retrieve a copy of the rule that computes thelocation of the ReferencePoint relative to other elements inthe document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_ReferencePoint_ShowNormalReferencePlaneOnly")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether all three coordinate planes are shown, or only thenormal (XY) plane.")]
	public class API_ReferencePoint_ShowNormalReferencePlaneOnly : dynRevitTransactionNodeWithOneOutput
	{
		public API_ReferencePoint_ShowNormalReferencePlaneOnly()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Whether all three coordinate planes are shown, or only thenormal (XY) plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePoint)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ReferencePoint));
			var result = arg0.ShowNormalReferencePlaneOnly;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ReferencePoint_CoordinatePlaneVisibility")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Visibility settings for the coordinate reference planes.")]
	public class API_ReferencePoint_CoordinatePlaneVisibility : dynRevitTransactionNodeWithOneOutput
	{
		public API_ReferencePoint_CoordinatePlaneVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Visibility settings for the coordinate reference planes.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePoint)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ReferencePoint));
			var result = arg0.CoordinatePlaneVisibility;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ReferencePoint_Visible")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the point is visible when the family is loadedinto a project.")]
	public class API_ReferencePoint_Visible : dynRevitTransactionNodeWithOneOutput
	{
		public API_ReferencePoint_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Whether the point is visible when the family is loadedinto a project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePoint)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ReferencePoint));
			var result = arg0.Visible;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ReferencePoint_Position")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The position of the ReferencePoint.")]
	public class API_ReferencePoint_Position : dynRevitTransactionNodeWithOneOutput
	{
		public API_ReferencePoint_Position()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","The position of the ReferencePoint.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ReferencePoint)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ReferencePoint));
			var result = arg0.Position;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_HermiteFace_MixedDerivs")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Mixed derivatives of the surface.")]
	public class API_HermiteFace_MixedDerivs : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteFace_MixedDerivs()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Mixed derivatives of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteFace));
			var result = arg0.MixedDerivs;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_HermiteFace_Points")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Interpolation points of the surface.")]
	public class API_HermiteFace_Points : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteFace_Points()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Interpolation points of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteFace));
			var result = arg0.Points;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.AlmostEqual
	///</summary>
	[NodeName("API_Transform_AlmostEqual")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this transformation and the specified transformation are the same within the tolerance (1.0e-09).")]
	public class API_Transform_AlmostEqual : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.AlmostEqual
		///</summary>
		public API_Transform_AlmostEqual()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AlmostEqual", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The transformation to compare with this transformation.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Determines whether this transformation and the specified transformation are the same within the tolerance (1.0e-09).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasisAndOrigin
	///</summary>
	[NodeName("API_Transform_ScaleBasisAndOrigin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scales the basis vectors and the origin of this transformation and returns the result.")]
	public class API_Transform_ScaleBasisAndOrigin : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasisAndOrigin
		///</summary>
		public API_Transform_ScaleBasisAndOrigin()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ScaleBasisAndOrigin", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The scale value.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Scales the basis vectors and the origin of this transformation and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasis
	///</summary>
	[NodeName("API_Transform_ScaleBasis")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scales the basis vectors of this transformation and returns the result.")]
	public class API_Transform_ScaleBasis : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasis
		///</summary>
		public API_Transform_ScaleBasis()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ScaleBasis", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The scale value.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Scales the basis vectors of this transformation and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.Multiply
	///</summary>
	[NodeName("API_Transform_Multiply")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this transformation by the specified transformation and returns the result.")]
	public class API_Transform_Multiply : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.Multiply
		///</summary>
		public API_Transform_Multiply()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Multiply", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The specified transformation.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Multiplies this transformation by the specified transformation and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.OfVector
	///</summary>
	[NodeName("API_Transform_OfVector")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Applies the transform to the vector")]
	public class API_Transform_OfVector : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.OfVector
		///</summary>
		public API_Transform_OfVector()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "OfVector", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to be transformed",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Applies the transform to the vector",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.OfPoint
	///</summary>
	[NodeName("API_Transform_OfPoint")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Applies the transformation to the point and returns the result.")]
	public class API_Transform_OfPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.OfPoint
		///</summary>
		public API_Transform_OfPoint()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "OfPoint", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The point to transform.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Applies the transformation to the point and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateReflection
	///</summary>
	[NodeName("API_Transform_CreateReflection")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a transform that represents a reflection across the given plane.")]
	public class API_Transform_CreateReflection : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateReflection
		///</summary>
		public API_Transform_CreateReflection()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateReflection", false, new Type[]{typeof(Autodesk.Revit.DB.Plane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("p", "The plane.",typeof(Autodesk.Revit.DB.Plane)));
			OutPortData.Add(new PortData("out","Creates a transform that represents a reflection across the given plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateRotationAtPoint
	///</summary>
	[NodeName("API_Transform_CreateRotationAtPoint")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a transform that represents a rotation about the given axis at the specified point.")]
	public class API_Transform_CreateRotationAtPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateRotationAtPoint
		///</summary>
		public API_Transform_CreateRotationAtPoint()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateRotationAtPoint", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The rotation axis.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The angle.",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The origin point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a transform that represents a rotation about the given axis at the specified point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateRotation
	///</summary>
	[NodeName("API_Transform_CreateRotation")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a transform that represents a rotation about the given axis at (0, 0, 0).")]
	public class API_Transform_CreateRotation : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateRotation
		///</summary>
		public API_Transform_CreateRotation()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateRotation", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The rotation axis.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The angle.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a transform that represents a rotation about the given axis at (0, 0, 0).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateTranslation
	///</summary>
	[NodeName("API_Transform_CreateTranslation")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a transform that represents a translation via the specified vector.")]
	public class API_Transform_CreateTranslation : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.CreateTranslation
		///</summary>
		public API_Transform_CreateTranslation()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateTranslation", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The translation vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a transform that represents a translation via the specified vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_Inverse")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The inverse transformation of this transformation.")]
	public class API_Transform_Inverse : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_Inverse()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The inverse transformation of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.Inverse;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_Determinant")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The determinant of this transformation.")]
	public class API_Transform_Determinant : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_Determinant()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The determinant of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.Determinant;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_IsConformal")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is conformal.")]
	public class API_Transform_IsConformal : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_IsConformal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is conformal.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.IsConformal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_HasReflection")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation produces reflection.")]
	public class API_Transform_HasReflection : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_HasReflection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation produces reflection.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.HasReflection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_Scale")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The real number that represents the scale of the transformation.")]
	public class API_Transform_Scale : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_Scale()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The real number that represents the scale of the transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.Scale;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_IsTranslation")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is a translation.")]
	public class API_Transform_IsTranslation : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_IsTranslation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is a translation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.IsTranslation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_IsIdentity")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is an identity.")]
	public class API_Transform_IsIdentity : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_IsIdentity()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is an identity.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.IsIdentity;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_Origin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Defines the origin of the old coordinate system in the new coordinate system.")]
	public class API_Transform_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Defines the origin of the old coordinate system in the new coordinate system.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_BasisZ")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the Z axis of this transformation.")]
	public class API_Transform_BasisZ : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_BasisZ()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Z axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.BasisZ;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_BasisY")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the Y axis of this transformation.")]
	public class API_Transform_BasisY : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_BasisY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Y axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.BasisY;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Transform_BasisX")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the X axis of this transformation.")]
	public class API_Transform_BasisX : dynRevitTransactionNodeWithOneOutput
	{
		public API_Transform_BasisX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the X axis of this transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Transform)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Transform));
			var result = arg0.BasisX;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Mesh_MaterialElementId")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Element ID of the material from which this mesh is composed.")]
	public class API_Mesh_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Mesh_MaterialElementId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this mesh is composed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = arg0.MaterialElementId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Mesh_Vertices")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves all vertices used to define this mesh. Intended for indexed access.")]
	public class API_Mesh_Vertices : dynRevitTransactionNodeWithOneOutput
	{
		public API_Mesh_Vertices()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Retrieves all vertices used to define this mesh. Intended for indexed access.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = arg0.Vertices;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Mesh_NumTriangles")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of triangles that the mesh contains.")]
	public class API_Mesh_NumTriangles : dynRevitTransactionNodeWithOneOutput
	{
		public API_Mesh_NumTriangles()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","The number of triangles that the mesh contains.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Mesh)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Mesh));
			var result = arg0.NumTriangles;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Ellipse.Create
	///</summary>
	[NodeName("API_Ellipse_Create")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric ellipse or elliptical arc object.")]
	public class API_Ellipse_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Ellipse.Create
		///</summary>
		public API_Ellipse_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.Ellipse);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The center.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The x vector radius of the ellipse.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The y vector radius of the ellipse.",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The x axis to define the ellipse plane.  Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The y axis to define the ellipse plane.   Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The raw parameter value at the start of the ellipse.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The raw parameter value at the end of the ellipse.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new geometric ellipse or elliptical arc object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_RadiusY")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Y vector radius of the ellipse.")]
	public class API_Ellipse_RadiusY : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_RadiusY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the Y vector radius of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.RadiusY;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Ellipse_RadiusX")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the X vector radius of the ellipse.")]
	public class API_Ellipse_RadiusX : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_RadiusX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the X vector radius of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.RadiusX;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Ellipse_YDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The Y direction.")]
	public class API_Ellipse_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_YDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The Y direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.YDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Ellipse_XDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The X direction.")]
	public class API_Ellipse_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_XDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The X direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.XDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Ellipse_Normal")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal to the plane in which the ellipse is defined.")]
	public class API_Ellipse_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_Normal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the ellipse is defined.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.Normal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Ellipse_Center")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the center of the ellipse.")]
	public class API_Ellipse_Center : dynRevitTransactionNodeWithOneOutput
	{
		public API_Ellipse_Center()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the center of the ellipse.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Ellipse)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Ellipse));
			var result = arg0.Center;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDividedSurface
	///</summary>
	[NodeName("API_FamilyItemFactory_NewDividedSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a DividedSurface element on one surface of another element.")]
	public class API_FamilyItemFactory_NewDividedSurface : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDividedSurface
		///</summary>
		public API_FamilyItemFactory_NewDividedSurface()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDividedSurface", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to a surface on an existing element. The elementmust be one of the following:",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Create a DividedSurface element on one surface of another element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewCurveByPoints
	///</summary>
	[NodeName("API_FamilyItemFactory_NewCurveByPoints")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a 3d curve through two or more points in an AutodeskRevit family document.")]
	public class API_FamilyItemFactory_NewCurveByPoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewCurveByPoints
		///</summary>
		public API_FamilyItemFactory_NewCurveByPoints()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurveByPoints", false, new Type[]{typeof(Autodesk.Revit.DB.ReferencePointArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Two or more PointElements. The curve will interpolatethese points.",typeof(Autodesk.Revit.DB.ReferencePointArray)));
			OutPortData.Add(new PortData("out","Create a 3d curve through two or more points in an AutodeskRevit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSymbolicCurve
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSymbolicCurve")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a symbolic curve in an Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewSymbolicCurve : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSymbolicCurve
		///</summary>
		public API_FamilyItemFactory_NewSymbolicCurve()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSymbolicCurve", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The geometry curve of the newly created symbolic curve.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane for the symbolic curve.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Create a symbolic curve in an Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewControl
	///</summary>
	[NodeName("API_FamilyItemFactory_NewControl")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new control into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewControl : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewControl
		///</summary>
		public API_FamilyItemFactory_NewControl()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewControl", false, new Type[]{typeof(Autodesk.Revit.DB.ControlShape),typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The shape of the control.",typeof(Autodesk.Revit.DB.ControlShape)));
			InPortData.Add(new PortData("v", "The view in which the control is to be visible. Itmust be a FloorPlan view or a CeilingPlan view.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the control.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Add a new control into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewModelText
	///</summary>
	[NodeName("API_FamilyItemFactory_NewModelText")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a model text in the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewModelText : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewModelText
		///</summary>
		public API_FamilyItemFactory_NewModelText()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewModelText", false, new Type[]{typeof(System.String),typeof(Autodesk.Revit.DB.ModelTextType),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.HorizontalAlign),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The text to be displayed.",typeof(System.String)));
			InPortData.Add(new PortData("mtt", "The type of model text. If this parameter is",typeof(Autodesk.Revit.DB.ModelTextType)));
			InPortData.Add(new PortData("sp", "The sketch plane of the model text. The direction of model text is determined by the normal of the sketch plane.To extrude in the other direction set the depth value to negative.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("xyz", "The position of the model text. The position must lie in the sketch plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("ha", "The horizontal alignment.",typeof(Autodesk.Revit.DB.HorizontalAlign)));
			InPortData.Add(new PortData("n", "The depth of the model text.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Create a model text in the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewOpening
	///</summary>
	[NodeName("API_FamilyItemFactory_NewOpening")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create an opening to cut the wall or ceiling.")]
	public class API_FamilyItemFactory_NewOpening : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewOpening
		///</summary>
		public API_FamilyItemFactory_NewOpening()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewOpening", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.CurveArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("el", "Host elements that new opening would lie in. The host can only be a wall or a ceiling.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created opening. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. The profiles will be projected into the host plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","Create an opening to cut the wall or ceiling.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRadialDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new radial dimension object using a specified dimension type.")]
	public class API_FamilyItemFactory_NewRadialDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
		///</summary>
		public API_FamilyItemFactory_NewRadialDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRadialDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.DimensionType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","Generate a new radial dimension object using a specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDiameterDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewDiameterDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new diameter dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewDiameterDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDiameterDimension
		///</summary>
		public API_FamilyItemFactory_NewDiameterDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDiameterDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the diameter dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new diameter dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRadialDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new radial dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewRadialDimension_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
		///</summary>
		public API_FamilyItemFactory_NewRadialDimension_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRadialDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point where the witness line of the radial dimension will lie.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new radial dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewArcLengthDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new arc length dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewArcLengthDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
		///</summary>
		public API_FamilyItemFactory_NewArcLengthDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArcLengthDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Arc),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.DimensionType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","Creates a new arc length dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewArcLengthDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new arc length dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewArcLengthDimension_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
		///</summary>
		public API_FamilyItemFactory_NewArcLengthDimension_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArcLengthDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Arc),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "Geometric reference of the arc to which the dimension is to be bound.This reference must be parallel to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound. This reference must intersect the arcRef reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a new arc length dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewAngularDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new angular dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewAngularDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
		///</summary>
		public API_FamilyItemFactory_NewAngularDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAngularDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Arc),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.DimensionType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","Creates a new angular dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewAngularDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new angular dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewAngularDimension_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
		///</summary>
		public API_FamilyItemFactory_NewAngularDimension_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAngularDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Arc),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("arc", "The extension arc of the dimension.",typeof(Autodesk.Revit.DB.Arc)));
			InPortData.Add(new PortData("ref", "The first geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second geometric reference to which the dimension is to be bound.The reference must be perpendicular to the extension arc.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a new angular dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLinearDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewLinearDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
		///</summary>
		public API_FamilyItemFactory_NewLinearDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLinearDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.DimensionType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","Creates a new linear dimension object using the specified dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLinearDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new linear dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewLinearDimension_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
		///</summary>
		public API_FamilyItemFactory_NewLinearDimension_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLinearDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The extension line of the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.You must supply at least two references, and all references supplied must be parallel to each other and perpendicular to the extension line.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Generate a new linear dimension object using the default dimension type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByThickenSingleSurface
	///</summary>
	[NodeName("API_FamilyItemFactory_NewFormByThickenSingleSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewFormByThickenSingleSurface : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByThickenSingleSurface
		///</summary>
		public API_FamilyItemFactory_NewFormByThickenSingleSurface()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFormByThickenSingleSurface", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.Form),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("frm", "The single-surface form element. It can have one top/bottom face or one side face.",typeof(Autodesk.Revit.DB.Form)));
			InPortData.Add(new PortData("xyz", "The offset of capped solid.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByCap
	///</summary>
	[NodeName("API_FamilyItemFactory_NewFormByCap")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewFormByCap : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByCap
		///</summary>
		public API_FamilyItemFactory_NewFormByCap()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFormByCap", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of the newly created cap. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolveForms
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRevolveForms")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewRevolveForms : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolveForms
		///</summary>
		public API_FamilyItemFactory_NewRevolveForms()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRevolveForms", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.Reference),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of the newly created revolution. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("ref", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlendForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlendForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewSweptBlendForm : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlendForm
		///</summary>
		public API_FamilyItemFactory_NewSweptBlendForm()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSweptBlendForm", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.ReferenceArrayArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The path of the swept blend. The path should be 2D, where all input curves lie in one plane. If there’s more than one profile, the path should be a single curve. It’s required to reference existing geometry.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created swept blend. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArrayArray)));
			OutPortData.Add(new PortData("out","Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusionForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewExtrusionForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewExtrusionForm : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusionForm
		///</summary>
		public API_FamilyItemFactory_NewExtrusionForm()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewExtrusionForm", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The profile of extrusion. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("xyz", "The direction of extrusion, with its length the length of the extrusion. The direction must be perpendicular to the plane determined by profile. The length of vector must be non-zero.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLoftForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLoftForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Loft operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewLoftForm : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLoftForm
		///</summary>
		public API_FamilyItemFactory_NewLoftForm()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoftForm", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArrayArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Form is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("arar", "The profile set of the newly created loft. Each profile should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.ReferenceArrayArray)));
			OutPortData.Add(new PortData("out","Create new Form element by Loft operation, and add it into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new swept blend into the family document, using a selected reference as the path.")]
	public class API_FamilyItemFactory_NewSweptBlend : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
		///</summary>
		public API_FamilyItemFactory_NewSweptBlend()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSweptBlend", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.SweepProfile),typeof(Autodesk.Revit.DB.SweepProfile)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("ref", "The path of the swept blend. The path might be a reference of single curve or edge obtained from existing geometry.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			OutPortData.Add(new PortData("out","Adds a new swept blend into the family document, using a selected reference as the path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlend_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new swept blend into the family document, using a curve as the path.")]
	public class API_FamilyItemFactory_NewSweptBlend_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
		///</summary>
		public API_FamilyItemFactory_NewSweptBlend_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSweptBlend", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.SweepProfile),typeof(Autodesk.Revit.DB.SweepProfile)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the swept blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crv", "The path of the swept blend. The path should be a single curve.Or the path can be a single sketched curve, and the curve is not required to reference existing geometry.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("swpp", "The bottom profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("swpp", "The top profile of the newly created Swept blend. It should consist of only one curve loop.the input profile must be in one plane.",typeof(Autodesk.Revit.DB.SweepProfile)));
			OutPortData.Add(new PortData("out","Add a new swept blend into the family document, using a curve as the path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweep")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form into the family document, using an array of selected references as a 3D path.")]
	public class API_FamilyItemFactory_NewSweep : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
		///</summary>
		public API_FamilyItemFactory_NewSweep()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSweep", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.SweepProfile),typeof(System.Int32),typeof(Autodesk.Revit.DB.ProfilePlaneLocation)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("refa", "The path of the sweep. The path should be reference of curve or edge obtained from existing geometry.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("swpp", "The profile to create the new Sweep. The profile must lie in the XY plane, and it will be transformed to the profile plane automatically. This may contain more than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(System.Int32)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(Autodesk.Revit.DB.ProfilePlaneLocation)));
			OutPortData.Add(new PortData("out","Adds a new sweep form into the family document, using an array of selected references as a 3D path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweep_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form to the family document, using a path of curve elements.")]
	public class API_FamilyItemFactory_NewSweep_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
		///</summary>
		public API_FamilyItemFactory_NewSweep_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSweep", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.SweepProfile),typeof(System.Int32),typeof(Autodesk.Revit.DB.ProfilePlaneLocation)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Sweep is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The path of the sweep. The path should be 2D, where all input curves lie in one plane, and the curves are not required to reference existing geometry.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the path. Use this when you want to create a 2D path that resides on an existing planar face. Optional, can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(System.Int32)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(Autodesk.Revit.DB.ProfilePlaneLocation)));
			OutPortData.Add(new PortData("out","Adds a new sweep form to the family document, using a path of curve elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolution
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRevolution")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Revolution instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewRevolution : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolution
		///</summary>
		public API_FamilyItemFactory_NewRevolution()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRevolution", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.CurveArrArray),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.Line),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Revolution is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created revolution. This may containmore than one curve loop. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the revolution.  The direction of revolutionis determined by the normal for the sketch plane.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crv", "The axis of revolution. This axis must lie in the same plane as the curve loops.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The start angle of Revolution in radians.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of Revolution in radians.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Add a new Revolution instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Blend instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewBlend : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewBlend
		///</summary>
		public API_FamilyItemFactory_NewBlend()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBlend", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Blend is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The top blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("crvs", "The base blend section. It should consist of only one curve loop.The input profile must be in one plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the base profile. Use this to associate the base of the blend to geometry from another element. Optional, it can be",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Add a new Blend instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusion
	///</summary>
	[NodeName("API_FamilyItemFactory_NewExtrusion")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Extrusion instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewExtrusion : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusion
		///</summary>
		public API_FamilyItemFactory_NewExtrusion()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewExtrusion", false, new Type[]{typeof(System.Boolean),typeof(Autodesk.Revit.DB.CurveArrArray),typeof(Autodesk.Revit.DB.SketchPlane),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("b", "Indicates if the Extrusion is Solid or Void.",typeof(System.Boolean)));
			InPortData.Add(new PortData("crvs", "The profile of the newly created Extrusion. This may contain more than one curve loop. Each loop must be a fully closed curve loop and the loops may not intersect. All input curves must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			InPortData.Add(new PortData("sp", "The sketch plane for the extrusion.  The direction of extrusionis determined by the normal for the sketch plane.  To extrude in the other direction set the end value to negative.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("n", "The length of the extrusion.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Add a new Extrusion instance into the Autodesk Revit family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewAlignment
	///</summary>
	[NodeName("API_ItemFactoryBase_NewAlignment")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new locked alignment into the Autodesk Revit document.")]
	public class API_ItemFactoryBase_NewAlignment : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewAlignment
		///</summary>
		public API_ItemFactoryBase_NewAlignment()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAlignment", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view that determines the orientation of the alignment.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The first reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Add a new locked alignment into the Autodesk Revit document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.PlaceGroup
	///</summary>
	[NodeName("API_ItemFactoryBase_PlaceGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Place an instance of a Model Group into the Autodesk Revit document, using a locationand a group type.")]
	public class API_ItemFactoryBase_PlaceGroup : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.PlaceGroup
		///</summary>
		public API_ItemFactoryBase_PlaceGroup()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "PlaceGroup", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.GroupType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the group is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "A GroupType object that represents the type of group that is to be placed.",typeof(Autodesk.Revit.DB.GroupType)));
			OutPortData.Add(new PortData("out","Place an instance of a Model Group into the Autodesk Revit document, using a locationand a group type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
	///</summary>
	[NodeName("API_ItemFactoryBase_NewTextNote")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new text note with a single leader.")]
	public class API_ItemFactoryBase_NewTextNote : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
		///</summary>
		public API_ItemFactoryBase_NewTextNote()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTextNote", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(Autodesk.Revit.DB.TextAlignFlags),typeof(Autodesk.Revit.DB.TextNoteLeaderTypes),typeof(Autodesk.Revit.DB.TextNoteLeaderStyles),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
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
			OutPortData.Add(new PortData("out","Creates a new text note with a single leader.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
	///</summary>
	[NodeName("API_ItemFactoryBase_NewTextNote_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TextNote object without a leader.")]
	public class API_ItemFactoryBase_NewTextNote_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
		///</summary>
		public API_ItemFactoryBase_NewTextNote_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTextNote", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(Autodesk.Revit.DB.TextAlignFlags),typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(System.Double)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(Autodesk.Revit.DB.TextAlignFlags)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates a new TextNote object without a leader.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane on a reference to existing planar geometry.")]
	public class API_ItemFactoryBase_NewSketchPlane : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
		///</summary>
		public API_ItemFactoryBase_NewSketchPlane()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSketchPlane", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The planar face reference to locate sketch plane.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a new sketch plane on a reference to existing planar geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane on a planar face of existing geometry.")]
	public class API_ItemFactoryBase_NewSketchPlane_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
		///</summary>
		public API_ItemFactoryBase_NewSketchPlane_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSketchPlane", false, new Type[]{typeof(Autodesk.Revit.DB.PlanarFace)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The geometry planar face to locate sketch plane.",typeof(Autodesk.Revit.DB.PlanarFace)));
			OutPortData.Add(new PortData("out","Creates a new sketch plane on a planar face of existing geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane from an arbitrary geometric plane.")]
	public class API_ItemFactoryBase_NewSketchPlane_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
		///</summary>
		public API_ItemFactoryBase_NewSketchPlane_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSketchPlane", false, new Type[]{typeof(Autodesk.Revit.DB.Plane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("p", "The geometry plane to locate sketch plane.",typeof(Autodesk.Revit.DB.Plane)));
			OutPortData.Add(new PortData("out","Creates a new sketch plane from an arbitrary geometric plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane2
	///</summary>
	[NodeName("API_ItemFactoryBase_NewReferencePlane2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of ReferencePlane.")]
	public class API_ItemFactoryBase_NewReferencePlane2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane2
		///</summary>
		public API_ItemFactoryBase_NewReferencePlane2()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePlane2", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A third point needed to define the reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new instance of ReferencePlane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewReferencePlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of ReferencePlane.")]
	public class API_ItemFactoryBase_NewReferencePlane : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane
		///</summary>
		public API_ItemFactoryBase_NewReferencePlane()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePlane", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The bubble end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The free end applied to reference plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The cut vector apply to reference plane, should perpendicular to the vector  (bubbleEnd-freeEnd).",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("v", "The specific view apply to the Reference plane.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new instance of ReferencePlane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewLevel
	///</summary>
	[NodeName("API_ItemFactoryBase_NewLevel")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new level.")]
	public class API_ItemFactoryBase_NewLevel : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewLevel
		///</summary>
		public API_ItemFactoryBase_NewLevel()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLevel", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The elevation to apply to the new level.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewModelCurve
	///</summary>
	[NodeName("API_ItemFactoryBase_NewModelCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new model line element.")]
	public class API_ItemFactoryBase_NewModelCurve : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewModelCurve
		///</summary>
		public API_ItemFactoryBase_NewModelCurve()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewModelCurve", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The internal geometry curve for model line.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("sp", "The sketch plane this new model line resides in.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new model line element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewGroup
	///</summary>
	[NodeName("API_ItemFactoryBase_NewGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type of group.")]
	public class API_ItemFactoryBase_NewGroup : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewGroup
		///</summary>
		public API_ItemFactoryBase_NewGroup()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGroup", false, new Type[]{typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			OutPortData.Add(new PortData("out","Creates a new type of group.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstances2
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstances2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Family instances within the document.")]
	public class API_ItemFactoryBase_NewFamilyInstances2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstances2
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstances2()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstances2", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of FamilyInstanceCreationData which wraps the creation arguments of the families to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)));
			OutPortData.Add(new PortData("out","Creates Family instances within the document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a line based detail family instance into the Autodesk Revit document, using an line and a view where the instance should be placed.")]
	public class API_ItemFactoryBase_NewFamilyInstance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The line location of family instance. The line must in the plane of the view.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the family instance.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Add a line based detail family instance into the Autodesk Revit document, using an line and a view where the instance should be placed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance into the Autodesk Revit document, using an origin and a view where the instance should be placed.")]
	public class API_ItemFactoryBase_NewFamilyInstance_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The origin of family instance. If created on a",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A family symbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("v", "The 2D view in which to place the family instance.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Add a new family instance into the Autodesk Revit document, using an origin and a view where the instance should be placed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a line on that face for its position, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted. Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face referenced by the input Reference instance, using a line on that face for its position, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_3")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a location, reference direction, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to a face.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face referenced by the input Reference instance, using a location, reference direction, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_4")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a line on that face for its position, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_4 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_4()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Face),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face of an existing element, using a line on that face for its position, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_5")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a location, reference direction, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_5 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_5()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Face),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.Note that this direction defines the rotation of the instance on the face, and thus cannot be parallelto the face normal.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.Note that this symbol must represent a family whose",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family onto a face of an existing element, using a location, reference direction, and a type/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_6")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location and atype/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_6 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_6()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a location and atype/symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_7")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, and the host element.")]
	public class API_ItemFactoryBase_NewFamilyInstance_7 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_7()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "The object into which the FamilyInstance is to be inserted, often known as the host.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, and the host element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_8")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a reference direction.")]
	public class API_ItemFactoryBase_NewFamilyInstance_8 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstance_8()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a reference direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDimension")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the specified dimension style.")]
	public class API_ItemFactoryBase_NewDimension : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
		///</summary>
		public API_ItemFactoryBase_NewDimension()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.DimensionType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("dimt", "The dimension style to be used for the dimension.",typeof(Autodesk.Revit.DB.DimensionType)));
			OutPortData.Add(new PortData("out","Creates a new linear dimension object using the specified dimension style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDimension_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the default dimension style.")]
	public class API_ItemFactoryBase_NewDimension_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
		///</summary>
		public API_ItemFactoryBase_NewDimension_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDimension", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The line drawn for the dimension.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("refa", "An array of geometric references to which the dimension is to be bound.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Creates a new linear dimension object using the default dimension style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurveArray
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDetailCurveArray")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an array of new detail curve elements.")]
	public class API_ItemFactoryBase_NewDetailCurveArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurveArray
		///</summary>
		public API_ItemFactoryBase_NewDetailCurveArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDetailCurveArray", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.CurveArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the detail curves are to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crvs", "An array containing the internal geometry curves for detail lines. The curve in array should be bound curve.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","Creates an array of new detail curve elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurve
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDetailCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new detail curve element.")]
	public class API_ItemFactoryBase_NewDetailCurve : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurve
		///</summary>
		public API_ItemFactoryBase_NewDetailCurve()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDetailCurve", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Curve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the detail curve is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("crv", "The internal geometry curve for detail curve. It should be a bound curve.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Creates a new detail curve element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Clone
	///</summary>
	[NodeName("API_PolyLine_Clone")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this polyline.")]
	public class API_PolyLine_Clone : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Clone
		///</summary>
		public API_PolyLine_Clone()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Clone", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a copy of this polyline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetOutline
	///</summary>
	[NodeName("API_PolyLine_GetOutline")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the outline of the polyline.")]
	public class API_PolyLine_GetOutline : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetOutline
		///</summary>
		public API_PolyLine_GetOutline()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetOutline", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the outline of the polyline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinates
	///</summary>
	[NodeName("API_PolyLine_GetCoordinates")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the coordinate points of the polyline.")]
	public class API_PolyLine_GetCoordinates : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinates
		///</summary>
		public API_PolyLine_GetCoordinates()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinates", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the coordinate points of the polyline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinate
	///</summary>
	[NodeName("API_PolyLine_GetCoordinate")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the coordinate point of the specified index.")]
	public class API_PolyLine_GetCoordinate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinate
		///</summary>
		public API_PolyLine_GetCoordinate()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCoordinate", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			InPortData.Add(new PortData("i", "The index of the coordinates.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Gets the coordinate point of the specified index.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Evaluate
	///</summary>
	[NodeName("API_PolyLine_Evaluate")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the polyline.")]
	public class API_PolyLine_Evaluate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Evaluate
		///</summary>
		public API_PolyLine_Evaluate()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Evaluate", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated. It is expected to be in [0,1] interval mapped to the bounds of the whole polyline.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Evaluates a parameter on the polyline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_PolyLine_NumberOfCoordinates")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the number of the coordinate points.")]
	public class API_PolyLine_NumberOfCoordinates : dynRevitTransactionNodeWithOneOutput
	{
		public API_PolyLine_NumberOfCoordinates()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine",typeof(Autodesk.Revit.DB.PolyLine)));
			OutPortData.Add(new PortData("out","Gets the number of the coordinate points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PolyLine)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PolyLine));
			var result = arg0.NumberOfCoordinates;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.ToString
	///</summary>
	[NodeName("API_XYZ_ToString")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets formatted string showing (X, Y, Z) with values formatted to 9 decimal places.")]
	public class API_XYZ_ToString : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.ToString
		///</summary>
		public API_XYZ_ToString()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ToString", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets formatted string showing (X, Y, Z) with values formatted to 9 decimal places.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleOnPlaneTo
	///</summary>
	[NodeName("API_XYZ_AngleOnPlaneTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector projected to the specified plane.")]
	public class API_XYZ_AngleOnPlaneTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleOnPlaneTo
		///</summary>
		public API_XYZ_AngleOnPlaneTo()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AngleOnPlaneTo", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The normal vector that defines the plane.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Returns the angle between this vector and the specified vector projected to the specified plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleTo
	///</summary>
	[NodeName("API_XYZ_AngleTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector.")]
	public class API_XYZ_AngleTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleTo
		///</summary>
		public API_XYZ_AngleTo()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AngleTo", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The specified vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Returns the angle between this vector and the specified vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.DistanceTo
	///</summary>
	[NodeName("API_XYZ_DistanceTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the distance from this point to the specified point.")]
	public class API_XYZ_DistanceTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.DistanceTo
		///</summary>
		public API_XYZ_DistanceTo()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DistanceTo", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Returns the distance from this point to the specified point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
	///</summary>
	[NodeName("API_XYZ_IsAlmostEqualTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether 2 vectors are the same within the given tolerance.")]
	public class API_XYZ_IsAlmostEqualTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
		///</summary>
		public API_XYZ_IsAlmostEqualTo()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAlmostEqualTo", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Determines whether 2 vectors are the same within the given tolerance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
	///</summary>
	[NodeName("API_XYZ_IsAlmostEqualTo_1")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this vector and the specified vector are the same within the tolerance (1.0e-09).")]
	public class API_XYZ_IsAlmostEqualTo_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
		///</summary>
		public API_XYZ_IsAlmostEqualTo_1()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAlmostEqualTo", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Determines whether this vector and the specified vector are the same within the tolerance (1.0e-09).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Divide
	///</summary>
	[NodeName("API_XYZ_Divide")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Divides this vector by the specified value and returns the result.")]
	public class API_XYZ_Divide : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Divide
		///</summary>
		public API_XYZ_Divide()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Divide", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Divides this vector by the specified value and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Multiply
	///</summary>
	[NodeName("API_XYZ_Multiply")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this vector by the specified value and returns the result.")]
	public class API_XYZ_Multiply : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Multiply
		///</summary>
		public API_XYZ_Multiply()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Multiply", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Multiplies this vector by the specified value and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Negate
	///</summary>
	[NodeName("API_XYZ_Negate")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Negates this vector.")]
	public class API_XYZ_Negate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Negate
		///</summary>
		public API_XYZ_Negate()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Negate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Negates this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Subtract
	///</summary>
	[NodeName("API_XYZ_Subtract")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Subtracts the specified vector from this vector and returns the result.")]
	public class API_XYZ_Subtract : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Subtract
		///</summary>
		public API_XYZ_Subtract()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Subtract", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to subtract from this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Subtracts the specified vector from this vector and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Add
	///</summary>
	[NodeName("API_XYZ_Add")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds the specified vector to this vector and returns the result.")]
	public class API_XYZ_Add : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Add
		///</summary>
		public API_XYZ_Add()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Add", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to add to this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Adds the specified vector to this vector and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.TripleProduct
	///</summary>
	[NodeName("API_XYZ_TripleProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The triple product of this vector and the two specified vectors.")]
	public class API_XYZ_TripleProduct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.TripleProduct
		///</summary>
		public API_XYZ_TripleProduct()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "TripleProduct", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The second vector.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The third vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The triple product of this vector and the two specified vectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.CrossProduct
	///</summary>
	[NodeName("API_XYZ_CrossProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cross product of this vector and the specified vector.")]
	public class API_XYZ_CrossProduct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.CrossProduct
		///</summary>
		public API_XYZ_CrossProduct()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CrossProduct", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The cross product of this vector and the specified vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.DotProduct
	///</summary>
	[NodeName("API_XYZ_DotProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The dot product of this vector and the specified vector.")]
	public class API_XYZ_DotProduct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.DotProduct
		///</summary>
		public API_XYZ_DotProduct()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DotProduct", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","The dot product of this vector and the specified vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.GetLength
	///</summary>
	[NodeName("API_XYZ_GetLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the length of this vector.")]
	public class API_XYZ_GetLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.GetLength
		///</summary>
		public API_XYZ_GetLength()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the length of this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Normalize
	///</summary>
	[NodeName("API_XYZ_Normalize")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new XYZ whose coordinates are the normalized values from this vector.")]
	public class API_XYZ_Normalize : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.Normalize
		///</summary>
		public API_XYZ_Normalize()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Normalize", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a new XYZ whose coordinates are the normalized values from this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsUnitLength
	///</summary>
	[NodeName("API_XYZ_IsUnitLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this vector is of unit length.")]
	public class API_XYZ_IsUnitLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsUnitLength
		///</summary>
		public API_XYZ_IsUnitLength()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsUnitLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this vector is of unit length.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsZeroLength
	///</summary>
	[NodeName("API_XYZ_IsZeroLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this vector is a zero vector.")]
	public class API_XYZ_IsZeroLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsZeroLength
		///</summary>
		public API_XYZ_IsZeroLength()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsZeroLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this vector is a zero vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
	///</summary>
	[NodeName("API_XYZ")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an XYZ with the supplied coordinates.")]
	public class API_XYZ : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
		///</summary>
		public API_XYZ()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "XYZ", true, new Type[]{typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The third coordinate.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates an XYZ with the supplied coordinates.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
	///</summary>
	[NodeName("API_XYZ_1")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a default XYZ with the values (0, 0, 0).")]
	public class API_XYZ_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
		///</summary>
		public API_XYZ_1()
		{
			base_type = typeof(Autodesk.Revit.DB.XYZ);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "XYZ", true, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a default XYZ with the values (0, 0, 0).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_XYZ_Z")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the third coordinate.")]
	public class API_XYZ_Z : dynRevitTransactionNodeWithOneOutput
	{
		public API_XYZ_Z()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the third coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = arg0.Z;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_XYZ_Y")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the second coordinate.")]
	public class API_XYZ_Y : dynRevitTransactionNodeWithOneOutput
	{
		public API_XYZ_Y()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = arg0.Y;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_XYZ_X")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the first coordinate.")]
	public class API_XYZ_X : dynRevitTransactionNodeWithOneOutput
	{
		public API_XYZ_X()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.XYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.XYZ));
			var result = arg0.X;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointConstraintType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPointConstraintType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets constrain type of an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPointConstraintType : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointConstraintType
		///</summary>
		public API_AdaptiveComponentFamilyUtils_SetPointConstraintType()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetPointConstraintType", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.AdaptivePointConstraintType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Constraint type of the Adaptive Shape Handle Point.",typeof(Autodesk.Revit.DB.AdaptivePointConstraintType)));
			OutPortData.Add(new PortData("out","Sets constrain type of an Adaptive Shape Handle Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointConstraintType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPointConstraintType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets constrain type of an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPointConstraintType : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointConstraintType
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetPointConstraintType()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPointConstraintType", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Gets constrain type of an Adaptive Shape Handle Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointOrientationType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPointOrientationType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets orientation type of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPointOrientationType : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointOrientationType
		///</summary>
		public API_AdaptiveComponentFamilyUtils_SetPointOrientationType()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetPointOrientationType", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.AdaptivePointOrientationType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Orientation type of the Adaptive Placement Point.",typeof(Autodesk.Revit.DB.AdaptivePointOrientationType)));
			OutPortData.Add(new PortData("out","Sets orientation type of an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointOrientationType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPointOrientationType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets orientation type of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPointOrientationType : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointOrientationType
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetPointOrientationType()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPointOrientationType", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Gets orientation type of an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPlacementNumber
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPlacementNumber")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets Placement Number of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPlacementNumber : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPlacementNumber
		///</summary>
		public API_AdaptiveComponentFamilyUtils_SetPlacementNumber()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetPlacementNumber", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("i", "Placement number of the Adaptive Placement Point.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Sets Placement Number of an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPlacementNumber
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPlacementNumber")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Placement number of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPlacementNumber : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPlacementNumber
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetPlacementNumber()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPlacementNumber", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Gets Placement number of an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.MakeAdaptivePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_MakeAdaptivePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Makes Reference Point an Adaptive Point or makes an Adaptive Point a Reference Point.")]
	public class API_AdaptiveComponentFamilyUtils_MakeAdaptivePoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.MakeAdaptivePoint
		///</summary>
		public API_AdaptiveComponentFamilyUtils_MakeAdaptivePoint()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MakeAdaptivePoint", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.AdaptivePointType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "The Adaptive Point Type",typeof(Autodesk.Revit.DB.AdaptivePointType)));
			OutPortData.Add(new PortData("out","Makes Reference Point an Adaptive Point or makes an Adaptive Point a Reference Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfShapeHandlePoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Shape Handle Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfShapeHandlePoints
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetNumberOfShapeHandlePoints", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyBase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Gets number of Shape Handle Point Elements in Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfPlacementPoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Placement Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfPlacementPoints
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetNumberOfPlacementPoints", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyBase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Gets number of Placement Point Elements in Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfAdaptivePoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Adaptive Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfAdaptivePoints
		///</summary>
		public API_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetNumberOfAdaptivePoints", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyBase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Gets number of Adaptive Point Elements in Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveShapeHandlePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveShapeHandlePoint
		///</summary>
		public API_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptiveShapeHandlePoint", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Verifies if the Reference Point is an Adaptive Shape Handle Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePlacementPoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePlacementPoint
		///</summary>
		public API_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptivePlacementPoint", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Verifies if the Reference Point is an Adaptive Placement Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptivePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptivePoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePoint
		///</summary>
		public API_AdaptiveComponentFamilyUtils_IsAdaptivePoint()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptivePoint", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The ReferencePoint id",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Verifies if the Reference Point is an Adaptive Point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Family is an Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily
		///</summary>
		public API_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentFamilyUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptiveComponentFamily", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyBase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentFamilyUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Family",typeof(Autodesk.Revit.DB.FamilyBase)));
			OutPortData.Add(new PortData("out","Verifies if the Family is an Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_CylindricalFace_Axis")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_CylindricalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public API_CylindricalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CylindricalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.CylindricalFace));
			var result = arg0.Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_CylindricalFace_Origin")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_CylindricalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public API_CylindricalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.CylindricalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.CylindricalFace));
			var result = arg0.Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ConicalFace_HalfAngle")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Half angle of the surface.")]
	public class API_ConicalFace_HalfAngle : dynRevitTransactionNodeWithOneOutput
	{
		public API_ConicalFace_HalfAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Half angle of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = arg0.HalfAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ConicalFace_Axis")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_ConicalFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public API_ConicalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = arg0.Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ConicalFace_Origin")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_ConicalFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public API_ConicalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ConicalFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ConicalFace));
			var result = arg0.Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_RuledFace_RulingsAreParallel")]
	[NodeSearchTags("face","ruled","rule")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines if the rulings of this ruled surface are parallel.")]
	public class API_RuledFace_RulingsAreParallel : dynRevitTransactionNodeWithOneOutput
	{
		public API_RuledFace_RulingsAreParallel()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RuledFace",typeof(Autodesk.Revit.DB.RuledFace)));
			OutPortData.Add(new PortData("out","Determines if the rulings of this ruled surface are parallel.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RuledFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RuledFace));
			var result = arg0.RulingsAreParallel;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTopographySurface
	///</summary>
	[NodeName("API_Document_NewTopographySurface")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TopographySurface element in the document, and initializes it with a set of points.")]
	public class API_Document_NewTopographySurface : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTopographySurface
		///</summary>
		public API_Document_NewTopographySurface()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTopographySurface", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "An array of initial points for the surface.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			OutPortData.Add(new PortData("out","Creates a new TopographySurface element in the document, and initializes it with a set of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTakeoffFitting
	///</summary>
	[NodeName("API_Document_NewTakeoffFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an takeoff fitting into the Autodesk Revit document,using one connector and one MEP curve.")]
	public class API_Document_NewTakeoffFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTakeoffFitting
		///</summary>
		public API_Document_NewTakeoffFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTakeoffFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.MEPCurve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The connector to be connected to the takeoff.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("mepcrv", "The duct or pipe which is the trunk for the takeoff.",typeof(Autodesk.Revit.DB.MEPCurve)));
			OutPortData.Add(new PortData("out","Add a new family instance of an takeoff fitting into the Autodesk Revit document,using one connector and one MEP curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewUnionFitting
	///</summary>
	[NodeName("API_Document_NewUnionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an union fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewUnionFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewUnionFitting
		///</summary>
		public API_Document_NewUnionFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewUnionFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the union.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the union.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","Add a new family instance of an union fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCrossFitting
	///</summary>
	[NodeName("API_Document_NewCrossFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a cross fitting into the Autodesk Revit document,using four connectors.")]
	public class API_Document_NewCrossFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCrossFitting
		///</summary>
		public API_Document_NewCrossFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCrossFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The fourth connector to be connected to the cross.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","Add a new family instance of a cross fitting into the Autodesk Revit document,using four connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTransitionFitting
	///</summary>
	[NodeName("API_Document_NewTransitionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an transition fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewTransitionFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTransitionFitting
		///</summary>
		public API_Document_NewTransitionFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTransitionFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the transition.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the transition.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","Add a new family instance of an transition fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTeeFitting
	///</summary>
	[NodeName("API_Document_NewTeeFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a tee fitting into the Autodesk Revit document,using three connectors.")]
	public class API_Document_NewTeeFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTeeFitting
		///</summary>
		public API_Document_NewTeeFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTeeFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the tee.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the tee.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The third connector to be connected to the tee. This should be connected to the branch of the tee.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","Add a new family instance of a tee fitting into the Autodesk Revit document,using three connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElbowFitting
	///</summary>
	[NodeName("API_Document_NewElbowFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an elbow fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewElbowFitting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElbowFitting
		///</summary>
		public API_Document_NewElbowFitting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElbowFitting", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the elbow.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the elbow.",typeof(Autodesk.Revit.DB.Connector)));
			OutPortData.Add(new PortData("out","Add a new family instance of an elbow fitting into the Autodesk Revit document,using two connectors.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using two connector, and flexible pipe type.")]
	public class API_Document_NewFlexPipe : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
		///</summary>
		public API_Document_NewFlexPipe()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexPipe", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using two connector, and flexible pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a connector, point array and pipe type.")]
	public class API_Document_NewFlexPipe_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
		///</summary>
		public API_Document_NewFlexPipe_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexPipe", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The connector to be connected to the flexible pipe, including the end points.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using a connector, point array and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a point array and pipe type.")]
	public class API_Document_NewFlexPipe_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
		///</summary>
		public API_Document_NewFlexPipe_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexPipe", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible pipe, including the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("fpt", "The type of the flexible pipe.",typeof(Autodesk.Revit.DB.Plumbing.FlexPipeType)));
			OutPortData.Add(new PortData("out","Adds a new flexible pipe into the document, using a point array and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document,  using two connectors and duct type.")]
	public class API_Document_NewPipe : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
		///</summary>
		public API_Document_NewPipe()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPipe", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Plumbing.PipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document,  using two connectors and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using a point, connector and pipe type.")]
	public class API_Document_NewPipe_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
		///</summary>
		public API_Document_NewPipe_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPipe", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Plumbing.PipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("con", "The connector to be connected to the pipe.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document, using a point, connector and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using two points and pipe type.")]
	public class API_Document_NewPipe_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
		///</summary>
		public API_Document_NewPipe_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPipe", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Plumbing.PipeType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The first point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second point of the pipe.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("pt", "The type of the pipe.",typeof(Autodesk.Revit.DB.Plumbing.PipeType)));
			OutPortData.Add(new PortData("out","Adds a new pipe into the document, using two points and pipe type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using two connector, and duct type.")]
	public class API_Document_NewFlexDuct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
		///</summary>
		public API_Document_NewFlexDuct()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexDuct", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using two connector, and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a connector, point array and duct type.")]
	public class API_Document_NewFlexDuct_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
		///</summary>
		public API_Document_NewFlexDuct_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexDuct", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The connector to be connected to the duct, including the end points.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using a connector, point array and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a point array and duct type.")]
	public class API_Document_NewFlexDuct_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
		///</summary>
		public API_Document_NewFlexDuct_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFlexDuct", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The point array indicating the path of the flexible duct, including the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("val", "The type of the flexible duct.",typeof(Autodesk.Revit.DB.Mechanical.FlexDuctType)));
			OutPortData.Add(new PortData("out","Adds a new flexible duct into the document, using a point array and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two connectors and duct type.")]
	public class API_Document_NewDuct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
		///</summary>
		public API_Document_NewDuct()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDuct", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Mechanical.DuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The first connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The second connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using two connectors and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using a point, connector and duct type.")]
	public class API_Document_NewDuct_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
		///</summary>
		public API_Document_NewDuct_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDuct", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Mechanical.DuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("con", "The connector to be connected to the duct.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using a point, connector and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two points and duct type.")]
	public class API_Document_NewDuct_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
		///</summary>
		public API_Document_NewDuct_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDuct", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Mechanical.DuctType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The first point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second point of the duct.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "The type of the duct.",typeof(Autodesk.Revit.DB.Mechanical.DuctType)));
			OutPortData.Add(new PortData("out","Adds a new duct into the document, using two points and duct type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a curve, type/symbol and reference level.")]
	public class API_Document_NewFamilyInstance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
		///</summary>
		public API_Document_NewFamilyInstance()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a curve, type/symbol and reference level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location,type/symbol and a base level.")]
	public class API_Document_NewFamilyInstance_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
		///</summary>
		public API_Document_NewFamilyInstance_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document, using a location,type/symbol and a base level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a base level.")]
	public class API_Document_NewFamilyInstance_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
		///</summary>
		public API_Document_NewFamilyInstance_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed on the specified level.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "A host object into which the instance will be embedded",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a base level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
	///</summary>
	[NodeName("API_Document_NewFascia")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference.")]
	public class API_Document_NewFascia : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
		///</summary>
		public API_Document_NewFascia()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFascia", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.FasciaType),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(Autodesk.Revit.DB.Architecture.FasciaType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the fascia.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a fascia along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
	///</summary>
	[NodeName("API_Document_NewFascia_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference array.")]
	public class API_Document_NewFascia_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
		///</summary>
		public API_Document_NewFascia_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFascia", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.FasciaType),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the fascia to create",typeof(Autodesk.Revit.DB.Architecture.FasciaType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the fascia.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Creates a fascia along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
	///</summary>
	[NodeName("API_Document_NewGutter")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference.")]
	public class API_Document_NewGutter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
		///</summary>
		public API_Document_NewGutter()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGutter", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.GutterType),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(Autodesk.Revit.DB.Architecture.GutterType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the gutter.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a gutter along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
	///</summary>
	[NodeName("API_Document_NewGutter_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference array.")]
	public class API_Document_NewGutter_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
		///</summary>
		public API_Document_NewGutter_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGutter", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.GutterType),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the gutter to create",typeof(Autodesk.Revit.DB.Architecture.GutterType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the gutter.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Creates a gutter along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
	///</summary>
	[NodeName("API_Document_NewSlabEdge")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference.")]
	public class API_Document_NewSlabEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
		///</summary>
		public API_Document_NewSlabEdge()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSlabEdge", false, new Type[]{typeof(Autodesk.Revit.DB.SlabEdgeType),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(Autodesk.Revit.DB.SlabEdgeType)));
			InPortData.Add(new PortData("ref", "A planar line or arc that represents the place where youwant to place the slab edge.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a slab edge along a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
	///</summary>
	[NodeName("API_Document_NewSlabEdge_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference array.")]
	public class API_Document_NewSlabEdge_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
		///</summary>
		public API_Document_NewSlabEdge_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSlabEdge", false, new Type[]{typeof(Autodesk.Revit.DB.SlabEdgeType),typeof(Autodesk.Revit.DB.ReferenceArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type of the slab edge to create",typeof(Autodesk.Revit.DB.SlabEdgeType)));
			InPortData.Add(new PortData("refa", "An array of planar lines and arcs that represents the place where youwant to place the slab edge.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			OutPortData.Add(new PortData("out","Creates a slab edge along a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem
	///</summary>
	[NodeName("API_Document_NewCurtainSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of faces.")]
	public class API_Document_NewCurtainSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem
		///</summary>
		public API_Document_NewCurtainSystem()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurtainSystem", false, new Type[]{typeof(Autodesk.Revit.DB.FaceArray),typeof(Autodesk.Revit.DB.CurtainSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The faces new CurtainSystem will be created on.",typeof(Autodesk.Revit.DB.FaceArray)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(Autodesk.Revit.DB.CurtainSystemType)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of faces.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem2
	///</summary>
	[NodeName("API_Document_NewCurtainSystem2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class API_Document_NewCurtainSystem2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem2
		///</summary>
		public API_Document_NewCurtainSystem2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurtainSystem2", false, new Type[]{typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.CurtainSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("refa", "The faces new CurtainSystem will be created on.",typeof(Autodesk.Revit.DB.ReferenceArray)));
			InPortData.Add(new PortData("val", "The Type of CurtainSystem to be created.",typeof(Autodesk.Revit.DB.CurtainSystemType)));
			OutPortData.Add(new PortData("out","Creates a new CurtainSystem element from a set of face references.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWire
	///</summary>
	[NodeName("API_Document_NewWire")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new wire element.")]
	public class API_Document_NewWire : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWire
		///</summary>
		public API_Document_NewWire()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWire", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Electrical.WireType),typeof(Autodesk.Revit.DB.Electrical.WiringType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The base line of the wire.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("v", "The view in which the wire is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("con", "The connector which connects with the start point connector of wire, if it is",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("con", "The connector which connects with the end point connector of wire, if it is",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Specify wire type of new created wire.",typeof(Autodesk.Revit.DB.Electrical.WireType)));
			InPortData.Add(new PortData("val", "Specify wiring type(Arc or chamfer) of new created wire.",typeof(Autodesk.Revit.DB.Electrical.WiringType)));
			OutPortData.Add(new PortData("out","Creates a new wire element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewZone
	///</summary>
	[NodeName("API_Document_NewZone")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Zone element.")]
	public class API_Document_NewZone : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewZone
		///</summary>
		public API_Document_NewZone()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewZone", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level on which the Zone is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The associative phase on which the Zone is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","Creates a new Zone element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomBoundaryLines
	///</summary>
	[NodeName("API_Document_NewRoomBoundaryLines")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Room border.")]
	public class API_Document_NewRoomBoundaryLines : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomBoundaryLines
		///</summary>
		public API_Document_NewRoomBoundaryLines()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoomBoundaryLines", false, new Type[]{typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("v", "The View for the new Room",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Room border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceBoundaryLines
	///</summary>
	[NodeName("API_Document_NewSpaceBoundaryLines")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Space border.")]
	public class API_Document_NewSpaceBoundaryLines : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceBoundaryLines
		///</summary>
		public API_Document_NewSpaceBoundaryLines()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaceBoundaryLines", false, new Type[]{typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("sp", "The sketch plan",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crvs", "The geometry curves on which the boundary lines are",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("v", "The View for the new Space",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Space border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceTag
	///</summary>
	[NodeName("API_Document_NewSpaceTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new SpaceTag.")]
	public class API_Document_NewSpaceTag : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceTag
		///</summary>
		public API_Document_NewSpaceTag()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaceTag", false, new Type[]{typeof(Autodesk.Revit.DB.Mechanical.Space),typeof(Autodesk.Revit.DB.UV),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Space which the tag refers.",typeof(Autodesk.Revit.DB.Mechanical.Space)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the space.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new SpaceTag.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces2
	///</summary>
	[NodeName("API_Document_NewSpaces2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new spaces on the available plan circuits of a the given level.")]
	public class API_Document_NewSpaces2_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces2
		///</summary>
		public API_Document_NewSpaces2_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaces2", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level on which the spaces is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase in which the spaces is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("v", "The view on which the space tags for the spaces are to display.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates new spaces on the available plan circuits of a the given level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level, at the given location, and assigned to the given phase.")]
	public class API_Document_NewSpace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
		///</summary>
		public API_Document_NewSpace()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpace", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a new space element on the given level, at the given location, and assigned to the given phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level at the given location.")]
	public class API_Document_NewSpace_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
		///</summary>
		public API_Document_NewSpace_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpace", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level on which the space is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a new space element on the given level at the given location.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unplaced space on a given phase.")]
	public class API_Document_NewSpace_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
		///</summary>
		public API_Document_NewSpace_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpace", false, new Type[]{typeof(Autodesk.Revit.DB.Phase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The phase in which the space is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","Creates a new unplaced space on a given phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipingSystem
	///</summary>
	[NodeName("API_Document_NewPipingSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP piping system element.")]
	public class API_Document_NewPipingSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipingSystem
		///</summary>
		public API_Document_NewPipingSystem()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPipingSystem", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.ConnectorSet),typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(Autodesk.Revit.DB.ConnectorSet)));
			InPortData.Add(new PortData("pst", "The System type.",typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)));
			OutPortData.Add(new PortData("out","Creates a new MEP piping system element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewMechanicalSystem
	///</summary>
	[NodeName("API_Document_NewMechanicalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP mechanical system element.")]
	public class API_Document_NewMechanicalSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewMechanicalSystem
		///</summary>
		public API_Document_NewMechanicalSystem()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewMechanicalSystem", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.ConnectorSet),typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "One connector within base equipment which is used to connect with the system. The base equipment is optional for the system, so this argument may be",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("val", "Connectors that will connect to the system.The owner elements of these connectors will be added into system as its elements.",typeof(Autodesk.Revit.DB.ConnectorSet)));
			InPortData.Add(new PortData("dst", "The system type.",typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)));
			OutPortData.Add(new PortData("out","Creates a new MEP mechanical system element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
	///</summary>
	[NodeName("API_Document_NewElectricalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from a set of electrical components.")]
	public class API_Document_NewElectricalSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
		///</summary>
		public API_Document_NewElectricalSystem()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElectricalSystem", false, new Type[]{typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from a set of electrical components.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
	///</summary>
	[NodeName("API_Document_NewElectricalSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from an unused Connector.")]
	public class API_Document_NewElectricalSystem_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
		///</summary>
		public API_Document_NewElectricalSystem_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElectricalSystem", false, new Type[]{typeof(Autodesk.Revit.DB.Connector),typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("con", "The Connector to create this Electrical System.",typeof(Autodesk.Revit.DB.Connector)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from an unused Connector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewExtrusionRoof
	///</summary>
	[NodeName("API_Document_NewExtrusionRoof")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Extrusion Roof.")]
	public class API_Document_NewExtrusionRoof : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewExtrusionRoof
		///</summary>
		public API_Document_NewExtrusionRoof()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewExtrusionRoof", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.ReferencePlane),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.RoofType),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The profile of the extrusion roof.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "The work plane for the extrusion roof.",typeof(Autodesk.Revit.DB.ReferencePlane)));
			InPortData.Add(new PortData("l", "The level of the extrusion roof.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "Type of the extrusion roof.",typeof(Autodesk.Revit.DB.RoofType)));
			InPortData.Add(new PortData("n", "Start the extrusion.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "End the extrusion.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new Extrusion Roof.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTruss
	///</summary>
	[NodeName("API_Document_NewTruss")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a New Truss.")]
	public class API_Document_NewTruss : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTruss
		///</summary>
		public API_Document_NewTruss()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTruss", false, new Type[]{typeof(Autodesk.Revit.DB.Structure.TrussType),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.Curve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The type for truss.",typeof(Autodesk.Revit.DB.Structure.TrussType)));
			InPortData.Add(new PortData("sp", "The sketch plane where the truss is going to reside. It could be",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crv", "The curve that represents truss's base curve.It must be a line, must not be a vertical line, and must be within the sketch plane if sketchPlane is valid.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Creates a New Truss.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreas
	///</summary>
	[NodeName("API_Document_NewAreas")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new areas")]
	public class API_Document_NewAreas : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreas
		///</summary>
		public API_Document_NewAreas()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreas", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.AreaCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of AreaCreationData which wraps the creation arguments of the areas to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.AreaCreationData>)));
			OutPortData.Add(new PortData("out","Creates new areas",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewArea
	///</summary>
	[NodeName("API_Document_NewArea")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new area")]
	public class API_Document_NewArea : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewArea
		///</summary>
		public API_Document_NewArea()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArea", false, new Type[]{typeof(Autodesk.Revit.DB.ViewPlan),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view of area element.",typeof(Autodesk.Revit.DB.ViewPlan)));
			InPortData.Add(new PortData("uv", "The point which lies in the enclosed region of AreaBoundaryLines to put the new created Area",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a new area",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryLine
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryLine")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Area border.")]
	public class API_Document_NewAreaBoundaryLine : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryLine
		///</summary>
		public API_Document_NewAreaBoundaryLine()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaBoundaryLine", false, new Type[]{typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.ViewPlan)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("sp", "The sketch plane.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("crv", "The geometry curve on which the boundary line are",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("v", "The View for the new Area",typeof(Autodesk.Revit.DB.ViewPlan)));
			OutPortData.Add(new PortData("out","Creates a new boundary line as an Area border.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationWall
	///</summary>
	[NodeName("API_Document_NewFoundationWall")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new continuous footing object.")]
	public class API_Document_NewFoundationWall : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationWall
		///</summary>
		public API_Document_NewFoundationWall()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFoundationWall", false, new Type[]{typeof(Autodesk.Revit.DB.ContFootingType),typeof(Autodesk.Revit.DB.Wall)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The ContFooting type.",typeof(Autodesk.Revit.DB.ContFootingType)));
			InPortData.Add(new PortData("val", "The Wall to append a ContFooting.",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Creates a new continuous footing object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlab
	///</summary>
	[NodeName("API_Document_NewSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab within the project with the given horizontal profile using the default floor style.")]
	public class API_Document_NewSlab : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlab
		///</summary>
		public API_Document_NewSlab()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSlab", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Line),typeof(System.Double),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the slab.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the slab is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("crv", "A line use to control the sloped angle of the slab. It should be in the same face with profile.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The slope.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a slab within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTag
	///</summary>
	[NodeName("API_Document_NewTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new IndependentTag Element.")]
	public class API_Document_NewTag : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTag
		///</summary>
		public API_Document_NewTag()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTag", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Element),typeof(System.Boolean),typeof(Autodesk.Revit.DB.TagMode),typeof(Autodesk.Revit.DB.TagOrientation),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the tag is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("el", "The host object of tag.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("b", "Whether there will be a leader.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The mode of the tag. Add by Category, add by Multi-Category, or add by material.",typeof(Autodesk.Revit.DB.TagMode)));
			InPortData.Add(new PortData("val", "The orientation of the tag.",typeof(Autodesk.Revit.DB.TagOrientation)));
			InPortData.Add(new PortData("xyz", "The position of the tag.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new IndependentTag Element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new opening in a roof, floor and ceiling.")]
	public class API_Document_NewOpening : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
		///</summary>
		public API_Document_NewOpening()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewOpening", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "Host element of the opening. Can be a roof, floor, or ceiling.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "Profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "True if the profile is cut perpendicular to the intersecting face of the host. False if the profile is cut vertically.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new opening in a roof, floor and ceiling.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a rectangular opening on a wall.")]
	public class API_Document_NewOpening_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
		///</summary>
		public API_Document_NewOpening_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewOpening", false, new Type[]{typeof(Autodesk.Revit.DB.Wall),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Host element of the opening.",typeof(Autodesk.Revit.DB.Wall)));
			InPortData.Add(new PortData("xyz", "One corner of the rectangle.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The opposite corner of the rectangle.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a rectangular opening on a wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new shaft opening between a set of levels.")]
	public class API_Document_NewOpening_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
		///</summary>
		public API_Document_NewOpening_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewOpening", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.CurveArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "bottom level",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("l", "top level",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","Creates a new shaft opening between a set of levels.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new opening in a beam, brace and column.")]
	public class API_Document_NewOpening_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
		///</summary>
		public API_Document_NewOpening_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewOpening", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.Creation.eRefFace)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "host element of the opening, can be a beam, brace and column.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "profile of the opening.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "face on which opening is based on.",typeof(Autodesk.Revit.Creation.eRefFace)));
			OutPortData.Add(new PortData("out","Creates a new opening in a beam, brace and column.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Area BoundaryConditions element on a host element.")]
	public class API_Document_NewAreaBoundaryConditions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
		///</summary>
		public API_Document_NewAreaBoundaryConditions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaBoundaryConditions", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "A Wall, Slab or Slab Foundation to host the boundary conditions.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new Area BoundaryConditions element on a host element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewLineBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Line BoundaryConditions element on a host element.")]
	public class API_Document_NewLineBoundaryConditions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
		///</summary>
		public API_Document_NewLineBoundaryConditions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineBoundaryConditions", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "A Beam.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new Line BoundaryConditions element on a host element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Area BoundaryConditions element on a reference.")]
	public class API_Document_NewAreaBoundaryConditions_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
		///</summary>
		public API_Document_NewAreaBoundaryConditions_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaBoundaryConditions", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The Geometry reference obtained from a Wall, Slab or Slab Foundation.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new Area BoundaryConditions element on a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewLineBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Line BoundaryConditions element on a reference.")]
	public class API_Document_NewLineBoundaryConditions_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
		///</summary>
		public API_Document_NewLineBoundaryConditions_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineBoundaryConditions", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The Geometry reference to a Beam's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical line.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("val", "A value indicating the X axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for X axis. Ignored if X_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Y axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Y axis. Ignored if Y_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the Z axis translation option.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Translation Spring Modulus for Z axis. Ignored if Z_Translation is not \"Spring\".",typeof(System.Double)));
			InPortData.Add(new PortData("val", "A value indicating the option for rotation about the X axis.",typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue)));
			InPortData.Add(new PortData("n", "Rotation Spring Modulus for X axis. Ignored if X_Rotation is not \"Spring\"",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new Line BoundaryConditions element on a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewPointBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Point BoundaryConditions Element.")]
	public class API_Document_NewPointBoundaryConditions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointBoundaryConditions
		///</summary>
		public API_Document_NewPointBoundaryConditions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointBoundaryConditions", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double),typeof(Autodesk.Revit.DB.Structure.TranslationRotationValue),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
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
			OutPortData.Add(new PortData("out","Creates a new Point BoundaryConditions Element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
		///</summary>
		public API_Document_NewBeamSystem()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBeamSystem", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the sketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem. This argument is optional – may be null.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Whether the BeamSystem is 3D or not",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new 2D BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
		///</summary>
		public API_Document_NewBeamSystem_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBeamSystem", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.Level)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the level.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the sketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Creates a new 2D BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
		///</summary>
		public API_Document_NewBeamSystem_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBeamSystem", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The profile of the BeamSystem. The profile must be a closed curve loop in the sketch plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "If the BeamSystem is 3D, the sketchPlane must be a level, oran exception will be thrown.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
		///</summary>
		public API_Document_NewBeamSystem_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBeamSystem", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The profile is the profile of the BeamSystem.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "The work plane of the BeamSystem.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new BeamSystem with specified profile curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
	///</summary>
	[NodeName("API_Document_NewRoomTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new RoomTag referencing a room in the host model or in a Revit link.")]
	public class API_Document_NewRoomTag : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
		///</summary>
		public API_Document_NewRoomTag()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoomTag", false, new Type[]{typeof(Autodesk.Revit.DB.LinkElementId),typeof(Autodesk.Revit.DB.UV),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The HostOrLinkElementId of the Room.",typeof(Autodesk.Revit.DB.LinkElementId)));
			InPortData.Add(new PortData("uv", "A 2D point that defines the tag location on the level of the room.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("val", "The id of the view where the tag will be shown. If",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Creates a new RoomTag referencing a room in the host model or in a Revit link.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
	///</summary>
	[NodeName("API_Document_NewRoomTag_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new RoomTag.")]
	public class API_Document_NewRoomTag_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
		///</summary>
		public API_Document_NewRoomTag_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoomTag", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.Room),typeof(Autodesk.Revit.DB.UV),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Room which the tag refers.",typeof(Autodesk.Revit.DB.Architecture.Room)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location on the level of the room.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("v", "The view where the tag will lie.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Creates a new RoomTag.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new unplaced rooms in the given phase.")]
	public class API_Document_NewRooms2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
		///</summary>
		public API_Document_NewRooms2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms2", false, new Type[]{typeof(Autodesk.Revit.DB.Phase),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The phase on which the rooms are to exist.",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("i", "The number of the rooms to be created.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Creates new unplaced rooms in the given phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the given phase.")]
	public class API_Document_NewRooms2_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
		///</summary>
		public API_Document_NewRooms2_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms2", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The phase on which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","Creates new rooms in each plan circuit found in the given level in the given phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the last phase.")]
	public class API_Document_NewRooms2_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
		///</summary>
		public API_Document_NewRooms2_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms2", false, new Type[]{typeof(Autodesk.Revit.DB.Level)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level from which the circuits are found.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Creates new rooms in each plan circuit found in the given level in the last phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new room within the confines of a plan circuit, or places an unplaced room within the confines of the plan circuit.")]
	public class API_Document_NewRoom : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
		///</summary>
		public API_Document_NewRoom()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoom", false, new Type[]{typeof(Autodesk.Revit.DB.Architecture.Room),typeof(Autodesk.Revit.DB.PlanCircuit)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The room which you want to locate in the circuit.  Pass",typeof(Autodesk.Revit.DB.Architecture.Room)));
			InPortData.Add(new PortData("val", "The circuit in which you want to locate a room.",typeof(Autodesk.Revit.DB.PlanCircuit)));
			OutPortData.Add(new PortData("out","Creates a new room within the confines of a plan circuit, or places an unplaced room within the confines of the plan circuit.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unplaced room and with an assigned phase.")]
	public class API_Document_NewRoom_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
		///</summary>
		public API_Document_NewRoom_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoom", false, new Type[]{typeof(Autodesk.Revit.DB.Phase)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The phase in which the room is to exist.",typeof(Autodesk.Revit.DB.Phase)));
			OutPortData.Add(new PortData("out","Creates a new unplaced room and with an assigned phase.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new room on a level at a specified point.")]
	public class API_Document_NewRoom_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
		///</summary>
		public API_Document_NewRoom_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoom", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("l", "The level on which the room is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("uv", "A 2D point that dictates the location of the room on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a new room on a level at a specified point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrids
	///</summary>
	[NodeName("API_Document_NewGrids")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new grid lines.")]
	public class API_Document_NewGrids : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrids
		///</summary>
		public API_Document_NewGrids()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGrids", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The curves which represent the new grid lines.  These curves must be lines or bounded arcs.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","Creates new grid lines.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
	///</summary>
	[NodeName("API_Document_NewGrid")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new radial grid line.")]
	public class API_Document_NewGrid : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
		///</summary>
		public API_Document_NewGrid()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGrid", false, new Type[]{typeof(Autodesk.Revit.DB.Arc)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("arc", "An arc object that represents the location of the new grid line.",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Creates a new radial grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
	///</summary>
	[NodeName("API_Document_NewGrid_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear grid line.")]
	public class API_Document_NewGrid_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
		///</summary>
		public API_Document_NewGrid_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGrid", false, new Type[]{typeof(Autodesk.Revit.DB.Line)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "A line object which represents the location of the grid line.",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Creates a new linear grid line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewSheet
	///</summary>
	[NodeName("API_Document_NewViewSheet")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sheet view.")]
	public class API_Document_NewViewSheet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewSheet
		///</summary>
		public API_Document_NewViewSheet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewViewSheet", false, new Type[]{typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("fs", "The titleblock family symbol to apply to this sheet.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Creates a new sheet view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewDrafting
	///</summary>
	[NodeName("API_Document_NewViewDrafting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new drafting view.")]
	public class API_Document_NewViewDrafting : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewDrafting
		///</summary>
		public API_Document_NewViewDrafting()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewViewDrafting", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new drafting view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationSlab
	///</summary>
	[NodeName("API_Document_NewFoundationSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level.")]
	public class API_Document_NewFoundationSlab : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationSlab
		///</summary>
		public API_Document_NewFoundationSlab()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFoundationSlab", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.FloorType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector.")]
	public class API_Document_NewFloor : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
		///</summary>
		public API_Document_NewFloor()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFloor", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.FloorType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the floor is consideredto be upper and down.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile and floor style on the specified level.")]
	public class API_Document_NewFloor_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
		///</summary>
		public API_Document_NewFloor_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFloor", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.FloorType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "A floor type to be used by the new floor instead of the default type.",typeof(Autodesk.Revit.DB.FloorType)));
			InPortData.Add(new PortData("l", "The level on which the floor is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a floor within the project with the given horizontal profile and floor style on the specified level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile using the default floor style.")]
	public class API_Document_NewFloor_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
		///</summary>
		public API_Document_NewFloor_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFloor", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the horizontal profile of the floor.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "If set, specifies that the floor is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a floor within the project with the given horizontal profile using the default floor style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotElevation
	///</summary>
	[NodeName("API_Document_NewSpotElevation")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new spot elevation object within the project.")]
	public class API_Document_NewSpotElevation : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotElevation
		///</summary>
		public API_Document_NewSpotElevation()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpotElevation", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the spot elevation is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The reference to which the spot elevation is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point which the spot elevation evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot elevation.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point for the spot elevation.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot elevation evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Generate a new spot elevation object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotCoordinate
	///</summary>
	[NodeName("API_Document_NewSpotCoordinate")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new spot coordinate object within the project.")]
	public class API_Document_NewSpotCoordinate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotCoordinate
		///</summary>
		public API_Document_NewSpotCoordinate()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpotCoordinate", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in which the spot coordinate is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("ref", "The reference to which the spot coordinate is to be bound.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The point which the spot coordinate evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The bend point for the spot coordinate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point for the spot coordinate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The actual point on the reference which the spot coordinate evaluate.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Indicate if it has leader or not.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Generate a new spot coordinate object within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCombination
	///</summary>
	[NodeName("API_Document_NewLoadCombination")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination element     within the project.")]
	public class API_Document_NewLoadCombination : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCombination
		///</summary>
		public API_Document_NewLoadCombination()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadCombination", false, new Type[]{typeof(System.String),typeof(System.Int32),typeof(System.Int32),typeof(System.Double[]),typeof(Autodesk.Revit.DB.Structure.LoadCaseArray),typeof(Autodesk.Revit.DB.Structure.LoadCombinationArray),typeof(Autodesk.Revit.DB.Structure.LoadUsageArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The not empty name for the Load Combination Element to create.",typeof(System.String)));
			InPortData.Add(new PortData("i", "LoadCombination Type Index: 0-Combination, 1-Envelope.",typeof(System.Int32)));
			InPortData.Add(new PortData("i", "LoadCombination State Index: 0-Servicebility, 1-Ultimate.",typeof(System.Int32)));
			InPortData.Add(new PortData("val", "Factors array for Load Combination formula.",typeof(System.Double[])));
			InPortData.Add(new PortData("val", "Load Cases array for Load Combination formula.",typeof(Autodesk.Revit.DB.Structure.LoadCaseArray)));
			InPortData.Add(new PortData("val", "Load Combinations array for Load Combination formula.",typeof(Autodesk.Revit.DB.Structure.LoadCombinationArray)));
			InPortData.Add(new PortData("val", "Load Usages array.",typeof(Autodesk.Revit.DB.Structure.LoadUsageArray)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCombination element     within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCase
	///</summary>
	[NodeName("API_Document_NewLoadCase")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase element within the project.")]
	public class API_Document_NewLoadCase : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCase
		///</summary>
		public API_Document_NewLoadCase()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadCase", false, new Type[]{typeof(System.String),typeof(Autodesk.Revit.DB.Structure.LoadNature),typeof(Autodesk.Revit.DB.Category)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The not empty name for the Load Case Element to create.",typeof(System.String)));
			InPortData.Add(new PortData("val", "The Load Case nature.",typeof(Autodesk.Revit.DB.Structure.LoadNature)));
			InPortData.Add(new PortData("val", "The Load Case category.",typeof(Autodesk.Revit.DB.Category)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCase element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadUsage
	///</summary>
	[NodeName("API_Document_NewLoadUsage")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage element within the project.")]
	public class API_Document_NewLoadUsage : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadUsage
		///</summary>
		public API_Document_NewLoadUsage()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadUsage", false, new Type[]{typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The not empty name for the Load Usage Element to create.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadUsage element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadNature
	///</summary>
	[NodeName("API_Document_NewLoadNature")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadNature element within the project.")]
	public class API_Document_NewLoadNature : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadNature
		///</summary>
		public API_Document_NewLoadNature()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadNature", false, new Type[]{typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The name for the Load Nature Element to create.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadNature element within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform hosted area load with polygonal shape within the project.")]
	public class API_Document_NewAreaLoad : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
		///</summary>
		public API_Document_NewAreaLoad()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaLoad", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.AreaLoadType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "The host element (Floor or Wall) of the AreaLoad application.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","Creates a new uniform hosted area load with polygonal shape within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class API_Document_NewAreaLoad_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
		///</summary>
		public API_Document_NewAreaLoad_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaLoad", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Int32[]),typeof(System.Int32[]),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.AreaLoadType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(System.Int32[])));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(System.Int32[])));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the first reference point.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the second reference point. Ignored if only one or two reference points are supplied.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d area force applied to the third reference point.  Ignored if only one or two reference points are supplied.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class API_Document_NewAreaLoad_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
		///</summary>
		public API_Document_NewAreaLoad_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaLoad", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Int32[]),typeof(System.Int32[]),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.AreaLoadType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of curves that define the shape of the area load curves.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("val", "The indices of the curves in curvesArr that will be used to define the reference points for the load.",typeof(System.Int32[])));
			InPortData.Add(new PortData("val", "Identifies which of the curve end points should be used for the reference points, for each member of refPntIdxs.  The value should be 0 for the start point or 1 for the end point of the curve.",typeof(System.Int32[])));
			InPortData.Add(new PortData("lst", "The 3d area forces applied to each of the reference points in the refPntIdxs array.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","Creates a new unhosted area load with variable forces at the vertices within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform unhosted area load with polygonal shape within the project.")]
	public class API_Document_NewAreaLoad_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
		///</summary>
		public API_Document_NewAreaLoad_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaLoad", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.AreaLoadType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "Vertexes of AreaLoad shape polygon.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("xyz", "The applied 3d area force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the AreaLoad.",typeof(Autodesk.Revit.DB.Structure.AreaLoadType)));
			OutPortData.Add(new PortData("out","Creates a new uniform unhosted area load with polygonal shape within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at an array of points.")]
	public class API_Document_NewLineLoad : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
		///</summary>
		public API_Document_NewLineLoad()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineLoad", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(System.Boolean),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.LineLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, Wall's, Wall Foundation's, Slab's or Slab Foundation's analytical lines.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new hosted line load within the project using data at an array of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at two points.")]
	public class API_Document_NewLineLoad_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
		///</summary>
		public API_Document_NewLineLoad_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineLoad", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(System.Boolean),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.LineLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "The host element (Beam, Brace or Column) of the LineLoad application.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new hosted line load within the project using data at two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at an array of points.")]
	public class API_Document_NewLineLoad_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
		///</summary>
		public API_Document_NewLineLoad_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineLoad", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(System.Boolean),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.LineLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The end points of the LineLoad application.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear forces in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The applied 3d linear moments in the end points.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the uniform load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Set to True if you wish to create the projected load.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the LineLoad.",typeof(Autodesk.Revit.DB.Structure.LineLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the LineLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new unhosted line load within the project using data at an array of points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at two points.")]
	public class API_Document_NewLineLoad_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
		///</summary>
		public API_Document_NewLineLoad_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineLoad", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(System.Boolean),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.LineLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
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
			OutPortData.Add(new PortData("out","Creates a new unhosted line load within the project using data at two points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
	///</summary>
	[NodeName("API_Document_NewPointLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted point load within the project.")]
	public class API_Document_NewPointLoad : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
		///</summary>
		public API_Document_NewPointLoad()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointLoad", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.PointLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The Geometry reference to Beam's, Brace's, Column's, analytical line's end.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(Autodesk.Revit.DB.Structure.PointLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new hosted point load within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
	///</summary>
	[NodeName("API_Document_NewPointLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted point load within the project.")]
	public class API_Document_NewPointLoad_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
		///</summary>
		public API_Document_NewPointLoad_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointLoad", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean),typeof(Autodesk.Revit.DB.Structure.PointLoadType),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The point of the PointLoad application.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d force.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The 3d moment.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Specifies if the load is a reaction load. The load cannot be modified if isReaction=True.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The symbol of the PointLoad.",typeof(Autodesk.Revit.DB.Structure.PointLoadType)));
			InPortData.Add(new PortData("sp", "Indicate the work plane of the PointLoad.",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates a new unhosted point load within the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPathReinforcement
	///</summary>
	[NodeName("API_Document_NewPathReinforcement")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a Path Reinforcement element within the project")]
	public class API_Document_NewPathReinforcement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPathReinforcement
		///</summary>
		public API_Document_NewPathReinforcement()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPathReinforcement", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "The element to which the Path Reinforcement belongs. The element must be a structural floor or wall.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "An array of curves forming a chain.  Bars will be placed orthogonal to the chain with their hook ends near the chain, offset by the side cover setting.  The curves must belong to the top face of the floor or the exterior face of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "A flag controlling the bars relative to the curves. If the curves are given in order and with consistent orientation, the bars will lie to the right of the chain if flip=false, to the left if flip=true, when viewed from above the floor or outside the wall.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new instance of a Path Reinforcement element within the project",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRebarBarType
	///</summary>
	[NodeName("API_Document_NewRebarBarType")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of Rebar Bar Type, which defines the bar diameter, bar bend diameter and bar material of the rebar.")]
	public class API_Document_NewRebarBarType : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRebarBarType
		///</summary>
		public API_Document_NewRebarBarType()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRebarBarType", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of Rebar Bar Type, which defines the bar diameter, bar bend diameter and bar material of the rebar.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.HermiteSpline.Create
	///</summary>
	[NodeName("API_HermiteSpline_Create")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with specified tangency at its endpoints.")]
	public class API_HermiteSpline_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.HermiteSpline.Create
		///</summary>
		public API_HermiteSpline_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.HermiteSpline);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(Autodesk.Revit.DB.HermiteSplineTangents)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic, false otherwise.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The object which indicates tangency at the start, the end, or both ends of the curve.",typeof(Autodesk.Revit.DB.HermiteSplineTangents)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with specified tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.HermiteSpline.Create
	///</summary>
	[NodeName("API_HermiteSpline_Create_1")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with default tangency at its endpoints.")]
	public class API_HermiteSpline_Create_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.HermiteSpline.Create
		///</summary>
		public API_HermiteSpline_Create_1()
		{
			base_type = typeof(Autodesk.Revit.DB.HermiteSpline);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic, false otherwise.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with default tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteSpline_Parameters")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the params of the Hermite spline.")]
	public class API_HermiteSpline_Parameters : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteSpline_Parameters()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the params of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = arg0.Parameters;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_HermiteSpline_Tangents")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the tangents of the Hermite spline.")]
	public class API_HermiteSpline_Tangents : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteSpline_Tangents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the tangents of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = arg0.Tangents;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_HermiteSpline_ControlPoints")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The control points of the Hermite spline.")]
	public class API_HermiteSpline_ControlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteSpline_ControlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","The control points of the Hermite spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = arg0.ControlPoints;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_HermiteSpline_IsPeriodic")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns whether the Hermite spline is periodic or not.")]
	public class API_HermiteSpline_IsPeriodic : dynRevitTransactionNodeWithOneOutput
	{
		public API_HermiteSpline_IsPeriodic()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns whether the Hermite spline is periodic or not.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.HermiteSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteSpline));
			var result = arg0.IsPeriodic;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Blend.GetVertexConnectionMap
	///</summary>
	[NodeName("API_Blend_GetVertexConnectionMap")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the mapping between the vertices in the top and bottom profiles.")]
	public class API_Blend_GetVertexConnectionMap : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Blend.GetVertexConnectionMap
		///</summary>
		public API_Blend_GetVertexConnectionMap()
		{
			base_type = typeof(Autodesk.Revit.DB.Blend);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetVertexConnectionMap", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the mapping between the vertices in the top and bottom profiles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_TopProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class API_Blend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.TopProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Blend_BottomProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class API_Blend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.BottomProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Blend_TopOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the top end of the blend relative to the sketch plane.")]
	public class API_Blend_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_TopOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the top end of the blend relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.TopOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Blend_BottomOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the bottom end of the blend relative to the sketch plane.")]
	public class API_Blend_BottomOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_BottomOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the bottom end of the blend relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.BottomOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Blend_BottomSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Bottom Sketch of the Blend.")]
	public class API_Blend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Bottom Sketch of the Blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.BottomSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Blend_TopSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Top Sketch of the Blend.")]
	public class API_Blend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Blend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Top Sketch of the Blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Blend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Blend));
			var result = arg0.TopSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_MaxSegmentAngle")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The maximum segment angle of the sweep in radians.")]
	public class API_Sweep_MaxSegmentAngle : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_MaxSegmentAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The maximum segment angle of the sweep in radians.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.MaxSegmentAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_IsTrajectorySegmentationEnabled")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The trajectory segmentation option for the sweep.")]
	public class API_Sweep_IsTrajectorySegmentationEnabled : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_IsTrajectorySegmentationEnabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The trajectory segmentation option for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.IsTrajectorySegmentationEnabled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_Path3d")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The selected curves used for the sweep path.")]
	public class API_Sweep_Path3d : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_Path3d()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The selected curves used for the sweep path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.Path3d;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_PathSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The sketched path for the sweep.")]
	public class API_Sweep_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The sketched path for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.PathSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_ProfileSymbol")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The family symbol profile details for the sweep.")]
	public class API_Sweep_ProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_ProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The family symbol profile details for the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.ProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Sweep_ProfileSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The profile sketch of the sweep.")]
	public class API_Sweep_ProfileSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Sweep_ProfileSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The profile sketch of the sweep.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Sweep)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Sweep));
			var result = arg0.ProfileSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.SetInstanceFlipped
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_SetInstanceFlipped")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the value of the flip parameter on the adaptive instance.")]
	public class API_AdaptiveComponentInstanceUtils_SetInstanceFlipped : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.SetInstanceFlipped
		///</summary>
		public API_AdaptiveComponentInstanceUtils_SetInstanceFlipped()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetInstanceFlipped", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			InPortData.Add(new PortData("b", "The flip flag",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Sets the value of the flip parameter on the adaptive instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsInstanceFlipped
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsInstanceFlipped")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the value of the flip parameter on the adaptive instance.")]
	public class API_AdaptiveComponentInstanceUtils_IsInstanceFlipped : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsInstanceFlipped
		///</summary>
		public API_AdaptiveComponentInstanceUtils_IsInstanceFlipped()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsInstanceFlipped", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Gets the value of the flip parameter on the adaptive instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds
		///</summary>
		public API_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInstanceShapeHandlePointElementRefIds", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Gets Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Placement Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds
		///</summary>
		public API_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInstancePlacementPointElementRefIds", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Gets Placement Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds
		///</summary>
		public API_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInstancePointElementRefIds", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance.",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Gets Adaptive Point Element Ref ids to which the instance geometry adapts.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.MoveAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_MoveAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves Adaptive Component Instance by the specified transformation.")]
	public class API_AdaptiveComponentInstanceUtils_MoveAdaptiveComponentInstance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.MoveAdaptiveComponentInstance
		///</summary>
		public API_AdaptiveComponentInstanceUtils_MoveAdaptiveComponentInstance()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MoveAdaptiveComponentInstance", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance),typeof(Autodesk.Revit.DB.Transform),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			InPortData.Add(new PortData("val", "The Transformation",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("b", "True if the move should disassociate the Point Element Refs from their hosts.   False if the Point Element Refs remain hosted.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Moves Adaptive Component Instance by the specified transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a FamilyInstance of Adaptive Component Family.")]
	public class API_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance
		///</summary>
		public API_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateAdaptiveComponentInstance", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The Document",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Creates a FamilyInstance of Adaptive Component Family.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilyInstance is an Adaptive Component Instance.")]
	public class API_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance
		///</summary>
		public API_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptiveComponentInstance", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Verifies if a FamilyInstance is an Adaptive Component Instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilyInstance has an Adaptive Family Symbol.")]
	public class API_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol
		///</summary>
		public API_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "HasAdaptiveFamilySymbol", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Verifies if a FamilyInstance has an Adaptive Family Symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilySymbol is a valid Adaptive Family Symbol.")]
	public class API_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol
		///</summary>
		public API_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol()
		{
			base_type = typeof(Autodesk.Revit.DB.AdaptiveComponentInstanceUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAdaptiveFamilySymbol", false, new Type[]{typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AdaptiveComponentInstanceUtils", typeof(object)));
			}
			InPortData.Add(new PortData("fs", "The FamilySymbol",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Verifies if a FamilySymbol is a valid Adaptive Family Symbol.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Clone
	///</summary>
	[NodeName("API_Curve_Clone")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this curve.")]
	public class API_Curve_Clone : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Clone
		///</summary>
		public API_Curve_Clone()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Clone", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a copy of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Project
	///</summary>
	[NodeName("API_Curve_Project")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this curve.")]
	public class API_Curve_Project : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Project
		///</summary>
		public API_Curve_Project()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Project", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Projects the specified point on this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
	///</summary>
	[NodeName("API_Curve_Intersect")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve and returns the intersection results.")]
	public class API_Curve_Intersect : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
		///</summary>
		public API_Curve_Intersect()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.IntersectionResultArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(Autodesk.Revit.DB.IntersectionResultArray)));
			OutPortData.Add(new PortData("out","Calculates the intersection of this curve with the specified curve and returns the intersection results.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
	///</summary>
	[NodeName("API_Curve_Intersect_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve.")]
	public class API_Curve_Intersect_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
		///</summary>
		public API_Curve_Intersect_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Curve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this curve.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Calculates the intersection of this curve with the specified curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
	///</summary>
	[NodeName("API_Curve_IsInside")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds and outputs the end index.")]
	public class API_Curve_IsInside : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
		///</summary>
		public API_Curve_IsInside()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsInside", false, new Type[]{typeof(System.Double),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(System.Double)));
			InPortData.Add(new PortData("val", "The end index is equal to 0 for the start point, 1 for the end point, or -1 if the parameter is not at the end.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Indicates whether the specified parameter value is within this curve's bounds and outputs the end index.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
	///</summary>
	[NodeName("API_Curve_IsInside_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds.")]
	public class API_Curve_IsInside_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
		///</summary>
		public API_Curve_IsInside_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsInside", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The raw curve parameter to be evaluated.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Indicates whether the specified parameter value is within this curve's bounds.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeDerivatives
	///</summary>
	[NodeName("API_Curve_ComputeDerivatives")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the curve at the specified parameter.")]
	public class API_Curve_ComputeDerivatives : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeDerivatives
		///</summary>
		public API_Curve_ComputeDerivatives()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeDerivatives", false, new Type[]{typeof(System.Double),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Indicates that the specified parameter is normalized.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Returns the vectors describing the curve at the specified parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.CreateTransformed
	///</summary>
	[NodeName("API_Curve_CreateTransformed")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Crates a new instance of a curve as a transformation of this curve.")]
	public class API_Curve_CreateTransformed : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.CreateTransformed
		///</summary>
		public API_Curve_CreateTransformed()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateTransformed", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The transform to apply.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Crates a new instance of a curve as a transformation of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Distance
	///</summary>
	[NodeName("API_Curve_Distance")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the shortest distance from the specified point to this curve.")]
	public class API_Curve_Distance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Distance
		///</summary>
		public API_Curve_Distance()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Distance", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The specified point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Returns the shortest distance from the specified point to this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeRawParameter
	///</summary>
	[NodeName("API_Curve_ComputeRawParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the raw parameter from the normalized parameter.")]
	public class API_Curve_ComputeRawParameter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeRawParameter
		///</summary>
		public API_Curve_ComputeRawParameter()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeRawParameter", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The normalized parameter.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Computes the raw parameter from the normalized parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeNormalizedParameter
	///</summary>
	[NodeName("API_Curve_ComputeNormalizedParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the normalized curve parameter from the raw parameter.")]
	public class API_Curve_ComputeNormalizedParameter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeNormalizedParameter
		///</summary>
		public API_Curve_ComputeNormalizedParameter()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeNormalizedParameter", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The raw parameter.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Computes the normalized curve parameter from the raw parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeUnbound
	///</summary>
	[NodeName("API_Curve_MakeUnbound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Makes this curve unbound.")]
	public class API_Curve_MakeUnbound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeUnbound
		///</summary>
		public API_Curve_MakeUnbound()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MakeUnbound", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Makes this curve unbound.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeBound
	///</summary>
	[NodeName("API_Curve_MakeBound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Changes the bounds of this curve to the specified values.")]
	public class API_Curve_MakeBound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeBound
		///</summary>
		public API_Curve_MakeBound()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MakeBound", false, new Type[]{typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The new parameter of the start point.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The new parameter of the end point.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Changes the bounds of this curve to the specified values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndParameter
	///</summary>
	[NodeName("API_Curve_GetEndParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the raw parameter value at the start or end of this curve.")]
	public class API_Curve_GetEndParameter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndParameter
		///</summary>
		public API_Curve_GetEndParameter()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetEndParameter", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("i", "0 for the start or 1 for end of the curve.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Returns the raw parameter value at the start or end of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndPointReference
	///</summary>
	[NodeName("API_Curve_GetEndPointReference")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the start point or the end point of the curve.")]
	public class API_Curve_GetEndPointReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndPointReference
		///</summary>
		public API_Curve_GetEndPointReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetEndPointReference", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Use 0 for the start point; 1 for the end point.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the start point or the end point of the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndPoint
	///</summary>
	[NodeName("API_Curve_GetEndPoint")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the 3D point at the start or end of this curve.")]
	public class API_Curve_GetEndPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.GetEndPoint
		///</summary>
		public API_Curve_GetEndPoint()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetEndPoint", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("i", "0 for the start or 1 for end of the curve.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Returns the 3D point at the start or end of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Evaluate
	///</summary>
	[NodeName("API_Curve_Evaluate")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the curve.")]
	public class API_Curve_Evaluate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Evaluate
		///</summary>
		public API_Curve_Evaluate()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Evaluate", false, new Type[]{typeof(System.Double),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "If false, param is interpreted as natural parameterization of the curve. If true, param is expected to be in [0,1] interval mapped to the bounds of the curve. Setting to true is valid only if the curve is bound.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Evaluates a parameter on the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Tessellate
	///</summary>
	[NodeName("API_Curve_Tessellate")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Valid only if the curve is bound. Returns a polyline approximation to the curve.")]
	public class API_Curve_Tessellate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Curve.Tessellate
		///</summary>
		public API_Curve_Tessellate()
		{
			base_type = typeof(Autodesk.Revit.DB.Curve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Tessellate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Valid only if the curve is bound. Returns a polyline approximation to the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_Period")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The period of this curve.")]
	public class API_Curve_Period : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_Period()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The period of this curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.Period;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Curve_IsCyclic")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this curve is cyclic.")]
	public class API_Curve_IsCyclic : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_IsCyclic()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this curve is cyclic.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.IsCyclic;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Curve_Length")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The exact length of the curve.")]
	public class API_Curve_Length : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_Length()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The exact length of the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.Length;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Curve_ApproximateLength")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The approximate length of the curve.")]
	public class API_Curve_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_ApproximateLength()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The approximate length of the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.ApproximateLength;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Curve_Reference")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the curve.")]
	public class API_Curve_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_Reference()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Curve_IsBound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Describes whether the parameter of the curve is restricted to a particular interval.")]
	public class API_Curve_IsBound : dynRevitTransactionNodeWithOneOutput
	{
		public API_Curve_IsBound()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Describes whether the parameter of the curve is restricted to a particular interval.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Curve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Curve));
			var result = arg0.IsBound;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.SweptBlend.GetVertexConnectionMap
	///</summary>
	[NodeName("API_SweptBlend_GetVertexConnectionMap")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the mapping between the vertices in the top and bottom profiles.")]
	public class API_SweptBlend_GetVertexConnectionMap : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.SweptBlend.GetVertexConnectionMap
		///</summary>
		public API_SweptBlend_GetVertexConnectionMap()
		{
			base_type = typeof(Autodesk.Revit.DB.SweptBlend);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetVertexConnectionMap", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the mapping between the vertices in the top and bottom profiles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_TopProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class API_SweptBlend_TopProfile : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.TopProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_BottomProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class API_SweptBlend_BottomProfile : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.BottomProfile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_SelectedPath")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The selected curve used for the swept blend path.")]
	public class API_SweptBlend_SelectedPath : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_SelectedPath()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The selected curve used for the swept blend path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.SelectedPath;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_PathSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The sketched path for the swept blend.")]
	public class API_SweptBlend_PathSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The sketched path for the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.PathSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_BottomProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The bottom family symbol profile of the swept blend.")]
	public class API_SweptBlend_BottomProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_BottomProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom family symbol profile of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.BottomProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_BottomSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The bottom profile sketch of the swept blend.")]
	public class API_SweptBlend_BottomSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom profile sketch of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.BottomSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_TopProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The top family symbol profile of the swept blend.")]
	public class API_SweptBlend_TopProfileSymbol : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_TopProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top family symbol profile of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.TopProfileSymbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_SweptBlend_TopSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The top profile sketch of the swept blend.")]
	public class API_SweptBlend_TopSketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_SweptBlend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top profile sketch of the swept blend.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.SweptBlend)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.SweptBlend));
			var result = arg0.TopSketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
	///</summary>
	[NodeName("API_Form_Rehost")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rehost Form to sketch plane")]
	public class API_Form_Rehost : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
		///</summary>
		public API_Form_Rehost()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Rehost", false, new Type[]{typeof(Autodesk.Revit.DB.SketchPlane),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("sp", "The sketch plane on which to rehost the form.",typeof(Autodesk.Revit.DB.SketchPlane)));
			InPortData.Add(new PortData("xyz", "The location to which to Rehost the form.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Rehost Form to sketch plane",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
	///</summary>
	[NodeName("API_Form_Rehost_1")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rehost Form to edge, face or curve.")]
	public class API_Form_Rehost_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
		///</summary>
		public API_Form_Rehost_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Rehost", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference on which to rehost the form.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The location to which to Rehost the form.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Rehost Form to edge, face or curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddProfile
	///</summary>
	[NodeName("API_Form_AddProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a profile into the form, by a specified edge/param.")]
	public class API_Form_AddProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.AddProfile
		///</summary>
		public API_Form_AddProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddProfile", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of edge.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The param on edge to specify the location.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Add a profile into the form, by a specified edge/param.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on same/different profile, by a pair of specified points.")]
	public class API_Form_AddEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
		///</summary>
		public API_Form_AddEdge()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of start point",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The geometry reference of end point",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Add an edge to the form, connecting two edges on same/different profile, by a pair of specified points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge_1")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on same/different profile, by a pair of specified edge/param.")]
	public class API_Form_AddEdge_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
		///</summary>
		public API_Form_AddEdge_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(System.Double),typeof(Autodesk.Revit.DB.Reference),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of start edge",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The param on start edge to specify the location.",typeof(System.Double)));
			InPortData.Add(new PortData("ref", "The geometry reference of end edge",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The param on end edge to specify the location.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Add an edge to the form, connecting two edges on same/different profile, by a pair of specified edge/param.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge_2")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on different profiles, by a specified face of the form and a point on face.")]
	public class API_Form_AddEdge_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
		///</summary>
		public API_Form_AddEdge_2()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "A point on the face, defining the position of edge to be created.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Add an edge to the form, connecting two edges on different profiles, by a specified face of the form and a point on face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleProfile
	///</summary>
	[NodeName("API_Form_ScaleProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scale a profile of the form, by a specified origin and scale factor.")]
	public class API_Form_ScaleProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleProfile
		///</summary>
		public API_Form_ScaleProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ScaleProfile", false, new Type[]{typeof(System.Int32),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile.",typeof(System.Int32)));
			InPortData.Add(new PortData("n", "The scale factor, it should be large than zero.",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The origin where scale happens.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Scale a profile of the form, by a specified origin and scale factor.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleSubElement
	///</summary>
	[NodeName("API_Form_ScaleSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scale a face/edge/curve/vertex of the form, by a specified origin and scale factor.")]
	public class API_Form_ScaleSubElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleSubElement
		///</summary>
		public API_Form_ScaleSubElement()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ScaleSubElement", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face/edge/curve/vertex",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("n", "The scale factor, it should be large than zero.",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The origin where scale happens.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Scale a face/edge/curve/vertex of the form, by a specified origin and scale factor.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.RotateProfile
	///</summary>
	[NodeName("API_Form_RotateProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotate a profile of the form, by a specified angle around a given axis.")]
	public class API_Form_RotateProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.RotateProfile
		///</summary>
		public API_Form_RotateProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RotateProfile", false, new Type[]{typeof(System.Int32),typeof(Autodesk.Revit.DB.Line),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile.",typeof(System.Int32)));
			InPortData.Add(new PortData("crv", "An unbounded line that represents the axis of rotation.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The angle, in radians, by which the element is to be rotated around the specified axis.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Rotate a profile of the form, by a specified angle around a given axis.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.RotateSubElement
	///</summary>
	[NodeName("API_Form_RotateSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotate a face/edge/curve/vertex of the form, by a specified angle around a given axis.")]
	public class API_Form_RotateSubElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.RotateSubElement
		///</summary>
		public API_Form_RotateSubElement()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RotateSubElement", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Line),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face/edge/curve/vertex",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("crv", "An unbounded line that represents the axis of rotation.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The angle, in radians, by which the element is to be rotated around the specified axis.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Rotate a face/edge/curve/vertex of the form, by a specified angle around a given axis.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.MoveProfile
	///</summary>
	[NodeName("API_Form_MoveProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Move a profile of the form, specified by a reference, and an offset vector.")]
	public class API_Form_MoveProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.MoveProfile
		///</summary>
		public API_Form_MoveProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MoveProfile", false, new Type[]{typeof(System.Int32),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile.",typeof(System.Int32)));
			InPortData.Add(new PortData("xyz", "The vector by which the element is to be moved.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Move a profile of the form, specified by a reference, and an offset vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.MoveSubElement
	///</summary>
	[NodeName("API_Form_MoveSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Move a face/edge/curve/vertex of the form, specified by a reference, and an offset vector.")]
	public class API_Form_MoveSubElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.MoveSubElement
		///</summary>
		public API_Form_MoveSubElement()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MoveSubElement", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face/edge/curve/vertex",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("xyz", "The vector by which the element is to be moved.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Move a face/edge/curve/vertex of the form, specified by a reference, and an offset vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteProfile
	///</summary>
	[NodeName("API_Form_DeleteProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Delete a profile of the form.")]
	public class API_Form_DeleteProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteProfile
		///</summary>
		public API_Form_DeleteProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DeleteProfile", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Delete a profile of the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteSubElement
	///</summary>
	[NodeName("API_Form_DeleteSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Delete a face/edge/curve/vertex of the form, specified by a reference.")]
	public class API_Form_DeleteSubElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteSubElement
		///</summary>
		public API_Form_DeleteSubElement()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DeleteSubElement", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face/edge/curve/vertex",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Delete a face/edge/curve/vertex of the form, specified by a reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateProfile
	///</summary>
	[NodeName("API_Form_CanManipulateProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if a profile can be deleted/moved/rotated.")]
	public class API_Form_CanManipulateProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateProfile
		///</summary>
		public API_Form_CanManipulateProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanManipulateProfile", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Tell if a profile can be deleted/moved/rotated.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateSubElement
	///</summary>
	[NodeName("API_Form_CanManipulateSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if a sub element can be deleted/moved/rotated/scaled.")]
	public class API_Form_CanManipulateSubElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateSubElement
		///</summary>
		public API_Form_CanManipulateSubElement()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanManipulateSubElement", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The geometry reference of face/edge/curve/vertex",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if a sub element can be deleted/moved/rotated/scaled.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetCurvesAndEdgesReference
	///</summary>
	[NodeName("API_Form_GetCurvesAndEdgesReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a point, return all edges and curves that it is lying on.")]
	public class API_Form_GetCurvesAndEdgesReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.GetCurvesAndEdgesReference
		///</summary>
		public API_Form_GetCurvesAndEdgesReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCurvesAndEdgesReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of a point.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given a point, return all edges and curves that it is lying on.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetControlPoints
	///</summary>
	[NodeName("API_Form_GetControlPoints")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given an edge or a curve or a face, return all control points lying on it (in form of geometry references).")]
	public class API_Form_GetControlPoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.GetControlPoints
		///</summary>
		public API_Form_GetControlPoints()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetControlPoints", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of an edge or curve or face.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given an edge or a curve or a face, return all control points lying on it (in form of geometry references).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsConnectingEdge
	///</summary>
	[NodeName("API_Form_IsConnectingEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge is a connecting edge on a side face. Connecting edges connect vertices on different profiles.")]
	public class API_Form_IsConnectingEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsConnectingEdge
		///</summary>
		public API_Form_IsConnectingEdge()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsConnectingEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the edge to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if an edge is a connecting edge on a side face. Connecting edges connect vertices on different profiles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsProfileEdge
	///</summary>
	[NodeName("API_Form_IsProfileEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge or curve is generated from a profile.")]
	public class API_Form_IsProfileEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsProfileEdge
		///</summary>
		public API_Form_IsProfileEdge()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsProfileEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the edge or curve to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if an edge or curve is generated from a profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsAutoCreaseEdge
	///</summary>
	[NodeName("API_Form_IsAutoCreaseEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge is an auto-crease on a top/bottom cap face.")]
	public class API_Form_IsAutoCreaseEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsAutoCreaseEdge
		///</summary>
		public API_Form_IsAutoCreaseEdge()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAutoCreaseEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the edge to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if an edge is an auto-crease on a top/bottom cap face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsSideFace
	///</summary>
	[NodeName("API_Form_IsSideFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is a side face.")]
	public class API_Form_IsSideFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsSideFace
		///</summary>
		public API_Form_IsSideFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsSideFace", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the  face to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given a face, tell if it is a side face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsEndFace
	///</summary>
	[NodeName("API_Form_IsEndFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is an end cap face.")]
	public class API_Form_IsEndFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsEndFace
		///</summary>
		public API_Form_IsEndFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsEndFace", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the face to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given a face, tell if it is an end cap face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsBeginningFace
	///</summary>
	[NodeName("API_Form_IsBeginningFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is a beginning cap face.")]
	public class API_Form_IsBeginningFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsBeginningFace
		///</summary>
		public API_Form_IsBeginningFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsBeginningFace", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the  face to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given a face, tell if it is a beginning cap face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsReferenceOnlyProfile
	///</summary>
	[NodeName("API_Form_IsReferenceOnlyProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the profile is made by referencing existing geometry in the Revit model.")]
	public class API_Form_IsReferenceOnlyProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsReferenceOnlyProfile
		///</summary>
		public API_Form_IsReferenceOnlyProfile()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsReferenceOnlyProfile", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile to be checked.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Tell if the profile is made by referencing existing geometry in the Revit model.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsFaceReference
	///</summary>
	[NodeName("API_Form_IsFaceReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a face of the form.")]
	public class API_Form_IsFaceReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsFaceReference
		///</summary>
		public API_Form_IsFaceReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsFaceReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if the pick is the reference to a face of the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsCurveReference
	///</summary>
	[NodeName("API_Form_IsCurveReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a curve of the form.")]
	public class API_Form_IsCurveReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsCurveReference
		///</summary>
		public API_Form_IsCurveReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsCurveReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if the pick is the reference to a curve of the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsEdgeReference
	///</summary>
	[NodeName("API_Form_IsEdgeReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to an edge of the form.")]
	public class API_Form_IsEdgeReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsEdgeReference
		///</summary>
		public API_Form_IsEdgeReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsEdgeReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if the pick is the reference to an edge of the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsVertexReference
	///</summary>
	[NodeName("API_Form_IsVertexReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a vertex of the form.")]
	public class API_Form_IsVertexReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.IsVertexReference
		///</summary>
		public API_Form_IsVertexReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsVertexReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to be checked.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Tell if the pick is the reference to a vertex of the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ConstrainProfiles
	///</summary>
	[NodeName("API_Form_ConstrainProfiles")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Constrain form profiles using the specified profile as master. This is an advanced version of property \"AreProfilesConstrained\", allowing specify the master profile.")]
	public class API_Form_ConstrainProfiles : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.ConstrainProfiles
		///</summary>
		public API_Form_ConstrainProfiles()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ConstrainProfiles", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Index to specify the profile used as master profile.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Constrain form profiles using the specified profile as master. This is an advanced version of property \"AreProfilesConstrained\", allowing specify the master profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetPathCurveIndexByCurveReference
	///</summary>
	[NodeName("API_Form_GetPathCurveIndexByCurveReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a reference to certain curve in the path, return its index.")]
	public class API_Form_GetPathCurveIndexByCurveReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Form.GetPathCurveIndexByCurveReference
		///</summary>
		public API_Form_GetPathCurveIndexByCurveReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Form);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPathCurveIndexByCurveReference", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Reference to the curve in path",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Given a reference to certain curve in the path, return its index.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Form_BaseOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve/set the base offset of the form object. It is only valid for locked form.")]
	public class API_Form_BaseOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_BaseOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the base offset of the form object. It is only valid for locked form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.BaseOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_TopOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve/set the top offset of the form object. It is only valid for locked form.")]
	public class API_Form_TopOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_TopOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the top offset of the form object. It is only valid for locked form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.TopOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_HasOpenGeometry")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the form has an open geometry.")]
	public class API_Form_HasOpenGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_HasOpenGeometry()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has an open geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.HasOpenGeometry;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_AreProfilesConstrained")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get/set if the form's profiles are constrained.")]
	public class API_Form_AreProfilesConstrained : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_AreProfilesConstrained()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form's profiles are constrained.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.AreProfilesConstrained;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_IsInXRayMode")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get/set if the form is in X-Ray mode.")]
	public class API_Form_IsInXRayMode : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_IsInXRayMode()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form is in X-Ray mode.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.IsInXRayMode;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_HasOneOrMoreReferenceProfiles")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the form has any reference profile.")]
	public class API_Form_HasOneOrMoreReferenceProfiles : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_HasOneOrMoreReferenceProfiles()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has any reference profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.HasOneOrMoreReferenceProfiles;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_PathCurveCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of curves in the form path.")]
	public class API_Form_PathCurveCount : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_PathCurveCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of curves in the form path.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.PathCurveCount;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Form_ProfileCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of profiles in the form.")]
	public class API_Form_ProfileCount : dynRevitTransactionNodeWithOneOutput
	{
		public API_Form_ProfileCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of profiles in the form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Form)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Form));
			var result = arg0.ProfileCount;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
	///</summary>
	[NodeName("API_BoundingBoxUV")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates object with supplied values.")]
	public class API_BoundingBoxUV : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
		///</summary>
		public API_BoundingBoxUV()
		{
			base_type = typeof(Autodesk.Revit.DB.BoundingBoxUV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "BoundingBoxUV", true, new Type[]{typeof(System.Double),typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate of min.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate of min.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The first coordinate of max.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate of max.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates object with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
	///</summary>
	[NodeName("API_BoundingBoxUV_1")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("default constructor")]
	public class API_BoundingBoxUV_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
		///</summary>
		public API_BoundingBoxUV_1()
		{
			base_type = typeof(Autodesk.Revit.DB.BoundingBoxUV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "BoundingBoxUV", true, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","default constructor",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxUV_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Maximum coordinates (upper-right corner of the box).")]
	public class API_BoundingBoxUV_Max : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxUV_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxUV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxUV));
			var result = arg0.Max;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_BoundingBoxUV_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Minimum coordinates (lower-left corner of the box).")]
	public class API_BoundingBoxUV_Min : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxUV_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxUV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxUV));
			var result = arg0.Min;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTotalTransform
	///</summary>
	[NodeName("API_Instance_GetTotalTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the total transform, which includes the true north transform for instances like import instances.")]
	public class API_Instance_GetTotalTransform : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTotalTransform
		///</summary>
		public API_Instance_GetTotalTransform()
		{
			base_type = typeof(Autodesk.Revit.DB.Instance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTotalTransform", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the total transform, which includes the true north transform for instances like import instances.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTransform
	///</summary>
	[NodeName("API_Instance_GetTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the transform of the instance.")]
	public class API_Instance_GetTransform : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTransform
		///</summary>
		public API_Instance_GetTransform()
		{
			base_type = typeof(Autodesk.Revit.DB.Instance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTransform", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Instance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the transform of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Project
	///</summary>
	[NodeName("API_Face_Project")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this face.")]
	public class API_Face_Project : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Project
		///</summary>
		public API_Face_Project()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Project", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The point to be projected.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Projects the specified point on this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified face with this face and returns the intersection results.")]
	public class API_Face_Intersect : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Face),typeof(Autodesk.Revit.DB.Curve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("f", "The specified face to intersect with this face.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("val", "A single Curve representing the intersection.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Calculates the intersection of the specified face with this face and returns the intersection results.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified face with this face and returns the intersection results.")]
	public class API_Face_Intersect_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Face)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("f", "The specified face to intersect with this face.",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Calculates the intersection of the specified face with this face and returns the intersection results.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect_2")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face and returns the intersection results.")]
	public class API_Face_Intersect_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect_2()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.IntersectionResultArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Provides more information about the intersection.",typeof(Autodesk.Revit.DB.IntersectionResultArray)));
			OutPortData.Add(new PortData("out","Calculates the intersection of the specified curve with this face and returns the intersection results.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect_3")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face.")]
	public class API_Face_Intersect_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect_3()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Intersect", false, new Type[]{typeof(Autodesk.Revit.DB.Curve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Calculates the intersection of the specified curve with this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
	///</summary>
	[NodeName("API_Face_IsInside")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face and outputs additional results.")]
	public class API_Face_IsInside : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
		///</summary>
		public API_Face_IsInside()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsInside", false, new Type[]{typeof(Autodesk.Revit.DB.UV),typeof(Autodesk.Revit.DB.IntersectionResult)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("val", "Provides more information when the point is on the edge; otherwise,",typeof(Autodesk.Revit.DB.IntersectionResult)));
			OutPortData.Add(new PortData("out","Indicates whether the specified point is within this face and outputs additional results.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
	///</summary>
	[NodeName("API_Face_IsInside_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face.")]
	public class API_Face_IsInside_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
		///</summary>
		public API_Face_IsInside_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsInside", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Indicates whether the specified point is within this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeNormal
	///</summary>
	[NodeName("API_Face_ComputeNormal")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal vector for the face at the given point.")]
	public class API_Face_ComputeNormal : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeNormal
		///</summary>
		public API_Face_ComputeNormal()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeNormal", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Returns the normal vector for the face at the given point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeDerivatives
	///</summary>
	[NodeName("API_Face_ComputeDerivatives")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the face at the specified point.")]
	public class API_Face_ComputeDerivatives : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeDerivatives
		///</summary>
		public API_Face_ComputeDerivatives()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeDerivatives", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Returns the vectors describing the face at the specified point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.GetBoundingBox
	///</summary>
	[NodeName("API_Face_GetBoundingBox")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the UV bounding box of the face.")]
	public class API_Face_GetBoundingBox : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.GetBoundingBox
		///</summary>
		public API_Face_GetBoundingBox()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetBoundingBox", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the UV bounding box of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Evaluate
	///</summary>
	[NodeName("API_Face_Evaluate")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates parameters on the face.")]
	public class API_Face_Evaluate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Evaluate
		///</summary>
		public API_Face_Evaluate()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Evaluate", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The parameters to be evaluated, in natural parameterization of the face.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Evaluates parameters on the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
	///</summary>
	[NodeName("API_Face_Triangulate")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class API_Face_Triangulate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
		///</summary>
		public API_Face_Triangulate()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Triangulate", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The level of detail. Its range is from 0 to 1. 0 is the lowest level of detail and 1 is the highest.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Returns a triangular mesh approximation to the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
	///</summary>
	[NodeName("API_Face_Triangulate_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class API_Face_Triangulate_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
		///</summary>
		public API_Face_Triangulate_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Triangulate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a triangular mesh approximation to the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.GetRegions
	///</summary>
	[NodeName("API_Face_GetRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Face regions (created with the Split Face command) of the face.")]
	public class API_Face_GetRegions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.GetRegions
		///</summary>
		public API_Face_GetRegions()
		{
			base_type = typeof(Autodesk.Revit.DB.Face);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetRegions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Face regions (created with the Split Face command) of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Face_Area")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The area of this face.")]
	public class API_Face_Area : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_Area()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","The area of this face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.Area;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Face_Reference")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the face.")]
	public class API_Face_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_Reference()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Face_IsTwoSided")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines if a face is two-sided (degenerate)")]
	public class API_Face_IsTwoSided : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_IsTwoSided()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Determines if a face is two-sided (degenerate)",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.IsTwoSided;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Face_MaterialElementId")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Element ID of the material from which this face is composed.")]
	public class API_Face_MaterialElementId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_MaterialElementId()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this face is composed.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.MaterialElementId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Face_EdgeLoops")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Each edge loop is a closed boundary of the face.")]
	public class API_Face_EdgeLoops : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_EdgeLoops()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Each edge loop is a closed boundary of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.EdgeLoops;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Face_HasRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Reports if the face contains regions created with the Split Face command.")]
	public class API_Face_HasRegions : dynRevitTransactionNodeWithOneOutput
	{
		public API_Face_HasRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Reports if the face contains regions created with the Split Face command.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Face)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Face));
			var result = arg0.HasRegions;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Level_PlaneReference")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a reference to this element as a plane.")]
	public class API_Level_PlaneReference : dynRevitTransactionNodeWithOneOutput
	{
		public API_Level_PlaneReference()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Returns a reference to this element as a plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = arg0.PlaneReference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Level_LevelType")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The level style of this level.")]
	public class API_Level_LevelType : dynRevitTransactionNodeWithOneOutput
	{
		public API_Level_LevelType()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","The level style of this level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = arg0.LevelType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Level_ProjectElevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.")]
	public class API_Level_ProjectElevation : dynRevitTransactionNodeWithOneOutput
	{
		public API_Level_ProjectElevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = arg0.ProjectElevation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Level_Elevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes the elevation above or below the ground level.")]
	public class API_Level_Elevation : dynRevitTransactionNodeWithOneOutput
	{
		public API_Level_Elevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieves or changes the elevation above or below the ground level.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Level)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Level));
			var result = arg0.Elevation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_BoundingBoxXYZ_Enabled")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Defines whether bounding box is turned on.")]
	public class API_BoundingBoxXYZ_Enabled : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxXYZ_Enabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Defines whether bounding box is turned on.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = arg0.Enabled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_BoundingBoxXYZ_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Maximum coordinates (upper-right-front corner of the box).")]
	public class API_BoundingBoxXYZ_Max : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxXYZ_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right-front corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = arg0.Max;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_BoundingBoxXYZ_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Minimum coordinates (lower-left-rear corner of the box).")]
	public class API_BoundingBoxXYZ_Min : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxXYZ_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left-rear corner of the box).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = arg0.Min;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_BoundingBoxXYZ_Transform")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transform FROM the coordinate space of the box TO the model space.")]
	public class API_BoundingBoxXYZ_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public API_BoundingBoxXYZ_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","The transform FROM the coordinate space of the box TO the model space.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.BoundingBoxXYZ)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.BoundingBoxXYZ));
			var result = arg0.Transform;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetOriginalGeometry
	///</summary>
	[NodeName("API_FamilyInstance_GetOriginalGeometry")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the original geometry of the instance, before the instance is modified by joins, cuts, coping, extensions, or other post-processing.")]
	public class API_FamilyInstance_GetOriginalGeometry : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetOriginalGeometry
		///</summary>
		public API_FamilyInstance_GetOriginalGeometry()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetOriginalGeometry", false, new Type[]{typeof(Autodesk.Revit.DB.Options)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The options used to obtain the geometry.  Note that ComputeReferences may notbe set to true.",typeof(Autodesk.Revit.DB.Options)));
			OutPortData.Add(new PortData("out","Returns the original geometry of the instance, before the instance is modified by joins, cuts, coping, extensions, or other post-processing.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetFamilyPointPlacementReferences
	///</summary>
	[NodeName("API_FamilyInstance_GetFamilyPointPlacementReferences")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Point Placement References for the Family Instance.")]
	public class API_FamilyInstance_GetFamilyPointPlacementReferences : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetFamilyPointPlacementReferences
		///</summary>
		public API_FamilyInstance_GetFamilyPointPlacementReferences()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetFamilyPointPlacementReferences", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the Point Placement References for the Family Instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.RemoveCoping
	///</summary>
	[NodeName("API_FamilyInstance_RemoveCoping")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes a coping (cut) from a steel beam.")]
	public class API_FamilyInstance_RemoveCoping : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.RemoveCoping
		///</summary>
		public API_FamilyInstance_RemoveCoping()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RemoveCoping", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A steel beam or column for which this beam currently has a coping cut. May not be",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Removes a coping (cut) from a steel beam.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.AddCoping
	///</summary>
	[NodeName("API_FamilyInstance_AddCoping")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a coping (cut) to a steel beam.")]
	public class API_FamilyInstance_AddCoping : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.AddCoping
		///</summary>
		public API_FamilyInstance_AddCoping()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddCoping", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyInstance)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A steel beam or column. May not be",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Adds a coping (cut) to a steel beam.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.SetCopingIds
	///</summary>
	[NodeName("API_FamilyInstance_SetCopingIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Specifies the set of coping cutters on this element.")]
	public class API_FamilyInstance_SetCopingIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.SetCopingIds
		///</summary>
		public API_FamilyInstance_SetCopingIds()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetCopingIds", false, new Type[]{typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A set of coping cutters (steel beams and steel columns).",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			OutPortData.Add(new PortData("out","Specifies the set of coping cutters on this element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetCopingIds
	///</summary>
	[NodeName("API_FamilyInstance_GetCopingIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Lists the elements currently used as coping cutters for this element.")]
	public class API_FamilyInstance_GetCopingIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetCopingIds
		///</summary>
		public API_FamilyInstance_GetCopingIds()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCopingIds", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Lists the elements currently used as coping cutters for this element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetSubComponentIds
	///</summary>
	[NodeName("API_FamilyInstance_GetSubComponentIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the sub component ElementIds of the current family instance.")]
	public class API_FamilyInstance_GetSubComponentIds : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetSubComponentIds
		///</summary>
		public API_FamilyInstance_GetSubComponentIds()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetSubComponentIds", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the sub component ElementIds of the current family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.FlipFromToRoom
	///</summary>
	[NodeName("API_FamilyInstance_FlipFromToRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Flips the settings of \"From Room\" and \"To Room\" for the door or window instance.")]
	public class API_FamilyInstance_FlipFromToRoom : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.FlipFromToRoom
		///</summary>
		public API_FamilyInstance_FlipFromToRoom()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "FlipFromToRoom", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Flips the settings of \"From Room\" and \"To Room\" for the door or window instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.rotate
	///</summary>
	[NodeName("API_FamilyInstance_rotate")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The family instance will be flipped by 180 degrees. If it can not be rotated, return false, otherwise return true.")]
	public class API_FamilyInstance_rotate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.rotate
		///</summary>
		public API_FamilyInstance_rotate()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "rotate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The family instance will be flipped by 180 degrees. If it can not be rotated, return false, otherwise return true.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipFacing
	///</summary>
	[NodeName("API_FamilyInstance_flipFacing")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The orientation of family instance facing will be flipped. If it can not be flipped, return false, otherwise return true.")]
	public class API_FamilyInstance_flipFacing : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipFacing
		///</summary>
		public API_FamilyInstance_flipFacing()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "flipFacing", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The orientation of family instance facing will be flipped. If it can not be flipped, return false, otherwise return true.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipHand
	///</summary>
	[NodeName("API_FamilyInstance_flipHand")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The orientation of family instance hand will be flipped. If it can not be flipped, return false, otherwise return true.")]
	public class API_FamilyInstance_flipHand : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipHand
		///</summary>
		public API_FamilyInstance_flipHand()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "flipHand", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The orientation of family instance hand will be flipped. If it can not be flipped, return false, otherwise return true.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_IsWorkPlaneFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the instance's work plane is flipped.")]
	public class API_FamilyInstance_IsWorkPlaneFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_IsWorkPlaneFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance's work plane is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.IsWorkPlaneFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_CanFlipWorkPlane")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the instance can flip its work plane.")]
	public class API_FamilyInstance_CanFlipWorkPlane : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_CanFlipWorkPlane()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance can flip its work plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.CanFlipWorkPlane;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_IsSlantedColumn")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates if the family instance is a slanted column.")]
	public class API_FamilyInstance_IsSlantedColumn : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_IsSlantedColumn()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Indicates if the family instance is a slanted column.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.IsSlantedColumn;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_ExtensionUtility")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to check whether the instance can be extended and return the interface for extension operation.")]
	public class API_FamilyInstance_ExtensionUtility : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_ExtensionUtility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to check whether the instance can be extended and return the interface for extension operation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.ExtensionUtility;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_SuperComponent")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the super component of current family instance.")]
	public class API_FamilyInstance_SuperComponent : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_SuperComponent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the super component of current family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.SuperComponent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_ToRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The \"To Room\" set for the door or window in the last phase of the project.")]
	public class API_FamilyInstance_ToRoom : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_ToRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"To Room\" set for the door or window in the last phase of the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.ToRoom;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_FromRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The \"From Room\" set for the door or window in the last phase of the project.")]
	public class API_FamilyInstance_FromRoom : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_FromRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"From Room\" set for the door or window in the last phase of the project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.FromRoom;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_CanRotate")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance can be rotated by 180 degrees.")]
	public class API_FamilyInstance_CanRotate : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_CanRotate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance can be rotated by 180 degrees.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.CanRotate;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_CanFlipFacing")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance facing can be flipped.")]
	public class API_FamilyInstance_CanFlipFacing : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_CanFlipFacing()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing can be flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.CanFlipFacing;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_CanFlipHand")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance hand can be flipped.")]
	public class API_FamilyInstance_CanFlipHand : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_CanFlipHand()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand can be flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.CanFlipHand;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Mirrored")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance is mirrored. (only one axis is flipped)")]
	public class API_FamilyInstance_Mirrored : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Mirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is mirrored. (only one axis is flipped)",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Mirrored;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Invisible")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance is invisible.")]
	public class API_FamilyInstance_Invisible : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Invisible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is invisible.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Invisible;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_FacingFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance facing is flipped.")]
	public class API_FamilyInstance_FacingFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_FacingFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.FacingFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_HandFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance hand is flipped.")]
	public class API_FamilyInstance_HandFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_HandFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.HandFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_FacingOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the orientation of family instance facing.")]
	public class API_FamilyInstance_FacingOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_FacingOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance facing.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.FacingOrientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_HandOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the orientation of family instance hand.")]
	public class API_FamilyInstance_HandOrientation : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_HandOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance hand.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.HandOrientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_HostFace")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the reference to the host face of family instance.")]
	public class API_FamilyInstance_HostFace : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_HostFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the reference to the host face of family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.HostFace;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_HostParameter")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the instance is hosted by a wall, this property returns the parameter value of the insertionpoint of the instance along the wall's location curve.")]
	public class API_FamilyInstance_HostParameter : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_HostParameter()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","If the instance is hosted by a wall, this property returns the parameter value of the insertionpoint of the instance along the wall's location curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.HostParameter;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Host")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.")]
	public class API_FamilyInstance_Host : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Host;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Location")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This property is used to find the physical location of an instance within project.")]
	public class API_FamilyInstance_Location : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Location()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property is used to find the physical location of an instance within project.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Location;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Space")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The space in which the instance is located (during the last phase of the project).")]
	public class API_FamilyInstance_Space : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Space()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The space in which the instance is located (during the last phase of the project).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Space;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Room")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The room in which the instance is located (during the last phase of the project).")]
	public class API_FamilyInstance_Room : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Room()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The room in which the instance is located (during the last phase of the project).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Room;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_StructuralType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Provides the primary structural type of the instance, such as beam or column etc.")]
	public class API_FamilyInstance_StructuralType : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_StructuralType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural type of the instance, such as beam or column etc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.StructuralType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_StructuralUsage")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Provides the primary structural usage of the instance, such as brace, girder etc.")]
	public class API_FamilyInstance_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural usage of the instance, such as brace, girder etc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.StructuralUsage;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_StructuralMaterialId")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies the material that defines the instance's structural analysis properties.")]
	public class API_FamilyInstance_StructuralMaterialId : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_StructuralMaterialId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies the material that defines the instance's structural analysis properties.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.StructuralMaterialId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_StructuralMaterialType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This property returns the physical material from which the instance is made.")]
	public class API_FamilyInstance_StructuralMaterialType : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_StructuralMaterialType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property returns the physical material from which the instance is made.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.StructuralMaterialType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_MEPModel")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves the MEP model for the family instance.")]
	public class API_FamilyInstance_MEPModel : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_MEPModel()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Retrieves the MEP model for the family instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.MEPModel;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_FamilyInstance_Symbol")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns or changes the FamilySymbol object that represents the type of the instance.")]
	public class API_FamilyInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public API_FamilyInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Returns or changes the FamilySymbol object that represents the type of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.FamilyInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.FamilyInstance));
			var result = arg0.Symbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Color.#ctor
	///</summary>
	[NodeName("API_Color")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Constructor that takes the red, green and blue channels of the color.")]
	public class API_Color : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Color.#ctor
		///</summary>
		public API_Color()
		{
			base_type = typeof(Autodesk.Revit.DB.Color);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Color", true, new Type[]{typeof(System.Byte),typeof(System.Byte),typeof(System.Byte)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The red channel of the color that ranges from 0 to 255.",typeof(System.Byte)));
			InPortData.Add(new PortData("val", "The green channel of the color that ranges from 0 to 255.",typeof(System.Byte)));
			InPortData.Add(new PortData("val", "The blue channel of the color that ranges from 0 to 255.",typeof(System.Byte)));
			OutPortData.Add(new PortData("out","Constructor that takes the red, green and blue channels of the color.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Color_InvalidColorValue")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the invalid Color whose IntegerValue is -1.")]
	public class API_Color_InvalidColorValue : dynRevitTransactionNodeWithOneOutput
	{
		public API_Color_InvalidColorValue()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get the invalid Color whose IntegerValue is -1.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
            Autodesk.Revit.DB.Color arg0 = (Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = arg0.InvalidColorValue;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Color_IsValid")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the color represents a valid color, or an uninitialized/invalid value.")]
	public class API_Color_IsValid : dynRevitTransactionNodeWithOneOutput
	{
		public API_Color_IsValid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Identifies if the color represents a valid color, or an uninitialized/invalid value.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = arg0.IsValid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Color_Blue")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the blue channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.")]
	public class API_Color_Blue : dynRevitTransactionNodeWithOneOutput
	{
		public API_Color_Blue()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get the blue channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = arg0.Blue;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Color_Green")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the green channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.")]
	public class API_Color_Green : dynRevitTransactionNodeWithOneOutput
	{
		public API_Color_Green()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get the green channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = arg0.Green;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Color_Red")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the red channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.")]
	public class API_Color_Red : dynRevitTransactionNodeWithOneOutput
	{
		public API_Color_Red()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get the red channel of the color.  Setting a channel is obsolete in Autodesk Revit 2013.  Please create a new color instead.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Color)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Color));
			var result = arg0.Red;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GenericForm.GetVisibility
	///</summary>
	[NodeName("API_GenericForm_GetVisibility")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the generic form.")]
	public class API_GenericForm_GetVisibility : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GenericForm.GetVisibility
		///</summary>
		public API_GenericForm_GetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.GenericForm);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetVisibility", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the visibility for the generic form.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_GenericForm_Subcategory")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The subcategory.")]
	public class API_GenericForm_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public API_GenericForm_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = arg0.Subcategory;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_GenericForm_Name")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get and Set the Name property")]
	public class API_GenericForm_Name : dynRevitTransactionNodeWithOneOutput
	{
		public API_GenericForm_Name()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Get and Set the Name property",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = arg0.Name;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_GenericForm_IsSolid")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the GenericForm is a solid or a void element.")]
	public class API_GenericForm_IsSolid : dynRevitTransactionNodeWithOneOutput
	{
		public API_GenericForm_IsSolid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Identifies if the GenericForm is a solid or a void element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = arg0.IsSolid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_GenericForm_Visible")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The visibility of the GenericForm.")]
	public class API_GenericForm_Visible : dynRevitTransactionNodeWithOneOutput
	{
		public API_GenericForm_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The visibility of the GenericForm.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GenericForm)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GenericForm));
			var result = arg0.Visible;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Point_Reference")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the point.")]
	public class API_Point_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public API_Point_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Point)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Point));
			var result = arg0.Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Point_Coord")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the coordinates of the point.")]
	public class API_Point_Coord : dynRevitTransactionNodeWithOneOutput
	{
		public API_Point_Coord()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns the coordinates of the point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Point)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Point));
			var result = arg0.Coord;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.CanBeIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_CanBeIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Checks if the element can be an intersection reference.")]
	public class API_DividedSurface_CanBeIntersectionElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.CanBeIntersectionElement
		///</summary>
		public API_DividedSurface_CanBeIntersectionElement()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanBeIntersectionElement", false, new Type[]{typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The element to be checked.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Checks if the element can be an intersection reference.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveAllIntersectionElements
	///</summary>
	[NodeName("API_DividedSurface_RemoveAllIntersectionElements")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes all the intersection elements from a divided surface.")]
	public class API_DividedSurface_RemoveAllIntersectionElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveAllIntersectionElements
		///</summary>
		public API_DividedSurface_RemoveAllIntersectionElements()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RemoveAllIntersectionElements", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Removes all the intersection elements from a divided surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_RemoveIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes an intersection element from a divided surface.")]
	public class API_DividedSurface_RemoveIntersectionElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveIntersectionElement
		///</summary>
		public API_DividedSurface_RemoveIntersectionElement()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RemoveIntersectionElement", false, new Type[]{typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The intersection element to be removed.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Removes an intersection element from a divided surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.AddIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_AddIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds an intersection element to the divided surface.")]
	public class API_DividedSurface_AddIntersectionElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.AddIntersectionElement
		///</summary>
		public API_DividedSurface_AddIntersectionElement()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AddIntersectionElement", false, new Type[]{typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The intersection element to be added.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Adds an intersection element to the divided surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetAllIntersectionElements
	///</summary>
	[NodeName("API_DividedSurface_GetAllIntersectionElements")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets all intersection elements which produce division lines.")]
	public class API_DividedSurface_GetAllIntersectionElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetAllIntersectionElements
		///</summary>
		public API_DividedSurface_GetAllIntersectionElements()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetAllIntersectionElements", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets all intersection elements which produce division lines.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetDividedSurfaceForReference
	///</summary>
	[NodeName("API_DividedSurface_GetDividedSurfaceForReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get a divided surface for a given reference.  Returns null if the reference does not host a divided surface.")]
	public class API_DividedSurface_GetDividedSurfaceForReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetDividedSurfaceForReference
		///</summary>
		public API_DividedSurface_GetDividedSurfaceForReference()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetDividedSurfaceForReference", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("ref", "Reference that represents a face.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Get a divided surface for a given reference.  Returns null if the reference does not host a divided surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetReferencesWithDividedSurfaces
	///</summary>
	[NodeName("API_DividedSurface_GetReferencesWithDividedSurfaces")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("For a given host element get references to all the faces that host a divided surface")]
	public class API_DividedSurface_GetReferencesWithDividedSurfaces : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetReferencesWithDividedSurfaces
		///</summary>
		public API_DividedSurface_GetReferencesWithDividedSurfaces()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetReferencesWithDividedSurfaces", false, new Type[]{typeof(Autodesk.Revit.DB.Element)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("el", "The element that hosts the divided surfaces",typeof(Autodesk.Revit.DB.Element)));
			OutPortData.Add(new PortData("out","For a given host element get references to all the faces that host a divided surface",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.CanBeDivided
	///</summary>
	[NodeName("API_DividedSurface_CanBeDivided")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This returns true if the reference represents a face that can be used to create a divided surface.")]
	public class API_DividedSurface_CanBeDivided : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.CanBeDivided
		///</summary>
		public API_DividedSurface_CanBeDivided()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanBeDivided", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("ref", "The reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","This returns true if the reference represents a face that can be used to create a divided surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.Create
	///</summary>
	[NodeName("API_DividedSurface_Create")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a divided surface with a default layout.")]
	public class API_DividedSurface_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.Create
		///</summary>
		public API_DividedSurface_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("ref", "Reference that represents a face.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Creates a new instance of a divided surface with a default layout.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_NumberOfVGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the number of V-gridlines used on thesurface.")]
	public class API_DividedSurface_NumberOfVGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_NumberOfVGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of V-gridlines used on thesurface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.NumberOfVGridlines;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_NumberOfUGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the number of U-gridlines used on thesurface.")]
	public class API_DividedSurface_NumberOfUGridlines : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_NumberOfUGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of U-gridlines used on thesurface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.NumberOfUGridlines;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_IsComponentFlipped")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the pattern is flipped.")]
	public class API_DividedSurface_IsComponentFlipped : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_IsComponentFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.IsComponentFlipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_IsComponentMirrored")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the pattern is mirror-imaged.")]
	public class API_DividedSurface_IsComponentMirrored : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_IsComponentMirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is mirror-imaged.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.IsComponentMirrored;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_ComponentRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The rotation of the pattern by a multipleof 90 degrees.")]
	public class API_DividedSurface_ComponentRotation : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_ComponentRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The rotation of the pattern by a multipleof 90 degrees.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.ComponentRotation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_VPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset applied to the pattern by an integral number of grid nodes in the V-direction.")]
	public class API_DividedSurface_VPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_VPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by an integral number of grid nodes in the V-direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.VPatternIndent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_UPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset applied to the pattern by anintegral number of grid nodes in the U-direction.")]
	public class API_DividedSurface_UPatternIndent : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_UPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by anintegral number of grid nodes in the U-direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.UPatternIndent;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_BorderTile")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines the handling of tiles that overlap the surface'sboundary.")]
	public class API_DividedSurface_BorderTile : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_BorderTile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Determines the handling of tiles that overlap the surface'sboundary.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.BorderTile;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_AllGridRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Angle of rotation applied to the U- and V- directions together.")]
	public class API_DividedSurface_AllGridRotation : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_AllGridRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Angle of rotation applied to the U- and V- directions together.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.AllGridRotation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_VSpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Access to the rule for laying out the second series of equidistantparallel lines on the surface.")]
	public class API_DividedSurface_VSpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_VSpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the second series of equidistantparallel lines on the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.VSpacingRule;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_USpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Access to the rule for laying out the first series of equidistantparallel lines on the surface.")]
	public class API_DividedSurface_USpacingRule : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_USpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the first series of equidistantparallel lines on the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.USpacingRule;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_HostReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference to the divided face on the host.")]
	public class API_DividedSurface_HostReference : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_HostReference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","A reference to the divided face on the host.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.HostReference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_DividedSurface_Host")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The element whose surface has been divided.")]
	public class API_DividedSurface_Host : dynRevitTransactionNodeWithOneOutput
	{
		public API_DividedSurface_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The element whose surface has been divided.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.DividedSurface)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.DividedSurface));
			var result = arg0.Host;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
	///</summary>
	[NodeName("API_PointCloudInstance_GetPoints")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Extracts a collection of points based on a filter.")]
	public class API_PointCloudInstance_GetPoints : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
		///</summary>
		public API_PointCloudInstance_GetPoints()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPoints", false, new Type[]{typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter),typeof(System.Double),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The filter to control which points are extracted. The filter should be passed in the coordinates   of the Revit model.",typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter)));
			InPortData.Add(new PortData("n", "Desired average distance between \"adjacent\" cloud points (Revit units of length).   The smaller the averageDistance the larger number of points will be returned up to the numPoints limit.   Specifying this parameter makes actual number of points returned for a given filter independent of the   density of coverage produced by the scanner.",typeof(System.Double)));
			InPortData.Add(new PortData("i", "The maximum number of points requested.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Extracts a collection of points based on a filter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
	///</summary>
	[NodeName("API_PointCloudInstance_GetPoints_1")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Extracts a collection of points based on a filter.")]
	public class API_PointCloudInstance_GetPoints_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
		///</summary>
		public API_PointCloudInstance_GetPoints_1()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetPoints", false, new Type[]{typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The filter to control which points are extracted. The filter should be passed in the coordinates   of the Revit model.",typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter)));
			InPortData.Add(new PortData("i", "The maximum number of points requested.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Extracts a collection of points based on a filter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.Create
	///</summary>
	[NodeName("API_PointCloudInstance_Create")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a point cloud based on an input point cloud type and transformation.")]
	public class API_PointCloudInstance_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.Create
		///</summary>
		public API_PointCloudInstance_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new instance is created",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The element id of the PointCloudType.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "The transform that defines the placement of the instance in the Revit document coordinate system.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Creates a new instance of a point cloud based on an input point cloud type and transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.ContainsScan
	///</summary>
	[NodeName("API_PointCloudInstance_ContainsScan")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies whether the instance contains a scan.")]
	public class API_PointCloudInstance_ContainsScan : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.ContainsScan
		///</summary>
		public API_PointCloudInstance_ContainsScan()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ContainsScan", false, new Type[]{typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("s", "Name of the scan.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Identifies whether the instance contains a scan.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.SetSelectionFilter
	///</summary>
	[NodeName("API_PointCloudInstance_SetSelectionFilter")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets active selection filter by cloning of the one passed to it.")]
	public class API_PointCloudInstance_SetSelectionFilter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.SetSelectionFilter
		///</summary>
		public API_PointCloudInstance_SetSelectionFilter()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetSelectionFilter", false, new Type[]{typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The filter object to be made active.  If",typeof(Autodesk.Revit.DB.PointClouds.PointCloudFilter)));
			OutPortData.Add(new PortData("out","Sets active selection filter by cloning of the one passed to it.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetSelectionFilter
	///</summary>
	[NodeName("API_PointCloudInstance_GetSelectionFilter")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the currently active selection filter for this point cloud.")]
	public class API_PointCloudInstance_GetSelectionFilter : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetSelectionFilter
		///</summary>
		public API_PointCloudInstance_GetSelectionFilter()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetSelectionFilter", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the currently active selection filter for this point cloud.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetScanOrigin
	///</summary>
	[NodeName("API_PointCloudInstance_GetScanOrigin")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the origin point of a scan in model coordinates.")]
	public class API_PointCloudInstance_GetScanOrigin : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetScanOrigin
		///</summary>
		public API_PointCloudInstance_GetScanOrigin()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetScanOrigin", false, new Type[]{typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			InPortData.Add(new PortData("s", "Name of the scan.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Returns the origin point of a scan in model coordinates.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetScans
	///</summary>
	[NodeName("API_PointCloudInstance_GetScans")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns array of scan names.")]
	public class API_PointCloudInstance_GetScans : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetScans
		///</summary>
		public API_PointCloudInstance_GetScans()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetScans", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns array of scan names.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.HasColor
	///</summary>
	[NodeName("API_PointCloudInstance_HasColor")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns true if at least one scan of the element have color, false otherwise.")]
	public class API_PointCloudInstance_HasColor : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.HasColor
		///</summary>
		public API_PointCloudInstance_HasColor()
		{
			base_type = typeof(Autodesk.Revit.DB.PointCloudInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "HasColor", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns true if at least one scan of the element have color, false otherwise.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_PointCloudInstance_SupportsOverrides")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies whether the instance can have graphic overrides.")]
	public class API_PointCloudInstance_SupportsOverrides : dynRevitTransactionNodeWithOneOutput
	{
		public API_PointCloudInstance_SupportsOverrides()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			OutPortData.Add(new PortData("out","Identifies whether the instance can have graphic overrides.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointCloudInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PointCloudInstance));
			var result = arg0.SupportsOverrides;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_PointCloudInstance_FilterAction")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The action taken based on the results of the selection filter applied to this point cloud.")]
	public class API_PointCloudInstance_FilterAction : dynRevitTransactionNodeWithOneOutput
	{
		public API_PointCloudInstance_FilterAction()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			OutPortData.Add(new PortData("out","The action taken based on the results of the selection filter applied to this point cloud.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.PointCloudInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.PointCloudInstance));
			var result = arg0.FilterAction;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetInstanceGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes a transformation of the geometric representation of the instance.")]
	public class API_GeometryInstance_GetInstanceGeometry : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
		///</summary>
		public API_GeometryInstance_GetInstanceGeometry()
		{
			base_type = typeof(Autodesk.Revit.DB.GeometryInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInstanceGeometry", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Computes a transformation of the geometric representation of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetInstanceGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the geometric representation of the instance.")]
	public class API_GeometryInstance_GetInstanceGeometry_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
		///</summary>
		public API_GeometryInstance_GetInstanceGeometry_1()
		{
			base_type = typeof(Autodesk.Revit.DB.GeometryInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetInstanceGeometry", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Computes the geometric representation of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetSymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes a transformation of the geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_GetSymbolGeometry : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
		///</summary>
		public API_GeometryInstance_GetSymbolGeometry()
		{
			base_type = typeof(Autodesk.Revit.DB.GeometryInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetSymbolGeometry", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The transformation to apply to the geometry.",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Computes a transformation of the geometric representation of the symbol which generates this instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetSymbolGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_GetSymbolGeometry_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
		///</summary>
		public API_GeometryInstance_GetSymbolGeometry_1()
		{
			base_type = typeof(Autodesk.Revit.DB.GeometryInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetSymbolGeometry", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Computes the geometric representation of the symbol which generates this instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_GeometryInstance_SymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_SymbolGeometry : dynRevitTransactionNodeWithOneOutput
	{
		public API_GeometryInstance_SymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The geometric representation of the symbol which generates this instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = arg0.SymbolGeometry;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_GeometryInstance_Symbol")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The symbol element that this object is referring to.")]
	public class API_GeometryInstance_Symbol : dynRevitTransactionNodeWithOneOutput
	{
		public API_GeometryInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The symbol element that this object is referring to.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = arg0.Symbol;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_GeometryInstance_Transform")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.")]
	public class API_GeometryInstance_Transform : dynRevitTransactionNodeWithOneOutput
	{
		public API_GeometryInstance_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.GeometryInstance)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.GeometryInstance));
			var result = arg0.Transform;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Extrusion_EndOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the end of the extrusion relative to the sketch plane.")]
	public class API_Extrusion_EndOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Extrusion_EndOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the end of the extrusion relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = arg0.EndOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Extrusion_StartOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the start of the extrusion relative to the sketch plane.")]
	public class API_Extrusion_StartOffset : dynRevitTransactionNodeWithOneOutput
	{
		public API_Extrusion_StartOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the start of the extrusion relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = arg0.StartOffset;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Extrusion_Sketch")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Sketch of the Extrusion.")]
	public class API_Extrusion_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Extrusion_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Extrusion.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Extrusion)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Extrusion));
			var result = arg0.Sketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferencePointArray
	///</summary>
	[NodeName("API_Application_NewReferencePointArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store ReferencePoint objects.")]
	public class API_Application_NewReferencePointArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferencePointArray
		///</summary>
		public API_Application_NewReferencePointArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePointArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates an empty array that can store ReferencePoint objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointRelativeToPoint
	///</summary>
	[NodeName("API_Application_NewPointRelativeToPoint")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.")]
	public class API_Application_NewPointRelativeToPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointRelativeToPoint
		///</summary>
		public API_Application_NewPointRelativeToPoint()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointRelativeToPoint", false, new Type[]{typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference of the host point.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdgeEdgeIntersection
	///</summary>
	[NodeName("API_Application_NewPointOnEdgeEdgeIntersection")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnEdgeEdgeIntersection object which is used to define the placement of a ReferencePoint given two references to edge.")]
	public class API_Application_NewPointOnEdgeEdgeIntersection : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdgeEdgeIntersection
		///</summary>
		public API_Application_NewPointOnEdgeEdgeIntersection()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointOnEdgeEdgeIntersection", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The first edge reference.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "The second edge reference.",typeof(Autodesk.Revit.DB.Reference)));
			OutPortData.Add(new PortData("out","Construct a PointOnEdgeEdgeIntersection object which is used to define the placement of a ReferencePoint given two references to edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnFace
	///</summary>
	[NodeName("API_Application_NewPointOnFace")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnFace object which is used to define the placement of a ReferencePoint given a reference and a location on the face.")]
	public class API_Application_NewPointOnFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnFace
		///</summary>
		public API_Application_NewPointOnFace()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointOnFace", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference whose face the object will be created on.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("uv", "A 2-dimensional position.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Construct a PointOnFace object which is used to define the placement of a ReferencePoint given a reference and a location on the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnPlane
	///</summary>
	[NodeName("API_Application_NewPointOnPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnPlane object which is used to define the placement of a ReferencePoint from its property values.")]
	public class API_Application_NewPointOnPlane : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnPlane
		///</summary>
		public API_Application_NewPointOnPlane()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointOnPlane", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.UV),typeof(Autodesk.Revit.DB.UV),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to some planein the document. (Note: the reference must satisfyIsValidPlaneReference(), but this is not checked until this PointOnPlane objectis assigned to a ReferencePoint.)",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("uv", "Coordinates of the point's projection onto the plane;see the Position property.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("uv", "The direction of the point'sX-coordinate vector in the plane's coordinates; see the XVec property. Optional;default value is (1, 0).",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "Signed offset from the plane; see the Offset property.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Construct a PointOnPlane object which is used to define the placement of a ReferencePoint from its property values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdge
	///</summary>
	[NodeName("API_Application_NewPointOnEdge")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointOnEdge object which is used to define the placement of a ReferencePoint.")]
	public class API_Application_NewPointOnEdge : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdge
		///</summary>
		public API_Application_NewPointOnEdge()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointOnEdge", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.PointLocationOnCurve)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "The reference whose edge the object will be created on.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("loc", "The location on the edge.",typeof(Autodesk.Revit.DB.PointLocationOnCurve)));
			OutPortData.Add(new PortData("out","Create a PointOnEdge object which is used to define the placement of a ReferencePoint.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilySymbolProfile
	///</summary>
	[NodeName("API_Application_NewFamilySymbolProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new FamilySymbolProfile object.")]
	public class API_Application_NewFamilySymbolProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilySymbolProfile
		///</summary>
		public API_Application_NewFamilySymbolProfile()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilySymbolProfile", false, new Type[]{typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("fs", "The family symbol of the Profile.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Creates a new FamilySymbolProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveLoopsProfile
	///</summary>
	[NodeName("API_Application_NewCurveLoopsProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurveLoopsProfile object.")]
	public class API_Application_NewCurveLoopsProfile : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveLoopsProfile
		///</summary>
		public API_Application_NewCurveLoopsProfile()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurveLoopsProfile", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArrArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The curve loops of the Profile.",typeof(Autodesk.Revit.DB.CurveArrArray)));
			OutPortData.Add(new PortData("out","Creates a new CurveLoopsProfile object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementId
	///</summary>
	[NodeName("API_Application_NewElementId")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Autodesk::Revit::DB::ElementId^ object.")]
	public class API_Application_NewElementId : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementId
		///</summary>
		public API_Application_NewElementId()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElementId", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new Autodesk::Revit::DB::ElementId^ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewAreaCreationData
	///</summary>
	[NodeName("API_Application_NewAreaCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of Area for batch creation.")]
	public class API_Application_NewAreaCreationData : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewAreaCreationData
		///</summary>
		public API_Application_NewAreaCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.ViewPlan),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view of area element.",typeof(Autodesk.Revit.DB.ViewPlan)));
			InPortData.Add(new PortData("uv", "A point which lies in an enclosed region of area boundary where the new area will reside.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of Area for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Face),typeof(Autodesk.Revit.DB.Line),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("crv", "A line on the face defining where the symbol is to be placed.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Face),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("f", "A face of a geometry object.",typeof(Autodesk.Revit.DB.Face)));
			InPortData.Add(new PortData("xyz", "Point on the face where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector that defines the direction of the family instance.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("xyz", "A vector that dictates the direction of certain family instances.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_3")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_4")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_4 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_4()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("el", "The object into which the family instance is to be inserted, often known as the host.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_5")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_5 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_5()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_6")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_6 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_6()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The curve where the instance is based.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("l", "A Level object that is used as the base level for the object.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("st", "If structural then specify the type of the component.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_7")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_7 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
		///</summary>
		public API_Application_NewFamilyInstanceCreationData_7()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstanceCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.FamilySymbol),typeof(Autodesk.Revit.DB.Structure.StructuralType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The physical location where the instance is to be placed.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("fs", "A FamilySymbol object that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.FamilySymbol)));
			InPortData.Add(new PortData("st", "Specify if the family instance is structural.",typeof(Autodesk.Revit.DB.Structure.StructuralType)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSpaceSet
	///</summary>
	[NodeName("API_Application_NewSpaceSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a space set.")]
	public class API_Application_NewSpaceSet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSpaceSet
		///</summary>
		public API_Application_NewSpaceSet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaceSet", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a space set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCombinationArray
	///</summary>
	[NodeName("API_Application_NewLoadCombinationArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination array.")]
	public class API_Application_NewLoadCombinationArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCombinationArray
		///</summary>
		public API_Application_NewLoadCombinationArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadCombinationArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCombination array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadUsageArray
	///</summary>
	[NodeName("API_Application_NewLoadUsageArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage array.")]
	public class API_Application_NewLoadUsageArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadUsageArray
		///</summary>
		public API_Application_NewLoadUsageArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadUsageArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadUsage array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCaseArray
	///</summary>
	[NodeName("API_Application_NewLoadCaseArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase array.")]
	public class API_Application_NewLoadCaseArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCaseArray
		///</summary>
		public API_Application_NewLoadCaseArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLoadCaseArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a LoadCase array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewViewSet
	///</summary>
	[NodeName("API_Application_NewViewSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a View set.")]
	public class API_Application_NewViewSet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewViewSet
		///</summary>
		public API_Application_NewViewSet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewViewSet", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a View set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewIntersectionResultArray
	///</summary>
	[NodeName("API_Application_NewIntersectionResultArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an IntersectionResult array.")]
	public class API_Application_NewIntersectionResultArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewIntersectionResultArray
		///</summary>
		public API_Application_NewIntersectionResultArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewIntersectionResultArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of an IntersectionResult array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFaceArray
	///</summary>
	[NodeName("API_Application_NewFaceArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a face array.")]
	public class API_Application_NewFaceArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFaceArray
		///</summary>
		public API_Application_NewFaceArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFaceArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a face array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferenceArray
	///</summary>
	[NodeName("API_Application_NewReferenceArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a reference array.")]
	public class API_Application_NewReferenceArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferenceArray
		///</summary>
		public API_Application_NewReferenceArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferenceArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a reference array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDoubleArray
	///</summary>
	[NodeName("API_Application_NewDoubleArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a double array.")]
	public class API_Application_NewDoubleArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDoubleArray
		///</summary>
		public API_Application_NewDoubleArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDoubleArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a double array.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVolumeCalculationOptions
	///</summary>
	[NodeName("API_Application_NewVolumeCalculationOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates options related to room volume and area computations.")]
	public class API_Application_NewVolumeCalculationOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVolumeCalculationOptions
		///</summary>
		public API_Application_NewVolumeCalculationOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewVolumeCalculationOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates options related to room volume and area computations.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLImportOptions
	///</summary>
	[NodeName("API_Application_NewGBXMLImportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Import options.")]
	public class API_Application_NewGBXMLImportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLImportOptions
		///</summary>
		public API_Application_NewGBXMLImportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGBXMLImportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates Green-Building XML Import options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewImageImportOptions
	///</summary>
	[NodeName("API_Application_NewImageImportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Image Import options.")]
	public class API_Application_NewImageImportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewImageImportOptions
		///</summary>
		public API_Application_NewImageImportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewImageImportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates Image Import options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBuildingSiteExportOptions
	///</summary>
	[NodeName("API_Application_NewBuildingSiteExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Building Site Export options.")]
	public class API_Application_NewBuildingSiteExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBuildingSiteExportOptions
		///</summary>
		public API_Application_NewBuildingSiteExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBuildingSiteExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates Building Site Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFBXExportOptions
	///</summary>
	[NodeName("API_Application_NewFBXExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates 3D-Studio Max (FBX) Export options.")]
	public class API_Application_NewFBXExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFBXExportOptions
		///</summary>
		public API_Application_NewFBXExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFBXExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates 3D-Studio Max (FBX) Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLExportOptions
	///</summary>
	[NodeName("API_Application_NewGBXMLExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Export options.")]
	public class API_Application_NewGBXMLExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLExportOptions
		///</summary>
		public API_Application_NewGBXMLExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGBXMLExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates Green-Building XML Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFXExportOptions
	///</summary>
	[NodeName("API_Application_NewDWFXExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWFX Export options.")]
	public class API_Application_NewDWFXExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFXExportOptions
		///</summary>
		public API_Application_NewDWFXExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDWFXExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates DWFX Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFExportOptions
	///</summary>
	[NodeName("API_Application_NewDWFExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWF Export options.")]
	public class API_Application_NewDWFExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFExportOptions
		///</summary>
		public API_Application_NewDWFExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDWFExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates DWF Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSATExportOptions
	///</summary>
	[NodeName("API_Application_NewSATExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates SAT Export options.")]
	public class API_Application_NewSATExportOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSATExportOptions
		///</summary>
		public API_Application_NewSATExportOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSATExportOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates SAT Export options.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object by copying the supplied UV object.")]
	public class API_Application_NewUV : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
		///</summary>
		public API_Application_NewUV()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewUV", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The supplied UV object",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a UV object by copying the supplied UV object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object representing coordinates in 2-space with supplied values.")]
	public class API_Application_NewUV_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
		///</summary>
		public API_Application_NewUV_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewUV", false, new Type[]{typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a UV object representing coordinates in 2-space with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object at the origin.")]
	public class API_Application_NewUV_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
		///</summary>
		public API_Application_NewUV_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewUV", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a UV object at the origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object by copying the supplied XYZ object.")]
	public class API_Application_NewXYZ : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
		///</summary>
		public API_Application_NewXYZ()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewXYZ", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The supplied XYZ object",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a XYZ object by copying the supplied XYZ object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object representing coordinates in 3-space with supplied values.")]
	public class API_Application_NewXYZ_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
		///</summary>
		public API_Application_NewXYZ_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewXYZ", false, new Type[]{typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The third coordinate.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a XYZ object representing coordinates in 3-space with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object at the origin.")]
	public class API_Application_NewXYZ_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
		///</summary>
		public API_Application_NewXYZ_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewXYZ", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a XYZ object at the origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
	///</summary>
	[NodeName("API_Application_NewBoundingBoxUV")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a two-dimensional rectangle with supplied values.")]
	public class API_Application_NewBoundingBoxUV : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
		///</summary>
		public API_Application_NewBoundingBoxUV()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBoundingBoxUV", false, new Type[]{typeof(System.Double),typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate of min.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate of min.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The first coordinate of max.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate of max.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a two-dimensional rectangle with supplied values.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
	///</summary>
	[NodeName("API_Application_NewBoundingBoxUV_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty two-dimensional rectangle.")]
	public class API_Application_NewBoundingBoxUV_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
		///</summary>
		public API_Application_NewBoundingBoxUV_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBoundingBoxUV", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates an empty two-dimensional rectangle.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxXYZ
	///</summary>
	[NodeName("API_Application_NewBoundingBoxXYZ")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a three-dimensional rectangular box.")]
	public class API_Application_NewBoundingBoxXYZ : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxXYZ
		///</summary>
		public API_Application_NewBoundingBoxXYZ()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewBoundingBoxXYZ", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a three-dimensional rectangular box.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
	///</summary>
	[NodeName("API_Application_NewHermiteSpline")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with specified tangency at its endpoints.")]
	public class API_Application_NewHermiteSpline : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
		///</summary>
		public API_Application_NewHermiteSpline()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewHermiteSpline", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the start of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Tangent vector at the end of the spline. Can be null, in which case the tangent is computed from the control points.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with specified tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
	///</summary>
	[NodeName("API_Application_NewHermiteSpline_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with default tangency at its endpoints.")]
	public class API_Application_NewHermiteSpline_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
		///</summary>
		public API_Application_NewHermiteSpline_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewHermiteSpline", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the Hermite spline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("b", "True if the Hermite spline is to be periodic.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a Hermite spline with default tangency at its endpoints.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
	///</summary>
	[NodeName("API_Application_NewNurbSpline")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.")]
	public class API_Application_NewNurbSpline : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
		///</summary>
		public API_Application_NewNurbSpline()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewNurbSpline", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<System.Double>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The weights of the nurbSpline.",typeof(System.Collections.Generic.IList<System.Double>)));
			OutPortData.Add(new PortData("out","Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
	///</summary>
	[NodeName("API_Application_NewNurbSpline_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric nurbSpline object.")]
	public class API_Application_NewNurbSpline_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
		///</summary>
		public API_Application_NewNurbSpline_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewNurbSpline", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.DoubleArray),typeof(Autodesk.Revit.DB.DoubleArray),typeof(System.Int32),typeof(System.Boolean),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the nurbSpline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("arr", "The weights of the nurbSpline.",typeof(Autodesk.Revit.DB.DoubleArray)));
			InPortData.Add(new PortData("arr", "The knots of the nurbSpline.",typeof(Autodesk.Revit.DB.DoubleArray)));
			InPortData.Add(new PortData("i", "The degree of the nurbSpline.",typeof(System.Int32)));
			InPortData.Add(new PortData("b", "The nurbSpline is closed or not.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "The nurbSpline is rational or not rational.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new geometric nurbSpline object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewEllipse
	///</summary>
	[NodeName("API_Application_NewEllipse")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric ellipse object.")]
	public class API_Application_NewEllipse : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewEllipse
		///</summary>
		public API_Application_NewEllipse()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewEllipse", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The center of the ellipse.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The x vector radius of the ellipse. Should be > 0.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The y vector radius of the ellipse. Should be > 0.",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The x axis to define the ellipse plane.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The y axis to define the ellipse plane. xVec must be orthogonal with yVec.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The raw parameter value at the start of the ellipse. Should be greater than or equal to -2PI and less than Param1.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The raw parameter value at the end of the ellipse. Should be greater than Param0 and less than or equal to 2*PI.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new geometric ellipse object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProjectPosition
	///</summary>
	[NodeName("API_Application_NewProjectPosition")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new project position object.")]
	public class API_Application_NewProjectPosition : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProjectPosition
		///</summary>
		public API_Application_NewProjectPosition()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewProjectPosition", false, new Type[]{typeof(System.Double),typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("n", "East to West offset in feet.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "North to South offset in feet.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Elevation above sea level in feet.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Rotation angle away from true north in the range of -PI to +PI.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new project position object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
	///</summary>
	[NodeName("API_Application_NewArc")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on three points.")]
	public class API_Application_NewArc : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
		///</summary>
		public API_Application_NewArc()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArc", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The start point of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A point on the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on three points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
	///</summary>
	[NodeName("API_Application_NewArc_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on center, radius, unit vectors, and angles.")]
	public class API_Application_NewArc_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
		///</summary>
		public API_Application_NewArc_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArc", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The center of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The radius of the arc.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The x axis to define the arc plane. Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The y axis to define the arc plane. Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on center, radius, unit vectors, and angles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPoint
	///</summary>
	[NodeName("API_Application_NewPoint")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric point object.")]
	public class API_Application_NewPoint : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPoint
		///</summary>
		public API_Application_NewPoint()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPoint", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The coordinates of the point.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric point object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane from a loop of planar curves.")]
	public class API_Application_NewPlane : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
		///</summary>
		public API_Application_NewPlane()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPlane", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "The closed loop of planar curves to locate plane.",typeof(Autodesk.Revit.DB.CurveArray)));
			OutPortData.Add(new PortData("out","Creates a new geometric plane from a loop of planar curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on a normal vector and an origin.")]
	public class API_Application_NewPlane_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
		///</summary>
		public API_Application_NewPlane_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPlane", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "Z vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric plane object based on a normal vector and an origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on two coordinate vectors and an origin.")]
	public class API_Application_NewPlane_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
		///</summary>
		public API_Application_NewPlane_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPlane", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "X vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Y vector of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "Origin of the plane coordinate system.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric plane object based on two coordinate vectors and an origin.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewColor
	///</summary>
	[NodeName("API_Application_NewColor")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new color object.")]
	public class API_Application_NewColor : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewColor
		///</summary>
		public API_Application_NewColor()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewColor", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a new color object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCombinableElementArray
	///</summary>
	[NodeName("API_Application_NewCombinableElementArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold combinable element objects.")]
	public class API_Application_NewCombinableElementArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCombinableElementArray
		///</summary>
		public API_Application_NewCombinableElementArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCombinableElementArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns an array that can hold combinable element objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPairArray
	///</summary>
	[NodeName("API_Application_NewVertexIndexPairArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold VertexIndexPair objects.")]
	public class API_Application_NewVertexIndexPairArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPairArray
		///</summary>
		public API_Application_NewVertexIndexPairArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewVertexIndexPairArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns an array that can hold VertexIndexPair objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPair
	///</summary>
	[NodeName("API_Application_NewVertexIndexPair")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new VertexIndexPair object.")]
	public class API_Application_NewVertexIndexPair : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPair
		///</summary>
		public API_Application_NewVertexIndexPair()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewVertexIndexPair", false, new Type[]{typeof(System.Int32),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("i", "The index of the vertex pair from the top profile of a blend.",typeof(System.Int32)));
			InPortData.Add(new PortData("i", "The index of the vertex pair from the bottom profile of a blend.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Creates a new VertexIndexPair object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArrArray
	///</summary>
	[NodeName("API_Application_NewCurveArrArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curve loops.")]
	public class API_Application_NewCurveArrArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArrArray
		///</summary>
		public API_Application_NewCurveArrArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurveArrArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates an empty array that can store geometric curve loops.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArray
	///</summary>
	[NodeName("API_Application_NewCurveArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curves.")]
	public class API_Application_NewCurveArray : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArray
		///</summary>
		public API_Application_NewCurveArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurveArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates an empty array that can store geometric curves.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGeometryOptions
	///</summary>
	[NodeName("API_Application_NewGeometryOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object to specify user preferences in parsing of geometry.")]
	public class API_Application_NewGeometryOptions : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGeometryOptions
		///</summary>
		public API_Application_NewGeometryOptions()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGeometryOptions", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates an object to specify user preferences in parsing of geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineUnbound
	///</summary>
	[NodeName("API_Application_NewLineUnbound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unbounded geometric line object.")]
	public class API_Application_NewLineUnbound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineUnbound
		///</summary>
		public API_Application_NewLineUnbound()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineUnbound", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "A point through which the line will pass.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A vector for the direction of the line.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new unbounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineBound
	///</summary>
	[NodeName("API_Application_NewLineBound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bounded geometric line object.")]
	public class API_Application_NewLineBound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineBound
		///</summary>
		public API_Application_NewLineBound()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLineBound", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "A start point for the line.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "An end point for the line.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new bounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLine
	///</summary>
	[NodeName("API_Application_NewLine")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bound or unbounded geometric line object.")]
	public class API_Application_NewLine : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLine
		///</summary>
		public API_Application_NewLine()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewLine", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "A start point or a point through which the line will pass.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "An end point of a vector for the direction of the line.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Set to True if you wish the line to be bound or False is the line is to be infinite.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new bound or unbounded geometric line object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementSet
	///</summary>
	[NodeName("API_Application_NewElementSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding elements.")]
	public class API_Application_NewElementSet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementSet
		///</summary>
		public API_Application_NewElementSet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElementSet", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a set specifically for holding elements.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
	///</summary>
	[NodeName("API_Application_NewTypeBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type binding object containing the categories passed as a parameter.")]
	public class API_Application_NewTypeBinding : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
		///</summary>
		public API_Application_NewTypeBinding()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTypeBinding", false, new Type[]{typeof(Autodesk.Revit.DB.CategorySet)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(Autodesk.Revit.DB.CategorySet)));
			OutPortData.Add(new PortData("out","Creates a new type binding object containing the categories passed as a parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
	///</summary>
	[NodeName("API_Application_NewTypeBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty type binding object.")]
	public class API_Application_NewTypeBinding_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
		///</summary>
		public API_Application_NewTypeBinding_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTypeBinding", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new empty type binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
	///</summary>
	[NodeName("API_Application_NewInstanceBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance binding object containing the categories passed as a parameter.")]
	public class API_Application_NewInstanceBinding : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
		///</summary>
		public API_Application_NewInstanceBinding()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewInstanceBinding", false, new Type[]{typeof(Autodesk.Revit.DB.CategorySet)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("cats", "A set of categories that will be added to the binding.",typeof(Autodesk.Revit.DB.CategorySet)));
			OutPortData.Add(new PortData("out","Creates a new instance binding object containing the categories passed as a parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
	///</summary>
	[NodeName("API_Application_NewInstanceBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty instance binding object.")]
	public class API_Application_NewInstanceBinding_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
		///</summary>
		public API_Application_NewInstanceBinding_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewInstanceBinding", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new empty instance binding object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCategorySet
	///</summary>
	[NodeName("API_Application_NewCategorySet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding category objects.")]
	public class API_Application_NewCategorySet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCategorySet
		///</summary>
		public API_Application_NewCategorySet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCategorySet", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new instance of a set specifically for holding category objects.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.ToString
	///</summary>
	[NodeName("API_UV_ToString")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets formatted string showing (U, V) with values formatted to 9 decimal places.")]
	public class API_UV_ToString : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.ToString
		///</summary>
		public API_UV_ToString()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ToString", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets formatted string showing (U, V) with values formatted to 9 decimal places.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.AngleTo
	///</summary>
	[NodeName("API_UV_AngleTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector.")]
	public class API_UV_AngleTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.AngleTo
		///</summary>
		public API_UV_AngleTo()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AngleTo", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The specified vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Returns the angle between this vector and the specified vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.DistanceTo
	///</summary>
	[NodeName("API_UV_DistanceTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the distance from this 2-D point to the specified 2-D point.")]
	public class API_UV_DistanceTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.DistanceTo
		///</summary>
		public API_UV_DistanceTo()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DistanceTo", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The specified point.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Returns the distance from this 2-D point to the specified 2-D point.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
	///</summary>
	[NodeName("API_UV_IsAlmostEqualTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this 2-D vector and the specified 2-D vector are the same within a specified tolerance.")]
	public class API_UV_IsAlmostEqualTo : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
		///</summary>
		public API_UV_IsAlmostEqualTo()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAlmostEqualTo", false, new Type[]{typeof(Autodesk.Revit.DB.UV),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.UV)));
			InPortData.Add(new PortData("n", "The tolerance for equality check.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Determines whether this 2-D vector and the specified 2-D vector are the same within a specified tolerance.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
	///</summary>
	[NodeName("API_UV_IsAlmostEqualTo_1")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this 2-D vector and the specified 2-D vector are the same within the tolerance (1.0e-09).")]
	public class API_UV_IsAlmostEqualTo_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
		///</summary>
		public API_UV_IsAlmostEqualTo_1()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsAlmostEqualTo", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to compare with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Determines whether this 2-D vector and the specified 2-D vector are the same within the tolerance (1.0e-09).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Divide
	///</summary>
	[NodeName("API_UV_Divide")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Divides this 2-D vector by the specified value and returns the result.")]
	public class API_UV_Divide : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Divide
		///</summary>
		public API_UV_Divide()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Divide", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The value to divide this vector by.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Divides this 2-D vector by the specified value and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Multiply
	///</summary>
	[NodeName("API_UV_Multiply")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this 2-D vector by the specified value and returns the result.")]
	public class API_UV_Multiply : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Multiply
		///</summary>
		public API_UV_Multiply()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Multiply", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The value to multiply with this vector.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Multiplies this 2-D vector by the specified value and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Negate
	///</summary>
	[NodeName("API_UV_Negate")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Negates this 2-D vector.")]
	public class API_UV_Negate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Negate
		///</summary>
		public API_UV_Negate()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Negate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Negates this 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Subtract
	///</summary>
	[NodeName("API_UV_Subtract")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Subtracts the specified 2-D vector from this 2-D vector and returns the result.")]
	public class API_UV_Subtract : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Subtract
		///</summary>
		public API_UV_Subtract()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Subtract", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to subtract from this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Subtracts the specified 2-D vector from this 2-D vector and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Add
	///</summary>
	[NodeName("API_UV_Add")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds the specified 2-D vector to this 2-D vector and returns the result.")]
	public class API_UV_Add : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Add
		///</summary>
		public API_UV_Add()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Add", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to add to this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Adds the specified 2-D vector to this 2-D vector and returns the result.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.CrossProduct
	///</summary>
	[NodeName("API_UV_CrossProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cross product of this 2-D vector and the specified 2-D vector.")]
	public class API_UV_CrossProduct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.CrossProduct
		///</summary>
		public API_UV_CrossProduct()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CrossProduct", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The cross product of this 2-D vector and the specified 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.DotProduct
	///</summary>
	[NodeName("API_UV_DotProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The dot product of this 2-D vector and the specified 2-D vector.")]
	public class API_UV_DotProduct : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.DotProduct
		///</summary>
		public API_UV_DotProduct()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "DotProduct", false, new Type[]{typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("uv", "The vector to multiply with this vector.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","The dot product of this 2-D vector and the specified 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsUnitLength
	///</summary>
	[NodeName("API_UV_IsUnitLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value indicates whether this 2-D vector is of unit length.")]
	public class API_UV_IsUnitLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.IsUnitLength
		///</summary>
		public API_UV_IsUnitLength()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsUnitLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The boolean value indicates whether this 2-D vector is of unit length.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsZeroLength
	///</summary>
	[NodeName("API_UV_IsZeroLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value indicates whether this 2-D vector is a zero vector.")]
	public class API_UV_IsZeroLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.IsZeroLength
		///</summary>
		public API_UV_IsZeroLength()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsZeroLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The boolean value indicates whether this 2-D vector is a zero vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.GetLength
	///</summary>
	[NodeName("API_UV_GetLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The length of this 2-D vector.")]
	public class API_UV_GetLength : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.GetLength
		///</summary>
		public API_UV_GetLength()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetLength", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The length of this 2-D vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Normalize
	///</summary>
	[NodeName("API_UV_Normalize")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new UV whose coordinates are the normalized values from this vector.")]
	public class API_UV_Normalize : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.Normalize
		///</summary>
		public API_UV_Normalize()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Normalize", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a new UV whose coordinates are the normalized values from this vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
	///</summary>
	[NodeName("API_UV")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV with the supplied coordinates.")]
	public class API_UV : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
		///</summary>
		public API_UV()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "UV", true, new Type[]{typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The first coordinate.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The second coordinate.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a UV with the supplied coordinates.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
	///</summary>
	[NodeName("API_UV_1")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a default UV with the values (0, 0).")]
	public class API_UV_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
		///</summary>
		public API_UV_1()
		{
			base_type = typeof(Autodesk.Revit.DB.UV);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "UV", true, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a default UV with the values (0, 0).",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_UV_V")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the second coordinate.")]
	public class API_UV_V : dynRevitTransactionNodeWithOneOutput
	{
		public API_UV_V()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = arg0.V;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_UV_U")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the first coordinate.")]
	public class API_UV_U : dynRevitTransactionNodeWithOneOutput
	{
		public API_UV_U()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.UV)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.UV));
			var result = arg0.U;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_RevolvedFace_Curve")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Profile curve of the surface.")]
	public class API_RevolvedFace_Curve : dynRevitTransactionNodeWithOneOutput
	{
		public API_RevolvedFace_Curve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Profile curve of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = arg0.Curve;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_RevolvedFace_Axis")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_RevolvedFace_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public API_RevolvedFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = arg0.Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_RevolvedFace_Origin")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_RevolvedFace_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public API_RevolvedFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.RevolvedFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.RevolvedFace));
			var result = arg0.Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.ComputeDerivatives
	///</summary>
	[NodeName("API_Edge_ComputeDerivatives")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the edge at the specified parameter.")]
	public class API_Edge_ComputeDerivatives : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.ComputeDerivatives
		///</summary>
		public API_Edge_ComputeDerivatives()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeDerivatives", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Returns the vectors describing the edge at the specified parameter.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.GetEndPointReference
	///</summary>
	[NodeName("API_Edge_GetEndPointReference")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the start or the end point of the edge.")]
	public class API_Edge_GetEndPointReference : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.GetEndPointReference
		///</summary>
		public API_Edge_GetEndPointReference()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetEndPointReference", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("i", "Use 0 for the start point; 1 for the end point.",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the start or the end point of the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurveFollowingFace
	///</summary>
	[NodeName("API_Edge_AsCurveFollowingFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a curve that corresponds to this edge as oriented in its topological direction on the specified face.")]
	public class API_Edge_AsCurveFollowingFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurveFollowingFace
		///</summary>
		public API_Edge_AsCurveFollowingFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AsCurveFollowingFace", false, new Type[]{typeof(Autodesk.Revit.DB.Face)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("f", "Specifies the face, on which the curve will follow the topological direction of the edge.",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Returns a curve that corresponds to this edge as oriented in its topological direction on the specified face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurve
	///</summary>
	[NodeName("API_Edge_AsCurve")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a curve that corresponds to the edge's parametric orientation.")]
	public class API_Edge_AsCurve : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurve
		///</summary>
		public API_Edge_AsCurve()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "AsCurve", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a curve that corresponds to the edge's parametric orientation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.EvaluateOnFace
	///</summary>
	[NodeName("API_Edge_EvaluateOnFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the edge to produce UV coordinates on the face.")]
	public class API_Edge_EvaluateOnFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.EvaluateOnFace
		///</summary>
		public API_Edge_EvaluateOnFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "EvaluateOnFace", false, new Type[]{typeof(System.Double),typeof(Autodesk.Revit.DB.Face)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated, in [0,1].",typeof(System.Double)));
			InPortData.Add(new PortData("f", "The face on which to perform the evaluation. Must belong to the edge.",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Evaluates a parameter on the edge to produce UV coordinates on the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.Evaluate
	///</summary>
	[NodeName("API_Edge_Evaluate")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the edge.")]
	public class API_Edge_Evaluate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.Evaluate
		///</summary>
		public API_Edge_Evaluate()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Evaluate", false, new Type[]{typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("n", "The parameter to be evaluated, in [0,1].",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Evaluates a parameter on the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.TessellateOnFace
	///</summary>
	[NodeName("API_Edge_TessellateOnFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a polyline approximation to the edge in UV parameters of the face.")]
	public class API_Edge_TessellateOnFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.TessellateOnFace
		///</summary>
		public API_Edge_TessellateOnFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "TessellateOnFace", false, new Type[]{typeof(Autodesk.Revit.DB.Face)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("f", "The face on which to perform the tessellation. Must belong to the edge.",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Returns a polyline approximation to the edge in UV parameters of the face.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.Tessellate
	///</summary>
	[NodeName("API_Edge_Tessellate")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a polyline approximation to the edge.")]
	public class API_Edge_Tessellate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.Tessellate
		///</summary>
		public API_Edge_Tessellate()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Tessellate", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a polyline approximation to the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.GetFace
	///</summary>
	[NodeName("API_Edge_GetFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns one of the two faces that meet at the edge.")]
	public class API_Edge_GetFace : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Edge.GetFace
		///</summary>
		public API_Edge_GetFace()
		{
			base_type = typeof(Autodesk.Revit.DB.Edge);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetFace", false, new Type[]{typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge", typeof(object)));
			}
			InPortData.Add(new PortData("i", "The index of the face (0 or 1).",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Returns one of the two faces that meet at the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Edge_ApproximateLength")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the approximate length of the edge.")]
	public class API_Edge_ApproximateLength : dynRevitTransactionNodeWithOneOutput
	{
		public API_Edge_ApproximateLength()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns the approximate length of the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var result = arg0.ApproximateLength;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Edge_Reference")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the edge.")]
	public class API_Edge_Reference : dynRevitTransactionNodeWithOneOutput
	{
		public API_Edge_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the edge.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Edge)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Edge));
			var result = arg0.Reference;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
	///</summary>
	[NodeName("API_Arc_Create")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on three points.")]
	public class API_Arc_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
		///</summary>
		public API_Arc_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.Arc);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The start point of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The end point of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "A point on the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on three points.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
	///</summary>
	[NodeName("API_Arc_Create_1")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on plane, radius, and angles.")]
	public class API_Arc_Create_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
		///</summary>
		public API_Arc_Create_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Arc);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Plane),typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc", typeof(object)));
			}
			InPortData.Add(new PortData("p", "The plane which the arc resides. The plane's origin is the center of the arc.",typeof(Autodesk.Revit.DB.Plane)));
			InPortData.Add(new PortData("n", "The radius of the arc.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on plane, radius, and angles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
	///</summary>
	[NodeName("API_Arc_Create_2")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on center, radius, unit vectors, and angles.")]
	public class API_Arc_Create_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Arc.Create
		///</summary>
		public API_Arc_Create_2()
		{
			base_type = typeof(Autodesk.Revit.DB.Arc);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(System.Double),typeof(System.Double),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The center of the arc.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The radius of the arc.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The start angle of the arc (in radians).",typeof(System.Double)));
			InPortData.Add(new PortData("n", "The end angle of the arc (in radians).",typeof(System.Double)));
			InPortData.Add(new PortData("xyz", "The x axis to define the arc plane. Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The y axis to define the arc plane. Must be normalized.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on center, radius, unit vectors, and angles.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_Radius")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the radius of the arc.")]
	public class API_Arc_Radius : dynRevitTransactionNodeWithOneOutput
	{
		public API_Arc_Radius()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the radius of the arc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = arg0.Radius;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Arc_YDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Y direction.")]
	public class API_Arc_YDirection : dynRevitTransactionNodeWithOneOutput
	{
		public API_Arc_YDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the Y direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = arg0.YDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Arc_XDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the X direction.")]
	public class API_Arc_XDirection : dynRevitTransactionNodeWithOneOutput
	{
		public API_Arc_XDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the X direction.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = arg0.XDirection;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Arc_Normal")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal to the plane in which the arc is defined.")]
	public class API_Arc_Normal : dynRevitTransactionNodeWithOneOutput
	{
		public API_Arc_Normal()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the arc is defined.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = arg0.Normal;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Arc_Center")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the center of the arc.")]
	public class API_Arc_Center : dynRevitTransactionNodeWithOneOutput
	{
		public API_Arc_Center()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the center of the arc.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Arc)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Arc));
			var result = arg0.Center;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Line.CreateUnbound
	///</summary>
	[NodeName("API_Line_CreateUnbound")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an unbound linear curve.")]
	public class API_Line_CreateUnbound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Line.CreateUnbound
		///</summary>
		public API_Line_CreateUnbound()
		{
			base_type = typeof(Autodesk.Revit.DB.Line);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateUnbound", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The origin of the unbound line.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The direction of the unbound line.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new instance of an unbound linear curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Line.CreateBound
	///</summary>
	[NodeName("API_Line_CreateBound")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a bound linear curve.")]
	public class API_Line_CreateBound : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Line.CreateBound
		///</summary>
		public API_Line_CreateBound()
		{
			base_type = typeof(Autodesk.Revit.DB.Line);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CreateBound", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The first line endpoint.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The second line endpoint.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new instance of a bound linear curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Line_Direction")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the direction of the line.")]
	public class API_Line_Direction : dynRevitTransactionNodeWithOneOutput
	{
		public API_Line_Direction()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the direction of the line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Line));
			var result = arg0.Direction;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Line_Origin")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the origin of the line.")]
	public class API_Line_Origin : dynRevitTransactionNodeWithOneOutput
	{
		public API_Line_Origin()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the origin of the line.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Line)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Line));
			var result = arg0.Origin;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.ClearMaterialAspect
	///</summary>
	[NodeName("API_Material_ClearMaterialAspect")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes an aspect from the material.")]
	public class API_Material_ClearMaterialAspect : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.ClearMaterialAspect
		///</summary>
		public API_Material_ClearMaterialAspect()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ClearMaterialAspect", false, new Type[]{typeof(Autodesk.Revit.DB.MaterialAspect)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The material aspect.",typeof(Autodesk.Revit.DB.MaterialAspect)));
			OutPortData.Add(new PortData("out","Removes an aspect from the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspectByPropertySet
	///</summary>
	[NodeName("API_Material_SetMaterialAspectByPropertySet")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Set an aspect of the material to a shared property set.")]
	public class API_Material_SetMaterialAspectByPropertySet : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspectByPropertySet
		///</summary>
		public API_Material_SetMaterialAspectByPropertySet()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetMaterialAspectByPropertySet", false, new Type[]{typeof(Autodesk.Revit.DB.MaterialAspect),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The material aspect.",typeof(Autodesk.Revit.DB.MaterialAspect)));
			InPortData.Add(new PortData("val", "Identifier of a shared property set (an instance of PropertySetElement).",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Set an aspect of the material to a shared property set.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternColor
	///</summary>
	[NodeName("API_Material_GetCutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the color of the cut pattern for the material element.")]
	public class API_Material_GetCutPatternColor : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternColor
		///</summary>
		public API_Material_GetCutPatternColor()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCutPatternColor", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the color of the cut pattern for the material element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternId
	///</summary>
	[NodeName("API_Material_GetCutPatternId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the cut pattern id of the material.")]
	public class API_Material_GetCutPatternId : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternId
		///</summary>
		public API_Material_GetCutPatternId()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCutPatternId", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the cut pattern id of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.Duplicate
	///</summary>
	[NodeName("API_Material_Duplicate")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Duplicates the material")]
	public class API_Material_Duplicate : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.Duplicate
		///</summary>
		public API_Material_Duplicate()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Duplicate", false, new Type[]{typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("s", "Name of the new material.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Duplicates the material",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetRenderAppearance
	///</summary>
	[NodeName("API_Material_GetRenderAppearance")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the rendering appearance of the material.")]
	public class API_Material_GetRenderAppearance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.GetRenderAppearance
		///</summary>
		public API_Material_GetRenderAppearance()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetRenderAppearance", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Sets the rendering appearance of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetRenderAppearance
	///</summary>
	[NodeName("API_Material_SetRenderAppearance")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the rendering appearance of the material.")]
	public class API_Material_SetRenderAppearance : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.SetRenderAppearance
		///</summary>
		public API_Material_SetRenderAppearance()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetRenderAppearance", false, new Type[]{typeof(Autodesk.Revit.Utility.Asset)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The asset to set.",typeof(Autodesk.Revit.Utility.Asset)));
			OutPortData.Add(new PortData("out","Gets the rendering appearance of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.IsMaterialOrValidDefault
	///</summary>
	[NodeName("API_Material_IsMaterialOrValidDefault")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Validates whether the specified element id is a material element.")]
	public class API_Material_IsMaterialOrValidDefault : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.IsMaterialOrValidDefault
		///</summary>
		public API_Material_IsMaterialOrValidDefault()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsMaterialOrValidDefault", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("el", "An element which will be applied the material",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("val", "The element id to be checked.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Validates whether the specified element id is a material element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.Create
	///</summary>
	[NodeName("API_Material_Create")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new material.")]
	public class API_Material_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.Create
		///</summary>
		public API_Material_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which to create the material.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("s", "The name of the new material.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates a new material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Material_MaterialCagtegory")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The name of the material category, e.g. 'Wood'")]
	public class API_Material_MaterialCagtegory : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_MaterialCagtegory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The name of the material category, e.g. 'Wood'",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.MaterialCagtegory;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_MaterialClass")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The name of the general material type, e.g. 'Wood.'")]
	public class API_Material_MaterialClass : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_MaterialClass()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The name of the general material type, e.g. 'Wood.'",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.MaterialClass;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_ThermalAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The ElementId of the thermal PropertySetElement.")]
	public class API_Material_ThermalAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_ThermalAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the thermal PropertySetElement.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.ThermalAssetId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_StructuralAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The ElementId of the structural PropertySetElement.")]
	public class API_Material_StructuralAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_StructuralAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the structural PropertySetElement.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.StructuralAssetId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_AppearanceAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The ElementId of the AppearanceAssetElement.")]
	public class API_Material_AppearanceAssetId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_AppearanceAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the AppearanceAssetElement.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.AppearanceAssetId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_SurfacePattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the surface pattern element.")]
	public class API_Material_SurfacePattern : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_SurfacePattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Gets the surface pattern element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.SurfacePattern;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_CutPattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the cut pattern element.")]
	public class API_Material_CutPattern : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_CutPattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Gets the cut pattern element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.CutPattern;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_SurfacePatternId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets and sets the surface fill pattern element.")]
	public class API_Material_SurfacePatternId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_SurfacePatternId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Gets and sets the surface fill pattern element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.SurfacePatternId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_CutPatternId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets and sets the cut fill pattern element.")]
	public class API_Material_CutPatternId : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_CutPatternId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Gets and sets the cut fill pattern element.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.CutPatternId;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_CutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The color of the material cut pattern.")]
	public class API_Material_CutPatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_CutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color of the material cut pattern.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.CutPatternColor;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_SurfacePatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The color of the material surface pattern.")]
	public class API_Material_SurfacePatternColor : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_SurfacePatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color of the material surface pattern.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.SurfacePatternColor;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_Color")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The color of the material.")]
	public class API_Material_Color : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_Color()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.Color;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_Transparency")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transparency of the material.")]
	public class API_Material_Transparency : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_Transparency()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The transparency of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.Transparency;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_Smoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The smoothness of the material.")]
	public class API_Material_Smoothness : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_Smoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The smoothness of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.Smoothness;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_Shininess")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The shininess of the material.")]
	public class API_Material_Shininess : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_Shininess()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The shininess of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.Shininess;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_Glow")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the material can glow.")]
	public class API_Material_Glow : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_Glow()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Whether the material can glow.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.Glow;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Material_RenderAppearance")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the rendering appearance of the material.")]
	public class API_Material_RenderAppearance : dynRevitTransactionNodeWithOneOutput
	{
		public API_Material_RenderAppearance()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Gets the rendering appearance of the material.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Material)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Material));
			var result = arg0.RenderAppearance;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Solid.GetBoundingBox
	///</summary>
	[NodeName("API_Solid_GetBoundingBox")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves a box that circumscribes the solid geometry.")]
	public class API_Solid_GetBoundingBox : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Solid.GetBoundingBox
		///</summary>
		public API_Solid_GetBoundingBox()
		{
			base_type = typeof(Autodesk.Revit.DB.Solid);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetBoundingBox", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Retrieves a box that circumscribes the solid geometry.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Solid.IntersectWithCurve
	///</summary>
	[NodeName("API_Solid_IntersectWithCurve")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates and returns the intersection between a curve and this solid.")]
	public class API_Solid_IntersectWithCurve : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Solid.IntersectWithCurve
		///</summary>
		public API_Solid_IntersectWithCurve()
		{
			base_type = typeof(Autodesk.Revit.DB.Solid);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IntersectWithCurve", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.SolidCurveIntersectionOptions)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "The curve.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "The options.  If NULL, the default options will be used.",typeof(Autodesk.Revit.DB.SolidCurveIntersectionOptions)));
			OutPortData.Add(new PortData("out","Calculates and returns the intersection between a curve and this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Solid.ComputeCentroid
	///</summary>
	[NodeName("API_Solid_ComputeCentroid")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Centroid of this solid.")]
	public class API_Solid_ComputeCentroid : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Solid.ComputeCentroid
		///</summary>
		public API_Solid_ComputeCentroid()
		{
			base_type = typeof(Autodesk.Revit.DB.Solid);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ComputeCentroid", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns the Centroid of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Solid_Volume")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the signed volume of this solid.")]
	public class API_Solid_Volume : dynRevitTransactionNodeWithOneOutput
	{
		public API_Solid_Volume()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the signed volume of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = arg0.Volume;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Solid_SurfaceArea")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the total surface area of this solid.")]
	public class API_Solid_SurfaceArea : dynRevitTransactionNodeWithOneOutput
	{
		public API_Solid_SurfaceArea()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the total surface area of this solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = arg0.SurfaceArea;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Solid_Faces")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The faces that belong to the solid.")]
	public class API_Solid_Faces : dynRevitTransactionNodeWithOneOutput
	{
		public API_Solid_Faces()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The faces that belong to the solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = arg0.Faces;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Solid_Edges")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The edges that belong to the solid.")]
	public class API_Solid_Edges : dynRevitTransactionNodeWithOneOutput
	{
		public API_Solid_Edges()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The edges that belong to the solid.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Solid)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Solid));
			var result = arg0.Edges;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Profile.Clone
	///</summary>
	[NodeName("API_Profile_Clone")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this profile.")]
	public class API_Profile_Clone : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Profile.Clone
		///</summary>
		public API_Profile_Clone()
		{
			base_type = typeof(Autodesk.Revit.DB.Profile);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Clone", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns a copy of this profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Profile_Curves")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve the curves that make up the boundary of the profile.")]
	public class API_Profile_Curves : dynRevitTransactionNodeWithOneOutput
	{
		public API_Profile_Curves()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Retrieve the curves that make up the boundary of the profile.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Profile)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Profile));
			var result = arg0.Curves;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Profile_Filled")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get or set whether the profile is filled.")]
	public class API_Profile_Filled : dynRevitTransactionNodeWithOneOutput
	{
		public API_Profile_Filled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Get or set whether the profile is filled.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Profile)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Profile));
			var result = arg0.Filled;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.ChangeToReferenceLine
	///</summary>
	[NodeName("API_ModelCurve_ChangeToReferenceLine")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Changes this curve to a reference curve.")]
	public class API_ModelCurve_ChangeToReferenceLine : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.ChangeToReferenceLine
		///</summary>
		public API_ModelCurve_ChangeToReferenceLine()
		{
			base_type = typeof(Autodesk.Revit.DB.ModelCurve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "ChangeToReferenceLine", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Changes this curve to a reference curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.GetVisibility
	///</summary>
	[NodeName("API_ModelCurve_GetVisibility")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the model curve in a family document.")]
	public class API_ModelCurve_GetVisibility : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.GetVisibility
		///</summary>
		public API_ModelCurve_GetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.ModelCurve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetVisibility", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the visibility for the model curve in a family document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_ModelCurve_IsReferenceLine")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates if this curve is a reference curve.")]
	public class API_ModelCurve_IsReferenceLine : dynRevitTransactionNodeWithOneOutput
	{
		public API_ModelCurve_IsReferenceLine()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","Indicates if this curve is a reference curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = arg0.IsReferenceLine;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ModelCurve_TrussCurveType")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The truss curve type of this model curve.")]
	public class API_ModelCurve_TrussCurveType : dynRevitTransactionNodeWithOneOutput
	{
		public API_ModelCurve_TrussCurveType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The truss curve type of this model curve.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = arg0.TrussCurveType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_ModelCurve_Subcategory")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The subcategory.")]
	public class API_ModelCurve_Subcategory : dynRevitTransactionNodeWithOneOutput
	{
		public API_ModelCurve_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.ModelCurve)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.ModelCurve));
			var result = arg0.Subcategory;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Revolution_Axis")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Axis of the Revolution.")]
	public class API_Revolution_Axis : dynRevitTransactionNodeWithOneOutput
	{
		public API_Revolution_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Axis of the Revolution.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = arg0.Axis;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Revolution_EndAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The end angle of the revolution relative to the sketch plane.")]
	public class API_Revolution_EndAngle : dynRevitTransactionNodeWithOneOutput
	{
		public API_Revolution_EndAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The end angle of the revolution relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = arg0.EndAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Revolution_StartAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The start angle of the revolution relative to the sketch plane.")]
	public class API_Revolution_StartAngle : dynRevitTransactionNodeWithOneOutput
	{
		public API_Revolution_StartAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The start angle of the revolution relative to the sketch plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = arg0.StartAngle;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Revolution_Sketch")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Sketch of the Revolution.")]
	public class API_Revolution_Sketch : dynRevitTransactionNodeWithOneOutput
	{
		public API_Revolution_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Revolution.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Revolution)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Revolution));
			var result = arg0.Sketch;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.Create
	///</summary>
	[NodeName("API_NurbSpline_Create")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.")]
	public class API_NurbSpline_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.Create
		///</summary>
		public API_NurbSpline_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.NurbSpline);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<System.Double>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the NURBSpline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The weights of the NURBSpline.",typeof(System.Collections.Generic.IList<System.Double>)));
			OutPortData.Add(new PortData("out","Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.Create
	///</summary>
	[NodeName("API_NurbSpline_Create_1")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric NURBSpline object.")]
	public class API_NurbSpline_Create_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.Create
		///</summary>
		public API_NurbSpline_Create_1()
		{
			base_type = typeof(Autodesk.Revit.DB.NurbSpline);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(System.Collections.Generic.IList<System.Double>),typeof(System.Collections.Generic.IList<System.Double>),typeof(System.Int32),typeof(System.Boolean),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "The control points of the NURBSpline.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("lst", "The weights of the NURBSpline.",typeof(System.Collections.Generic.IList<System.Double>)));
			InPortData.Add(new PortData("lst", "The knots of the NURBSpline.",typeof(System.Collections.Generic.IList<System.Double>)));
			InPortData.Add(new PortData("i", "The degree of the NURBSpline.",typeof(System.Int32)));
			InPortData.Add(new PortData("b", "True if the NURBSpline should be closed, false otherwise.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "True if the NURBSpline is rational, false if it is irrational.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new geometric NURBSpline object.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_Knots")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Return/set the knots of the nurb spline.")]
	public class API_NurbSpline_Knots : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_Knots()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the knots of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.Knots;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_NurbSpline_Weights")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the weights of the nurb spline.")]
	public class API_NurbSpline_Weights : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_Weights()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the weights of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.Weights;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_NurbSpline_CtrlPoints")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the control points of the nurb spline.")]
	public class API_NurbSpline_CtrlPoints : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_CtrlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the control points of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.CtrlPoints;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_NurbSpline_Degree")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the degree of the nurb spline.")]
	public class API_NurbSpline_Degree : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_Degree()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the degree of the nurb spline.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.Degree;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_NurbSpline_isRational")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns whether the nurb spline is rational or not.")]
	public class API_NurbSpline_isRational : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_isRational()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns whether the nurb spline is rational or not.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.isRational;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_NurbSpline_isClosed")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Return/set the nurb spline's isClosed property.")]
	public class API_NurbSpline_isClosed : dynRevitTransactionNodeWithOneOutput
	{
		public API_NurbSpline_isClosed()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the nurb spline's isClosed property.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.NurbSpline)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.NurbSpline));
			var result = arg0.isClosed;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_AdaptivePointType")]
	[NodeSearchTags("adaptive","point","type")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Shape Handle Point.")]
	public class API_AdaptivePointType : dynEnum
	{
		public API_AdaptivePointType()
		{
			WireToEnum(Enum.GetValues(typeof(AdaptivePointType)));
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the specified wall type and normal vector.")]
	public class API_Wall_Create : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
		///</summary>
		public API_Wall_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered to be inside and outside.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the specified wall type and normal vector.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_1")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the specified wall type.")]
	public class API_Wall_Create_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
		///</summary>
		public API_Wall_Create_1()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the specified wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_2")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the default wall type.")]
	public class API_Wall_Create_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
		///</summary>
		public API_Wall_Create_2()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.Curve>)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the default wall type.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_3")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.")]
	public class API_Wall_Create_3 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
		///</summary>
		public API_Wall_Create_3()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Double),typeof(System.Double),typeof(System.Boolean),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Id of the wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_4")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the default wall style.")]
	public class API_Wall_Create_4 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
		///</summary>
		public API_Wall_Create_4()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document in which the new wall is created.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("val", "Id of the level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new rectangular profile wall within the project using the default wall style.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Flip
	///</summary>
	[NodeName("API_Wall_Flip")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The wall orientation will be flipped.")]
	public class API_Wall_Flip : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Wall.Flip
		///</summary>
		public API_Wall_Flip()
		{
			base_type = typeof(Autodesk.Revit.DB.Wall);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Flip", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall", typeof(object)));
			}
			OutPortData.Add(new PortData("out","The wall orientation will be flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_Orientation")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The normal vector projected from the exterior side of the wall.")]
	public class API_Wall_Orientation : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_Orientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","The normal vector projected from the exterior side of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.Orientation;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Wall_Flipped")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the wall orientation is flipped.")]
	public class API_Wall_Flipped : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_Flipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Property to test whether the wall orientation is flipped.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.Flipped;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Wall_StructuralUsage")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes  the wall's designated structural usage.")]
	public class API_Wall_StructuralUsage : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes  the wall's designated structural usage.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.StructuralUsage;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Wall_Width")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the overall thickness of the wall.")]
	public class API_Wall_Width : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_Width()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the overall thickness of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.Width;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Wall_CurtainGrid")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the grid object of a curtain wall")]
	public class API_Wall_CurtainGrid : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_CurtainGrid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the grid object of a curtain wall",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.CurtainGrid;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	[NodeName("API_Wall_WallType")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes the type of the wall.")]
	public class API_Wall_WallType : dynRevitTransactionNodeWithOneOutput
	{
		public API_Wall_WallType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes the type of the wall.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
		public override Value Evaluate(FSharpList<Value> args)
		{
			var arg0=(Autodesk.Revit.DB.Wall)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.Wall));
			var result = arg0.WallType;
			return DynamoTypeConverter.ConvertToValue(result);
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.GetTransformFromViewToView
	///</summary>
	[NodeName("API_ElementTransformUtils_GetTransformFromViewToView")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a transformation that is applied to elements when copying from one view to another view.")]
	public class API_ElementTransformUtils_GetTransformFromViewToView : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.GetTransformFromViewToView
		///</summary>
		public API_ElementTransformUtils_GetTransformFromViewToView()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTransformFromViewToView", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The source view",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("v", "The destination view",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Returns a transformation that is applied to elements when copying from one view to another view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies a set of elements from source view to destination view.")]
	public class API_ElementTransformUtils_CopyElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
		///</summary>
		public API_ElementTransformUtils_CopyElements()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CopyElements", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.Transform),typeof(Autodesk.Revit.DB.CopyPasteOptions)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view in the source document that contains the elements to copy.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("val", "The set of elements to copy.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("v", "The view in the destination document that the elements will be pasted into.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("val", "The transform for the new elements, in addition to the transformation between the source and destination views. Can be",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("val", "Optional settings. Can be",typeof(Autodesk.Revit.DB.CopyPasteOptions)));
			OutPortData.Add(new PortData("out","Copies a set of elements from source view to destination view.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElements_1")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies a set of elements from source document to destination document.")]
	public class API_ElementTransformUtils_CopyElements_1 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
		///</summary>
		public API_ElementTransformUtils_CopyElements_1()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CopyElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.Transform),typeof(Autodesk.Revit.DB.CopyPasteOptions)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that contains the elements to copy.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The set of elements to copy.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("val", "The destination document to paste the elements into.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The transform for the new elements. Can be",typeof(Autodesk.Revit.DB.Transform)));
			InPortData.Add(new PortData("val", "Optional settings. Can be",typeof(Autodesk.Revit.DB.CopyPasteOptions)));
			OutPortData.Add(new PortData("out","Copies a set of elements from source document to destination document.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElement
	///</summary>
	[NodeName("API_ElementTransformUtils_RotateElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotates an element about the given axis and angle.")]
	public class API_ElementTransformUtils_RotateElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElement
		///</summary>
		public API_ElementTransformUtils_RotateElement()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RotateElement", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.Line),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The element to rotate.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("crv", "The axis of rotation.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The angle of rotation in radians.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Rotates an element about the given axis and angle.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElements
	///</summary>
	[NodeName("API_ElementTransformUtils_RotateElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotates a set of elements about the given axis and angle.")]
	public class API_ElementTransformUtils_RotateElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElements
		///</summary>
		public API_ElementTransformUtils_RotateElements()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "RotateElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.Line),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The set of elements to rotate.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("crv", "The axis of rotation.",typeof(Autodesk.Revit.DB.Line)));
			InPortData.Add(new PortData("n", "The angle of rotation in radians.",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Rotates a set of elements about the given axis and angle.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElement
	///</summary>
	[NodeName("API_ElementTransformUtils_MirrorElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a mirrored copy of an element about a given plane.")]
	public class API_ElementTransformUtils_MirrorElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElement
		///</summary>
		public API_ElementTransformUtils_MirrorElement()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MirrorElement", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.Plane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the element.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The element to mirror.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("p", "The mirror plane.",typeof(Autodesk.Revit.DB.Plane)));
			OutPortData.Add(new PortData("out","Creates a mirrored copy of an element about a given plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElements
	///</summary>
	[NodeName("API_ElementTransformUtils_MirrorElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a mirrored copy of a set of elements about a given plane.")]
	public class API_ElementTransformUtils_MirrorElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElements
		///</summary>
		public API_ElementTransformUtils_MirrorElements()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MirrorElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.Plane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The set of elements to mirror.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("p", "The mirror plane.",typeof(Autodesk.Revit.DB.Plane)));
			OutPortData.Add(new PortData("out","Creates a mirrored copy of a set of elements about a given plane.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElement
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies an element and places the copy at a location indicated by a given transformation.")]
	public class API_ElementTransformUtils_CopyElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElement
		///</summary>
		public API_ElementTransformUtils_CopyElement()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CopyElement", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the element.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The id of the element to copy.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("xyz", "The translation vector for the new element.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Copies an element and places the copy at a location indicated by a given transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElements_2")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies a set of elements and places the copies at a location indicated by a given translation.")]
	public class API_ElementTransformUtils_CopyElements_2 : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
		///</summary>
		public API_ElementTransformUtils_CopyElements_2()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CopyElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The set of elements to copy.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("xyz", "The translation vector for the new elements.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Copies a set of elements and places the copies at a location indicated by a given translation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElement
	///</summary>
	[NodeName("API_ElementTransformUtils_MoveElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves one element by a given transformation.")]
	public class API_ElementTransformUtils_MoveElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElement
		///</summary>
		public API_ElementTransformUtils_MoveElement()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MoveElement", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The id of the element to move.",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("xyz", "The translation vector for the elements.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Moves one element by a given transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElements
	///</summary>
	[NodeName("API_ElementTransformUtils_MoveElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves a set of elements by a given transformation.")]
	public class API_ElementTransformUtils_MoveElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElements
		///</summary>
		public API_ElementTransformUtils_MoveElements()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "MoveElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document that owns the elements.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The set of elements to move.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			InPortData.Add(new PortData("xyz", "The translation vector for the elements.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Moves a set of elements by a given transformation.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElement
	///</summary>
	[NodeName("API_ElementTransformUtils_CanMirrorElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether element can be mirrored.")]
	public class API_ElementTransformUtils_CanMirrorElement : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElement
		///</summary>
		public API_ElementTransformUtils_CanMirrorElement()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanMirrorElement", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(Autodesk.Revit.DB.ElementId)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document where the element reside.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The element identified by id.",typeof(Autodesk.Revit.DB.ElementId)));
			OutPortData.Add(new PortData("out","Determines whether element can be mirrored.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CanMirrorElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether elements can be mirrored.")]
	public class API_ElementTransformUtils_CanMirrorElements : dynRevitAPINode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElements
		///</summary>
		public API_ElementTransformUtils_CanMirrorElements()
		{
			base_type = typeof(Autodesk.Revit.DB.ElementTransformUtils);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "CanMirrorElements", false, new Type[]{typeof(Autodesk.Revit.DB.Document),typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ElementTransformUtils", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The document where the elements reside.",typeof(Autodesk.Revit.DB.Document)));
			InPortData.Add(new PortData("val", "The elements identified by id.",typeof(System.Collections.Generic.ICollection<Autodesk.Revit.DB.ElementId>)));
			OutPortData.Add(new PortData("out","Determines whether elements can be mirrored.",typeof(object)));
			NodeUI.RegisterAllPorts();
		}
	}

}
