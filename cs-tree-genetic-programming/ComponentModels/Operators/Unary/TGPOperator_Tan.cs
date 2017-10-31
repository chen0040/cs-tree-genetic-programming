using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Tan : TGPUnaryOperator
    {
        public TGPOperator_Tan()
            : base("tan")
        {

        }

        public override void Evaluate(params object[] tags)
        {
            mValue = System.Math.Tan(this[0]);
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Tan();
        }
    }
}
