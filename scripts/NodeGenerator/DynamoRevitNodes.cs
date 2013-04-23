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
	public class API_ReferencePoint_GetHubId : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetVisibility
	///</summary>
	[NodeName("API_ReferencePoint_SetVisibility")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the visibility for the point.")]
	public class API_ReferencePoint_SetVisibility : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetVisibility
		///</summary>
		public API_ReferencePoint_SetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetVisibility", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyElementVisibility)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyElementVisibility",typeof(Autodesk.Revit.DB.FamilyElementVisibility)));
			OutPortData.Add(new PortData("out","Sets the visibility for the point.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetVisibility
	///</summary>
	[NodeName("API_ReferencePoint_GetVisibility")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the point.")]
	public class API_ReferencePoint_GetVisibility : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXZ
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceXZ")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the XZ plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceXZ : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceYZ
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceYZ")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the YZ plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceYZ : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinatePlaneReferenceXY
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinatePlaneReferenceXY")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference for the XY plane of the coordinatesystem.")]
	public class API_ReferencePoint_GetCoordinatePlaneReferenceXY : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetInterpolatingCurves
	///</summary>
	[NodeName("API_ReferencePoint_GetInterpolatingCurves")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The set of CurveByPoints elements that interpolatea ReferencePoint.")]
	public class API_ReferencePoint_GetInterpolatingCurves : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetCoordinateSystem
	///</summary>
	[NodeName("API_ReferencePoint_SetCoordinateSystem")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The position and orientation of the ReferencePoint.")]
	public class API_ReferencePoint_SetCoordinateSystem : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetCoordinateSystem
		///</summary>
		public API_ReferencePoint_SetCoordinateSystem()
		{
			base_type = typeof(Autodesk.Revit.DB.ReferencePoint);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetCoordinateSystem", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The position and orientation of the ReferencePoint.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetCoordinateSystem
	///</summary>
	[NodeName("API_ReferencePoint_GetCoordinateSystem")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The position and orientation of the ReferencePoint.")]
	public class API_ReferencePoint_GetCoordinateSystem : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.SetPointElementReference
	///</summary>
	[NodeName("API_ReferencePoint_SetPointElementReference")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Change the rule for computing the location of the ReferencePoint relative to other elements inthe document.")]
	public class API_ReferencePoint_SetPointElementReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ReferencePoint.GetPointElementReference
	///</summary>
	[NodeName("API_ReferencePoint_GetPointElementReference")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve a copy of the rule that computes thelocation of the ReferencePoint relative to other elements inthe document.")]
	public class API_ReferencePoint_GetPointElementReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_ReferencePoint_ShowNormalReferencePlaneOnly")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether all three coordinate planes are shown, or only thenormal (XY) plane.")]
	public class API_ReferencePoint_ShowNormalReferencePlaneOnly : dynAPIPropertyNode
	{
		public API_ReferencePoint_ShowNormalReferencePlaneOnly()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Whether all three coordinate planes are shown, or only thenormal (XY) plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ReferencePoint_CoordinatePlaneVisibility")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Visibility settings for the coordinate reference planes.")]
	public class API_ReferencePoint_CoordinatePlaneVisibility : dynAPIPropertyNode
	{
		public API_ReferencePoint_CoordinatePlaneVisibility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Visibility settings for the coordinate reference planes.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ReferencePoint_Visible")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the point is visible when the family is loadedinto a project.")]
	public class API_ReferencePoint_Visible : dynAPIPropertyNode
	{
		public API_ReferencePoint_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","Whether the point is visible when the family is loadedinto a project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ReferencePoint_Position")]
	[NodeSearchTags("point","reference","pt","coordinate","system")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The position of the ReferencePoint.")]
	public class API_ReferencePoint_Position : dynAPIPropertyNode
	{
		public API_ReferencePoint_Position()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ReferencePoint",typeof(Autodesk.Revit.DB.ReferencePoint)));
			OutPortData.Add(new PortData("out","The position of the ReferencePoint.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteFace_MixedDerivs")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Mixed derivatives of the surface.")]
	public class API_HermiteFace_MixedDerivs : dynAPIPropertyNode
	{
		public API_HermiteFace_MixedDerivs()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Mixed derivatives of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteFace_Points")]
	[NodeSearchTags("face","hermite")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Interpolation points of the surface.")]
	public class API_HermiteFace_Points : dynAPIPropertyNode
	{
		public API_HermiteFace_Points()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteFace",typeof(Autodesk.Revit.DB.HermiteFace)));
			OutPortData.Add(new PortData("out","Interpolation points of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.AlmostEqual
	///</summary>
	[NodeName("API_Transform_AlmostEqual")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this transformation and the specified transformation are the same within the tolerance (1.0e-09).")]
	public class API_Transform_AlmostEqual : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasisAndOrigin
	///</summary>
	[NodeName("API_Transform_ScaleBasisAndOrigin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scales the basis vectors and the origin of this transformation and returns the result.")]
	public class API_Transform_ScaleBasisAndOrigin : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.ScaleBasis
	///</summary>
	[NodeName("API_Transform_ScaleBasis")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scales the basis vectors of this transformation and returns the result.")]
	public class API_Transform_ScaleBasis : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.Multiply
	///</summary>
	[NodeName("API_Transform_Multiply")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this transformation by the specified transformation and returns the result.")]
	public class API_Transform_Multiply : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.OfVector
	///</summary>
	[NodeName("API_Transform_OfVector")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Applies the transform to the vector")]
	public class API_Transform_OfVector : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.OfPoint
	///</summary>
	[NodeName("API_Transform_OfPoint")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Applies the transformation to the point and returns the result.")]
	public class API_Transform_OfPoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Transform.#ctor
	///</summary>
	[NodeName("API_Transform")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The copy constructor.")]
	public class API_Transform : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Transform.#ctor
		///</summary>
		public API_Transform()
		{
			base_type = typeof(Autodesk.Revit.DB.Transform);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Transform", true, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The copy constructor.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_Inverse")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The inverse transformation of this transformation.")]
	public class API_Transform_Inverse : dynAPIPropertyNode
	{
		public API_Transform_Inverse()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The inverse transformation of this transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_Determinant")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The determinant of this transformation.")]
	public class API_Transform_Determinant : dynAPIPropertyNode
	{
		public API_Transform_Determinant()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The determinant of this transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_IsConformal")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is conformal.")]
	public class API_Transform_IsConformal : dynAPIPropertyNode
	{
		public API_Transform_IsConformal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is conformal.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_HasReflection")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation produces reflection.")]
	public class API_Transform_HasReflection : dynAPIPropertyNode
	{
		public API_Transform_HasReflection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation produces reflection.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_Scale")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The real number that represents the scale of the transformation.")]
	public class API_Transform_Scale : dynAPIPropertyNode
	{
		public API_Transform_Scale()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The real number that represents the scale of the transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_IsTranslation")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is a translation.")]
	public class API_Transform_IsTranslation : dynAPIPropertyNode
	{
		public API_Transform_IsTranslation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is a translation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_IsIdentity")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this transformation is an identity.")]
	public class API_Transform_IsIdentity : dynAPIPropertyNode
	{
		public API_Transform_IsIdentity()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this transformation is an identity.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_Origin")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Defines the origin of the old coordinate system in the new coordinate system.")]
	public class API_Transform_Origin : dynAPIPropertyNode
	{
		public API_Transform_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Defines the origin of the old coordinate system in the new coordinate system.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_BasisZ")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the Z axis of this transformation.")]
	public class API_Transform_BasisZ : dynAPIPropertyNode
	{
		public API_Transform_BasisZ()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Z axis of this transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_BasisY")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the Y axis of this transformation.")]
	public class API_Transform_BasisY : dynAPIPropertyNode
	{
		public API_Transform_BasisY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the Y axis of this transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Transform_BasisX")]
	[NodeSearchTags("transform","coordinate system","cs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The basis of the X axis of this transformation.")]
	public class API_Transform_BasisX : dynAPIPropertyNode
	{
		public API_Transform_BasisX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","The basis of the X axis of this transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Mesh_MaterialElementId")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Element ID of the material from which this mesh is composed.")]
	public class API_Mesh_MaterialElementId : dynAPIPropertyNode
	{
		public API_Mesh_MaterialElementId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this mesh is composed.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Mesh_Vertices")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves all vertices used to define this mesh. Intended for indexed access.")]
	public class API_Mesh_Vertices : dynAPIPropertyNode
	{
		public API_Mesh_Vertices()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","Retrieves all vertices used to define this mesh. Intended for indexed access.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Mesh_NumTriangles")]
	[NodeSearchTags("mesh")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of triangles that the mesh contains.")]
	public class API_Mesh_NumTriangles : dynAPIPropertyNode
	{
		public API_Mesh_NumTriangles()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Mesh",typeof(Autodesk.Revit.DB.Mesh)));
			OutPortData.Add(new PortData("out","The number of triangles that the mesh contains.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetInstanceGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes a transformation of the geometric representation of the instance.")]
	public class API_GeometryInstance_GetInstanceGeometry : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetInstanceGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetInstanceGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the geometric representation of the instance.")]
	public class API_GeometryInstance_GetInstanceGeometry_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetSymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes a transformation of the geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_GetSymbolGeometry : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GeometryInstance.GetSymbolGeometry
	///</summary>
	[NodeName("API_GeometryInstance_GetSymbolGeometry_1")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_GetSymbolGeometry_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_GeometryInstance_SymbolGeometry")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The geometric representation of the symbol which generates this instance.")]
	public class API_GeometryInstance_SymbolGeometry : dynAPIPropertyNode
	{
		public API_GeometryInstance_SymbolGeometry()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The geometric representation of the symbol which generates this instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_GeometryInstance_Symbol")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The symbol element that this object is referring to.")]
	public class API_GeometryInstance_Symbol : dynAPIPropertyNode
	{
		public API_GeometryInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The symbol element that this object is referring to.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_GeometryInstance_Transform")]
	[NodeSearchTags("geometry","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.")]
	public class API_GeometryInstance_Transform : dynAPIPropertyNode
	{
		public API_GeometryInstance_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GeometryInstance",typeof(Autodesk.Revit.DB.GeometryInstance)));
			OutPortData.Add(new PortData("out","The affine transformation from the local coordinate space of the symbol into thecoordinate space of the instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDividedSurface
	///</summary>
	[NodeName("API_FamilyItemFactory_NewDividedSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a DividedSurface element on one surface of another element.")]
	public class API_FamilyItemFactory_NewDividedSurface : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewCurveByPoints
	///</summary>
	[NodeName("API_FamilyItemFactory_NewCurveByPoints")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a 3d curve through two or more points in an AutodeskRevit family document.")]
	public class API_FamilyItemFactory_NewCurveByPoints : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
	///</summary>
	[NodeName("API_FamilyItemFactory_NewReferencePoint")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point on an existing reference in an AutodeskRevit family document.")]
	public class API_FamilyItemFactory_NewReferencePoint : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
		///</summary>
		public API_FamilyItemFactory_NewReferencePoint()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePoint", false, new Type[]{typeof(Autodesk.Revit.DB.PointElementReference)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointElementReference",typeof(Autodesk.Revit.DB.PointElementReference)));
			OutPortData.Add(new PortData("out","Create a reference point on an existing reference in an AutodeskRevit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
	///</summary>
	[NodeName("API_FamilyItemFactory_NewReferencePoint_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point at a given location and with a givencoordinate system in an Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewReferencePoint_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
		///</summary>
		public API_FamilyItemFactory_NewReferencePoint_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePoint", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Create a reference point at a given location and with a givencoordinate system in an Autodesk Revit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
	///</summary>
	[NodeName("API_FamilyItemFactory_NewReferencePoint_2")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a reference point at a given location in an AutodeskRevit family document.")]
	public class API_FamilyItemFactory_NewReferencePoint_2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewReferencePoint
		///</summary>
		public API_FamilyItemFactory_NewReferencePoint_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewReferencePoint", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Create a reference point at a given location in an AutodeskRevit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSymbolicCurve
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSymbolicCurve")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a symbolic curve in an Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewSymbolicCurve : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewControl
	///</summary>
	[NodeName("API_FamilyItemFactory_NewControl")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new control into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewControl : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewModelText
	///</summary>
	[NodeName("API_FamilyItemFactory_NewModelText")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a model text in the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewModelText : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewOpening
	///</summary>
	[NodeName("API_FamilyItemFactory_NewOpening")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create an opening to cut the wall or ceiling.")]
	public class API_FamilyItemFactory_NewOpening : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewElectricalConnector
	///</summary>
	[NodeName("API_FamilyItemFactory_NewElectricalConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Electrical connector into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewElectricalConnector : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewElectricalConnector
		///</summary>
		public API_FamilyItemFactory_NewElectricalConnector()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElectricalConnector", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ett", "Indicates the system type of this new Electrical connector.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","Add a new Electrical connector into the Autodesk Revit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewPipeConnector
	///</summary>
	[NodeName("API_FamilyItemFactory_NewPipeConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new pipe connector into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewPipeConnector : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewPipeConnector
		///</summary>
		public API_FamilyItemFactory_NewPipeConnector()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPipeConnector", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("pst", "Indicates the system type of this new Pipe connector.",typeof(Autodesk.Revit.DB.Plumbing.PipeSystemType)));
			OutPortData.Add(new PortData("out","Add a new pipe connector into the Autodesk Revit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDuctConnector
	///</summary>
	[NodeName("API_FamilyItemFactory_NewDuctConnector")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new duct connector into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewDuctConnector : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDuctConnector
		///</summary>
		public API_FamilyItemFactory_NewDuctConnector()
		{
			base_type = typeof(Autodesk.Revit.Creation.FamilyItemFactory);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewDuctConnector", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.FamilyItemFactory", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "A reference to a planar face where the connector will be placed.",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("dst", "Indicates the system type of this new duct connector.",typeof(Autodesk.Revit.DB.Mechanical.DuctSystemType)));
			OutPortData.Add(new PortData("out","Add a new duct connector into the Autodesk Revit family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRadialDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new radial dimension object using a specified dimension type.")]
	public class API_FamilyItemFactory_NewRadialDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewDiameterDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewDiameterDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new diameter dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewDiameterDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRadialDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new radial dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewRadialDimension_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewArcLengthDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new arc length dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewArcLengthDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewArcLengthDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewArcLengthDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new arc length dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewArcLengthDimension_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewAngularDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new angular dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewAngularDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewAngularDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewAngularDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new angular dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewAngularDimension_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLinearDimension")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the specified dimension type.")]
	public class API_FamilyItemFactory_NewLinearDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLinearDimension
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLinearDimension_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new linear dimension object using the default dimension type.")]
	public class API_FamilyItemFactory_NewLinearDimension_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByThickenSingleSurface
	///</summary>
	[NodeName("API_FamilyItemFactory_NewFormByThickenSingleSurface")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a new Form element by thickening a single-surface form, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewFormByThickenSingleSurface : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewFormByCap
	///</summary>
	[NodeName("API_FamilyItemFactory_NewFormByCap")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by cap operation (to create a single-surface form), and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewFormByCap : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolveForms
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRevolveForms")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form elements by revolve operation, and add them into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewRevolveForms : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlendForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlendForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by swept blend operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewSweptBlendForm : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusionForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewExtrusionForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Extrude operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewExtrusionForm : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewLoftForm
	///</summary>
	[NodeName("API_FamilyItemFactory_NewLoftForm")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create new Form element by Loft operation, and add it into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewLoftForm : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new swept blend into the family document, using a selected reference as the path.")]
	public class API_FamilyItemFactory_NewSweptBlend : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweptBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweptBlend_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new swept blend into the family document, using a curve as the path.")]
	public class API_FamilyItemFactory_NewSweptBlend_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweep")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form into the family document, using an array of selected references as a 3D path.")]
	public class API_FamilyItemFactory_NewSweep : dynAPIMethodNode
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
			InPortData.Add(new PortData("swpp", "The profile of the newly created Sweep. This may containmore than one curve loop or a profile family. Each loop must be a fully closed curve loop and the loops must not intersect. All loops must lie in the same plane.The loop can be a unbound circle or ellipse,  but its geometry will be split in two in order to satisfy requirements for sketches used in extrusions.",typeof(Autodesk.Revit.DB.SweepProfile)));
			InPortData.Add(new PortData("i", "The index of the path curves. The curve upon which the profileplane will be determined.",typeof(System.Int32)));
			InPortData.Add(new PortData("ppl", "The location on the profileLocationCurve where the profileplane will be determined.",typeof(Autodesk.Revit.DB.ProfilePlaneLocation)));
			OutPortData.Add(new PortData("out","Adds a new sweep form into the family document, using an array of selected references as a 3D path.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewSweep
	///</summary>
	[NodeName("API_FamilyItemFactory_NewSweep_1")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new sweep form to the family document, using a path of curve elements.")]
	public class API_FamilyItemFactory_NewSweep_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRevolution
	///</summary>
	[NodeName("API_FamilyItemFactory_NewRevolution")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Revolution instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewRevolution : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewBlend
	///</summary>
	[NodeName("API_FamilyItemFactory_NewBlend")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Blend instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewBlend : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewExtrusion
	///</summary>
	[NodeName("API_FamilyItemFactory_NewExtrusion")]
	[NodeSearchTags("create","factory","family")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new Extrusion instance into the Autodesk Revit family document.")]
	public class API_FamilyItemFactory_NewExtrusion : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewAlignment
	///</summary>
	[NodeName("API_ItemFactoryBase_NewAlignment")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new locked alignment into the Autodesk Revit document.")]
	public class API_ItemFactoryBase_NewAlignment : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.PlaceGroup
	///</summary>
	[NodeName("API_ItemFactoryBase_PlaceGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Place an instance of a Model Group into the Autodesk Revit document, using a locationand a group type.")]
	public class API_ItemFactoryBase_PlaceGroup : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewViewSection
	///</summary>
	[NodeName("API_ItemFactoryBase_NewViewSection")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new section view.")]
	public class API_ItemFactoryBase_NewViewSection : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewViewSection
		///</summary>
		public API_ItemFactoryBase_NewViewSection()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewViewSection", false, new Type[]{typeof(Autodesk.Revit.DB.BoundingBoxXYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The view volume of the section will correspond geometrically to the specified bounding box.",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Creates a new section view.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewView3D
	///</summary>
	[NodeName("API_ItemFactoryBase_NewView3D")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new 3D view.")]
	public class API_ItemFactoryBase_NewView3D : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewView3D
		///</summary>
		public API_ItemFactoryBase_NewView3D()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewView3D", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The view direction - the vector pointing from the eye towards the model.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new 3D view.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNotes
	///</summary>
	[NodeName("API_ItemFactoryBase_NewTextNotes")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates TextNotes with the specified data.")]
	public class API_ItemFactoryBase_NewTextNotes : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNotes
		///</summary>
		public API_ItemFactoryBase_NewTextNotes()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTextNotes", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.TextNoteCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of TextNoteCreationData which wraps the creation arguments of the TextNotes to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.TextNoteCreationData>)));
			OutPortData.Add(new PortData("out","Creates TextNotes with the specified data.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
	///</summary>
	[NodeName("API_ItemFactoryBase_NewTextNote")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new text note with a single leader.")]
	public class API_ItemFactoryBase_NewTextNote : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewTextNote
	///</summary>
	[NodeName("API_ItemFactoryBase_NewTextNote_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TextNote object without a leader.")]
	public class API_ItemFactoryBase_NewTextNote_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane on a reference to existing planar geometry.")]
	public class API_ItemFactoryBase_NewSketchPlane : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane on a planar face of existing geometry.")]
	public class API_ItemFactoryBase_NewSketchPlane_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewSketchPlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewSketchPlane_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sketch plane from an arbitrary geometric plane.")]
	public class API_ItemFactoryBase_NewSketchPlane_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane2
	///</summary>
	[NodeName("API_ItemFactoryBase_NewReferencePlane2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of ReferencePlane.")]
	public class API_ItemFactoryBase_NewReferencePlane2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewReferencePlane
	///</summary>
	[NodeName("API_ItemFactoryBase_NewReferencePlane")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of ReferencePlane.")]
	public class API_ItemFactoryBase_NewReferencePlane : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewViewPlan
	///</summary>
	[NodeName("API_ItemFactoryBase_NewViewPlan")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a plan view based on the specified level.")]
	public class API_ItemFactoryBase_NewViewPlan : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewViewPlan
		///</summary>
		public API_ItemFactoryBase_NewViewPlan()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewViewPlan", false, new Type[]{typeof(System.String),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.ViewPlanType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("s", "The name for the new plan view, must be unique or",typeof(System.String)));
			InPortData.Add(new PortData("l", "The level on which the plan view is to be associated.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "The type of plan view to be created.",typeof(Autodesk.Revit.DB.ViewPlanType)));
			OutPortData.Add(new PortData("out","Creates a plan view based on the specified level.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewLevel
	///</summary>
	[NodeName("API_ItemFactoryBase_NewLevel")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new level.")]
	public class API_ItemFactoryBase_NewLevel : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewModelCurveArray
	///</summary>
	[NodeName("API_ItemFactoryBase_NewModelCurveArray")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an array of new model line elements.")]
	public class API_ItemFactoryBase_NewModelCurveArray : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewModelCurveArray
		///</summary>
		public API_ItemFactoryBase_NewModelCurveArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewModelCurveArray", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.SketchPlane)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("sp", "Autodesk.Revit.DB.SketchPlane",typeof(Autodesk.Revit.DB.SketchPlane)));
			OutPortData.Add(new PortData("out","Creates an array of new model line elements.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewModelCurve
	///</summary>
	[NodeName("API_ItemFactoryBase_NewModelCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new model line element.")]
	public class API_ItemFactoryBase_NewModelCurve : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewGroup
	///</summary>
	[NodeName("API_ItemFactoryBase_NewGroup")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type of group.")]
	public class API_ItemFactoryBase_NewGroup : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewGroup
	///</summary>
	[NodeName("API_ItemFactoryBase_NewGroup_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type of group.")]
	public class API_ItemFactoryBase_NewGroup_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewGroup
		///</summary>
		public API_ItemFactoryBase_NewGroup_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewGroup", false, new Type[]{typeof(Autodesk.Revit.DB.ElementSet)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A set of elements which will be made into the new group.",typeof(Autodesk.Revit.DB.ElementSet)));
			OutPortData.Add(new PortData("out","Creates a new type of group.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstances2
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstances2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Family instances within the document.")]
	public class API_ItemFactoryBase_NewFamilyInstances2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstances
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstances")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Family instances within the document.")]
	public class API_ItemFactoryBase_NewFamilyInstances : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstances
		///</summary>
		public API_ItemFactoryBase_NewFamilyInstances()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewFamilyInstances", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.FamilyInstanceCreationData>)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a line based detail family instance into the Autodesk Revit document, using an line and a view where the instance should be placed.")]
	public class API_ItemFactoryBase_NewFamilyInstance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance into the Autodesk Revit document, using an origin and a view where the instance should be placed.")]
	public class API_ItemFactoryBase_NewFamilyInstance_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_2")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a line on that face for its position, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_3")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face referenced by the input Reference instance, using a location, reference direction, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_4")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a line on that face for its position, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_4 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_5")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family onto a face of an existing element, using a location, reference direction, and a type/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_5 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_6")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location and atype/symbol.")]
	public class API_ItemFactoryBase_NewFamilyInstance_6 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_7")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, and the host element.")]
	public class API_ItemFactoryBase_NewFamilyInstance_7 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance
	///</summary>
	[NodeName("API_ItemFactoryBase_NewFamilyInstance_8")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a reference direction.")]
	public class API_ItemFactoryBase_NewFamilyInstance_8 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDimension")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the specified dimension style.")]
	public class API_ItemFactoryBase_NewDimension : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDimension
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDimension_1")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear dimension object using the default dimension style.")]
	public class API_ItemFactoryBase_NewDimension_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurveArray
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDetailCurveArray")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an array of new detail curve elements.")]
	public class API_ItemFactoryBase_NewDetailCurveArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewDetailCurve
	///</summary>
	[NodeName("API_ItemFactoryBase_NewDetailCurve")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new detail curve element.")]
	public class API_ItemFactoryBase_NewDetailCurve : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewAnnotationSymbol
	///</summary>
	[NodeName("API_ItemFactoryBase_NewAnnotationSymbol")]
	[NodeSearchTags("factory","create","item")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new instance of an Annotation Symbol into the Autodesk Revit document, using an origin and a view where the instance should be placed.")]
	public class API_ItemFactoryBase_NewAnnotationSymbol : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.ItemFactoryBase.NewAnnotationSymbol
		///</summary>
		public API_ItemFactoryBase_NewAnnotationSymbol()
		{
			base_type = typeof(Autodesk.Revit.Creation.ItemFactoryBase);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAnnotationSymbol", false, new Type[]{typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.AnnotationSymbolType),typeof(Autodesk.Revit.DB.View)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.ItemFactoryBase", typeof(object)));
			}
			InPortData.Add(new PortData("xyz", "The origin of the annotation symbol. If created on",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("val", "An annotation symbol type that represents the type of the instance that is to be inserted.",typeof(Autodesk.Revit.DB.AnnotationSymbolType)));
			InPortData.Add(new PortData("v", "A 2D view in which to display the annotation symbol.",typeof(Autodesk.Revit.DB.View)));
			OutPortData.Add(new PortData("out","Add a new instance of an Annotation Symbol into the Autodesk Revit document, using an origin and a view where the instance should be placed.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Create
	///</summary>
	[NodeName("API_PolyLine_Create")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a polyline with coordinate points provided.")]
	public class API_PolyLine_Create : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Create
		///</summary>
		public API_PolyLine_Create()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "Create", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "System.Collections.Generic.IList{Autodesk.Revit.DB.XYZ}",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			OutPortData.Add(new PortData("out","Creates a polyline with coordinate points provided.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Clone
	///</summary>
	[NodeName("API_PolyLine_Clone")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this polyline.")]
	public class API_PolyLine_Clone : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetTransformed
	///</summary>
	[NodeName("API_PolyLine_GetTransformed")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the copy of the polyline which is applied the specified transformation.")]
	public class API_PolyLine_GetTransformed : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetTransformed
		///</summary>
		public API_PolyLine_GetTransformed()
		{
			base_type = typeof(Autodesk.Revit.DB.PolyLine);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTransformed", false, new Type[]{typeof(Autodesk.Revit.DB.Transform)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Transform",typeof(Autodesk.Revit.DB.Transform)));
			OutPortData.Add(new PortData("out","Gets the copy of the polyline which is applied the specified transformation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetOutline
	///</summary>
	[NodeName("API_PolyLine_GetOutline")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the outline of the polyline.")]
	public class API_PolyLine_GetOutline : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinates
	///</summary>
	[NodeName("API_PolyLine_GetCoordinates")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the coordinate points of the polyline.")]
	public class API_PolyLine_GetCoordinates : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.GetCoordinate
	///</summary>
	[NodeName("API_PolyLine_GetCoordinate")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the coordinate point of the specified index.")]
	public class API_PolyLine_GetCoordinate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PolyLine.Evaluate
	///</summary>
	[NodeName("API_PolyLine_Evaluate")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the polyline.")]
	public class API_PolyLine_Evaluate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_PolyLine_NumberOfCoordinates")]
	[NodeSearchTags("pline","polyline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the number of the coordinate points.")]
	public class API_PolyLine_NumberOfCoordinates : dynAPIPropertyNode
	{
		public API_PolyLine_NumberOfCoordinates()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PolyLine",typeof(Autodesk.Revit.DB.PolyLine)));
			OutPortData.Add(new PortData("out","Gets the number of the coordinate points.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.ToString
	///</summary>
	[NodeName("API_XYZ_ToString")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets formatted string showing (X, Y, Z) with values formatted to 9 decimal places.")]
	public class API_XYZ_ToString : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleOnPlaneTo
	///</summary>
	[NodeName("API_XYZ_AngleOnPlaneTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector projected to the specified plane.")]
	public class API_XYZ_AngleOnPlaneTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.AngleTo
	///</summary>
	[NodeName("API_XYZ_AngleTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector.")]
	public class API_XYZ_AngleTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.DistanceTo
	///</summary>
	[NodeName("API_XYZ_DistanceTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the distance from this point to the specified point.")]
	public class API_XYZ_DistanceTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
	///</summary>
	[NodeName("API_XYZ_IsAlmostEqualTo")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether 2 vectors are the same within the given tolerance.")]
	public class API_XYZ_IsAlmostEqualTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsAlmostEqualTo
	///</summary>
	[NodeName("API_XYZ_IsAlmostEqualTo_1")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this vector and the specified vector are the same within the tolerance (1.0e-09).")]
	public class API_XYZ_IsAlmostEqualTo_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Divide
	///</summary>
	[NodeName("API_XYZ_Divide")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Divides this vector by the specified value and returns the result.")]
	public class API_XYZ_Divide : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Multiply
	///</summary>
	[NodeName("API_XYZ_Multiply")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this vector by the specified value and returns the result.")]
	public class API_XYZ_Multiply : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Negate
	///</summary>
	[NodeName("API_XYZ_Negate")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Negates this vector.")]
	public class API_XYZ_Negate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Subtract
	///</summary>
	[NodeName("API_XYZ_Subtract")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Subtracts the specified vector from this vector and returns the result.")]
	public class API_XYZ_Subtract : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Add
	///</summary>
	[NodeName("API_XYZ_Add")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds the specified vector to this vector and returns the result.")]
	public class API_XYZ_Add : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.TripleProduct
	///</summary>
	[NodeName("API_XYZ_TripleProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The triple product of this vector and the two specified vectors.")]
	public class API_XYZ_TripleProduct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.CrossProduct
	///</summary>
	[NodeName("API_XYZ_CrossProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cross product of this vector and the specified vector.")]
	public class API_XYZ_CrossProduct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.DotProduct
	///</summary>
	[NodeName("API_XYZ_DotProduct")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The dot product of this vector and the specified vector.")]
	public class API_XYZ_DotProduct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.GetLength
	///</summary>
	[NodeName("API_XYZ_GetLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the length of this vector.")]
	public class API_XYZ_GetLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.Normalize
	///</summary>
	[NodeName("API_XYZ_Normalize")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new XYZ whose coordinates are the normalized values from this vector.")]
	public class API_XYZ_Normalize : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsUnitLength
	///</summary>
	[NodeName("API_XYZ_IsUnitLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this vector is of unit length.")]
	public class API_XYZ_IsUnitLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.IsZeroLength
	///</summary>
	[NodeName("API_XYZ_IsZeroLength")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this vector is a zero vector.")]
	public class API_XYZ_IsZeroLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
	///</summary>
	[NodeName("API_XYZ")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an XYZ with the supplied coordinates.")]
	public class API_XYZ : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.XYZ.#ctor
	///</summary>
	[NodeName("API_XYZ_1")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a default XYZ with the values (0, 0, 0).")]
	public class API_XYZ_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_XYZ_Z")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the third coordinate.")]
	public class API_XYZ_Z : dynAPIPropertyNode
	{
		public API_XYZ_Z()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the third coordinate.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_XYZ_Y")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the second coordinate.")]
	public class API_XYZ_Y : dynAPIPropertyNode
	{
		public API_XYZ_Y()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_XYZ_X")]
	[NodeSearchTags("xyz","point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the first coordinate.")]
	public class API_XYZ_X : dynAPIPropertyNode
	{
		public API_XYZ_X()
		{
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointConstraintType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPointConstraintType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets constrain type of an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPointConstraintType : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointConstraintType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPointConstraintType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets constrain type of an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPointConstraintType : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPointOrientationType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPointOrientationType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets orientation type of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPointOrientationType : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPointOrientationType
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPointOrientationType")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets orientation type of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPointOrientationType : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.SetPlacementNumber
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_SetPlacementNumber")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets Placement Number of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_SetPlacementNumber : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetPlacementNumber
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetPlacementNumber")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Placement number of an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_GetPlacementNumber : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.MakeAdaptivePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_MakeAdaptivePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Makes Reference Point an Adaptive Point or makes an Adaptive Point a Reference Point.")]
	public class API_AdaptiveComponentFamilyUtils_MakeAdaptivePoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfShapeHandlePoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Shape Handle Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfShapeHandlePoints : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfPlacementPoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Placement Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfPlacementPoints : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.GetNumberOfAdaptivePoints
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets number of Adaptive Point Elements in Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_GetNumberOfAdaptivePoints : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveShapeHandlePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Shape Handle Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptiveShapeHandlePoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePlacementPoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Placement Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptivePlacementPoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptivePoint
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptivePoint")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Reference Point is an Adaptive Point.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptivePoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily
	///</summary>
	[NodeName("API_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily")]
	[NodeSearchTags("adaptive","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if the Family is an Adaptive Component Family.")]
	public class API_AdaptiveComponentFamilyUtils_IsAdaptiveComponentFamily : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_CylindricalFace_Axis")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_CylindricalFace_Axis : dynAPIPropertyNode
	{
		public API_CylindricalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_CylindricalFace_Origin")]
	[NodeSearchTags("face","cylinder","cylindrical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_CylindricalFace_Origin : dynAPIPropertyNode
	{
		public API_CylindricalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.CylindricalFace",typeof(Autodesk.Revit.DB.CylindricalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ConicalFace_HalfAngle")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Half angle of the surface.")]
	public class API_ConicalFace_HalfAngle : dynAPIPropertyNode
	{
		public API_ConicalFace_HalfAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Half angle of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ConicalFace_Axis")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_ConicalFace_Axis : dynAPIPropertyNode
	{
		public API_ConicalFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ConicalFace_Origin")]
	[NodeSearchTags("face","conical")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_ConicalFace_Origin : dynAPIPropertyNode
	{
		public API_ConicalFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ConicalFace",typeof(Autodesk.Revit.DB.ConicalFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTopographySurface
	///</summary>
	[NodeName("API_Document_NewTopographySurface")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new TopographySurface element in the document, and initializes it with a set of points.")]
	public class API_Document_NewTopographySurface : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTakeoffFitting
	///</summary>
	[NodeName("API_Document_NewTakeoffFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an takeoff fitting into the Autodesk Revit document,using one connector and one MEP curve.")]
	public class API_Document_NewTakeoffFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewUnionFitting
	///</summary>
	[NodeName("API_Document_NewUnionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an union fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewUnionFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCrossFitting
	///</summary>
	[NodeName("API_Document_NewCrossFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a cross fitting into the Autodesk Revit document,using four connectors.")]
	public class API_Document_NewCrossFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTransitionFitting
	///</summary>
	[NodeName("API_Document_NewTransitionFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an transition fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewTransitionFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTeeFitting
	///</summary>
	[NodeName("API_Document_NewTeeFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of a tee fitting into the Autodesk Revit document,using three connectors.")]
	public class API_Document_NewTeeFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElbowFitting
	///</summary>
	[NodeName("API_Document_NewElbowFitting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a new family instance of an elbow fitting into the Autodesk Revit document,using two connectors.")]
	public class API_Document_NewElbowFitting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using two connector, and flexible pipe type.")]
	public class API_Document_NewFlexPipe : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a connector, point array and pipe type.")]
	public class API_Document_NewFlexPipe_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexPipe
	///</summary>
	[NodeName("API_Document_NewFlexPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible pipe into the document, using a point array and pipe type.")]
	public class API_Document_NewFlexPipe_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document,  using two connectors and duct type.")]
	public class API_Document_NewPipe : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using a point, connector and pipe type.")]
	public class API_Document_NewPipe_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipe
	///</summary>
	[NodeName("API_Document_NewPipe_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new pipe into the document, using two points and pipe type.")]
	public class API_Document_NewPipe_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using two connector, and duct type.")]
	public class API_Document_NewFlexDuct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a connector, point array and duct type.")]
	public class API_Document_NewFlexDuct_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFlexDuct
	///</summary>
	[NodeName("API_Document_NewFlexDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new flexible duct into the document, using a point array and duct type.")]
	public class API_Document_NewFlexDuct_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two connectors and duct type.")]
	public class API_Document_NewDuct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using a point, connector and duct type.")]
	public class API_Document_NewDuct_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewDuct
	///</summary>
	[NodeName("API_Document_NewDuct_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a new duct into the document, using two points and duct type.")]
	public class API_Document_NewDuct_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a curve, type/symbol and reference level.")]
	public class API_Document_NewFamilyInstance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document, using a location,type/symbol and a base level.")]
	public class API_Document_NewFamilyInstance_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFamilyInstance
	///</summary>
	[NodeName("API_Document_NewFamilyInstance_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Inserts a new instance of a family into the document,using a location, type/symbol, the host element and a base level.")]
	public class API_Document_NewFamilyInstance_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
	///</summary>
	[NodeName("API_Document_NewFascia")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference.")]
	public class API_Document_NewFascia : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFascia
	///</summary>
	[NodeName("API_Document_NewFascia_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a fascia along a reference array.")]
	public class API_Document_NewFascia_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
	///</summary>
	[NodeName("API_Document_NewGutter")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference.")]
	public class API_Document_NewGutter : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGutter
	///</summary>
	[NodeName("API_Document_NewGutter_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a gutter along a reference array.")]
	public class API_Document_NewGutter_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
	///</summary>
	[NodeName("API_Document_NewSlabEdge")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference.")]
	public class API_Document_NewSlabEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlabEdge
	///</summary>
	[NodeName("API_Document_NewSlabEdge_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab edge along a reference array.")]
	public class API_Document_NewSlabEdge_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem
	///</summary>
	[NodeName("API_Document_NewCurtainSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of faces.")]
	public class API_Document_NewCurtainSystem : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem2
	///</summary>
	[NodeName("API_Document_NewCurtainSystem2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class API_Document_NewCurtainSystem2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem
	///</summary>
	[NodeName("API_Document_NewCurtainSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurtainSystem element from a set of face references.")]
	public class API_Document_NewCurtainSystem_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewCurtainSystem
		///</summary>
		public API_Document_NewCurtainSystem_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewCurtainSystem", false, new Type[]{typeof(Autodesk.Revit.DB.ReferenceArray),typeof(Autodesk.Revit.DB.CurtainSystemType)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWire
	///</summary>
	[NodeName("API_Document_NewWire")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new wire element.")]
	public class API_Document_NewWire : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewZone
	///</summary>
	[NodeName("API_Document_NewZone")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Zone element.")]
	public class API_Document_NewZone : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomBoundaryLines
	///</summary>
	[NodeName("API_Document_NewRoomBoundaryLines")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Room border.")]
	public class API_Document_NewRoomBoundaryLines : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceBoundaryLines
	///</summary>
	[NodeName("API_Document_NewSpaceBoundaryLines")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Space border.")]
	public class API_Document_NewSpaceBoundaryLines : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaceTag
	///</summary>
	[NodeName("API_Document_NewSpaceTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new SpaceTag.")]
	public class API_Document_NewSpaceTag : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces2
	///</summary>
	[NodeName("API_Document_NewSpaces2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a set of new unplaced spaces on a given phase.")]
	public class API_Document_NewSpaces2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces2
		///</summary>
		public API_Document_NewSpaces2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaces2", false, new Type[]{typeof(Autodesk.Revit.DB.Phase),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Creates a set of new unplaced spaces on a given phase.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces
	///</summary>
	[NodeName("API_Document_NewSpaces")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a set of new unplaced spaces on a given phase.")]
	public class API_Document_NewSpaces : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces
		///</summary>
		public API_Document_NewSpaces()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaces", false, new Type[]{typeof(Autodesk.Revit.DB.Phase),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Phase",typeof(Autodesk.Revit.DB.Phase)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Creates a set of new unplaced spaces on a given phase.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces2
	///</summary>
	[NodeName("API_Document_NewSpaces2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new spaces on the available plan circuits of a the given level.")]
	public class API_Document_NewSpaces2_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces
	///</summary>
	[NodeName("API_Document_NewSpaces_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new spaces on the available plan circuits of a the given level.")]
	public class API_Document_NewSpaces_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpaces
		///</summary>
		public API_Document_NewSpaces_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewSpaces", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase),typeof(Autodesk.Revit.DB.View)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level, at the given location, and assigned to the given phase.")]
	public class API_Document_NewSpace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new space element on the given level at the given location.")]
	public class API_Document_NewSpace_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpace
	///</summary>
	[NodeName("API_Document_NewSpace_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unplaced space on a given phase.")]
	public class API_Document_NewSpace_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPipingSystem
	///</summary>
	[NodeName("API_Document_NewPipingSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP piping system element.")]
	public class API_Document_NewPipingSystem : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewMechanicalSystem
	///</summary>
	[NodeName("API_Document_NewMechanicalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP mechanical system element.")]
	public class API_Document_NewMechanicalSystem : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
	///</summary>
	[NodeName("API_Document_NewElectricalSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from a set of electrical components.")]
	public class API_Document_NewElectricalSystem : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
	///</summary>
	[NodeName("API_Document_NewElectricalSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from a set of electrical components.")]
	public class API_Document_NewElectricalSystem_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
		///</summary>
		public API_Document_NewElectricalSystem_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElectricalSystem", false, new Type[]{typeof(Autodesk.Revit.DB.ElementSet),typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The electrical components in this system.",typeof(Autodesk.Revit.DB.ElementSet)));
			InPortData.Add(new PortData("ett", "The System Type of electrical system.",typeof(Autodesk.Revit.DB.Electrical.ElectricalSystemType)));
			OutPortData.Add(new PortData("out","Creates a new MEP Electrical System element from a set of electrical components.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
	///</summary>
	[NodeName("API_Document_NewElectricalSystem_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new MEP Electrical System element from an unused Connector.")]
	public class API_Document_NewElectricalSystem_2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewElectricalSystem
		///</summary>
		public API_Document_NewElectricalSystem_2()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewExtrusionRoof
	///</summary>
	[NodeName("API_Document_NewExtrusionRoof")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Extrusion Roof.")]
	public class API_Document_NewExtrusionRoof : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTruss
	///</summary>
	[NodeName("API_Document_NewTruss")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a New Truss.")]
	public class API_Document_NewTruss : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreas
	///</summary>
	[NodeName("API_Document_NewAreas")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new areas")]
	public class API_Document_NewAreas : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewArea
	///</summary>
	[NodeName("API_Document_NewArea")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new area")]
	public class API_Document_NewArea : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaTag
	///</summary>
	[NodeName("API_Document_NewAreaTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new area tag.")]
	public class API_Document_NewAreaTag : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaTag
		///</summary>
		public API_Document_NewAreaTag()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaTag", false, new Type[]{typeof(Autodesk.Revit.DB.ViewPlan),typeof(Autodesk.Revit.DB.Area),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("v", "Autodesk.Revit.DB.ViewPlan",typeof(Autodesk.Revit.DB.ViewPlan)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Area",typeof(Autodesk.Revit.DB.Area)));
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates a new area tag.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaViewPlan
	///</summary>
	[NodeName("API_Document_NewAreaViewPlan")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new view for the new area.")]
	public class API_Document_NewAreaViewPlan : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaViewPlan
		///</summary>
		public API_Document_NewAreaViewPlan()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaViewPlan", false, new Type[]{typeof(System.String),typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.AreaElemType)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("s", "System.String",typeof(System.String)));
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.AreaElemType",typeof(Autodesk.Revit.DB.AreaElemType)));
			OutPortData.Add(new PortData("out","Creates a new view for the new area.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryLine
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryLine")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new boundary line as an Area border.")]
	public class API_Document_NewAreaBoundaryLine : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationWall
	///</summary>
	[NodeName("API_Document_NewFoundationWall")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new continuous footing object.")]
	public class API_Document_NewFoundationWall : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSlab
	///</summary>
	[NodeName("API_Document_NewSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a slab within the project with the given horizontal profile using the default floor style.")]
	public class API_Document_NewSlab : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewTag
	///</summary>
	[NodeName("API_Document_NewTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new IndependentTag Element.")]
	public class API_Document_NewTag : dynAPIMethodNode
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
			InPortData.Add(new PortData("v", "The view in which the dimension is to be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("el", "The host object of tag.",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("b", "Whether there will be a leader.",typeof(System.Boolean)));
			InPortData.Add(new PortData("val", "The mode of the tag. Add by Category, add by Multi-Category, or add by material.",typeof(Autodesk.Revit.DB.TagMode)));
			InPortData.Add(new PortData("val", "The orientation of the tag.",typeof(Autodesk.Revit.DB.TagOrientation)));
			InPortData.Add(new PortData("xyz", "The position of the tag.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new IndependentTag Element.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new opening in a roof, floor and ceiling.")]
	public class API_Document_NewOpening : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a rectangular opening on a wall.")]
	public class API_Document_NewOpening_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new shaft opening between a set of levels.")]
	public class API_Document_NewOpening_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewOpening
	///</summary>
	[NodeName("API_Document_NewOpening_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new opening in a beam, brace and column.")]
	public class API_Document_NewOpening_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Area BoundaryConditions element on a host element.")]
	public class API_Document_NewAreaBoundaryConditions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewLineBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Line BoundaryConditions element on a host element.")]
	public class API_Document_NewLineBoundaryConditions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewAreaBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Area BoundaryConditions element on a reference.")]
	public class API_Document_NewAreaBoundaryConditions_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewLineBoundaryConditions_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Line BoundaryConditions element on a reference.")]
	public class API_Document_NewLineBoundaryConditions_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointBoundaryConditions
	///</summary>
	[NodeName("API_Document_NewPointBoundaryConditions")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Point BoundaryConditions Element.")]
	public class API_Document_NewPointBoundaryConditions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem : dynAPIMethodNode
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
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("xyz", "The direction is the direction of the BeamSystem. This argument is optional – may be null.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("b", "Whether the BeamSystem is 3D or not",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new BeamSystem with specified profile curves.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new 2D BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_1 : dynAPIMethodNode
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
			InPortData.Add(new PortData("l", "The level on which the BeamSystem is to be created. The work plane of the BeamSystem will be the ketch plane associated with the Level.If there is no current sketch plane associated with the level yet, we will create a default one.",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Creates a new 2D BeamSystem with specified profile curves.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewBeamSystem
	///</summary>
	[NodeName("API_Document_NewBeamSystem_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new BeamSystem with specified profile curves.")]
	public class API_Document_NewBeamSystem_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
	///</summary>
	[NodeName("API_Document_NewRoomTag")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new RoomTag.")]
	public class API_Document_NewRoomTag : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoomTag
		///</summary>
		public API_Document_NewRoomTag()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new unplaced rooms in the given phase.")]
	public class API_Document_NewRooms2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the given phase.")]
	public class API_Document_NewRooms2_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms2
	///</summary>
	[NodeName("API_Document_NewRooms2_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the last phase.")]
	public class API_Document_NewRooms2_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
	///</summary>
	[NodeName("API_Document_NewRooms")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new unplaced rooms in the given phase.")]
	public class API_Document_NewRooms : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
		///</summary>
		public API_Document_NewRooms()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms", false, new Type[]{typeof(Autodesk.Revit.DB.Phase),typeof(System.Int32)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
	///</summary>
	[NodeName("API_Document_NewRooms_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the given phase.")]
	public class API_Document_NewRooms_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
		///</summary>
		public API_Document_NewRooms_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.Phase)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
	///</summary>
	[NodeName("API_Document_NewRooms_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms in each plan circuit found in the given level in the last phase.")]
	public class API_Document_NewRooms_2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
		///</summary>
		public API_Document_NewRooms_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms", false, new Type[]{typeof(Autodesk.Revit.DB.Level)}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
	///</summary>
	[NodeName("API_Document_NewRooms_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new rooms using the specified placement data.")]
	public class API_Document_NewRooms_3 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRooms
		///</summary>
		public API_Document_NewRooms_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRooms", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.RoomCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of RoomCreationData which wraps the creation arguments of the rooms to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.RoomCreationData>)));
			OutPortData.Add(new PortData("out","Creates new rooms using the specified placement data.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new room within the confines of a plan circuit, or places an unplaced room within the confines of the plan circuit.")]
	public class API_Document_NewRoom : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unplaced room and with an assigned phase.")]
	public class API_Document_NewRoom_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRoom
	///</summary>
	[NodeName("API_Document_NewRoom_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new room on a level at a specified point.")]
	public class API_Document_NewRoom_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrids
	///</summary>
	[NodeName("API_Document_NewGrids")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates new grid lines.")]
	public class API_Document_NewGrids : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
	///</summary>
	[NodeName("API_Document_NewGrid")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new radial grid line.")]
	public class API_Document_NewGrid : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewGrid
	///</summary>
	[NodeName("API_Document_NewGrid_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new linear grid line.")]
	public class API_Document_NewGrid_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewSheet
	///</summary>
	[NodeName("API_Document_NewViewSheet")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new sheet view.")]
	public class API_Document_NewViewSheet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewViewDrafting
	///</summary>
	[NodeName("API_Document_NewViewDrafting")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new drafting view.")]
	public class API_Document_NewViewDrafting : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFoundationSlab
	///</summary>
	[NodeName("API_Document_NewFoundationSlab")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a foundation slab within the project with the given horizontal profile and floor style on the specified level.")]
	public class API_Document_NewFoundationSlab : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile and floor style on the specified level with the specified normal vector.")]
	public class API_Document_NewFloor : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile and floor style on the specified level.")]
	public class API_Document_NewFloor_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewFloor
	///</summary>
	[NodeName("API_Document_NewFloor_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a floor within the project with the given horizontal profile using the default floor style.")]
	public class API_Document_NewFloor_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWalls
	///</summary>
	[NodeName("API_Document_NewWalls")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates profile walls within the project.")]
	public class API_Document_NewWalls : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWalls
		///</summary>
		public API_Document_NewWalls()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWalls", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.ProfiledWallCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of ProfiledWallCreationData which wraps the creation arguments of the walls to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.ProfiledWallCreationData>)));
			OutPortData.Add(new PortData("out","Creates profile walls within the project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWalls
	///</summary>
	[NodeName("API_Document_NewWalls_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates rectangular walls within the project.")]
	public class API_Document_NewWalls_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWalls
		///</summary>
		public API_Document_NewWalls_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWalls", false, new Type[]{typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.RectangularWallCreationData>)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A list of RectangularWallCreationData which wraps the creation arguments of the walls to be created.",typeof(System.Collections.Generic.List<Autodesk.Revit.Creation.RectangularWallCreationData>)));
			OutPortData.Add(new PortData("out","Creates rectangular walls within the project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
	///</summary>
	[NodeName("API_Document_NewWall")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the specified wall type and normal vector.")]
	public class API_Document_NewWall : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
		///</summary>
		public API_Document_NewWall()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWall", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is consideredto be inside and outside.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the specified wall type and normal vector.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
	///</summary>
	[NodeName("API_Document_NewWall_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the specified wall type.")]
	public class API_Document_NewWall_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
		///</summary>
		public API_Document_NewWall_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWall", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the specified wall type.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
	///</summary>
	[NodeName("API_Document_NewWall_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the default wall type.")]
	public class API_Document_NewWall_2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
		///</summary>
		public API_Document_NewWall_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWall", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a non rectangular profile wall within the project using the default wall type.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
	///</summary>
	[NodeName("API_Document_NewWall_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.")]
	public class API_Document_NewWall_3 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
		///</summary>
		public API_Document_NewWall_3()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWall", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Double),typeof(System.Double),typeof(System.Boolean),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("n", "The height of the wall other than the default height.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "Modifies the wall's Base Offset parameter to determine its vertical placement.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
	///</summary>
	[NodeName("API_Document_NewWall_4")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the default wall style.")]
	public class API_Document_NewWall_4 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewWall
		///</summary>
		public API_Document_NewWall_4()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewWall", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates a new rectangular profile wall within the project using the default wall style.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotElevation
	///</summary>
	[NodeName("API_Document_NewSpotElevation")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new spot elevation object within the project.")]
	public class API_Document_NewSpotElevation : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewSpotCoordinate
	///</summary>
	[NodeName("API_Document_NewSpotCoordinate")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Generate a new spot coordinate object within the project.")]
	public class API_Document_NewSpotCoordinate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCombination
	///</summary>
	[NodeName("API_Document_NewLoadCombination")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination element     within the project.")]
	public class API_Document_NewLoadCombination : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadCase
	///</summary>
	[NodeName("API_Document_NewLoadCase")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase element within the project.")]
	public class API_Document_NewLoadCase : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadUsage
	///</summary>
	[NodeName("API_Document_NewLoadUsage")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage element within the project.")]
	public class API_Document_NewLoadUsage : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLoadNature
	///</summary>
	[NodeName("API_Document_NewLoadNature")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadNature element within the project.")]
	public class API_Document_NewLoadNature : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform hosted area load with polygonal shape within the project.")]
	public class API_Document_NewAreaLoad : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class API_Document_NewAreaLoad_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted area load with variable forces at the vertices within the project.")]
	public class API_Document_NewAreaLoad_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaLoad
	///</summary>
	[NodeName("API_Document_NewAreaLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new uniform unhosted area load with polygonal shape within the project.")]
	public class API_Document_NewAreaLoad_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at an array of points.")]
	public class API_Document_NewLineLoad : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted line load within the project using data at two points.")]
	public class API_Document_NewLineLoad_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_2")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at an array of points.")]
	public class API_Document_NewLineLoad_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewLineLoad
	///</summary>
	[NodeName("API_Document_NewLineLoad_3")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted line load within the project using data at two points.")]
	public class API_Document_NewLineLoad_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
	///</summary>
	[NodeName("API_Document_NewPointLoad")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new hosted point load within the project.")]
	public class API_Document_NewPointLoad : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPointLoad
	///</summary>
	[NodeName("API_Document_NewPointLoad_1")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unhosted point load within the project.")]
	public class API_Document_NewPointLoad_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewPathReinforcement
	///</summary>
	[NodeName("API_Document_NewPathReinforcement")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a Path Reinforcement element within the project")]
	public class API_Document_NewPathReinforcement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaReinforcement
	///</summary>
	[NodeName("API_Document_NewAreaReinforcement")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an Area Reinforcement element within the project")]
	public class API_Document_NewAreaReinforcement : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Document.NewAreaReinforcement
		///</summary>
		public API_Document_NewAreaReinforcement()
		{
			base_type = typeof(Autodesk.Revit.Creation.Document);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewAreaReinforcement", false, new Type[]{typeof(Autodesk.Revit.DB.Element),typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Document", typeof(object)));
			}
			InPortData.Add(new PortData("el", "Autodesk.Revit.DB.Element",typeof(Autodesk.Revit.DB.Element)));
			InPortData.Add(new PortData("crvs", "Autodesk.Revit.DB.CurveArray",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("xyz", "Autodesk.Revit.DB.XYZ",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates a new instance of an Area Reinforcement element within the project",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Document.NewRebarBarType
	///</summary>
	[NodeName("API_Document_NewRebarBarType")]
	[NodeSearchTags("create","document")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of Rebar Bar Type, which defines the bar diameter, bar bend diameter and bar material of the rebar.")]
	public class API_Document_NewRebarBarType : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteSpline_Parameters")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the params of the Hermite spline.")]
	public class API_HermiteSpline_Parameters : dynAPIPropertyNode
	{
		public API_HermiteSpline_Parameters()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the params of the Hermite spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteSpline_Tangents")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the tangents of the Hermite spline.")]
	public class API_HermiteSpline_Tangents : dynAPIPropertyNode
	{
		public API_HermiteSpline_Tangents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns the tangents of the Hermite spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteSpline_ControlPoints")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The control points of the Hermite spline.")]
	public class API_HermiteSpline_ControlPoints : dynAPIPropertyNode
	{
		public API_HermiteSpline_ControlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","The control points of the Hermite spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_HermiteSpline_IsPeriodic")]
	[NodeSearchTags("curve","hermite","spline")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns whether the Hermite spline is periodic or not.")]
	public class API_HermiteSpline_IsPeriodic : dynAPIPropertyNode
	{
		public API_HermiteSpline_IsPeriodic()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.HermiteSpline",typeof(Autodesk.Revit.DB.HermiteSpline)));
			OutPortData.Add(new PortData("out","Returns whether the Hermite spline is periodic or not.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Profile.Clone
	///</summary>
	[NodeName("API_Profile_Clone")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this profile.")]
	public class API_Profile_Clone : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Profile_Curves")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve the curves that make up the boundary of the profile.")]
	public class API_Profile_Curves : dynAPIPropertyNode
	{
		public API_Profile_Curves()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Retrieve the curves that make up the boundary of the profile.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Profile_Filled")]
	[NodeSearchTags("profile")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get or set whether the profile is filled.")]
	public class API_Profile_Filled : dynAPIPropertyNode
	{
		public API_Profile_Filled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Profile",typeof(Autodesk.Revit.DB.Profile)));
			OutPortData.Add(new PortData("out","Get or set whether the profile is filled.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_MaxSegmentAngle")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The maximum segment angle of the sweep in radians.")]
	public class API_Sweep_MaxSegmentAngle : dynAPIPropertyNode
	{
		public API_Sweep_MaxSegmentAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The maximum segment angle of the sweep in radians.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_IsTrajectorySegmentationEnabled")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The trajectory segmentation option for the sweep.")]
	public class API_Sweep_IsTrajectorySegmentationEnabled : dynAPIPropertyNode
	{
		public API_Sweep_IsTrajectorySegmentationEnabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The trajectory segmentation option for the sweep.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_Path3d")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The selected curves used for the sweep path.")]
	public class API_Sweep_Path3d : dynAPIPropertyNode
	{
		public API_Sweep_Path3d()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The selected curves used for the sweep path.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_PathSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The sketched path for the sweep.")]
	public class API_Sweep_PathSketch : dynAPIPropertyNode
	{
		public API_Sweep_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The sketched path for the sweep.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_ProfileSymbol")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The family symbol profile details for the sweep.")]
	public class API_Sweep_ProfileSymbol : dynAPIPropertyNode
	{
		public API_Sweep_ProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The family symbol profile details for the sweep.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Sweep_ProfileSketch")]
	[NodeSearchTags("generic","form","sweep")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The profile sketch of the sweep.")]
	public class API_Sweep_ProfileSketch : dynAPIPropertyNode
	{
		public API_Sweep_ProfileSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Sweep",typeof(Autodesk.Revit.DB.Sweep)));
			OutPortData.Add(new PortData("out","The profile sketch of the sweep.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.SetInstanceFlipped
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_SetInstanceFlipped")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the value of the flip parameter on the adaptive instance.")]
	public class API_AdaptiveComponentInstanceUtils_SetInstanceFlipped : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsInstanceFlipped
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsInstanceFlipped")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the value of the flip parameter on the adaptive instance.")]
	public class API_AdaptiveComponentInstanceUtils_IsInstanceFlipped : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Shape Handle Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstanceShapeHandlePointElementRefIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Placement Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstancePlacementPointElementRefIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets Adaptive Point Element Ref ids to which the instance geometry adapts.")]
	public class API_AdaptiveComponentInstanceUtils_GetInstancePointElementRefIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.MoveAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_MoveAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves Adaptive Component Instance by the specified transformation.")]
	public class API_AdaptiveComponentInstanceUtils_MoveAdaptiveComponentInstance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a FamilyInstance of Adaptive Component Family.")]
	public class API_AdaptiveComponentInstanceUtils_CreateAdaptiveComponentInstance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilyInstance is an Adaptive Component Instance.")]
	public class API_AdaptiveComponentInstanceUtils_IsAdaptiveComponentInstance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilyInstance has an Adaptive Family Symbol.")]
	public class API_AdaptiveComponentInstanceUtils_HasAdaptiveFamilySymbol : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol
	///</summary>
	[NodeName("API_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol")]
	[NodeSearchTags("adpative","component","family","utils")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Verifies if a FamilySymbol is a valid Adaptive Family Symbol.")]
	public class API_AdaptiveComponentInstanceUtils_IsAdaptiveFamilySymbol : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Clone
	///</summary>
	[NodeName("API_Curve_Clone")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a copy of this curve.")]
	public class API_Curve_Clone : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Project
	///</summary>
	[NodeName("API_Curve_Project")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this curve.")]
	public class API_Curve_Project : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
	///</summary>
	[NodeName("API_Curve_Intersect")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve and returns the intersection results.")]
	public class API_Curve_Intersect : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Intersect
	///</summary>
	[NodeName("API_Curve_Intersect_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of this curve with the specified curve.")]
	public class API_Curve_Intersect_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
	///</summary>
	[NodeName("API_Curve_IsInside")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds and outputs the end index.")]
	public class API_Curve_IsInside : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.IsInside
	///</summary>
	[NodeName("API_Curve_IsInside_1")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified parameter value is within this curve's bounds.")]
	public class API_Curve_IsInside_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeDerivatives
	///</summary>
	[NodeName("API_Curve_ComputeDerivatives")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the curve at the specified parameter.")]
	public class API_Curve_ComputeDerivatives : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Distance
	///</summary>
	[NodeName("API_Curve_Distance")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the shortest distance from the specified point to this curve.")]
	public class API_Curve_Distance : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeRawParameter
	///</summary>
	[NodeName("API_Curve_ComputeRawParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the raw parameter from the normalized parameter.")]
	public class API_Curve_ComputeRawParameter : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.ComputeNormalizedParameter
	///</summary>
	[NodeName("API_Curve_ComputeNormalizedParameter")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Computes the normalized curve parameter from the raw parameter.")]
	public class API_Curve_ComputeNormalizedParameter : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeUnbound
	///</summary>
	[NodeName("API_Curve_MakeUnbound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Makes this curve unbound.")]
	public class API_Curve_MakeUnbound : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.MakeBound
	///</summary>
	[NodeName("API_Curve_MakeBound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Changes the bounds of this curve to the specified values.")]
	public class API_Curve_MakeBound : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Evaluate
	///</summary>
	[NodeName("API_Curve_Evaluate")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the curve.")]
	public class API_Curve_Evaluate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Curve.Tessellate
	///</summary>
	[NodeName("API_Curve_Tessellate")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Valid only if the curve is bound. Returns a polyline approximation to the curve.")]
	public class API_Curve_Tessellate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_Period")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The period of this curve.")]
	public class API_Curve_Period : dynAPIPropertyNode
	{
		public API_Curve_Period()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The period of this curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_IsCyclic")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value that indicates whether this curve is cyclic.")]
	public class API_Curve_IsCyclic : dynAPIPropertyNode
	{
		public API_Curve_IsCyclic()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The boolean value that indicates whether this curve is cyclic.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_Length")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The exact length of the curve.")]
	public class API_Curve_Length : dynAPIPropertyNode
	{
		public API_Curve_Length()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The exact length of the curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_ApproximateLength")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The approximate length of the curve.")]
	public class API_Curve_ApproximateLength : dynAPIPropertyNode
	{
		public API_Curve_ApproximateLength()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","The approximate length of the curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_Reference")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the curve.")]
	public class API_Curve_Reference : dynAPIPropertyNode
	{
		public API_Curve_Reference()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Curve_IsBound")]
	[NodeSearchTags("curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Describes whether the parameter of the curve is restricted to a particular interval.")]
	public class API_Curve_IsBound : dynAPIPropertyNode
	{
		public API_Curve_IsBound()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Curve",typeof(Autodesk.Revit.DB.Curve)));
			OutPortData.Add(new PortData("out","Describes whether the parameter of the curve is restricted to a particular interval.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.ChangeToReferenceLine
	///</summary>
	[NodeName("API_ModelCurve_ChangeToReferenceLine")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Changes this curve to a reference curve.")]
	public class API_ModelCurve_ChangeToReferenceLine : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.SetVisibility
	///</summary>
	[NodeName("API_ModelCurve_SetVisibility")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the visibility for the model curve in a family document.")]
	public class API_ModelCurve_SetVisibility : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.SetVisibility
		///</summary>
		public API_ModelCurve_SetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.ModelCurve);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetVisibility", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyElementVisibility)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyElementVisibility",typeof(Autodesk.Revit.DB.FamilyElementVisibility)));
			OutPortData.Add(new PortData("out","Sets the visibility for the model curve in a family document.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ModelCurve.GetVisibility
	///</summary>
	[NodeName("API_ModelCurve_GetVisibility")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the model curve in a family document.")]
	public class API_ModelCurve_GetVisibility : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_ModelCurve_IsReferenceLine")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates if this curve is a reference curve.")]
	public class API_ModelCurve_IsReferenceLine : dynAPIPropertyNode
	{
		public API_ModelCurve_IsReferenceLine()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","Indicates if this curve is a reference curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ModelCurve_TrussCurveType")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The truss curve type of this model curve.")]
	public class API_ModelCurve_TrussCurveType : dynAPIPropertyNode
	{
		public API_ModelCurve_TrussCurveType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The truss curve type of this model curve.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_ModelCurve_Subcategory")]
	[NodeSearchTags("model","curve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The subcategory.")]
	public class API_ModelCurve_Subcategory : dynAPIPropertyNode
	{
		public API_ModelCurve_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.ModelCurve",typeof(Autodesk.Revit.DB.ModelCurve)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
	///</summary>
	[NodeName("API_Form_Rehost")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rehost Form to sketch plane")]
	public class API_Form_Rehost : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.Rehost
	///</summary>
	[NodeName("API_Form_Rehost_1")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rehost Form to edge, face or curve.")]
	public class API_Form_Rehost_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddProfile
	///</summary>
	[NodeName("API_Form_AddProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add a profile into the form, by a specified edge/param.")]
	public class API_Form_AddProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on same/different profile, by a pair of specified points.")]
	public class API_Form_AddEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge_1")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on same/different profile, by a pair of specified edge/param.")]
	public class API_Form_AddEdge_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.AddEdge
	///</summary>
	[NodeName("API_Form_AddEdge_2")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Add an edge to the form, connecting two edges on different profiles, by a specified face of the form and a point on face.")]
	public class API_Form_AddEdge_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleProfile
	///</summary>
	[NodeName("API_Form_ScaleProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scale a profile of the form, by a specified origin and scale factor.")]
	public class API_Form_ScaleProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ScaleSubElement
	///</summary>
	[NodeName("API_Form_ScaleSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Scale a face/edge/curve/vertex of the form, by a specified origin and scale factor.")]
	public class API_Form_ScaleSubElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.RotateProfile
	///</summary>
	[NodeName("API_Form_RotateProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotate a profile of the form, by a specified angle around a given axis.")]
	public class API_Form_RotateProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.RotateSubElement
	///</summary>
	[NodeName("API_Form_RotateSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotate a face/edge/curve/vertex of the form, by a specified angle around a given axis.")]
	public class API_Form_RotateSubElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.MoveProfile
	///</summary>
	[NodeName("API_Form_MoveProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Move a profile of the form, specified by a reference, and an offset vector.")]
	public class API_Form_MoveProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.MoveSubElement
	///</summary>
	[NodeName("API_Form_MoveSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Move a face/edge/curve/vertex of the form, specified by a reference, and an offset vector.")]
	public class API_Form_MoveSubElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteProfile
	///</summary>
	[NodeName("API_Form_DeleteProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Delete a profile of the form.")]
	public class API_Form_DeleteProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.DeleteSubElement
	///</summary>
	[NodeName("API_Form_DeleteSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Delete a face/edge/curve/vertex of the form, specified by a reference.")]
	public class API_Form_DeleteSubElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateProfile
	///</summary>
	[NodeName("API_Form_CanManipulateProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if a profile can be deleted/moved/rotated.")]
	public class API_Form_CanManipulateProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.CanManipulateSubElement
	///</summary>
	[NodeName("API_Form_CanManipulateSubElement")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if a sub element can be deleted/moved/rotated/scaled.")]
	public class API_Form_CanManipulateSubElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetCurvesAndEdgesReference
	///</summary>
	[NodeName("API_Form_GetCurvesAndEdgesReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a point, return all edges and curves that it is lying on.")]
	public class API_Form_GetCurvesAndEdgesReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetControlPoints
	///</summary>
	[NodeName("API_Form_GetControlPoints")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given an edge or a curve or a face, return all control points lying on it (in form of geometry references).")]
	public class API_Form_GetControlPoints : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsConnectingEdge
	///</summary>
	[NodeName("API_Form_IsConnectingEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge is a connecting edge on a side face. Connecting edges connect vertices on different profiles.")]
	public class API_Form_IsConnectingEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsProfileEdge
	///</summary>
	[NodeName("API_Form_IsProfileEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge or curve is generated from a profile.")]
	public class API_Form_IsProfileEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsAutoCreaseEdge
	///</summary>
	[NodeName("API_Form_IsAutoCreaseEdge")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if an edge is an auto-crease on a top/bottom cap face.")]
	public class API_Form_IsAutoCreaseEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsSideFace
	///</summary>
	[NodeName("API_Form_IsSideFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is a side face.")]
	public class API_Form_IsSideFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsEndFace
	///</summary>
	[NodeName("API_Form_IsEndFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is an end cap face.")]
	public class API_Form_IsEndFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsBeginningFace
	///</summary>
	[NodeName("API_Form_IsBeginningFace")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a face, tell if it is a beginning cap face.")]
	public class API_Form_IsBeginningFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsReferenceOnlyProfile
	///</summary>
	[NodeName("API_Form_IsReferenceOnlyProfile")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the profile is made by referencing existing geometry in the Revit model.")]
	public class API_Form_IsReferenceOnlyProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsFaceReference
	///</summary>
	[NodeName("API_Form_IsFaceReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a face of the form.")]
	public class API_Form_IsFaceReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsCurveReference
	///</summary>
	[NodeName("API_Form_IsCurveReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a curve of the form.")]
	public class API_Form_IsCurveReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsEdgeReference
	///</summary>
	[NodeName("API_Form_IsEdgeReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to an edge of the form.")]
	public class API_Form_IsEdgeReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.IsVertexReference
	///</summary>
	[NodeName("API_Form_IsVertexReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the pick is the reference to a vertex of the form.")]
	public class API_Form_IsVertexReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.ConstrainProfiles
	///</summary>
	[NodeName("API_Form_ConstrainProfiles")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Constrain form profiles using the specified profile as master. This is an advanced version of property \"AreProfilesConstrained\", allowing specify the master profile.")]
	public class API_Form_ConstrainProfiles : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Form.GetPathCurveIndexByCurveReference
	///</summary>
	[NodeName("API_Form_GetPathCurveIndexByCurveReference")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Given a reference to certain curve in the path, return its index.")]
	public class API_Form_GetPathCurveIndexByCurveReference : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_BaseOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve/set the base offset of the form object. It is only valid for locked form.")]
	public class API_Form_BaseOffset : dynAPIPropertyNode
	{
		public API_Form_BaseOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the base offset of the form object. It is only valid for locked form.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_TopOffset")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve/set the top offset of the form object. It is only valid for locked form.")]
	public class API_Form_TopOffset : dynAPIPropertyNode
	{
		public API_Form_TopOffset()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Retrieve/set the top offset of the form object. It is only valid for locked form.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_HasOpenGeometry")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the form has an open geometry.")]
	public class API_Form_HasOpenGeometry : dynAPIPropertyNode
	{
		public API_Form_HasOpenGeometry()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has an open geometry.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_AreProfilesConstrained")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get/set if the form's profiles are constrained.")]
	public class API_Form_AreProfilesConstrained : dynAPIPropertyNode
	{
		public API_Form_AreProfilesConstrained()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form's profiles are constrained.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_IsInXRayMode")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get/set if the form is in X-Ray mode.")]
	public class API_Form_IsInXRayMode : dynAPIPropertyNode
	{
		public API_Form_IsInXRayMode()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Get/set if the form is in X-Ray mode.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_HasOneOrMoreReferenceProfiles")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Tell if the form has any reference profile.")]
	public class API_Form_HasOneOrMoreReferenceProfiles : dynAPIPropertyNode
	{
		public API_Form_HasOneOrMoreReferenceProfiles()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","Tell if the form has any reference profile.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_PathCurveCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of curves in the form path.")]
	public class API_Form_PathCurveCount : dynAPIPropertyNode
	{
		public API_Form_PathCurveCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of curves in the form path.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Form_ProfileCount")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The number of profiles in the form.")]
	public class API_Form_ProfileCount : dynAPIPropertyNode
	{
		public API_Form_ProfileCount()
		{
			InPortData.Add(new PortData("frm", "Autodesk.Revit.DB.Form",typeof(Autodesk.Revit.DB.Form)));
			OutPortData.Add(new PortData("out","The number of profiles in the form.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
	///</summary>
	[NodeName("API_BoundingBoxUV")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates object with supplied values.")]
	public class API_BoundingBoxUV : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.BoundingBoxUV.#ctor
	///</summary>
	[NodeName("API_BoundingBoxUV_1")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("default constructor")]
	public class API_BoundingBoxUV_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxUV_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Maximum coordinates (upper-right corner of the box).")]
	public class API_BoundingBoxUV_Max : dynAPIPropertyNode
	{
		public API_BoundingBoxUV_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right corner of the box).",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxUV_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Minimum coordinates (lower-left corner of the box).")]
	public class API_BoundingBoxUV_Min : dynAPIPropertyNode
	{
		public API_BoundingBoxUV_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxUV",typeof(Autodesk.Revit.DB.BoundingBoxUV)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left corner of the box).",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTotalTransform
	///</summary>
	[NodeName("API_Instance_GetTotalTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the total transform, which includes the true north transform for instances like import instances.")]
	public class API_Instance_GetTotalTransform : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Instance.GetTransform
	///</summary>
	[NodeName("API_Instance_GetTransform")]
	[NodeSearchTags("instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the transform of the instance.")]
	public class API_Instance_GetTransform : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Project
	///</summary>
	[NodeName("API_Face_Project")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Projects the specified point on this face.")]
	public class API_Face_Project : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face and returns the intersection results.")]
	public class API_Face_Intersect : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
	///</summary>
	[NodeName("API_Face_Intersect_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Calculates the intersection of the specified curve with this face.")]
	public class API_Face_Intersect_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Face.Intersect
		///</summary>
		public API_Face_Intersect_1()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
	///</summary>
	[NodeName("API_Face_IsInside")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face and outputs additional results.")]
	public class API_Face_IsInside : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.IsInside
	///</summary>
	[NodeName("API_Face_IsInside_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates whether the specified point is within this face.")]
	public class API_Face_IsInside_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeNormal
	///</summary>
	[NodeName("API_Face_ComputeNormal")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal vector for the face at the given point.")]
	public class API_Face_ComputeNormal : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.ComputeDerivatives
	///</summary>
	[NodeName("API_Face_ComputeDerivatives")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the face at the specified point.")]
	public class API_Face_ComputeDerivatives : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.GetBoundingBox
	///</summary>
	[NodeName("API_Face_GetBoundingBox")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the UV bounding box of the face.")]
	public class API_Face_GetBoundingBox : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Evaluate
	///</summary>
	[NodeName("API_Face_Evaluate")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates parameters on the face.")]
	public class API_Face_Evaluate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
	///</summary>
	[NodeName("API_Face_Triangulate")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class API_Face_Triangulate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.Triangulate
	///</summary>
	[NodeName("API_Face_Triangulate_1")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a triangular mesh approximation to the face.")]
	public class API_Face_Triangulate_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Face.GetRegions
	///</summary>
	[NodeName("API_Face_GetRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Face regions (created with the Split Face command) of the face.")]
	public class API_Face_GetRegions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_Area")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The area of this face.")]
	public class API_Face_Area : dynAPIPropertyNode
	{
		public API_Face_Area()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","The area of this face.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_Reference")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the face.")]
	public class API_Face_Reference : dynAPIPropertyNode
	{
		public API_Face_Reference()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the face.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_IsTwoSided")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines if a face is two-sided (degenerate)")]
	public class API_Face_IsTwoSided : dynAPIPropertyNode
	{
		public API_Face_IsTwoSided()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Determines if a face is two-sided (degenerate)",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_MaterialElementId")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Element ID of the material from which this face is composed.")]
	public class API_Face_MaterialElementId : dynAPIPropertyNode
	{
		public API_Face_MaterialElementId()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Element ID of the material from which this face is composed.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_EdgeLoops")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Each edge loop is a closed boundary of the face.")]
	public class API_Face_EdgeLoops : dynAPIPropertyNode
	{
		public API_Face_EdgeLoops()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Each edge loop is a closed boundary of the face.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Face_HasRegions")]
	[NodeSearchTags("face")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Reports if the face contains regions created with the Split Face command.")]
	public class API_Face_HasRegions : dynAPIPropertyNode
	{
		public API_Face_HasRegions()
		{
			InPortData.Add(new PortData("f", "Autodesk.Revit.DB.Face",typeof(Autodesk.Revit.DB.Face)));
			OutPortData.Add(new PortData("out","Reports if the face contains regions created with the Split Face command.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxXYZ_Enabled")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Defines whether bounding box is turned on.")]
	public class API_BoundingBoxXYZ_Enabled : dynAPIPropertyNode
	{
		public API_BoundingBoxXYZ_Enabled()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Defines whether bounding box is turned on.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxXYZ_Max")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Maximum coordinates (upper-right-front corner of the box).")]
	public class API_BoundingBoxXYZ_Max : dynAPIPropertyNode
	{
		public API_BoundingBoxXYZ_Max()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Maximum coordinates (upper-right-front corner of the box).",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxXYZ_Min")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Minimum coordinates (lower-left-rear corner of the box).")]
	public class API_BoundingBoxXYZ_Min : dynAPIPropertyNode
	{
		public API_BoundingBoxXYZ_Min()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","Minimum coordinates (lower-left-rear corner of the box).",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_BoundingBoxXYZ_Transform")]
	[NodeSearchTags("bounding","box","bounds","bbox")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transform FROM the coordinate space of the box TO the model space.")]
	public class API_BoundingBoxXYZ_Transform : dynAPIPropertyNode
	{
		public API_BoundingBoxXYZ_Transform()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.BoundingBoxXYZ",typeof(Autodesk.Revit.DB.BoundingBoxXYZ)));
			OutPortData.Add(new PortData("out","The transform FROM the coordinate space of the box TO the model space.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetOriginalGeometry
	///</summary>
	[NodeName("API_FamilyInstance_GetOriginalGeometry")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the original geometry of the instance, before the instance is modified by joins, cuts, coping, extensions, or other post-processing.")]
	public class API_FamilyInstance_GetOriginalGeometry : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetFamilyPointPlacementReferences
	///</summary>
	[NodeName("API_FamilyInstance_GetFamilyPointPlacementReferences")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Point Placement References for the Family Instance.")]
	public class API_FamilyInstance_GetFamilyPointPlacementReferences : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.RemoveCoping
	///</summary>
	[NodeName("API_FamilyInstance_RemoveCoping")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes a coping (cut) from a steel beam.")]
	public class API_FamilyInstance_RemoveCoping : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.AddCoping
	///</summary>
	[NodeName("API_FamilyInstance_AddCoping")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds a coping (cut) to a steel beam.")]
	public class API_FamilyInstance_AddCoping : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.SetCopingIds
	///</summary>
	[NodeName("API_FamilyInstance_SetCopingIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Specifies the set of coping cutters on this element.")]
	public class API_FamilyInstance_SetCopingIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetCopingIds
	///</summary>
	[NodeName("API_FamilyInstance_GetCopingIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Lists the elements currently used as coping cutters for this element.")]
	public class API_FamilyInstance_GetCopingIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.SetCopings
	///</summary>
	[NodeName("API_FamilyInstance_SetCopings")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Specify the set of coping cutters on this element.")]
	public class API_FamilyInstance_SetCopings : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.SetCopings
		///</summary>
		public API_FamilyInstance_SetCopings()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetCopings", false, new Type[]{typeof(Autodesk.Revit.DB.ElementSet)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance", typeof(object)));
			}
			InPortData.Add(new PortData("val", "A set of coping cutters (steel beams and steel columns).",typeof(Autodesk.Revit.DB.ElementSet)));
			OutPortData.Add(new PortData("out","Specify the set of coping cutters on this element.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetCopings
	///</summary>
	[NodeName("API_FamilyInstance_GetCopings")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Lists the elements currently used as coping cutters for this element.")]
	public class API_FamilyInstance_GetCopings : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetCopings
		///</summary>
		public API_FamilyInstance_GetCopings()
		{
			base_type = typeof(Autodesk.Revit.DB.FamilyInstance);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetCopings", false, new Type[]{}, out return_type);
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.GetSubComponentIds
	///</summary>
	[NodeName("API_FamilyInstance_GetSubComponentIds")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the sub component ElementIds of the current family instance.")]
	public class API_FamilyInstance_GetSubComponentIds : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.FlipFromToRoom
	///</summary>
	[NodeName("API_FamilyInstance_FlipFromToRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Flips the settings of \"From Room\" and \"To Room\" for the door or window instance.")]
	public class API_FamilyInstance_FlipFromToRoom : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.rotate
	///</summary>
	[NodeName("API_FamilyInstance_rotate")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The family instance will be flipped by 180 degrees. If it can not be rotated, return false, otherwise return true.")]
	public class API_FamilyInstance_rotate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipFacing
	///</summary>
	[NodeName("API_FamilyInstance_flipFacing")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The orientation of family instance facing will be flipped. If it can not be flipped, return false, otherwise return true.")]
	public class API_FamilyInstance_flipFacing : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.FamilyInstance.flipHand
	///</summary>
	[NodeName("API_FamilyInstance_flipHand")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The orientation of family instance hand will be flipped. If it can not be flipped, return false, otherwise return true.")]
	public class API_FamilyInstance_flipHand : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_IsWorkPlaneFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the instance's work plane is flipped.")]
	public class API_FamilyInstance_IsWorkPlaneFlipped : dynAPIPropertyNode
	{
		public API_FamilyInstance_IsWorkPlaneFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance's work plane is flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_CanFlipWorkPlane")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the instance can flip its work plane.")]
	public class API_FamilyInstance_CanFlipWorkPlane : dynAPIPropertyNode
	{
		public API_FamilyInstance_CanFlipWorkPlane()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies if the instance can flip its work plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_IsSlantedColumn")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Indicates if the family instance is a slanted column.")]
	public class API_FamilyInstance_IsSlantedColumn : dynAPIPropertyNode
	{
		public API_FamilyInstance_IsSlantedColumn()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Indicates if the family instance is a slanted column.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_ExtensionUtility")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to check whether the instance can be extended and return the interface for extension operation.")]
	public class API_FamilyInstance_ExtensionUtility : dynAPIPropertyNode
	{
		public API_FamilyInstance_ExtensionUtility()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to check whether the instance can be extended and return the interface for extension operation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_SuperComponent")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the super component of current family instance.")]
	public class API_FamilyInstance_SuperComponent : dynAPIPropertyNode
	{
		public API_FamilyInstance_SuperComponent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the super component of current family instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_SubComponents")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the sub components of current family instance.")]
	public class API_FamilyInstance_SubComponents : dynAPIPropertyNode
	{
		public API_FamilyInstance_SubComponents()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the sub components of current family instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_ToRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The \"To Room\" set for the door or window in the last phase of the project.")]
	public class API_FamilyInstance_ToRoom : dynAPIPropertyNode
	{
		public API_FamilyInstance_ToRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"To Room\" set for the door or window in the last phase of the project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_FromRoom")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The \"From Room\" set for the door or window in the last phase of the project.")]
	public class API_FamilyInstance_FromRoom : dynAPIPropertyNode
	{
		public API_FamilyInstance_FromRoom()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The \"From Room\" set for the door or window in the last phase of the project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_CanRotate")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance can be rotated by 180 degrees.")]
	public class API_FamilyInstance_CanRotate : dynAPIPropertyNode
	{
		public API_FamilyInstance_CanRotate()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance can be rotated by 180 degrees.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_CanFlipFacing")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance facing can be flipped.")]
	public class API_FamilyInstance_CanFlipFacing : dynAPIPropertyNode
	{
		public API_FamilyInstance_CanFlipFacing()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing can be flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_CanFlipHand")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance hand can be flipped.")]
	public class API_FamilyInstance_CanFlipHand : dynAPIPropertyNode
	{
		public API_FamilyInstance_CanFlipHand()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand can be flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Mirrored")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance is mirrored. (only one axis is flipped)")]
	public class API_FamilyInstance_Mirrored : dynAPIPropertyNode
	{
		public API_FamilyInstance_Mirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is mirrored. (only one axis is flipped)",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Invisible")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the family instance is invisible.")]
	public class API_FamilyInstance_Invisible : dynAPIPropertyNode
	{
		public API_FamilyInstance_Invisible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the family instance is invisible.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_FacingFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance facing is flipped.")]
	public class API_FamilyInstance_FacingFlipped : dynAPIPropertyNode
	{
		public API_FamilyInstance_FacingFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance facing is flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_HandFlipped")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the orientation of family instance hand is flipped.")]
	public class API_FamilyInstance_HandFlipped : dynAPIPropertyNode
	{
		public API_FamilyInstance_HandFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to test whether the orientation of family instance hand is flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_FacingOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the orientation of family instance facing.")]
	public class API_FamilyInstance_FacingOrientation : dynAPIPropertyNode
	{
		public API_FamilyInstance_FacingOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance facing.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_HandOrientation")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the orientation of family instance hand.")]
	public class API_FamilyInstance_HandOrientation : dynAPIPropertyNode
	{
		public API_FamilyInstance_HandOrientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the orientation of family instance hand.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_HostFace")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to get the reference to the host face of family instance.")]
	public class API_FamilyInstance_HostFace : dynAPIPropertyNode
	{
		public API_FamilyInstance_HostFace()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Property to get the reference to the host face of family instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Host")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.")]
	public class API_FamilyInstance_Host : dynAPIPropertyNode
	{
		public API_FamilyInstance_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","If the instance is contained within another element, this property returns the containingelement. An instance that is face hosted will return the element containing the face.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Location")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This property is used to find the physical location of an instance within project.")]
	public class API_FamilyInstance_Location : dynAPIPropertyNode
	{
		public API_FamilyInstance_Location()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property is used to find the physical location of an instance within project.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Space")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The space in which the instance is located (during the last phase of the project).")]
	public class API_FamilyInstance_Space : dynAPIPropertyNode
	{
		public API_FamilyInstance_Space()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The space in which the instance is located (during the last phase of the project).",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Room")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The room in which the instance is located (during the last phase of the project).")]
	public class API_FamilyInstance_Room : dynAPIPropertyNode
	{
		public API_FamilyInstance_Room()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","The room in which the instance is located (during the last phase of the project).",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_StructuralType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Provides the primary structural type of the instance, such as beam or column etc.")]
	public class API_FamilyInstance_StructuralType : dynAPIPropertyNode
	{
		public API_FamilyInstance_StructuralType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural type of the instance, such as beam or column etc.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_StructuralUsage")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Provides the primary structural usage of the instance, such as brace, girder etc.")]
	public class API_FamilyInstance_StructuralUsage : dynAPIPropertyNode
	{
		public API_FamilyInstance_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Provides the primary structural usage of the instance, such as brace, girder etc.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_StructuralMaterialId")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies the material that defines the instance's structural analysis properties.")]
	public class API_FamilyInstance_StructuralMaterialId : dynAPIPropertyNode
	{
		public API_FamilyInstance_StructuralMaterialId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Identifies the material that defines the instance's structural analysis properties.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_StructuralMaterialType")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("This property returns the physical material from which the instance is made.")]
	public class API_FamilyInstance_StructuralMaterialType : dynAPIPropertyNode
	{
		public API_FamilyInstance_StructuralMaterialType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","This property returns the physical material from which the instance is made.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_MEPModel")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves the MEP model for the family instance.")]
	public class API_FamilyInstance_MEPModel : dynAPIPropertyNode
	{
		public API_FamilyInstance_MEPModel()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Retrieves the MEP model for the family instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_FamilyInstance_Symbol")]
	[NodeSearchTags("family","instance")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns or changes the FamilySymbol object that represents the type of the instance.")]
	public class API_FamilyInstance_Symbol : dynAPIPropertyNode
	{
		public API_FamilyInstance_Symbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyInstance",typeof(Autodesk.Revit.DB.FamilyInstance)));
			OutPortData.Add(new PortData("out","Returns or changes the FamilySymbol object that represents the type of the instance.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Color.#ctor
	///</summary>
	[NodeName("API_Color")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Constructor that takes the red, green and blue channels of the color.")]
	public class API_Color : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Color_IsValid")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the color represents a valid color, or an uninitialized/invalid value.")]
	public class API_Color_IsValid : dynAPIPropertyNode
	{
		public API_Color_IsValid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Identifies if the color represents a valid color, or an uninitialized/invalid value.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Color_Blue")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get or set the blue channel of the color.")]
	public class API_Color_Blue : dynAPIPropertyNode
	{
		public API_Color_Blue()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the blue channel of the color.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Color_Green")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get or set the green channel of the color.")]
	public class API_Color_Green : dynAPIPropertyNode
	{
		public API_Color_Green()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the green channel of the color.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Color_Red")]
	[NodeSearchTags("color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get or set the red channel of the color.")]
	public class API_Color_Red : dynAPIPropertyNode
	{
		public API_Color_Red()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Color",typeof(Autodesk.Revit.DB.Color)));
			OutPortData.Add(new PortData("out","Get or set the red channel of the color.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GenericForm.SetVisibility
	///</summary>
	[NodeName("API_GenericForm_SetVisibility")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the visibility for the generic form.")]
	public class API_GenericForm_SetVisibility : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.GenericForm.SetVisibility
		///</summary>
		public API_GenericForm_SetVisibility()
		{
			base_type = typeof(Autodesk.Revit.DB.GenericForm);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetVisibility", false, new Type[]{typeof(Autodesk.Revit.DB.FamilyElementVisibility)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.FamilyElementVisibility",typeof(Autodesk.Revit.DB.FamilyElementVisibility)));
			OutPortData.Add(new PortData("out","Sets the visibility for the generic form.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.GenericForm.GetVisibility
	///</summary>
	[NodeName("API_GenericForm_GetVisibility")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the visibility for the generic form.")]
	public class API_GenericForm_GetVisibility : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_GenericForm_Subcategory")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The subcategory.")]
	public class API_GenericForm_Subcategory : dynAPIPropertyNode
	{
		public API_GenericForm_Subcategory()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The subcategory.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_GenericForm_Name")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get and Set the Name property")]
	public class API_GenericForm_Name : dynAPIPropertyNode
	{
		public API_GenericForm_Name()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Get and Set the Name property",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_GenericForm_IsSolid")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Identifies if the GenericForm is a solid or a void element.")]
	public class API_GenericForm_IsSolid : dynAPIPropertyNode
	{
		public API_GenericForm_IsSolid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","Identifies if the GenericForm is a solid or a void element.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_GenericForm_Visible")]
	[NodeSearchTags("generic","form")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The visibility of the GenericForm.")]
	public class API_GenericForm_Visible : dynAPIPropertyNode
	{
		public API_GenericForm_Visible()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GenericForm",typeof(Autodesk.Revit.DB.GenericForm)));
			OutPortData.Add(new PortData("out","The visibility of the GenericForm.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Point_Reference")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the point.")]
	public class API_Point_Reference : dynAPIPropertyNode
	{
		public API_Point_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the point.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Point_Coord")]
	[NodeSearchTags("point","pt")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the coordinates of the point.")]
	public class API_Point_Coord : dynAPIPropertyNode
	{
		public API_Point_Coord()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Point",typeof(Autodesk.Revit.DB.Point)));
			OutPortData.Add(new PortData("out","Returns the coordinates of the point.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.CanBeIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_CanBeIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Checks if the element can be an intersection reference.")]
	public class API_DividedSurface_CanBeIntersectionElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveAllIntersectionElements
	///</summary>
	[NodeName("API_DividedSurface_RemoveAllIntersectionElements")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes all the intersection elements from a divided surface.")]
	public class API_DividedSurface_RemoveAllIntersectionElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.RemoveIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_RemoveIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes an intersection element from a divided surface.")]
	public class API_DividedSurface_RemoveIntersectionElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.AddIntersectionElement
	///</summary>
	[NodeName("API_DividedSurface_AddIntersectionElement")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds an intersection element to the divided surface.")]
	public class API_DividedSurface_AddIntersectionElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetAllIntersectionElements
	///</summary>
	[NodeName("API_DividedSurface_GetAllIntersectionElements")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets all intersection elements which produce division lines.")]
	public class API_DividedSurface_GetAllIntersectionElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetTileFamilyInstance
	///</summary>
	[NodeName("API_DividedSurface_GetTileFamilyInstance")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get a reference to a tile elementassociated with a given seed node.")]
	public class API_DividedSurface_GetTileFamilyInstance : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetTileFamilyInstance
		///</summary>
		public API_DividedSurface_GetTileFamilyInstance()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTileFamilyInstance", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Get a reference to a tile elementassociated with a given seed node.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetTileReference
	///</summary>
	[NodeName("API_DividedSurface_GetTileReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get a reference to one of the tile surfacesassociated with a given seed node.")]
	public class API_DividedSurface_GetTileReference : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetTileReference
		///</summary>
		public API_DividedSurface_GetTileReference()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetTileReference", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode),typeof(System.Int32)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			InPortData.Add(new PortData("i", "System.Int32",typeof(System.Int32)));
			OutPortData.Add(new PortData("out","Get a reference to one of the tile surfacesassociated with a given seed node.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.IsSeedNode
	///</summary>
	[NodeName("API_DividedSurface_IsSeedNode")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Reports whether a grid node is a \"seed node,\" a nodethat is associated with one or more tiles.")]
	public class API_DividedSurface_IsSeedNode : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.IsSeedNode
		///</summary>
		public API_DividedSurface_IsSeedNode()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "IsSeedNode", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			OutPortData.Add(new PortData("out","Reports whether a grid node is a \"seed node,\" a nodethat is associated with one or more tiles.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridSegmentReference
	///</summary>
	[NodeName("API_DividedSurface_GetGridSegmentReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get a reference to a line segment connectingtwo adjacent grid nodes.")]
	public class API_DividedSurface_GetGridSegmentReference : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridSegmentReference
		///</summary>
		public API_DividedSurface_GetGridSegmentReference()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetGridSegmentReference", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode),typeof(Autodesk.Revit.DB.GridSegmentDirection)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridSegmentDirection",typeof(Autodesk.Revit.DB.GridSegmentDirection)));
			OutPortData.Add(new PortData("out","Get a reference to a line segment connectingtwo adjacent grid nodes.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeReference
	///</summary>
	[NodeName("API_DividedSurface_GetGridNodeReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get a reference to the geometric pointassociated with a grid node.")]
	public class API_DividedSurface_GetGridNodeReference : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeReference
		///</summary>
		public API_DividedSurface_GetGridNodeReference()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetGridNodeReference", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			OutPortData.Add(new PortData("out","Get a reference to the geometric pointassociated with a grid node.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeLocation
	///</summary>
	[NodeName("API_DividedSurface_GetGridNodeLocation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Specify whether a particular grid node is interior to the surface, on the boundary, or outsidethe boundary.")]
	public class API_DividedSurface_GetGridNodeLocation : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeLocation
		///</summary>
		public API_DividedSurface_GetGridNodeLocation()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetGridNodeLocation", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			OutPortData.Add(new PortData("out","Specify whether a particular grid node is interior to the surface, on the boundary, or outsidethe boundary.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeUV
	///</summary>
	[NodeName("API_DividedSurface_GetGridNodeUV")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the position of a grid node in UVcoordinates in the surface.")]
	public class API_DividedSurface_GetGridNodeUV : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.DividedSurface.GetGridNodeUV
		///</summary>
		public API_DividedSurface_GetGridNodeUV()
		{
			base_type = typeof(Autodesk.Revit.DB.DividedSurface);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetGridNodeUV", false, new Type[]{typeof(Autodesk.Revit.DB.GridNode)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.GridNode",typeof(Autodesk.Revit.DB.GridNode)));
			OutPortData.Add(new PortData("out","Get the position of a grid node in UVcoordinates in the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_NumberOfVGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the number of V-gridlines used on thesurface.")]
	public class API_DividedSurface_NumberOfVGridlines : dynAPIPropertyNode
	{
		public API_DividedSurface_NumberOfVGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of V-gridlines used on thesurface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_NumberOfUGridlines")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the number of U-gridlines used on thesurface.")]
	public class API_DividedSurface_NumberOfUGridlines : dynAPIPropertyNode
	{
		public API_DividedSurface_NumberOfUGridlines()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Get the number of U-gridlines used on thesurface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_IsComponentFlipped")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the pattern is flipped.")]
	public class API_DividedSurface_IsComponentFlipped : dynAPIPropertyNode
	{
		public API_DividedSurface_IsComponentFlipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_IsComponentMirrored")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the pattern is mirror-imaged.")]
	public class API_DividedSurface_IsComponentMirrored : dynAPIPropertyNode
	{
		public API_DividedSurface_IsComponentMirrored()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Whether the pattern is mirror-imaged.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_ComponentRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The rotation of the pattern by a multipleof 90 degrees.")]
	public class API_DividedSurface_ComponentRotation : dynAPIPropertyNode
	{
		public API_DividedSurface_ComponentRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The rotation of the pattern by a multipleof 90 degrees.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_VPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset applied to the pattern by an integral number of grid nodes in the V-direction.")]
	public class API_DividedSurface_VPatternIndent : dynAPIPropertyNode
	{
		public API_DividedSurface_VPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by an integral number of grid nodes in the V-direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_UPatternIndent")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset applied to the pattern by anintegral number of grid nodes in the U-direction.")]
	public class API_DividedSurface_UPatternIndent : dynAPIPropertyNode
	{
		public API_DividedSurface_UPatternIndent()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The offset applied to the pattern by anintegral number of grid nodes in the U-direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_BorderTile")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines the handling of tiles that overlap the surface'sboundary.")]
	public class API_DividedSurface_BorderTile : dynAPIPropertyNode
	{
		public API_DividedSurface_BorderTile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Determines the handling of tiles that overlap the surface'sboundary.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_AllGridRotation")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Angle of rotation applied to the U- and V- directions together.")]
	public class API_DividedSurface_AllGridRotation : dynAPIPropertyNode
	{
		public API_DividedSurface_AllGridRotation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Angle of rotation applied to the U- and V- directions together.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_VSpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Access to the rule for laying out the second series of equidistantparallel lines on the surface.")]
	public class API_DividedSurface_VSpacingRule : dynAPIPropertyNode
	{
		public API_DividedSurface_VSpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the second series of equidistantparallel lines on the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_USpacingRule")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Access to the rule for laying out the first series of equidistantparallel lines on the surface.")]
	public class API_DividedSurface_USpacingRule : dynAPIPropertyNode
	{
		public API_DividedSurface_USpacingRule()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","Access to the rule for laying out the first series of equidistantparallel lines on the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_HostReference")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("A reference to the divided face on the host.")]
	public class API_DividedSurface_HostReference : dynAPIPropertyNode
	{
		public API_DividedSurface_HostReference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","A reference to the divided face on the host.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_DividedSurface_Host")]
	[NodeSearchTags("divide","divided","surface","adaptive","components")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The element whose surface has been divided.")]
	public class API_DividedSurface_Host : dynAPIPropertyNode
	{
		public API_DividedSurface_Host()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.DividedSurface",typeof(Autodesk.Revit.DB.DividedSurface)));
			OutPortData.Add(new PortData("out","The element whose surface has been divided.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
	///</summary>
	[NodeName("API_PointCloudInstance_GetPoints")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Extracts a collection of points based on a filter.")]
	public class API_PointCloudInstance_GetPoints : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetPoints
		///</summary>
		public API_PointCloudInstance_GetPoints()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.Create
	///</summary>
	[NodeName("API_PointCloudInstance_Create")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a point cloud based on an input point cloud type and transformation.")]
	public class API_PointCloudInstance_Create : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.SetSelectionFilter
	///</summary>
	[NodeName("API_PointCloudInstance_SetSelectionFilter")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets active selection filter by cloning of the one passed to it.")]
	public class API_PointCloudInstance_SetSelectionFilter : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.PointCloudInstance.GetSelectionFilter
	///</summary>
	[NodeName("API_PointCloudInstance_GetSelectionFilter")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the currently active selection filter for this point cloud.")]
	public class API_PointCloudInstance_GetSelectionFilter : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_PointCloudInstance_FilterAction")]
	[NodeSearchTags("point","cloud")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The action taken based on the results of the selection filter applied to this point cloud.")]
	public class API_PointCloudInstance_FilterAction : dynAPIPropertyNode
	{
		public API_PointCloudInstance_FilterAction()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.PointCloudInstance",typeof(Autodesk.Revit.DB.PointCloudInstance)));
			OutPortData.Add(new PortData("out","The action taken based on the results of the selection filter applied to this point cloud.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_RadiusY")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Y vector radius of the ellipse.")]
	public class API_Ellipse_RadiusY : dynAPIPropertyNode
	{
		public API_Ellipse_RadiusY()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the Y vector radius of the ellipse.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_RadiusX")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the X vector radius of the ellipse.")]
	public class API_Ellipse_RadiusX : dynAPIPropertyNode
	{
		public API_Ellipse_RadiusX()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the X vector radius of the ellipse.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_YDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The Y direction.")]
	public class API_Ellipse_YDirection : dynAPIPropertyNode
	{
		public API_Ellipse_YDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The Y direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_XDirection")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The X direction.")]
	public class API_Ellipse_XDirection : dynAPIPropertyNode
	{
		public API_Ellipse_XDirection()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","The X direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_Normal")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal to the plane in which the ellipse is defined.")]
	public class API_Ellipse_Normal : dynAPIPropertyNode
	{
		public API_Ellipse_Normal()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the ellipse is defined.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Ellipse_Center")]
	[NodeSearchTags("curve","ellipse")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the center of the ellipse.")]
	public class API_Ellipse_Center : dynAPIPropertyNode
	{
		public API_Ellipse_Center()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Ellipse",typeof(Autodesk.Revit.DB.Ellipse)));
			OutPortData.Add(new PortData("out","Returns the center of the ellipse.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Extrusion_EndOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the end of the extrusion relative to the sketch plane.")]
	public class API_Extrusion_EndOffset : dynAPIPropertyNode
	{
		public API_Extrusion_EndOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the end of the extrusion relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Extrusion_StartOffset")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the start of the extrusion relative to the sketch plane.")]
	public class API_Extrusion_StartOffset : dynAPIPropertyNode
	{
		public API_Extrusion_StartOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","The offset of the start of the extrusion relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Extrusion_Sketch")]
	[NodeSearchTags("generic","extrusion")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Sketch of the Extrusion.")]
	public class API_Extrusion_Sketch : dynAPIPropertyNode
	{
		public API_Extrusion_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Extrusion",typeof(Autodesk.Revit.DB.Extrusion)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Extrusion.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferencePointArray
	///</summary>
	[NodeName("API_Application_NewReferencePointArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store ReferencePoint objects.")]
	public class API_Application_NewReferencePointArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointRelativeToPoint
	///</summary>
	[NodeName("API_Application_NewPointRelativeToPoint")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointRelativeToPoint object, which is used to define the placement of a ReferencePoint relative to a host point.")]
	public class API_Application_NewPointRelativeToPoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdgeFaceIntersection
	///</summary>
	[NodeName("API_Application_NewPointOnEdgeFaceIntersection")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnEdgeFaceIntersection object which is used to define the placement of a ReferencePoint given a references to edge and a reference to face.")]
	public class API_Application_NewPointOnEdgeFaceIntersection : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdgeFaceIntersection
		///</summary>
		public API_Application_NewPointOnEdgeFaceIntersection()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewPointOnEdgeFaceIntersection", false, new Type[]{typeof(Autodesk.Revit.DB.Reference),typeof(Autodesk.Revit.DB.Reference),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("ref", "Autodesk.Revit.DB.Reference",typeof(Autodesk.Revit.DB.Reference)));
			InPortData.Add(new PortData("b", "System.Boolean",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Construct a PointOnEdgeFaceIntersection object which is used to define the placement of a ReferencePoint given a references to edge and a reference to face.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdgeEdgeIntersection
	///</summary>
	[NodeName("API_Application_NewPointOnEdgeEdgeIntersection")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnEdgeEdgeIntersection object which is used to define the placement of a ReferencePoint given two references to edge.")]
	public class API_Application_NewPointOnEdgeEdgeIntersection : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnFace
	///</summary>
	[NodeName("API_Application_NewPointOnFace")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnFace object which is used to define the placement of a ReferencePoint given a reference and a location on the face.")]
	public class API_Application_NewPointOnFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnPlane
	///</summary>
	[NodeName("API_Application_NewPointOnPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Construct a PointOnPlane object which is used to define the placement of a ReferencePoint from its property values.")]
	public class API_Application_NewPointOnPlane : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPointOnEdge
	///</summary>
	[NodeName("API_Application_NewPointOnEdge")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a PointOnEdge object which is used to define the placement of a ReferencePoint.")]
	public class API_Application_NewPointOnEdge : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilySymbolProfile
	///</summary>
	[NodeName("API_Application_NewFamilySymbolProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new FamilySymbolProfile object.")]
	public class API_Application_NewFamilySymbolProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveLoopsProfile
	///</summary>
	[NodeName("API_Application_NewCurveLoopsProfile")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new CurveLoopsProfile object.")]
	public class API_Application_NewCurveLoopsProfile : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementId
	///</summary>
	[NodeName("API_Application_NewElementId")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new Autodesk::Revit::DB::ElementId^ object.")]
	public class API_Application_NewElementId : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewAreaCreationData
	///</summary>
	[NodeName("API_Application_NewAreaCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of Area for batch creation.")]
	public class API_Application_NewAreaCreationData : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTextNoteCreationData
	///</summary>
	[NodeName("API_Application_NewTextNoteCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewTextNote() for batch creation.")]
	public class API_Application_NewTextNoteCreationData : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTextNoteCreationData
		///</summary>
		public API_Application_NewTextNoteCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTextNoteCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(Autodesk.Revit.DB.TextAlignFlags),typeof(Autodesk.Revit.DB.TextNoteLeaderTypes),typeof(Autodesk.Revit.DB.TextNoteLeaderStyles),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(System.Double)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(Autodesk.Revit.DB.TextAlignFlags)));
			InPortData.Add(new PortData("tnlts", "The type and alignment of the leader for the note.",typeof(Autodesk.Revit.DB.TextNoteLeaderTypes)));
			InPortData.Add(new PortData("tnls", "The style for the leader.",typeof(Autodesk.Revit.DB.TextNoteLeaderStyles)));
			InPortData.Add(new PortData("xyz", "The end point for the leader.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The elbow point for the leader.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewTextNote() for batch creation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTextNoteCreationData
	///</summary>
	[NodeName("API_Application_NewTextNoteCreationData_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewTextNote()  for batch creation.")]
	public class API_Application_NewTextNoteCreationData_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTextNoteCreationData
		///</summary>
		public API_Application_NewTextNoteCreationData_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewTextNoteCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.View),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(Autodesk.Revit.DB.XYZ),typeof(System.Double),typeof(Autodesk.Revit.DB.TextAlignFlags),typeof(System.String)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("v", "The view where the text note object will be visible.",typeof(Autodesk.Revit.DB.View)));
			InPortData.Add(new PortData("xyz", "The origin of the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The horizontal direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("xyz", "The vertical direction for text in the text note.",typeof(Autodesk.Revit.DB.XYZ)));
			InPortData.Add(new PortData("n", "The width of the rectangle bounding the note text.",typeof(System.Double)));
			InPortData.Add(new PortData("tafs", "Flags indicating the alignment of the note.  This should be a bitwise OR including one of TEF_ALIGN_TOP, TEF_ALIGN_MIDDLE and TEF_ALIGN_BOTTOM and one of TEF_ALIGN_LEFT, TEF_ALIGN_CENTER and TEF_ALIGN_RIGHT.The defaults for this flag are TEF_ALIGN_TOP | TEF_ALIGN_LEFT.",typeof(Autodesk.Revit.DB.TextAlignFlags)));
			InPortData.Add(new PortData("s", "Text to display in the text note.  Include new line characters to force a multiple line note to be created.  Notes may also wrap automatically based on the width of the note rectangle.",typeof(System.String)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewTextNote()  for batch creation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
	///</summary>
	[NodeName("API_Application_NewProfiledWallCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation")]
	public class API_Application_NewProfiledWallCreationData : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
		///</summary>
		public API_Application_NewProfiledWallCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewProfiledWallCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean),typeof(Autodesk.Revit.DB.XYZ)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			InPortData.Add(new PortData("xyz", "A vector that must be perpendicular to the profile which dictates which side of the wall is considered.",typeof(Autodesk.Revit.DB.XYZ)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
	///</summary>
	[NodeName("API_Application_NewProfiledWallCreationData_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation")]
	public class API_Application_NewProfiledWallCreationData_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
		///</summary>
		public API_Application_NewProfiledWallCreationData_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewProfiledWallCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
	///</summary>
	[NodeName("API_Application_NewProfiledWallCreationData_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation")]
	public class API_Application_NewProfiledWallCreationData_2 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProfiledWallCreationData
		///</summary>
		public API_Application_NewProfiledWallCreationData_2()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewProfiledWallCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.CurveArray),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crvs", "An array of planar lines and arcs that represent the vertical profile of the wall.",typeof(Autodesk.Revit.DB.CurveArray)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRectangularWallCreationData
	///</summary>
	[NodeName("API_Application_NewRectangularWallCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation")]
	public class API_Application_NewRectangularWallCreationData : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRectangularWallCreationData
		///</summary>
		public API_Application_NewRectangularWallCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRectangularWallCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.Level),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRectangularWallCreationData
	///</summary>
	[NodeName("API_Application_NewRectangularWallCreationData_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewWall() for batch creation")]
	public class API_Application_NewRectangularWallCreationData_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRectangularWallCreationData
		///</summary>
		public API_Application_NewRectangularWallCreationData_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRectangularWallCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Curve),typeof(Autodesk.Revit.DB.WallType),typeof(Autodesk.Revit.DB.Level),typeof(System.Double),typeof(System.Double),typeof(System.Boolean),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("crv", "An arc or line representing the base line of the wall.",typeof(Autodesk.Revit.DB.Curve)));
			InPortData.Add(new PortData("wt", "A wall type to be used by the new wall instead of the default type.",typeof(Autodesk.Revit.DB.WallType)));
			InPortData.Add(new PortData("l", "The level on which the wall is to be placed.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("n", "The height of the wall.",typeof(System.Double)));
			InPortData.Add(new PortData("n", "An offset distance, in feet from the specified baseline. The wall will be placed that distancefrom the baseline.",typeof(System.Double)));
			InPortData.Add(new PortData("b", "Change which side of the wall is considered to be the inside and outside of the wall.",typeof(System.Boolean)));
			InPortData.Add(new PortData("b", "If set, specifies that the wall is structural in nature.",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewWall() for batch creation",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRoomCreationData
	///</summary>
	[NodeName("API_Application_NewRoomCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewRoom() for batch creation.")]
	public class API_Application_NewRoomCreationData : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewRoomCreationData
		///</summary>
		public API_Application_NewRoomCreationData()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewRoomCreationData", false, new Type[]{typeof(Autodesk.Revit.DB.Level),typeof(Autodesk.Revit.DB.UV)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("l", "- The level on which the room is to exist.",typeof(Autodesk.Revit.DB.Level)));
			InPortData.Add(new PortData("uv", "A 2D point the dictates the location on that specified level.",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Creates an object which wraps the arguments of NewRoom() for batch creation.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_3")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_4")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_4 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_5")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_5 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_6")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_6 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFamilyInstanceCreationData
	///</summary>
	[NodeName("API_Application_NewFamilyInstanceCreationData_7")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object which wraps the arguments of NewFamilyInstance() for batch creation.")]
	public class API_Application_NewFamilyInstanceCreationData_7 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSpaceSet
	///</summary>
	[NodeName("API_Application_NewSpaceSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a space set.")]
	public class API_Application_NewSpaceSet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCombinationArray
	///</summary>
	[NodeName("API_Application_NewLoadCombinationArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCombination array.")]
	public class API_Application_NewLoadCombinationArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadUsageArray
	///</summary>
	[NodeName("API_Application_NewLoadUsageArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadUsage array.")]
	public class API_Application_NewLoadUsageArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLoadCaseArray
	///</summary>
	[NodeName("API_Application_NewLoadCaseArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a LoadCase array.")]
	public class API_Application_NewLoadCaseArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewViewSet
	///</summary>
	[NodeName("API_Application_NewViewSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a View set.")]
	public class API_Application_NewViewSet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewIntersectionResultArray
	///</summary>
	[NodeName("API_Application_NewIntersectionResultArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of an IntersectionResult array.")]
	public class API_Application_NewIntersectionResultArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFaceArray
	///</summary>
	[NodeName("API_Application_NewFaceArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a face array.")]
	public class API_Application_NewFaceArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewReferenceArray
	///</summary>
	[NodeName("API_Application_NewReferenceArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a reference array.")]
	public class API_Application_NewReferenceArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDoubleArray
	///</summary>
	[NodeName("API_Application_NewDoubleArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a double array.")]
	public class API_Application_NewDoubleArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVolumeCalculationOptions
	///</summary>
	[NodeName("API_Application_NewVolumeCalculationOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates options related to room volume and area computations.")]
	public class API_Application_NewVolumeCalculationOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLImportOptions
	///</summary>
	[NodeName("API_Application_NewGBXMLImportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Import options.")]
	public class API_Application_NewGBXMLImportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewImageImportOptions
	///</summary>
	[NodeName("API_Application_NewImageImportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Image Import options.")]
	public class API_Application_NewImageImportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBuildingSiteExportOptions
	///</summary>
	[NodeName("API_Application_NewBuildingSiteExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Building Site Export options.")]
	public class API_Application_NewBuildingSiteExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewFBXExportOptions
	///</summary>
	[NodeName("API_Application_NewFBXExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates 3D-Studio Max (FBX) Export options.")]
	public class API_Application_NewFBXExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGBXMLExportOptions
	///</summary>
	[NodeName("API_Application_NewGBXMLExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates Green-Building XML Export options.")]
	public class API_Application_NewGBXMLExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFXExportOptions
	///</summary>
	[NodeName("API_Application_NewDWFXExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWFX Export options.")]
	public class API_Application_NewDWFXExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewDWFExportOptions
	///</summary>
	[NodeName("API_Application_NewDWFExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates DWF Export options.")]
	public class API_Application_NewDWFExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewSATExportOptions
	///</summary>
	[NodeName("API_Application_NewSATExportOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates SAT Export options.")]
	public class API_Application_NewSATExportOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object by copying the supplied UV object.")]
	public class API_Application_NewUV : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object representing coordinates in 2-space with supplied values.")]
	public class API_Application_NewUV_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewUV
	///</summary>
	[NodeName("API_Application_NewUV_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV object at the origin.")]
	public class API_Application_NewUV_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object by copying the supplied XYZ object.")]
	public class API_Application_NewXYZ : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object representing coordinates in 3-space with supplied values.")]
	public class API_Application_NewXYZ_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewXYZ
	///</summary>
	[NodeName("API_Application_NewXYZ_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a XYZ object at the origin.")]
	public class API_Application_NewXYZ_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
	///</summary>
	[NodeName("API_Application_NewBoundingBoxUV")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a two-dimensional rectangle with supplied values.")]
	public class API_Application_NewBoundingBoxUV : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxUV
	///</summary>
	[NodeName("API_Application_NewBoundingBoxUV_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty two-dimensional rectangle.")]
	public class API_Application_NewBoundingBoxUV_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewBoundingBoxXYZ
	///</summary>
	[NodeName("API_Application_NewBoundingBoxXYZ")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a three-dimensional rectangular box.")]
	public class API_Application_NewBoundingBoxXYZ : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
	///</summary>
	[NodeName("API_Application_NewHermiteSpline")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with specified tangency at its endpoints.")]
	public class API_Application_NewHermiteSpline : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewHermiteSpline
	///</summary>
	[NodeName("API_Application_NewHermiteSpline_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a Hermite spline with default tangency at its endpoints.")]
	public class API_Application_NewHermiteSpline_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
	///</summary>
	[NodeName("API_Application_NewNurbSpline")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric NurbSpline object using the same calculations that Revit uses when sketching splines in the user interface.")]
	public class API_Application_NewNurbSpline : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewNurbSpline
	///</summary>
	[NodeName("API_Application_NewNurbSpline_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric nurbSpline object.")]
	public class API_Application_NewNurbSpline_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewEllipse
	///</summary>
	[NodeName("API_Application_NewEllipse")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric ellipse object.")]
	public class API_Application_NewEllipse : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewProjectPosition
	///</summary>
	[NodeName("API_Application_NewProjectPosition")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new project position object.")]
	public class API_Application_NewProjectPosition : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
	///</summary>
	[NodeName("API_Application_NewArc")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on three points.")]
	public class API_Application_NewArc : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
	///</summary>
	[NodeName("API_Application_NewArc_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on plane, radius, and angles.")]
	public class API_Application_NewArc_1 : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
		///</summary>
		public API_Application_NewArc_1()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewArc", false, new Type[]{typeof(Autodesk.Revit.DB.Plane),typeof(System.Double),typeof(System.Double),typeof(System.Double)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			InPortData.Add(new PortData("p", "Autodesk.Revit.DB.Plane",typeof(Autodesk.Revit.DB.Plane)));
			InPortData.Add(new PortData("n", "System.Double",typeof(System.Double)));
			InPortData.Add(new PortData("n", "System.Double",typeof(System.Double)));
			InPortData.Add(new PortData("n", "System.Double",typeof(System.Double)));
			OutPortData.Add(new PortData("out","Creates a new geometric arc object based on plane, radius, and angles.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewArc
	///</summary>
	[NodeName("API_Application_NewArc_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric arc object based on center, radius, unit vectors, and angles.")]
	public class API_Application_NewArc_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPoint
	///</summary>
	[NodeName("API_Application_NewPoint")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric point object.")]
	public class API_Application_NewPoint : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane from a loop of planar curves.")]
	public class API_Application_NewPlane : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on a normal vector and an origin.")]
	public class API_Application_NewPlane_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewPlane
	///</summary>
	[NodeName("API_Application_NewPlane_2")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new geometric plane object based on two coordinate vectors and an origin.")]
	public class API_Application_NewPlane_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewColor
	///</summary>
	[NodeName("API_Application_NewColor")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new color object.")]
	public class API_Application_NewColor : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCombinableElementArray
	///</summary>
	[NodeName("API_Application_NewCombinableElementArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold combinable element objects.")]
	public class API_Application_NewCombinableElementArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPairArray
	///</summary>
	[NodeName("API_Application_NewVertexIndexPairArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold VertexIndexPair objects.")]
	public class API_Application_NewVertexIndexPairArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewVertexIndexPair
	///</summary>
	[NodeName("API_Application_NewVertexIndexPair")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new VertexIndexPair object.")]
	public class API_Application_NewVertexIndexPair : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementArray
	///</summary>
	[NodeName("API_Application_NewElementArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns an array that can hold element objects.")]
	public class API_Application_NewElementArray : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementArray
		///</summary>
		public API_Application_NewElementArray()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewElementArray", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Returns an array that can hold element objects.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArrArray
	///</summary>
	[NodeName("API_Application_NewCurveArrArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curve loops.")]
	public class API_Application_NewCurveArrArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCurveArray
	///</summary>
	[NodeName("API_Application_NewCurveArray")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an empty array that can store geometric curves.")]
	public class API_Application_NewCurveArray : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewStringStringMap
	///</summary>
	[NodeName("API_Application_NewStringStringMap")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new map that maps one string to another string.")]
	public class API_Application_NewStringStringMap : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewStringStringMap
		///</summary>
		public API_Application_NewStringStringMap()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewStringStringMap", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Creates a new map that maps one string to another string.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewGeometryOptions
	///</summary>
	[NodeName("API_Application_NewGeometryOptions")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates an object to specify user preferences in parsing of geometry.")]
	public class API_Application_NewGeometryOptions : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineUnbound
	///</summary>
	[NodeName("API_Application_NewLineUnbound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new unbounded geometric line object.")]
	public class API_Application_NewLineUnbound : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLineBound
	///</summary>
	[NodeName("API_Application_NewLineBound")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bounded geometric line object.")]
	public class API_Application_NewLineBound : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewLine
	///</summary>
	[NodeName("API_Application_NewLine")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new bound or unbounded geometric line object.")]
	public class API_Application_NewLine : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewMaterialSet
	///</summary>
	[NodeName("API_Application_NewMaterialSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Create a new instance of MaterialSet.")]
	public class API_Application_NewMaterialSet : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.Creation.Application.NewMaterialSet
		///</summary>
		public API_Application_NewMaterialSet()
		{
			base_type = typeof(Autodesk.Revit.Creation.Application);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "NewMaterialSet", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.Creation.Application", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Create a new instance of MaterialSet.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewElementSet
	///</summary>
	[NodeName("API_Application_NewElementSet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding elements.")]
	public class API_Application_NewElementSet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
	///</summary>
	[NodeName("API_Application_NewTypeBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new type binding object containing the categories passed as a parameter.")]
	public class API_Application_NewTypeBinding : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewTypeBinding
	///</summary>
	[NodeName("API_Application_NewTypeBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty type binding object.")]
	public class API_Application_NewTypeBinding_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
	///</summary>
	[NodeName("API_Application_NewInstanceBinding")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance binding object containing the categories passed as a parameter.")]
	public class API_Application_NewInstanceBinding : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewInstanceBinding
	///</summary>
	[NodeName("API_Application_NewInstanceBinding_1")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new empty instance binding object.")]
	public class API_Application_NewInstanceBinding_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.Creation.Application.NewCategorySet
	///</summary>
	[NodeName("API_Application_NewCategorySet")]
	[NodeSearchTags("application","create")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new instance of a set specifically for holding category objects.")]
	public class API_Application_NewCategorySet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Revolution_Axis")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Axis of the Revolution.")]
	public class API_Revolution_Axis : dynAPIPropertyNode
	{
		public API_Revolution_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Axis of the Revolution.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Revolution_EndAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The end angle of the revolution relative to the sketch plane.")]
	public class API_Revolution_EndAngle : dynAPIPropertyNode
	{
		public API_Revolution_EndAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The end angle of the revolution relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Revolution_StartAngle")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The start angle of the revolution relative to the sketch plane.")]
	public class API_Revolution_StartAngle : dynAPIPropertyNode
	{
		public API_Revolution_StartAngle()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","The start angle of the revolution relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Revolution_Sketch")]
	[NodeSearchTags("generic","form","revolution")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Sketch of the Revolution.")]
	public class API_Revolution_Sketch : dynAPIPropertyNode
	{
		public API_Revolution_Sketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Revolution",typeof(Autodesk.Revit.DB.Revolution)));
			OutPortData.Add(new PortData("out","Returns the Sketch of the Revolution.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_RevolvedFace_Curve")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Profile curve of the surface.")]
	public class API_RevolvedFace_Curve : dynAPIPropertyNode
	{
		public API_RevolvedFace_Curve()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Profile curve of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_RevolvedFace_Axis")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Axis of the surface.")]
	public class API_RevolvedFace_Axis : dynAPIPropertyNode
	{
		public API_RevolvedFace_Axis()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Axis of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_RevolvedFace_Origin")]
	[NodeSearchTags("face","revolved","revolve")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Origin of the surface.")]
	public class API_RevolvedFace_Origin : dynAPIPropertyNode
	{
		public API_RevolvedFace_Origin()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.RevolvedFace",typeof(Autodesk.Revit.DB.RevolvedFace)));
			OutPortData.Add(new PortData("out","Origin of the surface.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.ComputeDerivatives
	///</summary>
	[NodeName("API_Edge_ComputeDerivatives")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the vectors describing the edge at the specified parameter.")]
	public class API_Edge_ComputeDerivatives : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurveFollowingFace
	///</summary>
	[NodeName("API_Edge_AsCurveFollowingFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a curve that corresponds to this edge as oriented in its topological direction on the specified face.")]
	public class API_Edge_AsCurveFollowingFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.AsCurve
	///</summary>
	[NodeName("API_Edge_AsCurve")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a curve that corresponds to the edge's parametric orientation.")]
	public class API_Edge_AsCurve : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.EvaluateOnFace
	///</summary>
	[NodeName("API_Edge_EvaluateOnFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the edge to produce UV coordinates on the face.")]
	public class API_Edge_EvaluateOnFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.Evaluate
	///</summary>
	[NodeName("API_Edge_Evaluate")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Evaluates a parameter on the edge.")]
	public class API_Edge_Evaluate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.TessellateOnFace
	///</summary>
	[NodeName("API_Edge_TessellateOnFace")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a polyline approximation to the edge in UV parameters of the face.")]
	public class API_Edge_TessellateOnFace : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Edge.Tessellate
	///</summary>
	[NodeName("API_Edge_Tessellate")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a polyline approximation to the edge.")]
	public class API_Edge_Tessellate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Edge_ApproximateLength")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the approximate length of the edge.")]
	public class API_Edge_ApproximateLength : dynAPIPropertyNode
	{
		public API_Edge_ApproximateLength()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns the approximate length of the edge.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Edge_Reference")]
	[NodeSearchTags("edge")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a stable reference to the edge.")]
	public class API_Edge_Reference : dynAPIPropertyNode
	{
		public API_Edge_Reference()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Edge",typeof(Autodesk.Revit.DB.Edge)));
			OutPortData.Add(new PortData("out","Returns a stable reference to the edge.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.ToString
	///</summary>
	[NodeName("API_UV_ToString")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets formatted string showing (U, V) with values formatted to 9 decimal places.")]
	public class API_UV_ToString : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.AngleTo
	///</summary>
	[NodeName("API_UV_AngleTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the angle between this vector and the specified vector.")]
	public class API_UV_AngleTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.DistanceTo
	///</summary>
	[NodeName("API_UV_DistanceTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the distance from this 2-D point to the specified 2-D point.")]
	public class API_UV_DistanceTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
	///</summary>
	[NodeName("API_UV_IsAlmostEqualTo")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this 2-D vector and the specified 2-D vector are the same within a specified tolerance.")]
	public class API_UV_IsAlmostEqualTo : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsAlmostEqualTo
	///</summary>
	[NodeName("API_UV_IsAlmostEqualTo_1")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether this 2-D vector and the specified 2-D vector are the same within the tolerance (1.0e-09).")]
	public class API_UV_IsAlmostEqualTo_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Divide
	///</summary>
	[NodeName("API_UV_Divide")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Divides this 2-D vector by the specified value and returns the result.")]
	public class API_UV_Divide : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Multiply
	///</summary>
	[NodeName("API_UV_Multiply")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Multiplies this 2-D vector by the specified value and returns the result.")]
	public class API_UV_Multiply : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Negate
	///</summary>
	[NodeName("API_UV_Negate")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Negates this 2-D vector.")]
	public class API_UV_Negate : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Subtract
	///</summary>
	[NodeName("API_UV_Subtract")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Subtracts the specified 2-D vector from this 2-D vector and returns the result.")]
	public class API_UV_Subtract : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Add
	///</summary>
	[NodeName("API_UV_Add")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Adds the specified 2-D vector to this 2-D vector and returns the result.")]
	public class API_UV_Add : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.CrossProduct
	///</summary>
	[NodeName("API_UV_CrossProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cross product of this 2-D vector and the specified 2-D vector.")]
	public class API_UV_CrossProduct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.DotProduct
	///</summary>
	[NodeName("API_UV_DotProduct")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The dot product of this 2-D vector and the specified 2-D vector.")]
	public class API_UV_DotProduct : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsUnitLength
	///</summary>
	[NodeName("API_UV_IsUnitLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value indicates whether this 2-D vector is of unit length.")]
	public class API_UV_IsUnitLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.IsZeroLength
	///</summary>
	[NodeName("API_UV_IsZeroLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The boolean value indicates whether this 2-D vector is a zero vector.")]
	public class API_UV_IsZeroLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.GetLength
	///</summary>
	[NodeName("API_UV_GetLength")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The length of this 2-D vector.")]
	public class API_UV_GetLength : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.Normalize
	///</summary>
	[NodeName("API_UV_Normalize")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a new UV whose coordinates are the normalized values from this vector.")]
	public class API_UV_Normalize : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
	///</summary>
	[NodeName("API_UV")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a UV with the supplied coordinates.")]
	public class API_UV : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.UV.#ctor
	///</summary>
	[NodeName("API_UV_1")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a default UV with the values (0, 0).")]
	public class API_UV_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_UV_V")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the second coordinate.")]
	public class API_UV_V : dynAPIPropertyNode
	{
		public API_UV_V()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the second coordinate.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_UV_U")]
	[NodeSearchTags("uv","point","param")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the first coordinate.")]
	public class API_UV_U : dynAPIPropertyNode
	{
		public API_UV_U()
		{
			InPortData.Add(new PortData("uv", "Autodesk.Revit.DB.UV",typeof(Autodesk.Revit.DB.UV)));
			OutPortData.Add(new PortData("out","Gets the first coordinate.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Line_Direction")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the direction of the line.")]
	public class API_Line_Direction : dynAPIPropertyNode
	{
		public API_Line_Direction()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the direction of the line.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Line_Origin")]
	[NodeSearchTags("curve","line")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the origin of the line.")]
	public class API_Line_Origin : dynAPIPropertyNode
	{
		public API_Line_Origin()
		{
			InPortData.Add(new PortData("crv", "Autodesk.Revit.DB.Line",typeof(Autodesk.Revit.DB.Line)));
			OutPortData.Add(new PortData("out","Returns the origin of the line.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.ClearMaterialAspect
	///</summary>
	[NodeName("API_Material_ClearMaterialAspect")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Removes an aspect from the material.")]
	public class API_Material_ClearMaterialAspect : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspectToIndependent
	///</summary>
	[NodeName("API_Material_SetMaterialAspectToIndependent")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Set an aspect of the material from a shared property set to a custom property set owned by the material.")]
	public class API_Material_SetMaterialAspectToIndependent : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspectToIndependent
		///</summary>
		public API_Material_SetMaterialAspectToIndependent()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetMaterialAspectToIndependent", false, new Type[]{typeof(Autodesk.Revit.DB.MaterialAspect)}, out return_type);
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
			OutPortData.Add(new PortData("out","Set an aspect of the material from a shared property set to a custom property set owned by the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspectByPropertySet
	///</summary>
	[NodeName("API_Material_SetMaterialAspectByPropertySet")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Set an aspect of the material to a shared property set.")]
	public class API_Material_SetMaterialAspectByPropertySet : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspect
	///</summary>
	[NodeName("API_Material_SetMaterialAspect")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Set the value of an aspect of the material")]
	public class API_Material_SetMaterialAspect : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.SetMaterialAspect
		///</summary>
		public API_Material_SetMaterialAspect()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetMaterialAspect", false, new Type[]{typeof(Autodesk.Revit.DB.MaterialAspect),typeof(Autodesk.Revit.DB.ElementId),typeof(System.Boolean)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The material aspect",typeof(Autodesk.Revit.DB.MaterialAspect)));
			InPortData.Add(new PortData("val", "Identifier of a property set (an instance of PropertySetElement)",typeof(Autodesk.Revit.DB.ElementId)));
			InPortData.Add(new PortData("b", "Set the property set as shared or custom",typeof(System.Boolean)));
			OutPortData.Add(new PortData("out","Set the value of an aspect of the material",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetMaterialAspectPropertySet
	///</summary>
	[NodeName("API_Material_GetMaterialAspectPropertySet")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the identifier of the shared property set for a given aspect.")]
	public class API_Material_GetMaterialAspectPropertySet : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.GetMaterialAspectPropertySet
		///</summary>
		public API_Material_GetMaterialAspectPropertySet()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetMaterialAspectPropertySet", false, new Type[]{typeof(Autodesk.Revit.DB.MaterialAspect)}, out return_type);
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
			OutPortData.Add(new PortData("out","Gets the identifier of the shared property set for a given aspect.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternColor
	///</summary>
	[NodeName("API_Material_GetCutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the color of the cut pattern for the material element.")]
	public class API_Material_GetCutPatternColor : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetCutPatternId
	///</summary>
	[NodeName("API_Material_GetCutPatternId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the cut pattern id of the material.")]
	public class API_Material_GetCutPatternId : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.SetSmoothness
	///</summary>
	[NodeName("API_Material_SetSmoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the smoothness value.")]
	public class API_Material_SetSmoothness : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.SetSmoothness
		///</summary>
		public API_Material_SetSmoothness()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetSmoothness", false, new Type[]{typeof(System.Single)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			InPortData.Add(new PortData("val", "The smoothness value, ranges from 0 to 1.",typeof(System.Single)));
			OutPortData.Add(new PortData("out","Sets the smoothness value.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.GetSmoothness
	///</summary>
	[NodeName("API_Material_GetSmoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the smoothness value.")]
	public class API_Material_GetSmoothness : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Material.GetSmoothness
		///</summary>
		public API_Material_GetSmoothness()
		{
			base_type = typeof(Autodesk.Revit.DB.Material);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "GetSmoothness", false, new Type[]{}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material", typeof(object)));
			}
			OutPortData.Add(new PortData("out","Gets the smoothness value.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.Create
	///</summary>
	[NodeName("API_Material_Create")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new material.")]
	public class API_Material_Create : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Material.Duplicate
	///</summary>
	[NodeName("API_Material_Duplicate")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Duplicates the material.")]
	public class API_Material_Duplicate : dynAPIMethodNode
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
			OutPortData.Add(new PortData("out","Duplicates the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_MaterialClass")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The name of the general material type, e.g. 'Wood.'")]
	public class API_Material_MaterialClass : dynAPIPropertyNode
	{
		public API_Material_MaterialClass()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The name of the general material type, e.g. 'Wood.'",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_ThermalAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The ElementId of the thermal PropertySetElement.")]
	public class API_Material_ThermalAssetId : dynAPIPropertyNode
	{
		public API_Material_ThermalAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the thermal PropertySetElement.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_StructuralAssetId")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The ElementId of the structural PropertySetElement.")]
	public class API_Material_StructuralAssetId : dynAPIPropertyNode
	{
		public API_Material_StructuralAssetId()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The ElementId of the structural PropertySetElement.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_Shininess")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The shininess of the material.")]
	public class API_Material_Shininess : dynAPIPropertyNode
	{
		public API_Material_Shininess()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The shininess of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_Glow")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Whether the material can glow.")]
	public class API_Material_Glow : dynAPIPropertyNode
	{
		public API_Material_Glow()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","Whether the material can glow.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_RenderAppearance")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The rendering appearance property of the material.")]
	public class API_Material_RenderAppearance : dynAPIPropertyNode
	{
		public API_Material_RenderAppearance()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The rendering appearance property of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_SurfacePatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The surface pattern color of the material.")]
	public class API_Material_SurfacePatternColor : dynAPIPropertyNode
	{
		public API_Material_SurfacePatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The surface pattern color of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_SurfacePattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The surface pattern of the material.")]
	public class API_Material_SurfacePattern : dynAPIPropertyNode
	{
		public API_Material_SurfacePattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The surface pattern of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_CutPatternColor")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cut pattern color of the material.")]
	public class API_Material_CutPatternColor : dynAPIPropertyNode
	{
		public API_Material_CutPatternColor()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The cut pattern color of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_CutPattern")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The cut pattern of the material.")]
	public class API_Material_CutPattern : dynAPIPropertyNode
	{
		public API_Material_CutPattern()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The cut pattern of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_Smoothness")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The smoothness of the material.")]
	public class API_Material_Smoothness : dynAPIPropertyNode
	{
		public API_Material_Smoothness()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The smoothness of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_Transparency")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The transparency of the material.")]
	public class API_Material_Transparency : dynAPIPropertyNode
	{
		public API_Material_Transparency()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The transparency of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Material_Color")]
	[NodeSearchTags("material","color")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The color of the material.")]
	public class API_Material_Color : dynAPIPropertyNode
	{
		public API_Material_Color()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Material",typeof(Autodesk.Revit.DB.Material)));
			OutPortData.Add(new PortData("out","The color of the material.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Solid.ComputeCentroid
	///</summary>
	[NodeName("API_Solid_ComputeCentroid")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Centroid of this solid.")]
	public class API_Solid_ComputeCentroid : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Solid_Volume")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the signed volume of this solid.")]
	public class API_Solid_Volume : dynAPIPropertyNode
	{
		public API_Solid_Volume()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the signed volume of this solid.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Solid_SurfaceArea")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the total surface area of this solid.")]
	public class API_Solid_SurfaceArea : dynAPIPropertyNode
	{
		public API_Solid_SurfaceArea()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","Returns the total surface area of this solid.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Solid_Faces")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The faces that belong to the solid.")]
	public class API_Solid_Faces : dynAPIPropertyNode
	{
		public API_Solid_Faces()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The faces that belong to the solid.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Solid_Edges")]
	[NodeSearchTags("solid")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The edges that belong to the solid.")]
	public class API_Solid_Edges : dynAPIPropertyNode
	{
		public API_Solid_Edges()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Solid",typeof(Autodesk.Revit.DB.Solid)));
			OutPortData.Add(new PortData("out","The edges that belong to the solid.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_Radius")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the radius of the arc.")]
	public class API_Arc_Radius : dynAPIPropertyNode
	{
		public API_Arc_Radius()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the radius of the arc.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_YDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Y direction.")]
	public class API_Arc_YDirection : dynAPIPropertyNode
	{
		public API_Arc_YDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the Y direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_XDirection")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the X direction.")]
	public class API_Arc_XDirection : dynAPIPropertyNode
	{
		public API_Arc_XDirection()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the X direction.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_Normal")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the normal to the plane in which the arc is defined.")]
	public class API_Arc_Normal : dynAPIPropertyNode
	{
		public API_Arc_Normal()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the normal to the plane in which the arc is defined.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Arc_Center")]
	[NodeSearchTags("curve","arc")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the center of the arc.")]
	public class API_Arc_Center : dynAPIPropertyNode
	{
		public API_Arc_Center()
		{
			InPortData.Add(new PortData("arc", "Autodesk.Revit.DB.Arc",typeof(Autodesk.Revit.DB.Arc)));
			OutPortData.Add(new PortData("out","Returns the center of the arc.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Blend.SetVertexConnectionMap
	///</summary>
	[NodeName("API_Blend_SetVertexConnectionMap")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the mapping between the vertices in the top and bottom profiles.")]
	public class API_Blend_SetVertexConnectionMap : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.Blend.SetVertexConnectionMap
		///</summary>
		public API_Blend_SetVertexConnectionMap()
		{
			base_type = typeof(Autodesk.Revit.DB.Blend);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetVertexConnectionMap", false, new Type[]{typeof(Autodesk.Revit.DB.VertexIndexPairArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.VertexIndexPairArray",typeof(Autodesk.Revit.DB.VertexIndexPairArray)));
			OutPortData.Add(new PortData("out","Sets the mapping between the vertices in the top and bottom profiles.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Blend.GetVertexConnectionMap
	///</summary>
	[NodeName("API_Blend_GetVertexConnectionMap")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the mapping between the vertices in the top and bottom profiles.")]
	public class API_Blend_GetVertexConnectionMap : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_TopProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class API_Blend_TopProfile : dynAPIPropertyNode
	{
		public API_Blend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_BottomProfile")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class API_Blend_BottomProfile : dynAPIPropertyNode
	{
		public API_Blend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_TopOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the top end of the blend relative to the sketch plane.")]
	public class API_Blend_TopOffset : dynAPIPropertyNode
	{
		public API_Blend_TopOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the top end of the blend relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_BottomOffset")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The offset of the bottom end of the blend relative to the sketch plane.")]
	public class API_Blend_BottomOffset : dynAPIPropertyNode
	{
		public API_Blend_BottomOffset()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","The offset of the bottom end of the blend relative to the sketch plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_BottomSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Bottom Sketch of the Blend.")]
	public class API_Blend_BottomSketch : dynAPIPropertyNode
	{
		public API_Blend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Bottom Sketch of the Blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Blend_TopSketch")]
	[NodeSearchTags("generic","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the Top Sketch of the Blend.")]
	public class API_Blend_TopSketch : dynAPIPropertyNode
	{
		public API_Blend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Blend",typeof(Autodesk.Revit.DB.Blend)));
			OutPortData.Add(new PortData("out","Returns the Top Sketch of the Blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.SweptBlend.SetVertexConnectionMap
	///</summary>
	[NodeName("API_SweptBlend_SetVertexConnectionMap")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Sets the mapping between the vertices in the top and bottom profiles.")]
	public class API_SweptBlend_SetVertexConnectionMap : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.SweptBlend.SetVertexConnectionMap
		///</summary>
		public API_SweptBlend_SetVertexConnectionMap()
		{
			base_type = typeof(Autodesk.Revit.DB.SweptBlend);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetVertexConnectionMap", false, new Type[]{typeof(Autodesk.Revit.DB.VertexIndexPairArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend", typeof(object)));
			}
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.VertexIndexPairArray",typeof(Autodesk.Revit.DB.VertexIndexPairArray)));
			OutPortData.Add(new PortData("out","Sets the mapping between the vertices in the top and bottom profiles.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.SweptBlend.GetVertexConnectionMap
	///</summary>
	[NodeName("API_SweptBlend_GetVertexConnectionMap")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Gets the mapping between the vertices in the top and bottom profiles.")]
	public class API_SweptBlend_GetVertexConnectionMap : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_TopProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the top profile of the sketch.")]
	public class API_SweptBlend_TopProfile : dynAPIPropertyNode
	{
		public API_SweptBlend_TopProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the top profile of the sketch.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_BottomProfile")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The curves which make up the bottom profile of the sketch.")]
	public class API_SweptBlend_BottomProfile : dynAPIPropertyNode
	{
		public API_SweptBlend_BottomProfile()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The curves which make up the bottom profile of the sketch.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_SelectedPath")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The selected curve used for the swept blend path.")]
	public class API_SweptBlend_SelectedPath : dynAPIPropertyNode
	{
		public API_SweptBlend_SelectedPath()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The selected curve used for the swept blend path.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_PathSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The sketched path for the swept blend.")]
	public class API_SweptBlend_PathSketch : dynAPIPropertyNode
	{
		public API_SweptBlend_PathSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The sketched path for the swept blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_BottomProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The bottom family symbol profile of the swept blend.")]
	public class API_SweptBlend_BottomProfileSymbol : dynAPIPropertyNode
	{
		public API_SweptBlend_BottomProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom family symbol profile of the swept blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_BottomSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The bottom profile sketch of the swept blend.")]
	public class API_SweptBlend_BottomSketch : dynAPIPropertyNode
	{
		public API_SweptBlend_BottomSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The bottom profile sketch of the swept blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_TopProfileSymbol")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The top family symbol profile of the swept blend.")]
	public class API_SweptBlend_TopProfileSymbol : dynAPIPropertyNode
	{
		public API_SweptBlend_TopProfileSymbol()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top family symbol profile of the swept blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_SweptBlend_TopSketch")]
	[NodeSearchTags("generic","sweep","swept","blend")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The top profile sketch of the swept blend.")]
	public class API_SweptBlend_TopSketch : dynAPIPropertyNode
	{
		public API_SweptBlend_TopSketch()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.SweptBlend",typeof(Autodesk.Revit.DB.SweptBlend)));
			OutPortData.Add(new PortData("out","The top profile sketch of the swept blend.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Level_PlaneReference")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns a reference to this element as a plane.")]
	public class API_Level_PlaneReference : dynAPIPropertyNode
	{
		public API_Level_PlaneReference()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Returns a reference to this element as a plane.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Level_LevelType")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The level style of this level.")]
	public class API_Level_LevelType : dynAPIPropertyNode
	{
		public API_Level_LevelType()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","The level style of this level.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Level_ProjectElevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.")]
	public class API_Level_ProjectElevation : dynAPIPropertyNode
	{
		public API_Level_ProjectElevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieve the elevation relative to project origin, no matter what values of the Elevation Base parameter is set.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Level_Elevation")]
	[NodeSearchTags("level")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes the elevation above or below the ground level.")]
	public class API_Level_Elevation : dynAPIPropertyNode
	{
		public API_Level_Elevation()
		{
			InPortData.Add(new PortData("l", "Autodesk.Revit.DB.Level",typeof(Autodesk.Revit.DB.Level)));
			OutPortData.Add(new PortData("out","Retrieves or changes the elevation above or below the ground level.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.SetControlPointsAndWeights
	///</summary>
	[NodeName("API_NurbSpline_SetControlPointsAndWeights")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Set the control points, weights simultaneously.")]
	public class API_NurbSpline_SetControlPointsAndWeights : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.NurbSpline.SetControlPointsAndWeights
		///</summary>
		public API_NurbSpline_SetControlPointsAndWeights()
		{
			base_type = typeof(Autodesk.Revit.DB.NurbSpline);
			mi = dynRevitUtils.GetAPIMethodInfo(base_type, "SetControlPointsAndWeights", false, new Type[]{typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>),typeof(Autodesk.Revit.DB.DoubleArray)}, out return_type);
			pi = mi.GetParameters();
			if (!mi.IsStatic &&
				!mi.IsConstructor &&
				base_type != typeof(Autodesk.Revit.Creation.Document) &&
				base_type != typeof(Autodesk.Revit.Creation.FamilyItemFactory) &&
				base_type != typeof(Autodesk.Revit.Creation.ItemFactoryBase))
			{
				InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline", typeof(object)));
			}
			InPortData.Add(new PortData("lst", "System.Collections.Generic.IList{Autodesk.Revit.DB.XYZ}",typeof(System.Collections.Generic.IList<Autodesk.Revit.DB.XYZ>)));
			InPortData.Add(new PortData("arr", "Autodesk.Revit.DB.DoubleArray",typeof(Autodesk.Revit.DB.DoubleArray)));
			OutPortData.Add(new PortData("out","Set the control points, weights simultaneously.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_Knots")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Return/set the knots of the nurb spline.")]
	public class API_NurbSpline_Knots : dynAPIPropertyNode
	{
		public API_NurbSpline_Knots()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the knots of the nurb spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_Weights")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the weights of the nurb spline.")]
	public class API_NurbSpline_Weights : dynAPIPropertyNode
	{
		public API_NurbSpline_Weights()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the weights of the nurb spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_CtrlPoints")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the control points of the nurb spline.")]
	public class API_NurbSpline_CtrlPoints : dynAPIPropertyNode
	{
		public API_NurbSpline_CtrlPoints()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the control points of the nurb spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_Degree")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns the degree of the nurb spline.")]
	public class API_NurbSpline_Degree : dynAPIPropertyNode
	{
		public API_NurbSpline_Degree()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns the degree of the nurb spline.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_isRational")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Returns whether the nurb spline is rational or not.")]
	public class API_NurbSpline_isRational : dynAPIPropertyNode
	{
		public API_NurbSpline_isRational()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Returns whether the nurb spline is rational or not.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_NurbSpline_isClosed")]
	[NodeSearchTags("curve","nurbs")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Return/set the nurb spline's isClosed property.")]
	public class API_NurbSpline_isClosed : dynAPIPropertyNode
	{
		public API_NurbSpline_isClosed()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.NurbSpline",typeof(Autodesk.Revit.DB.NurbSpline)));
			OutPortData.Add(new PortData("out","Return/set the nurb spline's isClosed property.",typeof(object)));
			RegisterAllPorts();
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
	public class API_Wall_Create : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_1")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the specified wall type.")]
	public class API_Wall_Create_1 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_2")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a non rectangular profile wall within the project using the default wall type.")]
	public class API_Wall_Create_2 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_3")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the specified wall type, height, and offset.")]
	public class API_Wall_Create_3 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Create
	///</summary>
	[NodeName("API_Wall_Create_4")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a new rectangular profile wall within the project using the default wall style.")]
	public class API_Wall_Create_4 : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.Wall.Flip
	///</summary>
	[NodeName("API_Wall_Flip")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The wall orientation will be flipped.")]
	public class API_Wall_Flip : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_Orientation")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("The normal vector projected from the exterior side of the wall.")]
	public class API_Wall_Orientation : dynAPIPropertyNode
	{
		public API_Wall_Orientation()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","The normal vector projected from the exterior side of the wall.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_Flipped")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Property to test whether the wall orientation is flipped.")]
	public class API_Wall_Flipped : dynAPIPropertyNode
	{
		public API_Wall_Flipped()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Property to test whether the wall orientation is flipped.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_StructuralUsage")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes  the wall's designated structural usage.")]
	public class API_Wall_StructuralUsage : dynAPIPropertyNode
	{
		public API_Wall_StructuralUsage()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes  the wall's designated structural usage.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_Width")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the overall thickness of the wall.")]
	public class API_Wall_Width : dynAPIPropertyNode
	{
		public API_Wall_Width()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the overall thickness of the wall.",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_CurtainGrid")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Get the grid object of a curtain wall")]
	public class API_Wall_CurtainGrid : dynAPIPropertyNode
	{
		public API_Wall_CurtainGrid()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Get the grid object of a curtain wall",typeof(object)));
			RegisterAllPorts();
		}
	}

	[NodeName("API_Wall_WallType")]
	[NodeSearchTags("wall")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Retrieves or changes the type of the wall.")]
	public class API_Wall_WallType : dynAPIPropertyNode
	{
		public API_Wall_WallType()
		{
			InPortData.Add(new PortData("val", "Autodesk.Revit.DB.Wall",typeof(Autodesk.Revit.DB.Wall)));
			OutPortData.Add(new PortData("out","Retrieves or changes the type of the wall.",typeof(object)));
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElement
	///</summary>
	[NodeName("API_ElementTransformUtils_RotateElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotates an element about the given axis and angle.")]
	public class API_ElementTransformUtils_RotateElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.RotateElements
	///</summary>
	[NodeName("API_ElementTransformUtils_RotateElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Rotates a set of elements about the given axis and angle.")]
	public class API_ElementTransformUtils_RotateElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElement
	///</summary>
	[NodeName("API_ElementTransformUtils_MirrorElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a mirrored copy of an element about a given plane.")]
	public class API_ElementTransformUtils_MirrorElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MirrorElements
	///</summary>
	[NodeName("API_ElementTransformUtils_MirrorElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Creates a mirrored copy of a set of elements about a given plane.")]
	public class API_ElementTransformUtils_MirrorElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElement
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies an element and places the copy at a location indicated by a given transformation.")]
	public class API_ElementTransformUtils_CopyElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CopyElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Copies a set of elements and places the copies at a location indicated by a given translation.")]
	public class API_ElementTransformUtils_CopyElements : dynAPIMethodNode
	{
		///<summary>
		///Auto-generated constructor for Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CopyElements
		///</summary>
		public API_ElementTransformUtils_CopyElements()
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElement
	///</summary>
	[NodeName("API_ElementTransformUtils_MoveElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves one element by a given transformation.")]
	public class API_ElementTransformUtils_MoveElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.MoveElements
	///</summary>
	[NodeName("API_ElementTransformUtils_MoveElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Moves a set of elements by a given transformation.")]
	public class API_ElementTransformUtils_MoveElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElement
	///</summary>
	[NodeName("API_ElementTransformUtils_CanMirrorElement")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether element can be mirrored.")]
	public class API_ElementTransformUtils_CanMirrorElement : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

	///<summary>
	///Auto-generated Dynamo node wrapping Autodesk.Revit.DB.ElementTransformUtils.CanMirrorElements
	///</summary>
	[NodeName("API_ElementTransformUtils_CanMirrorElements")]
	[NodeSearchTags("element","transform","utils","move","rotate","scale","mirror","reflect")]
	[NodeCategory(BuiltinNodeCategories.REVIT_API)]
	[NodeDescription("Determines whether elements can be mirrored.")]
	public class API_ElementTransformUtils_CanMirrorElements : dynAPIMethodNode
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
			RegisterAllPorts();
		}
	}

}
