using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    using TreeGP.Core.ProblemModels;
    using TreeGP.Distribution;

    /// <summary>
    /// Class that represents the encapsulation of a GP tree
    /// </summary>
    public class TGPProgram
    {
        /// <summary>
        /// Root node of the GP tree
        /// </summary>
        protected TGPNode mRootNode;
        /// <summary>
        /// Function Set contains the operators which are the internal nodes of the GP tree, as specified in the Section 3.2 of "A Field Guide to Genetic Programming"
        /// </summary>
        protected TGPOperatorSet mOperatorSet;
        /// <summary>
        /// Variable Set contains program's external inputs, which typically takes the form of named variables, as specified in Section 3.1 of "A Field Guide to Genetic Programming"
        /// </summary>
        protected TGPVariableSet mVariableSet;
        /// <summary>
        /// Constant Set contains two type of terminals: constant value and function with no arguments, as specified in Section 3.1 of "A Field Guide to Genetic Programming"
        /// </summary>
        protected TGPConstantSet mConstantSet;
        /// Primitive Set which contains all the terminals and functions used to assemble a program
        /// The value of the dictionary returns the accumulative weight such that mPrimitiveSet[0].Value contains the weight of mPrimitiveSet[0].Key
        /// while the mPrimitiveSet[mPrimitiveSet.Count-1].Value contains sum of the weight for all the primitives
        /// </summary>
        protected List<KeyValuePair<TGPPrimitive, double>> mPrimitiveSet;
        
        /// <summary>
        /// Tree size
        /// </summary>
        protected int mLength = 0;
        /// <summary>
        /// Tree depth
        /// </summary>
        protected int mDepth = 0;

        public static string CROSSOVER_SUBTREE_BIAS = "Subtree Bias";
        public static string CROSSVOER_SUBTREE_NO_BIAS = "Subtree No Bias";

        public static string MUTATION_SUBTREE = "Subtree";
        public static string MUTATION_SUBTREE_KINNEAR = "Subtree-Kinnear";
        public static string MUTATION_HOIST = "Hoist";
        public static string MUTATION_SHRINK = "Shrink";

        public static string INITIALIZATION_METHOD_GROW = "Grow";
        public static string INITIALIZATION_METHOD_FULL = "Full";
        public static string INITIALIZATION_METHOD_RANDOMBRANCH = "Random Branch";
        public static string INITIALIZATION_METHOD_PTC1 = "PTC1";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="os">The function set of the GP</param>
        /// <param name="ts">The named variable set of the GP, part of the terminal set</param>
        /// <param name="cs">The part of the terminal  set that contains constants and 0-arity functions</param>
        public TGPProgram(TGPOperatorSet os, TGPVariableSet ts, TGPConstantSet cs, List<KeyValuePair<TGPPrimitive, double>> primitives)
        {
            mOperatorSet = os;
            mVariableSet = ts;
            mConstantSet = cs;
            mPrimitiveSet = primitives;
        }

        /// <summary>
        /// Math expression description of the GP tree, by default, the ToString() prints the GP in a LISP like
        /// language style, which is not very user friendly, this method is used to remedy that
        /// </summary>
        public string MathExpression
        {
            get
            {
                if (mRootNode == null)
                {
                    return "";
                }
                return mRootNode.MathExpression;
            }
        }
        
        /// <summary>
        /// Method that returns a deep-copy clone of the current GP
        /// </summary>
        /// <returns>The deep-copy clone</returns>
        public virtual TGPProgram Clone()
        {
            TGPProgram clone = new TGPProgram(mOperatorSet, mVariableSet, mConstantSet, mPrimitiveSet);
            clone.Copy(this);
            
            return clone;
        }

        /// <summary>
        /// Method that performs deep copy of another GP
        /// </summary>
        /// <param name="rhs">The GP to copy</param>
        public virtual void Copy(TGPProgram rhs)
        {
            mDepth = rhs.mDepth;
            mLength = rhs.mLength;

            if (rhs.mRootNode != null)
            {
                mRootNode = rhs.mRootNode.Clone();
            }
        }


        /// <summary>
        /// Method that randomly select and returns a terminal primitive
        /// </summary>
        /// <returns>The randomly selected terminal primitive</returns>
        public TGPTerminal FindRandomTerminal()
        {
            int variable_count = mVariableSet.TerminalCount;
            int constant_count = mConstantSet.TerminalCount;
            int r = DistributionModel.NextInt(variable_count + constant_count);
            if (r < variable_count)
            {
                return mVariableSet.FindRandomTerminal();
            }
            else
            {
                return mConstantSet.FindTerminalByIndex(r - variable_count);
            }
        }

        /// <summary>
        /// Method that returns whether the current GP is better than another GP in quality
        /// </summary>
        /// <param name="rhs">The GP to compare with</param>
        /// <returns>True if the current GP is better</returns>
        public virtual bool IsBetterThan(TGPProgram rhs)
        {
            if (mDepth < rhs.Depth)
            {
                return true;
            }
            else if (mDepth == rhs.Depth)
            {
                if (mLength < rhs.Length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method that creates a subtree of maximum depth
        /// </summary>
        /// <param name="pRoot">The root node of the subtree</param>
        /// <param name="allowableDepth">The maximum depth</param>
        /// <param name="method">The method used to build the subtree</param>
        public void CreateWithDepth(TGPNode pRoot, int allowableDepth, string method)
        {
            int child_count = pRoot.Arity;

            for (int i = 0; i != child_count; ++i)
            {
                TGPPrimitive primitive = FindRandomPrimitive(allowableDepth, method);
                TGPNode child = pRoot.CreateChild(primitive);

                if (!primitive.IsTerminal)
                {
                    CreateWithDepth(child, allowableDepth - 1, method);
                }
            }
        }

        /// <summary>
        /// Method that executes the GP based on a fitness case and return a value as the output of the GP tree
        /// </summary>
        /// <param name="fitness_case">The fitness case which contains' the program's external inputs and stores the program's output</param>
        /// <returns>The output returned after executing the GP program</returns>
        public virtual object ExecuteOnFitnessCase(IGPFitnessCase fitness_case, params object[] tags)
        {
            mConstantSet.Update(tags);
            List<string> variable_names = mVariableSet.TerminalNames;
            foreach(string variable_name in variable_names)
            {
                object input;
                if (fitness_case.QueryInput(variable_name, out input))
                {
                    mVariableSet.FindTerminalBySymbol(variable_name).Value = input;
                }
                else
                {
                    mVariableSet.FindTerminalBySymbol(variable_name).Value = 0;
                }
            }
            return mRootNode.Evaluate(tags);
        }

        /// <summary>
        /// Method that executes the GP based on the values of the external inputs and return a double value as the output of the GP tree
        /// </summary>
        /// <param name="variables">The GP program's external inputs</param>
        /// <returns>The output returned after executing the GP program</returns>
        public virtual double Execute(Dictionary<string, double> variables, params object[] tags)
        {
            mConstantSet.Update(tags);
            List<string> variable_names = mVariableSet.TerminalNames;
            foreach (string variable_name in variable_names)
            {
                if (variables.ContainsKey(variable_name))
                {
                    mVariableSet.FindTerminalBySymbol(variable_name).Value = variables[variable_name];
                }
                else
                {
                    mVariableSet.FindTerminalBySymbol(variable_name).Value = 0;
                }
            }
            return mRootNode.Evaluate();
        }

        /// <summary>
        /// Method that creates a GP tree with a maximum tree depth
        /// </summary>
        /// <param name="allowableDepth">The maximum tree depth</param>
        /// <param name="method">The name of the method used to create the GP tree</param>
        /// <param name="tag">The additional information used to create the GP tree if any</param>
        public void CreateWithDepth(int allowableDepth, string method, object tag = null)
        {
            //  Population Initialization method following the "RandomBranch" method described in "Kumar Chellapilla. Evolving computer programs without subtree crossover. IEEE Transactions on Evolutionary Computation, 1(3):209–216, September 1997."
            if (method == INITIALIZATION_METHOD_RANDOMBRANCH)
            {
                int s = allowableDepth; //tree size
                TGPOperator non_terminal = FindRandomOperatorWithArityLessThan(s);
                if (non_terminal == null)
                {
                    mRootNode = new TGPNode(FindRandomTerminal());
                }
                else
                {
                    mRootNode = new TGPNode(non_terminal);

                    int b_n = non_terminal.Arity;
                    s = (int)System.Math.Floor((double)s / b_n);
                    RandomBranch(mRootNode, s);
                }
                CalcLength();
                CalcDepth();
            }
            // Population Initialization method following the "PTC1" method described in "Sean Luke. Two fast tree-creation algorithms for genetic programming. IEEE Transactions in Evolutionary Computation, 4(3), 2000b."
            else if(method==INITIALIZATION_METHOD_PTC1)
            {
                int expectedTreeSize=Convert.ToInt32(tag);

                int b_n_sum=0;
                for(int i=0; i < mOperatorSet.OperatorCount; ++i)
                {
                    b_n_sum+=mOperatorSet.FindOperatorByIndex(i).Arity;
                }
                double p= (1- 1.0 / expectedTreeSize) / ((double)b_n_sum / mOperatorSet.OperatorCount);

                TGPPrimitive data=null;
                if(DistributionModel.GetUniform() <=p)
                {
                    data=mOperatorSet.FindRandomOperator();
                }
                else
                {
                    data=FindRandomTerminal();
                }

                mRootNode = new TGPNode(data);
                PTC1(mRootNode, p, allowableDepth-1);

                CalcLength();
                CalcDepth();
            }
            else // handle full and grow method 
            {
                mRootNode = new TGPNode(FindRandomPrimitive(allowableDepth, method));

                CreateWithDepth(mRootNode, allowableDepth - 1, method);

                CalcLength();
                CalcDepth();
            }
        }

        /// <summary>
        /// Population Initialization Method described in "Kumar Chellapilla. Evolving computer programs without subtree crossover. IEEE Transactions on Evolutionary Computation, 1(3):209–216, September 1997."
        /// </summary>
        /// <param name="parent_node"></param>
        /// <param name="s"></param>
        /// <param name="?"></param>
        private void RandomBranch(TGPNode parent_node, int s)
        {
            int child_count = parent_node.Arity;

            for (int i = 0; i != child_count; i++)
            {
                TGPOperator non_terminal=FindRandomOperatorWithArityLessThan(s);
                if (non_terminal == null)
                {
                    TGPNode child = parent_node.CreateChild(FindRandomTerminal());
                }
                else
                {
                    TGPNode child = parent_node.CreateChild(non_terminal);
                    int b_n=non_terminal.Arity;
                    int s_pi = (int)System.Math.Floor((double)s / b_n);
                    RandomBranch(child, s_pi);
                }
            }
        }

        /// <summary>
        /// Population Initialization method following the "PTC1" method described in "Sean Luke. Two fast tree-creation algorithms for genetic programming. IEEE Transactions in Evolutionary Computation, 4(3), 2000b."
        /// </summary>
        /// <param name="parent_node">The node for which the child nodes are generated in this method</param>
        /// <param name="p">expected probability</param>
        /// <param name="allowableDepth">The maximum tree depth</param>
        private void PTC1(TGPNode parent_node, double p, int allowableDepth)
        {
            int child_count = parent_node.Arity;

            for (int i = 0; i != child_count; i++)
            {
                TGPPrimitive data = null;
                if (allowableDepth == 0)
                {
                    data = FindRandomTerminal();
                }
                else if (DistributionModel.GetUniform() <= p)
                {
                    data = mOperatorSet.FindRandomOperator();
                }
                else
                {
                    data = FindRandomTerminal();
                }

                TGPNode child = parent_node.CreateChild(data);

                if(!data.IsTerminal)
                {
                    PTC1(child, p, allowableDepth - 1);
                }
            }
        }

        /// <summary>
        /// Method that is used by the "RandomBranch" initialization algorithm to obtain a random function node with arity less than s
        /// </summary>
        /// <param name="s">The tree size</param>
        /// <returns></returns>
        private TGPOperator FindRandomOperatorWithArityLessThan(int s)
        {
            List<TGPOperator> ops = new List<TGPOperator>();
            for (int i = 0; i < mOperatorSet.OperatorCount; ++i)
            {
                TGPOperator op1=mOperatorSet.FindOperatorByIndex(i);
                if (op1.Arity < s)
                {
                    ops.Add(op1);
                }
            }
            if (ops.Count == 0) return null;
            return ops[DistributionModel.NextInt(ops.Count)];
        }

        /// <summary>
        /// Method that follows the implementation of GP initialization in Algorithm 2.1 of "A Field Guide to Genetic Programming"
        /// </summary>
        /// <param name="allowableDepth">Maximum depth of the GP tree</param>
        /// <param name="method">The initialization method, currently either "Grow" or "Full"</param>
        /// <returns></returns>
        public TGPPrimitive FindRandomPrimitive(int allowableDepth, string method)
        {
            int terminal_count=(mVariableSet.TerminalCount+mConstantSet.TerminalCount);
            int function_count=mOperatorSet.OperatorCount;

            double terminal_prob=(double)terminal_count / (terminal_count + function_count);
            if (allowableDepth <= 0 || (method == INITIALIZATION_METHOD_GROW && DistributionModel.GetUniform() <= terminal_prob))
            {
                return FindRandomTerminal();
            }
            else 
            {
                return mOperatorSet.FindRandomOperator();
            }
        }

        /// <summary>
        /// Method that returns a random primitive based on the weight associated with each primitive
        /// The method is similar to roulette wheel
        /// </summary>
        /// <returns>The randomly selected primitive</returns>
        public TGPPrimitive FindRandomPrimitive()
        {
            int p_count = mPrimitiveSet.Count;
            double sum_weight=mPrimitiveSet[p_count-1].Value;

            double r = DistributionModel.GetUniform() * sum_weight;

            for (int i = 0; i < p_count; ++i)
            {
                if (mPrimitiveSet[i].Value >= r)
                {
                    return mPrimitiveSet[i].Key;
                }
            }

            return null;

        }

        /// <summary>
        /// Method that calculates the size of the tree
        /// </summary>
        /// <returns>The tree size</returns>
        public int CalcLength()
        {
           return mLength = mRootNode.FindLength();
        }

        /// <summary>
        /// Method that calculates the tree depth
        /// </summary>
        /// <returns>The tree depth</returns>
        public int CalcDepth()
        {
            mDepth = mRootNode.FindDepth();
            return mDepth;
        }

        /// <summary>
        /// Method that returns a randomly selected node from the current tree
        /// The tree is first flatten into a list from which a node is randomly selected
        /// </summary>
        /// <returns></returns>
        public TGPNode FindRandomNode(bool bias=false)
        {
            List<TGPNode> nodes=FlattenNodes();
            if (bias)
            {
                if (DistributionModel.GetUniform() <= 0.1) // As specified by Koza, 90% select function node, 10% select terminal node
                {
                    List<TGPNode> terminal_nodes = new List<TGPNode>();
                    foreach (TGPNode node in nodes)
                    {
                        if (node.IsTerminal)
                        {
                            terminal_nodes.Add(node);
                        }
                    }
                    if (terminal_nodes.Count > 0)
                    {
                        return terminal_nodes[DistributionModel.NextInt(terminal_nodes.Count)];
                    }
                    else
                    {
                        return nodes[DistributionModel.NextInt(nodes.Count)];
                    }
                }
                else
                {
                    List<TGPNode> function_nodes = new List<TGPNode>();
                    foreach (TGPNode node in nodes)
                    {
                        if (!node.IsTerminal)
                        {
                            function_nodes.Add(node);
                        }
                    }
                    if (function_nodes.Count > 0)
                    {
                        return function_nodes[DistributionModel.NextInt(function_nodes.Count)];
                    }
                    else
                    {
                        return nodes[DistributionModel.NextInt(nodes.Count)]; 
                    }
                }
            }
            else
            {
                return nodes[DistributionModel.NextInt(nodes.Count)];
            }
            
        }

        /// <summary>
        /// Root node of the tree
        /// </summary>
        public TGPNode RootNode
        {
            get { return mRootNode; }
        }

        /// <summary>
        /// Method that implements the "Point Mutation" described in Section 2.4 of "A Field Guide to Genetic Programming"
        /// In Section 5.2.2 of "A Field Guide to Genetic Programming", this is also described as node replacement mutation
        /// </summary>
        public virtual void MicroMutate()
        {
            TGPNode node=FindRandomNode();

            if (node.IsTerminal)
            {
                TGPTerminal terminal = FindRandomTerminal();
                int trials = 0;
                int max_trials = 50;
                while (node.Primitive == terminal)
                {
                    terminal = FindRandomTerminal();
                    trials++;
                    if (trials > max_trials) break;
                }
                if (terminal != null)
                {
                    node.Primitive = terminal;
                }
               
            }
            else
            {
                int parameter_count = node.Arity;
                TGPOperator op = mOperatorSet.FindRandomOperator(parameter_count, (TGPOperator)node.Primitive);
                if (op != null)
                {
                    node.Primitive = op;
                }
                
            }
        }

        /// <summary>
        /// Method that implements the subtree crossover described in Section 2.4 of "A Field Guide to Genetic Programming"
        /// </summary>
        /// <param name="rhs">Another tree to be crossover with</param>
        /// <param name="iMaxDepthForCrossover">The maximum depth of the trees after the crossover</param>
        public virtual void SubtreeCrossover(TGPProgram rhs, int iMaxDepthForCrossover, string method, object tag=null)
        {
            if (method == CROSSOVER_SUBTREE_BIAS || method == CROSSVOER_SUBTREE_NO_BIAS)
            {
                bool bias = (method == CROSSOVER_SUBTREE_BIAS);

                int iMaxDepth1 = CalcDepth();
                int iMaxDepth2 = rhs.CalcDepth();

                TGPNode pCutPoint1 = null;
                TGPNode pCutPoint2 = null;

                bool is_crossover_performed = false;
                // Suppose that at the beginning both the current GP and the other GP do not violate max depth constraint
                // then try to see whether a crossover can be performed in such a way that after the crossover, both GP still have depth <= max depth
                if (iMaxDepth1 <= iMaxDepthForCrossover && iMaxDepth2 <= iMaxDepthForCrossover)
                {
                    int max_trials = 50;
                    int trials = 0;
                    do
                    {
                        pCutPoint1 = FindRandomNode(bias);
                        pCutPoint2 = rhs.FindRandomNode(bias);

                        if (pCutPoint1 != null && pCutPoint2 != null)
                        {
                            pCutPoint1.Swap(pCutPoint2);

                            iMaxDepth1 = CalcDepth();
                            iMaxDepth2 = rhs.CalcDepth();

                            if (iMaxDepth1 <= iMaxDepthForCrossover && iMaxDepth2 <= iMaxDepthForCrossover) //crossover is successful
                            {
                                is_crossover_performed = true;
                                break;
                            }
                            else
                            {
                                pCutPoint1.Swap(pCutPoint2); // swap back so as to restore to the original GP trees if the crossover is not valid due to max depth violation
                            }
                        }

                        trials++;
                    } while (trials < max_trials);
                }

                // force at least one crossover even if the maximum depth is violated above so that this operator won't end up like a reproduction operator
                if (!is_crossover_performed)
                {
                    pCutPoint1 = FindRandomNode(bias);
                    pCutPoint2 = rhs.FindRandomNode(bias);

                    if (pCutPoint1 != null && pCutPoint2 != null)
                    {
                        pCutPoint1.Swap(pCutPoint2);


                        CalcLength();
                        rhs.CalcLength();

                    }
                }
            }
            
        }

        /// <summary>
        /// Method that traverse the tree and randomly returns a node from one of the leave or function node
        /// </summary>
        /// <returns>The randomly selected node by traversing</returns>
        public TGPNode FindRandomNodeByTraversing()
        {
            int node_depth = 1;
            return FindRandomNodeByTraversing(mRootNode, ref node_depth);
        }

        /// <summary>
        /// Method that traverse the subtree and randomly returns a node from one of the leave or function node
        /// </summary>        
        /// <param name="pRoot">The root node of a subtree</param>
        /// <param name="node_depth">The depth at which the selected node is returned</param>
        /// <returns>The randomly selected node by traversing</returns>
        public TGPNode FindRandomNodeByTraversing(TGPNode pRoot, ref int node_depth)
        {
            int child_count = pRoot.Arity;
            int current_node_depth = node_depth;

            if (child_count == 0)
            {
                return pRoot;
            }

            TGPNode pSelectedGene = null;
            int selected_child_node_depth = node_depth;
            for (int iChild = 0; iChild != child_count; iChild++)
            {
                TGPNode pChild = pRoot.FindChildByIndex(iChild);
                int child_node_depth = node_depth + 1;
                TGPNode pChildPickedGene = FindRandomNodeByTraversing(pChild, ref child_node_depth);

                if (pChildPickedGene != null)
                {
                    if (pSelectedGene == null)
                    {
                        selected_child_node_depth = child_node_depth;

                        pSelectedGene = pChildPickedGene;
                    }
                    else
                    {
                        double selection_prob = pChildPickedGene.IsTerminal ? 0.1 : 0.9;
                        if (DistributionModel.GetUniform() < selection_prob)
                        {
                            selected_child_node_depth = child_node_depth;

                            pSelectedGene = pChildPickedGene;
                        }
                    }
                }
            }

            if (pSelectedGene == null)
            {
                node_depth = current_node_depth;
                pSelectedGene = pRoot;
            }
            else
            {
                node_depth = selected_child_node_depth;
                if (DistributionModel.GetUniform() < 0.5)
                {
                    node_depth = current_node_depth;
                    pSelectedGene = pRoot;
                }
            }

            return pSelectedGene;
        }

        /// <summary>
        /// Method that implements the subtree mutation or "headless chicken" crossover described in Section 2.4 of "A Field Guide to Genetic Programming"
        /// </summary>
        /// <param name="iMaxProgramDepth">The max depth of the tree after the mutation</param>
        public void Mutate(int iMaxProgramDepth, string method, object tag=null)
        {
            if (method == MUTATION_SUBTREE || method == MUTATION_SUBTREE_KINNEAR)
            {
                TGPNode node = FindRandomNode();

                if (method == MUTATION_SUBTREE)
                {
                    int node_depth = mRootNode.FindDepth2Node(node);
                    node.RemoveAllChildren();

                    node.Primitive = FindRandomPrimitive();

                    if (!node.Primitive.IsTerminal)
                    {
                        int max_depth=iMaxProgramDepth - node_depth;
                        CreateWithDepth(node, max_depth, INITIALIZATION_METHOD_GROW);
                    }
                }
                else
                {
                    int subtree_depth = mRootNode.FindDepth2Node(node);
                    int current_depth = mDepth - subtree_depth;
                    int max_depth = (int)(mDepth * 1.15) - current_depth;

                    node.RemoveAllChildren();
                    node.Primitive = FindRandomPrimitive();

                    if (!node.Primitive.IsTerminal)
                    {
                        CreateWithDepth(node, max_depth, INITIALIZATION_METHOD_GROW);
                    }
                }   
            }
            else if (method == MUTATION_HOIST)
            {
                TGPNode node = FindRandomNode();

                if (node != mRootNode)
                {
                    node.Parent = null;
                    mRootNode = node;
                }
            }
            else if (method == MUTATION_SHRINK)
            {
                TGPNode node = FindRandomNode();
                node.RemoveAllChildren();
                node.Primitive = FindRandomTerminal();
            }
            CalcDepth();
            CalcLength();
        }

        /// <summary>
        /// Method that returns the description of the GP tree, this prints out the GP tree in a LISP like language style
        /// </summary>
        /// <returns>The description of the GP tree</returns>
        public override string ToString()
        {
            if (mRootNode != null)
            {
                return mRootNode.ToString();
            }
            else
            {
                return "GP tree not available!";
            }
        }

        /// <summary>
        /// Tree depth
        /// </summary>
        public int Depth
        {
            get { return mDepth; }
        }

        /// <summary>
        /// Tree size
        /// </summary>
        public int Length
        {
            get { return mLength; }
        }

        /// <summary>
        /// Method that flatten the tree and then store all the primitives of the tree in a list
        /// </summary>
        /// <returns>The list of primitives in the tree</returns>
        public virtual List<TGPPrimitive> FlattenPrimitives()
        {
            List<TGPPrimitive> list = new List<TGPPrimitive>();
            if (mRootNode != null)
            {
                mRootNode.Flatten(list);
            }
            return list;
        }

        /// <summary>
        /// Method that flattens the tree and then stores all the nodes of the tree in a list
        /// </summary>
        /// <returns>The list of nodes in the tree</returns>
        public virtual List<TGPNode> FlattenNodes()
        {
            List<TGPNode> list = new List<TGPNode>();
            if (mRootNode != null)
            {
                mRootNode.Flatten(list);
            }
            return list;
        }

        
    }
}
