using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Wpf.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Dynamo.Wpf.Utilities.JobDebouncer;

namespace Dynamo.LibraryViewExtensionWebView2
{

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptingObject: IDisposable
    {
        private LibraryViewController controller;

        public ScriptingObject(LibraryViewController controller)
        {
            this.controller = controller;
        }

        public void Dispose()
        {
            controller = null;
        }

        /// <summary>
        /// Used to get access to the iconResourceProvider and return a base64encoded string version of an icon.
        /// </summary>
        /// <param name="iconurl"></param>
        public string GetBase64StringFromPath(string iconurl)
        {

            string ext;
            var iconAsBase64 = controller.iconProvider.GetResourceAsString(iconurl, out ext);
            if (string.IsNullOrEmpty(iconAsBase64))
            {
                return string.Empty;
            }
            if (ext.Contains("svg"))
            {
                ext = "svg+xml";
            }
            //send back result.
            return $"data:image/{ext};base64, {iconAsBase64}";
        }

        private static readonly JobDebouncer.DebounceQueueToken DebounceQueueToken = new();

        /// <summary>
        /// This method will receive any message sent from javascript and execute a specific code according to the message
        /// </summary>
        /// <param name="dataFromjs"></param>
        internal void Notify(string dataFromjs)
        {
            if (string.IsNullOrEmpty(dataFromjs))
            {
                return;
            }

            try
            {
                //a simple refresh of the libary is requested from js context.
                if (dataFromjs == "RefreshLibrary")
                {
                    controller.RefreshLibraryView(controller.browser);
                    return;
                }
                //a more complex action needs to be taken on the c# side.
                /*dataFromjs will be an object like:

                {func:funcName,
                data:"string" | data:object[] | bool}
                 */

                var simpleRPCPayload = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataFromjs);
                var funcName = simpleRPCPayload["func"] as string;
                if (funcName == "createNode")
                {
                    var data = simpleRPCPayload["data"] as string;
                    controller.CreateNode(data);
                    controller.CloseNodeTooltip(true);
                }
                else if (funcName == "showNodeTooltip")
                {
                    var data = (simpleRPCPayload["data"] as JArray).Children();
                    controller.ShowNodeTooltip(data.ElementAt(0).Value<string>(), data.ElementAt(1).Value<double>());
                }
                else if (funcName == "closeNodeTooltip")
                {
                    var data = (bool)simpleRPCPayload["data"];
                    controller.CloseNodeTooltip(data);
                }
                else if (funcName == "importLibrary")
                {
                    controller.ImportLibrary();
                }
                else if (funcName == "performSearch")
                {
                    var data = simpleRPCPayload["data"] as string;
                    var extension = string.Empty;

                    var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
                    JobDebouncer.EnqueueOptionalJobAsync(() => {
                        var searchStream = controller.searchResultDataProvider.GetResource(data, out extension);
                        var searchReader = new StreamReader(searchStream);
                        var results = searchReader.ReadToEnd();
                        dispatcher.Invoke(() =>
                        {
                            //send back results to librarie.js
                            LibraryViewController.ExecuteScriptFunctionAsync(controller.browser, "completeSearch", results);
                            searchReader.Dispose();
                        });
                    }, DebounceQueueToken);
                }
                //When the html <div> that contains the sample package is clicked then we will be moved to the next Step in the Guide
                else if (funcName == "NextStep")
                {
                    controller.MoveToNextStep();
                }
                else if (funcName == "ResizedEvent")
                {
                    controller.UpdatePopupLocation();
                }
            }
            catch (Exception e)
            {
                this.controller.LogToDynamoConsole($"Error while parsing command data from javascript{Environment.NewLine}{e.Message}");
            }
        }
    }
}
