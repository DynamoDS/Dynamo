using HarmonyLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dynamo.Utilities
{
    internal class NotificationLog
    {
        internal SortedSet<int> objects = new SortedSet<int>();
        internal int counter;
    }

    internal static class NotificationCounter
    {
        private static Harmony harmonyIntance;
        private static bool patchedCoreNotifications;
        private static bool patchUINotifications;

        private static double timerInterval = 5000;//5 second;
        private static Timer timer;
        private static string logFile = "";
        private static Dictionary<string, NotificationLog> notifications = new Dictionary<string, NotificationLog>();

        /// <summary>
        /// Tries to initialize the NotificationCounter class.
        /// </summary>
        /// <returns>true if debug mode 'NotificationCounter' is turned on and if Harmony.dll was successfully loaded. Returns false otherwise</returns>
        internal static void StartCounting()
        {
            try
            {
                lock (logFile)
                {
                    logFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "notification_counter.log");
                    if (File.Exists(logFile))
                    {
                        File.Delete(logFile);
                    }
                }

                timer = new Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = timerInterval;
                timer.Start();
                
                harmonyIntance = new Harmony("NotificationCounter");

                AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler((object sender, AssemblyLoadEventArgs args) => PatchFromAssmebly(args.LoadedAssembly));

                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                PatchFromAssmebly(loadedAssemblies.FirstOrDefault(x => x.GetName().Name == "DynamoCore"));
                PatchFromAssmebly(loadedAssemblies.FirstOrDefault(x => x.GetName().Name == "Microsoft.Practices.Prism"));
                PatchObservableCollection();
            }
            catch { }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (notifications.Count == 0) return;

                bool freshLog = !File.Exists(logFile);
                Dictionary<string, (int counter, int objects)> localNotifications = new Dictionary<string, (int counter, int objects)>();
                
                lock (notifications)
                {
                    foreach (var item in notifications)
                        localNotifications.Add(item.Key, (item.Value.counter, item.Value.objects.Count));

                    notifications.Clear();
                }

                lock (logFile)
                {
                    if (File.Exists(logFile))
                    {
                        var oldContent = File.ReadAllLines(logFile);
                        mergeLogContents(localNotifications, oldContent);
                    }
                }

                var local2 = localNotifications.Select(x => (Property: x.Key, Counter: x.Value.counter, Objects: x.Value.objects)).ToList();
                local2.Sort((x, y) => y.Counter.CompareTo(x.Counter));

                string[] contents = local2.Select(x => $"{x.Counter}-{x.Property}-{x.Objects}").ToArray();
                lock (logFile)
                {
                    File.WriteAllLines(logFile, contents);
                }
            }
            catch { }
        }

        private static void mergeLogContents(Dictionary<string, (int counter, int objects)> current, string[] oldContent)
        {
            foreach (string line in oldContent)
            {
                var records = line.Split('-');

                string key = records[1];
                int counter = int.Parse(records[0]);
                int objects = int.Parse(records[2]);
                if (!current.ContainsKey(key))
                {
                    current.Add(key, (counter, objects));
                }
                current[key] = (counter + current[key].counter, objects + current[key].objects);
            }
        }

        private static void PatchFromAssmebly(Assembly assembly)
        {
            if (assembly == null) return;

            try
            {
                var name = assembly.GetName().Name;
                if (name == "DynamoCore" && !patchedCoreNotifications)
                {
                    patchedCoreNotifications = true;
                    PatchCoreNotifications(assembly.GetType("Dynamo.Core.NotificationObject"));
                }
                if (name == "Microsoft.Practices.Prism" && !patchUINotifications)
                {
                    patchUINotifications = true;
                    PatchUINotifications(assembly.GetType("Microsoft.Practices.Prism.ViewModel.NotificationObject"));
                }
            }
            catch(Exception e) 
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private static void Log(object sender, string property)
        {
            if (!DebugModes.IsEnabled("EnableNotificationCounter"))
                return;

            try
            {
                timer.Interval = timerInterval;

                Type t = sender.GetType();
                string genericTypeInfo = t.IsGenericType ? $"[{t.GetGenericArguments()[0].FullName}]" : "";
                string typeString = $"{t.Namespace}.{t.Name}{genericTypeInfo}";

                string notificationId = $"{typeString}|{property}";

                lock (notifications)
                {
                    if (!notifications.ContainsKey(notificationId))
                        notifications.Add(notificationId, new NotificationLog());

                    notifications[notificationId].objects.Add(sender.GetHashCode());
                    notifications[notificationId].counter++;
                }
            }
            catch { }
        }

        private static void PatchObservableCollection()
        {
            var collection = typeof(ObservableCollection<>).MakeGenericType(typeof(object));
            var observableChanged = collection.GetMethod("OnCollectionChanged", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(NotifyCollectionChangedEventArgs) }, null);
            var postfix = typeof(NotificationCounter).GetMethod(nameof(CountCollectionChangedNotifications), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (postfix != null && observableChanged != null)
            {
                harmonyIntance.Patch(observableChanged, null, new HarmonyMethod(postfix));
            }
        }

        private static void PatchCoreNotifications(Type classType)
        {
            var notificationChanged = classType.GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
            var postfix = typeof(NotificationCounter).GetMethod(nameof(CountCoreNotifications), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (postfix != null && notificationChanged != null)
            {
                harmonyIntance.Patch(notificationChanged, null, new HarmonyMethod(postfix));
            }
        }

        private static void PatchUINotifications(Type classType)
        {
            var notificationChanged = classType.GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            var postfix = typeof(NotificationCounter).GetMethod(nameof(CountUINotifications), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (postfix != null && notificationChanged != null)
            {
                harmonyIntance.Patch(notificationChanged, null, new HarmonyMethod(postfix));
            }
        }

        private static void CountCollectionChangedNotifications(object __instance, NotifyCollectionChangedEventArgs e)
        {
            Log(__instance, e.Action.ToString());
        }

        private static void CountCoreNotifications(object __instance, string propertyName)
        {
            Log(__instance, propertyName);
        }

        private static void CountUINotifications(object __instance, string propertyName)
        {
            Log(__instance, propertyName);
        }
    }
}
