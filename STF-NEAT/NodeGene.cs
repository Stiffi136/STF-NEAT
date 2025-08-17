namespace STF.NEAT
{
    public class NodeGene(int id, NodeGene.NodeType type)
    {
        public enum NodeType
        {
            SENSOR,
            OUTPUT,
            HIDDEN
        };

        public int ID { get; set; } = id;
        public NodeType Type { get; set; } = type;
        public float Value { get; set; } = 0f;
        public bool Calculated { get; set; } = false;

        public void ResetValueAndCalculatedFlag()
        {
            Value = 0f;
            Calculated = false;
        }

        public void ResetCalculatedFlag()
        {
            Calculated = false;
        }

        public NodeGene Copy()
        {
            return new NodeGene(ID, Type);
        }
    }
}
