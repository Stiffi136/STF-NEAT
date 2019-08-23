using System;
using System.Collections.Generic;
using System.Linq;

namespace STF.NEAT
{
    public class Species
    {
        public Genome Champion;
        public Genome Representative;
        public List<Genome> Genomes;
        public float AdjustedFitness;
        public float SharedFitness;
        public int ID;
        public int StagnationCounter;
        public float MaxFitness;
        public bool Extinct;

        public Species(Genome representative, int id, float maxFitness, int stagnationCounter)
        {
            Representative = representative;
            Genomes = new List<Genome>();
            Genomes.Add(representative);
            ID = id;
            MaxFitness = maxFitness;
            StagnationCounter = stagnationCounter;
        }

        public void TryDeclareChampion()
        {
            if (Genomes.Count > 5)
            {
                Genomes = Genomes.OrderBy(x => x.Fitness).ToList();
                Champion = Genomes.Last();
            }
        }

        public void CalculateMaxFitness()
        {
            Genomes = Genomes.OrderBy(x => x.Fitness).ToList();
            MaxFitness = Genomes.Last().Fitness;
        }

        public void CalculateSharedFitness()
        {
            SharedFitness = 0f;
            foreach (var genome in Genomes)
            {
                SharedFitness += Math.Max(genome.Fitness / Genomes.Count, 1);
            }
        }

        public void CalculateAdjustedFitness()
        {
            float result = 0f;
            if (Evaluator.NeatConfig.UseMaxFitnessInsteadOfShared)
            {
                CalculateMaxFitness();
                result = MaxFitness;
            }
            else
            {
                CalculateSharedFitness();
                result = SharedFitness;
            }
            AdjustedFitness = result;
        }
    }
}