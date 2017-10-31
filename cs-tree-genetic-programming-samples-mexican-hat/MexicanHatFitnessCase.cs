using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.MexicanHat
{
    using TreeGP.Core.ProblemModels;
    using TreeGP.ComponentModels;

    public class MexicanHatFitnessCase : IGPFitnessCase
    {
        private double mX1;
        private double mX2;
        private double mY;
        private double mComputedY;

        public double ComputedY
        {
            get { return mComputedY; }
        }

        public double X1
        {
            get { return mX1; }
            set { mX1 = value; }
        }

        public double X2
        {
            get { return mX2; }
            set { mX2 = value; }
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
            if (variable_name == "x1")
            {
                input = mX1;
                return true;
            }
            else if (variable_name=="x2")
            {
                input = mX2;
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
