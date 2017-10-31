using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.SymbolicRegression
{
    using TreeGP.Core.ProblemModels;
    using TreeGP.ComponentModels;

    public class SymRegFitnessCase : IGPFitnessCase
    {
        private double mX;
        private double mY;
        private double mComputedY;

        public double ComputedY
        {
            get { return mComputedY; }
        }

        public double X
        {
            get { return mX; }
            set { mX = value; }
        }

        public double Y
        {
            get { return mY; }
            set { mY = value; }
        }

        public void StoreOutput(object result, int tree_index)
        {
            mComputedY = (double)result;
        }

        public bool QueryInput(string variable_name, out object input)
        {
            input = 0;
            if (variable_name == "x")
            {
                input = mX;
                return true;
            }
            
            return false;
        }


        public int GetInputCount()
        {
            return 1;
        }


    }
}
