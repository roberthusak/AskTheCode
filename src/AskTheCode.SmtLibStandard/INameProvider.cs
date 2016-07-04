namespace AskTheCode.SmtLibStandard
{
    public interface INameProvider
    {
        SymbolName GetName(Variable variable);
    }

    public interface INameProvider<TVariable> : INameProvider
        where TVariable : Variable
    {
        SymbolName GetName(TVariable variable);
    }
}