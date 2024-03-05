using System;
using System.IO;
using Dynamo.Properties;

namespace Dynamo.Utilities
{
    internal static class LocalizedExceptionHelper
    {
        /// <summary>
        /// This method will throw the exception with the translated string according to the selected Language in Dynamo
        /// </summary>
        /// <param name="ex"></param>
        /// <exception cref="Exception"></exception>
        internal static void ThrowLocalizedException(Exception ex)
        {
            if (ex is DirectoryNotFoundException)
                throw new Exception(Resources.StreamWriterNotFoundException);
            else
                throw ex;
        }
    }
}
