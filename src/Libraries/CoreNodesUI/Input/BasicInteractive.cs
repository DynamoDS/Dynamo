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
using Dynamo.Utilities;
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

        protected abstract T DeserializeValue(string val);

        protected virtual string SerializeValue()
        {
            return Value.ToString();
        }

        protected BasicInteractive()
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name));
            RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", SerializeValue());
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
                Value = DeserializeValue(subNode.Attributes[0].Value);
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
            var editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }
    }

    public abstract class Double : BasicInteractive<double>
    {
        public override bool IsConvertible
        {
            get { return true; }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("doubleValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Value = helper.ReadDouble("doubleValue");
            }
        }

        #endregion

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
        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("integerValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Value = helper.ReadInteger("integerValue");
            }
        }

        #endregion

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
        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("boolValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Value = helper.ReadBoolean("boolValue");
            }
        }

        #endregion

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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildBooleanNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class String : BasicInteractive<string>
    {
        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("stringValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                Value = helper.ReadString("stringValue");
            }
        }

        #endregion

        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow { DataContext = this };
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
    }
}
