using System.Collections;
using System.Collections.Generic;

namespace STF.NEAT
{
    public class ConnectionGene
    {
        public int InnovationNumber;
        public int InNode;
        public int OutNode;
        public float Weight;
        public bool Enabled;

        public ConnectionGene(int innovationNumber, int inNode, int outNode, float weight, bool enabled)
        {
            InnovationNumber = innovationNumber;
            InNode = inNode;
            OutNode = outNode;
            Weight = weight;
            Enabled = enabled;
        }

        public ConnectionGene Copy()
        {
            return new ConnectionGene(InnovationNumber, InNode, OutNode, Weight, Enabled);
        }
    }
}
