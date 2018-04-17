using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal interface IBuildingContext
    {
        BuildNode CurrentNode { get; set; }

        TypeModelManager ModelManager { get; }

        Task PendingTask { get; set; }

        SemanticModel SemanticModel { get; }

        DisplayNode AddDisplayNode(TextSpan span);

        BuildNode AddFinalNode(SyntaxNodeOrToken label, bool createDisplayNode = false);

        ITypeModel CreateTemporaryVariableModel(ITypeModelFactory factory, ITypeSymbol type);

        void DelayCompletion(Task pendingTask);

        BuildNode EnqueueNode(SyntaxNode syntax, DisplayNodeConfig displayConfig = DisplayNodeConfig.Ignore);

        IModellingContext GetModellingContext();

        BuildNode PrependCurrentNode(
            SyntaxNode prependedSyntax,
            DisplayNodeConfig displayConfig = DisplayNodeConfig.Ignore,
            bool isFinal = false);

        BuildNode ReenqueueCurrentNode(SyntaxNode syntaxUpdate, bool createDisplayNode = false);

        ITypeModel TryCreateTemporaryVariableModel(SyntaxNode syntax);

        ITypeModel TryGetDefinedVariableModel(ISymbol symbol);

        ITypeModel TryGetModel(SyntaxNode syntax);

        IValueModel TryGetValueModel(SyntaxNode syntax);

        ReferenceModel GetLocalInstanceModel(ITypeSymbol localType);
    }
}