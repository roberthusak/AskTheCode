﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AskTheCode.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    public sealed class ControlView : NotifyPropertyChangedBase
    {
        private readonly IIdeServices ideServices;
        private readonly InspectionContextProvider contextProvider;
        private InspectionContext context;

        private string searchedExpression;

        public ControlView(IIdeServices ideServices, InspectionContextProvider contextProvider)
        {
            Contract.Requires<ArgumentNullException>(ideServices != null, nameof(ideServices));
            Contract.Requires<ArgumentNullException>(contextProvider != null, nameof(contextProvider));

            this.contextProvider = contextProvider;
            this.ideServices = ideServices;
            this.SearchCommand = new Command(this.Search, name: "Search");
        }

        public string SearchedExpression
        {
            get { return this.searchedExpression; }
            set { this.SetProperty(ref this.searchedExpression, value); }
        }

        public ObservableCollection<TreeNodeView> TreeNodes { get; } = new ObservableCollection<TreeNodeView>();

        public ICommand SearchCommand { get; private set; }

        public async void Search(object parameter)
        {
            // TODO: Validate searched text

            var workspace = this.ideServices.Workspace;
            var solution = workspace.CurrentSolution;

            // TODO: Validate opened solution

            Document document;
            LinePosition position;
            this.ideServices.TryGetCaretPosition(out document, out position);

            // TODO: Validate position in the code

            this.TreeNodes.Clear();

            this.context = this.contextProvider.CreateContext(solution);
            await this.context.StartInspecting(document, position, this.SearchedExpression);
            await this.context.InspectNode(this.context.InspectionTreeRoot);

            var treeNode = new TreeNodeView(this.ideServices, this.context.InspectionTreeRoot);
            this.TreeNodes.Add(treeNode);

            // TODO: Handle inspecting of multiple nodes in one run
        }
    }
}