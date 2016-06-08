namespace AskTheCode.SmtLibStandard
{
    public interface INameProvider<TVariable>
    {
        SymbolName GetName(TVariable variable);
    }
}