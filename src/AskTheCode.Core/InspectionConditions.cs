namespace AskTheCode.Core
{
    public sealed class InspectionConditions
    {
        internal InspectionConditions(string expression)
        {
            this.Expression = expression;
        }

        public string Expression { get; private set; }
    }
}