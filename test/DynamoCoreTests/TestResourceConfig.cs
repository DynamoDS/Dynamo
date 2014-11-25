using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Dynamo
{
    public class TestResourceConfig
    {
        private const string CORE_TEST_PATH = @"C:\Users\t_anhp\Documents\GitHub\Dynamo\test\";
        private const string REVIT_TEST_PATH=@"C:\Program Files\Dynamo Test 0.7\test\System\revit\";
        private const string SAMPLES_PATH=@"C:\Users\t_anhp\Documents\GitHub\Dynamo\doc\distrib\Samples\";
 
        public string DynamoCoreTestPath{get;set;}
        public string RevitTestPath{get; set;}
        public string SamplePath{get;set;}

        public TestResourceConfig() {
            DynamoCoreTestPath = CORE_TEST_PATH;
            RevitTestPath = REVIT_TEST_PATH;
            SamplePath = SAMPLES_PATH;
        }

        public static TestResourceConfig Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(TestResourceConfig));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return serializer.Deserialize(fs) as TestResourceConfig;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void Save(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(TestResourceConfig));
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static TestResourceConfig GetSettings()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string filePath = Path.Combine(Path.GetDirectoryName(location), "TestResourceConfig.xml");

            //This code is just to create the default config file to
            //save the default settings, which later on can be modified
            //to re-direct it to other download target for testing.
            if (!File.Exists(filePath))
            {
                var config = new TestResourceConfig();
                config.Save(filePath);
            }

            return File.Exists(filePath) ? Load(filePath) : new TestResourceConfig();
        }
    }
}
