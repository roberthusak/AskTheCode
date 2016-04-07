namespace AskTheCode.Core
{
    public class InspectionConditions
    {
        internal InspectionConditions(string expression)
        {
            this.Expression = expression;
        }

        public string Expression { get; private set; }
    }
}