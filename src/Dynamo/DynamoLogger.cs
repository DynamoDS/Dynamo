using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Nodes;

namespace Dynamo
{
    public class DynamoLogger
    {
        private static DynamoLogger instance;

        public TextWriter Writer { get; set; }

        /// <summary>
        /// The singelton instance.
        /// </summary>
        public static DynamoLogger Instance
        {
            get
            {
                if(instance == null)
                    instance = new DynamoLogger();
                return instance;
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        private DynamoLogger()
        {
            
        }

        /// <summary>
        /// Log the time and the supplied message.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            if(Writer!=null)
                Writer.WriteLine(string.Format("{0} : {1}", DateTime.Now, message));
        }

        public void Log(dynNodeModel node)
        {
            string exp = node.PrintExpression();
            Log("> " + exp);
        }

        public void Log(FScheme.Expression expression)
        {
            Log(FScheme.printExpression("\t", expression));
        }

        /// <summary>
        /// Begin logging.
        /// </summary>
        public void StartLogging()
        {
            //create log files in a directory 
            //with the executing assembly
            string log_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dynamo_logs");
            if (!Directory.Exists(log_dir))
            {
                Directory.CreateDirectory(log_dir);
            }

            string logPath = Path.Combine(log_dir, string.Format("dynamoLog_{0}.txt", Guid.NewGuid().ToString()));

            Writer = new StreamWriter(logPath);
            Writer.WriteLine("Dynamo log started " + DateTime.Now.ToString());
        }

        /// <summary>
        /// Finish logging.
        /// </summary>
        public void FinishLogging()
        {
            if (Writer != null)
            {
                Writer.WriteLine("Goodbye.");
                Writer.Close();
            }
        }
    }
}
