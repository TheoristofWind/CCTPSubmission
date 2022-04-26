using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Bot.Utilities.Processed.BallPrediction;
using Bot.Utilities.Processed.FieldInfo;
using Bot.Utilities.Processed.Packet;
using RLBotDotNet;

namespace Bot
{
    public class NeuralNetwork
    {
        Random rnd = new Random();

        private float mutationRate = 1.0f;

        private int nbInputs;
        private int nbOutputs;
        private int nbHiddenLayers;
        private int nodesPerLayer;

        private List<List<float>> nodes;
        private List<List<float>> bias;
        private List<List<List<float>>> weights;

        public NeuralNetwork(int _nbInputs, int _nbOuputs, int _nbHiddenLayers, int _nodesPerLayer, float _mutationRate)
        {
            nbInputs = _nbInputs;
            nbOutputs = _nbOuputs;
            nbHiddenLayers = _nbHiddenLayers;
            nodesPerLayer = _nodesPerLayer;
            mutationRate = _mutationRate;

            nodes = new List<List<float>>();
            bias = new List<List<float>>();
            weights = new List<List<List<float>>>();

            //CreateNeuralNetwork();

            //WriteNeuralNetworkToFile("test.txt", "./NeuralNetwork/");
            SetNeuralNetworkFromFile("test.txt", "./NeuralNetwork/");
        }

        private void CreateNeuralNetwork()
        {
            nodes.Add(new List<float>());
            weights.Add(new List<List<float>>());
            for (int i = 0; i < nbInputs; i++)
            {
                nodes[0].Add(0.0f);
                weights[0].Add(new List<float>());
                for (int w = 0; w < nodesPerLayer; w++)
                {
                    weights[0][i].Add(((float)rnd.NextDouble() * 2)-1);
                }
            }

            for (int i = 0; i < nbHiddenLayers; i++)
            {
                nodes.Add(new List<float>());
                bias.Add(new List<float>());
                weights.Add(new List<List<float>>());
                for (int n = 0; n < nodesPerLayer; n++)
                {
                    nodes[i + 1].Add(0);
                    bias[i].Add(((float)rnd.NextDouble() * 2) - 1);

                    weights[i + 1].Add(new List<float>());
                    int howMany = (i + 1 == nbHiddenLayers ? nbOutputs : nodesPerLayer);
                    for (int w = 0; w < howMany; w++)
                    {
                        weights[i + 1][n].Add(((float)rnd.NextDouble() * 2) - 1);
                    }
                }
            }

            nodes.Add(new List<float>());
            bias.Add(new List<float>());
            for (int i = 0; i < nbOutputs; i++)
            {
                nodes[nodes.Count - 1].Add(0.0f);
                bias[bias.Count - 1].Add(((float)rnd.NextDouble() * 2) - 1);
            }
        }

        public List<float> GetOuputFromInput(List<float> input)
        {
            if (input.Count != nbInputs)
            {
                Console.WriteLine("Error: There seems to be " + input.Count.ToString() + " inputs but " + nbInputs.ToString() + " are expected for this neural network!");
                return new List<float>();
            }

            CleanNodes();

            for (int i = 0; i < input.Count; i++)
            {
                nodes[0][i] = input[i];
            }

            for (int l = 0; l < nodes.Count - 1; l++)
            {
                for (int n = 0; n < nodes[l].Count; n++)
                {
                    for (int w = 0; w < weights[l][n].Count; w++)
                    {
                        nodes[l + 1][w] += weights[l][n][w] * nodes[l][n];
                    }
                }

                for (int n = 0; n < nodes[l + 1].Count; n++)
                {
                    nodes[l + 1][n] += bias[l][n];
                    nodes[l + 1][n] /= nodes[l].Count + 1;
                }
            }

            return nodes[nodes.Count - 1];
        }

