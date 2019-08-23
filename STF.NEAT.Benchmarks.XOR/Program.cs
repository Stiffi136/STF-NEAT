using System;
using System.Collections.Generic;
using System.Linq;
using STF.NEAT;

namespace STF.NEAT.Benchmarks.XOR
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            var inputs = new List<List<float>> {
                new List<float> { 0, 0 },
                new List<float> { 1, 0 },
                new List<float> { 0, 1 },
                new List<float> { 1, 1 }
            };
            var expectedOutput = new List<float> {
                0,
                1,
                1,
                0
            };
            int genomeTargetCount = 100;

            Evaluator.LoadConfig(new NEATConfig
            {
                AddBiasSensor = true,
                SensorNodeCount = 2,
                OutputNodeCount = 1,
                AllowFeedForwardOnly = true,
                ClearValuesBeforeComputing = false,
                ClearValuesBeforeReplicating = true,

                DisjointCoefficient = 0.6f,
                ExcessCoefficient = 0.6f,
                WeightCoefficient = 0.4f,
                CompatibilityThreshold = 2f,

                MutationPower = 2f,
                UseWeightCap = true,
                WeightCap = 8f,

                UseSpeciesControl = false,
                TargetSpeciesAmount = 10,
                CompatibilityModifier = 1f,

                UseMaxFitnessInsteadOfShared = false,

                DoInitialLinkMutation = false,

                DebugNodes = false,
                DebugConnections = false
            });

            Evaluator.Reset();
            Evaluator.CreateGenome(genomeTargetCount);
            Evaluator.Random = new Random();

            while (Evaluator.Generation <= 1000)
            {
                //int rngNumber = random.Next(10000);
                //inputs.Shuffle(rngNumber);
                //expectedOutput.Shuffle(rngNumber);
                for (int i = 0; i < Evaluator.Genomes.Count; i++)
                {
                    float fitness = 4;
                    for (int j = 0; j < expectedOutput.Count; j++)
                    {
                        var result = Evaluator.ComputeGenome(i, inputs[j])[0];
                        var output = expectedOutput[j];
                        var bla = (float)Math.Pow(result - output, 2);
                        fitness -= bla;
                    }
                    Evaluator.MapCalculatedFitnessSingle(i, fitness);
                }
                Evaluator.Replication();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Generation: {Evaluator.Generation}, Genome Count: {Evaluator.Genomes.Count}, Species Count: {Evaluator.Species.Count}, Global Champ Fitness: {Evaluator.GlobalChampion.Fitness}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Global Champ Nodes: {Evaluator.GlobalChampion.NodeGenes.Count}, Global Champ Connections: {Evaluator.GlobalChampion.ConnectionGenes.Count}");
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var species in Evaluator.Species)
                {
                    if (species.Champion != null)
                    {
                        Console.WriteLine($"  Species ID: {species.ID}, Genome Count: {species.Genomes.Count}, Champ Fitness: {species.Champion.Fitness}");
                    }
                    else
                    {
                        Console.WriteLine($"  Species ID: {species.ID}, Genome Count: {species.Genomes.Count}");
                    }

                }
                Console.ResetColor();
            }
        }
    }
}


// 1. DESTROYEDTOOSOON
// 2. AMBUSHNOW
// 3. 2019GAMESCOM
// 4. FEELMYGUN
// 5. HIDDENBUTFOUND
// 6. RICOCHETOHNO
// 7. LIVINGONTHEEDGE
// 8. DESTROYTHEMALL
// 9. CANTBELIEVEISURVIVED
// 10. TOTALCONTROL
// 11. CAMPINGTIME