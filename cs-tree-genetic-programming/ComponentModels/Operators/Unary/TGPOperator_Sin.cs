using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Sin : TGPUnaryOperator
    {
        public TGPOperator_Sin()
            : base("sin")
        {

        }

        public override void Evaluate(params object[] tags)
        {
            mValue = System.Math.Sin(this[0]);
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Sin();
        }
    }
}
