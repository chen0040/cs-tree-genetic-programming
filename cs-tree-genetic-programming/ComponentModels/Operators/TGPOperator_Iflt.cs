using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    public class TGPOperator_Iflt : TGPOperator
    {
        public TGPOperator_Iflt()
            : base(4, "if<")
        {

        }

        public override void Evaluate(params object[] tags)
        {
            if (this[0] < this[1])
            {
                mValue = this[2];
            }
            else
            {
                mValue = this[3];
            }
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Iflt();
        }
    }
}
