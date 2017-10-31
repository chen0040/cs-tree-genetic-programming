using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.AlgorithmModels.PopInit
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Core.AlgorithmModels.PopInit;
    using TreeGP.ComponentModels;
    using TreeGP.Distribution;

    /// <summary>
    ///  Population Initialization method following the "PTC1" method described in "Sean Luke. Two fast tree-creation algorithms for genetic programming. IEEE Transactions in Evolutionary Computation, 4(3), 2000b."
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPPopInitInstruction_PTC1<P, S> : PopInitInstruction<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        private int mExpectedTreeSize=5;
        public int ExpectedTreeSize
        {
            get { return mExpectedTreeSize; }
            set { mExpectedTreeSize = value; }
        }

        public TGPPopInitInstruction_PTC1()
        {

        }

        public TGPPopInitInstruction_PTC1(XmlElement xml_level1)
            : base(xml_level1)
        {
            
        }

        public override void Initialize(P pop)
        {
	        int iPopulationSize=pop.PopulationSize;

            int iMaximumDepthForCreation = pop.MaximumDepthForCreation;

	        for(int i=0; i<iPopulationSize; i++)
	        {
                S program = (S)pop.CreateSolution();
                program.CreateWithDepth(iMaximumDepthForCreation, TGPProgram.INITIALIZATION_METHOD_PTC1, mExpectedTreeSize);

                pop.AddSolution(program);
	        }
        }

        public override PopInitInstruction<P, S> Clone()
        {
            TGPPopInitInstruction_PTC1<P, S> clone = new TGPPopInitInstruction_PTC1<P, S>();
            clone.mExpectedTreeSize = mExpectedTreeSize;
            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(">> Name: TGPPopInstruction_PTC1\n");
            sb.AppendFormat(">> Expected Tree Size: {0}", mExpectedTreeSize);

            return sb.ToString();
        }
    }
}
