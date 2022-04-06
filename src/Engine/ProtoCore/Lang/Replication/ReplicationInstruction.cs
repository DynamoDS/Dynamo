using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.Lang.Replication
{
    /// <summary>
    /// How should the zip algorithm handle cases where one data stream
    /// is longer than the other
    /// </summary>
    public enum ZipAlgorithm
    {
        Shortest = 0, //Default
        Longest
    }

    /// <summary>
    /// Representation of the replication algorithm
    /// </summary>
    public struct ReplicationInstruction
    {
        public int CartesianIndex;      //Index to cartesian product over
        public List<int> ZipIndecies;   //Collections that we should zip over
        public ZipAlgorithm ZipAlgorithm; //Method used for executing zip 


        public bool Zipped;

        public override string ToString()
        {
            if (!Zipped) return "Cartesian, Index: " + CartesianIndex;

            string indices = "";

            for (int i = 0; i < ZipIndecies.Count - 1; i++)
            {
                indices += ZipIndecies[i] + ", ";
            }

            indices += ZipIndecies[ZipIndecies.Count - 1];

            indices += " - " + ZipAlgorithm;

            return "Zipped, indecies: " + indices;

        }

        public Boolean Equals(ReplicationInstruction other) {

            if (this.Zipped == other.Zipped && this.ZipAlgorithm == other.ZipAlgorithm)
            {
                if (this.ZipIndecies == null && other.ZipIndecies == null)
                {
                    if (this.CartesianIndex == other.CartesianIndex) return true;

                    return false;
                }

                if (this.ZipIndecies != null && other.ZipIndecies != null)
                {
                    // Fastest way to compare all elements of 2 lists. 
                    // Excluding one list from another list and checking if the leftover lists is empty or not. 
                    // https://stackoverflow.com/questions/12795882/quickest-way-to-compare-two-list
                    var currentExcludesOldList = this.ZipIndecies.Except(other.ZipIndecies).ToList();
                    var oldExcludescurrentList = other.ZipIndecies.Except(this.ZipIndecies).ToList();

                    return !currentExcludesOldList.Any() && !oldExcludescurrentList.Any();
                }
            }
            return false;
        }
    }
}
