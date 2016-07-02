using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.PathExploration.Heuristics;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration
{
    public class Explorer
    {
        private ExplorationContext context;
        private StartingNodeInfo startingNode;
        private IFinalNodeRecognizer finalNodeRecognizer;
        private Action<ExplorationResult> resultCallback;

        private SmtContextHandler smtContextHandler;

        private FlowGraphsNodeOverlay<List<ExplorationNode>> nodesOnLocations =
            new FlowGraphsNodeOverlay<List<ExplorationNode>>(() => new List<ExplorationNode>());

        internal Explorer(
            ExplorationContext explorationContext,
            IContextFactory smtContextFactory,
            StartingNodeInfo startingNode,
            IFinalNodeRecognizer finalNodeRecognizer,
            Action<ExplorationResult> resultCallback)
        {
            // TODO: Solve this marginal case directly in the ExplorationContext
            Contract.Requires(!finalNodeRecognizer.IsFinalNode(startingNode.Node));

            this.context = explorationContext;
            this.startingNode = startingNode;
            this.finalNodeRecognizer = finalNodeRecognizer;
            this.resultCallback = resultCallback;

            this.smtContextHandler = new SmtContextHandler(smtContextFactory);

            var rootPath = new Path(
                ImmutableArray<Path>.Empty,
                0,
                this.startingNode.Node,
                ImmutableArray<FlowEdge>.Empty);
            var rootNode = new ExplorationNode(
                rootPath,
                this.smtContextHandler.CreateEmptySolver(rootPath, this.startingNode));
            this.AddNode(rootNode);
        }

        // TODO: Make readonly for the heuristics
        public HashSet<ExplorationNode> Nodes { get; private set; } = new HashSet<ExplorationNode>();

        public IExplorationHeuristic ExplorationHeuristic { get; internal set; }

        public IMergingHeuristic MergingHeuristic { get; internal set; }

        public ISmtHeuristic SmtHeuristic { get; internal set; }

        // TODO: Divide into submethods to make more readable
        internal void Explore()
        {
            for (
                var currentNode = this.ExplorationHeuristic.PickNextNode();
                currentNode != null;
                currentNode = this.ExplorationHeuristic.PickNextNode())
            {
                // TODO: Consider reusing the node instead of discarding
                this.RemoveNode(currentNode);

                IReadOnlyList<FlowEdge> edges;
                if (currentNode.Path.Node is InnerFlowNode)
                {
                    edges = currentNode.Path.Node.IngoingEdges;
                }
                else
                {
                    // TODO: Handle also border edges and their connections
                    throw new NotImplementedException();
                }

                var toSolve = new List<ExplorationNode>();

                int i = 0;
                foreach (bool doBranch in this.ExplorationHeuristic.DoBranch(currentNode, edges))
                {
                    if (doBranch)
                    {
                        var branchedPath = new Path(
                            ImmutableArray.Create(currentNode.Path),
                            currentNode.Path.Depth + 1,
                            edges[i].From,
                            ImmutableArray.Create(edges[i]));
                        var branchedNode = new ExplorationNode(branchedPath, currentNode.SolverHandler);

                        bool wasMerged = false;
                        foreach (var mergeCandidate in this.nodesOnLocations[branchedNode.Path.Node].ToArray())
                        {
                            if (this.MergingHeuristic.DoMerge(branchedNode, mergeCandidate))
                            {
                                SmtSolverHandler solverHandler;
                                if (branchedNode.SolverHandler != mergeCandidate.SolverHandler)
                                {
                                    solverHandler = this.SmtHeuristic.SelectMergedSolverHandler(
                                        branchedNode,
                                        mergeCandidate);
                                }
                                else
                                {
                                    solverHandler = branchedNode.SolverHandler;
                                }

                                mergeCandidate.Merge(branchedNode, solverHandler);
                                wasMerged = true;

                                break;
                            }
                        }

                        if (!wasMerged)
                        {
                            this.AddNode(branchedNode);
                        }

                        if (this.finalNodeRecognizer.IsFinalNode(branchedNode.Path.Node)
                            || this.SmtHeuristic.DoSolve(branchedNode))
                        {
                            toSolve.Add(branchedNode);
                        }
                    }
                    else
                    {
                        // TODO: Inform about the uncertainty of the verification at this location
                    }

                    i++;
                }

                if (toSolve.Count > 0)
                {
                    int j = 0;
                    foreach (bool doReuse in this.SmtHeuristic.DoReuse(currentNode.SolverHandler, toSolve))
                    {
                        if (!doReuse)
                        {
                            toSolve[j].SolverHandler = currentNode.SolverHandler.Clone();
                        }

                        j++;
                    }

                    foreach (var branchedNode in toSolve)
                    {
                        var resultKind = branchedNode.SolverHandler.Solve(branchedNode.Path);

                        if (resultKind != ExplorationResultKind.Reachable || this.finalNodeRecognizer.IsFinalNode(branchedNode.Path.Node))
                        {
                            this.RemoveNode(branchedNode);
                            var result = branchedNode.SolverHandler.LastResult;
                            this.resultCallback(result);
                        }
                    }
                }
            }
        }

        private void AddNode(ExplorationNode node)
        {
            this.Nodes.Add(node);
            this.nodesOnLocations[node.Path.Node].Add(node);
        }

        private void RemoveNode(ExplorationNode node)
        {
            this.Nodes.Remove(node);
            this.nodesOnLocations[node.Path.Node].Remove(node);
        }
    }
}
