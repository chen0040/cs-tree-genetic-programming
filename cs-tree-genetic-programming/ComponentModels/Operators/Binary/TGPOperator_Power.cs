using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    public class TGPOperator_Power : TGPBinaryOperator
    {
        public TGPOperator_Power()
            : base("^")
        {

        }

        public override void Evaluate(params object[] tags)
        {
            double x1 = this[0];
            double x2 = this[1];
            if (System.Math.Abs(x1) < 10)
            {
                mValue = System.Math.Pow(System.Math.Abs(x1), x2);
            }
            else
            {
                mValue = x1 + x2 + TGPProtectedDefinition.Instance.UNDEFINED;
            }

        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Power();
        }
    }
}
