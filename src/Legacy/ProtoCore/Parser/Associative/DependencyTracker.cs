using System;
using System.Collections.Generic;

namespace ProtoAssociative.DependencyPass
{
    public class DependencyTracker
    {
        public DependencyTracker ()
        {
            AllNodes = new List<Node>();
            TopologicallySortedNotes = new List<Node>();
            DirectDependents = new Dictionary<Node, List<Node>>();
            DirectContingents = new Dictionary<Node, List<Node>>();
            //new data
            //Dependents = new Util.MultiMap<Node, Node>();
            //Contingents = new Util.MultiMap<Node, Node>();
            //Nodes = new HashSet<Node>();
        }

        public void AddNode(Node node)
        {
            if (AllNodes.Contains(node))
                return;

            AllNodes.Add(node);

            if (!DirectContingents.ContainsKey(node))
                DirectContingents.Add(node, new List<Node>());

            if (!DirectDependents.ContainsKey(node))
                DirectDependents.Add(node, new List<Node>());
        }

        public void AddDirectContingent(Node node, Node contingent)
        {
            if(!DirectContingents[node].Contains(contingent))
                DirectContingents[node].Add(contingent);
        }

        public void AddDirectDependent(Node node, Node dependent)
        {
            if (!DirectDependents[node].Contains(dependent))
                DirectDependents[node].Add(dependent);
        }

        public void RemoveDirectContingents(Node node)
        {
            DirectContingents[node].Clear();
        }

        public void RemoveDirectDependents(Node node)
        {   
            DirectDependents[node].Clear();
        }

        public List<Node> AllNodes  {
            get;
            private set;
        }
        
        public List<Node> TopologicallySortedNotes  {
            get;
            set;
        }
        
        public Dictionary<Node,List<Node>> DirectContingents  {
            get;
            private set;
        }
        
        public Dictionary<Node,List<Node>> DirectDependents  {
            get;
            private set;
        }
        /*
        public HashSet<Node> AllNodes
        {
            get;
            set;
        }

        public Util.MultiMap<Node, Node> DirectDependents
        {
            get;
            set;
        }
        
        public Util.MultiMap<Node, Node> DirectContingents
        {
            get;
            set;
        }
         * */
    }

    public static class DependencyTrackerExtensions
    {
        public static void GenerateDependencyGraph(this DependencyTracker tracker, List<Node> ast)
        {
            foreach (Node t in ast)
            {
                GenerateDependencyGraph(tracker, t);
            }
        }

        public static void GenerateDependencyGraph(this DependencyTracker tracker, Node t)
        {
            t.GenerateDependencyGraph(tracker);
        }
    }
}

