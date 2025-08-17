using System.Collections;
using System.Collections.Generic;

namespace STF.NEAT
{
    public class ConnectionGene(int innovationNumber, int inNode, int outNode, float weight, bool enabled)
    {
        public int InnovationNumber { get; set; } = innovationNumber;
        public int InNode { get; set; } = inNode;
        public int OutNode { get; set; } = outNode;
        public float Weight { get; set; } = weight;
        public bool Enabled { get; set; } = enabled;

        public ConnectionGene Copy()
        {
            return new ConnectionGene(InnovationNumber, InNode, OutNode, Weight, Enabled);
        }
    }
}
