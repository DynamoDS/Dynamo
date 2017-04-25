using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

using SDateTime = System.DateTime;

namespace CoreNodeModels.Input
{
    /// <summary>
    ///     Date Time Picker node.
    /// </summary>
    [NodeName("DateTime Picker")]
    //[NodeDescription("DateTimeNodeModelDescription", typeof(Resources))]
    [NodeCategory("Core.DateTime")]
    //[NodeSearchTags("DateTimeUISearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortNames("DateTime")]
    [OutPortTypes("DateTime")]
    [OutPortDescriptions("Selected Date and Time.")]
    public class DateTimePalette : NodeModel
    {
        private SDateTime sdateTime = SDateTime.Now;

        /// <summary>
        ///     DateTime value.
        /// </summary>
        public SDateTime SysDateTime
        {
            get { return sdateTime; }
            set
            {
                sdateTime = value;
                RaisePropertyChanged("SysDateTime");
                OnNodeModified();
            }
        }

        /// <summary>
        ///     Node constructor.
        /// </summary>
        public DateTimePalette()
        {
            RegisterAllPorts();
        }

        private SDateTime DeserializeValue(string val)
        {
            try
            {
                SDateTime result;
                return SDateTime.TryParseExact(val, PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ?
                    result : PreferenceSettings.DynamoDefaultTime;
            }
            catch
            {
                return SysDateTime = SDateTime.Now;
            }
        }

        private string SerializeValue()
        {
            return SysDateTime.ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>`
        ///     Store date and time value when graph is saved.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement dateTime = element.OwnerDocument.CreateElement("dateTime");
            dateTime.InnerText = SerializeValue();
            element.AppendChild(dateTime);
        }

        /// <summary>
        ///     Restore stored value and set "scolor" to it.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);

            var dateTimeNode = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "dateTime");

            if (dateTimeNode != null)
            {
                this.sdateTime = DeserializeValue(dateTimeNode.InnerText);
            }
        }

        /// <summary>
        ///     AST Output.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var day = AstFactory.BuildIntNode(SysDateTime.Day);
            var month = AstFactory.BuildIntNode(SysDateTime.Month);
            var year = AstFactory.BuildIntNode(SysDateTime.Year);
            var hour = AstFactory.BuildIntNode(SysDateTime.Hour);
            var minute = AstFactory.BuildIntNode(SysDateTime.Minute);
            var second = AstFactory.BuildIntNode(SysDateTime.Second);
            var millisecond = AstFactory.BuildIntNode(SysDateTime.Millisecond);

            var function = AstFactory.BuildFunctionCall(
                    new Func<int, int, int, int, int, int, int, SDateTime>(DSCore.DateTime.ByDateAndTime), new List<AssociativeNode> { year, month, day, hour, minute, second, millisecond });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), function) };
        }
    }
}
