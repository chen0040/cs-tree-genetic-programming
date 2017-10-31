using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators
{
    /// <summary>
    /// User-defined binary operator
    /// </summary>
    public class UDBinaryOperator : TGPBinaryOperator
    {
        public delegate object FunctionHandle(UDBinaryOperator sender, params object[] tags);
        private FunctionHandle mComputer;
        public UDBinaryOperator(string symbol, FunctionHandle handle)
            : base(symbol)
        {
            mComputer = handle;
        }

        public override void Evaluate(params object[] tags)
        {
            mValue = mComputer(this, tags);
        }

        public override TGPOperator Clone()
        {
            return new UDBinaryOperator(mSymbol, mComputer);
        }
    }
}
