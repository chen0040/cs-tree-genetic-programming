using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Sqrt : TGPUnaryOperator
    {
        public TGPOperator_Sqrt()
            : base("cos")
        {

        }

        public override void Evaluate(params object[] tags)
        {
            mValue = System.Math.Sqrt(this[0]);
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Sqrt();
        }
    }
}
