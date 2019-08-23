using System;
using System.Collections.Generic;
using System.Linq;

namespace STF.NEAT
{
    public class Genome
    {
        public Dictionary<int, NodeGene> NodeGenes;
        public Dictionary<int, ConnectionGene> ConnectionGenes;
        public float Fitness;

        private ILookup<int, ConnectionGene> connectionGenesByOutNode { get => ConnectionGenes.ToLookup(x => x.Value.OutNode, x => x.Value); }
        private List<NodeGene> sensorNodes { get => NodeGenes.Values.ToList().FindAll(x => x.Type == NodeGene.NodeType.SENSOR); }
        private List<NodeGene> outputNodes { get => NodeGenes.Values.ToList().FindAll(x => x.Type == NodeGene.NodeType.OUTPUT); }
        private Random random = Evaluator.Random;
        private int randomNodeIndex { get => random.Next(0, NodeGenes.Count); }
        private float randomWeight { get => (float)random.NextDouble()*4-2; }

        public Genome(int sensors, int outputs)
        {
            NodeGenes = new Dictionary<int, NodeGene>();
            ConnectionGenes = new Dictionary<int, ConnectionGene>();

            for (int i = NodeGenes.Count; i < sensors; i++)
            {
                var id = i + 1;
                NodeGenes.Add(id, new NodeGene(id, NodeGene.NodeType.SENSOR));
            }

            for (int i = sensors; i < sensors + outputs; i++)
            {
                var id = i + 1;
                NodeGenes.Add(id, new NodeGene(id, NodeGene.NodeType.OUTPUT));
            }

            if (Evaluator.NeatConfig.DoInitialLinkMutation)
            {
                MutateLink();
            }
        }
        public Genome()
        {
            NodeGenes = new Dictionary<int, NodeGene>();
            ConnectionGenes = new Dictionary<int, ConnectionGene>();
        }

        public Genome Copy()
        {
            var newGenome = new Genome();
            foreach (var node in NodeGenes.Values)
            {
                newGenome.NodeGenes.Add(node.ID, node.Copy());
            }
            foreach (var connection in ConnectionGenes.Values)
            {
                newGenome.ConnectionGenes.Add(connection.InnovationNumber, connection.Copy());
            }
            return newGenome;
        }

        public List<float> Compute(List<float> sensors)
        {
            ResetGenome(Evaluator.NeatConfig.ClearValuesBeforeComputing);
            for (int i = 0; i < sensorNodes.Count; i++)
            {
                sensorNodes[i].Value = sensors[i];
            }


            foreach(var node in outputNodes)
            {
                CalculateNodeValue(node.ID);
            }

            var result = new List<float>();
            foreach(var node in outputNodes)
            {
                result.Add(node.Value);
            }
            
            if (Evaluator.NeatConfig.DebugNodes)
            {
                foreach (var node in NodeGenes.Values)
                {
                    Console.WriteLine($"Node ID: {node.ID}, Calculated: {node.Calculated}, Type: {node.Type.ToString()}, Value: {node.Value}");
                }
            }

            if (Evaluator.NeatConfig.DebugConnections)
            {
                foreach (var conn in ConnectionGenes.Values)
                {
                    Console.WriteLine($"Conn ID: {conn.InnovationNumber}, In Node: {conn.InNode}, Out Node: {conn.OutNode}, Weight: {conn.Weight}");
                }
            }

            return result;
        }

        public void MutateLink()
        {
            int inNode;
            int outNode;

            do
            {
                inNode = NodeGenes.ElementAt(randomNodeIndex).Key;
                outNode = NodeGenes.ElementAt(randomNodeIndex).Key;

            } while (Evaluator.NeatConfig.AllowFeedForwardOnly && CheckRecurring(inNode, outNode));

            AddNewConnection(inNode, outNode, randomWeight);
        }

        public void MutateNode()
        {
            if (ConnectionGenes.Count > 0)
            {
                var randomConnection = ConnectionGenes.Values.ToList()[random.Next(0, ConnectionGenes.Count)];

                var newNode = new NodeGene(Evaluator.NextAvailableNodeID(), NodeGene.NodeType.HIDDEN);
                NodeGenes.Add(newNode.ID, newNode);
                Evaluator.GlobalNodeGenes.Add(newNode.ID, newNode.Type);

                int inNode = randomConnection.InNode;
                int outNode = randomConnection.OutNode;

                AddNewConnection(inNode, newNode.ID, 1f);

                AddNewConnection(newNode.ID, outNode, randomConnection.Weight);

                ConnectionGenes[randomConnection.InnovationNumber].Enabled = false;
            }
        }

