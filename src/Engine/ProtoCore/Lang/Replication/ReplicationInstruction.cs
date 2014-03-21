using System.Collections.Generic;
using System.Text;

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
                    indecies += i + ", ";


                indecies += ZipIndecies[ZipIndecies.Count - 1];

                indecies += " - " + ZipAlgorithm;

                return "Zipped, indecies: " + indecies;
            }

        }
    }

    public struct ReplicationControl
    {
        public List<ReplicationInstruction> Instructions;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Replication Control Instructions: "+ Instructions.Count);

                foreach (ReplicationInstruction ri in Instructions)
                    sb.AppendLine("\t" + ri.ToString());

                return sb.ToString();
        }


    }

}
