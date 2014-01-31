using System;
using System.Collections.Generic;

namespace ProtoCore.AST.AssociativeAST
{
    public class DependencyTracker
    {
        public DependencyTracker ()
        {
            AllNodes = new List<AssociativeNode>();
            TopologicallySortedNotes = new List<AssociativeNode>();
            DirectDependents = new Dictionary<AssociativeNode, List<AssociativeNode>>();
            DirectContingents = new Dictionary<AssociativeNode, List<AssociativeNode>>();
            //new data
            //Dependents = new Util.MultiMap<AssociativeNode, AssociativeNode>();
            //Contingents = new Util.MultiMap<AssociativeNode, AssociativeNode>();
            //Nodes = new HashSet<AssociativeNode>();
        }

        public void AddNode(AssociativeNode node)
        {
            if (AllNodes.Contains(node))
                return;

            AllNodes.Add(node);

            if (!DirectContingents.ContainsKey(node))
                DirectContingents.Add(node, new List<AssociativeNode>());

            if (!DirectDependents.ContainsKey(node))
                DirectDependents.Add(node, new List<AssociativeNode>());
        }

        public void AddDirectContingent(AssociativeNode node, AssociativeNode contingent)
        {
            if(!DirectContingents[node].Contains(contingent))
                DirectContingents[node].Add(contingent);
        }

        public void AddDirectDependent(AssociativeNode node, AssociativeNode dependent)
        {
            if (!DirectDependents[node].Contains(dependent))
                DirectDependents[node].Add(dependent);
        }

        public void RemoveDirectContingents(AssociativeNode node)
        {
            DirectContingents[node].Clear();
        }

        public void RemoveDirectDependents(AssociativeNode node)
        {   
            DirectDependents[node].Clear();
        }

        public List<AssociativeNode> AllNodes  {
            get;
            private set;
        }
        
        public List<AssociativeNode> TopologicallySortedNotes  {
            get;
            set;
        }
        
        public Dictionary<AssociativeNode,List<AssociativeNode>> DirectContingents  {
            get;
            private set;
        }
        
        public Dictionary<AssociativeNode,List<AssociativeNode>> DirectDependents  {
            get;
            private set;
        }
        /*
        public HashSet<AssociativeNode> AllNodes
        {
            get;
            set;
        }

        public Util.MultiMap<AssociativeNode, AssociativeNode> DirectDependents
        {
            get;
            set;
        }
        
        public Util.MultiMap<AssociativeNode, AssociativeNode> DirectContingents
        {
            get;
            set;
        }
         * */
    }

    public static class DependencyTrackerExtensions
    {
        public static void GenerateDependencyGraph(this DependencyTracker tracker, List<AssociativeNode> ast)
        {
            foreach (AssociativeNode t in ast)
            {
                GenerateDependencyGraph(tracker, t);
            }
        }

        public static void GenerateDependencyGraph(this DependencyTracker tracker, AssociativeNode t)
        {
            throw new NotImplementedException();
        }
    }
}

