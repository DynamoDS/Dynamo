
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using System.Collections.Generic;
using Dynamo.Nodes;
using ProtoCore.Lang;

namespace Dynamo.Models
{
    public class AnnotationModel : ModelBase
    {
        private double initialTop;        
        private double initialHeight;
        private string modelGuids { get; set; }
        private readonly IEnumerable<ModelBase> modelsInWorkspace;
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
              
        private IEnumerable<ModelBase> selectedModels;
        public IEnumerable<ModelBase> SelectedModels
        {
            get { return selectedModels; }
            set
            {
                selectedModels = value.ToList(); ;
                if (selectedModels != null && selectedModels.Any())
                {
                    foreach (var model in selectedModels)
                    {
                        model.PropertyChanged +=model_PropertyChanged;
                        if (model is NodeModel)
                        {
                            var nodeModel = model as NodeModel;
                            nodeModel.Disposed += node_Disposed;
                        }

                        if (model is NoteModel)
                        {
                            var noteModel = model as NoteModel;
                            noteModel.Disposed += note_Disposed;
                        }
                    }
                }
            }
            
        }

        public override Rect2D Rect
        {
            get { return new Rect2D(this.Left, this.Top, this.Width, this.Height); }
        }

        private Double textBlockHeight;
        public Double TextBlockHeight
        {
            get { return textBlockHeight; }
            set
            {
                textBlockHeight = value;
                Top = initialTop - textBlockHeight;
                Height = initialHeight + textBlockHeight;
            }
        }

        public AnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes )
        {                      
            this.AnnotationText = "<<click to edit the grouping>>";
            var nodeModels = nodes as NodeModel[] ?? nodes.ToArray();           
            var noteModels = notes as NoteModel[] ?? notes.ToArray();

            this.SelectedModels = nodeModels.Concat(noteModels.Cast<ModelBase>()).ToList();
           
            CreateGroupingOnModels(SelectedModels);
        }


        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    CreateGroupingOnModels(SelectedModels);
                    break;
                case "Y":
                    CreateGroupingOnModels(SelectedModels);
                    break;
            }
        }
          
        private void node_Disposed(NodeModel node)
        {
            var nodesList = this.SelectedModels.ToList();
            bool remove = nodesList.Remove(node);
            if (remove)
            {
                CreateGroupingOnModels(SelectedModels);
            }
        }

        private void note_Disposed(NoteModel note)
        {
            var notesList = this.SelectedModels.ToList();
            bool remove = notesList.Remove(note);
            if (remove)
            {
                CreateGroupingOnModels(SelectedModels);
            }
        }

        private void CreateGroupingOnModels(IEnumerable<ModelBase> selectedModels)
        {
            var selectedModelsList = selectedModels.ToList();
          
            //var enumerable = nodes as NodeModel[] ?? nodes.ToArray();
            //var nodeModels = nodes as IList<NodeModel> ?? enumerable.ToList();
            if (selectedModelsList.Any())
            {
                var groupModels = selectedModelsList.OrderBy(x => x.X).ToList();

                var maxWidth = groupModels.Max(x => x.Width);
                var maxHeight = groupModels.Max(y => y.Height);

                var regionX = groupModels.Min(x => x.X) - 10;
                var regionY = groupModels.Min(y => y.Y) - TextBlockHeight;

                var xDistance = groupModels.Max(x => x.X) - regionX;
                var yDistance = groupModels.Max(x => x.Y) - regionY;

               
                var region = new Rect2D
                {
                    X = regionX,
                    Y = regionY,
                    Width = xDistance + maxWidth + 10,
                    Height = yDistance + maxHeight + 10
                };

                //special case for grouping just one node
                //if (enumerable.Count() == 1)
                //{
                //    var node = nodeModels.ElementAt(0);
                //    if (region.IntersectsWith(node.Rect))
                //    {
                //        var result = region;
                //        result.Intersect(node.Rect);
                //        region.Width = region.Width + 10;
                //        region.Height = region.Height + 70;
                //    }
                //}

                this.Left = region.X;
                this.Top = region.Y;
                this.Width = region.Width;
                this.Height = region.Height;
                this.initialTop = region.Y;
                this.initialHeight = region.Height;               
            }
        }
    
        private void DeserializeGroup()
        {
            var listOfModels = new List<ModelBase>();
            foreach (var objGuid in modelGuids.Split(','))
            {
                Guid result;
                if (Guid.TryParse(objGuid, out result))
                    if (SelectedModels != null)
                    {
                        var model = SelectedModels.FirstOrDefault(x => x.GUID == result);
                        listOfModels.Add(model);
                    }
            }
            SelectedModels = listOfModels;
            CreateGroupingOnModels(SelectedModels);
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
            helper.SetAttribute("ModelGUIDs", string.Join(",", this.SelectedModels.Select(x => x.GUID)));           
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
            modelGuids = helper.ReadString("ModelGUIDs", "");           
            DeserializeGroup();           
        }

        #endregion

        public virtual void Dispose()
        {
            if (this.SelectedModels.Any())
            {
                foreach (var model in this.SelectedModels)
                {
                    model.PropertyChanged -= model_PropertyChanged;
                }
            }
        }
    }   
}
