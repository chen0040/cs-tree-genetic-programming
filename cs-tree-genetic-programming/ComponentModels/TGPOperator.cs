using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    public abstract class TGPOperator : TGPPrimitive
    {
        internal int mOperatorIndex;
       

        public int OperatorIndex
        {
            get { return mOperatorIndex; }
        }

        public TGPOperator(int parameter_count, string symbol)
            : base(parameter_count)
        {
            mSymbol = symbol;
        }

        

        public override string ToString()
        {
            StringBuilder sb=new StringBuilder();
            sb.AppendFormat("({0}", mSymbol);
            for (int i = 0; i < Arity; ++i)
            {
                sb.AppendFormat(" {0}", this[i]);
            }
            sb.Append(")");
            return sb.ToString();
        }

        public abstract TGPOperator Clone();

        
    }
}
