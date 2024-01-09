using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    internal class TestUtilities
    {
        private static ConcurrentDictionary<string, int> WebView2Counter = new ConcurrentDictionary<string, int>();
        internal static void IncrementWebView2(string containingType)
        {
            WebView2Counter[containingType]++;
        }

        internal static void DecrementWebView2(string containingType)
        {
            WebView2Counter[containingType]--;
        }

        internal static void AssertCounters()
        {
            var exceptions = new List<Exception>();
            foreach (var counter in WebView2Counter)
            {
                if (counter.Value != 0)
                {
                    exceptions.Add(new Exception($"Unexpected number of webview2 allocations/deallocations: {counter.Value} webview2 instances for containing class {counter.Key}"));
                }
            }
            if( exceptions.Count > 0 ) { throw new AggregateException(exceptions.ToArray()); }
        }
    }
}
