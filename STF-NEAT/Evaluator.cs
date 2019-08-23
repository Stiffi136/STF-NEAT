using System;
using System.Collections.Generic;
using System.Linq;

namespace STF.NEAT
{
    public static class Evaluator
    {
        public static NEATConfig NeatConfig { get; private set; } = new NEATConfig();
        public static Dictionary<int, ConnectionGene> GlobalConnectionGenes = new Dictionary<int, ConnectionGene>();
        public static List<Genome> Genomes = new List<Genome>();
        public static List<Species> Species = new List<Species>();
        public static Genome GlobalChampion;
        public static Dictionary<int, NodeGene.NodeType> GlobalNodeGenes = new Dictionary<int, NodeGene.NodeType>();

        public static Random Random = new Random();

        public static int Generation = 0;

        private static int lastInnovationNumber;
        private static int lastNodeID = sensorNodeCount + outputNodeCount;
        private static int lastSpeciesID = 0;
        private static int genomeTargetPopulation = 0;
        private static float currentCompatibilityThreshold = NeatConfig.CompatibilityThreshold;
        private static int sensorNodeCount = NeatConfig.SensorNodeCount;
        private static int outputNodeCount = NeatConfig.OutputNodeCount;

        public static void LoadConfig(NEATConfig config)
        {
            NeatConfig = config;
        }

        public static void Reset()
        {
            GlobalConnectionGenes = new Dictionary<int, ConnectionGene>();
            Genomes = new List<Genome>();
            Species = new List<Species>();
            GlobalChampion = null;
            Generation = 0;
            lastInnovationNumber = 0;
            lastNodeID = sensorNodeCount + outputNodeCount;
            lastSpeciesID = 0;
            genomeTargetPopulation = 0;
            currentCompatibilityThreshold = NeatConfig.CompatibilityThreshold;
            sensorNodeCount = NeatConfig.SensorNodeCount;
            if (NeatConfig.AddBiasSensor)
                sensorNodeCount++;
            outputNodeCount = NeatConfig.OutputNodeCount;
        }

        public static ConnectionGene GetMatchingConnectionGene(int inNode, int outNode)
        {
            foreach (var conn in GlobalConnectionGenes)
            {
                if (conn.Value.InNode == inNode && conn.Value.OutNode == outNode)
                {
                    var connectionGene = conn.Value;
                    return new ConnectionGene(connectionGene.InnovationNumber, connectionGene.InNode, connectionGene.OutNode, connectionGene.Weight, true);
                }
            }

            return null;
        }

        public static int NextAvailableInnovationNumber()
        {
            lastInnovationNumber++;
            return lastInnovationNumber;
        }

        public static int NextAvailableNodeID()
        {
            lastNodeID++;
            return lastNodeID;
        }

        public static int NextAvailableSpeciesID()
        {
            lastSpeciesID++;
            return lastSpeciesID;
        }

