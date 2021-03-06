# cs-tree-genetic-programming

Tree-based genetic programming implemented using C#

# Install

```bash
Install-Package cs-tree-genetic-programming -Version 1.0.5
```

# Usage

Below shows the sample codes in which the symbolic regression is solved using TreeGP. The symbolic regression is trying to find a GP solution that approximate the function y = x^2 + x + 1.

```cs
class Program
{
	static double FunctionXY(double x)
	{
		return x * x + x + 1;
	}

	static DataTable LoadData()
	{
		DataTable table = new DataTable();
		table.Columns.Add("X");
		table.Columns.Add("Y");

		double lower_bound = -1.0;
		double upper_bound = 1.0;

		double interval = 0.1;

		for (double x = lower_bound; x <= upper_bound; x+=interval)
		{
			table.Rows.Add(x, FunctionXY(x));
		}


		return table;
	}
	static void Main(string[] args)
	{
		DataTable table = LoadData();

		TGPConfig config = new TGPConfig("TGPConfig.xml");

		// The problem is to minimize the sum of errors between predicted and actual function
		config.IsMaximization = false;

		// As specified in Chapter 4 of "A Field Guide to Genetic Programming"
		config.PopulationSize = 4;
		config.ElitismRatio = 0; //non elitist
		config.CrossoverRate = 0.5; // subtree crossover rate set to 0.5
		config.MacroMutationRate = 0.25; // subtree mutation rate set to 0.25; 
		config.MicroMutationRate = 0.0; // point mutation rate set to 0.0
		config.ReproductionRate = 0.25; // reproduction rate set to 0.25
		config.NormalizeEvolutionRates();
		//Question 1: Is the performance normal for the GP?

		config.MaximumDepthForCreation = 2;
		config.MaximumDepthForCrossover = 2; // no tree size limit by setting a very large max depth
		config.MaximumDepthForMutation = 2; 

		TGPPop<TGPSolution> pop = new TGPPop<TGPSolution>(config);
		pop.ReproductionSelectionInstruction = new SelectionInstruction_RouletteWheel<TGPSolution>(); //use roulette wheel selection 

		// Function Set = {+, -, %, *} where % is protected division that returns 1 if the denominator is 0
		pop.OperatorSet.AddOperator(new TGPOperator_Plus());
		pop.OperatorSet.AddOperator(new TGPOperator_Minus());
		pop.OperatorSet.AddOperator(new TGPOperator_Division());
		pop.OperatorSet.AddOperator(new TGPOperator_Multiplication());

		// Terminal Set = {R, x}
		pop.ConstantSet.AddConstant("R", DistributionModel.GetUniform()* 10.0 - 5.0);
		pop.VariableSet.AddVariable("x");

		pop.CreateFitnessCase += (index) =>
		{
			SymRegFitnessCase fitness_case = new SymRegFitnessCase();
			fitness_case.X = double.Parse(table.Rows[index]["X"].ToString());
			fitness_case.Y = double.Parse(table.Rows[index]["Y"].ToString());

			return fitness_case;
		};

		pop.GetFitnessCaseCount += () =>
		{
			return table.Rows.Count;
		};

		pop.EvaluateObjectiveForSolution += (fitness_cases, solution, objective_index) =>
		{
			double sum_of_error = 0;
			for (int i = 0; i < fitness_cases.Count; i++)
			{
				SymRegFitnessCase fitness_case = (SymRegFitnessCase)fitness_cases[i];
				double correct_y = fitness_case.Y;
				double computed_y = fitness_case.ComputedY;
				sum_of_error += System.Math.Abs(correct_y - computed_y);
			}

			return sum_of_error;
		};


		pop.BreedInitialPopulation();

		double error = pop.GlobalBestProgram.ObjectiveValue;
		while (error > 0.1)
		{
			pop.Evolve();
			error = pop.GlobalBestProgram.ObjectiveValue;

			Console.WriteLine("Symbolic Regression Generation: {0}", pop.CurrentGeneration);
			Console.WriteLine("Minimum Error: {0}", error.ToString("0.000"));
			Console.WriteLine("Global Best Solution:\n{0}", pop.GlobalBestProgram);
			
		}

		Console.WriteLine(pop.GlobalBestProgram.ToString());

	}
} 
```

The TGPConfig.xml and its child configuration files will be automatically generated if they do not exist, otherwise the configuration will be loaded from the existing TGPConfig.xml and its child configuration files.


