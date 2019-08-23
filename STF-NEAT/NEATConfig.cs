using System;
using System.Collections.Generic;
using System.Text;

namespace STF.NEAT
{
    public class NEATConfig
    {
        // Network parameters
        public bool AddBiasSensor = true;
        public int SensorNodeCount = 1;
        public int OutputNodeCount = 5;
        public bool AllowFeedForwardOnly = false;
        public bool ClearValuesBeforeComputing = true;
        public bool ClearValuesBeforeReplicating = true;

        // Speciation parameters
        public float ExcessCoefficient = 1f;
        public float DisjointCoefficient = 1f;
        public float WeightCoefficient = 0.4f;
        public float CompatibilityThreshold = 3f;

        // Stagnation
        public bool UseStagnation = false;
        public int StagnationThreshold = 20;

        // Percentage of genomes in each species to get killed during selection
        public float SpeciesKillPercentage = 0.5f;

        // Percentage of clones to produce during replication. The rest will get produced by crossover
        public float ClonePercentage = 0.25f;

        // Mutation chances
        public float WeightMutationChance = 0.8f;
        public float AdjustWeightChance = 0.9f;
        public float NewConnectionChance = 0.05f;
        public float NewNodeChance = 0.03f;

        // Mutation behaviours
        public float MutationPower = 2f;
        public bool DoInitialLinkMutation = false;
        public bool UseWeightCap = false;
        public float WeightCap = 8f;

        // Breeding
        public float InterspeciesMatingRate = 0.001f;

        // Species Control
        public bool UseSpeciesControl = false;
        public float CompatibilityModifier = 0.3f;
        public int TargetSpeciesAmount = 10;

        // Speciation
        public bool UseMaxFitnessInsteadOfShared = false;

        // Debug
        public bool DebugNodes = false;
        public bool DebugConnections = false;
    }
}