        public static Genome Crossover(Genome parent1, Genome parent2)
        {
            var Offspring = new Genome(sensorNodeCount, outputNodeCount);
            var higherFitnessParent = (parent1.Fitness > parent2.Fitness) ? parent1.ConnectionGenes : parent2.ConnectionGenes;
            var lowerFitnessparent = (parent1.Fitness <= parent2.Fitness) ? parent1.ConnectionGenes : parent2.ConnectionGenes;
            bool equalParents = (parent1.Fitness == parent2.Fitness);

            var sortedConnections = GlobalConnectionGenes.Values.ToList();
            sortedConnections = sortedConnections.OrderBy(x => x.InnovationNumber).ToList();

            foreach (var connection in sortedConnections)
            {
                ConnectionGene geneToTake = null;
                bool checkDisableStatus = false;

                if (equalParents)
                {
                    if (parent1.ConnectionGenes.ContainsKey(connection.InnovationNumber) && parent2.ConnectionGenes.ContainsKey(connection.InnovationNumber))
                    {
                        var g1 = parent1.ConnectionGenes[connection.InnovationNumber];
                        var g2 = parent2.ConnectionGenes[connection.InnovationNumber];

                        if (Random.NextDouble() > 0.5f)
                        {
                            geneToTake = g1.Copy();
                        }
                        else
                        {
                            geneToTake = g2.Copy();
                        }

                        if (!g1.Enabled || !g2.Enabled)
                        {
                            if (g1.Enabled || g2.Enabled)
                            {
                                checkDisableStatus = true;
                            }
                        }
                    }

                    if (parent1.ConnectionGenes.ContainsKey(connection.InnovationNumber) && !parent2.ConnectionGenes.ContainsKey(connection.InnovationNumber))
                    {
                        var g = parent1.ConnectionGenes[connection.InnovationNumber];

                        if (Random.NextDouble() < 0.5f)
                        {
                            geneToTake = g.Copy();

                            if (!g.Enabled)
                            {
                                checkDisableStatus = true;
                            }
                        }
                    }

                    if (!parent1.ConnectionGenes.ContainsKey(connection.InnovationNumber) && parent2.ConnectionGenes.ContainsKey(connection.InnovationNumber))
                    {
                        var g = parent2.ConnectionGenes[connection.InnovationNumber];

                        if (Random.NextDouble() < 0.5f)
                        {
                            geneToTake = g.Copy();

                            if (!g.Enabled)
                            {
                                checkDisableStatus = true;
                            }
                        }
                    }
                }
                else
                {
                    if (higherFitnessParent.ContainsKey(connection.InnovationNumber) && lowerFitnessparent.ContainsKey(connection.InnovationNumber))
                    {
                        var g1 = higherFitnessParent[connection.InnovationNumber];
                        var g2 = lowerFitnessparent[connection.InnovationNumber];

                        if (Random.NextDouble() < 0.5f)
                        {
                            geneToTake = g1.Copy();
                        }
                        else
                        {
                            geneToTake = g2.Copy();
                        }

                        if (!g1.Enabled || !g2.Enabled)
                        {
                            if (g1.Enabled || g2.Enabled)
                            {
                                checkDisableStatus = true;
                            }
                        }
                    }

                    if (higherFitnessParent.ContainsKey(connection.InnovationNumber) && !lowerFitnessparent.ContainsKey(connection.InnovationNumber))
                    {
                        var g = higherFitnessParent[connection.InnovationNumber];

                        geneToTake = g.Copy();

                        if (!g.Enabled)
                        {
                            checkDisableStatus = true;
                        }
                    }
                }

                if (checkDisableStatus)
                {
                    if (Random.NextDouble() < 0.75f)
                        geneToTake.Enabled = false;
                    else
                        geneToTake.Enabled = true;
                }

                if (geneToTake != null)
                {
                    Offspring.ConnectionGenes.Add(geneToTake.InnovationNumber, geneToTake);
                    AddNodeToOffspring(Offspring, geneToTake);
                }
            }

            return Offspring;
        }

        public static Genome CreateGenome()
        {
            GlobalNodeGenes.Clear();
            for (int i = 0; i < sensorNodeCount; i++)
            {
                GlobalNodeGenes.Add(i + 1, NodeGene.NodeType.SENSOR);
            }
            for (int i = 0; i < outputNodeCount; i++)
            {
                GlobalNodeGenes.Add(sensorNodeCount + i + 1, NodeGene.NodeType.OUTPUT);
            }

            var newGenome = new Genome(sensorNodeCount, outputNodeCount);
            Genomes.Add(newGenome);
            genomeTargetPopulation++;
            return newGenome;
        }
        public static List<Genome> CreateGenome(int count)
        {
            GlobalNodeGenes.Clear();
            for (int i = 0; i < sensorNodeCount; i++)
            {
                GlobalNodeGenes.Add(i + 1, NodeGene.NodeType.SENSOR);
            }
            for (int i = 0; i < outputNodeCount; i++)
            {
                GlobalNodeGenes.Add(sensorNodeCount + i + 1, NodeGene.NodeType.OUTPUT);
            }

            var newGenomes = new List<Genome>();
            for (int i = 0; i < count; i++)
            {
                var newGenome = new Genome(sensorNodeCount, outputNodeCount);
                Genomes.Add(newGenome);
                newGenomes.Add(newGenome);
                genomeTargetPopulation++;
            }
            return newGenomes;
        }

        public static List<List<float>> ComputeAll(List<float> sensorValues)
        {
            var sensorValuesCopy = sensorValues.ToList();
            if (NeatConfig.AddBiasSensor)
                sensorValuesCopy.Insert(0, 1f);

            List<List<float>> results = new List<List<float>>();
            foreach (var genome in Genomes)
            {
                results.Add(genome.Compute(sensorValuesCopy));
            }
            return results;
        }

