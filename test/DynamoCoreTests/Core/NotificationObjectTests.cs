using System;
using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    /// <summary>
    /// Test class to test the NotificationObject class
    /// </summary>
    [TestFixture]
    public class NotificationObjectTests : DynamoModelTestBase
    {
        private NotificationObjectTestingSubclass notificationObject;

        /// <summary>
        /// Tests initialization
        /// </summary>
        [SetUp]
        public void Init()
        {
            notificationObject = new NotificationObjectTestingSubclass();
            notificationObject.RaisePropertyChangedCalled = false;
        }

        /// <summary>
        /// RaisePropertyChanged method testing
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void RaisePropertyChangedTest()
        {
            notificationObject.TestPropertySingle = !notificationObject.TestPropertySingle;
            Assert.IsTrue(notificationObject.RaisePropertyChangedCalled);
            
            notificationObject.RaisePropertyChangedCalled = false; //Reset flag

            notificationObject.TestPopertyMultiple = !notificationObject.TestPopertyMultiple;
            Assert.IsTrue(notificationObject.RaisePropertyChangedCalled);
            
            notificationObject.RaisePropertyChangedCalled = false; //Reset flag

            //The RaisePropertyChanged method that receives a string[] throws an ArgumentNullException if the array is null;
            Assert.Throws<ArgumentNullException>(() => notificationObject.TestPropertyNull = !notificationObject.TestPropertyNull);
            Assert.IsFalse(notificationObject.RaisePropertyChangedCalled);
        }

        /// <summary>
        /// Derivate class from NotificationObject, designed to help in its testing
        /// </summary>
        private class NotificationObjectTestingSubclass : NotificationObject
        {
            private bool _testProperty;

            /// <summary>
            /// Used to test the RaisePopertyChanged method that receives a single property
            /// </summary>
            internal bool TestPropertySingle
            {
                get { return _testProperty; }
                set
                {
                    _testProperty = value;
                    RaisePropertyChanged("TestProperty");
                }
            }

            /// <summary>
            /// Used to test the RaisePopertyChanged method that receives multiple properties
            /// </summary>
            internal bool TestPopertyMultiple
            {
                get { return _testProperty; }
                set
                {
                    _testProperty = value;

                    string[] props = 
                        {
                            "TestProperty",
                            "TestProperty"
                        };

                    RaisePropertyChanged(props);
                }
            }

            /// <summary>
            /// Used to test the RaisePopertyChanged method that receives multiple properties when the parameter is null
            /// </summary>
            internal bool TestPropertyNull
            {
                get { return _testProperty; }
                set
                {
                    _testProperty = value;

                    string[] props = null;
                    RaisePropertyChanged(props);
                }
            }

            /// <summary>
            /// Flag property to check whether the RaisePropertyChanged event was triggered or not
            /// </summary>
            internal bool RaisePropertyChangedCalled { get; set; }

            internal NotificationObjectTestingSubclass()
            {
                PropertyChanged += Test_RaisePropertyChanged;
                RaisePropertyChangedCalled = false;
            }

            private void Test_RaisePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                RaisePropertyChangedCalled = true;
            }
        }
    }
}