using Dynamo;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace DynamoCoreWpfTests
{
    class InCanvasSearchTests
    {
        List<NodeSearchElementViewModel> members = new List<NodeSearchElementViewModel>();

        [SetUp]
        public void SetUpViewModel()
        {
            members.Add(CreateCustomNodeViewModel("A", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("B", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("C", "TopCategory"));
            members.Add(CreateCustomNodeViewModel("D", "TopCategory"));
        }

        [Test]
        [Category("UnitTests")]
        public void MoveToNextMemberTest()
        {
            var incanvasSearch = new Dynamo.UI.Controls.InCanvasSearchControl();

            var next_index = incanvasSearch.MoveToNextMember(true, members, members.ElementAt(0));
            Assert.AreEqual(1, next_index);
            Assert.AreEqual("B", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(true, members, members.ElementAt(1));
            Assert.AreEqual(2, next_index);
            Assert.AreEqual("C", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(false, members, members.ElementAt(2));
            Assert.AreEqual(1, next_index);
            Assert.AreEqual("B", members[next_index].Name);

            next_index = incanvasSearch.MoveToNextMember(false, members, members.ElementAt(1));
            Assert.AreEqual(0, next_index);
            Assert.AreEqual("A", members[next_index].Name);
        }

        #region Helpers

        private static NodeSearchElement CreateCustomNode(string name, string category,
            string description = "", string path = "")
        {
            var element = new CustomNodeSearchElement(null,
                new CustomNodeInfo(Guid.NewGuid(), name, category, description, path));

            return element;
        }

        private static NodeSearchElementViewModel CreateCustomNodeViewModel(string name, string category,
            string description = "", string path = "")
        {
            var element = CreateCustomNode(name, category, description, path);
            return new NodeSearchElementViewModel(element, null);
        }

        #endregion
    }
}
