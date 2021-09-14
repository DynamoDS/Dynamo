using Dynamo.Logging;
using Microsoft.Win32;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class LogTest : DynamoModelTestBase
    {
        public string UserId = GetUserID();
        public string SessionId = Guid.NewGuid().ToString();
        StringBuilder largeAppName = new StringBuilder("AppName");
        string specialCharsStr;
        private const int TEST_MAX_DATA_LENGTH = 500000;
        private const int TEST_MAX_NAME_LENGTH = 256;

        [SetUp]
        public void SetUp()
        {
            //Create largeAppName string that will exceed 256 chars
            largeAppName.Clear();
            for (int i = 0; i < 258; i++)
            {
                largeAppName.Append(i.ToString());
            }

            
            specialCharsStr = @"asdfas$$%&%$%!!------;:[]*¨¨";
        }

        /// <summary>
        /// This test method will validate all the exceptions generated when assigning values to properties
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_properties()
        {
            string AppNameValue = string.Empty;
            var logger = new UsageLog("Dynamo", UserId, SessionId);
            
            //Act
            //Get of the AppName property
            logger.AppName = "DynamoTest";
            AppNameValue = logger.AppName;  //Execute the Get of the AppName property         

            //Assert
            //AppName - The exception for AppName property is executed when has special chars
            Assert.Throws<ArgumentException>(() => logger.AppName = specialCharsStr);
            //AppName - The exception for AppName property is executed when exceed 256 chars
            Assert.Throws<ArgumentException>(() => logger.AppName = largeAppName.ToString());

            //SessionID - The exception for SessionID property is executed when has special chars
            Assert.Throws<ArgumentException>(() => logger.SessionID = specialCharsStr);
            //SessionID -The exception for SessionID property is executed when exceed 256 chars
            Assert.Throws<ArgumentException>(() => logger.SessionID = largeAppName.ToString());

            //UserID - - The exception for UserID property is executed when has special chars
            Assert.Throws<ArgumentException>(() => logger.UserID = specialCharsStr);
            //UserID -The exception for UserID property is executed when exceed 256 chars
            Assert.Throws<ArgumentException>(() => logger.UserID = largeAppName.ToString());

            //Validate UploadedItems.GET
            logger.Dispose();
            Assert.AreEqual(logger.UploadedItems, 0);


        }

        /// <summary>
        /// This test method will validate the exception executed in the UsageLog constructor
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_exception()
        {
            //Act
            //It will generate the exception related to large AppName and by consequence we reach the expection section in UsageLog constructor
            var logger = new UsageLog(largeAppName.ToString(), UserId, SessionId);

            //Assert
            logger.Dispose();
            //This asserts just check that the properties were released by the Garbage Collector correctly
            Assert.IsNull(logger.AppName);
            Assert.IsNull(logger.UserID);
            Assert.IsNull(logger.SessionID);
        }

        /// <summary>
        /// This test method will check several log methods inside UsageLog class (Debug, Error, Log and Verbose)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_log_methods()
        {
            //Arrange
            //It will generate the exception related to large AppName and by consequence we reach the expection section in UsageLog constructor
            var logger = new UsageLog("AppName1", UserId, SessionId);

            //Act
            logger.Debug("notificationD", "Debug Message");
            logger.Error("notificationE", "Error Message");
            logger.Log("notificationL", "Log Message");
            logger.Verbose("notificationV", "Verbose Message");

            //Using reflection we get the private field items inside UsageLog class
            FieldInfo varField = typeof(UsageLog).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemsList = (Queue<Dictionary<string, string>>)varField.GetValue(logger);

            //Assert
            //We inserted the Debug, Error, Log and Verbose so we should have only four elements in the queue
            //Because when using the UploaderExec method is using threads we can have 3 or 4 elements in the list.
            logger.Dispose();
            Assert.IsNotNull(itemsList); //Check that itemsList is not null
            Assert.IsTrue(itemsList.Any());//Check that itemsList has at least one item
            Assert.LessOrEqual(itemsList.Count, 4);//Check that itemsList has less than 4 or equals to 4

        }

        /// <summary>
        /// This test method will validate that the UsageLog.PrepAndPushItem is executed and also the Get in the items property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_prepandpushitem()
        {
            //Arrange
            //It will generate the exception related to large AppName and by consequence we reach the expection section in UsageLog constructor
            var logger = new UsageLog("AppName1", UserId, SessionId);

            //Using reflection we set to true the private property EnableDiagnosticsOutput
            PropertyInfo propertyInfo = typeof(UsageLog).GetProperty("EnableDiagnosticsOutput", BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo.SetValue(logger, true, null);

            //Act
            largeAppName.Clear();
            for (int i = 0; i < TEST_MAX_DATA_LENGTH + 1; i++)
            {
                largeAppName.Append(i.ToString());
            }
            logger.Debug("notificationD", largeAppName.ToString());

            //Using reflection we get the private field items inside UsageLog class
            FieldInfo varField = typeof(UsageLog).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemsList = (Queue<Dictionary<string, string>>)varField.GetValue(logger);

            //Assert
            //We inserted the Debug
            //Because when using the UploaderExec method is using threads we can have 3 or 4 elements in the list.
            logger.Dispose();
            Assert.IsNotNull(itemsList); //Check that itemsList is not null
            Assert.IsTrue(itemsList.Any());//Check that itemsList has at least one item
            Assert.IsTrue(itemsList.FirstOrDefault().FirstOrDefault().Value.StartsWith("notificationD"));//Check that the value contains the value logged in the Debug method

        }

        /// <summary>
        /// This test method will check the Set of the EnableDiagnosticsOutput property
        /// Also will check the MAX_ITEMS validation inside the private UsageLog.PushItem method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_push_item()
        {
            //Arrange
            //It will generate the exception related to large AppName and by consequence we reach the expection section in UsageLog constructor
            var logger = new UsageLog("AppName1", UserId, SessionId);

            //Using reflection we set to true the private property EnableDiagnosticsOutput
            PropertyInfo propertyInfo = typeof(UsageLog).GetProperty("EnableDiagnosticsOutput", BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo.SetValue(logger, true, null);

            StringBuilder debugMessage = new StringBuilder("Debug Message ");

            //Act
            for (int i = 0; i < TEST_MAX_DATA_LENGTH + 1; i++)
            {
                debugMessage.Append(i.ToString());
            }
            logger.Debug("notificationD", debugMessage.ToString());


            //Using reflection we get the private field items inside UsageLog class
            FieldInfo varField = typeof(UsageLog).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);

            Queue<Dictionary<string, string>> itemsLarge = new Queue<Dictionary<string, string>>();
            for (int i = 0; i < TEST_MAX_DATA_LENGTH + 1; i++)
            {
                var item = new Dictionary<string, string>
                {
                    { "Tag", i.ToString() },
                    { "Priority", "PR"},
                    { "AppIdent", "AN" },
                    { "UserID", "UserID" },
                    { "SessionID", "Session" },
                    { "DateTime", DateTime.Now.ToString() },
                    { "MicroTime", "MT" },
                    { "Data", "Test"+i.ToString() }
                };

                itemsLarge.Enqueue(item);
            }
            //This will set the items field with 5000000 items 
            varField.SetValue(logger,itemsLarge);


            var itemFinal = new Dictionary<string, string>
                {
                    { "Tag", "Test1" },
                    { "Priority", "PR"},
                    { "AppIdent", "AN" },
                    { "UserID", "UserID" },
                    { "SessionID", "Session" },
                    { "DateTime", DateTime.Now.ToString() },
                    { "MicroTime", "MT" },
                    { "Data", "Test1" }
                };

            //Using reflection we execute the PushItem method, the second parameter has to be a Dictionary<string, string>
            //This will rearch the validation inside PushItem about MAX_ITEMS
            MethodInfo dynMethod = typeof(UsageLog).GetMethod("PushItem", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(logger, new object[] { itemFinal });

            var itemsList = (Queue<Dictionary<string, string>>)varField.GetValue(logger);

            //Assert
            //We inserted the Debug, Error, Log and Verbose so we should have only four elements in the queue
            //Because when using the UploaderExec method is using threads we can have 3 or 4 elements in the list.
            logger.Dispose();
            Assert.IsNotNull(itemsList); //Check that itemsList is not null
            Assert.IsTrue(itemsList.Any());//Check that itemsList has at least one item
            Assert.AreEqual(itemsList.Count, TEST_MAX_DATA_LENGTH + 1);//Check that itemsList has 500 000 + 1 items
        }

        /// <summary>
        /// This test method will validate the exception section inside several log method in UsageLog class (Debug, Error, Log, Verbose)
        /// All are implemented inside a try - catch because the exception is not re-thrown so is not posible to catch it inside this method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_validate_inputs_exceptions()
        {
            //Arrange
            var logger = new UsageLog("AppName1", UserId, SessionId);

            //Act
            //Create a large string so the validations will fail when calling the logger functions
            largeAppName.Clear();
            for (int i = 0; i < TEST_MAX_DATA_LENGTH + 1; i++)
            {
                largeAppName.Append(i.ToString());
            }
            try
            {
                //Exception when the first parameter of Debug method is null
                logger.Debug(null, "Debug Message");                                          
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Tag must not be null"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            try
            {
                //Exception when the second parameter of Error method is null
                logger.Error("notificationE", null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Text must not be null"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            try
            {
                //Exception when the second parameter of Log method is null
                logger.Log(largeAppName.ToString(), "Log Message");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Tag must be 256 chars or less"));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            try
            {
                //Exception when the second parameter of Verbose method has special characters 
                logger.Verbose(specialCharsStr, "Verbose Message");
            }
            catch(ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Tag must only be letters, numbers or '-', '.'"));
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                logger.Dispose();
            }
        }

        /// <summary>
        /// This test method will check all the exceptions thrown by several methods inside the UsageLog class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_log()
        {
            //Arrange
            var logger = new UsageLog("AppName1", UserId, SessionId);

            //Assert/Act 
            //The next methods only throw the NotImplementedException
            Assert.Throws<NotImplementedException>(() => logger.Log("Exception"));
            Assert.Throws<NotImplementedException>(() => logger.LogError("Exception"));
            Assert.Throws<NotImplementedException>(() => logger.LogWarning("Exception", WarningLevel.Error));
            Assert.Throws<NotImplementedException>(() => logger.Log(new Exception("Test Exception")));

            logger.Dispose();
        }

        /// <summary>
        /// This test method will check the ValidateLength method when null is passed as parameter
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_validate_input_null()
        {
            //Arrange
            var logger = new UsageLog("AppName1", UserId, SessionId);
            bool validLenght = true;

            //Act
            //Using reflection we execute the ValidateLength method with null in the second parameter
            MethodInfo dynMethod = typeof(UsageLog).GetMethod("ValidateLength", BindingFlags.NonPublic | BindingFlags.Instance);
            validLenght = (bool)dynMethod.Invoke(logger, new object[] { null });

            //Assert
            //The ValidateLength returns false if the parameter passed is false 
            logger.Dispose();
            Assert.IsFalse(validLenght); 
        }

        /// <summary>
        /// This test method will check the UploadItem function when the EnableDiagnosticsOutput flag is true
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_usagelog_upload_item_exception()
        {
            //Arrange
            var logger = new UsageLog("AppName1", UserId, SessionId);
            bool uploadedItem = true;
            Dictionary<String, String> item = null;

            //Act
            //set the EnableDiagnosticsOutput=true
            PropertyInfo propertyInfo = typeof(UsageLog).GetProperty("EnableDiagnosticsOutput", BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo.SetValue(logger, true, null);

            //Using reflection we execute the ValidateLength method with null in the second parameter
            MethodInfo dynMethod = typeof(UsageLog).GetMethod("UploadItem", BindingFlags.NonPublic | BindingFlags.Instance);
            uploadedItem = (bool)dynMethod.Invoke(logger, new object[] { item });

            //Assert
            //The UploadItem returns false if the parameter passed is false 
            logger.Dispose();
            Assert.IsFalse(uploadedItem);
        }

        public static String GetUserID()
        {
            // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\DynamoUXG";
            const string keyName = userRoot + "\\" + subkey;

            // An int value can be stored without specifying the
            // registry data type, but long values will be stored
            // as strings unless you specify the type. Note that
            // the int is stored in the default name/value
            // pair.

            var tryGetValue = Registry.GetValue(keyName, "InstrumentationGUID", null) as string;

            if (tryGetValue != null)
            {
                Debug.WriteLine("User id found: " + tryGetValue);
                return tryGetValue;
            }

            String newGUID = Guid.NewGuid().ToString();
            Registry.SetValue(keyName, "InstrumentationGUID", newGUID);
            Debug.WriteLine("New User id: " + newGUID);
            return newGUID;
        }
    }
}