        public static List<float> ComputeGenome(int genomeIndex, List<float> sensorValues)
        {
            var sensorValuesCopy = sensorValues.ToList();
            if (NeatConfig.AddBiasSensor)
                sensorValuesCopy.Insert(0, 1f);

            List<float> result = Genomes[genomeIndex].Compute(sensorValuesCopy);
            return result;
        }

        public static void MapCalculatedFitness(List<float> fitnessValues)
        {
            for (int i = 0; i < fitnessValues.Count; i++)
            {
                Genomes[i].Fitness = Math.Max(fitnessValues[i], 0f);
            }
        }

        public static void MapCalculatedFitnessSingle(int genomeIndex, float fitnessValue)
        {
            Genomes[genomeIndex].Fitness = Math.Max(fitnessValue, 0f);
        }

        // Select, Replicate, Mutate
        public static void Replication()
        {
            Genomes = Genomes.OrderBy(x => x.Fitness).ToList();

            if (NeatConfig.ClearValuesBeforeReplicating)
            {
                foreach(var genome in Genomes)
                {
                    genome.ResetGenome(true);
                }
            }

            if (Generation > 0)
            {
                // Eliminate bad performing genomes in each species
                Selection();
            }

            // Speciate by comparing current gen with previous gen representatives
            Speciate();

            // Declare champions
            GlobalChampion = Genomes.Last();
            foreach (var species in Species)
            {
                species.TryDeclareChampion();
            }

            // Calculate the total adjusted fitness for later use
            foreach (var species in Species)
            {
                species.CalculateAdjustedFitness();
            }

            // Remove stagnating species if config flag is set
            if (NeatConfig.UseStagnation)
                CheckSpeciesStagnation();

            float totalAdjustedFitness = 0f;
            foreach (var species in Species)
            {
                totalAdjustedFitness += species.AdjustedFitness;
            }

            // OLD SELECTION POS


            // Breed entire new population (except champions) based on adjusted fitness scores
            Genomes.Clear();

            var extinctSpecies = new List<Species>();
            Species = Species.OrderBy(x => x.AdjustedFitness).ToList();


            int resultingPopulation = 0;
            foreach (var species in Species)
            {
                resultingPopulation += (int)Math.Floor((species.AdjustedFitness / totalAdjustedFitness) * genomeTargetPopulation);
            }
            int unassignedPopulation = genomeTargetPopulation - resultingPopulation;


            foreach (var species in Species)
            {
                int adjustedReplicationAmount = (int)Math.Floor((species.AdjustedFitness / totalAdjustedFitness) * genomeTargetPopulation);
                if (species.Champion != null)
                {
                    adjustedReplicationAmount--;
                }

                int bonusOffspring = (int)Math.Floor(unassignedPopulation * (double)((Species.IndexOf(species) + 1) / Species.Count));
                unassignedPopulation -= bonusOffspring;

                adjustedReplicationAmount += bonusOffspring;

                List<Genome> nextGen = new List<Genome>();

                int toBreed = (int)Math.Round(adjustedReplicationAmount * (1 - NeatConfig.ClonePercentage));
                int toClone = (int)Math.Round(adjustedReplicationAmount * NeatConfig.ClonePercentage);
                nextGen.AddRange(Breed(toBreed, species));
                nextGen.AddRange(Clone(toClone, species));
                MutateList(nextGen);

                species.Genomes.Clear();
                species.Genomes.AddRange(nextGen);
                if (species.Genomes.Count == 0)
                    extinctSpecies.Add(species);

                Genomes.AddRange(nextGen);

                if (species.Champion != null)
                {
                    species.Genomes.Add(species.Champion);
                    Genomes.Add(species.Champion);
                }
            }

            foreach(var species in extinctSpecies)
            {
                Species.Remove(species);
            }


            Generation++;
        }


        private static void MutateList(List<Genome> genomeList)
        {
            foreach (var genome in genomeList)
            {
                Mutate(genome);
            }
        }

