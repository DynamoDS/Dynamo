using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using System.Windows.Media.Imaging;
using System.Resources;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dynamo.Search.SearchElements
{

    /// <summary>
    /// A search element representing a local node </summary>
    public partial class NodeSearchElement : SearchElementBase, IEquatable<NodeSearchElement>
    {
        private static Dictionary<string, BitmapImage> _cachedIcons =
               new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

        #region Properties

        /// <summary>
        /// Node property </summary>
        /// <value>
        /// The node used to instantiate this object </value>
        public NodeModel Node { get; internal set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        private string _type;
        public override string Type { get { return _type; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name { get { return _name; } }

        private string _fullName;
        public string FullName { get { return _fullName; } }
        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description { get { return _description; } }

        private IEnumerable<Tuple<string, string>> _inputParametrs;
        public IEnumerable<Tuple<string, string>> InputParametrs 
        { 
            get 
            {
                if (_inputParametrs==null)
                {
                    List<Tuple<string, string>> list = new List<Tuple<string, string>>();
                    list.Add(Tuple.Create<string, string>("","none"));
                    return list;
                }
                    return _inputParametrs;
            } 
        }

        private string _outputParametrs;
        public string OutputParametrs { get { return _outputParametrs; } }

		private BitmapImage _smallIcon;
        public BitmapImage SmallIcon { get { return _smallIcon ?? (_smallIcon = GetSmallIcon(this)); } }
		
        private bool _searchable = true;
        public override bool Searchable { get { return _searchable; } }

        public void SetSearchable(bool s)
        {
            _searchable = s;
        }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override sealed double Weight { get; set; }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override sealed string Keywords { get; set; }

        /// <summary>
        /// Whether the description of this node should be visible or not
        /// </summary>
        private bool _descriptionVisibility = false;
        public bool DescriptionVisibility
        {
            get { return _descriptionVisibility; }
            set
            {
                _descriptionVisibility = value;
                RaisePropertyChanged("DescriptionVisibility");
            }
        }
        #endregion

        private string _assembly;
        private string smallIconPostfix = ".Small";

        /// <summary>
        ///     The class constructor - use this constructor for built-in types\
        ///     that are not yet loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="tags"></param>
        /// <param name="fullName"></param>
        public NodeSearchElement(string name, string description, IEnumerable<string> tags, string fullName = "", string assembly="", IEnumerable<Tuple<string, string>> inputParametrs = null, string outputParametrs = "")
        {
            this.Node = null;
            this._name = name;
            this.Weight = 1;
            this.Keywords = String.Join(" ", tags);
            this._type = "Node";
            this._description = description;
            this._fullName = fullName;
            this._inputParametrs = inputParametrs;
            this._outputParametrs = outputParametrs;
            this._assembly = assembly;
        }

        public virtual NodeSearchElement Copy()
        {
            var f = new NodeSearchElement(this.Name, this.Description, new List<string>(), this._fullName,"", this._inputParametrs, this._outputParametrs);
            f.FullCategoryName = this.FullCategoryName;
            return f;
        }

        private void ToggleIsVisible(object parameter)
        {
            if (this.DescriptionVisibility != true)
            {
                this.DescriptionVisibility = true;
            }
            else
            {
                this.DescriptionVisibility = false;
            }
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            // create node
            var guid = Guid.NewGuid();
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.CreateNodeCommand(guid, this._fullName, 0, 0, true, true));

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals(obj as NodeSearchElement);
        }

        /// <summary>
        /// Overriding equals, we need to override hashcode </summary>
        /// <returns> A unique hashcode for the object </returns>
        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Name.GetHashCode() + this.Description.GetHashCode();
        }

        public bool Equals(NodeSearchElement other)
        {
            return this.Name == other.Name && this.FullCategoryName == other.FullCategoryName;
        }

        private BitmapImage GetSmallIcon(NodeSearchElement member)
        {
            var resourceAssemblyPath = "";

            if (_cachedIcons.ContainsKey(member._fullName))
                return _cachedIcons[member._fullName];

            if (!string.IsNullOrEmpty(member._assembly))
                if (ResolveResourceAssembly(member._assembly, ref resourceAssemblyPath))
                {
                    System.Reflection.Assembly resourcesAssembly = System.Reflection.Assembly.LoadFrom(resourceAssemblyPath);

                    System.IO.Stream stream =
                        resourcesAssembly.GetManifestResourceStream
                        (resourcesAssembly.GetManifestResourceNames()[0]);

                    if (stream != null)
                    {

                        ResourceReader resReader = new ResourceReader(stream);
                        Dictionary<string, object> data = resReader
                                    .OfType<DictionaryEntry>()
                                    .Select(i => new { Key = i.Key.ToString(), value = i.Value })
                                    .ToDictionary(i => i.Key, i => i.value);

                        foreach (var item in data)
                        {
                            MemoryStream memory = new MemoryStream();
                            Bitmap bitmap;
                            BitmapImage bitmapImage = new BitmapImage();
                            if (item.Value != null)
                            {
                                bitmap = item.Value as Bitmap;
                                bitmap.Save(memory, ImageFormat.Png);
                                memory.Position = 0;
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                            }
                            if (!_cachedIcons.ContainsKey(item.Key))
                                _cachedIcons.Add(item.Key, bitmapImage);
                        }
                        if (_cachedIcons.ContainsKey(String.Concat(member._fullName, smallIconPostfix)))
                            return _cachedIcons[String.Concat(member._fullName, smallIconPostfix)];


                    }
                    return null;
                }
            return null;
        }

        public static bool ResolveResourceAssembly(string assemblyLocation, ref string resourceAssemblyPath)
        {


            var qualifiedPath = Path.GetFullPath(assemblyLocation);
            var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = fn + ".resources.dll";

            resourceAssemblyPath = Path.Combine(dir, fn);

            return File.Exists(resourceAssemblyPath);
        }

    }

}