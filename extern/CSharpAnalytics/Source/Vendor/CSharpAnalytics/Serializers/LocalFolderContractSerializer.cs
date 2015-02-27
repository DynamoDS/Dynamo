﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace CSharpAnalytics.Serializers
{
    /// <summary>
    /// Provides an easy way to serialize and deserialize simple classes to the local folder in
    /// WindowsStore applications.
    /// </summary>
    /// <typeparam name="T">Type of class to serialize.</typeparam>
    internal static class LocalFolderContractSerializer<T>
    {
        /// <summary>
        /// Restore an object from local folder storage.
        /// </summary>
        /// <param name="filename">Optional filename to use, name of the class if not provided.</param>
        /// <param name="deleteBadData">Optional boolean on whether delete the existing file if deserialization fails, defaults to false.</param>
        /// <returns>Task that holds the deserialized object once complete.</returns>
        public static async Task<T> RestoreAsync(string filename = null, bool deleteBadData = false)
        {
            var serializer = new DataContractSerializer(typeof (T), new[] { typeof (DateTimeOffset) });

            try
            {
                // We use create and not open to avoid missing file exceptions as they really mess with
                // exception-enabled debugging.
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename ?? typeof(T).Name, CreationCollisionOption.OpenIfExists);

                try
                {
                    using (var inputStream = await file.OpenStreamForReadAsync())
                        return inputStream.Length == 0
                            ? default(T)
                            : (T)serializer.ReadObject(inputStream);
                }
                catch (SerializationException)
                {
                    if (deleteBadData)
                        file.DeleteAsync().AsTask().Wait();
                    throw;
                }
            }
            catch (FileNotFoundException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Save an object to local folder storage asynchronously.
        /// </summary>
        /// <param name="value">Object to save to local storage.</param>
        /// <param name="filename">Optional filename to save to, defaults to the name of the class.</param>
        /// <returns>Task that completes once the object is saved.</returns>
        public static async Task SaveAsync(T value, string filename = null)
        {
            var serializer = new DataContractSerializer(typeof(T), new[] { typeof(DateTimeOffset) });

            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, value);
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename ?? typeof(T).Name, CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
        }
    }
}