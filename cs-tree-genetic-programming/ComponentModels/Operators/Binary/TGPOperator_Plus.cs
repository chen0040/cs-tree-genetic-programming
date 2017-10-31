﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels.Operators.Binary
{
    public class TGPOperator_Plus : TGPBinaryOperator
    {
        public TGPOperator_Plus()
            : base("+")
        {
            
        }

        public override void Evaluate(params object[] tags)
        {
            mValue = this[0] + this[1];
        }

        public override TGPOperator Clone()
        {
            return new TGPOperator_Plus();
        }
    }
}
