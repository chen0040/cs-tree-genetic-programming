using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGP.Core.AlgorithmModels.Selection;

namespace TreeGP.ComponentModels
{
    using TreeGP.Distribution;

    using Core.ComponentModels;

    using AlgorithmModels.Crossover;
    using AlgorithmModels.Mutation;
    using AlgorithmModels.PopInit;

    using TreeGP.Core.ProblemModels;
    using TreeGP.Core.AlgorithmModels.Survival;
    using TreeGP.Core.AlgorithmModels.Selection;
    using TreeGP.Core.AlgorithmModels.Crossover;
    using TreeGP.Core.AlgorithmModels.PopInit;
    using TreeGP.Core.AlgorithmModels.Mutation;
    using System.Threading.Tasks;

    /// <summary>
    /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
    /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", it is common to initialize population randomly using ramped half-and-half with a depth range of 2-6
    /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", the population size should be at least 500 and people often use much larger population
    /// Typically the number of generations is limited to between 10 to 50; the most productive search is usually performed in those early generations and if a solution hasn't been found then, it is unlikely to be found in a reasonable amount of time
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class TGPPop<S> : IGPPop
        where S : TGPSolution, new()
    {
        /// <summary>
        /// To make the gp evaluation parallel task
        /// </summary>
        private bool mParallelEvaluation = false;

        /// <summary>
        /// Function Set contains the set of functions to be used for constructing the internal nodes of a GP tree
        /// </summary>
        protected TGPOperatorSet mOperatorSet = new TGPOperatorSet();
        /// <summary>
        /// Constant Set contains two type of terminals: constant value and function with no arguments (i.e., 0-arity function), as specified in Section 3.1 of "A Field Guide to Genetic Programming"
        /// </summary>
        protected TGPConstantSet mConstantSet = new TGPConstantSet();
        /// <summary>
        /// Variable Set contains program's external inputs, which typically takes the form of named variables, as specified in Section 3.1 of "A Field Guide to Genetic Programming"
        /// </summary>
        protected TGPVariableSet mVariableSet = new TGPVariableSet();
        /// <summary>
        /// Primitive Set which contains all the terminals and functions used to assemble a program
        /// The value of the dictionary returns the accumulative weight such that mPrimitiveSet[0].Value contains the weight of mPrimitiveSet[0].Key
        /// while the mPrimitiveSet[mPrimitiveSet.Count-1].Value contains sum of the weight for all the primitives
        /// </summary>
        protected List<KeyValuePair<TGPPrimitive, double>> mPrimitiveSet = new List<KeyValuePair<TGPPrimitive, double>>();

        /// <summary>
        /// Number of tree count per solution
        /// </summary>
        protected int mTreeCount = 1;

        /// <summary>
        /// A solution creator class used as template to create a new solution
        /// </summary>
        protected S mSolutionFactory = new S();

        /// <summary>
        /// The current population of solution
        /// </summary>
        private List<S> mSolutions = new List<S>();
        /// <summary>
        /// The global best solution obtained during optimization
        /// </summary>
        protected S mGlobalBestProgram = null;

        /// <summary>
        /// The problem environment which create a set of fitness cases during objective evaluation
        /// </summary>
        private IGPEnvironment mEnvironment;
        /// <summary>
        /// Flag determining whether the algorithm environment has been set up
        /// </summary>
        private bool mSetup = false;
        protected int mCurrentGeneration = 0;
        protected TGPConfig mConfig;
        public object Tag;

        protected int mMaxParallelTaskCount = 8;

        private Gaussian mGaussian = new Gaussian();

        protected MutationInstructionFactory<IGPPop, S> mMutationInstructionFactory;
        protected CrossoverInstructionFactory<IGPPop, S> mCrossoverInstructionFactory;
        protected PopInitInstructionFactory<IGPPop, S> mPopInitInstructionFactory;
        protected SelectionInstructionFactory<IGPPop, S> mReproductionSelectionInstructionFactory;
        protected SurvivalInstructionFactory<IGPPop, S> mSurvivalInstructionFactory;

        public delegate double EvaluateObjectiveForSolutionHandle(List<IGPFitnessCase> cases, ISolution solution, int objective_index);
        public event EvaluateObjectiveForSolutionHandle EvaluateObjectiveForSolution;

        /// <summary>
        /// The number of trees in a solution
        /// </summary>
        public int TreeCount
        {
            get { return mTreeCount; }
            set { mTreeCount = value; }
        }

        /// <summary>
        /// Property that sets the gp evaluation to a parallel task
        /// </summary>
        public bool ParallelEvaluation
        {
            get { return mParallelEvaluation; }
            set { mParallelEvaluation = value; }
        }

        public delegate IGPFitnessCase CreateFitnessCaseHandle(int index);
        public event CreateFitnessCaseHandle CreateFitnessCase;

        public delegate int GetFitnessCaseCountHandle();
        public event GetFitnessCaseCountHandle GetFitnessCaseCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration which describe the behaviors and parameters of the GP</param>
        public TGPPop(TGPConfig config)
        {
            mConfig = config;
            Setup();
        }

        /// <summary>
        /// Method that implements the IGPPop interface which restricts the tree depth during a solution is created during population initialization
        /// </summary>
        public int MaximumDepthForCreation
        {
            get { return mConfig.MaximumDepthForCreation; }
            set { mConfig.MaximumDepthForCreation = value; }
        }

        /// <summary>
        /// Method that implements the IGPPop interface which restricts the tree depth when a solution is produced by crossover, this is a soft constraints which can be violated if necessary
        /// </summary>
        public int MaximumDepthForCrossover
        {
            get { return mConfig.MaximumDepthForCrossover; }
            set { mConfig.MaximumDepthForCrossover = value; }
        }

        /// <summary>
        /// Method that implements the IGPPop interface which restricts the tree depth when a solution is produced by mutation, this is a soft constraints which can be violated if necessary
        /// </summary>
        public int MaximumDepthForMutation
        {
            get { return mConfig.MaximumDepthForMutation; }
            set { mConfig.MaximumDepthForMutation = value; }
        }

        protected virtual IGPEnvironment CreateEnvironment()
        {
            IGPEnvironment environment = new IGPEnvironment();
            environment.CreateFitnessCaseTriggered += (index) =>
            {
                return CreateFitnessCase(index);
            };
            environment.GetFitnessCaseCountTriggered += () =>
            {
                return GetFitnessCaseCount();
            };

            return environment;
        }

        /// <summary>
        /// Property the implements the IPop interface, which describe the mutation rate, a value between 0 and 1
        /// </summary>
        public double MacroMutationRate
        {
            get { return mConfig.MacroMutationRate; }
            set { mConfig.MacroMutationRate = value; }
        }

        /// <summary>
        /// Property that describes the point mutation rate for a GP
        /// </summary>
        public double MicroMutationRate
        {
            get { return mConfig.MicroMutationRate; }
            set { mConfig.MicroMutationRate = value; }
        }

        /// <summary>
        /// Property the implements the IPop interface, which describes the crossover rate, a value between 0 and 1
        /// </summary>
        public double CrossoverRate
        {
            get { return mConfig.CrossoverRate; }
            set { mConfig.CrossoverRate = value; }
        }

        /// <summary>
        /// Property that describes the reproduction rate, a value between 0 and 1
        /// </summary>
        public double ReproductionRate
        {
            get { return mConfig.ReproductionRate; }
            set { mConfig.ReproductionRate = value; }
        }

        /// <summary>
        /// Method that implements the interface of IGPPop, which create a program to be added to a solution
        /// </summary>
        /// <returns>The GP program created and to be added to a solution</returns>
        public virtual object CreateProgram()
        {
            TGPProgram program = new TGPProgram(mOperatorSet, mVariableSet, mConstantSet, mPrimitiveSet);
            return program;
        }

        /// <summary>
        /// Method that implements the interface of IPop, which create a solution
        /// </summary>
        /// <returns>The solution created</returns>
        public virtual ISolution CreateSolution()
        {
           S solution = mSolutionFactory.Create(this, mTreeCount) as S;
            return solution;
        }

        /// <summary>
        /// Method that performs the population initialization and evaluation
        /// </summary>
        /// <param name="handler">The progress change observer method handle</param>
        public virtual void BreedInitialPopulation(EvolutionProgressReportHandler handler=null)
        {
            mSolutions.Clear();
            mGlobalBestProgram = null;

            SetupPrimitiveSet();

            // statistics about the frequency of primitives used by GP in each generation
            SetupPrimitiveStatistics();

            mPopInitInstructionFactory.Initialize(this);

            EvaluateFitnessForPopulation(handler);

            mPrimitiveStatistics.Analyze(mSolutions);
            mCurrentGeneration = 1;
        }

        private PrimitiveStatistics<IGPPop, S> mPrimitiveStatistics = new PrimitiveStatistics<IGPPop, S>();
        private void SetupPrimitiveSet()
        {
            mPrimitiveSet.Clear();

            int variable_count = mVariableSet.TerminalCount;
            int constant_count = mConstantSet.TerminalCount;
            int op_count = mOperatorSet.OperatorCount;

            double sum_weight = 0;
            List<string> variable_names = mVariableSet.TerminalNames;
            foreach (string variable_name in variable_names)
            {
                TGPPrimitive primitive = mVariableSet.FindTerminalBySymbol(variable_name);
                double weight = mVariableSet.FindTerminalWeightBySymbol(variable_name);
                sum_weight += weight;
                mPrimitiveSet.Add(new KeyValuePair<TGPPrimitive, double>(primitive, sum_weight));
            }

            for (int i = 0; i < constant_count; ++i)
            {
                TGPPrimitive primitive = mConstantSet.FindTerminalByIndex(i);
                double weight = mConstantSet.FindTerminalWeightByIndex(i);
                sum_weight += weight;
                mPrimitiveSet.Add(new KeyValuePair<TGPPrimitive, double>(primitive, sum_weight));
            }

            for (int i = 0; i < op_count; ++i)
            {
                TGPPrimitive primitive = mOperatorSet.FindOperatorByIndex(i);
                double weight = mOperatorSet.FindOperatorWeightByIndex(i);
                sum_weight += weight;
                mPrimitiveSet.Add(new KeyValuePair<TGPPrimitive, double>(primitive, sum_weight));
            }
        }

        private void SetupPrimitiveStatistics()
        {
            mPrimitiveStatistics.Clear();
            
            for (int i = 0; i < mOperatorSet.OperatorCount; ++i)
            {
                TGPOperator op=mOperatorSet.FindOperatorByIndex(i);
                mPrimitiveStatistics.AddPrimitive(op);
            }
            List<string> variable_names = mVariableSet.TerminalNames;
            foreach(string variable_name in variable_names)
            {
                TGPTerminal t = mVariableSet.FindTerminalBySymbol(variable_name);
                mPrimitiveStatistics.AddPrimitive(t);
            }
            for (int i = 0; i < mConstantSet.TerminalCount; ++i)
            {
                TGPTerminal t = mConstantSet.FindTerminalByIndex(i);
                mPrimitiveStatistics.AddPrimitive(t);
            }
            
        }

        protected void Setup()
        {
            if (mSetup == false)
            {
                mEnvironment = CreateEnvironment();
                mMutationInstructionFactory = CreateMutationInstructionFactory(mConfig.GetScript("MutationInstructionFactory"));
                mCrossoverInstructionFactory = CreateCrossoverInstructionFactory(mConfig.GetScript("CrossoverInstructionFactory"));
                mPopInitInstructionFactory = CreatePopInitInstructionFactory(mConfig.GetScript("PopInitInstructionFactory"));
                mReproductionSelectionInstructionFactory = CreateReproductionSelectionInstructionFactory(mConfig.GetScript("ReproductionSelectionInstructionFactory"));
                mSurvivalInstructionFactory = CreateSurvivalInstructionFactory(mConfig.GetScript("SurvivalInstructionFactory"));
                mSetup = true;
            }
        }

        protected virtual MutationInstructionFactory<IGPPop, S> CreateMutationInstructionFactory(string filename)
        {
            return new TGPMutationInstructionFactory<IGPPop, S>(filename);
        }
        protected virtual CrossoverInstructionFactory<IGPPop, S> CreateCrossoverInstructionFactory(string filename)
        {
            return new TGPCrossoverInstructionFactory<IGPPop, S>(filename);
        }
        protected virtual PopInitInstructionFactory<IGPPop, S> CreatePopInitInstructionFactory(string filename)
        {
            return new TGPPopInitInstructionFactory<IGPPop, S>(filename);
        }
        protected virtual SelectionInstructionFactory<IGPPop, S> CreateReproductionSelectionInstructionFactory(string filename)
        {
            SelectionInstructionFactory<IGPPop, S> factory = new SelectionInstructionFactory<IGPPop, S>(filename);
            if (factory.CurrentInstruction is SelectionInstruction_Tournament<IGPPop, S>)
            {
                SelectionInstruction_Tournament<IGPPop, S> tournament_instruction = (SelectionInstruction_Tournament<IGPPop, S>)factory.CurrentInstruction;
                tournament_instruction.TournamentSize = this.mConfig.ReproductionSelectionTournamentSize;
            }
            return factory;
        }

        protected virtual SurvivalInstructionFactory<IGPPop, S> CreateSurvivalInstructionFactory(string filename)
        {
            return new SurvivalInstructionFactory<IGPPop, S>(filename);
        }

        public string Mutation
        {
            set { mMutationInstructionFactory.InstructionType = value; }
        }

        public string Crossover
        {
            set { mCrossoverInstructionFactory.InstructionType = value; }
        }

        public string PopulationInitialization
        {
            set { mPopInitInstructionFactory.InstructionType = value; }
        }


        /// <summary>
        /// Property that accepts a string description to switch a population initialization method
        /// </summary>
        public PopInitInstruction<IGPPop, S> PopInitInstruction
        {
            set
            {
                mPopInitInstructionFactory.CurrentInstruction = value;
            }
        }


        /// <summary>
        /// Property the accepts a string description to switch the reproduction selection method
        /// </summary>
        public SelectionInstruction<IGPPop, S> ReproductionSelectionInstruction
        {
            set
            {
                mReproductionSelectionInstructionFactory.CurrentInstruction = value;
            }
        }

        public delegate bool EvolutionProgressReportHandler(int program_index, S prog);

        /// <summary>
        /// Method that evolve the GP population in a generation.
        /// </summary>
        /// <param name="report_handler">The progress change observer method handle</param>
        public virtual void Evolve(EvolutionProgressReportHandler report_handler=null)
        {
            OptimizeMemory();

            if (PopulationReplacement == PopulationReplacementMode.MuPlusLambda)
            {
                MuPlusLambdaEvolve(report_handler);
            }
            else if (PopulationReplacement == PopulationReplacementMode.TinyGP)
            {
                TinyGPEvolve(report_handler);
            }

            mCurrentGeneration++;

            Analyze();
        }

        /// <summary>
        /// The mode that indicates how the current population will be replaced by the offspring
        /// If the Default mode is selected, the (mu+lambda) population mode is used
        /// If the TinyGP mode is selected, the population replacement will be based on TinyGP which is as described in "A Field Guide to Genetic Programming", 
        /// in which a solution in the parent population is replaced by a child solution based on fitness
        /// </summary>
        public enum PopulationReplacementMode
        {
            MuPlusLambda,
            TinyGP
        }

        /// <summary>
        /// The mode that indicates how the current population will be replaced by the offspring
        /// If the Default mode is selected, the (mu+lambda) population mode is used
        /// If the TinyGP mode is selected, the population replacement will be based on TinyGP which is as described in "A Field Guide to Genetic Programming", 
        /// in which a solution in the parent population is replaced by a child solution based on fitness
        /// </summary>
        public PopulationReplacementMode PopulationReplacement = PopulationReplacementMode.MuPlusLambda;

        /// <summary>
        /// Method that analyzes the composition of solution in terms of primitives that constitutes them during optimization
        /// </summary>
        protected void Analyze()
        {
            mPrimitiveStatistics.Analyze(mSolutions);
        }

 
        /// <summary>
        /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
        /// </summary>
        /// <param name="report_handler"></param>
        protected void MuPlusLambdaEvolve(EvolutionProgressReportHandler report_handler = null)
        {

            int iPopSize = this.mConfig.PopulationSize;
            SelectionInstruction<IGPPop, S> parent_selection=mReproductionSelectionInstructionFactory.CurrentInstruction;
            
            int elite_count=(int)(mConfig.ElitismRatio * iPopSize);

            int crossover_count = (int)(this.mConfig.CrossoverRate * iPopSize);

            if (crossover_count % 2 != 0) crossover_count += 1;

            int micro_mutation_count = (int)(this.mConfig.MicroMutationRate * iPopSize);
            int macro_mutation_count = (int)(this.mConfig.MacroMutationRate * iPopSize);
            int reproduction_count = iPopSize - crossover_count - micro_mutation_count - macro_mutation_count;

            

            List<S> offspring = new List<S>();

            S best_solution = null;
            int best_solution_index = 0;

            //do crossover
            for (int offspring_index = 0; offspring_index < crossover_count; offspring_index += 2)
            {
                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();
                mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 2, Compare);


                List<S> children = mCrossoverInstructionFactory.Crossover(this, good_parents.ToArray());

                foreach (S child in children)
                {
                    offspring.Add(child);
                }
            }

            // do point mutation
            for (int offspring_index = 0; offspring_index < micro_mutation_count; ++offspring_index)
            {
                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();

                mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);

               S child = good_parents[0].Clone() as S;

                child.MicroMutate();

                offspring.Add(child);
            }

