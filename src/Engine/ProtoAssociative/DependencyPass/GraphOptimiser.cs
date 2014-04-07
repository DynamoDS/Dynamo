using System.Collections.Generic;

namespace ProtoCore.AST.AssociativeAST
{
	public class GraphOptimiser
	{
		private DependencyTracker tracker;

	    public void Execute(DependencyTracker tracker)
		{
			this.tracker = tracker;
			StraightLineCondensePass();	
			TopologicalSortPass();

		}
		
		//Topological sort on nodes
		private void TopologicalSortPass()
		{
			//Hash table for the visited status
			List<AssociativeNode> visited = new List<AssociativeNode>();
			
			List<AssociativeNode> nodesWithNoDependents = new List<AssociativeNode>();
			List<AssociativeNode> topologicalSort = new List<AssociativeNode>();
			
			foreach (AssociativeNode node in tracker.AllNodes)
				if (tracker.DirectDependents[node].Count == 0)
					nodesWithNoDependents.Add(node);
			
			//@PERF: If this is a bottleneck, replace recursive method with more efficent stack implementation
			foreach (AssociativeNode node in nodesWithNoDependents)
				TopologicalSortVisit(node, visited, topologicalSort);
			
			tracker.TopologicallySortedNotes = topologicalSort;
			
			
		}
		private void TopologicalSortVisit(AssociativeNode n, List<AssociativeNode> visited, List<AssociativeNode> topologicalSort)
		{
			if (visited.Contains(n))
				return;
			
			visited.Add(n);
			foreach (AssociativeNode node in tracker.DirectContingents[n])
				TopologicalSortVisit(node, visited, topologicalSort);
			
			topologicalSort.Add(n);
		}
		
		//Condense all straight line dependencies
		private void StraightLineCondensePass()
		{
			List<AssociativeNode> nodesWithAContingent = new List<AssociativeNode>();
			
			foreach (AssociativeNode node in tracker.AllNodes)
				if (tracker.DirectContingents[node].Count == 1)
					nodesWithAContingent.Add(node);
			
			foreach (AssociativeNode startNode in nodesWithAContingent)
			{
				//We might have already removed this, if so, skip
				if (!tracker.AllNodes.Contains(startNode))
					continue;
				
				AssociativeNode walkerNode = startNode;
				List<AssociativeNode> accNodeList = new List<AssociativeNode>();
				
				
				while (tracker.DirectDependents[walkerNode].Count < 2 &&
				       (tracker.DirectDependents[walkerNode].Count == 0 || //Last node in the tree
				        tracker.DirectContingents[tracker.DirectDependents[walkerNode][0]].Count == 1)) //straight line node	
				{
					
					//Disabled for Mono
					//Validity.Assert(tracker.DirectContingents[tracker.DirectDependents[walkerNode]] == 1);
					
					accNodeList.Add(walkerNode);
					
					if (tracker.DirectDependents[walkerNode].Count == 0)
						break;
					
					//Disabled for Mono
					//Validity.Assert(tracker.DirectDependents[walkerNode].Count == 1);
					
					walkerNode = tracker.DirectDependents[walkerNode][0];
					
				}
				
				if (accNodeList.Count > 1)
				{
					//We have a straight line to merge
					MergeNode newNode = new MergeNode();
					
					foreach (AssociativeNode n in accNodeList)
					{
						newNode.MergedNodes.Add(n);
						tracker.AllNodes.Remove(n);
					}
					
					//Register the new node
					tracker.AddNode(newNode);

					//Update the existing dependency nodes to target the new nodes
					foreach (AssociativeNode n in tracker.DirectContingents[accNodeList[0]])
					{
						tracker.AddDirectDependent(n, accNodeList[0]);
						tracker.AddDirectDependent(n, newNode);
                        tracker.AddDirectContingent(newNode, n);
					}
					
					foreach (AssociativeNode n in tracker.DirectDependents[accNodeList[accNodeList.Count -1]])
					{
						tracker.AddDirectContingent(n, accNodeList[accNodeList.Count -1]);
						tracker.AddDirectContingent(n, newNode);
						tracker.AddDirectDependent(newNode, n);
					}
					
					foreach (AssociativeNode n in accNodeList)
					{
						tracker.RemoveDirectDependents(n);
						tracker.RemoveDirectContingents(n);
					}
				}
			}
		}
	}
}

