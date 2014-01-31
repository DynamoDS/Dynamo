using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitServices.Elements
{
    public static class ElementUtils
    {
        /// <summary>
        /// Attempts to fetch the Element corresponding to the given Unique Identifier in the given
        /// document.
        /// </summary>
        /// <typeparam name="T">The expected type of the Element being retreived.</typeparam>
        /// <param name="document">The Revit Document containing the Unique Identifier.</param>
        /// <param name="uniqueId">Unique Identifier for a Revit Element.</param>
        /// <param name="element">Element to be retrieved, only valid if call returns true.</param>
        /// <returns>
        /// True if the Unique Identifier was found and the corresponding Element is valid and of type T,
        /// false otherwise.
        /// </returns>
        public static bool TryGetElement<T>(this Document document, string uniqueId, out T element)
            where T : Element
        {
            try
            {
                element = document.GetElement(uniqueId) as T;
                return element != null && element.Id != null;
            }
            catch (Exception)
            {
                element = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to fetch the Element corresponding to the given ElementId in the given
        /// document.
        /// </summary>
        /// <typeparam name="T">The expected type of the Element being retreived.</typeparam>
        /// <param name="document">The Revit Document containing the ElementId.</param>
        /// <param name="id">ElementId for a Revit Element.</param>
        /// <param name="element">Element to be retrieved, only valid if call returns true.</param>
        /// <returns>
        /// True if the ElementId was found and the corresponding Element is valid and of type T,
        /// false otherwise.
        /// </returns>
        public static bool TryGetElement<T>(this Document document, ElementId id, out T element)
            where T : Element
        {
            try
            {
                element = document.GetElement(id) as T;
                return element != null && element.Id != null;
            }
            catch (Exception)
            {
                element = null;
                return false;
            }
        }
    }
}