            // do subtree mutation
            for (int offspring_index = 0; offspring_index < macro_mutation_count; ++offspring_index)
            {
                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();

                mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);

                S child = good_parents[0].Clone() as S;

                mMutationInstructionFactory.Mutate(this, child);

                offspring.Add(child);

            }

            // do reproduction
            for (int offspring_index = 0; offspring_index < reproduction_count; ++offspring_index)
            {
                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();

                mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);

                S child = good_parents[0].Clone() as S;

                offspring.Add(child);
            }

            if (mParallelEvaluation)
            {
                int parallel_task_batch_count = (int)(System.Math.Ceiling((double)iPopSize / mMaxParallelTaskCount));

                for (int batch_index = 0; batch_index < parallel_task_batch_count; ++batch_index)
                {
                    int task_count = mMaxParallelTaskCount;
                    if (batch_index == parallel_task_batch_count - 1)
                    {
                        task_count = iPopSize - (parallel_task_batch_count - 1) * mMaxParallelTaskCount;
                    }
                    Task[] tasks = new Task[task_count];

                    for (int i = 0; i < task_count; ++i)
                    {
                        int task_id = i;
                        int solution_index = mMaxParallelTaskCount * batch_index + i;
                        tasks[task_id] = Task.Factory.StartNew(() =>
                        {
                            S child = offspring[solution_index];

                            if (child.IsFitnessValid == false)
                            {
                                child.EvaluateFitness();
                            }
                        });
                    }
                    Task.WaitAll(tasks);

                    for (int i = 0; i < task_count; ++i)
                    {
                        int solution_index = mMaxParallelTaskCount * batch_index + i;
                        S child = offspring[solution_index];

                        if (best_solution == null || child.IsBetterThan(best_solution))
                        {
                            if (best_solution != null)
                            {
                                best_solution.RemoveAttributes();
                            }
                            best_solution = child;
                            best_solution_index = solution_index;
                        }
                        else
                        {
                            child.RemoveAttributes();
                        }
                    }

                }

                if (report_handler != null)
                {
                    if(best_solution != null)
                    report_handler(best_solution_index, best_solution);
                }

                
            }
            else
            {
                for (int i = 0; i < iPopSize; ++i)
                {
                    S child = offspring[i];

                    if (child.IsFitnessValid == false)
                    {
                        child.EvaluateFitness();
                    }

                    if (report_handler != null)
                    {
                        if (!report_handler(i, child))
                        {
                            return;
                        }
                    }

                    if (best_solution==null || child.IsBetterThan(best_solution))
                    {
                        if (best_solution != null)
                        {
                            best_solution.RemoveAttributes();
                        }
                        best_solution = child;
                    }
                    else
                    {
                        child.RemoveAttributes();
                    }
                }
            }
            

            if (best_solution != null && best_solution != mGlobalBestProgram && best_solution.IsBetterThan(mGlobalBestProgram))
            {
                mGlobalBestProgram = best_solution.Clone() as S;
                best_solution.RemoveAttributes();
            }

            mSolutions=mSolutions.OrderByDescending(o => o.Fitness).ToList();
            
            offspring=offspring.OrderByDescending(o => o.Fitness).ToList();

            for (int offspring_index = elite_count; offspring_index < iPopSize; ++offspring_index)
            {
                mSolutions[offspring_index] = offspring[offspring_index-elite_count];
            }
        }

        /// <summary>
        /// Similar to Evolve2, but use offspring solution to replace bad parent, in a way similar to TinyGP as as specified in "A Field Guide to Genetic Programming"
        /// </summary>
        /// <param name="report_handler"></param>
        protected void TinyGPEvolve(EvolutionProgressReportHandler report_handler = null)
        {
            int iPopSize = this.mConfig.PopulationSize;
            SelectionInstruction<IGPPop, S> parent_selection = mReproductionSelectionInstructionFactory.CurrentInstruction;

            double sum_rate=CrossoverRate+MacroMutationRate+MicroMutationRate+ReproductionRate;
            double crossover_disk = CrossoverRate / sum_rate;
            double micro_mutation_disk = (CrossoverRate + MicroMutationRate) / sum_rate;
            double macro_mutation_disk = (CrossoverRate + MicroMutationRate + MacroMutationRate) / sum_rate;

            List<S> parallel_offspring = new List<S>();
            
            S best_solution = mGlobalBestProgram;
            for (int offspring_index = 0; offspring_index < iPopSize; offspring_index += 1)
            {
                double r = DistributionModel.GetUniform();
                List<S> children = new List<S>();

                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();

                if (r <= crossover_disk) // do crossover
                {    
                    mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 2, Compare);
                    children = mCrossoverInstructionFactory.Crossover(this, good_parents.ToArray());
                }
                else if (r <= micro_mutation_disk) // do point mutation
                {
                    mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);
                    S child = good_parents[0].Clone() as S;
                    child.MicroMutate();
                    children.Add(child);
                }
                else if (r <= macro_mutation_disk) // do subtree mutation
                {
                    mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);
                    S child = good_parents[0].Clone() as S;
                    mMutationInstructionFactory.Mutate(this, child);
                    children.Add(child);
                }
                else // do reproduction
                {
                    mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);
                    S child = good_parents[0].Clone() as S;
                    children.Add(child);
                }

                if (!mParallelEvaluation)
                {
                    bool sucessfully_replaced = false;
                    foreach (S child in children)
                    {
                        if (child.IsFitnessValid == false)
                        {
                            child.EvaluateFitness();
                        }

                        if (best_solution == null || child.IsBetterThan(best_solution))
                        {
                            if (best_solution != null)
                            {
                                best_solution.RemoveAttributes();
                            }
                            best_solution = child;
                        }
                        else
                        {
                            child.RemoveAttributes();
                        }

                        foreach (S bad_parent in bad_parents)
                        {
                            if (child.IsBetterThan(bad_parent))
                            {
                                sucessfully_replaced = true;
                                Replace(bad_parent, child);
                                break;
                            }
                        }
                        if (sucessfully_replaced)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foreach (S child in children)
                    {
                        parallel_offspring.Add(child);
                    }
                }
            }

            if (mParallelEvaluation)
            {
                int max_parallel_task_count = 8;

                int parallel_task_batch_count = (int)(System.Math.Ceiling((double)iPopSize / max_parallel_task_count));

                for (int batch_index = 0; batch_index < parallel_task_batch_count; ++batch_index)
                {
                    int task_count = max_parallel_task_count;
                    if (batch_index == parallel_task_batch_count - 1)
                    {
                        task_count = iPopSize - (parallel_task_batch_count - 1) * max_parallel_task_count;
                    }
                    Task[] tasks = new Task[task_count];
                    for (int i = 0; i < task_count; ++i)
                    {
                        int task_id = i;
                        int solution_index = max_parallel_task_count * batch_index + i;
                        tasks[task_id] = Task.Factory.StartNew(() =>
                            {
                                S child = parallel_offspring[solution_index];
                                if (child.IsFitnessValid == false)
                                {
                                    child.EvaluateFitness();
                                }
                            });
                    }

                    Task.WaitAll(tasks);

                    for (int i = 0; i < task_count; ++i)
                    {
                        int solution_index = max_parallel_task_count * batch_index + i;
                        S child = parallel_offspring[solution_index];
                        if (best_solution==null || child.IsBetterThan(best_solution))
                        {
                            if (best_solution != null)
                            {
                                best_solution.RemoveAttributes();
                            }
                            best_solution = child;
                        }

                        // negative tournament selection
                        List<S> good_parents = new List<S>();
                        List<S> bad_parents = new List<S>();
                        mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 1, Compare);

                        foreach (S bad_parent in bad_parents)
                        {
                            if (child.IsBetterThan(bad_parent))
                            {
                                Replace(bad_parent, child);
                            }
                        }
                    }

                    for (int i = 0; i < task_count; ++i)
                    {
                        int solution_index = max_parallel_task_count * batch_index + i;
                        S child = parallel_offspring[solution_index];
                        if (child != best_solution)
                        {
                            child.RemoveAttributes();
                        }
                    }
                }

                if (report_handler != null)
                {
                    int p_index=DistributionModel.NextInt(iPopSize);
                    report_handler(p_index, parallel_offspring[p_index]);
                }
            }

            if (best_solution != null && best_solution != mGlobalBestProgram && best_solution.IsBetterThan(mGlobalBestProgram))
            {
                mGlobalBestProgram = best_solution.Clone() as S;
                best_solution.RemoveAttributes();
            }
        }

        /// <summary>
        /// Method invoked by the tournament selection during the optimization, this will return a 1 is s2 is better than s1, 0, if their fitnesses are equal, and -1 otherwise
        /// This is causes a descending ordering when the Sort() method is called, e.g. on a list of solution, such that the best solution is the first solution
        /// </summary>
        /// <param name="s1">The solution 1</param>
        /// <param name="s2">The solution 2</param>
        /// <returns>The returned value is 1 is s2 is better than s1, 0, if their fitnesses are equal, and -1 otherwise</returns>
        private int Compare(S s1, S s2)
        {
            return s2.Compare(s1);
        }

        /// <summary>
        /// Property between 0 and 1, and determines what percentage of population should be kept using elitism
        /// </summary>
        public double ElitismRatio
        {
            get { return mConfig.ElitismRatio; }
            set { mConfig.ElitismRatio = value; }
        }

        /// <summary>
        /// Method that evolve by applying composite evolution operator (e.g. crossover+mutation) on a single solution
        /// This is different from traditional GP, as specified in "A Field Guide to Genetic Programming", in which a single
        /// evolution operator is applied to each solution to generate an offspring
        /// </summary>
        /// <param name="report_handler">A progress change observer method handle</param>
        public void CompositeEvolve(EvolutionProgressReportHandler report_handler = null)
        {

            int iPopSize = this.mConfig.PopulationSize;
            SelectionInstruction<IGPPop, S> parent_selection = mReproductionSelectionInstructionFactory.CurrentInstruction;

            int elite_count = (int)(mConfig.ElitismRatio * iPopSize);

            List<S> offspring = new List<S>();

           S best_solution = mGlobalBestProgram;
            for (int offspring_index = 0; offspring_index < iPopSize; offspring_index += 2)
            {
                List<S> good_parents = new List<S>();
                List<S> bad_parents = new List<S>();
                mReproductionSelectionInstructionFactory.Select(this, good_parents, bad_parents, 2, Compare);

                List<S> children = null;
                if (DistributionModel.GetUniform() < mConfig.CrossoverRate)
                {
                    children = mCrossoverInstructionFactory.Crossover(this, good_parents.ToArray());
                }
                else
                {
                    children = new List<S>();
                    foreach (S parent in good_parents)
                    {
                        children.Add(parent.Clone() as S);
                    }
                }

                foreach (S child in children)
                {
                    if (DistributionModel.GetUniform() < mConfig.MacroMutationRate)
                    {
                        mMutationInstructionFactory.Mutate(this, child);
                    }
                    if (DistributionModel.GetUniform() < mConfig.MicroMutationRate)
                    {
                        child.MicroMutate();
                    }
                   
                    offspring.Add(child);
                }
            }

            for (int i = 0; i < iPopSize; ++i)
            {
               S child = offspring[i];

                if (child.IsFitnessValid == false)
                {
                    child.EvaluateFitness();
                }

                if (report_handler != null)
                {
                    if (!report_handler(i, child))
                    {
                        return;
                    }
                }

                if (child.IsBetterThan(best_solution))
                {
                    best_solution = child;
                }
            }

            if (best_solution != null && best_solution != mGlobalBestProgram && best_solution.IsBetterThan(mGlobalBestProgram))
            {
                mGlobalBestProgram = best_solution.Clone() as S;
            }

            mSolutions = mSolutions.OrderByDescending(o => o.Fitness).ToList();

            offspring = offspring.OrderByDescending(o => o.Fitness).ToList();

            for (int offspring_index = elite_count; offspring_index < iPopSize; ++offspring_index)
            {
                mSolutions[offspring_index] = offspring[offspring_index-elite_count];
            }
        }

        

        /// <summary>
        /// Method the evaluate the fitness of each solution in the current solution
        /// </summary>
        /// <param name="handler">The progess change observer method handle</param>
        /// <returns>The returned flag indicates whether the solution should continue (TRUE) or otherwise (FALSE)</returns>
        protected virtual bool EvaluateFitnessForPopulation(EvolutionProgressReportHandler handler=null)
        {
            if (mParallelEvaluation)
            {
                int parallel_task_batch_count = (int)(System.Math.Ceiling((double)PopulationSize / mMaxParallelTaskCount));

                S best_solution = null;
                for (int batch_index = 0; batch_index < parallel_task_batch_count; ++batch_index)
                {
                    int task_count = mMaxParallelTaskCount;
                    if (batch_index == parallel_task_batch_count - 1)
                    {
                        task_count = PopulationSize - (parallel_task_batch_count - 1) * mMaxParallelTaskCount;
                    }

                    Task[] tasks = new Task[task_count];
                    for (int task_index = 0; task_index < task_count; ++task_index)
                    {
                        int k = task_index;
                        int solution_index = mMaxParallelTaskCount * batch_index + task_index;
                        tasks[k] = Task.Factory.StartNew(() =>
                        {
                            S solution = mSolutions[solution_index];
                            if (!solution.IsFitnessValid)
                            {
                                solution.EvaluateFitness();
                            }
                        });
                    };

                    //Block until all tasks complete.
                    Task.WaitAll(tasks);

                    for (int task_index = 0; task_index < task_count; ++task_index)
                    {
                        int solution_index = mMaxParallelTaskCount * batch_index + task_index;
                        S solution = mSolutions[solution_index];
                        if (best_solution == null || solution.IsBetterThan(best_solution))
                        {
                            if (best_solution != null)
                            {
                                best_solution.RemoveAttributes();
                            }
                            best_solution = solution;
                        }
                    }

                    for (int task_index = 0; task_index < task_count; ++task_index)
                    {
                        int solution_index = mMaxParallelTaskCount * batch_index + task_index;
                        S solution = mSolutions[solution_index];
                        if (solution != best_solution)
                        {
                            solution.RemoveAttributes();
                        }
                    }
                }

                mGlobalBestProgram = best_solution.Clone() as S;
                OptimizeMemory();

                if (handler != null)
                {
                    if (!handler(0, mSolutions[0]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                S solution;
                for (int i = 0; i < mSolutions.Count; i++)
                {
                    solution = mSolutions[i];
                    if (!solution.IsFitnessValid)
                    {
                        solution.EvaluateFitness();
                    }
                    if (handler != null)
                    {
                        if (!handler(i, solution))
                        {
                            return false;
                        }
                    }
                }

                mGlobalBestProgram = FindFittestProgramInCurrentGeneration().Clone() as S;
            }
            return true;
        }

        /// <summary>
        /// Property that indicates whether the optimization should stop
        /// </summary>
        public virtual bool IsTerminated
        {
            get
            {
                return mCurrentGeneration >= mConfig.MaxGenerations;
            }
        }

        /// <summary>
        /// Method that implements the IPop interface, which returns the solution at a particular index with the current population
        /// </summary>
        /// <param name="idx">The solution index within the population</param>
        /// <returns>The solution at that index</returns>
        public ISolution FindSolutionByIndex(int idx)
        {
            return mSolutions[idx];
        }

        /// <summary>
        /// Method that implements the IPop interface, which return the flag indicating whether the problem is a maximization or minimization problem
        /// </summary>
        public bool IsMaximization
        {
            get { return mConfig.IsMaximization; }
            set { mConfig.IsMaximization = value; }
        }

        public delegate double ObjectiveEvaluatorHandler(ISolution solution, int objective_index);
        public event ObjectiveEvaluatorHandler ObjectiveEvaluator;

        /// <summary>
        /// Method that implements the IGPPop interface, which returns the fitness of a solution 
        /// </summary>
        /// <param name="solution">The solution for which the fitness is to be returned</param>
        /// <param name="objective_index">The index of the objective if it is for multi-objective, which is normally 0 for single objective</param>
        /// <returns>The fitness of a solution</returns>
        public double EvaluateObjective(ISolution solution, int objective_index)
        {
            double objective_value = 0;
            if (ObjectiveEvaluator == null)
            {
                S s = solution as S;
                int tree_count = s.ProgramCount;

                IGPEnvironment env = Environment;
                int fitness_case_count = env.GetFitnessCaseCount();
                List<IGPFitnessCase> cases = new List<IGPFitnessCase>();
                for (int i = 0; i < fitness_case_count; ++i)
                {
                    IGPFitnessCase fitness_case = env.CreateFitnessCase(i);
                    for (int tindex = 0; tindex < tree_count; ++tindex)
                    {
                        TGPProgram program = s.FindProgramAt(tindex) as TGPProgram;
                        fitness_case.StoreOutput(program.ExecuteOnFitnessCase(fitness_case), tindex);
                    }

                    cases.Add(fitness_case);
                }

                if (EvaluateObjectiveForSolution != null)
                {
                    objective_value = EvaluateObjectiveForSolution(cases, s, objective_index);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            else
            {
                objective_value = ObjectiveEvaluator(solution, objective_index);
            }
            return objective_value;
        }

        /// <summary>
        /// Property that implements the IGPPop interface
        /// </summary>
        public IGPEnvironment Environment
        {
            get { return mEnvironment; }
        }

        /// <summary>
        /// Property that indicate the current optimization generation
        /// </summary>
        public int CurrentGeneration
        {
            get { return mCurrentGeneration; }
        }

        /// <summary>
        /// property that implements the IPop interface, which specifies the size of the population should be
        /// </summary>
        public int PopulationSize
        {
            get { return mConfig.PopulationSize; }
            set { mConfig.PopulationSize = value; }
        }

        /// <summary>
        /// Method that implements the IPop interface
        /// </summary>
        /// <param name="lgp">The solution to be added</param>
        public void AddSolution(ISolution lgp)
        {
            mSolutions.Add(lgp as S);
        }

        /// <summary>
        /// Method that returns the count of solutions currently in the population
        /// </summary>
        public int SolutionCount 
        { 
            get { return mSolutions.Count; } 
        }

        /// <summary>
        /// Method that shuffles the current population
        /// </summary>
        public void RandomShuffle()
        {
           Shuffle(mSolutions);
        }

        /// <summary>
        /// Method that implements the IPop interface
        /// </summary>
        /// <returns></returns>
        public List<ISolution> ToList()
        {
            List<ISolution> solutions = new List<ISolution>();
            foreach (S solution in mSolutions)
            {
                solutions.Add(solution);
            }
            return solutions;
        }

        /// <summary>
        /// Method that return the description string of the population
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (S s in mSolutions)
            {
                sb.AppendFormat("\n{0}", s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Method that shuffles a list of solutions 
        /// </summary>
        /// <param name="list"></param>
        public static void Shuffle(IList<S> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
               S value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Method that sort the population such that the solution with the highest fitness is the first solution in the population
        /// </summary>
        public void SortFittestProgramFirst()
        {
            mSolutions = mSolutions.OrderByDescending(o => o.Fitness).ToList();
        }

        /// <summary>
        /// Method that sort the population such that the solution with the highest fitness is the last solution in the population
        /// </summary>
        public void SortFittestProgramLast()
        {
            mSolutions = mSolutions.OrderBy(o => o.Fitness).ToList();
        }

        /// <summary>
        /// Method that returns the solution in the current generation which has the highest fitness
        /// </summary>
        /// <returns>The solution in the current population that has the highest fitness</returns>
        public S FindFittestProgramInCurrentGeneration()
        {
            SortFittestProgramFirst();
            return mSolutions[0];
        }

        /// <summary>
        /// The global best solution obtained during the optimization
        /// </summary>
        public S GlobalBestProgram
        {
            get { return mGlobalBestProgram; }
        }

        /// <summary>
        /// Set of functions for the GP evolution
        /// </summary>
        public TGPOperatorSet OperatorSet
        {
            get { return mOperatorSet; }
        }

        /// <summary>
        /// Set of terminals which are the program's external input, usually as named variables
        /// </summary>
        public TGPVariableSet VariableSet
        {
            get { return mVariableSet; }
        }

        /// <summary>
        /// Set of terminals which are either constraints or 0-arity functions
        /// </summary>
        public TGPConstantSet ConstantSet
        {
            get { return mConstantSet; }
        }

        /// <summary>
        /// Method implemented for the IPop interface for replace the bad solution with a new solution
        /// </summary>
        /// <param name="weak_program_in_current_pop">The bad solution currently in the population</param>
        /// <param name="child_program">The new solution that will replace the bad solution</param>
        public void Replace(ISolution weak_program_in_current_pop, ISolution child_program)
        {
            for (int i = 0; i < mSolutions.Count; ++i)
            {
                if (mSolutions[i] == weak_program_in_current_pop)
                {
                    mSolutions[i] = child_program as S;
                }
            }
        }

        /// <summary>
        /// Maximum number of generations before the TreeGP algorithm terminates
        /// </summary>
        public int MaxGeneration
        {
            get { return mConfig.MaxGenerations; }
            set { mConfig.MaxGenerations = value; }
        }

        /// <summary>
        /// Statistics about the primitives obtained during the optimization
        /// </summary>
        public PrimitiveStatistics<IGPPop, S> PrimitiveStats
        {
            get { return mPrimitiveStatistics; }
        }

        private void OptimizeMemory()
        {
            foreach (S s in mSolutions)
            {
                s.RemoveAttributes();
            }
            GC.Collect();
        }
    }
}
