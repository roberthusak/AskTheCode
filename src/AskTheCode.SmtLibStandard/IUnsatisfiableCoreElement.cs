namespace AskTheCode.SmtLibStandard
{
    public interface IUnsatisfiableCoreElement
    {
        IAssertionStackLevel AssertionStackLevel { get; }

        int AssertionIndex { get; }

        Expression Assertion { get; }
    }
}
