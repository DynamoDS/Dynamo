
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using System.Collections.Generic;
using Dynamo.Nodes;
namespace Dynamo.Models
{
    public class AnnotationModel : ModelBase
    {
       // private readonly INodeRepository nodeRepository;

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private double _top;
        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged("Top");
            }
        }

        private double _left;
        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                RaisePropertyChanged("Left");
            }
        }

        private string _annotationText;
        public String AnnotationText
        {
            get { return _annotationText; }
            set
            {
                _annotationText = value;
                RaisePropertyChanged("AnnotationText");
            }

        }

        private string _backGroundColor;
        public string BackGroundColor
        {
            get { return _backGroundColor ?? "#ff7bac"; }
            set
            {
                _backGroundColor = value;
                RaisePropertyChanged("BackGroundColor");
            }
        }
       
        private string nodeGuids { get; set; }

        private IEnumerable<NodeModel> nodesInWorkspace;

        public IEnumerable<NodeModel> NodesInWorkspace
        {
            get
            {
                return nodesInWorkspace;                
            }
            set { nodesInWorkspace = value; }
        }

        private IEnumerable<NodeModel> selectedNodes;
        public IEnumerable<NodeModel> SelectedNodes
        {
            get { return selectedNodes; }
            set
            {
                selectedNodes = value;
                if (selectedNodes != null)
                {
                    selectedNodes = value.ToList();
                    if (selectedNodes.Any())
                    {
                        foreach (var node in selectedNodes)
                        {
                            node.PropertyChanged += OnNodePropertyChanged;                           
                        }

                        var selectedRegion = SelectRegionFromNodes(selectedNodes);
                        this.Left = selectedRegion.X;
                        this.Top = selectedRegion.Y;
                        this.Width = selectedRegion.Width;
                        this.Height = selectedRegion.Height;
                        this.RectRegion = new Rect2D(this.Left, this.Top, this.Width, this.Height);
                    }
                }              
            }
        }

        private Rect2D rectRegion;
        public Rect2D RectRegion
        {
            get { return rectRegion; }
            set { rectRegion = value; }
        }

        private bool isInDrag;
        public bool IsInDrag
        {
            get { return isInDrag; }
            set { isInDrag = value; }
        }

        public AnnotationModel(IEnumerable<NodeModel> nodes )
        {
            this.nodesInWorkspace = nodes;
            this.SelectedNodes = nodes.Where(x => x.IsSelected);
            this.AnnotationText = "<<Double click to edit the grouping>>";
        }

        private void OnNodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            if (!IsInDrag)
            {
                var node = sender as NodeModel;
                var selectedRegion = SelectRegionFromNodes(SelectedNodes);
                this.Left = selectedRegion.X;
                this.Top = selectedRegion.Y;
                this.Width = selectedRegion.Width;
                this.Height = selectedRegion.Height;
                this.RectRegion = new Rect2D(this.Left, this.Top, this.Width, this.Height);                
            }
        }

        internal static Rect2D SelectRegionFromNodes(IEnumerable<NodeModel> nodes)
        {
            var enumerable = nodes as NodeModel[] ?? nodes.ToArray();
            var nodeModels = nodes as IList<NodeModel> ?? enumerable.ToList();
            if (nodes == null || !nodeModels.Any())
                throw new ArgumentException("nodes");

            nodeModels = nodeModels.ToList().OrderBy(x => x.X).ToList();

           // var maxWidth = nodeModels.ToList().Select(x => x.Width).Max();
           // var maxHeight = nodeModels.ToList().Select(x => x.Height).Max();

            var xNodeModels = nodeModels.ToList().OrderBy(x => x.X).ToList();
            var yNodeModels = nodeModels.ToList().OrderBy(x => x.Y).ToList();

            var maxWidth = xNodeModels.Last().Width;
            var maxHeight = yNodeModels.Last().Height;

            var regionX = xNodeModels.First().X - 30;
            var regionY = yNodeModels.First().Y - 50;

            var xDistance = xNodeModels.Last().X - regionX;
            var yDistance = yNodeModels.Last().Y - regionY;

            var region = new Rect2D
            {
                X = regionX,
                Y = regionY,
                Width = xDistance + maxWidth,
                Height = yDistance + maxHeight
            };

            //special case for grouping just one node
            if (enumerable.Count() == 1)
            {
                var node = nodeModels.ElementAt(0);
                if (region.IntersectsWith(node.Rect))
                {
                    var result = region;
                    result.Intersect(node.Rect);
                    region.Width = region.Width + 10;
                    region.Height = region.Height + 70;
                }
            }
         
            return region;
        }

        private void DeserializeNodeModels()
        {
            var listOfNodes = new List<NodeModel>();
            this.isInDrag = false;
            foreach (var objGuid in nodeGuids.Split(','))
            {
                Guid result;
                if (Guid.TryParse(objGuid, out result))
                    if (NodesInWorkspace != null)
                    {
                        var model = NodesInWorkspace.FirstOrDefault(x => x.GUID == result);                        
                        listOfNodes.Add(model);
                    }
            }
            this.SelectedNodes = listOfNodes;          
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name != "AnnotationText")
                return base.UpdateValueCore(updateValueParams);

            AnnotationText = value;
            return true;
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("text", this.AnnotationText);
            helper.SetAttribute("left", this.Left);
            helper.SetAttribute("top", this.Top);
            helper.SetAttribute("width", this.Width);
            helper.SetAttribute("height", this.Height);
            helper.SetAttribute("annotationColor", (this.BackGroundColor == null ? "" : this.BackGroundColor.ToString()));
            helper.SetAttribute("NodeModelGUIDs", string.Join(",", this.SelectedNodes.Select(x => x.GUID)));
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("guid", this.GUID);
            this.AnnotationText = helper.ReadString("text", "<<Double click to edit the grouping>>");
            this.Left = helper.ReadDouble("left", 0.0);
            this.Top = helper.ReadDouble("top", 0.0);
            this.Width = helper.ReadDouble("width", 0.0);
            this.Height = helper.ReadDouble("height", 0.0);
            this.BackGroundColor = helper.ReadString("annotationColor", "");
            nodeGuids = helper.ReadString("NodeModelGUIDs", "");
            DeserializeNodeModels();
            this.RectRegion = new Rect2D(this.Left, this.Top, this.Width, this.Height);           
        }

        #endregion

        public virtual void Dispose()
        {
            if (this.SelectedNodes.Any())
            {
                foreach (var node in this.SelectedNodes)
                {
                    node.PropertyChanged -= OnNodePropertyChanged;
                }
            }
        }
    }
}
