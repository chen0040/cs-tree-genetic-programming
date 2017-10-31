using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    /// <summary>
    /// Protected Division as described in Section 3.2.1 of "A Field Guide to Genetic Programming"
    /// </summary>
    public class TGPOperator_Division : TGPBinaryOperator
    {
        public TGPOperator_Division()
            : base("/")
        {
            
        }

        public override void Evaluate(params object[] tags)
        {
            double x1 = this[0];
            double x2 = this[1];
            if (System.Math.Abs(x2) < 0.001)
            {
                mValue = x1;
            }
            else
            {
                mValue = x1 / x2;
            }
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Division();
        }
    }
}
