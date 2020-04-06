using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;
using System.ComponentModel;

namespace Dynamo.Tests.Core
{
    [TestFixture]
    public class NotificationObjectTests : DynamoModelTestBase
    {
        private class NotificationObjectTestingSubclass : NotificationObject
        {
            public bool TestProperty { get; set; }

            public NotificationObjectTestingSubclass()
            {
                PropertyChanged += Test_PropertyChanged;
            }

            
            private void Test_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("TestProperty"))
                {
                    RaisePropertyChanged("TestProperty");

                    var manyProps = new string[] { "TestProperty", "TestProperty" };
                    RaisePropertyChanged(manyProps);
                }
            }
        }

        NotificationObjectTestingSubclass notificationObject;

        [SetUp]
        public void Init()
        {
            notificationObject = new NotificationObjectTestingSubclass();
        }

        [Test]
        [NUnit.Framework.Category("UnitTests")]
        public void RaisePropertyChangedTest()
        {
            notificationObject.TestProperty = !notificationObject.TestProperty;
        }
    }
}