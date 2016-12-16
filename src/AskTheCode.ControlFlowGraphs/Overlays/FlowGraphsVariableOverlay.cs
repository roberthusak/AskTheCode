using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class FlowGraphsVariableOverlay<T> :
        IOverlay<GlobalFlowVariableId, GlobalFlowVariable, T>,
        IOverlay<FlowGraphId, FlowGraph, LocalFlowVariableOverlay<T>>,
        IReadOnlyOverlay<GlobalFlowVariableId, GlobalFlowVariable, T>,
        IReadOnlyOverlay<FlowGraphId, FlowGraph, LocalFlowVariableOverlay<T>>
    {
        private GlobalFlowVariableOverlay<T> globalVariableOverlay;
        private FlowGraphOverlay<LocalFlowVariableOverlay<T>> localVariableOverlay;

        public FlowGraphsVariableOverlay(Func<T> defaultValueFactory = null)
        {
            this.globalVariableOverlay = new GlobalFlowVariableOverlay<T>(defaultValueFactory);
            this.localVariableOverlay = new FlowGraphOverlay<LocalFlowVariableOverlay<T>>(
                () => new LocalFlowVariableOverlay<T>(defaultValueFactory));
        }

        private FlowGraphsVariableOverlay(FlowGraphsVariableOverlay<T> other, Func<T, T> valueCloner)
        {
            this.globalVariableOverlay = other.globalVariableOverlay.Clone(valueCloner);
            this.localVariableOverlay = other.localVariableOverlay.Clone(overlay => overlay.Clone(valueCloner));
        }

        public Func<T> DefaultValueFactory
        {
            get { return this.globalVariableOverlay.DefaultValueFactory; }
            set { this.globalVariableOverlay.DefaultValueFactory = value; }
        }

        Func<LocalFlowVariableOverlay<T>> IOverlay<FlowGraphId, FlowGraph, LocalFlowVariableOverlay<T>>.DefaultValueFactory
        {
            get { return this.localVariableOverlay.DefaultValueFactory; }
            set { this.localVariableOverlay.DefaultValueFactory = value; }
        }

        public T this[GlobalFlowVariable globalVariable]
        {
            get { return this.globalVariableOverlay[globalVariable]; }
            set { this.globalVariableOverlay[globalVariable] = value; }
        }

        public T this[GlobalFlowVariableId globalVariableId]
        {
            get { return this.globalVariableOverlay[globalVariableId]; }
            set { this.globalVariableOverlay[globalVariableId] = value; }
        }

        public LocalFlowVariableOverlay<T> this[FlowGraph graph]
        {
            get { return this.localVariableOverlay[graph]; }
            set { this.localVariableOverlay[graph] = value; }
        }

        public LocalFlowVariableOverlay<T> this[FlowGraphId graphId]
        {
            get { return this.localVariableOverlay[graphId]; }
            set { this.localVariableOverlay[graphId] = value; }
        }

        public T this[LocalFlowVariable localVariable]
        {
            get { return this.localVariableOverlay[localVariable.Graph][localVariable]; }
            set { this.localVariableOverlay[localVariable.Graph][localVariable] = value; }
        }

        public T this[FlowVariable variable]
        {
            get
            {
                var localVariable = variable as LocalFlowVariable;
                if (localVariable != null)
                {
                    return this[localVariable];
                }
                else
                {
                    return this[(GlobalFlowVariable)variable];
                }
            }

            set
            {
                var localVariable = variable as LocalFlowVariable;
                if (localVariable != null)
                {
                    this[localVariable] = value;
                }
                else
                {
                    this[(GlobalFlowVariable)variable] = value;
                }
            }
        }

        public FlowGraphsVariableOverlay<T> Clone(Func<T, T> valueCloner = null)
        {
            return new FlowGraphsVariableOverlay<T>(this, valueCloner);
        }
    }
}
