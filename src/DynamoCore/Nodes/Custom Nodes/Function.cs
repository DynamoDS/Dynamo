using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchable(false)]
    [IsMetaNode]
    public class Function 
        : FunctionCallBase<CustomNodeController<CustomNodeDefinition>, CustomNodeDefinition>
    {
        public Function(
            CustomNodeDefinition def, string nickName, string description, string category)
            : base(new CustomNodeController<CustomNodeDefinition>(def))
        {
            ArgumentLacing = LacingStrategy.Disabled;
            NickName = nickName;
            Description = description;
            Category = category;
        }

        public CustomNodeDefinition Definition { get { return Controller.Definition; } }
        
        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called

            Controller.SerializeCore(element, context);

            var xmlDoc = element.OwnerDocument;

            var outEl = xmlDoc.CreateElement("Name");
            outEl.SetAttribute("value", NickName);
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Description");
            outEl.SetAttribute("value", Description);
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (string input in InPortData.Select(x => x.NickName))
            {
                XmlElement inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (string output in OutPortData.Select(x => x.NickName))
            {
                XmlElement outputEl = xmlDoc.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outEl.AppendChild(outputEl);
            }
            element.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); //Base implementation must be called

            List<XmlNode> childNodes = nodeElement.ChildNodes.Cast<XmlNode>().ToList();

            XmlNode nameNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Name"));
            if (nameNode != null && nameNode.Attributes != null)
                NickName = nameNode.Attributes["value"].Value;

            XmlNode descNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Description"));
            if (descNode != null && descNode.Attributes != null)
                Description = descNode.Attributes["value"].Value;

            if (!Controller.IsInSyncWithNode(this))
            {
                Controller.SyncNodeWithDefinition(this);
                OnAstUpdated();
            }
            else
            {
                foreach (XmlNode subNode in childNodes)
                {
                    if (subNode.Name.Equals("Outputs"))
                    {
                        var data =
                            subNode.ChildNodes.Cast<XmlNode>()
                                   .Select(
                                       (outputNode, i) =>
                                           new
                                           {
                                               data = new PortData(outputNode.Attributes[0].Value, "Output #" + (i + 1)),
                                               idx = i
                                           });

                        foreach (var dataAndIdx in data)
                        {
                            if (OutPortData.Count > dataAndIdx.idx)
                                OutPortData[dataAndIdx.idx] = dataAndIdx.data;
                            else
                                OutPortData.Add(dataAndIdx.data);
                        }
                    }
                    else if (subNode.Name.Equals("Inputs"))
                    {
                        var data =
                            subNode.ChildNodes.Cast<XmlNode>()
                                   .Select(
                                       (inputNode, i) =>
                                           new
                                           {
                                               data = new PortData(inputNode.Attributes[0].Value, "Input #" + (i + 1)),
                                               idx = i
                                           });

                        foreach (var dataAndIdx in data)
                        {
                            if (InPortData.Count > dataAndIdx.idx)
                                InPortData[dataAndIdx.idx] = dataAndIdx.data;
                            else
                                InPortData.Add(dataAndIdx.data);
                        }
                    }

                    #region Legacy output support

                    else if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(subNode.Attributes[0].Value, "function output");

                        if (OutPortData.Any())
                            OutPortData[0] = data;
                        else
                            OutPortData.Add(data);
                    }

                    #endregion
                }

                RegisterAllPorts();
            }

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;
        }

        #endregion

        public void ResyncWithDefinition(CustomNodeDefinition def)
        {
            Controller.Definition = def;
            Controller.SyncNodeWithDefinition(this);
        }
    }

    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function parameter, use with custom nodes")]
    [NodeSearchTags("variable", "argument", "parameter")]
    [IsInteractive(false)]
    [NotSearchableInHomeWorkspace]
    [IsDesignScriptCompatible]
    public class Symbol : NodeModel
    {
        private string inputSymbol = "";

        public Symbol()
        {
            OutPortData.Add(new PortData("", "Symbol"));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public string InputSymbol
        {
            get { return inputSymbol; }
            set
            {
                inputSymbol = value;
                OnAstUpdated();
                RaisePropertyChanged("InputSymbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return
                AstFactory.BuildIdentifier(
                    InputSymbol == null ? AstIdentifierBase : InputSymbol + "__" + AstIdentifierBase);
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (var subNode in
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                InputSymbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            if (name == "InputSymbol")
            {
                InputSymbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value, recorder);
        }
    }

    [NodeName("Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function output, use with custom nodes")]
    [IsInteractive(false)]
    [NotSearchableInHomeWorkspace]
    [IsDesignScriptCompatible]
    public class Output : NodeModel
    {
        private string symbol = "";

        public Output()
        {
            InPortData.Add(new PortData("", ""));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                OnAstUpdated();
                RaisePropertyChanged("Symbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > OutPortData.Count)
                throw new ArgumentOutOfRangeException("outputIndex", @"Index must correspond to an OutPortData index.");

            return AstIdentifierForPreview;
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode assignment;
            if (null == inputAstNodes || inputAstNodes.Count == 0)
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, AstFactory.BuildNullNode());
            else
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0]);

            return new[] { assignment };
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (var subNode in 
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                Symbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            if (name == "Symbol")
            {
                Symbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value, recorder);
        }
    }
}
