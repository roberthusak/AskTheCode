﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{
    public interface IContextFactory
    {
        IContext CreateContext();
    }
}
