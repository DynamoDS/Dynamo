using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Units;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;
using Utils = Dynamo.FSchemeInterop.Utils;

namespace Dynamo.Nodes
{
    [NodeName("Get Family Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a Family Instance or Symbol, allows the user to select a parameter as a string.")]
    [NodeSearchTags("fam")]
    [IsInteractive(true)]
    public class FamilyInstanceParameterSelector : DropDrownBase
    {
        private ElementId storedId = null;
        private Element element;

        public FamilyInstanceParameterSelector()
        {
            InPortData.Add(new PortData("f", "Family Symbol or Instance", typeof (Value.Container)));
            OutPortData.Add(new PortData("", "Parameter Name", typeof (Value.String)));

            RegisterAllPorts();
        }

        private static string getStorageTypeString(StorageType st)
        {
            switch (st)
            {
                case StorageType.Integer:
                    return "int";
                case StorageType.Double:
                    return "dbl";
                case StorageType.String:
                    return "str";
                case StorageType.ElementId:
                default:
                    return "id";
            }
        }

        public override void PopulateItems() //(IEnumerable set, bool readOnly)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument.Document;

            this.Items.Clear();

            if (element is FamilySymbol)
            {
                var paramDict = new Dictionary<string, dynamic>();

                var fs = element as FamilySymbol;

                foreach (dynamic p in fs.Parameters)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                        new DynamoDropDownItem(
                            string.Format("{0}(Type)({1})", p.Definition.Name, getStorageTypeString(p.StorageType)), p));
                }

                //this was causing duplication of parameters
                //in the drop-down. 
                //var fd = doc.EditFamily(fs.Family);
                //var ps = fd.FamilyManager.Parameters;