        private static void Mutate(Genome genome)
        {
            if (Random.NextDouble() <= NeatConfig.WeightMutationChance)
            {
                if (Random.NextDouble() <= NeatConfig.AdjustWeightChance)
                {
                    genome.MutateWeightShift();
                }
                else
                {
                    genome.MutateNewWeight();
                }
            }
            else
            {
                genome.MutateLinkToggle();
            }

            if (Random.NextDouble() <= NeatConfig.NewConnectionChance)
            {
                genome.MutateLink();
            }

            if (Random.NextDouble() <= NeatConfig.NewNodeChance)
            {
                genome.MutateNode();
            }
        }

        private static void Selection()
        {
            Genomes.Clear();
            foreach (var species in Species)
            {
                species.Genomes = species.Genomes.OrderBy(x => x.Fitness).ToList();
                int toKill = (int)Math.Ceiling(species.Genomes.Count * NeatConfig.SpeciesKillPercentage);
                species.Genomes.RemoveRange(0, toKill);
                Genomes.AddRange(species.Genomes);
            }
        }

        private static List<Genome> Clone(int count, Species species)
        {
            var g = species.Genomes;
            g = g.OrderBy(x => x.Fitness).ToList();
            List<Genome> result = new List<Genome>();
            float max = g.Count;

            for (int i = 0; i < count; i++)
            {
                bool choosing = true;
                while (choosing)
                {
                    var randomIndex = Random.Next(0, g.Count);
                    if (Random.NextDouble() <= Math.Pow(randomIndex + 1 / (max), 2))
                    {
                        result.Add(g[randomIndex].Copy());
                        choosing = false;
                    }
                }
            }
            return result;
        }

        private static List<Genome> Breed(int count, Species species)
        {
            var g = species.Genomes;
            g = g.OrderBy(x => x.Fitness).ToList();
            List<Genome> result = new List<Genome>();

            float max = g.Count;

            for (int i = 0; i < count; i++)
            {
                Genome parent1 = null;
                Genome parent2 = null;

                bool choosingFirst = true;
                while (choosingFirst)
                {
                    var randomIndex = Random.Next(0, g.Count);
                    if (Random.NextDouble() <= Math.Pow(randomIndex + 1 / (max), 2))
                    {
                        parent1 = g[randomIndex];
                        choosingFirst = false;
                    }
                }

                bool choosingSecond = true;
                if (Random.NextDouble() < NeatConfig.InterspeciesMatingRate && Species.Count > 1)
                {
                    var otherSpecies = Species.Where(x => x != species && x.Genomes.Count > 0).ToList();
                    var randomSpeciesIndex = Random.Next(0, otherSpecies.Count);
                    var randomGenomeIndex = Random.Next(0, otherSpecies[randomSpeciesIndex].Genomes.Count);
                    parent2 = otherSpecies[randomSpeciesIndex].Genomes[randomGenomeIndex];
                }
                else
                {
                    while (choosingSecond)
                    {
                        var randomIndex = Random.Next(0, g.Count);
                        if (Random.NextDouble() <= Math.Pow(randomIndex + 1 / (max), 2))
                        {
                            parent2 = g[randomIndex];
                            choosingSecond = false;
                        }
                    }
                }

                var offspring = Crossover(parent1, parent2);
                result.Add(offspring);
            }
            return result;
        }

