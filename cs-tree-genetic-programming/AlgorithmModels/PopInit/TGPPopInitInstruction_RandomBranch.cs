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
    ///  Population Initialization method following the "RandomBranch" method described in "Kumar Chellapilla. Evolving computer programs without subtree crossover. IEEE Transactions on Evolutionary Computation, 1(3):209–216, September 1997."
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPPopInitInstruction_RandomBranch<P, S> : PopInitInstruction<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public TGPPopInitInstruction_RandomBranch()
        {

        }

        public TGPPopInitInstruction_RandomBranch(XmlElement xml_level1)
            : base(xml_level1)
        {
            foreach (XmlElement xml_level2 in xml_level1.ChildNodes)
            {
                if (xml_level2.Name == "param")
                {
                    string attrname = xml_level2.Attributes["name"].Value;
                    string attrvalue = xml_level2.Attributes["value"].Value;
                   
                }
            }
        }

        public override void Initialize(P pop)
        {
	        int iPopulationSize=pop.PopulationSize;

            int iTreeSize = pop.MaximumDepthForCreation;

	        for(int i=0; i<iPopulationSize; i++)
	        {
                S solution = (S)pop.CreateSolution();
                solution.CreateWithDepth(iTreeSize, TGPProgram.INITIALIZATION_METHOD_RANDOMBRANCH);

                pop.AddSolution(solution);
	        }
        }

        public override PopInitInstruction<P, S> Clone()
        {
            TGPPopInitInstruction_RandomBranch<P, S> clone = new TGPPopInitInstruction_RandomBranch<P, S>();
            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(">> Name: TGPPopInitInstruction_RandomBranch\n");

            return sb.ToString();
        }
    }
}
