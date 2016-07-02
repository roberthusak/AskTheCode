namespace AskTheCode.SmtLibStandard
{
    public interface INameProvider<TVariable>
        where TVariable : Variable
    {
        SymbolName GetName(TVariable variable);
    }
}