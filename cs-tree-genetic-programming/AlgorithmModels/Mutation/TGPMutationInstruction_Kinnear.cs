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
    /// Subtree mutation proposed by Kinnear (1993) (as described in "A Field Guide to Genetic Programming"), which has restriction that
    /// prevents child to have depth more than 15% of its parent's depth
    /// The method is described in Section 5.2.2 of "A Field Guide to Genetic Programming",
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPMutationInstruction_Kinnear<P, S> : MutationInstruction<P, S>
        where S : IGPSolution
        where P: IGPPop
    {
        public TGPMutationInstruction_Kinnear()
        {
            
        }

        public TGPMutationInstruction_Kinnear(XmlElement xml_level1)
            : base(xml_level1)
        {
            
        }

        public override void Mutate(P pop, S child)
        {
            int iMaxProgramDepth = pop.MaximumDepthForMutation;
            child.Mutate(iMaxProgramDepth, TGPProgram.MUTATION_SUBTREE_KINNEAR);
        }

        public override MutationInstruction<P, S> Clone()
        {
            TGPMutationInstruction_Kinnear<P, S> clone = new TGPMutationInstruction_Kinnear<P, S>();
            return clone;
        }
    }
}
