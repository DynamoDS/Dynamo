//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;

using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        public static ObservableDictionary<Guid, FunctionDefinition> FunctionDict =
            new ObservableDictionary<Guid, FunctionDefinition>();

        public static HashSet<FunctionDefinition> FunctionWasEvaluated =
            new HashSet<FunctionDefinition>();

        static dynSettings()
        {
        }

        public static Dynamo.Controls.DragCanvas Workbench { get; internal set; }

        public static dynCollection Collection { get; internal set; }

        //public static LinearGradientBrush ErrorBrush { get; internal set; }

        //public static LinearGradientBrush ActiveBrush { get; internal set; }

        //public static LinearGradientBrush SelectedBrush { get; internal set; }

        //public static LinearGradientBrush DeadBrush { get; internal set; }

        public static dynBench Bench { get; internal set; }

        public static TextWriter Writer { get; set; }

        public static DynamoController Controller { get; internal set; }

        public static PackageManagerClient PackageManagerClient { get; internal set; }

        public static void StartLogging()
        {
            //create log files in a directory 
            //with the executing assembly
            string log_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dynamo_logs");
            if (!Directory.Exists(log_dir))
            {
                Directory.CreateDirectory(log_dir);
            }

            string logPath = Path.Combine(log_dir, string.Format("dynamoLog_{0}.txt", Guid.NewGuid().ToString()));

            TextWriter tw = new StreamWriter(logPath);
            tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());

            Writer = tw;
        }

        public static void FinishLogging()
        {
            if (Writer != null)
            {
                Writer.WriteLine("Goodbye.");
                Writer.Close();
            }
        }
    }

    //http://blogs.microsoft.co.il/blogs/shimmy/archive/2010/12/26/observabledictionary-lt-tkey-tvalue-gt-c.aspx
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";
    private const string KeysName = "Keys";
    private const string ValuesName = "Values";

    private IDictionary<TKey, TValue> _Dictionary;
    protected IDictionary<TKey, TValue> Dictionary
    {
        get { return _Dictionary; }
    }
 
    #region Constructors
    public ObservableDictionary()
    {
        _Dictionary = new Dictionary<TKey, TValue>();
    }
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _Dictionary = new Dictionary<TKey, TValue>(dictionary);
    }
    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _Dictionary = new Dictionary<TKey, TValue>(comparer);
    }
    public ObservableDictionary(int capacity)
    {
        _Dictionary = new Dictionary<TKey, TValue>(capacity);
    }
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        _Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
    }
    public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
        _Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
    }
    #endregion
 
    #region IDictionary<TKey,TValue> Members
 
    public void Add(TKey key, TValue value)
    {
        Insert(key, value, true);
    }
 
    public bool ContainsKey(TKey key)
    {
        return Dictionary.ContainsKey(key);
    }
 
    public ICollection<TKey> Keys
    {
        get { return Dictionary.Keys; }
    }
 
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException("key");
 
        TValue value;
        Dictionary.TryGetValue(key, out value);
        var removed = Dictionary.Remove(key);
        if (removed)
        //OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
        OnCollectionChanged();
        return removed;
    }
 
    public bool TryGetValue(TKey key, out TValue value)
    {
        return Dictionary.TryGetValue(key, out value);
    }
 
    public ICollection<TValue> Values
    {
        get { return Dictionary.Values; }
    }
 
    public TValue this[TKey key]
    {
        get
        {
        return Dictionary[key];
        }
        set
        {
        Insert(key, value, false);
        }
    }
 
    #endregion
 
    #region ICollection<KeyValuePair<TKey,TValue>> Members
 
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Insert(item.Key, item.Value, true);
    }
 
    public void Clear()
    {
        if (Dictionary.Count > 0)
        {
        Dictionary.Clear();
        OnCollectionChanged();
        }
    }
 
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return Dictionary.Contains(item);
    }
 
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        Dictionary.CopyTo(array, arrayIndex);
    }
 
    public int Count
    {
        get { return Dictionary.Count; }
    }
 
    public bool IsReadOnly
    {
        get { return Dictionary.IsReadOnly; }
    }
 
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }
 
 
    #endregion
 
    #region IEnumerable<KeyValuePair<TKey,TValue>> Members
 
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }
 
    #endregion
 
    #region IEnumerable Members
 
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Dictionary).GetEnumerator();
    }
 
    #endregion
 
    #region INotifyCollectionChanged Members
 
    public event NotifyCollectionChangedEventHandler CollectionChanged;
 
    #endregion
 
    #region INotifyPropertyChanged Members
 
    public event PropertyChangedEventHandler PropertyChanged;
 
    #endregion
 
    public void AddRange(IDictionary<TKey, TValue> items)
    {
        if (items == null) throw new ArgumentNullException("items");
 
        if (items.Count > 0)
        {
        if (Dictionary.Count > 0)
        {
            if (items.Keys.Any((k) => Dictionary.ContainsKey(k)))
            throw new ArgumentException("An item with the same key has already been added.");
            else
            foreach (var item in items) Dictionary.Add(item);
        }
        else
            _Dictionary = new Dictionary<TKey, TValue>(items);
 
        OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }                                                     
    }
 
    private void Insert(TKey key, TValue value, bool add)
    {
        if (key == null) throw new ArgumentNullException("key");
 
        TValue item;
        if (Dictionary.TryGetValue(key, out item))
        {
        if (add) throw new ArgumentException("An item with the same key has already been added.");
        if (Equals(item, value)) return;
        Dictionary[key] = value;

        OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
        }
        else
        {
        Dictionary[key] = value;

        OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
        }
    }
 
    private void OnPropertyChanged()
    {
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(KeysName);
        OnPropertyChanged(ValuesName);
    }
 
    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
 
    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
 
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }
 
    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }
 
    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }
    }
 
}