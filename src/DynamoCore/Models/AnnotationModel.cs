//AnnotationModel
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using ProtoCore.AST;
using System.Collections.ObjectModel;
using Dynamo.Selection;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Models
{
    public class AnnotationModel : ModelBase
    {
        private readonly INodeRepository nodeRepository;

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
            get { return _backGroundColor ?? "#00655682"; }
            set
            {
                _backGroundColor = value;
                RaisePropertyChanged("BackGroundColor");
            }
        }

        private IEnumerable<NodeModel> selectedNodes;
        private IEnumerable<NodeModel> SelectedNodes
        {
            get { return selectedNodes; }
            set
            {
                selectedNodes = value;
                if (selectedNodes != null)
                {
                    selectedNodes = value.ToList();
                    foreach (var node in selectedNodes)
                    {
                        node.PropertyChanged += OnNodePropertyChanged;
                    }
                    var selectedRegion = SelectRegionFromNodes(selectedNodes);
                    this.Left = selectedRegion.X - 30;
                    this.Top = selectedRegion.Y - 30;
                    this.Width = selectedRegion.Width;
                    this.Height = selectedRegion.Height;
                }
            }
        }

        public AnnotationModel(INodeRepository nodeRepository, IEnumerable<NodeModel> selectedNodeModels)
        {
            this.nodeRepository = nodeRepository;
            this.SelectedNodes = selectedNodeModels;
            this.AnnotationText = "testing";
        }

        private void OnNodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var selectedRegion = SelectRegionFromNodes(SelectedNodes);
            this.Left = selectedRegion.X - 30;
            this.Top = selectedRegion.Y - 30;
            this.Width = selectedRegion.Width;
            this.Height = selectedRegion.Height;
        }

        internal static Rect2D SelectRegionFromNodes(IEnumerable<NodeModel> nodes)
        {
            var nodeModels = nodes as IList<NodeModel> ?? nodes.ToList();
            if (nodes == null || !nodeModels.Any())
                throw new ArgumentException("nodes");

            var region = new Rect2D
            {
                X = nodeModels.ElementAt(0).X,
                Y = nodeModels.ElementAt(0).Y,
                Width = nodeModels.ElementAt(0).Width,
                Height = nodeModels.ElementAt(0).Height
            };

            foreach (var node in nodeModels)
            {
                // Extend lower bounds if needed.
                if (node.X < region.X)
                    region.X = node.X;
                if (node.Y < region.Y)
                    region.Y = node.Y;

                // Extend upper bound if needed.
                //if ((node.X + node.Width) > region.Right)
                //    region.Width = node.X + node.Width;
                //if ((node.Y + node.Height) > region.Bottom)
                //    region.Height = node.Y - node.Height > 0 ? 
                //                        node.Y - node.Height : 
                //                        node.Y + node.Height;

                if ((node.X + node.Width) > region.Right)
                    region.Width = (node.X + (2 * node.Width)) - region.X;
                else
                {
                    region.Width = region.Right > 0 ? region.Right : -(region.Right) + region.Width;
                }
                if ((node.Y + node.Height) > region.Bottom)
                    region.Height = (node.Y + (2 * node.Height)) - region.Y;
                else
                {
                    region.Height = region.Bottom > 0 ? region.Bottom : -(region.Bottom) + region.Height;
                }
            }

            return region;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name != "Text")
                return base.UpdateValueCore(updateValueParams);

            Text = value;
            return true;
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("text", this.Text);
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
            this.Text = helper.ReadString("text", "New Annotation");
            this.Left = helper.ReadDouble("left", 0.0);
            this.Top = helper.ReadDouble("top", 0.0);
            this.Width = helper.ReadDouble("width", 0.0);
            this.Height = helper.ReadDouble("height", 0.0);
            this.BackGroundColor = helper.ReadString("annotationColor", "");
            var objNodes = helper.ReadString("NodeModelGUIDs", "");
            var listOfNodes = new List<NodeModel>();
            foreach (var objGuid in objNodes.Split(','))
            {
                Guid result;
                if (Guid.TryParse(objGuid, out result))
                    if (nodeRepository != null)
                    {
                        var model = nodeRepository.GetNodeModel(result);
                        listOfNodes.Add(model);
                    }
            }
            this.SelectedNodes = listOfNodes;
        }

        #endregion
    }
}
