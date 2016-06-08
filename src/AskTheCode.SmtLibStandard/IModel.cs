using AskTheCode.SmtLibStandard.Handles;
using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public interface IModel
    {
        Interpretation GetInterpretation(Variable variable);

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
