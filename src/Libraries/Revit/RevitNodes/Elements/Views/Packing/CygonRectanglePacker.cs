#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2011 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

//using Microsoft.Xna.Framework;

namespace Nuclex.Game.Packing
{

    /// <summary>Packer using a custom algorithm by Markus 'Cygon' Ewald</summary>
    /// <remarks>
    ///   <para>
    ///     Algorithm conceived by Markus Ewald (cygon at nuclex dot org), though
    ///     I'm quite sure I'm not the first one to come up with it :)
    ///   </para>
    ///   <para>
    ///     The algorithm always places rectangles as low as possible in the packing
    ///     area. So, for any new rectangle that is to be added, the packer has to
    ///     determine the X coordinate at which the rectangle can have the lowest
    ///     overall height without intersecting any other rectangles.
    ///   </para>
    ///   <para>
    ///     To quickly discover these locations, the packer uses a sophisticated
    ///     data structure that stores the upper silhouette of the packing area. When
    ///     a new rectangle needs to be added, only the silouette edges need to be
    ///     analyzed to find the position where the rectangle would achieve the lowest
    ///     placement possible in the packing area.
    ///   </para>
    /// </remarks>
    internal class CygonRectanglePacker : RectanglePacker
    {

        #region class SliceStartComparer

        /// <summary>Compares the starting position of height slices</summary>
        private class SliceStartComparer : IComparer<UV>
        {

            /// <summary>Provides a default instance for the anchor rank comparer</summary>
            public static SliceStartComparer Default = new SliceStartComparer();

            /// <summary>Compares the starting position of two height slices</summary>
            /// <param name="left">Left slice start that will be compared</param>
            /// <param name="right">Right slice start that will be compared</param>
            /// <returns>The relation of the two slice starts ranks to each other</returns>
            public int Compare(UV left, UV right)
            {
                if (left.U < right.U)
                    return -1;
                if (left.U > right.U)
                    return 1;
                else
                    return 0;
                //return left.U - right.U;
            }

        }

        #endregion

        /// <summary>Initializes a new rectangle packer</summary>
        /// <param name="packingAreaWidth">Maximum width of the packing area</param>
        /// <param name="packingAreaHeight">Maximum height of the packing area</param>
        public CygonRectanglePacker(double packingAreaWidth, double packingAreaHeight) :
            base(packingAreaWidth, packingAreaHeight)
        {

            this.heightSlices = new List<UV>();

            // At the beginning, the packing area is a single slice of height 0
            this.heightSlices.Add(new UV(0, 0));
        }

        /// <summary>Tries to allocate space for a rectangle in the packing area</summary>
        /// <param name="rectangleWidth">Width of the rectangle to allocate</param>
        /// <param name="rectangleHeight">Height of the rectangle to allocate</param>
        /// <param name="placement">Output parameter receiving the rectangle's placement</param>
        /// <returns>True if space for the rectangle could be allocated</returns>
        public override bool TryPack(
          double rectangleWidth, double rectangleHeight, out UV placement
        )
        {
            // If the rectangle is larger than the packing area in any dimension,
            // it will never fit!
            if (
              (rectangleWidth > PackingAreaWidth) || (rectangleHeight > PackingAreaHeight)
            )
            {
                placement = UV.Zero;
                return false;
            }

            // Determine the placement for the new rectangle
            bool fits = tryFindBestPlacement(rectangleWidth, rectangleHeight, out placement);

            // If a place for the rectangle could be found, update the height slice table to
            // mark the region of the rectangle as being taken.
            if (fits)
                integrateRectangle(placement.U, rectangleWidth, placement.V + rectangleHeight);

            return fits;
        }