                //foreach (dynamic p in ps)
                //{
                //    if (p.IsReadOnly || p.StorageType == StorageType.None)
                //        continue;
                //    Items.Add(
                //            new DynamoDropDownItem(p.Definition.Name + " (" + getStorageTypeString(p.StorageType) + ")", p));
                //}

            }
            else if (element is FamilyInstance)
            {
                var fi = element as FamilyInstance;

                foreach (dynamic p in fi.Parameters)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                        new DynamoDropDownItem(
                            string.Format("{0}({1})", p.Definition.Name, getStorageTypeString(p.StorageType)), p));
                }

                var fs = fi.Symbol;

                foreach (dynamic p in fs.Parameters)
                {
                    if (p.IsReadOnly || p.StorageType == StorageType.None)
                        continue;
                    Items.Add(
                        new DynamoDropDownItem(
                            string.Format("{0}(Type)({1})", p.Definition.Name, getStorageTypeString(p.StorageType)), p));
                }
            }
            else
            {
                storedId = null;
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            element = (Element) ((Value.Container) args[0]).Item;

            if (element == null)
            {
                throw new Exception("The input is not a family instance or symbol.");
            }

            //only update the collection on evaluate
            //if the item coming in is different
            if (element != null && !element.Id.Equals(this.storedId))
            {
                this.storedId = element.Id;
                PopulateItems();
            }

            if (SelectedIndex == -1)
                throw new Exception("Please select a parameter.");

            return Value.NewContainer(((Parameter) Items[SelectedIndex].Item).Definition);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (this.storedId != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("familyid");
                outEl.SetAttribute("value", this.storedId.IntegerValue.ToString(CultureInfo.InvariantCulture));
                nodeElement.AppendChild(outEl);

                XmlElement param = xmlDoc.CreateElement("index");
                param.SetAttribute("value", SelectedIndex.ToString(CultureInfo.InvariantCulture));
                nodeElement.AppendChild(param);
            }

        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument.Document;

            int index = -1;

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("familyid"))
                {
                    int id;
                    try
                    {
                        id = Convert.ToInt32(subNode.Attributes[0].Value);
                    }
                    catch
                    {
                        continue;
                    }
                    this.storedId = new ElementId(id);

                    element = doc.GetElement(this.storedId);

                }
                else if (subNode.Name.Equals("index"))
                {
                    try
                    {
                        index = Convert.ToInt32(subNode.Attributes[0].Value);
                    }
                    catch
                    {
                    }
                }
            }

            if (element != null)
            {
                PopulateItems();
                SelectedIndex = index;
            }
        }
    }
    
    [NodeName("Create Family Instance")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Creates family instances at a given XYZ location.")]
    public class FamilyInstanceCreatorXyz : RevitTransactionNodeWithOneOutput
    {
        public FamilyInstanceCreatorXyz()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof (Value.Container)));
            InPortData.Add(new PortData("type", "The Family Symbol to use for instantiation.", typeof (Value.Container)));
            OutPortData.Add(new PortData("fi", "Family instances created by this operation.", typeof (Value.Container)));

            RegisterAllPorts();
        }

        private Value makeFamilyInstance(object location, FamilySymbol fs, int count)
        {
            XYZ pos = location is ReferencePoint
                          ? (location as ReferencePoint).Position
                          : (XYZ) location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                if (dynUtils.TryGetElement(this.Elements[count], out fi))
                {
                    fi.Symbol = fs;
                    var lp = fi.Location as LocationPoint;
                    lp.Point = pos;
                }
                else
                {
                    fi = this.UIDocument.Document.IsFamilyDocument
                             ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                                 pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                                   )
                             : this.UIDocument.Document.Create.NewFamilyInstance(
                                 pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                                   );

                    this.Elements[count] = fi.Id;
                }
            }
            else
            {
                fi = this.UIDocument.Document.IsFamilyDocument
                         ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                             pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
                         : this.UIDocument.Document.Create.NewFamilyInstance(
                             pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                this.Elements.Add(fi.Id);
            }

            return Value.NewContainer(fi);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FamilySymbol fs = (FamilySymbol) ((Value.Container) args[1]).Item;
            var input = args[0];

            if (input.IsList)
            {
                var locList = (input as Value.List).Item;

                int count = 0;

                var result = Value.NewList(
                    Utils.SequenceToFSharpList(
                        locList.Select(
                            x =>
                            this.makeFamilyInstance(
                                ((Value.Container) x).Item,
                                fs,
                                count++
                                )
                            )
                        )
                    );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeFamilyInstance(
                    ((Value.Container) input).Item,
                    fs,
                    0
                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "DSRevitNodes.dll", 
                "FamilyInstance.ByPoint", "FamilyInstance.ByPoint@FamilySymbol,Point");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    [NodeName("Create Family Instance at Level")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Creates family instances in the given level.")]
    public class FamilyInstanceCreatorLevel : RevitTransactionNodeWithOneOutput
    {
        public FamilyInstanceCreatorLevel()
        {
            InPortData.Add(new PortData("xyz", "xyz", typeof (Value.Container)));
            InPortData.Add(new PortData("typ", "The Family Symbol to use for instantiation.", typeof (Value.Container)));
            InPortData.Add(new PortData("lev", "The Level to use for instantiation.", typeof (Value.Container)));

            OutPortData.Add(new PortData("fi", "Family instances created by this operation.", typeof (Value.Container)));

            RegisterAllPorts();
        }

        private Value makeFamilyInstance(object location, FamilySymbol fs, int count, Autodesk.Revit.DB.Level level)
        {
            XYZ pos = location is ReferencePoint
                          ? (location as ReferencePoint).Position
                          : (XYZ) location;

            FamilyInstance fi;

            if (this.Elements.Count > count)
            {
                if (dynUtils.TryGetElement(this.Elements[count], out fi))
                {
                    fi.Symbol = fs;
                    var lp = fi.Location as LocationPoint;
                    lp.Point = pos;
                }
                else
                {
                    fi = this.UIDocument.Document.IsFamilyDocument
                             ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                                 pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
                             : this.UIDocument.Document.Create.NewFamilyInstance(
                                 pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    this.Elements[count] = fi.Id;
                }
            }
            else
            {
                fi = this.UIDocument.Document.IsFamilyDocument
                         ? this.UIDocument.Document.FamilyCreate.NewFamilyInstance(
                             pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
                         : this.UIDocument.Document.Create.NewFamilyInstance(
                             pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                this.Elements.Add(fi.Id);
            }

            return Value.NewContainer(fi);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var fs = (FamilySymbol) ((Value.Container) args[1]).Item;
            var input = args[0];
            var level = (Autodesk.Revit.DB.Level) ((Value.Container) args[2]).Item;

            if (input.IsList)
            {
                var locList = (input as Value.List).Item;

                int count = 0;

                var result = Value.NewList(
                    Utils.SequenceToFSharpList(
                        locList.Select(
                            x =>
                            this.makeFamilyInstance(
                                ((Value.Container) x).Item,
                                fs,
                                count++,
                                level
                                )
                            )
                        )
                    );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else
            {
                var result = this.makeFamilyInstance(
                    ((Value.Container) input).Item,
                    fs,
                    0,
                    level
                    );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
             MigrationManager.SetFunctionSignature(dsRevitNode, "DSRevitNodes.dll", 
                 "FamilyInstance.ByPointAndLevel", 
                 "FamilyInstance.ByPointAndLevel@FamilySymbol,Point,Level");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);
            data.ReconnectToPort(connector2, newInPort2);

            return migratedData;
        }
    }

    [NodeName("Curves from Family Instance")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Extracts curves from family instances.")]
    public class CurvesFromFamilyInstance : RevitTransactionNodeWithOneOutput
    {
        public CurvesFromFamilyInstance()
        {
            InPortData.Add(new PortData("fi", "family instance", typeof (Value.Container)));

            OutPortData.Add(new PortData("curves", "Curves extracted by this operation.", typeof (Value.Container)));

            RegisterAllPorts();
        }

        private Value GetCurvesFromFamily(Autodesk.Revit.DB.FamilyInstance fi, int count,
                                          Autodesk.Revit.DB.Options options)
        {
            FamilySymbol fs = fi.Symbol;
            //Autodesk.Revit.DB.GeometryElement geomElem = fs.get_Geometry(options);
            Autodesk.Revit.DB.GeometryElement geomElem = fi.get_Geometry(options);
                // our particular case of a loaded mass family with no joins has no geom in the instance

            //fi.GetOriginalGeometry(options);
            //fi.GetTransform()

            Autodesk.Revit.DB.CurveArray curves = new CurveArray();
            Autodesk.Revit.DB.ReferenceArray curveRefs = new ReferenceArray();


            //Find all curves and insert them into curve array
            AddCurves(fi, geomElem, count, ref curves);

            //curves.Append(GetCurve(fi, options)); //test 

            //extract references for downstream use
            foreach (Curve c in curves)
            {
                curveRefs.Append(c.Reference);
            }

            //convert curvearray into list using Stephens MakeEnumerable
            Value result = Value.NewList(Utils.SequenceToFSharpList(
                dynUtils.MakeEnumerable(curves).Select(Value.NewContainer)
                                             ));


            return result;

        }

        /// <summary>
        /// Retrieve the first curve found for 
        /// the given element. In case the element is a 
        /// family instance, it may have its own non-empty
        /// solid, in which case we use that. Otherwise we 
        /// search the symbol geometry. If we use the 
        /// symbol geometry, we have to keep track of the 
        /// instance transform to map it to the actual
        /// instance project location.
        /// </summary>
        private Curve GetCurve(Element e, Options opt)
        {
            GeometryElement geo = e.get_Geometry(opt);

            Curve curve = null;
            GeometryInstance inst = null;
            Transform t = Transform.Identity;

            // Some columns have no solids, and we have to 
            // retrieve the geometry from the symbol; 
            // others do have solids on the instance itself 
            // and no contents in the instance geometry 
            // (e.g. in rst_basic_sample_project.rvt).

            foreach (GeometryObject obj in geo)
            {
                curve = obj as Curve;

                if (null != curve)
                {
                    break;
                }

                inst = obj as GeometryInstance;
            }

            if (null == curve && null != inst)
            {
                geo = inst.GetSymbolGeometry();
                t = inst.Transform;

                foreach (GeometryObject obj in geo)
                {
                    curve = obj as Curve;

                    if (null != curve)
                    {
                        break;
                    }
                }
            }
            return curve;
        }

        private Value AddCurves(FamilyInstance fi, GeometryElement geomElem, int count, ref CurveArray curves)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                Curve curve = geomObj as Curve;
                if (null != curve)
                {
                    curves.Append(curve);
                    continue;
                }

                //If this GeometryObject is Instance, call AddCurve
                GeometryInstance geomInst = geomObj as GeometryInstance;
                if (null != geomInst)
                {
                    GeometryElement transformedGeomElem // curves transformed into project coords
                        = geomInst.GetInstanceGeometry();
                    AddCurves(fi, transformedGeomElem, count, ref curves);
                }
            }
            return Value.NewContainer(curves);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //create some geometry options so that we compute references
            Autodesk.Revit.DB.Options opts = new Options();
            opts.ComputeReferences = true;
            opts.DetailLevel = ViewDetailLevel.Medium;
            opts.IncludeNonVisibleObjects = false;


            if (input.IsList)
            {
                var familyList = (input as Value.List).Item;
                int count = 0;

                var result = Value.NewList(
                    Utils.SequenceToFSharpList(
                        familyList.Select(
                            x =>
                            this.GetCurvesFromFamily(
                                (FamilyInstance) ((Value.Container) x).Item,
                                count++,
                                opts
                                )
                            )
                        )
                    );

                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                }

                return result;
            }
            else // single instance passed in
            {
                int count = 0;
                var result = this.GetCurvesFromFamily(
                    (FamilyInstance) ((Value.Container) input).Item,
                    count,
                    opts
                    );

                foreach (var e in this.Elements.Skip(1)) // cleanup in case of going from list to single instance.
                {
                    this.DeleteElement(e);
                }

                return result;
            }
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "FamilyInstance.Curves", "FamilyInstance.Curves");
        }
    }

    [NodeName("Set Element Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Modifies a parameter on an element.")]
    public class FamilyInstanceParameterSetter : RevitTransactionNodeWithOneOutput
    {
        public FamilyInstanceParameterSetter()
        {
            InPortData.Add(new PortData("element", "An Element.", typeof (Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to modify (string).", typeof (Value.String)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof (object)));
            OutPortData.Add(new PortData("element", "Modified Element.", typeof (Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        private static Value setParam(Element fi, string paramName, Value valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value setParam(Element fi, Definition paramDef, Value valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _setParam(Element ft, Parameter p, Value valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((Value.Number) valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int) ((Value.Number) valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((Value.String) valueExpr).Item);
            }
            else if (p.StorageType == StorageType.ElementId)
            {
                p.Set((ElementId) ((Value.Container) valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int) (valueExpr as Value.Number).Item));
            }
            else
            {
                p.Set((ElementId) ((Value.Container) valueExpr).Item);
            }
            return Value.NewContainer(ft);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var valueExpr = args[2];
            var fs = (Element)((Value.Container)args[0]).Item;

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String) param).Item;
                return setParam(fs, paramName, valueExpr);
            }
            var paramDef = (Definition) ((Value.Container) param).Item;
            return setParam(fs, paramDef, valueExpr);
        }
    }

    [NodeName("Get Element Parameter Value")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Fetches the value of a parameter of an element.")]
    public class FamilyInstanceParameterGetter : RevitTransactionNodeWithOneOutput
    {
        public FamilyInstanceParameterGetter()
        {
            InPortData.Add(new PortData("element", "An Element.", typeof (Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to fetch (string).", typeof (Value.String)));

            OutPortData.Add(new PortData("val", "Parameter value.", typeof (object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        private static Value getParam(Element fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static Value getParam(Element fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static Value _getParam(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.ElementId:
                    return Value.NewContainer(p.AsElementId());
                case StorageType.String:
                    return Value.NewString(p.AsString());
                case StorageType.Integer:
                case StorageType.Double:
                    switch (p.Definition.ParameterType)
                {
                    case ParameterType.Length:
                        return Value.NewContainer(Units.Length.FromFeet(p.AsDouble()));
                    case ParameterType.Area:
                        return Value.NewContainer(Units.Area.FromSquareFeet(p.AsDouble()));
                    case ParameterType.Volume:
                        return Value.NewContainer(Units.Volume.FromCubicFeet(p.AsDouble()));
                    default:
                        return Value.NewNumber(p.AsDouble());
                }
                default:
                    throw new Exception(string.Format("Parameter {0} has no storage type.", p));
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var fi = (Element)((Value.Container)args[0]).Item;

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((Value.String) param).Item;

                return getParam(fi, paramName);
            }
            else
            {
                var paramDef = (Definition) ((Value.Container) param).Item;

                return getParam(fi, paramDef);
            }
        }
    }

    [NodeName("Get Family Instance Location")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Returns all family instances of the selected type in the active model.")]
    public class GetFamilyInstanceLocation : NodeWithOneOutput
    {
        public GetFamilyInstanceLocation()
        {
            InPortData.Add(new PortData("instance", "The family instance for which to find a location.",
                                        typeof (Value.Container)));
            OutPortData.Add(new PortData("location",
                                         "The geometric location of the family instance defined as an XYZ or a curve.",
                                         typeof (Value.List)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var instance = (FamilyInstance) ((Value.Container) args[0]).Item;

            // ADAPTIVE COMPONENT
            if (AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(instance))
            {
                var refPtIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);
                FSharpList<Value> refPts = FSharpList<Value>.Empty;
                foreach (var id in refPtIds)
                {
                    var pt = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(id) as ReferencePoint;
                    refPts = FSharpList<Value>.Cons(Value.NewContainer(pt.Position), refPts);
                }
                return Value.NewList(Utils.SequenceToFSharpList(refPts.Reverse()));
            }

            // INSTANCE WITH PLACEMENT POINT
            var ptRefs = instance.GetFamilyPointPlacementReferences();
            if (ptRefs.Any())
            {
                var pts = ptRefs.Select(x => x.Location.Origin);
                var containers = pts.Select(Value.NewContainer);
                return Value.NewList(Utils.SequenceToFSharpList(containers));
            }

            LocationPoint point = null;
            LocationCurve c = null;

            // INSTANCE WITH LOCATION POINT
            point = instance.Location as LocationPoint;
            if (point != null)
            {
                return Value.NewContainer(point.Point);
            }
            

            //INSTANCE WITH LOCATION CURVE
            c = instance.Location as LocationCurve;
            if (c != null)
            {
                return Value.NewContainer(c.Curve);
            }

            throw new Exception("A location could not be found for the selected family instance(s).");
        }
    }

    [NodeName("Get Parameters")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Get parameters on an element by name.")]
    public class GetParameters : VariableInputAndOutput
    {
        public GetParameters()
        {
            InPortData.Add(new PortData("element", "The element from which to get parameters.",
                                        typeof (Value.Container)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var element = (Element)((Value.Container) args[0]).Item;
            var results = FSharpList<Value>.Empty;

            for(int i=args.Count()-1; i>0; i--)
            {
                var paramName = ((Value.String) args[i]).Item;
                var param = element.get_Parameter(paramName);
                if (param != null)
                {
                    var pd = OutPortData[i - 1];
                    switch (param.StorageType)
                    {
                        case StorageType.Double:
                            switch (param.Definition.ParameterType)
                            {
                                case ParameterType.Length:
                                    outPuts[pd] = Value.NewContainer(Units.Length.FromFeet(param.AsDouble()));
                                    break;
                                case ParameterType.Area:
                                    outPuts[pd] = Value.NewContainer(Units.Area.FromSquareFeet(param.AsDouble()));
                                    break;
                                case ParameterType.Volume:
                                    outPuts[pd] = Value.NewContainer(Units.Volume.FromCubicFeet(param.AsDouble()));
                                    break;
                                default:
                                    outPuts[pd] = Value.NewNumber(param.AsDouble());
                                    break;
                            }
                            break;
                        case StorageType.ElementId:
                            outPuts[pd] = FScheme.Value.NewContainer(param.AsElementId());
                            break;
                        case StorageType.Integer:
                            outPuts[pd] = FScheme.Value.NewNumber(param.AsInteger());
                            break;
                        case StorageType.String:
                            if (string.IsNullOrEmpty(param.AsString()))
                            {
                                outPuts[pd] = FScheme.Value.NewString(string.Empty);
                            }
                            else
                            {
                                outPuts[pd] = FScheme.Value.NewString(param.AsString());
                            }
                            break;
                        default:
                            if (string.IsNullOrEmpty(param.AsValueString()))
                            {
                                outPuts[pd] = FScheme.Value.NewString(string.Empty);
                            }
                            else
                            {
                                outPuts[pd] = FScheme.Value.NewString(param.AsValueString());
                            }
                            break;
                    }
                }
            }
        }

        protected override void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);

                //this node will always have one input
                //so the inputs collection will be one larger
                //than the outputs
                OutPortData.RemoveAt(count - 2);
            }
        }

        protected override string GetInputRootName()
        {
            return "parameter";
        }

        protected override string GetOutputRootName()
        {
            return "value";
        }

        protected override string GetTooltipRootName()
        {
            return "parameter";
        }

    }

}


