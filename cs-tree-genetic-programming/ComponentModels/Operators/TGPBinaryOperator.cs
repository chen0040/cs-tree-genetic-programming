using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators
{
    public abstract class TGPBinaryOperator : TGPOperator
    {
        public TGPBinaryOperator(string symbol)
            : base(2, symbol)
        {

        }
    }
}
