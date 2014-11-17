using System;
using System.Collections.Generic;
using System.Linq;
using DSNodeServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements.InternalUtilities;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Level
    /// </summary>
    [RegisterForTrace]
    public class Level : Element
    {
        #region Internal properties

        /// <summary>
        /// Internal reference to Revit element
        /// </summary>
        internal Autodesk.Revit.DB.Level InternalLevel
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalLevel; }
        }

        #endregion

        #region Private constructor

        /// <summary>
        /// Private constructor for Level
        /// </summary>
        /// <param name="elevation"></param>
        /// <param name="name"></param>
        private Level(double elevation, string name)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Level>(Document);

            //There was an element, bind & mutate
            if (oldEle != null)
            {
                InternalSetLevel(oldEle);
                InternalSetElevation(elevation);
                InternalSetName(name);
                return;
            }

            //There was no element, create a new one
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.Level level;

            if (Document.IsFamilyDocument)
            {
                level = Document.FamilyCreate.NewLevel(elevation);
            }
            else
            {
                level = Document.Create.NewLevel(elevation);
            }

            InternalSetLevel(level);
            InternalSetName(name);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);

        }

        private Level(Autodesk.Revit.DB.Level level)
        {
            this.InternalSetLevel(level);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the Element, it's Id, and it's uniqueId
        /// </summary>
        /// <param name="level"></param>
        private void InternalSetLevel(Autodesk.Revit.DB.Level level)
        {
            this.InternalLevel = level;
            this.InternalElementId = level.Id;
            this.InternalUniqueId = level.UniqueId;
        }

        /// <summary>
        /// Mutate the height of the level
        /// </summary>
        /// <param name="elevation"></param>
        private void InternalSetElevation(double elevation)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            this.InternalLevel.Elevation = elevation;
            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Mutate the name of the level
        /// </summary>
        /// <param name="name"></param>
        private void InternalSetName(string name)
        {
            if (String.IsNullOrEmpty(name) ||
                string.CompareOrdinal(InternalLevel.Name, name) == 0)
                return;

            //Check to see whether there is an existing level with the same name
            var levels = ElementQueries.GetAllLevels();
            while (levels.Any(x => string.CompareOrdinal(x.Name, name) == 0))
            {
                //Update the level name
                ElementUtils.UpdateLevelName(ref name);
            }

            TransactionManager.Instance.EnsureInTransaction(Document);
            this.InternalLevel.Name = name;
            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The elevation of the level above ground level
        /// </summary>
        public double Elevation
        {
            get { return InternalLevel.Elevation*UnitConverter.HostToDynamoFactor; }
        }

        /// <summary>
        /// Elevation relative to the Project origin
        /// </summary>
        public double ProjectElevation
        {
            get
            {
                return InternalLevel.ProjectElevation * UnitConverter.HostToDynamoFactor;
            }
        }

        /// <summary>
        /// The name of the level
        /// </summary>
        public new string Name
        {
            get
            {
                return InternalLevel.Name;
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Level given it's elevation and name in the project
        /// </summary>
        /// <param name="elevation"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Level ByElevationAndName(double elevation, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return new Level(elevation * UnitConverter.DynamoToHostFactor, name);
        }

        /// <summary>
        /// Create a Revit Level given it's elevation.  The name will be whatever
        /// Revit gives it.
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public static Level ByElevation(double elevation)
        {
            return new Level(elevation * UnitConverter.DynamoToHostFactor, null);
        }

        /// <summary>
        /// Create a Revit Level given it's length offset from an existing level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Level ByLevelAndOffset(Level level, double offset)
        {
            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            return new Level((level.Elevation + offset) * UnitConverter.DynamoToHostFactor, null);
        }

        /// <summary>
        /// Create a Revit Level given a distance offset from an existing 
        /// level and a name for the new level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="offset"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Level ByLevelOffsetAndName(Level level, double offset, string name)
        {
            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return new Level((level.Elevation + offset) * UnitConverter.DynamoToHostFactor, name);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Level from a user selected Element.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Level FromExisting(Autodesk.Revit.DB.Level level, bool isRevitOwned)
        {
            return new Level(level)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Level(Name={0}, Elevation={1})", Name, Elevation);
        }
    }
}
