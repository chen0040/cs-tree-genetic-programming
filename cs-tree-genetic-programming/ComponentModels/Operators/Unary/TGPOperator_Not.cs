using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Unary
{
    public class TGPOperator_Not : TGPUnaryOperator
    {
        private double mTolerance = 0.0000001;
        public TGPOperator_Not(double tolerance = 0.0000001)
            : base("NOT")
        {
            mTolerance = tolerance;
        }

        public override void Evaluate(params object[] tags)
        {
            double x=this[0];
            if (IsTrue(x))
            {
                mValue = 0;
            }
            else
            {
                mValue = 1;
            }
        }

        public bool IsTrue(double x)
        {
            if (x > -mTolerance && x < mTolerance)
            {
                return false;
            }
            return true;
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Not();
        }
    }
}
