using System;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Dynamo.Graph.Workspaces.Locking
{
    internal static class GraphLockFile
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.Indented,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        });

        internal static string PathFor(string graphPath)
        {
            var fullPath = Path.GetFullPath(graphPath);
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return Path.Combine(directory, "." + fileName + ".dynlock");
        }

        internal static bool TryCreateExclusive(string sidecarPath, GraphLockInfo info)
        {
            try
            {
                using (var stream = new FileStream(sidecarPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                using (var writter = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    Serializer.Serialize(writter, info);
                }

                return true;
            }
            catch (IOException) when (File.Exists(sidecarPath))
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        internal static bool TryRead(string sidecarPath, out GraphLockInfo info)
        {
            info = null;

            const int maxAttempts = 2;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    using (var stream = File.Open(sidecarPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        info = Serializer.Deserialize<GraphLockInfo>(jsonReader);
                    }

                    return info != null;
                }
                catch (Exception ex) when (ex is IOException || ex is JsonException)
                {
                    if (attempt == maxAttempts - 1)
                    {
                        return false;
                    }

                    // Wait for 50 milliseconds before retrying
                    Thread.Sleep(50);
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
                catch (SecurityException)
                {
                    return false;
                }
            }

            return false;
        }

        internal static void WriteHeartbeat(string sidecarPath, GraphLockInfo info)
        {
            var tempPath = sidecarPath + "temp";

            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                Serializer.Serialize(writer, info);
            }

            try
            {
                File.Replace(tempPath, sidecarPath, null, true);
            }
            catch (IOException)
            {
                File.Move(tempPath, sidecarPath, true);
            }
            catch (PlatformNotSupportedException)
            {
                File.Move(tempPath, sidecarPath, true);
            }
        }

        internal static void TryDelete(string sidecarPath)
        {
            try
            {
                File.Delete(sidecarPath);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            catch (SecurityException)
            {
                return;
            }
        }
    }
}
