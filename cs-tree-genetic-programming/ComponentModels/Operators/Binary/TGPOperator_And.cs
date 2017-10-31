using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    public class TGPOperator_And : TGPBinaryOperator
    {
        private double mTolerance = 0.0000001;
        public TGPOperator_And(double tolerance=0.0000001)
            : base("AND")
        {
            mTolerance = tolerance;
        }

        public override void Evaluate(params object[] tags)
        {
            double x1 = this[0];
            double x2 = this[1];
            if (IsTrue(x1) && IsTrue(x2))
            {
                mValue = 1;
            }
            else
            {
                mValue = 0;
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
            return new TGPOperator_And();
        }
    }
}
