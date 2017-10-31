using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    public class TGPOperator_Modulo : TGPBinaryOperator
    {
        public TGPOperator_Modulo()
            : base("%")
        {
            
        }

        public override void Evaluate(params object[] tags)
        {
            int x1 = (int)this[0];
            int x2 = (int)this[1];
            if (x1 < 0) x1 = -x1;
            if (x2 < 0) x2 = -x2;
            if (x2 == 0)
            {
                mValue = x1;
            }
            else
            {
                mValue = x1 % x2;
            }
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Modulo();
        }
    }
}
