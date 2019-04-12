using System;
using System.Collections;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace ProtoFFI
{
    class ContextDataManager
    {
        private Dictionary<string, IContextData> mData = new Dictionary<string, IContextData>();
        private static Dictionary<string, IContextDataProvider> mDataProviders;
        private CoreDataProvider mCoreDataProvider;

        private ContextDataManager(ProtoCore.Core core)
        {
            mCoreDataProvider = new CoreDataProvider(core);
        }

        public static ContextDataManager GetInstance(ProtoCore.Core core)
        {
            if (core.ContextDataManager != null)
                return core.ContextDataManager;

            core.ContextDataManager = new ContextDataManager(core);
            return core.ContextDataManager;
        }

        public void AddData(Dictionary<string, Object> data)
        {
            foreach (var item in data)
            {
                AddData(item.Key, item.Value);
            }
        }

        public void AddData(string name, Object data)
        {
            AddData(new ContextData(name, data, mCoreDataProvider));
        }

        public void AddData(IEnumerable<IContextData> dataItems)
        {
            foreach (var item in dataItems)
            {
                AddData(item);
            }
        }

        public void AddData(IContextData item)
        {
            mData.Add(item.Name, item);
        }

        public IContextDataProvider GetDataProvider(string name)
        {
            if (string.Compare(name, mCoreDataProvider.Name, true) == 0)
                return mCoreDataProvider;

            IContextDataProvider provider = null;
            if (DataProviders.TryGetValue(name, out provider))
                return provider;

            return null;
        }

        public static Dictionary<string, IContextDataProvider> DataProviders
        {
            get
            {
                if (null == mDataProviders)
                    LoadDataProviders();
                return mDataProviders;
            }
        }

        private static void LoadDataProviders()
        {
            mDataProviders = new Dictionary<string, IContextDataProvider>();
            //string installDir = ProtoCore.Utils.FileUtils.GetInstallLocation();
            //string[] files = Directory.GetFiles(installDir);
            //foreach (var item in files)
            //{
            //    if (string.Compare(Path.GetExtension(item), ".dll", true) != 0)
            //        continue;
            //    try
            //    {
            //        Type t = CLRDLLModule.GetImplemetationType(Assembly.LoadFrom(item),
            //            typeof(IContextDataProvider), typeof(Autodesk.DesignScript.Runtime.ContextDataProviderAttribute), false);
            //        if (null != t)
            //        {
            //            IContextDataProvider provider = (IContextDataProvider)Activator.CreateInstance(t, true);
            //            mDataProviders.Add(provider.Name, provider);
            //        }
            //    }
            //    catch
            //    {
            //        continue;
            //    }
            //}
        }

        public IContextData this[string name]
        {
            get
            {
                IContextData data = null;
                if (!mData.TryGetValue(name, out data))
                    return null;

                return data;
            }
        }

        internal ImportNode Compile(ImportModuleHandler importer)
        {
            ImportNode impNode = null;
            foreach (var item in mData)
            {
                SortedSet<Type> types = GetTypesForImport(item.Value.Data);
                foreach (var type in types)
                {
                    if (CLRObjectMarshaler.IsMarshaledAsNativeType(type))
                        continue;
                    ImportNode node = importer.Import(type.Assembly.Location, type.FullName, "");
                    if (impNode != null && node != null)
                        impNode.CodeNode.Body.AddRange(node.CodeNode.Body);
                    else
                        impNode = node;
                }
                if (impNode == null)
                    impNode = new ImportNode() { ModuleName="ExternalContext", CodeNode = new ProtoCore.AST.AssociativeAST.CodeBlockNode() };
                impNode.CodeNode.Body.Add(ContextDataMethodCallNode(item.Value));
            }
            return impNode;
        }

        //recursively get all the types this object represents.
        private SortedSet<Type> GetTypesForImport(Object value)
        {
            Type valueType = value.GetType();
            SortedSet<Type> set = new SortedSet<Type>();
            IEnumerable collection = value as IEnumerable;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    set.UnionWith(GetTypesForImport(item));
                }
            }
            else if (valueType.IsGenericType)
            {
                Type generictype = valueType.GetGenericTypeDefinition();
                set.UnionWith(generictype.GetGenericArguments());
                //also add the valueType to be used as raw data
                set.Add(valueType);
            }
            else 
                set.Add(CLRObjectMarshaler.GetPublicType(valueType));

            return set;
        }

        private AssociativeNode ContextDataMethodCallNode(IContextData data)
        {
            string appname = data.ContextProvider.Name;
            string connectionstring = data.Name;
            string varname = data.Name;
            //
            //Build a functioncall node for expression varname = ImportData(appname, connectionstring);

            var func = new ProtoCore.AST.AssociativeAST.IdentifierNode();
            func.Value = func.Name = ProtoCore.DSASM.Constants.kImportData;

            var paramAppName = new ProtoCore.AST.AssociativeAST.StringNode();
            paramAppName.Value = appname;

            var paramConnectionString = new ProtoCore.AST.AssociativeAST.StringNode();
            paramConnectionString.Value = connectionstring;

            var funcCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            funcCall.Function = func;
            funcCall.Name = ProtoCore.DSASM.Constants.kImportData;
            funcCall.FormalArguments.Add(paramAppName);
            funcCall.FormalArguments.Add(paramConnectionString);

            var var = new ProtoCore.AST.AssociativeAST.IdentifierNode();
            var.Name = var.Value = varname;

            var assignExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode();
            assignExpr.LeftNode = var;
            assignExpr.Optr = ProtoCore.DSASM.Operator.assign;
            assignExpr.RightNode = funcCall;

            return assignExpr;
        }
    }
}
