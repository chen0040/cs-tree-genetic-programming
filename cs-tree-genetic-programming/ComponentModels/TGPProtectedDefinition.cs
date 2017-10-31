using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    public class TGPProtectedDefinition
    {
        private static TGPProtectedDefinition mInstance = null;
        private static object mSyncObj = new object();

        public const double DEFAULT_LGP_REG_POSITIVE_INF=10000000;
        public const double DEFAULT_LGP_REG_NEGATIVE_INF=-10000000;

        private bool mUseUndefinedLow = true;
        private double mLGP_REG_POSITIVE_INF = DEFAULT_LGP_REG_POSITIVE_INF;
        private double mLGP_REG_NEGATIVE_INF = DEFAULT_LGP_REG_NEGATIVE_INF;
        private double mUndefinedLow=1;
        private double mUndefinedHigh = 1000000;

        public double LGP_REG_POSITIVE_INF
        {
            get { return mLGP_REG_POSITIVE_INF; }
            set { mLGP_REG_POSITIVE_INF = value; }
        }

        public double LGP_REG_NEGATIVE_INF
        {
            get { return mLGP_REG_NEGATIVE_INF; }
            set { mLGP_REG_NEGATIVE_INF = value; }
        }

        public double UNDEFINED
        {
            get
            {
                if (mUseUndefinedLow)
                {
                    return mUndefinedLow;
                }
                return mUndefinedHigh;
            }
        }

        private TGPProtectedDefinition()
        {

        }

        public static TGPProtectedDefinition Instance
        {
            get
            {
                if (mInstance == null)
                {
                    lock (mSyncObj)
                    {
                        mInstance = new TGPProtectedDefinition();
                    }
                }
                return mInstance;
            }
        }
    }
}
