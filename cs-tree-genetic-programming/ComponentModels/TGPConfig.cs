using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.ComponentModels
{
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
    /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", it is common to initialize population randomly using ramped half-and-half with a depth range of 2-6
    /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", the population size should be at least 500 and people often use much larger population
    /// Typically the number of generations is limited to between 10 to 50; the most productive search is usually performed in those early generations and if a solution hasn't been found then, it is unlikely to be found in a reasonable amount of time
    /// </summary>
    public class TGPConfig
    {
        private bool mIsMaximization;
        /// <summary>
        /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", the population size should be at least 500 and people often use much larger population
        /// </summary>
        private int mPopulationSize=500;
        /// <summary>
        /// Typically the number of generations is limited to between 10 to 50; the most productive search is usually performed in those early generations and if a solution hasn't been found then, it is unlikely to be found in a reasonable amount of time
        /// </summary>
        private int mMaxGenerations=100;

        /// <summary>
        /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", it is common to initialize population randomly using ramped half-and-half with a depth range of 2-6
        /// </summary>
        private int mMaximumDepthForCreation=7;
        private int mMaximumDepthForCrossover = 10;
        private int mMaximumDepthForMutation = 10;

        /// <summary>
        /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
        /// </summary>
        private double mCrossoverRate=0.9;
        /// <summary>
        /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
        /// </summary>
        private double mMicroMutationRate=0.005;
        /// <summary>
        /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
        /// </summary>
        private double mMacroMutationRate=0.005;
        /// <summary>
        /// Typically, crossover rate is 90% or higher, mutation rate is much smaller, within the region of 1%, and the rest is the reproduction rate, however, a 50-50 mixture of crossover an a variety of mutations also appears to work well
        /// </summary>
        private double mReproductionRate=0.09;
        private double mElitismRatio = 0.1;
        protected Dictionary<string, string> mAttributes = new Dictionary<string, string>();
        private int mReproductionSelectionTournamentSize = 5;

        public int ReproductionSelectionTournamentSize
        {

            get { return mReproductionSelectionTournamentSize; }
            set { mReproductionSelectionTournamentSize = value; }
        }

        public string this[string index]
        {
            get
            {
                return mAttributes[index];
            }
            set
            {
                mAttributes[index] = value;
            }
        }

        public int MaximumDepthForCreation { get { return mMaximumDepthForCreation; }
            set
            {
                mMaximumDepthForCreation = value;
            }
        }
        public int MaximumDepthForCrossover { get { return mMaximumDepthForCrossover; }
            set
            {
                mMaximumDepthForCrossover = value;
            }
        }
        public int MaximumDepthForMutation { get { return mMaximumDepthForMutation; }
            set
            {
                mMaximumDepthForMutation = value;
            }
        }
        public int MaxGenerations
        {
            get { return mMaxGenerations; }
            set
            {
                mMaxGenerations = value;
            }
        }
        public double CrossoverRate { get { return mCrossoverRate; }
            set
            {
                mCrossoverRate = value;
            }
        }
        public bool IsMaximization { get { return mIsMaximization; }
            set { mIsMaximization = value; }
        }
        public double MicroMutationRate { get { return mMicroMutationRate; } 
        set{
            mMicroMutationRate = value;
        }
        }
        public double MacroMutationRate { get { return mMacroMutationRate; }
            set
            {
                mMacroMutationRate = value;
            }
        }

        public int PopulationSize
        {
            get { return mPopulationSize; }
            set { mPopulationSize = value; }
        }

        private Dictionary<string, string> mScripts = new Dictionary<string, string>();

        protected string mFilename;

        public string GetScript(string p)
        {
            if (mScripts.ContainsKey(p))
            {
                string scriptPath = mScripts[p];
                if(!File.Exists(scriptPath))
                {
                    DirectoryInfo parentDir = Directory.GetParent(scriptPath);
                    if(!parentDir.Exists)
                    {
                        parentDir.Create();
                    }
                    if (p == ScriptNames.CrossoverInstructionFactory)
                    {
                        File.WriteAllText(scriptPath, Properties.Resources.CrossoverInstructionFactory);
                    } else if(p == ScriptNames.MutationInstructionFactory)
                    {
                        File.WriteAllText(scriptPath, Properties.Resources.MutationInstructionFactory);
                    } else if(p == ScriptNames.PopInitInstructionFactory)
                    {
                        File.WriteAllText(scriptPath, Properties.Resources.PopInitInstructionFactory);
                    } else if(p == ScriptNames.ReproductionSelectionInstructionFactory)
                    {
                        File.WriteAllText(scriptPath, Properties.Resources.ReproductionSelectionInstructionFactory);
                    } else if(p == ScriptNames.SurvivalInstructionFactory)
                    {
                        File.WriteAllText(scriptPath, Properties.Resources.SurvivalInstructionFactory);
                    }

                }
                
            }
            return null;
        }

        public TGPConfig()
        {

        }

        public TGPConfig(string filename)
        {
            if(!File.Exists(filename))
            {
                File.WriteAllText(filename, Properties.Resources.TGPConfig);
            }

            Load(filename);

        }

        public void Load(String filename)
        {
            mFilename = filename;
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement doc_root = doc.DocumentElement;

            foreach (XmlElement xml_level1 in doc_root.ChildNodes)
            {
                if (xml_level1.Name == "parameters")
                {
                    foreach (XmlElement xml_level2 in xml_level1.ChildNodes)
                    {
                        if (xml_level2.Name == "param")
                        {
                            string attrname = xml_level2.Attributes["name"].Value;
                            string attrvalue = xml_level2.Attributes["value"].Value;
                            if (attrname == "PopulationSize")
                            {
                                int value = 0;
                                int.TryParse(attrvalue, out value);
                                mPopulationSize = value;
                            }
                            else if (attrname == "MaxGenerations")
                            {
                                int value = 0;
                                int.TryParse(attrvalue, out value);
                                mMaxGenerations = value;
                            }
                            else if (attrname == "Maximization")
                            {
                                bool value = false;
                                bool.TryParse(attrvalue, out value);
                                mIsMaximization = value;
                            }
                            else if (attrname == "CrossoverRate")
                            {
                                double value = 0;
                                double.TryParse(attrvalue, out value);
                                mCrossoverRate = value;
                            }
                            else if (attrname == "MacroMutationRate")
                            {
                                double value = 0;
                                double.TryParse(attrvalue, out value);
                                mMacroMutationRate = value;
                            }
                            else if (attrname == "MicroMutationRate")
                            {
                                double value = 0;
                                double.TryParse(attrvalue, out value);
                                mMicroMutationRate = value;
                            }
                            else if (attrname == "ReproductionRate")
                            {
                                double value = 0;
                                double.TryParse(attrvalue, out value);
                                mReproductionRate = value;
                            }
                            else if (attrname == "MaxDepthForCrossover")
                            {
                                int.TryParse(attrvalue, out mMaximumDepthForCrossover);
                            }
                            else if (attrname == "MaxDepthForMutation")
                            {
                                int.TryParse(attrvalue, out mMaximumDepthForMutation);
                            }
                            else if (attrname == "MaxDepthForCreation")
                            {
                                int.TryParse(attrvalue, out mMaximumDepthForCreation);
                            }
                        }
                    }
                }

                else if (xml_level1.Name == "lgp_scripts")
                {
                    foreach (XmlElement xml_level2 in xml_level1.ChildNodes)
                    {
                        if (xml_level2.Name == "script")
                        {
                            string script_name = xml_level2.Attributes["name"].Value;
                            string script_src = xml_level2.Attributes["src"].Value;
                            mScripts[script_name] = script_src;
                        }
                    }
                }
            }

            NormalizeEvolutionRates();
        }

        public double ReproductionRate
        {
            get { return mReproductionRate;  }
            set
            {
                mReproductionRate = value;
            }
        }

        public double ElitismRatio
        {
            get
            {
                return mElitismRatio;
            }
            set
            {
                mElitismRatio = value;
            }
        }

        public void NormalizeEvolutionRates()
        {
            double rate_sum = mCrossoverRate + mMicroMutationRate + mMacroMutationRate + mReproductionRate;
            if (rate_sum == 0) rate_sum = 1;
            mCrossoverRate /= rate_sum;
            mMacroMutationRate /= rate_sum;
            mMicroMutationRate /= rate_sum;
            mReproductionRate /= rate_sum;
        }
    }
}
