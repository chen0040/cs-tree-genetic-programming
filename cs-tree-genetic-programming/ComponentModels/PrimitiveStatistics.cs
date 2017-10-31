using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    using TreeGP.Core.ComponentModels;
    public class PrimitiveStatistics<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        private List<string> mPrimitives = new List<string>();
        private List<string> mVariables = new List<string>();

        private Dictionary<string, int> mPrimitiveIdentifiers = new Dictionary<string, int>();
        private Dictionary<string, int> mVariableIdentifiers = new Dictionary<string, int>();

        private List<int[]> mPrimitiveCountMatrix = new List<int[]>();
        private List<int[]> mVariableCountMatrix = new List<int[]>();
        private List<double[]> mFitnessMatrix = new List<double[]>();
        private Dictionary<int, bool> mIsVariable = new Dictionary<int, bool>();

        public delegate double SolutionFitnessRequestedHandler(S solution);
        public event SolutionFitnessRequestedHandler SolutionFitnessRequested;

        private double FindSolutionFitness(S solution)
        {
            if (SolutionFitnessRequested != null)
            {
                return SolutionFitnessRequested(solution);
            }
            return 0;
        }

        public void Clear()
        {
            mPrimitiveCountMatrix.Clear();
            mFitnessMatrix.Clear();
            mPrimitives.Clear();
            mVariables.Clear();
            mPrimitiveIdentifiers.Clear();
            mVariableIdentifiers.Clear();
            mIsVariable.Clear();
            mVariableCountMatrix.Clear();
        }

        public PrimitiveStatistics<P, S> Clone()
        {
            PrimitiveStatistics<P, S> clone = new PrimitiveStatistics<P, S>();
            foreach (string primitive_name in mPrimitives)
            {
                clone.mPrimitives.Add(primitive_name);
            }
            foreach (string variable_name in mVariables)
            {
                clone.mVariables.Add(variable_name);
            }
            foreach (string primitive_name in mPrimitiveIdentifiers.Keys)
            {
                clone.mPrimitiveIdentifiers[primitive_name] = mPrimitiveIdentifiers[primitive_name];
            }
            foreach (string variable_name in mVariableIdentifiers.Keys)
            {
                clone.mVariableIdentifiers[variable_name] = mVariableIdentifiers[variable_name];
            }
            foreach (int[] count_row in mPrimitiveCountMatrix)
            {
                int[] clone_count_row = count_row.ToArray();
                clone.mPrimitiveCountMatrix.Add(clone_count_row);
            }
            foreach (int[] count_row in mVariableCountMatrix)
            {
                int[] clone_count_row = count_row.ToArray();
                clone.mVariableCountMatrix.Add(clone_count_row);
            }
            foreach (double[] count_row in mFitnessMatrix)
            {
                double[] clone_count_row = count_row.ToArray();
                clone.mFitnessMatrix.Add(clone_count_row);
            }
            foreach (int primitive_index in mIsVariable.Keys)
            {
                clone.mIsVariable[primitive_index] = mIsVariable[primitive_index];
            }

            return clone;
        }

        public void Analyze(List<S> solutions)
        {
            int[] primitive_count_array = new int[mPrimitives.Count];
            int[] variable_count_array = new int[mVariables.Count];
            double[] variable_fitness_array = new double[mVariables.Count];

            for (int i = 0; i < primitive_count_array.Length; ++i)
            {
                primitive_count_array[i] = 0;
            }
            for(int i=0; i < variable_fitness_array.Length; ++i)
            {
                variable_fitness_array[i]=0;
                variable_count_array[i] = 0;
            }

            double average_fitness=0;
            foreach(S solution in solutions)
            {
                average_fitness+=FindSolutionFitness(solution);
            }
            if (solutions.Count > 0)
            {
                average_fitness /= solutions.Count;
            }

            foreach (S solution in solutions)
            {
                double fitness = FindSolutionFitness(solution);
                if (fitness < average_fitness)
                {
                    fitness = 0;
                }

                for (int i = 0; i < solution.ProgramCount; ++i)
                {
                    TGPProgram p = solution.FindProgramAt(i) as TGPProgram;
                    List<TGPPrimitive> plist = p.FlattenPrimitives();
                    int variable_count = 0;
                    foreach (TGPPrimitive primitive in plist)
                    {
                        if (IsVariable(primitive))
                        {
                            variable_count++;
                        }
                    }

                    double primitive_fitness = fitness;
                    
                    if (variable_count > 0)
                    {
                        primitive_fitness /= variable_count;
                    }
                    foreach (TGPPrimitive te in plist)
                    {
                        int id = mPrimitiveIdentifiers[te.Symbol];
                        primitive_count_array[id] += 1;
                        if (mVariableIdentifiers.ContainsKey(te.Symbol))
                        {
                            id = mVariableIdentifiers[te.Symbol];
                            variable_fitness_array[id] += primitive_fitness;
                            variable_count_array[id] += 1;
                        }
                    }
                }
            }
            mPrimitiveCountMatrix.Add(primitive_count_array);
            mVariableCountMatrix.Add(variable_count_array);
            mFitnessMatrix.Add(variable_fitness_array);
        }

        public List<string> PrimitiveNames
        {
            get { return mPrimitives; }
        }

        public List<string> VariableNames
        {
            get { return mVariables; }
        }

        public List<int[]> PrimitiveFrequencyTimeSeries
        {
            get
            {
                int dim1=GenerationCount;
                int dim2=PrimitiveCount;
                List<int[]> points=new List<int[]>();
                for (int i = 0; i < dim1; ++i)
                {
                    for (int j = 0; j < dim2; ++j)
                    {
                        int count=mPrimitiveCountMatrix[i][j];
                        int[] point = new int[3];
                        point[0] = i;
                        point[1] = j;
                        point[2] = count;
                        points.Add(point);
                    }
                }
                return points;
            }
        }

        public List<int[]> VariableFrequencyTimeSeries
        {
            get
            {
                int dim1 = GenerationCount;
                int dim2 = VariableCount;
                List<int[]> points = new List<int[]>();
                for (int i = 0; i < dim1; ++i)
                {
                    for (int j = 0; j < dim2; ++j)
                    {
                        int count = mVariableCountMatrix[i][j];
                        int[] point = new int[3];
                        point[0] = i;
                        point[1] = j;
                        point[2] = count;
                        points.Add(point);
                    }
                }
                return points;
            }
        }

        public List<double[]> VariableFitnessTimeSeries
        {
            get
            {
                int dim1 = GenerationCount;
                int dim2 = VariableCount;
                List<double[]> points = new List<double[]>();
                for (int i = 0; i < dim1; ++i)
                {
                    for (int j = 0; j < dim2; ++j)
                    {
                        double fitness = mFitnessMatrix[i][j];
                        double[] point = new double[3];
                        point[0] = i;
                        point[1] = j;
                        point[2] = fitness;
                        points.Add(point);
                    }
                }
                return points;
            }
        }

        public List<double[]> VariableFitnessMatrix
        {
            get
            {
                return mFitnessMatrix;
            }
        }

        public int VariableCount
        {
            get
            {
                return mVariables.Count;
            }
        }

        public int PrimitiveCount
        {
            get
            {
                return mPrimitives.Count;
            }
        }

        public int GenerationCount
        {

            get
            {
                return mPrimitiveCountMatrix.Count;
            }
        }

        public int FindCount(int generation, int primitive_id)
        {
            return mPrimitiveCountMatrix[generation][primitive_id];
        }

        public bool IsVariable(TGPPrimitive primitive)
        {
            if (primitive.IsTerminal)
            {
                TGPTerminal terminal = primitive as TGPTerminal;
                if (!terminal.IsConstant)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddPrimitive(TGPPrimitive primitive)
        {
            mPrimitiveIdentifiers[primitive.Symbol] = mPrimitives.Count;
            mPrimitives.Add(primitive.Symbol);

            if (IsVariable(primitive))
            {
                mVariableIdentifiers[primitive.Symbol] = mVariables.Count;
                mVariables.Add(primitive.Symbol);
            }
        }

        public static List<int[]> GetPrimitiveFrequencyFromGPSolutions(List<S> solutions, Dictionary<S, PrimitiveStatistics<P, S>> stats_archive)
        {
            List<int[]> points = new List<int[]>();

            foreach (S gs in solutions)
            {
                List<int[]> points2 = stats_archive[gs].PrimitiveFrequencyTimeSeries;
                int points2_count = points2.Count;
                if (points.Count == 0)
                {
                    foreach (int[] points2_entry in points2)
                    {
                        int[] points_entry = new int[points2_entry.Length];
                        for (int i = 0; i < points2_entry.Length; ++i)
                        {
                            points_entry[i] = points2_entry[i];
                        }
                        points.Add(points_entry);
                    }
                }
                else
                {
                    int points_count = points.Count;
                    for (int points_entry_index = 0; points_entry_index < points_count; ++points_entry_index)
                    {
                        if (points_entry_index >= points2_count) break;
                        int[] points2_entry = points2[points_entry_index];
                        int[] points_entry = points[points_entry_index];
                        for (int i = 0; i < points_entry.Length; ++i)
                        {
                            if (i >= points2_entry.Length) break;
                            points_entry[i] += points2_entry[i];
                        }
                    }

                }
            }

            for (int points_entry_index = 0; points_entry_index < points.Count; ++points_entry_index)
            {
                int[] points_entry = points[points_entry_index];
                for (int i = 0; i < points_entry.Length; ++i)
                {
                    points_entry[i] /= solutions.Count;
                }
            }

            return points;
        }

        public static List<int[]> GetVariableFrequencyFromGPSolutions(List<S> solutions, Dictionary<S, PrimitiveStatistics<P, S>> stats_archive)
        {
            List<int[]> points = new List<int[]>();

            foreach (S gs in solutions)
            {
                List<int[]> points2 = stats_archive[gs].VariableFrequencyTimeSeries;
                int points2_count = points2.Count;
                if (points.Count == 0)
                {
                    foreach (int[] points2_entry in points2)
                    {
                        int[] points_entry = new int[points2_entry.Length];
                        for (int i = 0; i < points2_entry.Length; ++i)
                        {
                            points_entry[i] = points2_entry[i];
                        }
                        points.Add(points_entry);
                    }
                }
                else
                {
                    int points_count = points.Count;
                    for (int points_entry_index = 0; points_entry_index < points_count; ++points_entry_index)
                    {
                        if (points_entry_index >= points2_count) break;
                        int[] points2_entry = points2[points_entry_index];
                        int[] points_entry = points[points_entry_index];
                        for (int i = 0; i < points_entry.Length; ++i)
                        {
                            if (i >= points2_entry.Length) break;
                            points_entry[i] += points2_entry[i];
                        }
                    }

                }
            }

            for (int points_entry_index = 0; points_entry_index < points.Count; ++points_entry_index)
            {
                int[] points_entry = points[points_entry_index];
                for (int i = 0; i < points_entry.Length; ++i)
                {
                    points_entry[i] /= solutions.Count;
                }
            }

            return points;
        }
    }
}
