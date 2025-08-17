using System;
using System.Collections.Generic;
using System.Text;

namespace STF.NEAT
{
    public class NEATConfig
    {
        // Network parameters
        public bool AddBiasSensor { get; set; } = true;
        public int SensorNodeCount { get; set; } = 1;
        public int OutputNodeCount { get; set; } = 5;
        public bool AllowFeedForwardOnly { get; set; } = false;
        public bool ClearValuesBeforeComputing { get; set; } = true;
        public bool ClearValuesBeforeReplicating { get; set; } = true;

        // Speciation parameters
        public float ExcessCoefficient { get; set; } = 1f;
        public float DisjointCoefficient { get; set; } = 1f;
        public float WeightCoefficient { get; set; } = 0.4f;
        public float CompatibilityThreshold { get; set; } = 3f;

        // Stagnation
        public bool UseStagnation { get; set; } = false;
        public int StagnationThreshold { get; set; } = 20;

        // Percentage of genomes in each species to get killed during selection
        public float SpeciesKillPercentage { get; set; } = 0.5f;

        // Percentage of clones to produce during replication. The rest will get produced by crossover
        public float ClonePercentage { get; set; } = 0.25f;

        // Mutation chances
        public float WeightMutationChance { get; set; } = 0.8f;
        public float AdjustWeightChance { get; set; } = 0.9f;
        public float NewConnectionChance { get; set; } = 0.05f;
        public float NewNodeChance { get; set; } = 0.03f;

        // Mutation behaviours
        public float MutationPower { get; set; } = 2f;
        public bool DoInitialLinkMutation { get; set; } = false;
        public bool UseWeightCap { get; set; } = false;
        public float WeightCap { get; set; } = 8f;

        // Breeding
        public float InterspeciesMatingRate { get; set; } = 0.001f;

        // Species Control
        public bool UseSpeciesControl { get; set; } = false;
        public float CompatibilityModifier { get; set; } = 0.3f;
        public int TargetSpeciesAmount { get; set; } = 10;

        // Speciation
        public bool UseMaxFitnessInsteadOfShared { get; set; } = false;

        // Debug
        public bool DebugNodes { get; set; } = false;
        public bool DebugConnections { get; set; } = false;
    }
}
