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
    /// Population Initialization method following the "Ramped Grow" method described in Section 2.2 of "A Field Guide to Genetic Programming"
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPPopInitInstruction_RampedGrow<P, S> : PopInitInstruction<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public TGPPopInitInstruction_RampedGrow()
        {

        }

        public TGPPopInitInstruction_RampedGrow(XmlElement xml_level1)
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
            int part_count = iMaximumDepthForCreation - 1;

            int interval = iPopulationSize / part_count;

            for (int i = 0; i < part_count; i++)
            {
                for (int j = 0; j < interval; ++j)
                {
                    S program = (S)pop.CreateSolution();
                    program.CreateWithDepth(i + 1, TGPProgram.INITIALIZATION_METHOD_GROW);
                    pop.AddSolution(program);
                }
                
            }

            int pop_count = pop.SolutionCount;

            
            for (int i = pop_count; i < iPopulationSize; ++i)
            {
                S program = (S)pop.CreateSolution();
                program.CreateWithDepth(iMaximumDepthForCreation, TGPProgram.INITIALIZATION_METHOD_GROW);
                pop.AddSolution(program);
            }

            
        }

        public override PopInitInstruction<P, S> Clone()
        {
            TGPPopInitInstruction_RampedGrow<P, S> clone = new TGPPopInitInstruction_RampedGrow<P, S>();
            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(">> Name: TGPPopInitInstruction_RampedGrow\n");
            return sb.ToString();
        }
    }
}
