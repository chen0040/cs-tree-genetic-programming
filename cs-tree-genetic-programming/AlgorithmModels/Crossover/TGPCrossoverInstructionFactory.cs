using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.AlgorithmModels.Crossover
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Core.AlgorithmModels.Crossover;
    using TreeGP.ComponentModels;

    public class TGPCrossoverInstructionFactory<P, S> : CrossoverInstructionFactory<P, S>
        where S : IGPSolution
        where P: IGPPop
    {
        public TGPCrossoverInstructionFactory()
            : base()
        {

        }

        public TGPCrossoverInstructionFactory(string filename)
            : base(filename)
        {
           
        }

        public override string InstructionType
        {
            set
            {
                if (value == TGPProgram.CROSSVOER_SUBTREE_NO_BIAS)
                {
                    mCurrentCrossover = new TGPCrossoverInstruction_SubtreeNoBias<P, S>();
                }
                else if (value == TGPProgram.CROSSOVER_SUBTREE_BIAS)
                {
                    mCurrentCrossover = new TGPCrossoverInstruction_SubtreeBias<P, S>();
                }
            }
        }

        protected override CrossoverInstruction<P, S> CreateInstructionFromXml(string strategy_name, XmlElement xml)
        {
            if (strategy_name == TGPProgram.CROSSOVER_SUBTREE_BIAS)
            {
                return new TGPCrossoverInstruction_SubtreeBias<P, S>(xml);
            }
            else if (strategy_name == TGPProgram.CROSSVOER_SUBTREE_NO_BIAS)
            {
                return new TGPCrossoverInstruction_SubtreeNoBias<P, S>(xml);
            }
            return null;
        }
        protected override CrossoverInstruction<P, S> CreateDefaultInstruction()
        {
            return new TGPCrossoverInstruction_SubtreeNoBias<P, S>();
        }

        public override CrossoverInstructionFactory<P, S> Clone()
        {
            TGPCrossoverInstructionFactory<P, S> clone = new TGPCrossoverInstructionFactory<P, S>(mFilename);
            return clone;
        }
    }
}
