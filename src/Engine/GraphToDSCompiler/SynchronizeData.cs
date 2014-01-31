using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace GraphToDSCompiler
{
    // Stub enum
    public enum RequestStatus
    {
        None,
        StillBusy,
        DataNotFound,
        Succeeded
    }

    // Stub enum
    public enum SnapshotNodeType
    {
        None,
        Identifier,
        Literal,
        Function,
        CodeBlock,
        Array,
        Property,
        Method
    }
    public enum StringExpressionType
    {
        None,
        Identifier,
        Literal,
        CodeBlock,
        Expression
    }

    // Stub class
    [Serializable]
    public struct Connection
    {
        [XmlElement("OtherNode")]
        public uint OtherNode;
        [XmlElement("LocalIndex")]
        public int LocalIndex;
        [XmlElement("OtherIndex")]
        public int OtherIndex;
        [XmlElement("LocalName")]
        public string LocalName;
        [XmlElement("IsImplicit")]
        public bool IsImplicit;
        /*
        public string ToString()
        {
            char ConnectionDataMemberMarker = 'ά';
            char ConnectionEndMarker = 'Ψ';
            string s = string.Empty;
            if (OtherNode != null)
                s += OtherNode + ConnectionDataMemberMarker.ToString();
            if (LocalIndex != null)
                s += LocalIndex + ConnectionDataMemberMarker.ToString();
            if (OtherIndex != null)
                s += OtherIndex + ConnectionDataMemberMarker.ToString();
            if (LocalName != null)
                s += LocalName + ConnectionDataMemberMarker.ToString();
            s += ConnectionEndMarker.ToString();
            return s;
        }*/
    }

    [Serializable]
    public class AssignmentStatement
    {
        [XmlElement("Assignee")]
        public string Assignee { get; set; }

        [XmlArray("References"), XmlArrayItem(typeof(string), ElementName = "References")]
        public List<string> References { get; set; }

        public AssignmentStatement() // For XML serializer use.
        {
        }

        public AssignmentStatement(string assignee)
        {
            this.Assignee = assignee;
            this.References = new List<string>();
        }
    }

    // Stub class
    [Serializable]
    public class SnapshotNode
    {
        //private SnapshotNodeType type=Type;
        // These two are for UI to construct.
        public void ConnectTo(uint connectToUid, int localIndex, int otherIndex, bool isConnectingFromInputSlot, string name = "")
        {
            Connection connection = new Connection();
            connection.LocalIndex = localIndex;
            connection.LocalName = name;
            connection.OtherIndex = otherIndex;
            connection.OtherNode = connectToUid;

            if (isConnectingFromInputSlot)
            {
                InputList.Add(connection);
            }
            else
            {
                OutputList.Add(connection);
            }
        }

        public void ConnectTo(uint connectToUid, int localIndex, int otherIndex, bool isConnectingFromInputSlot, bool isImplicit, string name = "")
        {
            Connection connection = new Connection();
            connection.LocalIndex = localIndex;
            connection.LocalName = name;
            connection.OtherIndex = otherIndex;
            connection.OtherNode = connectToUid;
            connection.IsImplicit = isImplicit;

            if (isConnectingFromInputSlot)
            {
                InputList.Add(connection);
            }
            else
            {
                OutputList.Add(connection);
            }
        }

        public int GetNumOutputs()
        {
            int outputSlots = 0;
            foreach (Connection connect in OutputList)
            {
                if (connect.LocalIndex >= outputSlots)
                {
                    outputSlots++;
                }
            }
            return outputSlots;
        }

        // These are for LiveRunner to consume.
        [XmlElement("Id")]
        public uint Id { get; set; }
        [XmlElement("Type")]
        public SnapshotNodeType Type { get; set; }
        [XmlElement("Content")]
        public string Content { get; set; }
        [XmlArray("Assignments"), XmlArrayItem(typeof(AssignmentStatement), ElementName = "Assignment")]
        public List<AssignmentStatement> Assignments { get; set; }
        [XmlArray("InputList"), XmlArrayItem(typeof(Connection), ElementName = "Connection")]
        public List<Connection> InputList { get; set; }
        [XmlArray("OutputList"), XmlArrayItem(typeof(Connection), ElementName = "Connection")]
        public List<Connection> OutputList { get; set; }
        [XmlArray("UndefinedVariables"), XmlArrayItem(typeof(string), ElementName = "UndefinedVariables")]
        public List<string> UndefinedVariables { get; set; }

        public SnapshotNode() { }
        public SnapshotNode(uint id, SnapshotNodeType type, string content)
        {
            this.Id = id;
            this.Type = type;
            this.Content = content;
            Assignments = new List<AssignmentStatement>();
            InputList = new List<Connection>();
            OutputList = new List<Connection>();
            UndefinedVariables = new List<string>();
        }

        public static string CreateReplicationGuideText(List<int> data)
        {
            string result = string.Empty;

            if (data.Count > 0)
            {
                foreach (int i in data)
                {
                    if (i == 0)
                        break;
                    result += i.ToString();
                    result += ",";
                }
                result = result.Remove(result.Length - 1);
            }
            return result;
        }

        public static string ParseReplicationGuideText(string replicationText)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(replicationText))
            {
                string[] replications = replicationText.Split(',');
                string temp = "<>";
                foreach (string rep in replications)
                {
                    if (!string.IsNullOrEmpty(rep))
                    {
                        temp = temp.Insert(1, rep);
                        result += temp;
                        temp = "<>";
                    }
                }
            }
            return result;
        }
        /*
        public string ToString()
        {
            string s = string.Empty;
            char IDMarker = 'Ί';
            char TypeMarker = 'Δ';
            char ContentLengthMarker = 'ʢ';
            char ContentEndMarker = 'Ώ';
            char ConnectionListCountMarker = 'Γ';
            char ConnectionListElementMarker = 'ε';
            char InputListEndMarker = 'θ';
            char OutputListEndMarker= 'λ';
            char ConnectionListEndMarker = 'δ';
            char SnapshotNodeEndMarker = 'ξ';
            if (Id != null)
                s += Id;
            s += IDMarker.ToString();
            if (Type != null)
                s += Type;
            s += TypeMarker;
            if (Content.Equals(string.Empty)) s += "0" + ContentLengthMarker.ToString();
            else
                s += Content.Length + ContentLengthMarker.ToString() + Content;
            s += ContentEndMarker.ToString();
            s += InputList.Count + ConnectionListCountMarker.ToString();
            if (InputList.Count > 0)
                foreach (Connection i in InputList)
                    s += i.ToString() + ConnectionListElementMarker.ToString();
            s += InputListEndMarker.ToString();
            s += ConnectionListEndMarker.ToString();
            s += OutputList.Count + ConnectionListCountMarker.ToString();
            if (OutputList.Count > 0)
                foreach (Connection i in OutputList)
                    s += i.ToString() + ConnectionListElementMarker.ToString();
            s += OutputListEndMarker.ToString();
            s += ConnectionListEndMarker.ToString();
            s += SnapshotNodeEndMarker.ToString();
            return s;
        }*/
    }

    [Serializable]
    [XmlRoot("SynchronizeDataCollection")]
    public class SynchronizeDataCollection
    {
        [XmlArray("SynchronizeDataCollection"), XmlArrayItem(typeof(SynchronizeData), ElementName = "SynchronizeData")]
        public List<SynchronizeData> sdList = new List<SynchronizeData>();
        public string Serialize()
        {
            var xml = new StringWriter();
            XmlSerializer ser = new XmlSerializer(typeof(SynchronizeDataCollection));
            ser.Serialize(xml, this);
            return xml.ToString();
        }
        public static SynchronizeDataCollection Deserialize(string xml)
        {
            var xs = new XmlSerializer(typeof(SynchronizeDataCollection));
            return (SynchronizeDataCollection)xs.Deserialize(new StringReader(xml));
        }
    }

    // Stub class
    [Serializable]
    public class SynchronizeData
    {
        [XmlArray("RemovedNodes"), XmlArrayItem(typeof(uint), ElementName = "RemovedNodeIDs")]
        public List<uint> RemovedNodes;
        [XmlArray("AddedNodes"), XmlArrayItem(typeof(SnapshotNode), ElementName = "AddedNode")]
        public List<SnapshotNode> AddedNodes;
        [XmlArray("ModifiedNodes"), XmlArrayItem(typeof(SnapshotNode), ElementName = "ModifiedNode")]
        public List<SnapshotNode> ModifiedNodes;

        static char NodesCountMarker = '¥';
        static char NodesContentMarker = 'Σ';
        static char RemovedNodesListEndMarker = 'Ř';
        static char AddedNodesListEndMarker = 'Ă';
        static char ModifiedNodesListEndMarker = 'Ň';
        static char NodeListEndMarker = 'Ł';
        static char SyncDataEndMarker = '¡';

        static char IDMarker = 'Ί';
        static char TypeMarker = 'Δ';
        static char ContentLengthMarker = 'ʢ';
        static char ContentEndMarker = 'Ώ';
        static char ConnectionListCountMarker = 'Γ';
        static char ConnectionListElementMarker = 'ε';
        static char InputListEndMarker = 'θ';
        static char OutputListEndMarker = 'λ';
        static char ConnectionListEndMarker = 'δ';
        static char SnapshotNodeEndMarker = 'ξ';

        static char ConnectionDataMemberMarker = 'ά';

        public SynchronizeData()
        {
            RemovedNodes = new List<uint>();
            AddedNodes = new List<SnapshotNode>();
            ModifiedNodes = new List<SnapshotNode>();
        }

        /*
        public string ToString()
        {
            
            string s = string.Empty;
            s += RemovedNodes.Count + NodesCountMarker.ToString();
            if (RemovedNodes.Count > 0)
                foreach (uint i in RemovedNodes)
                    s += i + NodesContentMarker.ToString();
            s += RemovedNodesListEndMarker.ToString();
            s += NodeListEndMarker.ToString();
            s += AddedNodes.Count + NodesCountMarker.ToString();
            if (AddedNodes.Count > 0)
                foreach (SnapshotNode i in AddedNodes)
                    s += i.ToString() + NodesContentMarker.ToString();
            s += AddedNodesListEndMarker.ToString();
            s += NodeListEndMarker.ToString();
            s += ModifiedNodes.Count + NodesCountMarker.ToString();
            if (ModifiedNodes.Count > 0)
                foreach (SnapshotNode i in ModifiedNodes)
                    s += i.ToString() + NodesContentMarker.ToString();
            s += ModifiedNodesListEndMarker.ToString();
            s += NodeListEndMarker.ToString();
            s += SyncDataEndMarker.ToString();
            var xml = new StringWriter();
            return s; 
        }
        */
        private static List<SynchronizeData> SDLevel(string s)
        {
            List<SynchronizeData> sdList = new List<SynchronizeData>();
            s.Trim();
            string[] a = s.Split(SyncDataEndMarker);
            for (int i = 0; i < a.Length - 1; i++)
            {
                string[] b = a[i].Split(NodeListEndMarker);
                sdList.Add(ListLevel(b));
            }
            /* string s = string.Empty;
            s += "SyncDataStart\n{";
            s += "\nRemovedNodesCount - " + RemovedNodes.Count + "\n[";
            if (RemovedNodes.Count > 0)  
                foreach (uint i in RemovedNodes)
                    s += i + " ";
            s += "RemovedNodesEndHere\n]";
            s += "\nAddedNodesCount - " + AddedNodes.Count + "\n[";
            if (AddedNodes.Count > 0)
                foreach (SnapshotNode i in AddedNodes)
                    s += i.ToString() + " ";
            s += "AddedNodesEndHere\n]";
            s += "\nModifiedNodesCount - " + ModifiedNodes.Count + "\n[";
            if (ModifiedNodes.Count > 0)
                foreach (SnapshotNode i in ModifiedNodes)
                    s += i.ToString() + " ";
            s += "ModifiedNodesEndHere\n]";
            s += "EndSyncData\n}¡";
            return s;*/
            return sdList;
        }

        private static SynchronizeData ListLevel(string[] b)
        {
            SynchronizeData sd = new SynchronizeData();
            for (int i = 0; i < b.Length - 1; i++)
            {
                string[] c = b[i].Split(NodesCountMarker);
                int count = Int32.Parse(c[0]);
                if (count == 0) continue;
                else
                {
                    if (i == 0) sd.RemovedNodes = UintNodesProcess(c[1]);
                    else if (i == 1) sd.AddedNodes = SnapshotNodeListProcess(c[1]);
                    else sd.ModifiedNodes = SnapshotNodeListProcess(c[1]);
                }
            }
            return sd;
        }

        private static List<SnapshotNode> SnapshotNodeListProcess(string p)
        {
            List<SnapshotNode> lsn = new List<SnapshotNode>();
            string[] uin = p.Split(NodesContentMarker);
            for (int i = 0; i < uin.Length - 1; i++)
            {
                lsn.Add(SnapshotNodeProcess(uin[i]));
            }
            return lsn;
        }

        private static SnapshotNode SnapshotNodeProcess(string p)
        {
            uint ID;
            SnapshotNodeType t;
            string Content;
            List<Connection> inputs = new List<Connection>();
            List<Connection> outputs = new List<Connection>();
            string[] a = p.Split(IDMarker);
            ID = uint.Parse(a[0]);
            string[] b = a[1].Split(TypeMarker);
            switch (b[0])
            {
                case "Identifier": t = SnapshotNodeType.Identifier; break;
                case "Literal": t = SnapshotNodeType.Literal; break;
                case "Function": t = SnapshotNodeType.Function; break;
                case "CodeBlock": t = SnapshotNodeType.CodeBlock; break;
                case "Array": t = SnapshotNodeType.Array; break;
                default: t = SnapshotNodeType.None; break;
            }
            string[] c = b[1].Split(ContentLengthMarker);
            string[] d = c[1].Split(ContentEndMarker);
            if (Int32.Parse(c[0]) == 0) Content = string.Empty;
            else Content = d[0];
            string[] e = d[1].Split(ConnectionListEndMarker);
            inputs = ProcessConnectionLists(e[0]);
            outputs = ProcessConnectionLists(e[1]);
            SnapshotNode ssn = new SnapshotNode(ID, t, Content);
            ssn.InputList = inputs;
            ssn.OutputList = outputs;
            return ssn;
        }

        private static List<Connection> ProcessConnectionLists(string p)
        {
            List<Connection> lc = new List<Connection>();
            string[] e = p.Split(ConnectionListCountMarker);
            if (Int32.Parse(e[0]) == 0) ;
            else
            {
                string[] f = e[1].Split(ConnectionListElementMarker);
                lc = ProcessConnectionLists(f);
            }
            return lc;
        }

        private static List<Connection> ProcessConnectionLists(string[] f)
        {
            List<Connection> lc = new List<Connection>();
            for (int i = 0; i < f.Length - 1; i++)
            {
                Connection con = new Connection();
                string[] c = f[i].Split(ConnectionDataMemberMarker);
                con.OtherNode = uint.Parse(c[0]);
                con.LocalIndex = Int32.Parse(c[1]);
                con.OtherIndex = Int32.Parse(c[2]);
                con.LocalName = c[3];
                lc.Add(con);
            }
            return lc;
        }

        private static List<uint> UintNodesProcess(string p)
        {
            string[] uin = p.Split(NodesContentMarker);
            List<uint> removedNodes = new List<uint>();
            for (int i = 0; i < uin.Length - 1; i++) removedNodes.Add(uint.Parse(uin[i]));
            return removedNodes;
        }

        public static List<SynchronizeData> FromString(string s)
        {
            List<SynchronizeData> sd = new List<SynchronizeData>();
            sd = SDLevel(s);
            return sd;
        }
    }
}