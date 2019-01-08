using System;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public abstract class ViewModelBase : NotificationObject,IDisposable
    {
        [JsonIgnore]
        public bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Dispose this viewModel, in the case of our ViewModels this usually means
        /// unsubscribing from events which we subscribed to in the construction of
        /// this viewModel. We must unsubscribe to events for which producers live longer
        /// - like DynamoViewModel. In some cases we must also unsubscribe from event
        /// producers like our models, which are held by other references and were not
        /// being garbage collected.
        /// </summary>
        public virtual void Dispose()
        {
           
        }
    }
}
