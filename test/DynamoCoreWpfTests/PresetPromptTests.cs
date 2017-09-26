using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

using SystemTestServices;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture,Ignore]
    public class PresetPromptTests
    {
        [Test]
        public void TestCharacterLength()
        {
           //create a preset prompt
           var prompt = new PresetPrompt();
           
            //check for the max length
           Assert.AreEqual(prompt.MaxLength , 50);

           //Enter the text in the textbox - more than 50 characters
           prompt.Text = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaqqq";

           //check for the characters in the textbox - characters after the max length
           //should get cut off.
           Assert.AreEqual(prompt.Text,
               "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        }
    }
}
