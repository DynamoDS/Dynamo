using System.Collections.Generic;
using System.Text;

namespace ProtoCore.Lang.Replication
{
    public struct ReplicationInstruction
    {
        public int CartesianIndex;      //Index to cartesian product over
        public List<int> ZipIndecies;   //Collections that we should zip over

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
