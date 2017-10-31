using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators
{
    public abstract class TGPTernaryOperator : TGPOperator
    {
        public TGPTernaryOperator(string symbol)
            : base(3, symbol)
        {

        }
    }
}
