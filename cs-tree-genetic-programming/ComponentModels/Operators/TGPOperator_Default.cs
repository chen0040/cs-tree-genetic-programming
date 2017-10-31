using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators
{
    public class TGPOperator_Default : TGPOperator
    {
        public TGPOperator_Default(string symbol)
            : base(1, symbol)
        {

        }

        public override TGPOperator Clone()
        {
            TGPOperator_Default clone = new TGPOperator_Default(mSymbol);
            return clone;
        }
    }
}
