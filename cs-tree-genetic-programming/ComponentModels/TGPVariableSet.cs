using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    public class TGPVariableSet
    {
        private Dictionary<string, KeyValuePair<TGPTerminal, double>> mTerminals = new Dictionary<string, KeyValuePair<TGPTerminal, double>>();
        private double mWeightSum = 0;

        public TGPVariableSet()
        {

        }

        public void AddVariables(params TGPTerminal[] terminals)
        {
            foreach (TGPTerminal terminal in terminals)
            {
                AddTerminal(terminal);
            }
        }

        public void AddVariable(string symbol, double weight=1)
        {
            AddTerminal(new TGPTerminal(symbol), weight);
        }

        public TGPVariableSet Clone()
        {
            TGPVariableSet clone = new TGPVariableSet();
            clone.mWeightSum = mWeightSum;
            foreach (KeyValuePair<TGPTerminal, double> point in mTerminals.Values)
            {
                clone.mTerminals[point.Key.Symbol]=new KeyValuePair<TGPTerminal, double>(point.Key.Clone(), point.Value);
            }
            return clone;
        }

        public TGPTerminal FindRandomTerminal(TGPTerminal current_terminal=null)
        {
            for(int attempts=0; attempts < 10; attempts++)
            {
                double r = mWeightSum * TreeGP.Distribution.DistributionModel.GetUniform();

                double current_sum = 0;
                foreach (KeyValuePair<TGPTerminal, double> point in mTerminals.Values)
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

        public TGPTerminal FindTerminalBySymbol(string terminal_index)
        {
            return mTerminals[terminal_index].Key;
        }

        public double FindTerminalWeightBySymbol(string terminal_index)
        {
            return mTerminals[terminal_index].Value;
        }

        public int TerminalCount
        {
            get { return mTerminals.Count; }
        }

        public List<string> TerminalNames
        {
            get { return mTerminals.Keys.ToList(); }
        }

        public void AddTerminal(TGPTerminal terminal, double weight = 1)
        {
            mTerminals[terminal.Symbol]=new KeyValuePair<TGPTerminal, double>(terminal, weight);
            terminal.mTerminalIndex = mTerminals.Count - 1;
            terminal.mIsConstant = false;
            mWeightSum += weight;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string symbol in mTerminals.Keys)
            {
                sb.AppendFormat("terminal[{0}]: {1}\n", symbol, mTerminals[symbol].Key);
            }
            return sb.ToString();
        }
    }
}
