﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.DSEngine;
using Dynamo.Models;
using ProtoCore.Mirror;

namespace Dynamo.Utilities
{
    public static class VersionUtilities
    {
        /// <summary>
        /// Parse the first n fields of a version string.  Delegates to
        /// Version.Parse.
        /// </summary>
        public static Version PartialParse(string versionString, int numberOfFields = 3)
        {
            var splitVersion = versionString.Split('.');
            if (splitVersion.Length < numberOfFields)
                throw new FormatException("You specified too many fields for the given string.");

            var rejoinedVersion = string.Join(".", splitVersion.Take(numberOfFields));

            return Version.Parse(rejoinedVersion);
        }
    }
}
