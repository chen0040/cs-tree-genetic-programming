using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Exp : TGPUnaryOperator
    {
        public TGPOperator_Exp()
            : base("exp")
        {

        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Exp();
        }

        public override void Evaluate(params object[] tags)
        {
            
            mValue = System.Math.Exp(this[0]);
        }
    }
}
