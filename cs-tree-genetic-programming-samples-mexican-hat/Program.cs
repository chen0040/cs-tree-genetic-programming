using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.MexicanHat
{
    using System.IO;
    using System.Data;
    using TreeGP.ComponentModels;
    using TreeGP.ComponentModels.Operators.Unary;
    using TreeGP.ComponentModels.Operators.Binary;

    class Program
    {
        static double FunctionXY(double x1, double x2)
        {
            return (1 - x1 * x1 / 4 - x2 * x2 / 4) * System.Math.Exp(-x1 * x2 / 8 - x2 * x2 / 8);
        }

        static DataTable LoadData()
        {
            DataTable table = new DataTable();
            table.Columns.Add("x1");
            table.Columns.Add("x2");
            table.Columns.Add("y");

            double lower_bound = -4;
            double upper_bound = 4;
            int period = 16;

            double interval = (upper_bound - lower_bound) / period;

            for (int i = 0; i < period; i++)
            {
                double x1 = lower_bound + interval * i;
                for (int j = 0; j < period; j++)
                {
                    double x2 = lower_bound + interval * j;
                    table.Rows.Add(x1, x2, FunctionXY(x1, x2));
                }
            }


            return table;
        }
        static void Main(string[] args)
        {
            DataTable table = LoadData();

            TGPConfig config = new TGPConfig("TGPConfig.xml");

            TGPPop<TGPSolution> pop = new TGPPop<TGPSolution>(config);
            pop.OperatorSet.AddOperator(new TGPOperator_Plus());
            pop.OperatorSet.AddOperator(new TGPOperator_Minus());
            pop.OperatorSet.AddOperator(new TGPOperator_Division());
            pop.OperatorSet.AddOperator(new TGPOperator_Multiplication());
            pop.OperatorSet.AddOperator(new TGPOperator_Power());
            pop.OperatorSet.AddIfltOperator();

            for (int i = 1; i < 10; ++i)
            {
                pop.ConstantSet.AddConstant(string.Format("C{0}", i), i);
            }

            pop.VariableSet.AddVariable("X1");
            pop.VariableSet.AddVariable("X2");

            pop.CreateFitnessCase += (index) =>
            {
                MexicanHatFitnessCase fitness_case = new MexicanHatFitnessCase();
                fitness_case.X1 = double.Parse(table.Rows[index]["X1"].ToString());
                fitness_case.X2 = double.Parse(table.Rows[index]["X2"].ToString());
                fitness_case.Y = double.Parse(table.Rows[index]["Y"].ToString());
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
                    MexicanHatFitnessCase fitness_case = (MexicanHatFitnessCase)fitness_cases[i];
                    double correct_y = fitness_case.Y;
                    double computed_y = fitness_case.ComputedY;
                    fitness += (correct_y - computed_y) * (correct_y - computed_y);
                }

                return fitness;
            };


            pop.BreedInitialPopulation();


            while (!pop.IsTerminated)
            {
                pop.Evolve();
                Console.WriteLine("Mexican Hat Symbolic Regression Generation: {0}", pop.CurrentGeneration);
                Console.WriteLine("Global Fitness: {0}\tCurrent Fitness: {1}", pop.GlobalBestProgram.Fitness.ToString("0.000"), pop.FindFittestProgramInCurrentGeneration().Fitness.ToString("0.000"));
                Console.WriteLine("Global Best Solution:\n{0}", pop.GlobalBestProgram);
            }

            Console.WriteLine(pop.GlobalBestProgram.ToString());

        }
    }
}
