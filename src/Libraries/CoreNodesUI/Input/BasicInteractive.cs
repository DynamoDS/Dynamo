using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class BasicInteractive<T> : NodeModel, IWpfNode
    {
        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (Equals(_value, null) || !_value.Equals(value))
                {
                    _value = value;
                    RequiresRecalc = !Equals(value, null);
                    RaisePropertyChanged("Value");
                }
            }
        }

        // Making these abstract so that derived classes are forced to come up 
        // with their implementations rather than default silently taking over.
        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue();

        protected BasicInteractive(WorkspaceModel workspace)
            : base(workspace)
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name));
            RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.InnerText = SerializeValue();
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (
                XmlNode subNode in
                    nodeElement.ChildNodes.Cast<XmlNode>()
                               .Where(subNode => subNode.Name.Equals(typeof(T).FullName)))
            {
// ReSharper disable once PossibleNullReferenceException
                Value = DeserializeValue(subNode.InnerText);
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        public virtual void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add an edit window option to the 
            //main context window
            var editWindowItem = new MenuItem
            {
                Header = "Edit...",
                IsCheckable = false,
                Tag = nodeUI.ViewModel.DynamoViewModel
            };

            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called

            if (context == SaveContext.Undo)
            {
                var xmlDocument = element.OwnerDocument;
                var subNode = xmlDocument.CreateElement(typeof(T).FullName);
                subNode.InnerText = SerializeValue();
                element.AppendChild(subNode);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); // Base implementation must be called

            if (context == SaveContext.Undo)
            {
                foreach (XmlNode subNode in element.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name.Equals(typeof(T).FullName)))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Value = DeserializeValue(subNode.InnerText);
                }
            }
        }

        #endregion
    }

    public abstract class Double : BasicInteractive<double>
    {
        protected Double(WorkspaceModel workspace) : base(workspace) { }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildDoubleNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public abstract class Integer : BasicInteractive<int>
    {
        protected Integer(WorkspaceModel workspace) : base(workspace) { }

        protected override int DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class Bool : BasicInteractive<bool>
    {
        protected Bool(WorkspaceModel workspace) : base(workspace) { }

        protected override bool DeserializeValue(string val)
        {
            try
            {
                return val.ToLower().Equals("true");
            }
            catch
            {
                return false;
            }
        }

        protected override string SerializeValue()
        {
            return this.Value.ToString();
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                Value = DeserializeValue(value);
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildBooleanNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class String : BasicInteractive<string>
    {
        protected String(WorkspaceModel workspace) : base(workspace) { }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = GetDynamoViewModelFromMenuItem(sender as MenuItem);
            var editWindow = new EditWindow(viewModel) { DataContext = this };
            editWindow.BindToProperty(
                null,
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay(),
                    NotifyOnValidationError = false,
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            editWindow.ShowDialog();
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                var converter = new StringDisplay();
                Value = converter.ConvertBack(value, typeof(string), null, null) as string;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildStringNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        protected override string SerializeValue()
        {
            return this.Value;
        }
    }
}
