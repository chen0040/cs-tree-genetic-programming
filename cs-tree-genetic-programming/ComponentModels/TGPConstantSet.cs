using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    /// <summary>
    /// Constant Set contains two type of terminals: constant value and function with no arguments, as specified in Section 3.1 of "A Field Guide to Genetic Programming"
    /// </summary>
    public class TGPConstantSet
    {
        private List<KeyValuePair<TGPTerminal, double>> mTerminals = new List<KeyValuePair<TGPTerminal, double>>();
        private double mWeightSum = 0;

        public TGPConstantSet()
        {

        }

        public void AddTerminals(params TGPTerminal[] terminals)
        {
            foreach (TGPTerminal terminal in terminals)
            {
                AddTerminal(terminal);
            }
        }

        public TGPConstantSet Clone()
        {
            TGPConstantSet clone = new TGPConstantSet();
            clone.mWeightSum = mWeightSum;
            foreach (KeyValuePair<TGPTerminal, double> point in mTerminals)
            {
                clone.mTerminals.Add(new KeyValuePair<TGPTerminal, double>(point.Key.Clone(), point.Value));
            }
            return clone;
        }

        public TGPTerminal FindRandomTerminal(TGPTerminal current_terminal=null)
        {
            for(int attempts=0; attempts < 10; attempts++)
            {
                double r = mWeightSum * TreeGP.Distribution.DistributionModel.GetUniform();

                double current_sum = 0;
                foreach (KeyValuePair<TGPTerminal, double> point in mTerminals)
                {
                    current_sum += point.Value;
                    if (current_sum >= r)
                    {
                        if (point.Key != current_terminal)
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

            return current_terminal;
        }

        public TGPTerminal FindTerminalByIndex(int terminal_index)
        {
            return mTerminals[terminal_index].Key;
        }

        public int TerminalCount
        {
            get { return mTerminals.Count; }
        }

        public double FindTerminalWeightByIndex(int terminal_index)
        {
            return mTerminals[terminal_index].Value;
        }

        public void AddTerminal(TGPTerminal terminal, double weight = 1)
        {
            mTerminals.Add(new KeyValuePair<TGPTerminal, double>(terminal, weight));
            terminal.mTerminalIndex = mTerminals.Count - 1;
            terminal.mIsConstant = true;
            mWeightSum += weight;
        }

        public void AddConstant(string symbol, object value, double weight = 1)
        {
            TGPTerminal terminal = new TGPTerminal("");
            terminal.Value = value;
            AddTerminal(terminal);
            terminal.mSymbol = symbol;
        }

       
        /// <summary>
        /// Method that adds a function with no arguments to the constant set
        /// </summary>
        /// <param name="symbol">The symbol for the function</param>
        /// <param name="function">The actual mechanism of the function which returns a value</param>
        /// <param name="weight">weight for the function for selection</param>
        public void AddFunction(string symbol, TGPTerminal.FunctorHandle function, double weight = 1)
        {
            TGPTerminal terminal = new TGPTerminal("");
            terminal.FunctionHandle = function;
            terminal.Value = null;
            AddTerminal(terminal);
            terminal.mSymbol = symbol;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mTerminals.Count; ++i)
            {
                sb.AppendFormat("terminal[{0}]: {1}\n", i, mTerminals[i].Key);
            }
            return sb.ToString();
        }

       

        internal void Update(params object[] tags)
        {
            foreach (KeyValuePair<TGPTerminal, double> pair in mTerminals)
            {
                TGPTerminal terminal = pair.Key;
                if (terminal.FunctionHandle != null)
                {
                    terminal.Value=terminal.FunctionHandle(tags);
                }
            }
        }
    }
}
