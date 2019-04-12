using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProtoFFI
{
    /// <summary>
    /// Represents an external data to be used as context for execution.
    /// </summary>
    interface IContextData
    {
        /// <summary>
        /// Returns name of the data. This context data can be identified with
        /// name in designscript world.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The context data as represented in DesignScript
        /// </summary>
        Object Data { get; }

        /// <summary>
        /// Event notifier to notify when it's data changes.
        /// </summary>
        event EventHandler DataChanged;

        /// <summary>
        /// Returns the context provider for interpretation of data in designscript
        /// world.
        /// </summary>
        IContextDataProvider ContextProvider { get; }
    }

    /// <summary>
    /// Represents a connector to external data source to provide context 
    /// specific data. This interface provide import/export feature for any 
    /// context specific data. It also provides a mechanism to capture data 
    /// interactively.
    /// </summary>
    interface IContextDataProvider
    {
        /// <summary>
        /// Returns the name of this data provider to identify it uniquely.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Imports the specific context data using the given identifier string.
        /// </summary>
        /// <param name="connectionParameters">Input dictionary of connection parameters 
        /// to connect to the data source to import the data. Each context data in 
        /// the list contains pair of connection parameter name and value</param>
        /// <returns></returns>
        IContextData[] ImportData(Dictionary<string, Object> connectionParameters);

        /// <summary>
        /// Exports data to the specified file. This context provider determines the
        /// format for data store and returns the connection string for the given
        /// file using which this data can be imported back again.
        /// </summary>
        /// <param name="data">Collection of data that needs to be exported.</param>
        /// <param name="filePath">Path for the file where this data can be 
        /// exported and saved.</param>
        /// <returns>The connection parameters using which the exported data can be 
        /// imported in future.Each context data in the list contains 
        /// pair of connection parameter name and value</returns>
        Dictionary<string, Object> ExportData(IContextData[] data, string filePath);

        /// <summary>
        /// Begins data capture interaction in the specific context and returns 
        /// collection of captured data.
        /// </summary>
        /// <returns>Dictionary of connection parameters to import the data 
        /// captured by interaction. Each context data in the list contains 
        /// pair of connection parameter name and value</returns>
        Dictionary<string, Object> CaptureData();

        /// <summary>
        /// Returns DesignScript expression for given parameters assigned to input
        /// variable.
        /// </summary>
        /// <param name="parameters">Captured parameters to be converted to
        /// DesignScript expression assigned to the input variable.</param>
        /// <param name="variable">Variable name to which imported data to be 
        /// assigned.</param>
        /// <returns>DesignScript expression string</returns>
        string GetExpression(Dictionary<string, Object> parameters, string variable);
    }

    class ContextData : IContextData
    {
        private IContextDataProvider mContextDataProvider;

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
