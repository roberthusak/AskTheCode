namespace AskTheCode.SmtLibStandard
{
    public interface IUnsatisfiableCoreElement
    {
        int AssertionIndex { get; }

        bool IsExpression(Expression expression);
    }
}
