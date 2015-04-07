
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
        private const double doubleValue = 0.0;
        public bool loadFromXML { get; set; }
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged("Text");
            }
        }

        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        private double top;
        public double Top
        {
            get { return top; }
            set
            {
                top = value;
                RaisePropertyChanged("Top");
            }
        }

        private double left;
        public double Left
        {
            get { return left; }
            set
            {
                left = value;
                RaisePropertyChanged("Left");
            }
        }

        private string annotationText;
        public String AnnotationText
        {
            get { return annotationText; }
            set
            {
                annotationText = value;               
                RaisePropertyChanged("AnnotationText");
            }

        }

        private string background;
        public string Background
        {
            get { return background ?? "#ff7bac"; }
            set
            {
                background = value;
                RaisePropertyChanged("Background");
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
                        model.Disposed+=model_Disposed;
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
            loadFromXML = loadFromGraph;
            if (!loadFromGraph)
                UpdateBoundaryFromSelection();
        }


        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    UpdateBoundaryFromSelection();
                    break;
                case "Y":
                    UpdateBoundaryFromSelection();
                    break;
                case "Position":
                    if(!loadFromXML)
                        UpdateBoundaryFromSelection();
                    break;
            }
        }

        /// <summary>
        /// Recalculate the group when a node is disposed
        /// </summary>
        /// <param name="node">The node.</param>
        private void model_Disposed(ModelBase model)
        {
            var modelList = this.SelectedModels.ToList();
            bool remove = modelList.Remove(model);
            if (remove)
            {
                SelectedModels = modelList;
                UpdateBoundaryFromSelection();
            }
        }
      
        /// <summary>
        /// Updates the group boundary based on the nodes / notes selection.
        /// </summary>      
        private void UpdateBoundaryFromSelection()
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

                //Calculate the boundary if there is any overlap
                ModelBase overlap = null;
                foreach (var nodes in SelectedModels)
                {
                    if (!region.Contains(nodes.Rect))
                    {
                        overlap = nodes;
                        if (overlap.Rect.Top < this.Top ||
                                    overlap.Rect.Bottom > region.Bottom) //Overlap in height - increase the region height
                        {
                            this.Height += overlap.Rect.Bottom - region.Bottom + 10;
                            region.Height = this.Height;
                        }
                        if (overlap.Rect.Left < this.Left ||
                                overlap.Rect.Right > region.Right) //Overlap in width - increase the region width
                        {
                            this.Width += overlap.Rect.Right - region.Right + 10;
                            region.Width = this.Width;
                        }
                    }
                }
                               
                //Initial Height is to store the Actual height of the group.
                this.InitialHeight = region.Height;
            }
        }

        /// <summary>
        /// Group the Models based on Height and Width
        /// </summary>
        /// <returns> the width and height of the last model </returns>
        private Tuple<Double,Double> CalculateWidthAndHeight()
        {           
            var xgroup = SelectedModels.OrderBy(x => x.X).ToList();
            var ygroup = SelectedModels.OrderBy(x => x.Y).ToList();
          
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
            selectedModels = listOfModels;           
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("GUID", this.GUID);
            helper.SetAttribute("annotationText", this.AnnotationText);
            helper.SetAttribute("left", this.Left);
            helper.SetAttribute("top", this.Top);
            helper.SetAttribute("width", this.Width);
            helper.SetAttribute("height", this.Height);
            helper.SetAttribute("fontSize", this.FontSize);
            helper.SetAttribute("InitialTop", this.InitialTop);
            helper.SetAttribute("InitialHeight", this.InitialHeight);
            helper.SetAttribute("backgrouund", (this.Background == null ? "" : this.Background.ToString()));        
            //Serialize Selected models
            XmlDocument xmlDoc = element.OwnerDocument;            
            foreach (var guids in this.SelectedModels.Select(x => x.GUID))
            {
                if (xmlDoc != null)
                {
                    var modelElement = xmlDoc.CreateElement("Models");
                    element.AppendChild(modelElement);
                    XmlElementHelper mhelper = new XmlElementHelper(modelElement);
                    modelElement.SetAttribute("ModelGuid", guids.ToString());
                }
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("GUID", this.GUID);
            this.annotationText = helper.ReadString("annotationText", String.Empty);
            this.left = helper.ReadDouble("left", doubleValue);
            this.top = helper.ReadDouble("top", doubleValue);
            this.width = helper.ReadDouble("width", doubleValue);
            this.height = helper.ReadDouble("height", doubleValue);
            this.background = helper.ReadString("backgrouund", "");
            this.fontSize = helper.ReadDouble("fontSize", fontSize);
            this.InitialTop = helper.ReadDouble("InitialTop", doubleValue);
            this.InitialHeight = helper.ReadDouble("InitialHeight", doubleValue);
            //Deserialize Selected models
            if (element.HasChildNodes) 
            {
                var listOfModels = new List<ModelBase>();
                foreach (var childnode in element.ChildNodes)
                {
                    XmlElementHelper mhelper = new XmlElementHelper(childnode as XmlElement);
                     if (SelectedModels != null)
                     {
                         var result = mhelper.ReadGuid("ModelGuid", new Guid());
                        var model = SelectedModels.FirstOrDefault(x => x.GUID == result);
                        listOfModels.Add(model);
                    }                  
                }
                selectedModels = listOfModels;        
            }
          
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
