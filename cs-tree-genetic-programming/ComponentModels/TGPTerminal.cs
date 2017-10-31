using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    public class TGPTerminal: TGPPrimitive
    {
        /// <summary>
        /// Handle for a constant which stands for a function with no arguments
        /// </summary>
        /// <returns></returns>
        public delegate object FunctorHandle(params object[] tags);

        public FunctorHandle FunctionHandle = null;


        public TGPTerminal(string symbol)
            : base(0)
        {
            mSymbol = symbol;
        }

        internal bool mIsConstant = false;
        internal int mTerminalIndex = 0;

        public TGPTerminal Clone()
        {
            TGPTerminal clone = new TGPTerminal(mSymbol);
            clone.mTerminalIndex = mTerminalIndex;
            clone.mIsConstant = mIsConstant;
            clone.mValue = mValue;
            clone.FunctionHandle = FunctionHandle;
            return clone;
        }

        public bool IsConstant
        {
            get { return mIsConstant; }
        }

        public override string ToString()
        {
            if (mIsConstant)
            {
                return string.Format("{0}", mValue);
            }
            else
            {
                return mSymbol;
            }
        }
    }
}
