using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    public class TGPPrimitive
    {
        internal string mSymbol;
        protected double[] mParameters = null;
        public int Arity
        {
            get 
            {
                if (mParameters == null)
                {
                    return 0;
                }
                return mParameters.Length; 
            }
        }

        public double this[int index]
        {
            get
            {
                return mParameters[index];
            }
            set
            {
                mParameters[index] = value;
            }
        }

        public bool IsTerminal
        {
            get { return mParameters==null || mParameters.Length == 0; }
        }

        public object mValue;
        public object Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public double DoubleValue
        {
            get { return Convert.ToDouble(mValue); }
            set { mValue = value; }
        }

        public TGPPrimitive(int parameter_count)
        {
            if (parameter_count > 0)
            {
                mParameters = new double[parameter_count];
                for (int i = 0; i < parameter_count; ++i)
                {
                    mParameters[i] = 0;
                }
            }
            
        }

        public virtual void Evaluate(params object[] tags)
        {

        }

        public string Symbol
        {
            get { return mSymbol; }
        }


    }
}
