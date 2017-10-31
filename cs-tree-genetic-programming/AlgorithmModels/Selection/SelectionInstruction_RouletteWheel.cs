using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGP.Core.ComponentModels;
using TreeGP.ComponentModels;

namespace TreeGP.AlgorithmModels.Selection
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Distribution;
    using TreeGP.Core.AlgorithmModels.Selection;

    /// <summary>
    /// Reproduction selection mechanisms that implements the roulette wheel selection based on the fitness of the GP solution
    /// </summary>
    /// <typeparam name="S">The generic type of a GP solution</typeparam>
    public class SelectionInstruction_RouletteWheel<S> : SelectionInstruction_RouletteWheel<IGPPop, S>
        where S : TGPSolution
    {
        
        public SelectionInstruction_RouletteWheel()
        {

        }

        public SelectionInstruction_RouletteWheel(XmlElement xml_level1)
            : base(xml_level1)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class SelectionInstruction_RouletteWheel<P, S> : SelectionInstruction<P, S>
        where P : IPop
        where S : TGPSolution
    {
        
        public SelectionInstruction_RouletteWheel()
        {

        }

        public SelectionInstruction_RouletteWheel(XmlElement xml_level1)
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

        public override S Select(P pop, Comparison<S> comparer)
        {
            int solution_count = pop.SolutionCount;
            double sum_fitness = 0;
            double min_fitness = double.MaxValue;
            for (int i = 0; i < solution_count; ++i)
            {
                S s = pop.FindSolutionByIndex(i) as S;
                double fitness = s.Fitness;
                if (fitness < min_fitness)
                {
                    min_fitness = fitness;
                }
            }

            List<double> acc_fitnesses = new List<double>();
            for (int i = 0; i < solution_count; ++i)
            {
                S s = pop.FindSolutionByIndex(i) as S;
                double fitness = s.Fitness;

                sum_fitness += (fitness - min_fitness);
                acc_fitnesses.Add(sum_fitness);
            }

            double r = DistributionModel.GetUniform() * sum_fitness;
            for (int i = 0; i < solution_count; ++i)
            {
                if (acc_fitnesses[i] >= r)
                {
                    return pop.FindSolutionByIndex(i) as S;
                }
            }
            return null; 
        }

        public void RandomShuffle(List<ISolution> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                ISolution value = list[k];
                list[k] = list[n];
                list[n] = value;
            }  
        }

        public override void Select(P pop, List<S> best_pair, List<S> worst_pair, int tournament_count,  Comparison<S> comparer)
        {
            int solution_count = pop.SolutionCount;
            double sum_fitness = 0;
            double min_fitness = double.MaxValue;
            for (int i = 0; i < solution_count; ++i)
            {
                S s = pop.FindSolutionByIndex(i) as S;
                double fitness = s.Fitness;
                if (fitness < min_fitness)
                {
                    min_fitness = fitness;
                }
            }

            List<double> acc_fitnesses = new List<double>();
            for (int i = 0; i < solution_count; ++i)
            {
                S s = pop.FindSolutionByIndex(i) as S;
                double fitness = s.Fitness;

                sum_fitness += (fitness - min_fitness);
                acc_fitnesses.Add(sum_fitness);
            }

            for (int tindex = 0; tindex < tournament_count; ++tindex)
            {
                double r = DistributionModel.GetUniform() * sum_fitness;
                for (int i = 0; i < solution_count; ++i)
                {
                    if (acc_fitnesses[i] >= r)
                    {
                        best_pair.Add(pop.FindSolutionByIndex(i) as S);
                    }
                }
            }
        }

        public override SelectionInstruction<P, S> Clone()
        {
            SelectionInstruction_RouletteWheel<P, S> clone = new SelectionInstruction_RouletteWheel<P, S>();
            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(">> Name: SelectionInstruction_RouletteWheel");

            return sb.ToString();
        }
    }
}
