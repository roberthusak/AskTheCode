using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationTests.Annotations
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LinearlyParametrizedEvaluationAttribute : Attribute
    {
        public LinearlyParametrizedEvaluationAttribute(string constMemberName, int startValue, int count, int step)
        {
            this.ConstMemberName = constMemberName;
            this.StartValue = startValue;
            this.Count = count;
            this.Step = step;
        }

        public string ConstMemberName { get; private set; }

        public int StartValue { get; private set; }

        public int Count { get; private set; }

        public int Step { get; private set; }
    }
}
