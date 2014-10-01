using System.Collections.Generic;
using System.Xml;

using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Nodes;

using DynamoCoreUITests;

using Microsoft.Practices.Prism;

using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class DropDownTests : DynamoTestUIBase
    {
        [Test]
        public void Save_SelectedIndex()
        {
            //var node = new TestDropDown(ViewModel.CurrentSpace, "test");

            //node.SelectedIndex = 2;

            //var xmlDoc = new XmlDocument();
            //var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
            //xmlDoc.AppendChild(dynEl);
            //node.Save(xmlDoc, dynEl, SaveContext.File);

        }

        [Test]
        public void Save_SelectedIndexAndName()
        {
            //var node = new TestDropDown(ViewModel.CurrentSpace, "test");

            //node.SelectedIndex = 2;

            //var xmlDoc = new XmlDocument();
            //var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
            //        xmlDoc.AppendChild(dynEl);
            //        node.Save(xmlDoc, dynEl, SaveContext.File);

        }

        [Test]
        public void Load_SemiColons()
        {
            
        }

        [Test]
        public void Load_SpecialCharacters()
        {
            
        }


    }
}
