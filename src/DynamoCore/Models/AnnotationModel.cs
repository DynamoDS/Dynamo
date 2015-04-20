
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
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
        public event Func<Guid,ModelBase> GetModelBaseEvent;      
        public double InitialTop { get; set; } //required to calculate the TOP position in a group         
        public double InitialHeight { get; set; } //required to calculate the HEIGHT of a group          
        private const double DoubleValue = 0.0;
        private const double MinTextHeight = 20.0;
        private const double ExtendSize = 10.0;
        //DeletedModelBases is used to keep track of deleted / ungrouped models. 
        //During Undo operations this is used to get those models that are deleted from the group
        public List<ModelBase> DeletedModelBases { get; set; }
        public bool loadFromXML { get; set; }

        private double width;
        public new double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        private double height;
        public new double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

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
            get { return new Rect2D(this.X, this.Y, this.Width, this.Height); }
        }

        private Double textBlockHeight;
        public Double TextBlockHeight
        {
            get { return textBlockHeight; }
            set
            {
                textBlockHeight = value;                
                Y = InitialTop - textBlockHeight;
                Height = InitialHeight + textBlockHeight - MinTextHeight;
            }
        }

        private double fontSize = 14;
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
            DeletedModelBases = new List<ModelBase>(); 
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
                case "Text":
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
                DeletedModelBases.Add(model);
                SelectedModels = modelList;
                UpdateBoundaryFromSelection();
            }
        }
      
        /// <summary>
        /// Updates the group boundary based on the nodes / notes selection.
        /// </summary>      
        internal void UpdateBoundaryFromSelection()
        {          
            var selectedModelsList = selectedModels.ToList();
          
            if (selectedModelsList.Any())
            {
                var groupModels = selectedModelsList.OrderBy(x => x.X).ToList();
              
                //Shifting x by 10 and y to the height of textblock
                var regionX = groupModels.Min(x => x.X) - ExtendSize;
                var regionY = groupModels.Min(y => y.Y) - (TextBlockHeight == 0.0 ? MinTextHeight : TextBlockHeight);
              
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
                    Width = xDistance + maxWidth + ExtendSize,
                    Height = yDistance + maxHeight + ExtendSize
                };
             
                this.X = region.X;              
                this.Y = region.Y;
                this.Width = region.Width;
                this.Height = region.Height;

                //Calculate the boundary if there is any overlap
                ModelBase overlap = null;
                foreach (var nodes in SelectedModels)
                {
                    if (!region.Contains(nodes.Rect))
                    {
                        overlap = nodes;
                        if (overlap.Rect.Top < this.X ||
                                    overlap.Rect.Bottom > region.Bottom) //Overlap in height - increase the region height
                        {
                            if (overlap.Rect.Bottom - region.Bottom > 0)
                            {
                                this.Height += overlap.Rect.Bottom - region.Bottom + ExtendSize;
                            }
                            region.Height = this.Height;
                        }
                        if (overlap.Rect.Left < this.Y ||
                                overlap.Rect.Right > region.Right) //Overlap in width - increase the region width
                        {
                            if (overlap.Rect.Right - region.Right > 0)
                            {
                                this.Width += overlap.Rect.Right - region.Right + ExtendSize;
                            }
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
              
        #region Serialization/Deserialization Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "FontSize":
                    FontSize = Convert.ToDouble(value);
                    break;
                case "Background":
                    Background = value;
                    break;  
                case "TextBlockText":
                    AnnotationText = value;
                    break;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        protected override
             void SerializeCore(XmlElement element, SaveContext context)
        {            
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("annotationText", this.AnnotationText);
            helper.SetAttribute("left", this.X);
            helper.SetAttribute("top", this.Y);
            helper.SetAttribute("width", this.Width);
            helper.SetAttribute("height", this.Height);
            helper.SetAttribute("fontSize", this.FontSize);
            helper.SetAttribute("InitialTop", this.InitialTop);
            helper.SetAttribute("InitialHeight", this.InitialHeight);
            helper.SetAttribute("TextblockHeight", this.TextBlockHeight);
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
            this.GUID = helper.ReadGuid("guid", this.GUID);
            this.annotationText = helper.ReadString("annotationText", Resources.GroupDefaultText);
            this.X = helper.ReadDouble("left", DoubleValue);
            this.Y = helper.ReadDouble("top", DoubleValue);
            this.width = helper.ReadDouble("width", DoubleValue);
            this.height = helper.ReadDouble("height", DoubleValue);
            this.background = helper.ReadString("backgrouund", "");
            this.fontSize = helper.ReadDouble("fontSize", fontSize);
            this.textBlockHeight = helper.ReadDouble("TextblockHeight", DoubleValue);
            this.InitialTop = helper.ReadDouble("InitialTop", DoubleValue);
            this.InitialHeight = helper.ReadDouble("InitialHeight", DoubleValue);
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
                         ModelBase model = null;
                         if(GetModelBaseEvent != null)
                              model = GetModelBaseEvent(result);
                         else
                         {
                             model = SelectedModels.FirstOrDefault(x => x.GUID == result);
                         }
                        listOfModels.Add(model);
                    }                  
                }
                selectedModels = listOfModels;        
            }

            //On any Undo Operation, current values are restored to previous values.
            //These properties should be Raised, so that they get the correct value on Undo.
            RaisePropertyChanged("Background");
            RaisePropertyChanged("FontSize");
            RaisePropertyChanged("AnnotationText");
            RaisePropertyChanged("SelectedModels");
        }

        /// <summary>
        /// This is called when a model is deleted from a group
        /// and UNDO is clicked.
        /// </summary>
        /// <param name="model">The model.</param>
        internal void AddToSelectedModels(ModelBase model)
        {           
            var list = this.SelectedModels.ToList();
            list.Add(model);
            this.SelectedModels = list;
            this.loadFromXML = false;
            this.UpdateBoundaryFromSelection();
        }

        #endregion

        public override void Dispose()
        {
            if (this.SelectedModels.Any())
            {
                foreach (var model in this.SelectedModels)
                {
                    model.PropertyChanged -= model_PropertyChanged;
                    model.Disposed -= model_Disposed;
                }
            }
        }     
    }   
}
