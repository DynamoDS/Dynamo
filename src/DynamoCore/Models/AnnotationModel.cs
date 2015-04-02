
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using Dynamo.Properties;
using Dynamo.Utilities;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Editing;
using ProtoCore.AST;

namespace Dynamo.Models
{
    public class AnnotationModel : ModelBase
    {
        #region Properties
        public double InitialTop { get; set; } //required to calculate the TOP position in a group         
        public double InitialHeight { get; set; } //required to calculate the HEIGHT of a group 
        private string modelGuids { get; set; }        
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
                if (value == String.Empty)
                    _annotationText = Resources.GroupDefaultText;
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

        /// <summary>
        /// Overriding the Rect from Modelbase
        /// This gets the actual RECT of the group. 
        /// This is required to make the group as ILocatable.
        /// </summary>      
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
                Top = InitialTop - textBlockHeight;
                Height = InitialHeight + textBlockHeight;
            }
        }

        private double fontSize = 10;
        public Double FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                RaisePropertyChanged("FontSize");
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationModel"/> class.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="loadFromGraph">This is true when graph is loaded from XML</param>
        public AnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes, bool loadFromGraph=false)
        {                                 
            var nodeModels = nodes as NodeModel[] ?? nodes.ToArray();           
            var noteModels = notes as NoteModel[] ?? notes.ToArray();

            this.SelectedModels = nodeModels.Concat(noteModels.Cast<ModelBase>()).ToList();

            if (!loadFromGraph)
                CreateGroupingOnModels();
        }


        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    CreateGroupingOnModels();
                    break;
                case "Y":
                    CreateGroupingOnModels();
                    break;
            }
        }

        /// <summary>
        /// Recalculate the group when a node is disposed
        /// </summary>
        /// <param name="node">The node.</param>
        private void node_Disposed(NodeModel node)
        {
            var nodesList = this.SelectedModels.ToList();
            bool remove = nodesList.Remove(node);
            if (remove)
            {
                SelectedModels = nodesList;
                CreateGroupingOnModels();
            }
        }

        /// <summary>
        /// Recaluclate the group when a note is disposed
        /// </summary>
        /// <param name="note">The note.</param>
        private void note_Disposed(NoteModel note)
        {
            var notesList = this.SelectedModels.ToList();
            bool remove = notesList.Remove(note);
            if (remove)
            {
                SelectedModels = notesList;
                CreateGroupingOnModels();
            }
        }

        /// <summary>
        /// Creates the grouping on selected models.
        /// </summary>      
        private void CreateGroupingOnModels()
        {
            var selectedModelsList = selectedModels.ToList();
          
            if (selectedModelsList.Any())
            {
                var groupModels = selectedModelsList.OrderBy(x => x.X).ToList();
              
                //Shifting x by 10 and y to the height of textblock
                var regionX = groupModels.Min(x => x.X) - 10;
                var regionY = groupModels.Min(y => y.Y) - TextBlockHeight;
              
                //calculates the distance between the nodes
                var xDistance = groupModels.Max(x => x.X) - regionX;
                var yDistance = groupModels.Max(x => x.Y) - regionY;

                var widthandheight = CalculateWidthAndHeight();

                var maxWidth = widthandheight.Item1;
                var maxHeight = widthandheight.Item2;

                // InitialTop is to store the Y value without the Textblock height
                this.InitialTop = groupModels.Min(y => y.Y);

                var region = new Rect2D
                {
                    X = regionX,
                    Y = regionY,
                    Width = xDistance + maxWidth + 10,
                    Height = yDistance + maxHeight + 10
                };
             
                this.Left = region.X;              
                this.Top = region.Y;
                this.Width = region.Width;
                this.Height = region.Height; 
                //Initial Height is to store the Actual height of the group.
                this.InitialHeight = region.Height;               
            }
        }

        /// <summary>
        /// Group the Models based on Height and Width
        /// </summary>
        /// <returns></returns>
        private Tuple<Double,Double> CalculateWidthAndHeight()
        {           
            var xgroup = SelectedModels.OrderBy(x => x.X + x.Width).ToList();
            var ygroup = SelectedModels.OrderBy(x => x.Y + x.Height ).ToList();
           
            return Tuple.Create(xgroup.Last().Width, ygroup.Last().Height);
        }
        
        /// <summary>
        /// Deserializes the model guids from XML
        /// and creates group on those model guids.
        /// </summary>
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
        }

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
            helper.SetAttribute("fontSize", this.FontSize);
            helper.SetAttribute("initialTop", this.InitialTop);
            helper.SetAttribute("initialHeight", this.InitialHeight);
            helper.SetAttribute("annotationColor", (this.BackGroundColor == null ? "" : this.BackGroundColor.ToString()));
            helper.SetAttribute("ModelGUIDs", string.Join(",", this.SelectedModels.Select(x => x.GUID)));           
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("guid", this.GUID);
            this.AnnotationText = helper.ReadString("text", "<<Click to edit the grouping>>");
            this.Left = helper.ReadDouble("left", 0.0);
            this.Top = helper.ReadDouble("top", 0.0);
            this.Width = helper.ReadDouble("width", 0.0);
            this.Height = helper.ReadDouble("height", 0.0);
            this.BackGroundColor = helper.ReadString("annotationColor", "");
            this.FontSize = helper.ReadDouble("fontSize", 10.0);
            this.InitialTop = helper.ReadDouble("initialTop", 0.0);
            this.InitialHeight = helper.ReadDouble("initialHeight", 0.0);
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
