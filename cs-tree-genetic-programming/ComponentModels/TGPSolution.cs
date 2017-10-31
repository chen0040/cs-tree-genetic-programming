using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGP.Core.ComponentModels;
using TreeGP.Core.ProblemModels;
using TreeGP.Distribution;

namespace TreeGP.ComponentModels
{
    public class TGPSolution : IGPSolution
    {
        protected double mFitness;
        protected double mObjectiveValue;
        protected bool mIsFitnessValid = false;
        protected Dictionary<string, object> mAttributes = new Dictionary<string, object>();

        protected IGPPop mPop;
        protected List<TGPProgram> mTrees = new List<TGPProgram>();
        protected TGPSolution(IGPPop pop, int tree_count)
        {
            mPop = pop;
            for (int i = 0; i < tree_count; ++i)
            {
                mTrees.Add(pop.CreateProgram() as TGPProgram);
            }
        }


        public void RemoveAttributes()
        {
            mAttributes.Clear();
        }

        public TGPSolution()
        {

        }

        public virtual ISolution Create(IGPPop pop, int tree_count)
        {
            return new TGPSolution(pop, tree_count);
        }

        public virtual ISolution Clone()
        {
            TGPSolution clone = new TGPSolution(mPop, mTrees.Count);
            clone.Copy(this);
            return clone;
        }

        public object this[string attrname]
        {
            get
            {
                if (mAttributes.ContainsKey(attrname))
                {
                    return mAttributes[attrname];
                }
                return null;
            }
            set
            {
                mAttributes[attrname] = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mTrees.Count; ++i)
            {
                if (i != 0)
                {
                    sb.AppendLine();
                }
                sb.AppendFormat("Trees[{0}]: {1}", i, mTrees[i]);
            }
            return sb.ToString();
        }

        protected virtual void Copy(TGPSolution rhs)
        {
            mFitness = rhs.mFitness;
            mIsFitnessValid = rhs.mIsFitnessValid;
            mObjectiveValue = rhs.mObjectiveValue;
            mTrees.Clear();
            for (int i = 0; i < rhs.mTrees.Count; ++i)
            {
                mTrees.Add(rhs.mTrees[i].Clone());
            }
            foreach(string attrname in rhs.mAttributes.Keys)
            {
                mAttributes[attrname] = rhs.mAttributes[attrname];
            }
        }

        public double Fitness
        {
            get { return mFitness; }
        }

        public double ObjectiveValue
        {
            get { return mObjectiveValue; }
        }

        public bool IsFitnessValid
        {
            get { return mIsFitnessValid; }
        }

        public void TrashFitness()
        {
            mIsFitnessValid = false;
        }

        public delegate void RunFitnessCaseCompletedHandler(IGPFitnessCase fitness_case);

        public virtual void DoWork(RunFitnessCaseCompletedHandler handler)
        {
            IGPEnvironment env = mPop.Environment as IGPEnvironment;
            int fitness_case_count = env.GetFitnessCaseCount();
            List<IGPFitnessCase> cases = new List<IGPFitnessCase>();
            for (int i = 0; i < fitness_case_count; ++i)
            {
                IGPFitnessCase fitness_case = env.CreateFitnessCase(i);

                for (int tindex = 0; tindex < mTrees.Count; ++tindex)
                {
                    fitness_case.StoreOutput(mTrees[tindex].ExecuteOnFitnessCase(fitness_case), tindex);
                }

                handler(fitness_case);
            }

        }

         

        public string MathExpression
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < mTrees.Count; ++i)
                {
                    if (i != 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendFormat("Trees[{0}]: {1}", i, mTrees[i].MathExpression);
                }
                return sb.ToString();
            }
        }

        public virtual void EvaluateFitness()
        {
            mObjectiveValue = mPop.EvaluateObjective(this, 0);

            if (mPop.IsMaximization)
            {
                mFitness = mObjectiveValue;
            }
            else
            {
                mFitness = -mObjectiveValue;
            }
            mIsFitnessValid = true;
        }


        public int AverageTreeDepth
        {
            get{
                if (mTrees.Count == 0) return 0;
                int sum=0;
                for (int i = 0; i < mTrees.Count; ++i)
                {
                    sum += (mTrees[i]).Depth;
                }
                return sum / mTrees.Count;
            }
            
        }

        public int AverageTreeLength
        {
            get
            {
                if (mTrees.Count == 0) return 0;
                int sum = 0;
                for (int i = 0; i < mTrees.Count; ++i)
                {
                    sum += (mTrees[i]).Length;
                }
                return sum / mTrees.Count;
            }

        }



        public virtual List<double> Execute(Dictionary<string, double> variables)
        {
            List<double> results = new List<double>();
            for (int i = 0; i < mTrees.Count; ++i)
            {
                results.Add(mTrees[i].Execute(variables));
            }
            return results;
        }

        public int Compare(TGPSolution rhs)
        {
            return mFitness.CompareTo(rhs.mFitness);
            //if (mFitness > rhs.mFitness)
            //{
            //    return 1;
            //}
            //else if (mFitness == rhs.mFitness)
            //{
            //    return 0;
            //}
            //else
            //{
            //    return -1;
            //}
        }

        public bool IsBetterThan(TGPSolution rhs)
        {
            if (mFitness > rhs.Fitness)
            {
                return true;
            }
            else if (mFitness == rhs.Fitness)
            {
                int this_better_count = 0;
                for (int i = 0; i < mTrees.Count; ++i)
                {
                    if (mTrees[i].IsBetterThan(rhs.mTrees[i]))
                    {
                        this_better_count++;
                    }
                }

                if (this_better_count * 2 > mTrees.Count)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public void SubtreeCrossover(IGPSolution _rhs, int iMaxDepthForCrossOver, string method, object tag=null)
        {
            TGPSolution rhs = (TGPSolution)_rhs;
            int tree_count= mTrees.Count;
            for (int i = 0; i < tree_count; ++i)
            {
                if (tree_count > 1 && DistributionModel.GetUniform() < 0.5)
                {
                    TGPProgram temp=rhs.mTrees[i];
                    rhs.mTrees[i] = mTrees[i];
                    mTrees[i] = temp;
                }
                else
                {
                    mTrees[i].SubtreeCrossover(rhs.mTrees[i], iMaxDepthForCrossOver, method, tag);
                }
                
            }


            TrashFitness();
            rhs.TrashFitness();
        }

        public void Mutate(int iMaxProgramDepth, string method, object tag=null)
        {
            for (int i = 0; i < mTrees.Count; ++i)
            {
                mTrees[i].Mutate(iMaxProgramDepth, method, tag);
            }

            TrashFitness();
        }

        public void CreateWithDepth(int iAllowableDepth, string method, object tag = null)
        {
            for (int i = 0; i < mTrees.Count; ++i)
            {
                mTrees[i].CreateWithDepth(iAllowableDepth, method, tag);
            }
            
            TrashFitness();
        }

        public virtual void MicroMutate()
        {
            for (int i = 0; i < mTrees.Count; ++i)
            {
                mTrees[i].MicroMutate();
            }
        }

        public int ProgramCount
        {
            get
            {
                return mTrees.Count;
            }
        }

        public object FindProgramAt(int i)
        {
            return mTrees[i];
        }
    }
}
