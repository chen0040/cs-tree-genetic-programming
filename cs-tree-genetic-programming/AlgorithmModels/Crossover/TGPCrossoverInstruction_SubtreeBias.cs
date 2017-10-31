﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.AlgorithmModels.Crossover
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Core.AlgorithmModels.Crossover;
    using TreeGP.ComponentModels;
    using TreeGP.Distribution;
    
    /// <summary>
    /// Subtree crossover which is a homologous one-point crossover between two GP trees, during crossover, function node has 90% chance to be selected, while terminal node has 10% change to be selected
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPCrossoverInstruction_SubtreeBias<P, S> : CrossoverInstruction<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public TGPCrossoverInstruction_SubtreeBias()
            : base()
        {
           
        }

        public TGPCrossoverInstruction_SubtreeBias(XmlElement xml_level1)
            : base(xml_level1)
        {
            
        }

        public override CrossoverInstruction<P, S> Clone()
        {
            TGPCrossoverInstruction_SubtreeBias<P, S> clone = new TGPCrossoverInstruction_SubtreeBias<P, S>();
            return clone;
        }

        public override List<S> Crossover(P pop, params S[] parents)
        {
            S gp1 = (S)parents[0].Clone();
            S gp2 = (S)parents[1].Clone();

            gp1.SubtreeCrossover(gp2, pop.MaximumDepthForCrossover, TGPProgram.CROSSOVER_SUBTREE_BIAS);

            List<S> children = new List<S>();
            children.Add(gp1);
            children.Add(gp2);

            return children;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(">> Name: TGPCrossoverInstruction_SubtreeBias\n");
            
            return sb.ToString();
        }
    }
}
