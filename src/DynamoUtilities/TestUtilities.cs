using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    internal class TestUtilities
    {
        internal static bool RunningFromNUnit = false;
        static TestUtilities()
        {
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Can't do something like this as it will load the nUnit assembly
                // if (assem == typeof(NUnit.Framework.Assert))

                if (assem.FullName.ToLowerInvariant().StartsWith("nunit.framework"))
                {
                    RunningFromNUnit = true;
                    break;
                }
            }
        }

        private static ConcurrentDictionary<string, int> WebView2Counter = new ConcurrentDictionary<string, int>();
        internal static void IncrementWebView2(string containingType)
        {
            if (!WebView2Counter.TryGetValue(containingType, out _))
            {
                WebView2Counter.TryAdd(containingType, 0);
            }
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
            WebView2Counter.Clear();
            if ( exceptions.Count > 0 ) { throw new AggregateException(exceptions.ToArray()); }
        }
    }
}
