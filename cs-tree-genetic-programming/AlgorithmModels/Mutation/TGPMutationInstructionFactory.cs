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

    public class TGPMutationInstructionFactory<P, S> : MutationInstructionFactory<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public TGPMutationInstructionFactory()
        {

        }

        public override string InstructionType
        {
            set
            {
                if (value == TGPProgram.MUTATION_SUBTREE)
                {
                    mCurrentInstruction = new TGPMutationInstruction_Subtree<P, S>();
                }
                else if (value == TGPProgram.MUTATION_HOIST)
                {
                    mCurrentInstruction = new TGPMutationInstruction_Hoist<P, S>();
                }
                else if (value == TGPProgram.MUTATION_SUBTREE_KINNEAR)
                {
                    mCurrentInstruction = new TGPMutationInstruction_Kinnear<P, S>();
                }
                
                else if (value == TGPProgram.MUTATION_SHRINK)
                {
                    mCurrentInstruction = new TGPMutationInstruction_Shrink<P, S>();
                }
            }
        }

        public TGPMutationInstructionFactory(string filename)
            : base(filename)
        {
            
        }

        protected override MutationInstruction<P, S> LoadInstructionFromXml(string selected_strategy, XmlElement xml)
        {
            if (selected_strategy == TGPProgram.MUTATION_SUBTREE)
            {
                return new TGPMutationInstruction_Subtree<P, S>(xml);
            }
            else if (selected_strategy == TGPProgram.MUTATION_SUBTREE_KINNEAR)
            {
                return new TGPMutationInstruction_Kinnear<P, S>(xml);
            }
            else if (selected_strategy == TGPProgram.MUTATION_HOIST)
            {
                return new TGPMutationInstruction_Hoist<P, S>(xml);
            }
            else if (selected_strategy == TGPProgram.MUTATION_SHRINK)
            {
                return new TGPMutationInstruction_Shrink<P, S>(xml);
            }

            return LoadDefaultInstruction();
        }

        protected override MutationInstruction<P, S> LoadDefaultInstruction()
        {
            return new TGPMutationInstruction_Subtree<P, S>();
        }

        public override MutationInstructionFactory<P, S> Clone()
        {
            TGPMutationInstructionFactory<P, S> clone = new TGPMutationInstructionFactory<P, S>(mFilename);
            return clone;
        }
    }
}
