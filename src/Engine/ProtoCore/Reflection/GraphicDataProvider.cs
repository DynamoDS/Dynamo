using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.Reflection;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using Autodesk.DesignScript.Runtime;
using System.Collections;

namespace ProtoCore.Mirror
{
    class GraphicDataProvider : IGraphicDataProvider
    {
        private static Dictionary<Assembly, IGraphicDataProvider> mDataProviders =
            new Dictionary<Assembly, IGraphicDataProvider>();

        private static Dictionary<System.Type, IGraphicDataProvider> mGraphicTypes =
            new Dictionary<System.Type, IGraphicDataProvider>();

        public List<IGraphicItem> GetGraphicItems(object obj)
        {
            return GetGraphicItemsFromObject(obj);
        }

        public static List<IGraphicItem> GetGraphicItemsFromObject(object obj)
        {
            if(null == obj)
                return null;
            
            List<IGraphicItem> items = new List<IGraphicItem>();
            System.Type objType = obj.GetType();
            IEnumerable collection = obj as IEnumerable;
            if (null != collection)
            {
                foreach (var item in collection)
                {
                    List<IGraphicItem> graphics = GetGraphicItemsFromObject(item);
                    if (null != graphics)
                        items.AddRange(graphics);
                }
                return items;
            }

            IGraphicItem graphicItem = obj as IGraphicItem;
            if (graphicItem != null)
            {
                items.Add(graphicItem);
                return items;
            }

            IGraphicDataProvider dataProvider = GetGraphicDataProviderForType(objType);
            if (null == dataProvider)
            {
                dataProvider = new GraphicObjectType(objType);
                mGraphicTypes.Add(objType, dataProvider);
            }

            return dataProvider.GetGraphicItems(obj);
        }

        public static IGraphicDataProvider GetGraphicDataProviderForType(System.Type objType)
        {
            IGraphicDataProvider dataProvider = null;
            if (mGraphicTypes.TryGetValue(objType, out dataProvider))
                return dataProvider;

            //Get data provider from the assembly of the type
            System.Type type = objType;
            while (dataProvider == null)
            {
                dataProvider = GetDataProviderFromAssembly(type);
                if (null != dataProvider)
                {
                    mGraphicTypes.Add(objType, dataProvider);
                    return dataProvider;
                }

                type = type.BaseType;
                if (type == null)
                    break;
            }

            return null;
        }

        private static IGraphicDataProvider GetDataProviderFromAssembly(System.Type type)
        {
            //Check if we already have a provider for the type.Assembly
            IGraphicDataProvider provider = null;
            if (mDataProviders.TryGetValue(type.Assembly, out provider))
                return provider;

            mDataProviders.Add(type.Assembly, null); //initialize with no dataprovider from this type assembly

            //Check if this assembly implements IGraphicDataProvider interface
            var providerType = ProtoFFI.CLRDLLModule.GetImplemetationType(type.Assembly, typeof(IGraphicDataProvider), typeof(GraphicDataProviderAttribute), false);
            if (providerType == null)
                return null;

            //Got the type of IGraphicDataProvider interface implementation
            //Create an instance of IGraphicDataProvider using default constructor.
            provider = (IGraphicDataProvider)Activator.CreateInstance(providerType, true);
            if (null != provider)
                mDataProviders[type.Assembly] = provider;

            return provider;
        }

        public void Tessellate(List<object> objects, IRenderPackage package)
        {
            foreach (var item in objects)
            {
                List<IGraphicItem> graphicItems = GetGraphicItems(item);
                if (null == graphicItems || graphicItems.Count == 0)
                    continue;

                foreach (var g in graphicItems)
                {
                    g.Tessellate(package);
                }
            }
        }

        internal List<IGraphicItem> GetGraphicItems(DSASM.StackValue svData, Core core)
        {
            Validity.Assert(svData.optype == AddressType.Pointer);

            object obj = GetCLRObject(svData, core);
            if (obj != null)
                return GetGraphicItems(obj);

            return null;
        }

        internal object GetCLRObject(StackValue svData, Core core)
        {
            if (null == core.DSExecutable.classTable)
                return null;

            IList<ClassNode> classNodes = core.DSExecutable.classTable.ClassNodes;
            if (null == classNodes || (classNodes.Count <= 0))
                return null;

            ClassNode classnode = core.DSExecutable.classTable.ClassNodes[(int)svData.metaData.type];
            if (!classnode.IsImportedClass) //TODO: look at properties to see if it contains any FFI objects.
                return null;

            try
            {
                var helper = ProtoFFI.DLLFFIHandler.GetModuleHelper(ProtoFFI.FFILanguage.CSharp);
                var marshaler = helper.GetMarshaller(core);
                return marshaler.UnMarshal(svData, null, null, typeof(object));
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// This class reflects on properties of a given type and finds graphic data
    /// provider for them. If any of the property type has graphic data provider
    /// it gets the graphic items from those properties. It doesn't query indexed
    /// properties only uses public properties.
    /// </summary>
    class GraphicObjectType : IGraphicDataProvider
    {
        private List<PropertyInfo> mGraphicProperties;
        public GraphicObjectType(System.Type type)
        {
            mGraphicProperties = GetGraphicProperties(type);
        }

        private List<PropertyInfo> GetGraphicProperties(System.Type type)
        {
            List<PropertyInfo> graphicProperties = new List<PropertyInfo>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var item in properties)
            {
                //Check if we have a data provider for this property type.
                IGraphicDataProvider provider = GraphicDataProvider.GetGraphicDataProviderForType(item.PropertyType);
                if (null != provider)
                    graphicProperties.Add(item);
            }

            return graphicProperties;
        }

        public List<IGraphicItem> GetGraphicItems(object obj)
        {
            if (mGraphicProperties.Count <= 0)
                return null;

            List<IGraphicItem> graphics = new List<IGraphicItem>();
            foreach (var item in mGraphicProperties)
            {
                var property = item.GetValue(obj, null); //For now indexed property is not queried
                if (null != property)
                {
                    List<IGraphicItem> items = GraphicDataProvider.GetGraphicItemsFromObject(property);
                    if(null != items && items.Count > 0)
                        graphics.AddRange(items);
                }
            }
            if (graphics.Count > 0)
                return graphics;

            return null;
        }

        public void Tessellate(List<object> objects, IRenderPackage package)
        {
            throw new NotImplementedException();
        }
    }

}
