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

    /// <summary>
    /// Population Initialization method following the "Grow" method described in Algorithm 2.1 of "A Field Guide to Genetic Programming"
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPPopInitInstruction_Grow<P, S> : PopInitInstruction<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public TGPPopInitInstruction_Grow()
        {

        }

        public TGPPopInitInstruction_Grow(XmlElement xml_level1)
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
            int iPopulationSize = pop.PopulationSize;

            int iMaximumDepthForCreation = pop.MaximumDepthForCreation;


            for (int i = 0; i < iPopulationSize; i++)
            {
                S program = (S)pop.CreateSolution();
                program.CreateWithDepth(iMaximumDepthForCreation, TGPProgram.INITIALIZATION_METHOD_GROW);

                pop.AddSolution(program);
            }
        }

        public override PopInitInstruction<P, S> Clone()
        {
            TGPPopInitInstruction_Grow<P, S> clone = new TGPPopInitInstruction_Grow<P, S>();
           
            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(">> Name: TGPPopInitInstruction_Grow\n");
	       

            return sb.ToString();
        }
    }
}
