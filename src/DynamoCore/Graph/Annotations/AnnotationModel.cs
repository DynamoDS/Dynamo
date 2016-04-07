using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Properties;
using Dynamo.Utilities;

namespace Dynamo.Graph.Annotations
{
    public class AnnotationModel : ModelBase
    {
        #region Properties
        public event Func<Guid, ModelBase> ModelBaseRequested;      
        public double InitialTop { get; set; } //required to calculate the TOP position in a group         
        public double InitialHeight { get; set; } //required to calculate the HEIGHT of a group          
        private const double DoubleValue = 0.0;
        private const double MinTextHeight = 20.0;
        private const double ExtendSize = 10.0;
        private const double ExtendYHeight = 5.0;
        public  string GroupBackground = "#FFC1D676";
        //DeletedModelBases is used to keep track of deleted / ungrouped models. 
        //During Undo operations this is used to get those models that are deleted from the group
        public List<ModelBase> DeletedModelBases { get; set; }
        public bool loadFromXML { get; set; }

        private double width;
        public override double Width
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
        public override double Height
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
            get { return background ?? GroupBackground; }
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
                //Increase the Y value by 10. This provides the extra space between
                // a model and textbox. Otherwise there will be some overlap
                Y = InitialTop - ExtendSize - textBlockHeight;
                Height = InitialHeight + textBlockHeight - MinTextHeight;
                UpdateBoundaryFromSelection();
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
        public AnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes)
        {                                 
            var nodeModels = nodes as NodeModel[] ?? nodes.ToArray();           
            var noteModels = notes as NoteModel[] ?? notes.ToArray();
            DeletedModelBases = new List<ModelBase>(); 
            this.SelectedModels = nodeModels.Concat(noteModels.Cast<ModelBase>()).ToList();      
            UpdateBoundaryFromSelection();
        }


        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {                
                case "Position":                  
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
        /// <param name="model"></param>
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
                //Increase the Y value by 10. This provides the extra space between
                // a model and textbox. Otherwise there will be some overlap
                var regionY = groupModels.Min(y => y.Y) - ExtendSize - (TextBlockHeight == 0.0 ? MinTextHeight : TextBlockHeight);
              
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
                                this.Height += overlap.Rect.Bottom - region.Bottom + ExtendSize + ExtendYHeight;
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
                //that is the height should be the initial height without the textblock height.
                if (this.InitialHeight <= 0.0)
                    this.InitialHeight = region.Height;
            }
            else
            {
                this.Width = 0;
                this.height = 0;               
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
            double yheight = ygroup.Last().Height;

            //If the last model is Node, then increase the height so that 
            //node border does not overlap with the group
            if (ygroup.Last() is NodeModel)
                yheight = yheight + ExtendYHeight;

            return Tuple.Create(xgroup.Last().Width, yheight);
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
                if (SelectedModels != null)
                {
                    foreach (var childnode in element.ChildNodes)
                    {
                        var mhelper = new XmlElementHelper(childnode as XmlElement);
                        var result = mhelper.ReadGuid("ModelGuid", new Guid());
                        var model = ModelBaseRequested != null
                            ? ModelBaseRequested(result)
                            : SelectedModels.FirstOrDefault(x => x.GUID == result);

                        if (model != null)
                        {
                            listOfModels.Add(model);
                        }
                    }
                }

                SelectedModels = listOfModels;
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
        /// <param name="checkOverlap"> checkoverlap determines whether the selected model is 
        /// completely inside that group</param>
        internal void AddToSelectedModels(ModelBase model, bool checkOverlap = false)
        {           
            var list = this.SelectedModels.ToList();
            if (list.Where(x => x.GUID == model.GUID).Any()) return;
            if (!CheckModelIsInsideGroup(model, checkOverlap)) return;           
            list.Add(model);
            this.SelectedModels = list;
            this.UpdateBoundaryFromSelection();
        }

        private bool CheckModelIsInsideGroup(ModelBase model, bool checkOverlap)
        {
            if (!checkOverlap) return true;
            var modelRect = model.Rect;
            if (this.Rect.Contains(modelRect))
            {
                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Overriding the Select behavior
        /// because selecting the  group should select the models
        /// within that group
        /// </summary>
        public override void Select()
        {
            foreach (var models in SelectedModels)
            {
                models.IsSelected = true;
            }

            base.Select();
        }

        /// <summary>
        /// Overriding the Deselect behavior
        /// because deselecting the  group should deselect the models
        /// within that group
        /// </summary>
        public override void Deselect()
        {           
            foreach (var models in SelectedModels)
            {
                models.IsSelected = false;
            }   
       
            base.Deselect();
        }

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
            base.Dispose();
        }     
    }   
}
