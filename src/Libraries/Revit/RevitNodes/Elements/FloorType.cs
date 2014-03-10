using System;
using System.Linq;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit FloorType
    /// </summary>
    /// http://revitapisearch.com.s3-website-us-east-1.amazonaws.com/html/b6fd8c08-7eea-1ab4-b7ab-096778b46e8f.htm
    public class FloorType : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// An internal reference to the FloorType
        /// </summary>
        internal Autodesk.Revit.DB.FloorType InternalFloorType
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalFloorType; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for the Element
        /// </summary>
        /// <param name="floorType"></param>
        private FloorType(Autodesk.Revit.DB.FloorType floorType)
        {
            InternalSetFloorType(floorType);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the FloorType property, element id, and unique id
        /// </summary>
        /// <param name="floorType"></param>
        private void InternalSetFloorType( Autodesk.Revit.DB.FloorType floorType )
        {
            this.InternalFloorType = floorType;
            this.InternalElementId = floorType.Id;
            this.InternalUniqueId = floorType.UniqueId;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The name of the FloorType
        /// </summary>
        public string Name
        {
            get { return InternalFloorType.Name; }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Select a FloorType from the document given 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FloorType ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var floorType = DocumentManager.Instance
                .ElementsOfType<Autodesk.Revit.DB.FloorType>()
                .FirstOrDefault(x => x.Name == name);

            if (floorType == null)
            {
                throw new Exception("A Revit FloorType with that name could not be located in the document.");
            }

            // until there is a way to create a FloorType from Dynamo, 
            // this object should never be cleaned up
            return new FloorType(floorType)
            {
                IsRevitOwned = true
            };

        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a FloorType from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static FloorType FromExisting(Autodesk.Revit.DB.FloorType floorType, bool isRevitOwned)
        {
            return new FloorType(floorType)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        public override string ToString()
        {
            return InternalFloorType.Name;
        }
    }
}
