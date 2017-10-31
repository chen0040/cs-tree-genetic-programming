using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGP.ComponentModels.Operators;

namespace TreeGP.ComponentModels
{
    public class TGPNode
    {
        protected TGPNode mParent=null;
        protected TGPPrimitive mData = null;
        protected List<TGPNode> mChildNodes = new List<TGPNode>();

        public TGPNode(TGPPrimitive data, TGPNode parent=null)
        {
            mData = data;
            mParent = parent;
        }

        public void RemoveAllChildren()
        {
            mChildNodes.Clear();
        }

        public int Arity
        {
            get{return mData.Arity;}
        }

        public TGPNode CreateChild(TGPPrimitive data)
        {
            TGPNode node = new TGPNode(data, this);
            mChildNodes.Add(node);
            return node;
        }

        public int FindLength()
        {
            int lengthSoFar = 1;

            foreach (TGPNode child_node in mChildNodes)
            {
                lengthSoFar += child_node.FindLength();
            }

            return lengthSoFar;
        }

        public int FindDepth(int depthSoFar=0)
        {
            int maxDepthOfChild = depthSoFar;

            foreach (TGPNode child_node in mChildNodes)
            {
                int d = child_node.FindDepth(depthSoFar + 1);
                if (d > maxDepthOfChild)
                {
                    maxDepthOfChild = d;
                }
            }

            return maxDepthOfChild;
        }

        public virtual double Evaluate(params object[] tags)
        {
            int node_count = Arity;
            for (int i = 0; i < node_count; ++i )
            {
                mData[i]=mChildNodes[i].Evaluate(tags);
            }
            mData.Evaluate(tags);
            return mData.DoubleValue;
        }

        public virtual TGPNode Clone()
        {
            TGPNode clone = new TGPNode(mData, mParent);
            foreach (TGPNode child_node in mChildNodes)
            {
                TGPNode cloned_child = child_node.Clone();
                cloned_child.mParent = clone;
                clone.mChildNodes.Add(cloned_child);
            }
            
            return clone;
        }

        public TGPNode FindChildByIndex(int iChild)
        {
            return mChildNodes[iChild];
        }

        public void ReplaceChildAt(int index, TGPNode child)
        {
            mChildNodes[index] = child;
            child.mParent = this;
        }

        public int IndexOfChild(TGPNode child)
        {
            for (int i = 0; i < mChildNodes.Count; ++i)
            {
                if (mChildNodes[i] == child)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Swap(TGPNode point2)
        {
            TGPNode parent1 = this.mParent;
            TGPNode parent2 = point2.mParent;

            if (parent1 == null || parent2 == null)
            {
                TGPPrimitive content1 = this.mData;
                TGPPrimitive content2 = point2.mData;
                this.mData = content2;
                point2.mData = content1;
                List<TGPNode> children1 = this.mChildNodes.ToList();
                List<TGPNode> children2 = point2.mChildNodes.ToList();
                this.RemoveAllChildren();
                point2.RemoveAllChildren();
                for (int i = 0; i < children1.Count; ++i)
                {
                    point2.mChildNodes.Add(children1[i]);
                    children1[i].Parent = point2;
                }
                for (int i = 0; i < children2.Count; ++i)
                {
                    this.mChildNodes.Add(children2[i]);
                    children2[i].Parent = this;
                }
            }
            else
            {
                int child_index1 = parent1.IndexOfChild(this);
                int child_index2 = parent2.IndexOfChild(point2);

                parent1.ReplaceChildAt(child_index1, point2);
                parent2.ReplaceChildAt(child_index2, this);
            }
            
        }

        public bool IsTerminal
        {
            get { return mData.IsTerminal; }
        }

        public void RemoveChild(TGPNode child)
        {
            mChildNodes.Remove(child);
            child.Parent = null;
        }

        public TGPPrimitive Primitive
        {
            get { return mData; }
            set { mData = value; }
        }

        public TGPNode Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        public override string ToString()
        {
            if (mData.IsTerminal)
            {
                return mData.ToString();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("({0}", mData.Symbol);
                for (int i = 0; i < Arity; ++i)
                {
                    sb.AppendFormat(" {0}", mChildNodes[i]);
                }
                sb.Append(")");
                return sb.ToString();
            }
            
        }

        public string MathExpression
        {
            get
            {
                if (mData.IsTerminal)
                {
                    return mData.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (Arity == 1)
                    {
                        sb.AppendFormat("{0}({1})", mData.Symbol, mChildNodes[0].MathExpression);
                        
                    }
                    else if (Arity==2)
                    {
                        if (mChildNodes[0].Primitive.IsTerminal)
                        {
                            sb.AppendFormat("{0}", mChildNodes[0].MathExpression);

                        }
                        else
                        {
                            sb.AppendFormat("({0})", mChildNodes[0].MathExpression);
                        }
                        sb.AppendFormat(" {0} ", mData.Symbol);
                        if (mChildNodes[1].Primitive.IsTerminal)
                        {
                            sb.AppendFormat("{0}", mChildNodes[1].MathExpression);

                        }
                        else
                        {
                            sb.AppendFormat("({0})", mChildNodes[1].MathExpression);
                        }
                    }
                    else if (Arity == 4)
                    {
                        if (mData.Symbol == "if<")
                        {
                            sb.AppendFormat("if({0} < {1}, {2}, {3})", 
                                mChildNodes[0].MathExpression,
                                mChildNodes[1].MathExpression,
                                mChildNodes[2].MathExpression,
                                mChildNodes[3].MathExpression);
                        }
                        else if (mData.Symbol == "if>")
                        {
                            sb.AppendFormat("if({0} > {1}, {2}, {3})",
                               mChildNodes[0].MathExpression,
                               mChildNodes[1].MathExpression,
                               mChildNodes[2].MathExpression,
                               mChildNodes[3].MathExpression);
                        }
                        else
                        {
                            sb.AppendFormat("{0}(", mData.Symbol);
                            for (int i = 0; i < Arity; ++i)
                            {
                                if (i != 0)
                                {
                                    sb.Append(", ");
                                }

                                sb.AppendFormat("{0}", mChildNodes[i].MathExpression);
                            }
                            sb.Append(")");
                        }
                        
                    }
                    else
                    {
                        sb.AppendFormat("{0}", mData.Symbol);
                        for (int i = 0; i < Arity; ++i)
                        {
                            if (mChildNodes[i].Primitive.IsTerminal)
                            {
                                sb.AppendFormat(" {0}", mChildNodes[i].MathExpression);

                            }
                            else
                            {
                                sb.AppendFormat(" ({0})", mChildNodes[i].MathExpression);
                            }
                        }
                    }
                   
                    return sb.ToString();
                }
            }
        }

        internal void Flatten(List<TGPPrimitive> list)
        {
            list.Add(mData);
            for (int i = 0; i < Arity; ++i)
            {
                mChildNodes[i].Flatten(list);
            }
        }

        internal void Flatten(List<TGPNode> list)
        {
            list.Add(this);
            for (int i = 0; i < Arity; ++i)
            {
                mChildNodes[i].Flatten(list);
            }
        }

        internal int FindDepth2Node(TGPNode node, int depthSoFar=0)
        {
            if (this == node)
            {
                return depthSoFar;
            }

            int maxDepthOfChild = -1;
            foreach (TGPNode child_node in mChildNodes)
            {
                int d = child_node.FindDepth2Node(node, depthSoFar + 1);
                if (d > maxDepthOfChild)
                {
                    maxDepthOfChild = d;
                } 
            }

            return maxDepthOfChild;
        }
    }
}
