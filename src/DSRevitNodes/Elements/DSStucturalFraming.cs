using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.Revit.DB.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit FamilyInstance
    /// </summary>
    [RegisterForTrace]
    public class DSStructuralFraming : AbstractFamilyInstance
    {

        #region Internal properties

        internal Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalFamilyInstance; }
        }


        #endregion

        #region Private constructors

        /// <summary>
        /// Wrap an existing FamilyInstance. 
        /// </summary>
        /// <param name="instance"></param>
        private DSStructuralFraming(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        private FamilyInstanceCreationData GetCreationData(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.XYZ upVector, Autodesk.Revit.DB.Level level, StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {
            
            //calculate the desired rotation
            //we do this by finding the angle between the z axis
            //and vector between the start of the beam and the target point
            //both projected onto the start plane of the beam.
            XYZ zAxis = new XYZ(0, 0, 1);
            XYZ yAxis = new XYZ(0, 1, 0);

            //flatten the beam line onto the XZ plane
            //using the start's z coordinate
            XYZ start = curve.get_EndPoint(0);
            XYZ end = curve.get_EndPoint(1);
            XYZ newEnd = new XYZ(end.X, end.Y, start.Z); //drop end point to plane

            //catch the case where the end is directly above
            //the start, creating a normal with zero length
            //in that case, use the Z axis
            XYZ planeNormal = newEnd.IsAlmostEqualTo(start) ? zAxis : (newEnd - start).Normalize();

            double gamma = upVector.AngleOnPlaneTo(zAxis.IsAlmostEqualTo(planeNormal) ? yAxis : zAxis, planeNormal);

            return new FamilyInstanceCreationData(curve, symbol, level, structuralType)
            {
                RotateAngle = gamma
            };

        }

        /// <summary>
        /// Internal constructor - creates a single StructuralFraming instance
        /// </summary>
        internal DSStructuralFraming(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.XYZ upVector, Autodesk.Revit.DB.Level level, StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(symbol);
                InternalSetCurve(curve);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var creationData = GetCreationData(curve, upVector, level, structuralType, symbol);
            
            Autodesk.Revit.DB.FamilyInstance fi;
            if (Document.IsFamilyDocument)
            {
                var elementIds = Document.FamilyCreate.NewFamilyInstances2(new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance)Document.GetElement(elementIds.First());
            }
            else
            {
                var elementIds = Document.Create.NewFamilyInstances2(new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance) Document.GetElement(elementIds.First());
            }

            InternalSetFamilyInstance(fi);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private mutators

        private void InternalSetCurve(Autodesk.Revit.DB.Curve crv)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            //update the curve
            var locCurve = InternalFamilyInstance.Location as LocationCurve;
            locCurve.Curve = crv;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public DSFamilySymbol Symbol
        {
            get
            {
                return DSFamilySymbol.FromExisting(this.InternalFamilyInstance.Symbol, true);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Structural Framing Member by Curve and Level
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="upVector"></param>
        /// <param name="level"></param>
        /// <param name="structuralType"></param>
        /// <param name="structuralFamilySymbol"></param>
        /// <returns></returns>
        public static DSStructuralFraming ByCurveLevelUpVectorAndType(Autodesk.DesignScript.Geometry.Curve curve, DSLevel level, Autodesk.DesignScript.Geometry.Vector upVector, DSStructuralType structuralType, DSFamilySymbol structuralFamilySymbol)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (structuralFamilySymbol == null)
            {
                throw new ArgumentNullException("structuralFamilySymbol");
            }

            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            return new DSStructuralFraming(curve.ToRevitType(), upVector.ToXyz(), level.InternalLevel,
                structuralType.ToRevitType(), structuralFamilySymbol.InternalFamilySymbol);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct a FamilyInstance from the Revit document. 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSStructuralFraming FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            return new DSStructuralFraming(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }


}

/*
    [NodeName("Structural Framing")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Create structural framing.")]
    public class StructuralFraming : RevitTransactionNodeWithOneOutput
    {
        public StructuralFraming()
        {
            InPortData.Add(new PortData("type", "The framing type.", typeof(Value.Container)));
            InPortData.Add(new PortData("curves", "The curve(s) to be used as center lines for your framing elements.  Must be a list", typeof(Value.List)));
            InPortData.Add(new PortData("up", "The \"up\" vector for the beam.", typeof(Value)));
            OutPortData.Add(new PortData("framing", "The structural framing instance(s) created by this operation.", typeof(Value.List)));

            RegisterAllPorts();

            //we disable the lacing so that we can take advantage
            //of the family instance creation batching
            //TODO:figure out how to lace AND use batch creation
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var symbol = (FamilySymbol)((Value.Container)args[0]).Item;
            var curves = ((Value.List) args[1]).Item;

            IEnumerable<Tuple<Curve, XYZ>> data;
            if (args[2].IsList)
            {
                var targets = ((Value.List)args[2]).Item;

                if (curves.Count() != targets.Count())
                    throw new Exception("The number of curves and the number of up vectors must be the same.");

                //if we get a list of up vectors, then pair each
                //curve with a corresponding up vector
                data = curves.Zip(targets,
                    (first, second) =>
                        new Tuple<Curve, XYZ>((Curve) ((Value.Container) first).Item,
                            (XYZ) ((Value.Container) second).Item));
            }
            else
            {
                //if we get a single up vector, then pair each
                //curve with that up vector
                data = curves.Select(x=>new Tuple<Curve, XYZ>((Curve)((Value.Container)x).Item,
                            (XYZ)((Value.Container)args[2]).Item));
            }
            

            var instData = new List<FamilyInstanceCreationData>();

            int count = 0;

            foreach (var pair in data)
            {
                var curve = pair.Item1;
                var target = pair.Item2;

                //calculate the desired rotation
                //we do this by finding the angle between the z axis
                //and vector between the start of the beam and the target point
                //both projected onto the start plane of the beam.

                XYZ zAxis = new XYZ(0, 0, 1);
                XYZ yAxis = new XYZ(0, 1, 0);

                //flatten the beam line onto the XZ plane
                //using the start's z coordinate
                XYZ start = curve.get_EndPoint(0);
                XYZ end = curve.get_EndPoint(1);
                XYZ newEnd = new XYZ(end.X, end.Y, start.Z); //drop end point to plane

                ////use the x axis of the curve's transform 
                ////as the normal of the start plane
                //XYZ planeNormal = (curve.get_EndPoint(0) - curve.get_EndPoint(1)).Normalize();

                //catch the case where the end is directly above
                //the start, creating a normal with zero length
                //in that case, use the Z axis
                XYZ planeNormal = newEnd.IsAlmostEqualTo(start) ? zAxis : (newEnd - start).Normalize();

                XYZ target_project = target - target.DotProduct(planeNormal)*planeNormal;
                XYZ z_project = zAxis - zAxis.DotProduct(planeNormal)*planeNormal;

                //double gamma = target_project.AngleTo(z_project);
                double gamma = target.AngleOnPlaneTo(zAxis.IsAlmostEqualTo(planeNormal) ? yAxis : zAxis, planeNormal);

                FamilyInstance instance = null;
                if (this.Elements.Count > count)
                {
                    if (dynUtils.TryGetElement(this.Elements[count], out instance))
                    {
                        if (instance.Symbol != symbol)
                            instance.Symbol = symbol;

                        //update the curve
                        var locCurve = instance.Location as LocationCurve;
                        locCurve.Curve = curve;
                    }
                    else
                    {
                        var beamData = new FamilyInstanceCreationData(curve, symbol, dynRevitSettings.DefaultLevel, StructuralType.Beam)
                            {
                                RotateAngle = gamma
                            };
                        instData.Add(beamData);
                    }
                }
                else
                {
                    var beamData = new FamilyInstanceCreationData(curve, symbol, dynRevitSettings.DefaultLevel, StructuralType.Beam)
                        {
                            RotateAngle = gamma
                        };
                    instData.Add(beamData);
                }

                count++;
            }

            //trim the elements collection
            foreach (var e in this.Elements.Skip(count))
            {
                this.DeleteElement(e);
            }

            FSharpList<Value> results = FSharpList<Value>.Empty;

            if (instData.Any())
            {
                var ids = dynRevitSettings.Doc.Document.Create.NewFamilyInstances2(instData);

                //add our batch-created instances ids'
                //to the elements collection
                ids.ToList().ForEach(x=>Elements.Add(x));
            }

            //add all of the instances
            results = Elements.Aggregate(results, (current, id) => FSharpList<Value>.Cons(Value.NewContainer(dynRevitSettings.Doc.Document.GetElement(id)), current));
            results.Reverse();

            return Value.NewList(results);
        }
        
    }
*/