namespace STF.NEAT
{
    public class NodeGene
    {
        public enum NodeType
        {
            SENSOR,
            OUTPUT,
            HIDDEN
        };

        public int ID;
        public NodeType Type;
        public float Value;
        public bool Calculated;

        public NodeGene(int id, NodeType type)
        {
            ID = id;
            Type = type;
            Value = 0f;
            Calculated = false;
        }

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