        private void CleanNodes()
        {
            foreach (List<float> l in nodes)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    l[i] = 0;
                }
            }
        }

        public List<(List<float>, List<List<float>>)> GetWantedChanges(List<float> wantedOutputs)
        {
            List<(List<float>, List<List<float>>)> l = new List<(List<float>, List<List<float>>)>();

            for (int i = nbHiddenLayers + 1; i > 0; i--)
            {
                (List<float>, List<List<float>>, List<float>) changes = GetWantedChangeOnLayer(i, wantedOutputs);
                wantedOutputs = changes.Item3;
                l.Add((changes.Item1, changes.Item2));
            }

            return l;
        }

        public List<(List<float>, List<List<float>>)> AverageWantedChanges(List<List<(List<float>, List<List<float>>)>> allWantedChanges)
        {
            List<(List<float>, List<List<float>>)> l = allWantedChanges[0];

            for (int i = 1; i < allWantedChanges.Count; i++)
            {
                for (int u = 0; u < allWantedChanges[i].Count; u++)
                {
                    for (int y = 0; y < allWantedChanges[i][u].Item1.Count; y++)
                    {
                        l[u].Item1[y] += allWantedChanges[i][u].Item1[y];
                    }

                    for (int y = 0; y < allWantedChanges[i][u].Item2.Count; y++)
                    {
                        for (int t = 0; t < allWantedChanges[i][u].Item2[y].Count; t++)
                        {
                            l[u].Item2[y][t] += allWantedChanges[i][u].Item2[y][t];
                        }
                    }
                }
            }

            for (int u = 0; u < l.Count; u++)
            {
                for (int y = 0; y < l[u].Item1.Count; y++)
                {
                    l[u].Item1[y] /= allWantedChanges.Count;
                }

                for (int y = 0; y < l[u].Item2.Count; y++)
                {
                    for (int t = 0; t < l[u].Item2[y].Count; t++)
                    {
                        l[u].Item2[y][t] /= allWantedChanges.Count;
                    }
                }
            }

            return l;
        }

        public void Train(List<(List<float>, List<List<float>>)> train)
        {
            ApplyChanges(train);
        }

        private (List<float>, List<List<float>>, List<float>) GetWantedChangeOnLayer(int layerNb, List<float> outputWanted)
        {
            List<float> biasChanges = new List<float>();
            List<List<float>> weightChanges = new List<List<float>>();
            List<float> previousLayerWanted = new List<float>();

            for (int i = 0; i < nodes[layerNb].Count; i++)
            {
                float diff = outputWanted[i] - nodes[layerNb][i];
                diff *= mutationRate;
                biasChanges.Add(diff);
            }

            for (int i = 0; i < nodes[layerNb - 1].Count; i++)
            {
                weightChanges.Add(new List<float>());
                for (int n = 0; n < biasChanges.Count; n++)
                {
                    weightChanges[i].Add(nodes[layerNb - 1][i] * biasChanges[n]);
                }
            }

            for (int i = 0; i < nodes[layerNb - 1].Count; i++)
            {
                previousLayerWanted.Add(nodes[layerNb - 1][i]);
                for (int w = 0; w < nodes[layerNb].Count; w++)
                {
                    previousLayerWanted[i] += weights[layerNb - 1][i][w] * biasChanges[w];
                }
            }

            return (biasChanges, weightChanges, previousLayerWanted);
        }

        private void ApplyChanges(List<(List<float>, List<List<float>>)> changes)
        {
            for (int i = 0; i < changes.Count; i++)
            {
                float biasChange = 1;

                for (int b = 0; b < changes[i].Item1.Count; b++)
                {
                    bias[bias.Count - 1 - i][b] += changes[i].Item1[b];
                    if (bias[bias.Count - 1 - i][b] > biasChange)
                    {
                        biasChange = bias[bias.Count - 1 - i][b];
                    }
                    else if (bias[bias.Count - 1 - i][b] < -biasChange)
                    {
                        biasChange = -bias[bias.Count - 1 - i][b];
                    }
                }

                //if (biasChange > 1)
                //{
                //    for (int b = 0; b < changes[i].Item1.Count; b++)
                //    {
                //        bias[bias.Count - 1 - i][b] /= biasChange;
                //    }
                //}


                float weightsChange = 1;

                for (int w = 0; w < changes[i].Item2.Count; w++)
                {
                    for (int x = 0; x < changes[i].Item2[w].Count; x++)
                    {
                        weights[weights.Count - 1 - i][w][x] += changes[i].Item2[w][x];
                        if (weights[weights.Count - 1 - i][w][x] > weightsChange)
                        {
                            weightsChange = weights[weights.Count - 1 - i][w][x];
                        }
                        else if (weights[weights.Count - 1 - i][w][x] < -weightsChange)
                        {
                            weightsChange = -weights[weights.Count - 1 - i][w][x];
                        }
                    }
                }

                //if (weightsChange > 1)
                //{
                //    for (int w = 0; w < changes[i].Item2.Count; w++)
                //    {
                //        for (int x = 0; x < changes[i].Item2[w].Count; x++)
                //        {
                //            weights[weights.Count - 1 - i][w][x] /= weightsChange;
                //        }
                //    }
                //}
            }
        }

        public void WriteNeuralNetworkToFile(string fileName, string fileLocation)
        {
            string text = nbInputs.ToString() + ":" + nbOutputs.ToString() + ":" + nodesPerLayer.ToString() + ":" + nbHiddenLayers.ToString() + "\n";

            for (int i = 0; i < bias.Count; i++)
            {
                text += "Q\n";
                for (int y = 0; y < bias[i].Count; y++)
                {
                    text += bias[i][y].ToString() + "\n";
                }
            }

            text += "#\n";

            for (int i = 0; i < weights.Count; i++)
            {
                text += "W\n";
                for (int y = 0; y < weights[i].Count; y++)
                {
                    text += "Q\n";
                    for (int z = 0; z < weights[i][y].Count; z++)
                    {
                        text += weights[i][y][z].ToString() + "\n";
                    }
                }
            }

            WriteToFile(text, fileName, fileLocation);
        }

        public void WriteToFile(string text, string fileName, string fileLocation)
        {
            Console.WriteLine("Gonna try and write file");
            try
            {
                File.WriteAllText(fileLocation + fileName, text);
                Console.WriteLine("Wrote to file successfully!");
            }
            catch (DirectoryNotFoundException f)
            {
                if (!Directory.Exists(fileLocation))
                {
                    Directory.CreateDirectory(fileLocation);
                    WriteToFile(text, fileName, fileLocation);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Couldnt write to file with location: " + fileLocation + fileName);
            }
        }

        public void SetNeuralNetworkFromFile(string fileName, string fileLocation)
        {
            string[] lines = File.ReadAllLines(fileLocation + fileName);

            nodes.Clear();
            bias.Clear();
            weights.Clear();

            bool inBias = true;
            int listNb = 0;
            int list2Nb = 0;

            string[] firstLine = lines[0].Split(':');
            nbInputs = int.Parse(firstLine[0]);
            nbOutputs = int.Parse(firstLine[1]);
            nodesPerLayer = int.Parse(firstLine[2]);
            nbHiddenLayers = int.Parse(firstLine[3]);

            for (int i = 0; i < nbHiddenLayers +2; i++)
            {
                nodes.Add(new List<float>());
            }

            for (int inp = 0; inp < nbInputs; inp++)
            {
                nodes[0].Add(0.0f);
            }

            for (int otp = 0; otp < nbOutputs; otp++)
            {
                nodes[nodes.Count-1].Add(0.0f);
            }

            for (int i = 1; i < nbHiddenLayers + 1; i++)
            {
                for (int y = 0; y < nodesPerLayer; y++)
                {
                    nodes[i].Add(0.0f);
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                switch (lines[i])
                {
                    case "Q":
                        {
                            if (inBias)
                            {
                                bias.Add(new List<float>());
                                listNb = bias.Count-1;
                            }
                            else
                            {
                                weights[listNb].Add(new List<float>());
                                list2Nb = weights[listNb].Count - 1;
                            }
                        }
                        break;
                    case "W":
                        {
                            weights.Add(new List<List<float>>());
                            listNb = weights.Count - 1;
                        }
                        break;
                    case "#":
                        {
                            inBias = false;
                            listNb = 0;
                        }
                        break;
                    default:
                        SetValueInNeuralNet(lines[i], inBias ? bias[listNb] : weights[listNb][list2Nb]);
                        break;
                }
            }
        }

        private void SetValueInNeuralNet(string line, List<float> listToAddTo)
        {
            listToAddTo.Add(float.Parse(line));
        }
    }
}
