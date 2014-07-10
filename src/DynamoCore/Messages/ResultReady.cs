using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Messages
{
    public delegate void ResultReadyEventHandler(object sender, ResultReadyEventArgs e);

    public class ResultReadyEventArgs : EventArgs
    {
        public ResultReadyEventArgs(string message) 
        {
            this.Message = message;
        }

        public string Message { get; private set; }
        public string SessionID { get; set; }
    }
}