        public void MutateLinkToggle()
        {
            if (ConnectionGenes.Count > 0)
            {
                var randomConnection = ConnectionGenes.Values.ToList()[random.Next(0, ConnectionGenes.Count)].InnovationNumber;

                ConnectionGenes[randomConnection].Enabled = !ConnectionGenes[randomConnection].Enabled;
            }
        }

        public void MutateWeightShift()
        {
            if (ConnectionGenes.Count > 0)
            {
                var randomConnection = ConnectionGenes.Values.ToList()[random.Next(0, ConnectionGenes.Count)].InnovationNumber;

                float newWeight = ConnectionGenes[randomConnection].Weight * (float)random.NextDouble() * Evaluator.NeatConfig.MutationPower * 2 - 2;

                if (Evaluator.NeatConfig.UseWeightCap)
                    newWeight = (float)Math.Clamp(newWeight, Evaluator.NeatConfig.WeightCap * -1, Evaluator.NeatConfig.WeightCap);

                ConnectionGenes[randomConnection].Weight = newWeight;
            }
        }

        public void MutateNewWeight()
        {
            if (ConnectionGenes.Count > 0)
            {
                var randomConnection = ConnectionGenes.Values.ToList()[random.Next(0, ConnectionGenes.Count)].InnovationNumber;

                ConnectionGenes[randomConnection].Weight = randomWeight;
            }
        }

        private void AddNewConnection(int inNode, int outNode, float weight)
        {
            var matchingConnection = Evaluator.GetMatchingConnectionGene(inNode, outNode);
            if (matchingConnection == null)
            {
                int innovationNumber = Evaluator.NextAvailableInnovationNumber();
                var newConnection = new ConnectionGene(innovationNumber, inNode, outNode, weight, true);
                ConnectionGenes.Add(innovationNumber, newConnection);
                Evaluator.GlobalConnectionGenes.Add(innovationNumber, newConnection.Copy());
            }
            else if (!ConnectionGenes.ContainsKey(matchingConnection.InnovationNumber))
            {
                var copyMatch = matchingConnection.Copy();
                copyMatch.Weight = weight;
                ConnectionGenes.Add(copyMatch.InnovationNumber, copyMatch);
            }
        }

        private bool CheckRecurring(int inNode, int outNode)
        {
            var inNodeType = Evaluator.GlobalNodeGenes[inNode];
            var outNodeType = Evaluator.GlobalNodeGenes[outNode];

            if (inNode == outNode)
                return true;
            if (outNodeType == NodeGene.NodeType.SENSOR)
                return true;
            if (inNodeType == NodeGene.NodeType.OUTPUT)
                return true;
            if (inNode > outNode)
                return true;

            return false;
        }

        private void CalculateNodeValue(int nodeId)
        {
            var node = NodeGenes[nodeId];
            if (!node.Calculated)
            {
                node.Calculated = true;

                var directConnections = connectionGenesByOutNode[nodeId];
                float sum = 0f;
                foreach (var conn in directConnections)
                {
                    if (conn.Enabled)
                    {
                        var inNode = NodeGenes[conn.InNode];
                        if (!inNode.Calculated)
                            CalculateNodeValue(inNode.ID);
                        sum = node.Value + inNode.Value * conn.Weight;

                    }
                }
                if (node.Type == NodeGene.NodeType.HIDDEN)
                    node.Value = Tanh(sum);
                if (node.Type == NodeGene.NodeType.OUTPUT)
                    node.Value = Sigmoid(sum);
            }
        }

        public void ResetGenome(bool resetValuesAndCalculatedFlag)
        {
            if (resetValuesAndCalculatedFlag)
            {
                foreach (var node in NodeGenes.Values)
                {
                    node.ResetValueAndCalculatedFlag();
                }
            }
            else
            {
                foreach (var node in NodeGenes.Values)
                {
                    node.ResetCalculatedFlag();
                }
            }
        }

        private float Tanh(float value)
        {
            float k = (float)Math.Exp(value * -4.9f);
            return (2f / (1.0f + k)) - 1;
        }

        private float Sigmoid(float value)
        {
            float k = (float)Math.Exp(value * -4.9f);
            return (1f / (1.0f + k));
        }

        private float BinaryStep(float value)
        {
            if (value >= 0.5)
                return 1f;
            else
                return 0f;
        }
    }
}
