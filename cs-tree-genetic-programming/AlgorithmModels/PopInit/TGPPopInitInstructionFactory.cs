using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeGP.AlgorithmModels.PopInit
{
    using System.Xml;
    using TreeGP.Core.ComponentModels;
    using TreeGP.Core.AlgorithmModels.PopInit;
    using TreeGP.ComponentModels;

    public class TGPPopInitInstructionFactory<S> : TGPPopInitInstructionFactory<IGPPop, S>
        where S : IGPSolution
    {
        public TGPPopInitInstructionFactory()
        {

        }

        public TGPPopInitInstructionFactory(string filename)
            : base(filename)
        {

        }
    }

    /// <summary>
    /// As specified in Section 3.4 of "A Field Guide to Genetic Programming", it is common to initialize population randomly using ramped half-and-half with a depth range of 2-6
    /// </summary>
    /// <typeparam name="P">Generic type for population of genetic programs</typeparam>
    /// <typeparam name="S">Generic type for genetic program</typeparam>
    public class TGPPopInitInstructionFactory<P, S> : PopInitInstructionFactory<P, S>
        where S : IGPSolution
        where P : IGPPop
    {
        public static string INITIALIZATION_METHOD_RAMPED_FULL = "Ramped Full";
        public static string INITIALIZATION_METHOD_RAMPED_GROW = "Ramped Grow";
        public static string INITIALIZATION_METHOD_RAMPED_HALF_HALF = "Ramped Half-Half";

        public override string InstructionType
        {
            set
            {
                if (value == TGPProgram.INITIALIZATION_METHOD_FULL)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_Full<P, S>();
                }
                else if (value == TGPProgram.INITIALIZATION_METHOD_GROW)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_Grow<P, S>();
                }
                else if (value == TGPProgram.INITIALIZATION_METHOD_PTC1)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_PTC1<P, S>();
                }
                else if (value == TGPProgram.INITIALIZATION_METHOD_RANDOMBRANCH)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_RandomBranch<P, S>();
                }
                else if (value == INITIALIZATION_METHOD_RAMPED_FULL)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_RampedFull<P, S>();
                }
                else if (value == INITIALIZATION_METHOD_RAMPED_GROW)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_RampedGrow<P, S>();
                }
                else if (value == INITIALIZATION_METHOD_RAMPED_HALF_HALF)
                {
                    mCurrentInstruction = new TGPPopInitInstruction_RampedHalfAndHalf<P, S>();
                }
            }
        }

        public TGPPopInitInstructionFactory()
        {

        }

        public TGPPopInitInstructionFactory(string filename)
            : base(filename)
        {

        }

        protected override PopInitInstruction<P, S> LoadInstructionFromXml(string selected_strategy, XmlElement xml_level1)
        {
            if (selected_strategy == TGPProgram.INITIALIZATION_METHOD_RANDOMBRANCH)
            {
                return new TGPPopInitInstruction_RandomBranch<P, S>(xml_level1);
            }
            else if (selected_strategy == TGPProgram.INITIALIZATION_METHOD_GROW)
            {
                return new TGPPopInitInstruction_Grow<P, S>(xml_level1);
            }
            else if (selected_strategy == TGPProgram.INITIALIZATION_METHOD_FULL)
            {
                return new TGPPopInitInstruction_Full<P, S>(xml_level1);
            }
            else if (selected_strategy == INITIALIZATION_METHOD_RAMPED_GROW)
            {
                return new TGPPopInitInstruction_RampedGrow<P, S>(xml_level1);
            }
            else if (selected_strategy == INITIALIZATION_METHOD_RAMPED_FULL)
            {
                return new TGPPopInitInstruction_RampedFull<P, S>(xml_level1);
            }
            else if (selected_strategy == INITIALIZATION_METHOD_RAMPED_HALF_HALF)
            {
                return new TGPPopInitInstruction_RampedHalfAndHalf<P, S>(xml_level1);
            }
            else if (selected_strategy == TGPProgram.INITIALIZATION_METHOD_PTC1)
            {
                return new TGPPopInitInstruction_PTC1<P, S>(xml_level1);
            }

            return LoadDefaultInstruction();
        }

        protected override PopInitInstruction<P, S> LoadDefaultInstruction()
        {
            return new TGPPopInitInstruction_RampedHalfAndHalf<P, S>();
        }

        public override PopInitInstructionFactory<P, S> Clone()
        {
            TGPPopInitInstructionFactory<P, S> clone = new TGPPopInitInstructionFactory<P, S>(mFilename);
            return clone;
        }


    }
}
