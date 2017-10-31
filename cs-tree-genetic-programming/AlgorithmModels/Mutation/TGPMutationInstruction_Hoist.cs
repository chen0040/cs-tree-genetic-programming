using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.AlgorithmModels.Mutation
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Core.AlgorithmModels.Mutation;
    using TreeGP.ComponentModels;
    using TreeGP.Distribution;

    /// <summary>
    /// Mutation that creates a new offspring individual which is swap the , as specified in Section 5.2.2 of "A Field Guide to  Genetic Programming"
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPMutationInstruction_Hoist<P, S> : MutationInstruction<P, S>
        where S : IGPSolution
        where P: IGPPop
    {
        public TGPMutationInstruction_Hoist()
        {
            
        }

        public TGPMutationInstruction_Hoist(XmlElement xml_level1)
            : base(xml_level1)
        {
            
        }

        public override void Mutate(P pop, S child)
        {
            int iMaxProgramDepth = pop.MaximumDepthForMutation;
            child.Mutate(iMaxProgramDepth, TGPProgram.MUTATION_HOIST);
        }

        public override MutationInstruction<P, S> Clone()
        {
            TGPMutationInstruction_Hoist<P, S> clone = new TGPMutationInstruction_Hoist<P, S>();
            return clone;
        }
    }
}
