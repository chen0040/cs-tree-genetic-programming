using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    using TreeGP.Distribution;
    using Operators.Binary;
    using Operators;

    public class TGPOperatorSet
    {
        private List<KeyValuePair<TGPOperator, double>> mOperators = new List<KeyValuePair<TGPOperator, double>>();
        private double mWeightSum = 0;

        public TGPOperatorSet()
        {

        }

        public void AddOperators(params TGPOperator[] operators)
        {
            foreach (TGPOperator op in operators)
            {
                AddOperator(op);
            }
        }

        public TGPOperator FindRandomOperator(TGPOperator current_operator = null)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                double r = mWeightSum * DistributionModel.GetUniform();

                double current_sum = 0;
                foreach (KeyValuePair<TGPOperator, double> point in mOperators)
                {
                    current_sum += point.Value;
                    if (current_sum >= r)
                    {
                        if (point.Key != current_operator)
                        {
                            return point.Key;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            }

            return current_operator;
        }

        public TGPOperator FindOperatorByIndex(int op_index)
        {
            return mOperators[op_index].Key;
        }

        public double FindOperatorWeightByIndex(int terminal_index)
        {
            return mOperators[terminal_index].Value;
        }

        public TGPOperatorSet Clone()
        {
            TGPOperatorSet clone = new TGPOperatorSet();
            clone.mWeightSum = mWeightSum;
            foreach (KeyValuePair<TGPOperator, double> point in mOperators)
            {
                clone.mOperators.Add(new KeyValuePair<TGPOperator, double>(point.Key.Clone(), point.Value));
            }

            return clone;
        }

        public int OperatorCount
        {
            get { return mOperators.Count; }
        }

        public void AddOperator(TGPOperator op, double weight = 1)
        {
            mOperators.Add(new KeyValuePair<TGPOperator, double>(op, weight));
            op.mOperatorIndex = mOperators.Count - 1;
            mWeightSum += weight;
        }

        public void AddOperator(string op_symbol, double weight = 1)
        {
           AddOperator(new TGPOperator_Default(op_symbol));

        }

        public void AddIfltOperator(double weight = 1)
        {
            AddOperator(new TGPOperator_Iflt(), weight);
        }

        public void AddIfgtOperator(double weight = 1)
        {
            AddOperator(new TGPOperator_Ifgt(), weight);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mOperators.Count; ++i)
            {
                sb.AppendFormat("operators[{0}]: {1}\n", i, mOperators[i].Key);
            }
            return sb.ToString();
        }

        public TGPOperator FindRandomOperator(int parameter_count,TGPOperator current_operator=null)
        {
            TGPOperator selected_op = null;
            for (int attempts = 0; attempts < 10; attempts++)
            {
                double r = mWeightSum * DistributionModel.GetUniform();

                double current_sum = 0;
                foreach (KeyValuePair<TGPOperator, double> point in mOperators)
                {
                    if (selected_op == null && point.Key.Arity == parameter_count && point.Key != current_operator)
                    {
                        selected_op = point.Key;
                    }
                    current_sum += point.Value;
                    if (current_sum >= r)
                    {
                        if (point.Key != current_operator && point.Key.Arity == parameter_count)
                        {
                            return point.Key;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            }

            return selected_op;
        }
    }
}
