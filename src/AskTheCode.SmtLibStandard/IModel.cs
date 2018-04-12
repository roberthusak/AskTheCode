using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.SmtLibStandard
{
    public interface IModel
    {
        Interpretation GetInterpretation(SymbolName variableName);

        Interpretation GetInterpretation<TVariable>(INameProvider<TVariable> varNameProvider, Expression expression)
            where TVariable : Variable;

        //Interpretation<bool> GetInterpretation(BoolHandle handle);

        //Interpretation<long> GetInterpretation(IntHandle handle);

        //Interpretation<double> GetInterpretation(RealHandle handle);

        //Interpretation<string> GetInterpretation(StringHandle handle);

        object GetValue(Variable variable);

        bool GetValue(BoolHandle handle);

        long GetValue(IntHandle handle);

        double GetValue(RealHandle handle);

        string GetValue(StringHandle handle);
    }
}
