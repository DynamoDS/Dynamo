using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;

namespace ProtoFFI
{
    class ContextData : IContextData
    {
        private IContextDataProvider mContextDataProvider = null;

        public ContextData(string name, Object data, IContextDataProvider provider)
        {
            Name = name;
            Data = data;
            mContextDataProvider = provider;

            INotifyPropertyChanged notify = data as INotifyPropertyChanged;
            if(notify != null)
                notify.PropertyChanged += NotifyPropertyChanged;
        }

        void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (null != DataChanged)
                DataChanged(this, new EventArgs());
        }

        public string Name
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            private set;
        }

        public IContextDataProvider ContextProvider
        {
            get { return mContextDataProvider; }
        }

        public event EventHandler DataChanged;
    }

    class CoreDataProvider : IContextDataProvider
    {
        private static readonly string mName = "ProtoCoreDataProvider"; 
        private ProtoCore.Core mCoreObject = null;
        public CoreDataProvider(ProtoCore.Core core)
        {
            mCoreObject = core;
        }

        public string Name
        {
            get { return mName; }
        }

        public IContextData[] ImportData(Dictionary<string, object> connectionParameters)
        {
            //Expects context data name as connection string
            List<IContextData> data = new List<IContextData>();
            foreach (var item in connectionParameters)
            {
                IContextData d = mCoreObject.ContextDataManager[item.Value as String];
                data.Add(d);
            }
            return data.ToArray();
        }

        Dictionary<string, object> IContextDataProvider.ExportData(IContextData[] data, string filePath)
        {
            throw new NotImplementedException();
        }

        Dictionary<string, object> IContextDataProvider.CaptureData()
        {
            throw new NotImplementedException();
        }

        string IContextDataProvider.GetExpression(Dictionary<string, object> parameters, string variable)
        {
            throw new NotImplementedException();
        }
    }
}
