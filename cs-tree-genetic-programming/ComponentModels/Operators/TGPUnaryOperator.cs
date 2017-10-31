using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators
{
    public abstract class TGPUnaryOperator : TGPOperator
    {
        public TGPUnaryOperator(string symbol)
            : base(1, symbol)
        {

        }
    }
}
