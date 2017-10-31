using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Log : TGPUnaryOperator
    {
        public TGPOperator_Log()
            : base("log")
        {

        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Log();
        }

        public override void Evaluate(params object[] tags)
        {
            double x = this[0];
            if (x == 0)
            {
                mValue = x + TGPProtectedDefinition.Instance.UNDEFINED;
            }
            else
            {
                mValue = System.Math.Log(x);
            }
        }
    }
}