        /// <summary>Finds the best position for a rectangle of the given dimensions</summary>
        /// <param name="rectangleWidth">Width of the rectangle to find a position for</param>
        /// <param name="rectangleHeight">Height of the rectangle to find a position for</param>
        /// <param name="placement">Receives the best placement found for the rectangle</param>
        /// <returns>True if a valid placement for the rectangle could be found</returns>
        private bool tryFindBestPlacement(
          double rectangleWidth, double rectangleHeight, out UV placement
        )
        {
            int bestSliceIndex = -1; // Slice index where the best placement was found
            double bestSliceY = 0; // Y position of the best placement found
            double bestScore = PackingAreaHeight; // lower == better!

            // This is the counter for the currently checked position. The search works by
            // skipping from slice to slice, determining the suitability of the location for the
            // placement of the rectangle.
            int leftSliceIndex = 0;

            // Determine the slice in which the right end of the rectangle is located when
            // the rectangle is placed at the far left of the packing area.
            int rightSliceIndex = this.heightSlices.BinarySearch(
              new UV(rectangleWidth, 0), SliceStartComparer.Default
            );
            if (rightSliceIndex < 0)
                rightSliceIndex = ~rightSliceIndex;

            while (rightSliceIndex <= this.heightSlices.Count)
            {

                // Determine the highest slice within the slices covered by the rectangle at
                // its current placement. We cannot put the rectangle any lower than this without
                // overlapping the other rectangles.
                double highest = this.heightSlices[leftSliceIndex].V;
                for (int index = leftSliceIndex + 1; index < rightSliceIndex; ++index)
                    if (this.heightSlices[index].V > highest)
                        highest = this.heightSlices[index].V;

                // Only process this position if it doesn't leave the packing area
                if ((highest + rectangleHeight <= PackingAreaHeight))
                {
                    double score = highest;

                    if (score < bestScore)
                    {
                        bestSliceIndex = leftSliceIndex;
                        bestSliceY = highest;
                        bestScore = score;
                    }
                }

                // Advance the starting slice to the next slice start
                ++leftSliceIndex;
                if (leftSliceIndex >= this.heightSlices.Count)
                    break;

                // Advance the ending slice until we're on the proper slice again, given the new
                // starting position of the rectangle.
                double rightRectangleEnd = this.heightSlices[leftSliceIndex].U + rectangleWidth;
                for (; rightSliceIndex <= this.heightSlices.Count; ++rightSliceIndex)
                {
                    double rightSliceStart;
                    if (rightSliceIndex == this.heightSlices.Count)
                        rightSliceStart = PackingAreaWidth;
                    else
                        rightSliceStart = this.heightSlices[rightSliceIndex].U;

                    // Is this the slice we're looking for?
                    if (rightSliceStart > rightRectangleEnd)
                        break;
                }

                // If we crossed the end of the slice array, the rectangle's right end has left
                // the packing area, and thus, our search ends.
                if (rightSliceIndex > this.heightSlices.Count)
                    break;

            } // while rightSliceIndex <= this.heightSlices.Count

            // Return the best placement we found for this rectangle. If the rectangle
            // didn't fit anywhere, the slice index will still have its initialization value
            // of -1 and we can report that no placement could be found.
            if (bestSliceIndex == -1)
            {
                placement = UV.Zero;
                return false;
            }
            else
            {
                placement = new UV(this.heightSlices[bestSliceIndex].U, bestSliceY);
                return true;
            }
        }

        /// <summary>Integrates a new rectangle into the height slice table</summary>
        /// <param name="left">Position of the rectangle's left side</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="bottom">Position of the rectangle's lower side</param>
        private void integrateRectangle(double left, double width, double bottom)
        {

            // Find the first slice that is touched by the rectangle
            int startSlice = this.heightSlices.BinarySearch(
              new UV(left, 0), SliceStartComparer.Default
            );
            double firstSliceOriginalHeight;

            // Since the placement algorithm always places rectangles on the slices,
            // the binary search should never some up with a miss!
            Debug.Assert(
              startSlice >= 0, "Slice starts within another slice"
            );

            // We scored a direct hit, so we can replace the slice we have hit
            firstSliceOriginalHeight = this.heightSlices[startSlice].V;
            this.heightSlices[startSlice] = new UV(left, bottom);

            double right = left + width;
            ++startSlice;

            // Special case, the rectangle started on the last slice, so we cannot
            // use the start slice + 1 for the binary search and the possibly already
            // modified start slice height now only remains in our temporary
            // firstSliceOriginalHeight variable
            if (startSlice >= this.heightSlices.Count)
            {

                // If the slice ends within the last slice (usual case, unless it has the
                // exact same width the packing area has), add another slice to return to
                // the original height at the end of the rectangle.
                if (right < PackingAreaWidth)
                    this.heightSlices.Add(new UV(right, firstSliceOriginalHeight));

            }
            else
            { // The rectangle doesn't start on the last slice

                int endSlice = this.heightSlices.BinarySearch(
                  startSlice, this.heightSlices.Count - startSlice,
                  new UV(right, 0), SliceStartComparer.Default
                );

                // Another direct hit on the final slice's end?
                if (endSlice > 0)
                {

                    this.heightSlices.RemoveRange(startSlice, endSlice - startSlice);

                }
                else
                { // No direct hit, rectangle ends inside another slice

                    // Make index from negative BinarySearch() result
                    endSlice = ~endSlice;

                    // Find out to which height we need to return at the right end of
                    // the rectangle
                    double returnHeight;
                    if (endSlice == startSlice)
                        returnHeight = firstSliceOriginalHeight;
                    else
                        returnHeight = this.heightSlices[endSlice - 1].V;

                    // Remove all slices covered by the rectangle and begin a new slice at its end
                    // to return back to the height of the slice on which the rectangle ends.
                    this.heightSlices.RemoveRange(startSlice, endSlice - startSlice);
                    if (right < PackingAreaWidth)
                        this.heightSlices.Insert(startSlice, new UV(right, returnHeight));

                } // if endSlice > 0

            } // if startSlice >= this.heightSlices.Count

        }

        /// <summary>Stores the height silhouette of the rectangles</summary>
        private List<UV> heightSlices;

    }

} // namespace Nuclex.Game.Packing