        private static void Speciate()
        {
            if (NeatConfig.UseSpeciesControl && Generation > 0)
            {
                if (Species.Count > NeatConfig.TargetSpeciesAmount)
                    currentCompatibilityThreshold += NeatConfig.CompatibilityModifier;
                if (Species.Count < NeatConfig.TargetSpeciesAmount)
                    currentCompatibilityThreshold -= NeatConfig.CompatibilityModifier;
            }

            Dictionary<int, Species> nextGenSpecies = new Dictionary<int, Species>();
            foreach (var genome in Genomes)
            {
                bool matched = false;
                if (Generation == 0 || Species.Count == 0)
                {
                    foreach (var species in nextGenSpecies.Values)
                    {
                        float comp = CalculateCompability(genome, species.Representative);
                        if (comp < currentCompatibilityThreshold)
                        {
                            if (nextGenSpecies.ContainsKey(species.ID))
                            {
                                nextGenSpecies[species.ID].Genomes.Add(genome);
                            }
                            else
                            {
                                var id = NextAvailableSpeciesID();
                                nextGenSpecies.Add(id, new Species(genome, id, genome.Fitness, 0));
                            }
                            matched = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var species in Species)
                    {
                        float comp = CalculateCompability(genome, species.Representative);
                        if (comp < currentCompatibilityThreshold)
                        {
                            if (nextGenSpecies.ContainsKey(species.ID))
                            {
                                nextGenSpecies[species.ID].Genomes.Add(genome);
                            }
                            else
                            {
                                nextGenSpecies.Add(species.ID, new Species(genome, species.ID, species.MaxFitness, species.StagnationCounter));
                            }
                            matched = true;
                            break;
                        }
                    }
                }
                if (!matched)
                {
                    var id = NextAvailableSpeciesID();
                    nextGenSpecies.Add(id, new Species(genome, id, genome.Fitness, 0));
                }
            }
            Species = nextGenSpecies.Values.ToList();
        }

        private static void CheckSpeciesStagnation()
        {
            foreach(var species in Species)
            {
                if (species.Champion == null)
                {
                    species.Genomes = species.Genomes.OrderBy(x => x.Fitness).ToList();
                    if (species.Genomes.Last().Fitness <= species.MaxFitness)
                    {
                        species.StagnationCounter++;
                    }
                    else
                    {
                        species.StagnationCounter = 0;
                    }
                }
                else
                {
                    if (species.Champion.Fitness <= species.MaxFitness)
                    {
                        species.StagnationCounter++;
                    }
                    else
                    {
                        species.StagnationCounter = 0;
                    }
                }

                if (species.StagnationCounter > NeatConfig.StagnationThreshold && Species.Count > 1)
                    species.AdjustedFitness = 0;
            }
        }

        private static float CalculateCompability(Genome genome, Genome representative)
        {
            int floatingCountGenome = 0;
            int floatingCountRepresentative = 0;

            int excess;
            int disjoint = 0;
            int matching = 0;
            int geneCount = Math.Max(genome.ConnectionGenes.Count, representative.ConnectionGenes.Count);

            if (geneCount <= 0)
                return 0f;

            float averageWeightDiff = 0f;

            float compatibility;

            foreach (var gene in GlobalConnectionGenes.Values)
            {
                if (genome.ConnectionGenes.ContainsKey(gene.InnovationNumber) && representative.ConnectionGenes.ContainsKey(gene.InnovationNumber))
                {
                    disjoint += floatingCountGenome;
                    disjoint += floatingCountRepresentative;
                    floatingCountGenome = 0;
                    floatingCountRepresentative = 0;

                    averageWeightDiff += Math.Abs(genome.ConnectionGenes[gene.InnovationNumber].Weight - representative.ConnectionGenes[gene.InnovationNumber].Weight);
                    matching++;
                }
                if (!genome.ConnectionGenes.ContainsKey(gene.InnovationNumber) && representative.ConnectionGenes.ContainsKey(gene.InnovationNumber))
                {
                    disjoint += floatingCountGenome;
                    floatingCountGenome = 0;
                    floatingCountRepresentative++;
                }
                if (genome.ConnectionGenes.ContainsKey(gene.InnovationNumber) && !representative.ConnectionGenes.ContainsKey(gene.InnovationNumber))
                {
                    disjoint += floatingCountRepresentative;
                    floatingCountRepresentative = 0;
                    floatingCountGenome++;
                }
            }
            excess = floatingCountRepresentative + floatingCountGenome;

            compatibility = (NeatConfig.ExcessCoefficient * excess / geneCount) + (NeatConfig.DisjointCoefficient * disjoint / geneCount);
            if (matching > 0)
            {
                averageWeightDiff = averageWeightDiff / matching;
                compatibility += (NeatConfig.WeightCoefficient * averageWeightDiff);
            }

            return compatibility;
        }

        private static void AddNodeToOffspring(Genome offspring, ConnectionGene connection)
        {
            var inNode = connection.InNode;
            var outNode = connection.OutNode;
            if (!offspring.NodeGenes.ContainsKey(inNode))
            {
                offspring.NodeGenes.Add(inNode, new NodeGene(inNode, NodeGene.NodeType.HIDDEN));
            }
            if (!offspring.NodeGenes.ContainsKey(outNode))
            {
                offspring.NodeGenes.Add(outNode, new NodeGene(outNode, NodeGene.NodeType.HIDDEN));
            }
        }

    }
}
