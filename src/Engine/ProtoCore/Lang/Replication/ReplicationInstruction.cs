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
            if (!Zipped)
                return "Cartesian, Index: " + CartesianIndex;
            else
            {
                string indecies = "";

                for (int i = 0; i < ZipIndecies.Count - 1; i++)
                    indecies += ZipIndecies[i] + ", ";

                indecies += ZipIndecies[ZipIndecies.Count - 1];

                indecies += " - " + ZipAlgorithm;

                return "Zipped, indecies: " + indecies;
            }

        }

        public Boolean Equals(ReplicationInstruction oldOption) {

            if (this.Zipped == oldOption.Zipped && this.ZipAlgorithm == oldOption.ZipAlgorithm)
            {
                if (this.ZipIndecies == null && oldOption.ZipIndecies == null)
                    return true;

                if (this.ZipIndecies != null && oldOption.ZipIndecies != null)
                {
                    // Fastest way to compare all elements of 2 lists. 
                    // Excluding one list from another list and checking if the leftover lists is empty or not. 
                    // https://stackoverflow.com/questions/12795882/quickest-way-to-compare-two-list
                    var currentExcludesOldList = this.ZipIndecies.Except(oldOption.ZipIndecies).ToList();
                    var oldExcludescurrentList = oldOption.ZipIndecies.Except(this.ZipIndecies).ToList();

                    return !currentExcludesOldList.Any() && !oldExcludescurrentList.Any();
                }
            }
            return false;
        }
    }
}
