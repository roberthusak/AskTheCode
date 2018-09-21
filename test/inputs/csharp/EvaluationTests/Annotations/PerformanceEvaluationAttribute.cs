using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationTests.Annotations
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PerformanceEvaluationAttribute : Attribute
    {
        public PerformanceEvaluationAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
