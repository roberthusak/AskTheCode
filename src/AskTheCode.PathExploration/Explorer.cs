using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.PathExploration.Heuristics;
using AskTheCode.SmtLibStandard;
using System.Threading;

namespace AskTheCode.PathExploration
{
    public class Explorer
    {
        private ExplorationContext context;
        private StartingNodeInfo startingNode;
        private IEntryPointRecognizer finalNodeRecognizer;
        private Action<ExplorationResult> resultCallback;

        private SmtContextHandler smtContextHandler;

        private FlowGraphsNodeOverlay<List<ExplorationState>> statesOnLocations =
            new FlowGraphsNodeOverlay<List<ExplorationState>>(() => new List<ExplorationState>());

        internal Explorer(
            ExplorationContext explorationContext,
            IContextFactory smtContextFactory,
            StartingNodeInfo startingNode,
            IEntryPointRecognizer finalNodeRecognizer,
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
            var rootState = new ExplorationState(
                rootPath,
                this.smtContextHandler.CreateEmptySolver(rootPath, this.startingNode));
            this.AddState(rootState);
        }

        // TODO: Make readonly for the heuristics
        public HashSet<ExplorationState> States { get; private set; } = new HashSet<ExplorationState>();

        public IExplorationHeuristic ExplorationHeuristic { get; internal set; }

        public IMergingHeuristic MergingHeuristic { get; internal set; }

        public ISmtHeuristic SmtHeuristic { get; internal set; }

        // TODO: Divide into submethods to make more readable
        internal void Explore(CancellationToken cancelToken)
        {
            for (
                var currentNode = this.ExplorationHeuristic.PickNextState();
                currentNode != null;
                currentNode = this.ExplorationHeuristic.PickNextState())
            {
                // TODO: Consider reusing the node instead of discarding
                this.RemoveState(currentNode);

                IReadOnlyList<FlowEdge> edges;
                if (!(currentNode.Path.Node is EnterFlowNode))
                {
                    edges = currentNode.Path.Node.IngoingEdges;
                }
                else
                {
                    // TODO: Handle also border edges and their connections, properly process the call nodes
                    throw new NotImplementedException();
                }

                var toSolve = new List<ExplorationState>();

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
                        var branchedState = new ExplorationState(branchedPath, currentNode.SolverHandler);

                        bool wasMerged = false;
                        foreach (var mergeCandidate in this.statesOnLocations[branchedState.Path.Node].ToArray())
                        {
                            if (this.MergingHeuristic.DoMerge(branchedState, mergeCandidate))
                            {
                                SmtSolverHandler solverHandler;
                                if (branchedState.SolverHandler != mergeCandidate.SolverHandler)
                                {
                                    solverHandler = this.SmtHeuristic.SelectMergedSolverHandler(
                                        branchedState,
                                        mergeCandidate);
                                }
                                else
                                {
                                    solverHandler = branchedState.SolverHandler;
                                }

                                mergeCandidate.Merge(branchedState, solverHandler);
                                wasMerged = true;

                                break;
                            }
                        }

                        if (!wasMerged)
                        {
                            this.AddState(branchedState);
                        }

                        if (this.finalNodeRecognizer.IsFinalNode(branchedState.Path.Node)
                            || this.SmtHeuristic.DoSolve(branchedState))
                        {
                            toSolve.Add(branchedState);
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
                            this.RemoveState(branchedNode);
                            var result = branchedNode.SolverHandler.LastResult;
                            this.resultCallback(result);
                        }
                    }
                }

                // Check the cancellation before picking next node
                if (cancelToken.IsCancellationRequested)
                {
                    // It is an expected behaviour with well defined result, there is no need to throw an exception
                    break;
                }
            }
        }

        private void AddState(ExplorationState state)
        {
            this.States.Add(state);
            this.statesOnLocations[state.Path.Node].Add(state);
        }

        private void RemoveState(ExplorationState state)
        {
            this.States.Remove(state);
            this.statesOnLocations[state.Path.Node].Remove(state);
        }
    }
}
