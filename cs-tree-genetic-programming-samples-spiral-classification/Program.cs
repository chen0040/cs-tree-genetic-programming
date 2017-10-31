using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.SpiralClassification
{
    using System.IO;
    using System.Data;
    using TreeGP.ComponentModels;
    using TreeGP.ComponentModels.Operators;
    using TreeGP.ComponentModels.Operators.Unary;
    using TreeGP.ComponentModels.Operators.Binary;
    using TreeGP.Distribution;

    class Program
    {
        static DataTable LoadData(string filename)
        {
            DataTable table = new DataTable();
            table.Columns.Add("X");
            table.Columns.Add("Y");
            table.Columns.Add("Label");

            int line_count = 0;
            using (StreamReader reader = new StreamReader(filename))
            {
                string line = reader.ReadLine();
                int.TryParse(line, out line_count);

                while ((line = reader.ReadLine()) != null)
                {
                    string[] elements = line.Split(new char[] { '\t' });

                    double x, y;
                    int label;
                    double.TryParse(elements[0].Trim(), out x);
                    double.TryParse(elements[1].Trim(), out y);
                    int.TryParse(elements[2].Trim(), out label);

                    table.Rows.Add(x, y, label);
                }
            }
            return table;
        }
        static void Main(string[] args)
        {
            DataTable table = LoadData("dataset.txt");

            TGPConfig config = new TGPConfig("TGPConfig.xml");
            //config.CrossoverRate = 0.35;
            //config.MicroMutationRate = 0.15;
            //config.MacroMutationRate = 0.45;
            //config.ReproductionRate = 0.05;
            config.ElitismRatio = 0.1;
            TGPPop<TGPSolution> pop = new TGPPop<TGPSolution>(config);
            pop.PopulationReplacement = TGPPop<TGPSolution>.PopulationReplacementMode.TinyGP;

            pop.OperatorSet.AddOperator(new TGPOperator_Plus());
            pop.OperatorSet.AddOperator(new TGPOperator_Minus());
            pop.OperatorSet.AddOperator(new TGPOperator_Division());
            pop.OperatorSet.AddOperator(new TGPOperator_Multiplication());
            pop.OperatorSet.AddOperator(new TGPOperator_Sin());
            pop.OperatorSet.AddOperator(new TGPOperator_Cos());
            pop.OperatorSet.AddIfgtOperator();

            for (int i = 1; i < 10; ++i)
            {
                pop.ConstantSet.AddConstant(string.Format("C{0}", i), i);
            }

            pop.VariableSet.AddVariable("x");
            pop.VariableSet.AddVariable("y");

            pop.CreateFitnessCase += (index) =>
                {
                    SpiralFitnessCase fitness_case = new SpiralFitnessCase();
                    fitness_case.X = double.Parse(table.Rows[index]["X"].ToString());
                    fitness_case.Y = double.Parse(table.Rows[index]["Y"].ToString());
                    fitness_case.Label = int.Parse(table.Rows[index]["Label"].ToString());

                    return fitness_case;
                };

            pop.GetFitnessCaseCount += () =>
                {
                    return table.Rows.Count;
                };

            pop.EvaluateObjectiveForSolution += (fitness_cases, solution, objective_index) =>
            {
                double fitness = 0;
                for (int i = 0; i < fitness_cases.Count; i++)
                {
                    SpiralFitnessCase fitness_case = (SpiralFitnessCase)fitness_cases[i];
                    int correct_y = fitness_case.Label;
                    int computed_y = fitness_case.ComputedLabel;
                    fitness += (correct_y == computed_y) ? 0 : 1;
                }

                return fitness;
            };


            pop.BreedInitialPopulation();


            while (!pop.IsTerminated)
            {
                pop.Evolve();
                Console.WriteLine("Spiral Classification Generation: {0}", pop.CurrentGeneration);
                Console.WriteLine("Global Fitness: {0}\tCurrent Fitness: {1}", pop.GlobalBestProgram.Fitness, pop.FindFittestProgramInCurrentGeneration().Fitness);
                Console.WriteLine("Global Best Solution:\n{0}", pop.GlobalBestProgram);
            }

            Console.WriteLine(pop.GlobalBestProgram.ToString());
        }
    }
}
